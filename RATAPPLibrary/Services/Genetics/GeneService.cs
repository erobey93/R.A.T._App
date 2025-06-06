using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
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
    /// Handles genes, alleles, and genotypes.
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
    /// Gene Sub-Categories:
    /// - Physical
    ///     - Ear Type
    ///     - Coat Type
    ///     - Eye Color
    ///     - Coat Color
    ///     - 
    /// 
    /// Impact Levels:
    /// - Cosmetic: Appearance only
    /// - Health: Affects physical wellbeing
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
        private readonly string[] _validCategories = { "physical", "medical", "behavioral" };
        private readonly string[] _validImpactLevels = { "cosmetic", "health", "critical" };
        private readonly string[] _validExpressionAges = { "birth", "juvenile", "adult" };

        public GeneService(RatAppDbContextFactory contextFactory) : base(contextFactory)
        {
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
                // Check if genotype already exists in animal genotype table 
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

                // Check if the generic genotype already exists in generic genotype table  FIXME FIXME FIXME 
                var existingGenericGenotype = await _context.GenericGenotype
                .FirstOrDefaultAsync(g => g.ChromosomePairId == request.ChromosomePairId);

                //this is temporary, there should just be a genericgenotype id in the genome (not genotype) table, but I don't want to change the structure right now so FIXME/TODO 
                if (existingGenericGenotype == null)
                {
                   
                    //currently a hack until I re-factor to use a genome table for animals 
                    //I need genotypes and generic genotypes to sync up 
                    var genericGenotype = new GenericGenotype
                    {
                        GenotypeId = Guid.NewGuid(),
                        GenotypeCode = request.GenotypeCode,
                        ChromosomePairId = request.ChromosomePairId,
                        TraitId = request.TraitId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                }

                var genotype = new Genotype
                {
                    GenotypeId = Guid.NewGuid(),
                    AnimalId = request.AnimalId,
                    GenotypeCode = request.GenotypeCode,
                    TraitId = request.TraitId, 
                    ChromosomePairId = request.ChromosomePairId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Genotypes.Add(genotype);
                await _context.SaveChangesAsync();

                return genotype;
            });
        }

        public async Task<Genotype> AssignGenericGenotypeToAnimalAsync( int animalId, int traitId)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                // Step 1: Retrieve the Generic Genotype by TraitId
                var genericGenotype = await _context.GenericGenotype
                    .Where(gg => gg.TraitId == traitId)
                    .FirstOrDefaultAsync();

                // Step 2: Validate that the Generic Genotype exists
                if (genericGenotype == null)
                {
                    throw new Exception($"No GenericGenotype found for TraitId {traitId}.");
                }

                // validate that the Animal exists
                var animalExists = await _context.Animal
                    .AnyAsync(a => a.Id == animalId);
                if (!animalExists)
                {
                    throw new Exception($"Animal with ID {animalId} does not exist.");
                }

                // Step 3: Create a new Genotype entry
                var newGenotype = new Genotype
                {
                    AnimalId = animalId,
                    GenotypeCode = genericGenotype.GenotypeCode,
                    ChromosomePairId = genericGenotype.ChromosomePairId,
                    TraitId = traitId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Step 4: Add the new Genotype to the context and save changes
                _context.Genotypes.Add(newGenotype);
                await _context.SaveChangesAsync();

                // Step 5: Return the created Genotype
                return newGenotype;
            });
        }

        //get genotypes organized by type for animal 
        //returns genotype codes with trait type names so that the genotype can be easily built 
        public async Task<Dictionary<string, List<string>>> GetGenotypesOrganizedByType(int animalId)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                //First, fetch all genotypes for the animal
                var genotypes = await _context.Genotypes
                .Where(ag => ag.AnimalId == animalId)
                .Select(ag => new
                {
                    ag.GenotypeCode,
                    ag.GenotypeId,
                    ag.Trait.TraitTypeId,
                    ag.Trait.TraitType.Name
                })
                .ToListAsync();

                // Group the genotypes by their TraitTypeId
                var groupedGenotypes = genotypes
                    .GroupBy(g => g.Name)
                    .ToDictionary(
                        g => g.Key, // Key is the TraitTypeId
                        g => g.Select(x => x.GenotypeCode).ToList() // Value is a list of genotypes
                    );

                // Return the organized genotypes
                return groupedGenotypes;
            });
        }

        //get all genotype objects by animal id 
        public async Task<List<Genotype>> GetAllGenotypesByAnimalId(int animalId)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                //First, fetch all genotypes for the animal
                var genotypes = await _context.Genotypes
                 .Where(g => g.AnimalId == animalId)
                 .Include(g => g.ChromosomePair)          // Load ChromosomePair
                     .ThenInclude(cp => cp.Genes)         // Load Genes within ChromosomePair
                        .ThenInclude(g => g.Alleles)
                 .ToListAsync();


                // Return the organized genotypes
                return genotypes;
            });
        }

        //create genotype string
        public async Task<string> CreateGenotypeString(int animalId, string separator = ", ", string typeSeparator = " | ")
        {
            // Fetch the genotypes organized by type
            var organizedGenotypes = await GetGenotypesOrganizedByType(animalId);
            

            //FIXME I don't like how the format looks currently so just parsing off the first part for now
            foreach (var genotype in organizedGenotypes)
            {
                //parse off portion before string
                var getGeno = genotype.Value;
            }
            // Handle the case where there are no genotypes
            if (!organizedGenotypes.Any())
            {
                return "No genotypes available.";
            }

            // Build the genotype string
            //var genotypeString = organizedGenotypes
            //    .Select(group =>
            //        $"{group.Key ?? "Unknown Type"}: {string.Join(separator, group.Value ?? new List<string>())}"
            //    )
            //    .Aggregate((current, next) => $"{current}{typeSeparator}{next}");
            var genotypeString = organizedGenotypes 
                .Select(group =>
                $"{string.Join(separator, group.Value ?? new List<string>())}"
                    )
                    .Aggregate((current, next) => $"{current}{typeSeparator}{next}");

            return genotypeString;
        }

        //AnimalService is currently doing this, fine for now 
        //set genotypes based on phenotypes 
        //current case is very simple, simple dominant and recessive traits unless genotype is manually entered 
        //public async Task yyy()
        //{
        //    //get the trait id of the selected phenotype
        //    //get all the genotype with that trait id from the generic genotype table 
        //    //make an entry into the genotype table for the animal associated with it 
        //}

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

            //if (request.Position <= 0)
            //    throw new ArgumentException("Position must be positive", nameof(request.Position)); //will need another check here maybe just include every possible position and give them that list? FIXME 

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
