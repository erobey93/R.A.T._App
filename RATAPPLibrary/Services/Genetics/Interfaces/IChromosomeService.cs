using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RATAPPLibrary.Data.Models.Genetics;

namespace RATAPPLibrary.Services.Genetics.Interfaces
{
    /// <summary>
    /// Service for managing chromosomes and chromosome pairs
    /// </summary>
    public interface IChromosomeService
    {
        /// <summary>
        /// Creates a new chromosome for a specific species
        /// </summary>
        /// <param name="name">Name of the chromosome</param>
        /// <param name="number">Chromosome number within the species</param>
        /// <param name="speciesId">ID of the species this chromosome belongs to</param>
        /// <returns>The created chromosome</returns>
        /// <exception cref="InvalidOperationException">Thrown when a chromosome with the same number already exists for the species</exception>
        Task<Chromosome> CreateChromosomeAsync(string name, int number, int speciesId);

        /// <summary>
        /// Creates a chromosome pair from maternal and paternal chromosomes
        /// </summary>
        /// <param name="maternalId">ID of the maternal chromosome</param>
        /// <param name="paternalId">ID of the paternal chromosome</param>
        /// <param name="inheritancePattern">Pattern of inheritance (e.g., "autosomal", "x-linked")</param>
        /// <returns>The created chromosome pair</returns>
        /// <exception cref="InvalidOperationException">Thrown when chromosomes are incompatible or don't exist</exception>
        Task<ChromosomePair> CreateChromosomePairAsync(Guid maternalId, Guid paternalId, string inheritancePattern);

        /// <summary>
        /// Gets all chromosomes for a specific species
        /// </summary>
        /// <param name="speciesId">ID of the species</param>
        /// <returns>List of chromosomes for the species</returns>
        Task<List<Chromosome>> GetChromosomesBySpeciesAsync(int speciesId);

        /// <summary>
        /// Gets a specific chromosome pair by ID
        /// </summary>
        /// <param name="pairId">ID of the chromosome pair</param>
        /// <returns>The chromosome pair if found, null otherwise</returns>
        Task<ChromosomePair> GetChromosomePairAsync(Guid pairId);

        /// <summary>
        /// Gets all chromosome pairs associated with an animal
        /// </summary>
        /// <param name="animalId">ID of the animal</param>
        /// <returns>List of chromosome pairs for the animal</returns>
        Task<List<ChromosomePair>> GetChromosomePairsForAnimalAsync(int animalId);

        /// <summary>
        /// Validates if a chromosome number is available for a species
        /// </summary>
        /// <param name="number">Chromosome number to validate</param>
        /// <param name="speciesId">ID of the species</param>
        /// <returns>True if valid, throws exception if invalid</returns>
        /// <exception cref="InvalidOperationException">Thrown when validation fails</exception>
        Task ValidateChromosomeNumberAsync(int number, int speciesId);

        /// <summary>
        /// Validates if two chromosomes can form a pair
        /// </summary>
        /// <param name="maternalId">ID of the maternal chromosome</param>
        /// <param name="paternalId">ID of the paternal chromosome</param>
        /// <returns>True if valid, throws exception if invalid</returns>
        /// <exception cref="InvalidOperationException">Thrown when validation fails</exception>
        Task ValidateChromosomePairAsync(Guid maternalId, Guid paternalId);
    }
}
