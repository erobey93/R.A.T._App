using Microsoft.VisualStudio.TestTools.UnitTesting;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Services.Genetics.Interfaces;
using RATAPPLibraryUT.Genetics.Base;
using System;
using System.Threading.Tasks;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace RATAPPLibraryUT.Genetics.GeneServiceTests
{
    [TestClass]
    public class AlleleManagementTests : GenomicsTestBase
    {
        private ChromosomePair _testChromosomePair;
        private Gene _testGene;

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
            _testGene = await CreateTestGeneAsync(_testChromosomePair.PairId);
        }

        [TestMethod]
        public async Task CreateAllele_ValidData_Success()
        {
            // Arrange
            var request = new CreateAlleleRequest
            {
                GeneId = _testGene.GeneId,
                Name = "Albino",
                Symbol = "c",
                IsWildType = false,
                Phenotype = "White coat, red eyes",
                RiskLevel = "none",
                ManagementNotes = "No special care needed"
            };

            // Act
            var allele = await _geneService.CreateAlleleAsync(request);

            // Assert
            Assert.IsNotNull(allele);
            Assert.AreEqual(request.Name, allele.Name);
            Assert.AreEqual(request.Symbol, allele.Symbol);
            Assert.AreEqual(request.IsWildType, allele.IsWildType);
            Assert.AreEqual(request.Phenotype, allele.Phenotype);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CreateAllele_DuplicateSymbol_ThrowsException()
        {
            // Arrange
            var request1 = new CreateAlleleRequest
            {
                GeneId = _testGene.GeneId,
                Name = "First Allele",
                Symbol = "a",
                IsWildType = false,
                Phenotype = "Test Phenotype 1",
                RiskLevel = "none"
            };

            var request2 = new CreateAlleleRequest
            {
                GeneId = _testGene.GeneId,
                Name = "Second Allele",
                Symbol = "a", // Same symbol
                IsWildType = false,
                Phenotype = "Test Phenotype 2",
                RiskLevel = "none"
            };

            // Act
            await _geneService.CreateAlleleAsync(request1);
            await _geneService.CreateAlleleAsync(request2);
        }

        [TestMethod]
        public async Task GetAllelesForGene_ReturnsCorrectAlleles()
        {
            // Arrange
            var allele1 = await CreateTestAlleleAsync(_testGene.GeneId, true);
            var allele2 = await CreateTestAlleleAsync(_testGene.GeneId, false);

            // Act
            var alleles = await _geneService.GetAllelesForGeneAsync(_testGene.GeneId);

            // Assert
            Assert.AreEqual(2, alleles.Count);
            CollectionAssert.Contains(alleles, allele1);
            CollectionAssert.Contains(alleles, allele2);
        }

        [TestMethod]
        public async Task GetAlleleBySymbol_ExistingAllele_ReturnsCorrectAllele()
        {
            // Arrange
            var allele = await CreateTestAlleleAsync(_testGene.GeneId);

            // Act
            var result = await _geneService.GetAlleleBySymbolAsync(_testGene.GeneId, allele.Symbol);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(allele.AlleleId, result.AlleleId);
            Assert.AreEqual(allele.Symbol, result.Symbol);
        }

        [TestMethod]
        public async Task GetAlleleBySymbol_NonexistentAllele_ReturnsNull()
        {
            // Act
            var result = await _geneService.GetAlleleBySymbolAsync(_testGene.GeneId, "x");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CreateAllele_MultipleWildType_ThrowsException()
        {
            // Arrange
            var request1 = new CreateAlleleRequest
            {
                GeneId = _testGene.GeneId,
                Name = "Wild Type 1",
                Symbol = "+",
                IsWildType = true,
                Phenotype = "Normal",
                RiskLevel = "none"
            };

            var request2 = new CreateAlleleRequest
            {
                GeneId = _testGene.GeneId,
                Name = "Wild Type 2",
                Symbol = "++",
                IsWildType = true, // Second wild type
                Phenotype = "Also Normal",
                RiskLevel = "none"
            };

            // Act & Assert
            await _geneService.CreateAlleleAsync(request1);
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => _geneService.CreateAlleleAsync(request2)
            );
        }
    }
}
