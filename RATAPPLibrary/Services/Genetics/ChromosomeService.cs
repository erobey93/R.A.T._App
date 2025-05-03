using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Services.Genetics.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RATAPPLibrary.Services.Genetics
{
    public class ChromosomeService : IChromosomeService
    {
        private readonly RatAppDbContext _context;
        private readonly string[] _validInheritancePatterns = { "autosomal", "x-linked", "y-linked", "mitochondrial" };

        public ChromosomeService(RatAppDbContext context)
        {
            _context = context;
        }

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
