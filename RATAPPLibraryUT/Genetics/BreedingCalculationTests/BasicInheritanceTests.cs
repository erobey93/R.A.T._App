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
    public class BasicInheritanceTests : GenomicsTestBase
    {
        private ChromosomePair _testChromosomePair;
        private Gene _testGene;
        private Allele _dominantAllele;
        private Allele _recessiveAllele;

        [TestInitialize]
        public override async Task TestInitialize()
        {
            await base.TestInitialize();

            // Create test chromosome pair and gene
            var (maternal, paternal) = await CreateTestChromosomePairAsync();
            _testChromosomePair = await _chromosomeService.CreateChromosomePairAsync(
                maternal.ChromosomeId,
                paternal.ChromosomeId,
                "autosomal"
            );

            // Create gene for coat color
            _testGene = await _geneService.CreateGeneAsync(new CreateGeneRequest
            {
                Name = "TYR",
                CommonName = "Albino locus",
                ChromosomePairId = _testChromosomePair.PairId,
                Position = 1,
                Category = "physical",
                ImpactLevel = "cosmetic",
                ExpressionAge = "birth",
                Penetrance = "complete",
                Expressivity = "consistent"
            });

            // Create alleles (C = colored, c = albino)
            _dominantAllele = await _geneService.CreateAlleleAsync(new CreateAlleleRequest
            {
                GeneId = _testGene.GeneId,
                Name = "Wild Type",
                Symbol = "C",
                IsWildType = true,
                Phenotype = "Normal pigmentation",
                RiskLevel = "none"
            });

            _recessiveAllele = await _geneService.CreateAlleleAsync(new CreateAlleleRequest
            {
                GeneId = _testGene.GeneId,
                Name = "Albino",
                Symbol = "c",
                IsWildType = false,
                Phenotype = "White coat, red eyes",
                RiskLevel = "none"
            });
        }

        [TestMethod]
        public async Task CalculateOffspring_HeterozygousParents_CorrectRatios()
        {
            // Arrange - Set up Cc x Cc parents
            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testDam.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _dominantAllele.AlleleId,
                PaternalAlleleId = _recessiveAllele.AlleleId
            });

            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testSire.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _dominantAllele.AlleleId,
                PaternalAlleleId = _recessiveAllele.AlleleId
            });

            var calculation = await CreateTestBreedingCalculationAsync();

            // Act
            var offspring = await _breedingCalculationService.CalculateOffspringProbabilitiesAsync(
                calculation.CalculationId
            );

            // Assert
            Assert.AreEqual(3, offspring.Count); // CC, Cc, cc

            var homozygousDominant = offspring.Single(o => o.GenotypeDescription == "CC");
            var heterozygous = offspring.Single(o => o.GenotypeDescription == "Cc");
            var homozygousRecessive = offspring.Single(o => o.GenotypeDescription == "cc");

            Assert.AreEqual(0.25f, homozygousDominant.Probability);
            Assert.AreEqual(0.5f, heterozygous.Probability);
            Assert.AreEqual(0.25f, homozygousRecessive.Probability);

            Assert.IsTrue(homozygousDominant.Phenotype.Contains("Normal pigmentation"));
            Assert.IsTrue(heterozygous.Phenotype.Contains("Normal pigmentation"));
            Assert.IsTrue(homozygousRecessive.Phenotype.Contains("White coat"));
        }

        [TestMethod]
        public async Task CalculateOffspring_HomozygousParents_SingleOutcome()
        {
            // Arrange - Set up CC x cc parents
            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testDam.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _dominantAllele.AlleleId,
                PaternalAlleleId = _dominantAllele.AlleleId
            });

            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testSire.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _recessiveAllele.AlleleId,
                PaternalAlleleId = _recessiveAllele.AlleleId
            });

            var calculation = await CreateTestBreedingCalculationAsync();

            // Act
            var offspring = await _breedingCalculationService.CalculateOffspringProbabilitiesAsync(
                calculation.CalculationId
            );

            // Assert
            Assert.AreEqual(1, offspring.Count);
            var heterozygous = offspring.Single();
            Assert.AreEqual("Cc", heterozygous.GenotypeDescription);
            Assert.AreEqual(1.0f, heterozygous.Probability);
            Assert.IsTrue(heterozygous.Phenotype.Contains("Normal pigmentation"));
        }

        [TestMethod]
        public async Task ValidateBreedingPair_NoRisks_Compatible()
        {
            // Arrange - Set up healthy parents
            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testDam.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _dominantAllele.AlleleId,
                PaternalAlleleId = _dominantAllele.AlleleId
            });

            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testSire.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _dominantAllele.AlleleId,
                PaternalAlleleId = _recessiveAllele.AlleleId
            });

            // Act
            var result = await _breedingCalculationService.ValidateBreedingPairAsync(
                _testDam.Id,
                _testSire.Id
            );

            // Assert
            Assert.IsTrue(result.IsCompatible);
            Assert.AreEqual(0, result.Warnings.Count);
            Assert.AreEqual(0, result.Risks.Count);
        }
    }
}
