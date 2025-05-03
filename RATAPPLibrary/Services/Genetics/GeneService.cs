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
    public class GeneService : IGeneService
    {
        private readonly RatAppDbContext _context;
        private readonly string[] _validCategories = { "physical", "medical", "behavioral" };
        private readonly string[] _validImpactLevels = { "cosmetic", "health", "critical" };
        private readonly string[] _validExpressionAges = { "birth", "juvenile", "adult" };

        public GeneService(RatAppDbContext context)
        {
            _context = context;
        }

        public async Task<Gene> CreateGeneAsync(CreateGeneRequest request)
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
        }

        public async Task<Allele> CreateAlleleAsync(CreateAlleleRequest request)
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
        }

        public async Task<List<Gene>> GetGenesForChromosomePairAsync(Guid chromosomePairId)
        {
            return await _context.Genes
                .Where(g => g.ChromosomePairId == chromosomePairId)
                .OrderBy(g => g.Position)
                .ToListAsync();
        }

        public async Task<List<Allele>> GetAllelesForGeneAsync(Guid geneId)
        {
            return await _context.Alleles
                .Where(a => a.GeneId == geneId)
                .OrderByDescending(a => a.IsWildType)
                .ThenBy(a => a.Symbol)
                .ToListAsync();
        }

        public async Task<Dictionary<string, List<string>>> GetGeneMapForAnimalAsync(int animalId)
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
        }

        public async Task<Genotype> AssignGenotypeToAnimalAsync(AssignGenotypeRequest request)
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
        }

        public async Task<List<Gene>> GetGenesWithEffectAsync(string category, string impactLevel)
        {
            return await _context.Genes
                .Where(g => g.Category.ToLower() == category.ToLower() && 
                           g.ImpactLevel.ToLower() == impactLevel.ToLower())
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<Gene> GetGeneByNameAsync(string name)
        {
            return await _context.Genes
                .FirstOrDefaultAsync(g => g.Name.ToLower() == name.ToLower());
        }

        public async Task<Allele> GetAlleleBySymbolAsync(Guid geneId, string symbol)
        {
            return await _context.Alleles
                .FirstOrDefaultAsync(a => a.GeneId == geneId && 
                                        a.Symbol.ToLower() == symbol.ToLower());
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
