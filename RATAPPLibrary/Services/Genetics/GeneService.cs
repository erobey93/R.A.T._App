using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Services.Genetics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RATAPPLibrary.Services.Genetics
{
    /// <summary>
    /// Service for managing genetic information in the R.A.T. App.
    /// Handles genes, alleles, and genotypes at a molecular level.
    /// 
    /// Key Features:
    /// - Gene Management:
    ///   * Create and track genes
    ///   * Manage allele variants
    ///   * Track genotypes
    /// 
    /// Gene Categories:
    /// - Physical: Appearance traits
    /// - Medical: Health conditions
    /// - Behavioral: Temperament traits
    /// 
    /// Impact Levels:
    /// - Cosmetic: Appearance only
    /// - Health: Affects wellbeing
    /// - Critical: Serious conditions
    /// 
    /// Expression Ages:
    /// - Birth: Present at birth
    /// - Juvenile: Develops in youth
    /// - Adult: Late-onset traits
    /// 
    /// Validation Rules:
    /// - Gene positions must be unique per chromosome
    /// - Allele symbols must be unique per gene
    /// - Only one wild type allele per gene
    /// - Maternal/Paternal alleles must match gene
    /// 
    /// Dependencies:
    /// - Inherits from BaseService
    /// - Implements IGeneService
    /// </summary>
    public class GeneService : BaseService, IGeneService
    {
        //private readonly RatAppDbContext _context;
        private readonly string[] _validCategories = { "physical", "medical", "behavioral" };
        private readonly string[] _validImpactLevels = { "cosmetic", "health", "critical" };
        private readonly string[] _validExpressionAges = { "birth", "juvenile", "adult" };

        public GeneService(RatAppDbContextFactory contextFactory) : base(contextFactory)
        {
            //_context = context;
        }

        /// <summary>
        /// Creates a new gene with specified characteristics.
        /// 
        /// Required Information:
        /// - Name and common name
        /// - Chromosome pair and position
        /// - Category and impact level
        /// - Expression timing
        /// 
        /// Validation:
        /// - Position must be unique on chromosome
        /// - Category must be valid
        /// - Impact level must be valid
        /// - Expression age must be valid
        /// 
        /// Throws:
        /// - ArgumentException for invalid inputs
        /// - InvalidOperationException for duplicates
        /// </summary>
        /// <param name="request">Gene creation details</param>
        /// <returns>Created Gene object</returns>
        public async Task<Gene> CreateGeneAsync(CreateGeneRequest request)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                ValidateGeneRequest(request);

                // Check if position is already taken
                var exists = await _context.Genes
                    .AnyAsync(g => g.ChromosomePairId == request.ChromosomePairId && g.Position == request.Position);

                if (exists)
                    throw new InvalidOperationException($"Position {request.Position} is already occupied on this chromosome pair");

                var gene = new Gene
                {
                    GeneId = Guid.NewGuid(),
                    Name = request.Name,
                    CommonName = request.CommonName,
                    ChromosomePairId = request.ChromosomePairId,
                    Position = request.Position,
                    Category = request.Category.ToLower(),
                    ImpactLevel = request.ImpactLevel.ToLower(),
                    ExpressionAge = request.ExpressionAge.ToLower(),
                    Penetrance = request.Penetrance.ToLower(),
                    Expressivity = request.Expressivity.ToLower(),
                    RequiresMonitoring = request.RequiresMonitoring,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Genes.Add(gene);
                await _context.SaveChangesAsync();

                return gene;
            });
        }

        /// <summary>
        /// Creates a new allele variant for a gene.
        /// 
        /// Required Information:
        /// - Gene association
        /// - Name and symbol
        /// - Wild type status
        /// - Phenotype expression
        /// - Risk assessment
        /// 
        /// Validation:
        /// - Symbol must be unique per gene
        /// - Only one wild type allowed
        /// - Risk level must be specified
        /// 
        /// Note: Wild type alleles are typically dominant
        /// and represent the most common variant.
        /// 
        /// Throws:
        /// - ArgumentException for invalid inputs
        /// - InvalidOperationException for duplicates
        /// </summary>
        /// <param name="request">Allele creation details</param>
        /// <returns>Created Allele object</returns>
        public async Task<Allele> CreateAlleleAsync(CreateAlleleRequest request)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                ValidateAlleleRequest(request);

                // Check if symbol is already used for this gene
                var exists = await _context.Alleles
                    .AnyAsync(a => a.GeneId == request.GeneId && a.Symbol == request.Symbol);

                if (exists)
                    throw new InvalidOperationException($"Symbol {request.Symbol} is already used for this gene");

                // Check if trying to create a second wild type allele
                if (request.IsWildType)
                {
                    var hasWildType = await _context.Alleles
                        .AnyAsync(a => a.GeneId == request.GeneId && a.IsWildType);

                    if (hasWildType)
                        throw new InvalidOperationException("This gene already has a wild type allele");
                }

                var allele = new Allele
                {
                    AlleleId = Guid.NewGuid(),
                    GeneId = request.GeneId,
                    Name = request.Name,
                    Symbol = request.Symbol,
                    IsWildType = request.IsWildType,
                    Phenotype = request.Phenotype,
                    RiskLevel = request.RiskLevel.ToLower(),
                    ManagementNotes = request.ManagementNotes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Alleles.Add(allele);
                await _context.SaveChangesAsync();

                return allele;
            });
        }

        public async Task<List<Gene>> GetGenesForChromosomePairAsync(Guid chromosomePairId)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                return await _context.Genes
                .Where(g => g.ChromosomePairId == chromosomePairId)
                .OrderBy(g => g.Position)
                .ToListAsync();
            });
        }

        public async Task<List<Allele>> GetAllelesForGeneAsync(Guid geneId)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                return await _context.Alleles
                .Where(a => a.GeneId == geneId)
                .OrderByDescending(a => a.IsWildType)
                .ThenBy(a => a.Symbol)
                .ToListAsync();
            });
        }

        public async Task<Dictionary<string, List<string>>> GetGeneMapForAnimalAsync(int animalId)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                var genotypes = await _context.Genotypes
                .Where(g => g.AnimalId == animalId)
                .Include(g => g.ChromosomePair)
                    .ThenInclude(cp => cp.Genes)
                        .ThenInclude(g => g.Alleles)
                .ToListAsync();

                var geneMap = new Dictionary<string, List<string>>();

                foreach (var genotype in genotypes)
                {
                    foreach (var gene in genotype.ChromosomePair.Genes)
                    {
                        if (!geneMap.ContainsKey(gene.Name))
                        {
                            geneMap[gene.Name] = new List<string>();
                        }

                        // Add allele names to the map
                        foreach (var allele in gene.Alleles)
                        {
                            geneMap[gene.Name].Add(allele.Name);
                        }
                    }
                }

                return geneMap;
            });
        }

        /// <summary>
        /// Assigns a genotype to an animal for a specific chromosome pair.
        /// 
        /// Process:
        /// 1. Validates no existing genotype
        /// 2. Verifies allele compatibility
        /// 3. Creates genotype record
        /// 
        /// Validation:
        /// - No duplicate genotypes allowed
        /// - Alleles must be from same gene
        /// - Both alleles must exist
        /// 
        /// Used For:
        /// - Recording genetic makeup
        /// - Tracking inherited traits
        /// - Breeding calculations
        /// 
        /// Throws:
        /// - InvalidOperationException for validation failures
        /// </summary>
        /// <param name="request">Genotype assignment details</param>
        /// <returns>Created Genotype object</returns>
        public async Task<Genotype> AssignGenotypeToAnimalAsync(AssignGenotypeRequest request)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                // Check if genotype already exists
                var existingGenotype = await _context.Genotypes
                .FirstOrDefaultAsync(g => g.AnimalId == request.AnimalId &&
                                        g.ChromosomePairId == request.ChromosomePairId);

                if (existingGenotype != null)
                    throw new InvalidOperationException("Genotype already exists for this animal and chromosome pair");

                // Validate alleles belong to the same gene
                var maternalAllele = await _context.Alleles
                    .Include(a => a.Gene)
                    .FirstOrDefaultAsync(a => a.AlleleId == request.MaternalAlleleId);

                var paternalAllele = await _context.Alleles
                    .Include(a => a.Gene)
                    .FirstOrDefaultAsync(a => a.AlleleId == request.PaternalAlleleId);

                if (maternalAllele == null || paternalAllele == null)
                    throw new InvalidOperationException("Both alleles must exist");

                if (maternalAllele.Gene.GeneId != paternalAllele.Gene.GeneId)
                    throw new InvalidOperationException("Alleles must be from the same gene");

                var genotype = new Genotype
                {
                    GenotypeId = Guid.NewGuid(),
                    AnimalId = request.AnimalId,
                    ChromosomePairId = request.ChromosomePairId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Genotypes.Add(genotype);
                await _context.SaveChangesAsync();

                return genotype;
            });
        }

        public async Task<List<Gene>> GetGenesWithEffectAsync(string category, string impactLevel)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                return await _context.Genes
                .Where(g => g.Category.ToLower() == category.ToLower() &&
                           g.ImpactLevel.ToLower() == impactLevel.ToLower())
                .OrderBy(g => g.Name)
                .ToListAsync();
            });
        }

        public async Task<Gene> GetGeneByNameAsync(string name)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                return await _context.Genes
                .FirstOrDefaultAsync(g => g.Name.ToLower() == name.ToLower());
            });
        }

        public async Task<Allele> GetAlleleBySymbolAsync(Guid geneId, string symbol)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                return await _context.Alleles
                .FirstOrDefaultAsync(a => a.GeneId == geneId &&
                                        a.Symbol.ToLower() == symbol.ToLower());
            });
        }

        private void ValidateGeneRequest(CreateGeneRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name cannot be empty", nameof(request.Name));

            if (string.IsNullOrWhiteSpace(request.CommonName))
                throw new ArgumentException("Common name cannot be empty", nameof(request.CommonName));

            if (request.Position <= 0)
                throw new ArgumentException("Position must be positive", nameof(request.Position));

            if (!_validCategories.Contains(request.Category.ToLower()))
                throw new ArgumentException($"Invalid category: {request.Category}", nameof(request.Category));

            if (!_validImpactLevels.Contains(request.ImpactLevel.ToLower()))
                throw new ArgumentException($"Invalid impact level: {request.ImpactLevel}", nameof(request.ImpactLevel));

            if (!_validExpressionAges.Contains(request.ExpressionAge.ToLower()))
                throw new ArgumentException($"Invalid expression age: {request.ExpressionAge}", nameof(request.ExpressionAge));
        }

        private void ValidateAlleleRequest(CreateAlleleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name cannot be empty", nameof(request.Name));

            if (string.IsNullOrWhiteSpace(request.Symbol))
                throw new ArgumentException("Symbol cannot be empty", nameof(request.Symbol));

            if (string.IsNullOrWhiteSpace(request.Phenotype))
                throw new ArgumentException("Phenotype cannot be empty", nameof(request.Phenotype));

            if (string.IsNullOrWhiteSpace(request.RiskLevel))
                throw new ArgumentException("Risk level cannot be empty", nameof(request.RiskLevel));
        }
    }
}
