using Microsoft.VisualStudio.TestTools.UnitTesting;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Services.Genetics.Interfaces;
using RATAPPLibraryUT.Genetics.Base;
using System;
using System.Linq;
using System.Threading.Tasks;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace RATAPPLibraryUT.Genetics.BreedingCalculationTests
{
    [TestClass]
    public class ComplexInheritanceTests : GenomicsTestBase
    {
        private ChromosomePair _colorChromosomePair;
        private ChromosomePair _patternChromosomePair;
        private Gene _colorGene;
        private Gene _patternGene;
        private Allele _colorDominant;
        private Allele _colorRecessive;
        private Allele _patternDominant;
        private Allele _patternRecessive;

        [TestInitialize]
        public override async Task TestInitialize()
        {
            await base.TestInitialize();

            // Create chromosome pairs
            var (maternal1, paternal1) = await CreateTestChromosomePairAsync();
            _colorChromosomePair = await _chromosomeService.CreateChromosomePairAsync(
                maternal1.ChromosomeId,
                paternal1.ChromosomeId,
                "autosomal"
            );

            var (maternal2, paternal2) = await CreateTestChromosomePairAsync();
            _patternChromosomePair = await _chromosomeService.CreateChromosomePairAsync(
                maternal2.ChromosomeId,
                paternal2.ChromosomeId,
                "autosomal"
            );

            // Create color gene (C = colored, c = albino)
            _colorGene = await _geneService.CreateGeneAsync(new CreateGeneRequest
            {
                Name = "TYR",
                CommonName = "Albino locus",
                ChromosomePairId = _colorChromosomePair.PairId,
                Position = 1,
                Category = "physical",
                ImpactLevel = "cosmetic",
                ExpressionAge = "birth",
                Penetrance = "complete",
                Expressivity = "consistent"
            });

            _colorDominant = await _geneService.CreateAlleleAsync(new CreateAlleleRequest
            {
                GeneId = _colorGene.GeneId,
                Name = "Wild Type",
                Symbol = "C",
                IsWildType = true,
                Phenotype = "Normal pigmentation",
                RiskLevel = "none"
            });

            _colorRecessive = await _geneService.CreateAlleleAsync(new CreateAlleleRequest
            {
                GeneId = _colorGene.GeneId,
                Name = "Albino",
                Symbol = "c",
                IsWildType = false,
                Phenotype = "White coat, red eyes",
                RiskLevel = "none"
            });

            // Create pattern gene (H = self, h = hooded)
            _patternGene = await _geneService.CreateGeneAsync(new CreateGeneRequest
            {
                Name = "HOOD",
                CommonName = "Hooded locus",
                ChromosomePairId = _patternChromosomePair.PairId,
                Position = 1,
                Category = "physical",
                ImpactLevel = "cosmetic",
                ExpressionAge = "birth",
                Penetrance = "complete",
                Expressivity = "variable"
            });

            _patternDominant = await _geneService.CreateAlleleAsync(new CreateAlleleRequest
            {
                GeneId = _patternGene.GeneId,
                Name = "Self",
                Symbol = "H",
                IsWildType = true,
                Phenotype = "Full body color",
                RiskLevel = "none"
            });

            _patternRecessive = await _geneService.CreateAlleleAsync(new CreateAlleleRequest
            {
                GeneId = _patternGene.GeneId,
                Name = "Hooded",
                Symbol = "h",
                IsWildType = false,
                Phenotype = "Color on head and stripe",
                RiskLevel = "none"
            });
        }

        [TestMethod]
        public async Task CalculateOffspring_TwoGenes_CorrectRatios()
        {
            // Arrange - Set up CcHh x ccHh parents
            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testDam.Id,
                ChromosomePairId = _colorChromosomePair.PairId,
                MaternalAlleleId = _colorDominant.AlleleId,
                PaternalAlleleId = _colorRecessive.AlleleId
            });

            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testDam.Id,
                ChromosomePairId = _patternChromosomePair.PairId,
                MaternalAlleleId = _patternDominant.AlleleId,
                PaternalAlleleId = _patternRecessive.AlleleId
            });

            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testSire.Id,
                ChromosomePairId = _colorChromosomePair.PairId,
                MaternalAlleleId = _colorRecessive.AlleleId,
                PaternalAlleleId = _colorRecessive.AlleleId
            });

            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testSire.Id,
                ChromosomePairId = _patternChromosomePair.PairId,
                MaternalAlleleId = _patternDominant.AlleleId,
                PaternalAlleleId = _patternRecessive.AlleleId
            });

            var calculation = await CreateTestBreedingCalculationAsync();

            // Act
            var offspring = await _breedingCalculationService.CalculateOffspringProbabilitiesAsync(
                calculation.CalculationId
            );

            // Assert
            Assert.AreEqual(4, offspring.Count); // CcHH, Cchhh, ccHh, cchh

            // Check probabilities
            foreach (var outcome in offspring)
            {
                Assert.AreEqual(0.25f, outcome.Probability);
            }

            // Check specific phenotypes
            var coloredSelf = offspring.Single(o => 
                o.GenotypeDescription.Contains("Cc") && 
                o.GenotypeDescription.Contains("HH"));
            Assert.IsTrue(coloredSelf.Phenotype.Contains("Normal pigmentation"));
            Assert.IsTrue(coloredSelf.Phenotype.Contains("Full body"));

            var albinoHooded = offspring.Single(o => 
                o.GenotypeDescription.Contains("cc") && 
                o.GenotypeDescription.Contains("hh"));
            Assert.IsTrue(albinoHooded.Phenotype.Contains("White coat"));
            Assert.IsTrue(albinoHooded.Phenotype.Contains("stripe"));
        }

        [TestMethod]
        public async Task AnalyzeGeneticRisks_VariableExpressivity_IncludesWarning()
        {
            // Arrange - Set up parents with hooded pattern
            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testDam.Id,
                ChromosomePairId = _patternChromosomePair.PairId,
                MaternalAlleleId = _patternRecessive.AlleleId,
                PaternalAlleleId = _patternRecessive.AlleleId
            });

            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testSire.Id,
                ChromosomePairId = _patternChromosomePair.PairId,
                MaternalAlleleId = _patternRecessive.AlleleId,
                PaternalAlleleId = _patternRecessive.AlleleId
            });

            var calculation = await CreateTestBreedingCalculationAsync();

            // Act
            var risks = await _breedingCalculationService.AnalyzeGeneticRisksAsync(
                calculation.CalculationId
            );

            // Assert
            Assert.AreEqual(1, risks.Count);
            var risk = risks[0];
            Assert.AreEqual(_patternGene.Name, risk.GeneName);
            Assert.IsTrue(risk.Description.Contains("variable"));
            Assert.AreEqual(1.0f, risk.Probability);
        }
    }
}
