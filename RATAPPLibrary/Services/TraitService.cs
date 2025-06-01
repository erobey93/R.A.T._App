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

        //get all trait names by type and species - don't return full object just the names
        public async Task<List<string>> GetTraitsByTypeAndSpeciesAsync(string type, string species)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                List<string> traitList = new List<string>();
                // search the traitType by name
                var traitType = await _context.TraitType
                    .FirstOrDefaultAsync(tt => tt.Name.ToLower() == type.ToLower());
                if (traitType == null)
                {
                    throw new InvalidOperationException($"Trait type '{type}' does not exist.");
                }

                if (species != null && species != "Unknown")
                { // search Traits by TraitTypeId and SpeciesID
                    var speciesId = _context.Species
                        .Where(s => s.CommonName.ToLower() == species.ToLower())
                        .Select(s => s.Id)
                        .FirstOrDefault();

                    var traits = await _context.Trait
                        .Where(t => t.TraitTypeId == traitType.Id && t.SpeciesID == speciesId)
                        .ToListAsync();

                    foreach (var trait in traits)
                    {
                        traitList.Add(trait.CommonName);
                    }
                }
                else
                {
                    var traits = await _context.Trait
                       .Where(t => t.TraitTypeId == traitType.Id)
                       .ToListAsync();

                    foreach (var trait in traits)
                    {
                        traitList.Add(trait.CommonName);
                    }
                }

                return traitList;
            });
        }

        public async Task<Trait> GetTraitByNameAsync(string traitName)
        {
            return await ExecuteInContextAsync(async context =>
            {
                var trait = await context.Trait
                    .Include(t => t.TraitType)
                    .Include(t => t.Species)
                    .FirstOrDefaultAsync(t => t.CommonName == traitName);

                if (trait == null)
                {
                    throw new KeyNotFoundException($"Trait with common name {traitName} not found.");
                }

                return trait;
            });
        }

        public async Task<Trait> CreateTraitAsync_FromObject(Trait newTrait)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                // Validate the provided Trait object
                if (newTrait == null)
                {
                    throw new ArgumentNullException(nameof(newTrait), "Trait object must be provided.");
                }

                if (string.IsNullOrWhiteSpace(newTrait.CommonName))
                {
                    throw new InvalidOperationException("Trait name must be provided.");
                }

                if (newTrait.TraitTypeId <= 0)
                {
                    throw new InvalidOperationException("Valid TraitTypeId must be provided.");
                }

                if (newTrait.SpeciesID <= 0)
                {
                    throw new InvalidOperationException("Valid SpeciesID must be provided.");
                }

                // Default description if null or empty trait doesn't have description, but maybe it should? TODO 
                //newTrait.des = string.IsNullOrWhiteSpace(newTrait.Description) ? "N/A" : newTrait.Description;

                // Check if the species exists in the database
                var speciesExists = await _context.Species
                    .AnyAsync(s => s.Id == newTrait.SpeciesID);

                if (!speciesExists)
                {
                    throw new InvalidOperationException($"Species with ID '{newTrait.SpeciesID}' does not exist.");
                }

                // Check if the trait already exists
                var existingTrait = await _context.Trait
                    .FirstOrDefaultAsync(t => t.CommonName.Equals(newTrait.CommonName, StringComparison.OrdinalIgnoreCase) && t.TraitTypeId == newTrait.TraitTypeId);

                if (existingTrait != null)
                {
                    throw new InvalidOperationException($"Trait '{newTrait.CommonName}' already exists for TraitTypeId '{newTrait.TraitTypeId}'.");
                }

                // Set default Genotype if not provided
                newTrait.Genotype ??= "N/A"; // Placeholder; implement genotype logic later

                // Add the new Trait to the database
                _context.Trait.Add(newTrait);
                await _context.SaveChangesAsync();

                return newTrait;
            });
        }

        // Get all traits available in the database (for all species)
        //public async Task<IEnumerable<Trait>> GetAllTraitsAsync()
        //{
        //    return await ExecuteInContextAsync(async context =>
        //    {
        //        var trait = await context.Trait
        //            .Include(t => t.TraitType)
        //            .Include(t => t.Species)
        //            .FirstOrDefaultAsync(t => t.CommonName == name);

        //        if (trait == null)
        //        {
        //            throw new KeyNotFoundException($"Trait with name '{name}' not found.");
        //        }

        //        return trait;
        //    });
        //}

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
