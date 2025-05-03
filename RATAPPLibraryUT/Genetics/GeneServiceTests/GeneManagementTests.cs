using Microsoft.VisualStudio.TestTools.UnitTesting;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Services.Genetics.Interfaces;
using RATAPPLibraryUT.Genetics.Base;
using System;
using System.Threading.Tasks;

namespace RATAPPLibraryUT.Genetics.GeneServiceTests
{
    [TestClass]
    public class GeneManagementTests : GenomicsTestBase
    {
        private ChromosomePair _testChromosomePair;

        [TestInitialize]
        public override async Task TestInitialize()
        {
            await base.TestInitialize();

            // Create a test chromosome pair
            var (maternal, paternal) = await CreateTestChromosomePairAsync();
            _testChromosomePair = await _chromosomeService.CreateChromosomePairAsync(
                maternal.ChromosomeId,
                paternal.ChromosomeId,
                "autosomal"
            );
        }

        [TestMethod]
        public async Task CreateGene_ValidData_Success()
        {
            // Arrange
            var request = new CreateGeneRequest
            {
                Name = "TYR",
                CommonName = "Albino locus",
                ChromosomePairId = _testChromosomePair.PairId,
                Position = 1,
                Category = "physical",
                ImpactLevel = "cosmetic",
                ExpressionAge = "birth",
                Penetrance = "complete",
                Expressivity = "consistent",
                RequiresMonitoring = false
            };

            // Act
            var gene = await _geneService.CreateGeneAsync(request);

            // Assert
            Assert.IsNotNull(gene);
            Assert.AreEqual(request.Name, gene.Name);
            Assert.AreEqual(request.CommonName, gene.CommonName);
            Assert.AreEqual(request.ChromosomePairId, gene.ChromosomePairId);
            Assert.AreEqual(request.Position, gene.Position);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CreateGene_DuplicatePosition_ThrowsException()
        {
            // Arrange
            var request1 = new CreateGeneRequest
            {
                Name = "Gene1",
                CommonName = "First Gene",
                ChromosomePairId = _testChromosomePair.PairId,
                Position = 1,
                Category = "physical",
                ImpactLevel = "cosmetic",
                ExpressionAge = "birth",
                Penetrance = "complete",
                Expressivity = "consistent"
            };

            var request2 = new CreateGeneRequest
            {
                Name = "Gene2",
                CommonName = "Second Gene",
                ChromosomePairId = _testChromosomePair.PairId,
                Position = 1, // Same position
                Category = "physical",
                ImpactLevel = "cosmetic",
                ExpressionAge = "birth",
                Penetrance = "complete",
                Expressivity = "consistent"
            };

            // Act
            await _geneService.CreateGeneAsync(request1);
            await _geneService.CreateGeneAsync(request2);
        }

        [TestMethod]
        public async Task GetGenesForChromosomePair_ReturnsCorrectGenes()
        {
            // Arrange
            var gene1 = await CreateTestGeneAsync(_testChromosomePair.PairId);
            var gene2 = await CreateTestGeneAsync(_testChromosomePair.PairId);

            // Act
            var genes = await _geneService.GetGenesForChromosomePairAsync(_testChromosomePair.PairId);

            // Assert
            Assert.AreEqual(2, genes.Count);
            CollectionAssert.Contains(genes, gene1);
            CollectionAssert.Contains(genes, gene2);
        }

        [TestMethod]
        public async Task GetGeneByName_ExistingGene_ReturnsCorrectGene()
        {
            // Arrange
            var gene = await CreateTestGeneAsync(_testChromosomePair.PairId);

            // Act
            var result = await _geneService.GetGeneByNameAsync(gene.Name);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(gene.GeneId, result.GeneId);
            Assert.AreEqual(gene.Name, result.Name);
        }

        [TestMethod]
        public async Task GetGeneByName_NonexistentGene_ReturnsNull()
        {
            // Act
            var result = await _geneService.GetGeneByNameAsync("NonexistentGene");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetGenesWithEffect_ReturnsMatchingGenes()
        {
            // Arrange
            var request1 = new CreateGeneRequest
            {
                Name = "Gene1",
                CommonName = "Medical Gene",
                ChromosomePairId = _testChromosomePair.PairId,
                Position = 1,
                Category = "medical",
                ImpactLevel = "critical",
                ExpressionAge = "birth",
                Penetrance = "complete",
                Expressivity = "consistent"
            };

            var request2 = new CreateGeneRequest
            {
                Name = "Gene2",
                CommonName = "Physical Gene",
                ChromosomePairId = _testChromosomePair.PairId,
                Position = 2,
                Category = "physical",
                ImpactLevel = "cosmetic",
                ExpressionAge = "birth",
                Penetrance = "complete",
                Expressivity = "consistent"
            };

            await _geneService.CreateGeneAsync(request1);
            await _geneService.CreateGeneAsync(request2);

            // Act
            var medicalGenes = await _geneService.GetGenesWithEffectAsync("medical", "critical");

            // Assert
            Assert.AreEqual(1, medicalGenes.Count);
            Assert.AreEqual("Gene1", medicalGenes[0].Name);
        }
    }
}
