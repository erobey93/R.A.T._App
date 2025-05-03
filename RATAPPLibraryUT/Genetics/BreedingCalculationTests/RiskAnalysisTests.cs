using Microsoft.VisualStudio.TestTools.UnitTesting;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Services.Genetics.Interfaces;
using RATAPPLibraryUT.Genetics.Base;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RATAPPLibraryUT.Genetics.BreedingCalculationTests
{
    [TestClass]
    public class RiskAnalysisTests : GenomicsTestBase
    {
        private ChromosomePair _testChromosomePair;
        private Gene _healthGene;
        private Allele _healthyAllele;
        private Allele _riskAllele;

        [TestInitialize]
        public override async Task TestInitialize()
        {
            await base.TestInitialize();

            // Create test chromosome pair
            var (maternal, paternal) = await CreateTestChromosomePairAsync();
            _testChromosomePair = await _chromosomeService.CreateChromosomePairAsync(
                maternal.ChromosomeId,
                paternal.ChromosomeId,
                "autosomal"
            );

            // Create gene for health condition
            _healthGene = await _geneService.CreateGeneAsync(new CreateGeneRequest
            {
                Name = "PKD1",
                CommonName = "Polycystic Kidney Disease 1",
                ChromosomePairId = _testChromosomePair.PairId,
                Position = 1,
                Category = "medical",
                ImpactLevel = "critical",
                ExpressionAge = "adult",
                Penetrance = "complete",
                Expressivity = "variable",
                RequiresMonitoring = true
            });

            _healthyAllele = await _geneService.CreateAlleleAsync(new CreateAlleleRequest
            {
                GeneId = _healthGene.GeneId,
                Name = "Normal",
                Symbol = "P",
                IsWildType = true,
                Phenotype = "Normal kidney function",
                RiskLevel = "none",
                ManagementNotes = "No special care needed"
            });

            _riskAllele = await _geneService.CreateAlleleAsync(new CreateAlleleRequest
            {
                GeneId = _healthGene.GeneId,
                Name = "PKD Variant",
                Symbol = "p",
                IsWildType = false,
                Phenotype = "Polycystic kidney disease",
                RiskLevel = "high",
                ManagementNotes = "Regular kidney function monitoring required"
            });
        }

        [TestMethod]
        public async Task ValidateBreedingPair_BothCarriers_NotCompatible()
        {
            // Arrange - Set up Pp x Pp parents
            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testDam.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _healthyAllele.AlleleId,
                PaternalAlleleId = _riskAllele.AlleleId
            });

            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testSire.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _healthyAllele.AlleleId,
                PaternalAlleleId = _riskAllele.AlleleId
            });

            // Act
            var result = await _breedingCalculationService.ValidateBreedingPairAsync(
                _testDam.Id,
                _testSire.Id
            );

            // Assert
            Assert.IsFalse(result.IsCompatible);
            Assert.IsTrue(result.Risks.Any(r => r.Contains("25% chance of affected offspring")));
        }

        [TestMethod]
        public async Task AnalyzeGeneticRisks_CarrierParents_CorrectProbabilities()
        {
            // Arrange - Set up Pp x Pp parents
            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testDam.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _healthyAllele.AlleleId,
                PaternalAlleleId = _riskAllele.AlleleId
            });

            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testSire.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _healthyAllele.AlleleId,
                PaternalAlleleId = _riskAllele.AlleleId
            });

            var calculation = await CreateTestBreedingCalculationAsync();

            // Act
            var risks = await _breedingCalculationService.AnalyzeGeneticRisksAsync(
                calculation.CalculationId
            );

            // Assert
            Assert.AreEqual(1, risks.Count);
            var risk = risks[0];
            Assert.AreEqual(_healthGene.Name, risk.GeneName);
            Assert.AreEqual("high", risk.RiskLevel);
            Assert.AreEqual(0.25f, risk.Probability); // 25% chance of pp
            Assert.IsTrue(risk.ManagementRecommendation.Contains("monitoring"));
        }

        [TestMethod]
        public async Task GetTraitProbabilities_MedicalCondition_IncludesCarrierStatus()
        {
            // Arrange - Set up PP x pp parents
            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testDam.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _healthyAllele.AlleleId,
                PaternalAlleleId = _healthyAllele.AlleleId
            });

            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testSire.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _riskAllele.AlleleId,
                PaternalAlleleId = _riskAllele.AlleleId
            });

            var calculation = await CreateTestBreedingCalculationAsync();

            // Act
            var probabilities = await _breedingCalculationService.GetTraitProbabilitiesAsync(
                calculation.CalculationId
            );

            // Assert
            Assert.IsTrue(probabilities.ContainsKey("PKD1 Carrier"));
            Assert.AreEqual(1.0f, probabilities["PKD1 Carrier"]); // All offspring will be carriers
        }
    }
}
