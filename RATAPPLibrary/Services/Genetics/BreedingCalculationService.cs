using Microsoft.EntityFrameworkCore;
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
    /// Service for performing genetic calculations and breeding predictions in the R.A.T. App.
    /// Handles complex genetic inheritance patterns, trait probability calculations,
    /// and breeding risk assessments.
    /// 
    /// Key Features:
    /// - Breeding Calculations:
    ///   * Mendelian inheritance modeling
    ///   * Trait probability calculations
    ///   * Genetic risk assessment
    /// 
    /// Genetic Models:
    /// - Basic Mendelian inheritance (0.25 probability)
    /// - Wild type dominance handling
    /// - Recessive trait expression
    /// 
    /// Risk Assessment:
    /// - Gene monitoring requirements
    /// - Critical impact level tracking
    /// - Variable expressivity handling
    /// 
    /// Known Limitations:
    /// - Basic Mendelian probability only
    /// - Limited complex inheritance patterns
    /// - Some methods not implemented
    /// - Basic phenotype determination
    /// 
    /// Planned Improvements:
    /// - Complex inheritance patterns
    /// - Multi-gene interactions
    /// - Epigenetic factors
    /// - Complete test pairing implementation
    /// 
    /// Dependencies:
    /// - Inherits from BaseService
    /// - Uses Entity Framework for data access
    /// - Implements IBreedingCalculationService
    /// </summary>
    public class BreedingCalculationService : BaseService, IBreedingCalculationService
    {

        public BreedingCalculationService(RatAppDbContextFactory contextFactory) : base(contextFactory)
        {
        }

        //using a pre-existing pairingId, create a "Breeding Calculation object that can be used to perform various calculations on that pair 
        /// <summary>
        /// Creates a new breeding calculation for a specific pairing.
        /// 
        /// Process:
        /// 1. Validates pairing exists
        /// 2. Creates calculation record with unique ID
        /// 3. Associates with pairing
        /// 
        /// Note: This is the first step in performing breeding calculations.
        /// The calculation ID is used in subsequent operations.
        /// 
        /// Throws:
        /// - InvalidOperationException if pairing not found
        /// </summary>
        /// <param name="pairingId">ID of the breeding pair</param>
        /// <returns>New BreedingCalculation object</returns>
        public async Task<BreedingCalculation> CreateBreedingCalculationAsync(int pairingId)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                var pairing = await _context.Pairing
                .Include(p => p.Dam)
                .Include(p => p.Sire)
                .FirstOrDefaultAsync(p => p.Id == pairingId);

                if (pairing == null)
                    throw new InvalidOperationException("Pairing not found");

                var calculation = new BreedingCalculation
                {
                    CalculationId = Guid.NewGuid(),
                    PairingId = pairingId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.BreedingCalculations.Add(calculation);
                await _context.SaveChangesAsync();

                return calculation;
            });
        }

        /// <summary>
        /// Calculates possible offspring outcomes and their probabilities.
        /// 
        /// Process:
        /// 1. Loads parent genotypes and chromosomes
        /// 2. Calculates allele combinations
        /// 3. Determines phenotypes
        /// 4. Groups similar outcomes
        /// 
        /// Genetic Model:
        /// - Uses basic Mendelian inheritance (0.25 probability)
        /// - Considers chromosome pairs and genes
        /// - Handles allele combinations
        /// 
        /// Results Include:
        /// - Genotype descriptions
        /// - Phenotype predictions
        /// - Probability calculations
        /// - Parental allele tracking
        /// 
        /// TODO:
        /// - Add complex inheritance patterns
        /// - Consider gene interactions
        /// - Improve probability calculations
        /// </summary>
        /// <param name="calculationId">ID of breeding calculation</param>
        /// <returns>List of possible offspring with probabilities</returns>
        public async Task<List<PossibleOffspring>> CalculateOffspringProbabilitiesAsync(Guid calculationId)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                var calculation = await _context.BreedingCalculations
                .Include(bc => bc.Pairing)
                    .ThenInclude(p => p.Dam)
                        .ThenInclude(d => d.Genotypes)
                            .ThenInclude(g => g.ChromosomePair)
                                .ThenInclude(cp => cp.Genes)
                                    .ThenInclude(g => g.Alleles)
                .Include(bc => bc.Pairing)
                    .ThenInclude(p => p.Sire)
                        .ThenInclude(s => s.Genotypes)
                            .ThenInclude(g => g.ChromosomePair)
                                .ThenInclude(cp => cp.Genes)
                                    .ThenInclude(g => g.Alleles)
                .FirstOrDefaultAsync(bc => bc.CalculationId == calculationId);

                if (calculation == null)
                    throw new InvalidOperationException("Breeding calculation not found");

                var possibleOffspring = new List<PossibleOffspring>();
                var damGenotypes = calculation.Pairing.Dam.Genotypes;
                var sireGenotypes = calculation.Pairing.Sire.Genotypes;

                // For each chromosome pair
                foreach (var damGenotype in damGenotypes)
                {
                    var sireGenotype = sireGenotypes
                        .FirstOrDefault(g => g.ChromosomePairId == damGenotype.ChromosomePairId);

                    if (sireGenotype == null)
                        continue;

                    // For each gene on the chromosome pair
                    foreach (var gene in damGenotype.ChromosomePair.Genes)
                    {
                        var damAlleles = await _context.Alleles
                            .Where(a => a.GeneId == gene.GeneId)
                            .ToListAsync();

                        var sireAlleles = await _context.Alleles
                            .Where(a => a.GeneId == gene.GeneId)
                            .ToListAsync();

                        // Calculate possible combinations
                        foreach (var damAllele in damAlleles)
                        {
                            foreach (var sireAllele in sireAlleles)
                            {
                                var offspring = new PossibleOffspring
                                {
                                    OffspringId = Guid.NewGuid(),
                                    CalculationId = calculationId,
                                    Probability = 0.25f, // Basic Mendelian inheritance
                                    GenotypeDescription = $"{damAllele.Symbol}{sireAllele.Symbol}",
                                    Phenotype = DeterminePhenotype(damAllele, sireAllele),
                                    MaternalAlleles = damAllele.Symbol,
                                    PaternalAlleles = sireAllele.Symbol
                                };

                                possibleOffspring.Add(offspring);
                            }
                        }
                    }
                }

                // Group similar outcomes and sum probabilities
                var groupedOffspring = possibleOffspring
                    .GroupBy(o => new { o.GenotypeDescription, o.Phenotype })
                    .Select(g => new PossibleOffspring
                    {
                        OffspringId = Guid.NewGuid(),
                        CalculationId = calculationId,
                        Probability = g.Sum(o => o.Probability),
                        GenotypeDescription = g.Key.GenotypeDescription,
                        Phenotype = g.Key.Phenotype,
                        MaternalAlleles = g.First().MaternalAlleles,
                        PaternalAlleles = g.First().PaternalAlleles
                    })
                    .ToList();

                _context.PossibleOffspring.AddRange(groupedOffspring);
                await _context.SaveChangesAsync();

                return groupedOffspring;
            });
        }

        public async Task<List<BreedingCalculation>> GetBreedingCalculationsForPairingAsync(int pairingId)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                return await _context.BreedingCalculations
                .Where(bc => bc.PairingId == pairingId)
                .OrderByDescending(bc => bc.CreatedAt)
                .ToListAsync();
            });
        }

        public async Task<Dictionary<string, float>> GetTraitProbabilitiesAsync(Guid calculationId)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                var offspring = await _context.PossibleOffspring
                .Where(po => po.CalculationId == calculationId)
                .ToListAsync();

                var probabilities = new Dictionary<string, float>();

                foreach (var outcome in offspring)
                {
                    var traits = outcome.Phenotype.Split(',')
                        .Select(t => t.Trim())
                        .Where(t => !string.IsNullOrEmpty(t));

                    foreach (var trait in traits)
                    {
                        if (!probabilities.ContainsKey(trait))
                            probabilities[trait] = 0;

                        probabilities[trait] += outcome.Probability;
                    }
                }

                return probabilities;
            });
        }

        /// <summary>
        /// Validates the genetic compatibility of a potential breeding pair.
        /// 
        /// Checks:
        /// - Genes requiring monitoring
        /// - Critical impact genes
        /// - High-risk allele combinations
        /// 
        /// Risk Assessment:
        /// - Identifies monitored genes
        /// - Calculates affected offspring probability
        /// - Flags incompatible combinations
        /// 
        /// Results:
        /// - Overall compatibility status
        /// - List of warnings
        /// - List of genetic risks
        /// 
        /// TODO:
        /// - Add more compatibility criteria
        /// - Enhance risk calculations
        /// - Consider genetic diversity
        /// </summary>
        /// <param name="damId">ID of female animal</param>
        /// <param name="sireId">ID of male animal</param>
        /// <returns>Compatibility assessment result</returns>
        public async Task<BreedingCompatibilityResult> ValidateBreedingPairAsync(int damId, int sireId)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                var result = new BreedingCompatibilityResult
                {
                    IsCompatible = true,
                    Warnings = new List<string>(),
                    Risks = new List<string>()
                };

                var damGenotypes = await _context.Genotypes
                    .Where(g => g.AnimalId == damId)
                    .Include(g => g.ChromosomePair)
                        .ThenInclude(cp => cp.Genes)
                            .ThenInclude(g => g.Alleles)
                    .ToListAsync();

                var sireGenotypes = await _context.Genotypes
                    .Where(g => g.AnimalId == sireId)
                    .Include(g => g.ChromosomePair)
                        .ThenInclude(cp => cp.Genes)
                            .ThenInclude(g => g.Alleles)
                    .ToListAsync();

                foreach (var damGenotype in damGenotypes)
                {
                    foreach (var gene in damGenotype.ChromosomePair.Genes)
                    {
                        if (gene.RequiresMonitoring)
                        {
                            result.Warnings.Add($"Gene {gene.Name} requires monitoring");
                        }

                        if (gene.ImpactLevel == "critical")
                        {
                            var damAlleles = await _context.Alleles
                                .Where(a => a.GeneId == gene.GeneId)
                                .ToListAsync();

                            var sireAlleles = await _context.Alleles
                                .Where(a => a.GeneId == gene.GeneId && a.RiskLevel == "high")
                                .ToListAsync();

                            if (damAlleles.Any(a => a.RiskLevel == "high") && sireAlleles.Any())
                            {
                                result.IsCompatible = false;
                                result.Risks.Add($"25% chance of affected offspring for {gene.Name}");
                            }
                        }
                    }
                }

                return result;
            });
        }

        public async Task<List<InheritanceRisk>> AnalyzeGeneticRisksAsync(Guid calculationId)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                var calculation = await _context.BreedingCalculations
                .Include(bc => bc.PossibleOffspring)
                .FirstOrDefaultAsync(bc => bc.CalculationId == calculationId);

                if (calculation == null)
                    throw new InvalidOperationException("Breeding calculation not found");

                var risks = new List<InheritanceRisk>();

                // Group offspring by gene and analyze risks
                var geneGroups = calculation.PossibleOffspring
                    .GroupBy(po => po.GenotypeDescription.Substring(0, 1)); // Group by first character of genotype

                foreach (var group in geneGroups)
                {
                    var gene = await _context.Genes
                        .FirstOrDefaultAsync(g => g.Name.StartsWith(group.Key));

                    if (gene == null)
                        continue;

                    if (gene.Expressivity == "variable")
                    {
                        risks.Add(new InheritanceRisk
                        {
                            GeneName = gene.Name,
                            Description = "Expression of this trait may vary between individuals",
                            Probability = group.Sum(o => o.Probability),
                            ManagementRecommendation = "Monitor trait development"
                        });
                    }

                    if (gene.RequiresMonitoring)
                    {
                        risks.Add(new InheritanceRisk
                        {
                            GeneName = gene.Name,
                            Description = "This trait requires ongoing monitoring",
                            Probability = 1.0f,
                            ManagementRecommendation = "Regular health checks recommended"
                        });
                    }
                }

                return risks;
            });
        }

        private string DeterminePhenotype(Allele allele1, Allele allele2)
        {
            // If either allele is wild type, use its phenotype (dominant)
            if (allele1.IsWildType || allele2.IsWildType)
            {
                return allele1.IsWildType ? allele1.Phenotype : allele2.Phenotype;
            }

            // For recessive traits, both alleles must be the same
            return allele1.Symbol == allele2.Symbol ? allele1.Phenotype : allele1.Phenotype;
        }

        public Task<BreedingCalculation> GetBreedingCalculationAsync(Guid calculationId)
        {
            throw new NotImplementedException();
        }

        public Task<List<PossibleOffspring>> GetPossibleOffspringAsync(Guid calculationId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the inbreeding coefficient for a potential offspring using Wright's path method.
        /// The inbreeding coefficient (F) represents the probability that both alleles at any locus 
        /// in an individual are identical by descent.
        /// 
        /// Formula: F = Σ (0.5)^(n+m+1) * (1 + FA)
        /// Where:
        /// - n = number of generations from sire to common ancestor
        /// - m = number of generations from dam to common ancestor
        /// - FA = inbreeding coefficient of common ancestor
        /// </summary>
        /// <param name="damId">ID of the female parent</param>
        /// <param name="sireId">ID of the male parent</param>
        /// <returns>Inbreeding coefficient as a decimal between 0 and 1</returns>
        public async Task<double> CalculateInbreedingCoefficientAsync(int damId, int sireId)
        {
            return await ExecuteInTransactionAsync(async context =>
            {
                // Find all common ancestors between dam and sire
                var commonAncestors = await FindCommonAncestorsAsync(damId, sireId);
                
                double totalCoefficient = 0;

                // Calculate contribution from each common ancestor
                foreach (var ancestor in commonAncestors)
                {
                    double pathContribution = Math.Pow(0.5, ancestor.damPath + ancestor.sirePath + 1);
                    
                    // Recursively calculate ancestor's own inbreeding coefficient
                    double ancestorCoefficient = await GetAncestorInbreedingCoefficientAsync(ancestor.ancestor.Id);
                    
                    totalCoefficient += pathContribution * (1 + ancestorCoefficient);
                }

                return totalCoefficient;
            });
        }

        /// <summary>
        /// Finds all common ancestors between two animals and their path lengths.
        /// </summary>
        private async Task<List<(Animal ancestor, int damPath, int sirePath)>> FindCommonAncestorsAsync(int damId, int sireId)
        {
            return await ExecuteInContextAsync(async context =>
            {
                var commonAncestors = new List<(Animal ancestor, int damPath, int sirePath)>();

                // Get all ancestors for both dam and sire
                var damAncestors = await context.Lineages
                    .Include(l => l.Ancestor)
                    .Where(l => l.AnimalId == damId)
                    .ToListAsync();

                var sireAncestors = await context.Lineages
                    .Include(l => l.Ancestor)
                    .Where(l => l.AnimalId == sireId)
                    .ToListAsync();

                // Find common ancestors
                foreach (var damLine in damAncestors)
                {
                    var matchingSireLine = sireAncestors
                        .FirstOrDefault(s => s.AncestorId == damLine.AncestorId);

                    if (matchingSireLine != null)
                    {
                        commonAncestors.Add((
                            damLine.Ancestor,
                            damLine.Generation,  // Path length from dam to ancestor
                            matchingSireLine.Generation  // Path length from sire to ancestor
                        ));
                    }
                }

                return commonAncestors;
            });
        }

        /// <summary>
        /// Recursively calculates the inbreeding coefficient for an ancestor.
        /// </summary>
        private async Task<double> GetAncestorInbreedingCoefficientAsync(int ancestorId)
        {
            var parents = await ExecuteInContextAsync(async context =>
            {
                var lineages = await context.Lineages
                    .Where(l => l.AnimalId == ancestorId && l.Generation == 1)
                    .ToListAsync();

                var damId = lineages.FirstOrDefault(l => l.RelationshipType == "Maternal")?.AncestorId;
                var sireId = lineages.FirstOrDefault(l => l.RelationshipType == "Paternal")?.AncestorId;

                return (damId, sireId);
            });

            // If ancestor has no parents recorded, assume inbreeding coefficient of 0
            if (!parents.damId.HasValue || !parents.sireId.HasValue)
                return 0;

            // Recursively calculate ancestor's inbreeding coefficient
            return await CalculateInbreedingCoefficientAsync(parents.damId.Value, parents.sireId.Value);
        }

        //perform a test pairing to see possible outcomes 
        //TODO
        public IEnumerable<object> CalculateBreedingOutcomes(Animal dam, Animal sire)
        {
            throw new NotImplementedException();
        }
    }
}
