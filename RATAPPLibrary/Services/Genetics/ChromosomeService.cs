using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Services.Genetics.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RATAPPLibrary.Services.Genetics
{
    /// <summary>
    /// Service for managing chromosomes and chromosome pairs in the R.A.T. App.
    /// Handles the creation, validation, and organization of genetic material
    /// at the chromosome level.
    /// 
    /// Key Features:
    /// - Chromosome Management:
    ///   * Create and track chromosomes
    ///   * Manage chromosome pairs
    ///   * Validate genetic compatibility
    /// 
    /// Inheritance Patterns:
    /// - Supported Types:
    ///   * Autosomal
    ///   * X-linked
    ///   * Y-linked
    ///   * Mitochondrial
    /// 
    /// Validation Rules:
    /// - Chromosome numbers must be unique per species
    /// - Pairs must be from same species
    /// - Pairs must have matching chromosome numbers
    /// - Valid inheritance patterns only
    /// 
    /// Data Structure:
    /// - Chromosome: Individual genetic unit
    /// - ChromosomePair: Maternal/Paternal pair
    /// - Tracks creation and update timestamps
    /// 
    /// Dependencies:
    /// - RatAppDbContext for data access
    /// - Implements IChromosomeService
    /// </summary>
    public class ChromosomeService : IChromosomeService
    {
        private readonly RatAppDbContext _context;
        private readonly string[] _validInheritancePatterns = { "autosomal", "x-linked", "y-linked", "mitochondrial" };

        public ChromosomeService(RatAppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new chromosome for a specific species.
        /// 
        /// Validation:
        /// - Name cannot be empty
        /// - Number must be positive
        /// - Number must be unique per species
        /// 
        /// Process:
        /// 1. Validates input parameters
        /// 2. Checks for existing chromosome
        /// 3. Creates new chromosome with UUID
        /// 4. Sets creation timestamps
        /// 
        /// Throws:
        /// - ArgumentException for invalid inputs
        /// - InvalidOperationException for duplicates
        /// </summary>
        /// <param name="name">Chromosome identifier</param>
        /// <param name="number">Chromosome number</param>
        /// <param name="speciesId">Associated species</param>
        /// <returns>Created Chromosome object</returns>
        public async Task<Chromosome> CreateChromosomeAsync(string name, int number, int speciesId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));

            if (number <= 0)
                throw new ArgumentException("Number must be positive", nameof(number));

            // Check if chromosome number already exists for this species
            var exists = await _context.Chromosomes
                .AnyAsync(c => c.Number == number && c.SpeciesId == speciesId);

            if (exists)
                throw new InvalidOperationException($"Chromosome number {number} already exists for species {speciesId}");

            var chromosome = new Chromosome
            {
                ChromosomeId = Guid.NewGuid(),
                Name = name,
                Number = number,
                SpeciesId = speciesId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Chromosomes.Add(chromosome);
            await _context.SaveChangesAsync();

            return chromosome;
        }

        /// <summary>
        /// Creates a chromosome pair from maternal and paternal chromosomes.
        /// 
        /// Process:
        /// 1. Validates chromosome compatibility
        /// 2. Verifies inheritance pattern
        /// 3. Creates pair with unique ID
        /// 
        /// Inheritance Patterns:
        /// - autosomal: Standard chromosome pairs
        /// - x-linked: Sex chromosome X inheritance
        /// - y-linked: Sex chromosome Y inheritance
        /// - mitochondrial: Maternal inheritance
        /// 
        /// Validation:
        /// - Both chromosomes must exist
        /// - Must be from same species
        /// - Must have matching numbers
        /// - Pattern must be valid
        /// 
        /// Throws:
        /// - InvalidOperationException for validation failures
        /// </summary>
        public async Task<ChromosomePair> CreateChromosomePairAsync(
            Guid maternalId,
            Guid paternalId,
            string inheritancePattern)
        {
            await ValidateChromosomePairAsync(maternalId, paternalId);

            if (!Array.Exists(_validInheritancePatterns, p => p == inheritancePattern.ToLower()))
                throw new InvalidOperationException($"Invalid inheritance pattern: {inheritancePattern}");

            var pair = new ChromosomePair
            {
                PairId = Guid.NewGuid(),
                MaternalChromosomeId = maternalId,
                PaternalChromosomeId = paternalId,
                InheritancePattern = inheritancePattern.ToLower(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ChromosomePairs.Add(pair);
            await _context.SaveChangesAsync();

            return pair;
        }

        public async Task<List<Chromosome>> GetChromosomesBySpeciesAsync(int speciesId)
        {
            return await _context.Chromosomes
                .Where(c => c.SpeciesId == speciesId)
                .OrderBy(c => c.Number)
                .ToListAsync();
        }

        public async Task<ChromosomePair> GetChromosomePairAsync(Guid pairId)
        {
            return await _context.ChromosomePairs
                .Include(cp => cp.MaternalChromosome)
                .Include(cp => cp.PaternalChromosome)
                .FirstOrDefaultAsync(cp => cp.PairId == pairId);
        }

        public async Task<List<ChromosomePair>> GetChromosomePairsForAnimalAsync(int animalId)
        {
            return await _context.Genotypes
                .Where(g => g.AnimalId == animalId)
                .Include(g => g.ChromosomePair)
                    .ThenInclude(cp => cp.MaternalChromosome)
                .Include(g => g.ChromosomePair)
                    .ThenInclude(cp => cp.PaternalChromosome)
                .Select(g => g.ChromosomePair)
                .ToListAsync();
        }

        public async Task ValidateChromosomeNumberAsync(int number, int speciesId)
        {
            var exists = await _context.Chromosomes
                .AnyAsync(c => c.Number == number && c.SpeciesId == speciesId);

            if (exists)
                throw new InvalidOperationException($"Chromosome number {number} already exists for species {speciesId}");
        }

        /// <summary>
        /// Validates that two chromosomes can form a valid pair.
        /// 
        /// Checks:
        /// 1. Both chromosomes exist
        /// 2. Same species
        /// 3. Matching chromosome numbers
        /// 
        /// Used By:
        /// - CreateChromosomePairAsync
        /// - Other services needing pair validation
        /// 
        /// Throws:
        /// - InvalidOperationException if validation fails
        /// </summary>
        /// <param name="maternalId">Maternal chromosome ID</param>
        /// <param name="paternalId">Paternal chromosome ID</param>
        public async Task ValidateChromosomePairAsync(Guid maternalId, Guid paternalId)
        {
            var maternal = await _context.Chromosomes.FindAsync(maternalId);
            var paternal = await _context.Chromosomes.FindAsync(paternalId);

            if (maternal == null || paternal == null)
                throw new InvalidOperationException("Both chromosomes must exist");

            if (maternal.SpeciesId != paternal.SpeciesId)
                throw new InvalidOperationException("Chromosomes must be from the same species");

            if (maternal.Number != paternal.Number)
                throw new InvalidOperationException("Chromosomes must have the same number to form a pair");
        }
    }
}
