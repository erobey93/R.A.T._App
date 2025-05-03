using Microsoft.VisualStudio.TestTools.UnitTesting;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibraryUT.Genetics.Base;
using System;
using System.Threading.Tasks;
using RATAPPLibrary.Data.Models;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace RATAPPLibraryUT.Genetics.ChromosomeServiceTests
{
    [TestClass]
    public class CreateTests : GenomicsTestBase
    {
        [TestMethod]
        public async Task CreateChromosome_ValidData_Success()
        {
            // Arrange
            string name = "Test Chromosome";
            int number = 1;

            // Act
            var chromosome = await _chromosomeService.CreateChromosomeAsync(
                name: name,
                number: number,
                speciesId: _testSpecies.Id
            );

            // Assert
            Assert.IsNotNull(chromosome);
            Assert.AreEqual(name, chromosome.Name);
            Assert.AreEqual(number, chromosome.Number);
            Assert.AreEqual(_testSpecies.Id, chromosome.SpeciesId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CreateChromosome_DuplicateNumber_ThrowsException()
        {
            // Arrange
            int number = 1;

            // Act
            await _chromosomeService.CreateChromosomeAsync(
                name: "First Chromosome",
                number: number,
                speciesId: _testSpecies.Id
            );

            await _chromosomeService.CreateChromosomeAsync(
                name: "Second Chromosome",
                number: number, // Same number
                speciesId: _testSpecies.Id
            );
        }

        [TestMethod]
        public async Task CreateChromosomePair_ValidData_Success()
        {
            // Arrange
            var (maternal, paternal) = await CreateTestChromosomePairAsync();

            // Act
            var pair = await _chromosomeService.CreateChromosomePairAsync(
                maternalId: maternal.ChromosomeId,
                paternalId: paternal.ChromosomeId,
                inheritancePattern: "autosomal"
            );

            // Assert
            Assert.IsNotNull(pair);
            Assert.AreEqual(maternal.ChromosomeId, pair.MaternalChromosomeId);
            Assert.AreEqual(paternal.ChromosomeId, pair.PaternalChromosomeId);
            Assert.AreEqual("autosomal", pair.InheritancePattern);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CreateChromosomePair_DifferentSpecies_ThrowsException()
        {
            // Arrange
            var maternal = await _chromosomeService.CreateChromosomeAsync(
                name: "Maternal",
                number: 1,
                speciesId: _testSpecies.Id
            );

            var otherSpecies = new Species
            {
                CommonName = "Other Species",
                ScientificName = "Other test"
            };
            _context.Species.Add(otherSpecies);
            await _context.SaveChangesAsync();

            var paternal = await _chromosomeService.CreateChromosomeAsync(
                name: "Paternal",
                number: 1,
                speciesId: otherSpecies.Id
            );

            // Act
            await _chromosomeService.CreateChromosomePairAsync(
                maternalId: maternal.ChromosomeId,
                paternalId: paternal.ChromosomeId,
                inheritancePattern: "autosomal"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CreateChromosomePair_DifferentNumbers_ThrowsException()
        {
            // Arrange
            var maternal = await _chromosomeService.CreateChromosomeAsync(
                name: "Maternal",
                number: 1,
                speciesId: _testSpecies.Id
            );

            var paternal = await _chromosomeService.CreateChromosomeAsync(
                name: "Paternal",
                number: 2, // Different number
                speciesId: _testSpecies.Id
            );

            // Act
            await _chromosomeService.CreateChromosomePairAsync(
                maternalId: maternal.ChromosomeId,
                paternalId: paternal.ChromosomeId,
                inheritancePattern: "autosomal"
            );
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CreateChromosomePair_InvalidInheritancePattern_ThrowsException()
        {
            // Arrange
            var (maternal, paternal) = await CreateTestChromosomePairAsync();

            // Act
            await _chromosomeService.CreateChromosomePairAsync(
                maternalId: maternal.ChromosomeId,
                paternalId: paternal.ChromosomeId,
                inheritancePattern: "invalid_pattern"
            );
        }
    }
}
