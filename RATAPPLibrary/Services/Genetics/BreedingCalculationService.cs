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
    public class BreedingCalculationService : BaseService, IBreedingCalculationService
    {
        //private readonly RatAppDbContext _context;

        public BreedingCalculationService(RatAppDbContextFactory contextFactory) : base(contextFactory)
        {
            //_context = context;
        }

        //using a pre-existing pairingId, create a "Breeding Calculation object that can be used to perform various calculations on that pair 
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

        //perform a test pairing to see possible outcomes 
        //TODO
        public IEnumerable<object> CalculateBreedingOutcomes(Animal dam, Animal sire)
        {
            throw new NotImplementedException();
        }
    }
}
