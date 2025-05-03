using Microsoft.VisualStudio.TestTools.UnitTesting;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Services.Genetics.Interfaces;
using RATAPPLibraryUT.Genetics.Base;
using System;
using System.Threading.Tasks;

namespace RATAPPLibraryUT.Genetics.GeneServiceTests
{
    [TestClass]
    public class GenotypeTests : GenomicsTestBase
    {
        private ChromosomePair _testChromosomePair;
        private Gene _testGene;
        private Allele _wildTypeAllele;
        private Allele _variantAllele;

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
            _wildTypeAllele = await CreateTestAlleleAsync(_testGene.GeneId, true);
            _variantAllele = await CreateTestAlleleAsync(_testGene.GeneId, false);
        }

        [TestMethod]
        public async Task AssignGenotype_ValidData_Success()
        {
            // Arrange
            var request = new AssignGenotypeRequest
            {
                AnimalId = _testDam.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _wildTypeAllele.AlleleId,
                PaternalAlleleId = _variantAllele.AlleleId
            };

            // Act
            var genotype = await _geneService.AssignGenotypeToAnimalAsync(request);

            // Assert
            Assert.IsNotNull(genotype);
            Assert.AreEqual(_testDam.Id, genotype.AnimalId);
            Assert.AreEqual(_testChromosomePair.PairId, genotype.ChromosomePairId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task AssignGenotype_DifferentGenes_ThrowsException()
        {
            // Arrange
            var otherGene = await CreateTestGeneAsync(_testChromosomePair.PairId);
            var otherAllele = await CreateTestAlleleAsync(otherGene.GeneId);

            var request = new AssignGenotypeRequest
            {
                AnimalId = _testDam.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _wildTypeAllele.AlleleId,
                PaternalAlleleId = otherAllele.AlleleId // Allele from different gene
            };

            // Act
            await _geneService.AssignGenotypeToAnimalAsync(request);
        }

        [TestMethod]
        public async Task GetGeneMapForAnimal_ReturnsCorrectMap()
        {
            // Arrange
            await _geneService.AssignGenotypeToAnimalAsync(new AssignGenotypeRequest
            {
                AnimalId = _testDam.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _wildTypeAllele.AlleleId,
                PaternalAlleleId = _variantAllele.AlleleId
            });

            // Act
            var geneMap = await _geneService.GetGeneMapForAnimalAsync(_testDam.Id);

            // Assert
            Assert.IsTrue(geneMap.ContainsKey(_testGene.Name));
            var alleles = geneMap[_testGene.Name];
            Assert.AreEqual(2, alleles.Count);
            CollectionAssert.Contains(alleles, _wildTypeAllele.Name);
            CollectionAssert.Contains(alleles, _variantAllele.Name);
        }

        [TestMethod]
        public async Task GetGeneMapForAnimal_NoGenotype_ReturnsEmptyMap()
        {
            // Act
            var geneMap = await _geneService.GetGeneMapForAnimalAsync(_testDam.Id);

            // Assert
            Assert.AreEqual(0, geneMap.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task AssignGenotype_DuplicateAssignment_ThrowsException()
        {
            // Arrange
            var request = new AssignGenotypeRequest
            {
                AnimalId = _testDam.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _wildTypeAllele.AlleleId,
                PaternalAlleleId = _variantAllele.AlleleId
            };

            // Act
            await _geneService.AssignGenotypeToAnimalAsync(request);
            await _geneService.AssignGenotypeToAnimalAsync(request); // Duplicate assignment
        }

        [TestMethod]
        public async Task AssignGenotype_UpdateExisting_Success()
        {
            // Arrange
            var initialRequest = new AssignGenotypeRequest
            {
                AnimalId = _testDam.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _wildTypeAllele.AlleleId,
                PaternalAlleleId = _variantAllele.AlleleId
            };

            var updateRequest = new AssignGenotypeRequest
            {
                AnimalId = _testDam.Id,
                ChromosomePairId = _testChromosomePair.PairId,
                MaternalAlleleId = _variantAllele.AlleleId,
                PaternalAlleleId = _variantAllele.AlleleId
            };

            // Act
            await _geneService.AssignGenotypeToAnimalAsync(initialRequest);
            var updatedGenotype = await _geneService.AssignGenotypeToAnimalAsync(updateRequest);

            // Assert
            var geneMap = await _geneService.GetGeneMapForAnimalAsync(_testDam.Id);
            var alleles = geneMap[_testGene.Name];
            Assert.AreEqual(2, alleles.Count);
            Assert.AreEqual(_variantAllele.Name, alleles[0]);
            Assert.AreEqual(_variantAllele.Name, alleles[1]);
        }
    }
}
