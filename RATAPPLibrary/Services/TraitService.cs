using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Genetics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RATAPPLibrary.Services
{
    public class TraitService : BaseService
    {
        public TraitService(RatAppDbContextFactory contextFactory) : base(contextFactory)
        {
        }

        public async Task<List<Trait>> GetAllTraitsAsync()
        {
            return await ExecuteInContextAsync(async context =>
            {
                return await context.Trait
                    .Include(t => t.TraitType)
                    .Include(t => t.Species)
                    .ToListAsync();
            });
        }

        public async Task<List<TraitType>> GetAllTraitTypesAsync()
        {
            return await ExecuteInContextAsync(async context =>
            {
                return await context.TraitType.ToListAsync();
            });
        }

        public async Task<Trait> GetTraitByIdAsync(int id)
        {
            return await ExecuteInContextAsync(async context =>
            {
                var trait = await context.Trait
                    .Include(t => t.TraitType)
                    .Include(t => t.Species)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (trait == null)
                {
                    throw new KeyNotFoundException($"Trait with ID {id} not found.");
                }

                return trait;
            });
        }

        public async Task<Trait> GetTraitByNameAsync(string name)
        {
            return await ExecuteInContextAsync(async context =>
            {
                var trait = await context.Trait
                    .Include(t => t.TraitType)
                    .Include(t => t.Species)
                    .FirstOrDefaultAsync(t => t.CommonName == name);

                if (trait == null)
                {
                    throw new KeyNotFoundException($"Trait with name '{name}' not found.");
                }

                return trait;
            });
        }

        public async Task<Dictionary<string, List<string>>> GetTraitMapForSingleAnimal(int animalId)
        {
            return await ExecuteInContextAsync(async context =>
            {
                var animalTraits = await context.AnimalTrait
                    .Include(at => at.Trait)
                        .ThenInclude(t => t.TraitType)
                    .Where(at => at.AnimalId == animalId)
                    .ToListAsync();

                var traitMap = new Dictionary<string, List<string>>();

                foreach (var animalTrait in animalTraits)
                {
                    var traitType = animalTrait.Trait.TraitType?.Name ?? "Unknown";
                    var traitName = animalTrait.Trait.CommonName;

                    if (!traitMap.ContainsKey(traitType))
                    {
                        traitMap[traitType] = new List<string>();
                    }

                    traitMap[traitType].Add(traitName);
                }

                return traitMap;
            });
        }

        public async Task CreateAnimalTraitAsync(int traitId, int animalId)
        {
            await ExecuteInTransactionAsync(async context =>
            {
                var existingTrait = await context.AnimalTrait
                    .FirstOrDefaultAsync(at => at.TraitId == traitId && at.AnimalId == animalId);

                if (existingTrait != null)
                {
                    throw new InvalidOperationException($"Animal {animalId} already has trait {traitId}");
                }

                var animalTrait = new AnimalTrait
                {
                    TraitId = traitId,
                    AnimalId = animalId,
                    CreatedOn = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                context.AnimalTrait.Add(animalTrait);
                await context.SaveChangesAsync();
            });
        }

        public async Task<Trait> CreateTraitAsync(Trait trait)
        {
            return await ExecuteInTransactionAsync(async context =>
            {
                // Validate trait type exists
                var traitType = await context.TraitType
                    .FirstOrDefaultAsync(tt => tt.Id == trait.TraitTypeId);
                if (traitType == null)
                {
                    throw new InvalidOperationException($"TraitType with ID {trait.TraitTypeId} not found.");
                }

                // Validate species exists
                var species = await context.Species
                    .FirstOrDefaultAsync(s => s.Id == trait.SpeciesID);
                if (species == null)
                {
                    throw new InvalidOperationException($"Species with ID {trait.SpeciesID} not found.");
                }

                // Check for duplicate trait name
                var exists = await context.Trait
                    .AnyAsync(t => t.CommonName == trait.CommonName);
                if (exists)
                {
                    throw new InvalidOperationException($"Trait with name '{trait.CommonName}' already exists.");
                }

                context.Trait.Add(trait);
                await context.SaveChangesAsync();

                return trait;
            });
        }
    }
}
