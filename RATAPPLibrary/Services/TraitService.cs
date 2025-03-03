using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.Models.Genetics;
using System.Security.AccessControl;

namespace RATAPPLibrary.Services
{
    public class TraitService
    {
        //get all phenotypes - aka traits
        // this will require creating new trait types, setting the values for each trait type, and then adding the trait type to the animal
        //variety
        //coat color
        //ear type
        //ear set  (rat) TODO haven't implemented this yet i.e. distinguish between rat and mouse ear types
        //eye color 
        //markings 
        //tail type
        private readonly Data.DbContexts.RatAppDbContext _context;
        private SpeciesService _speciesService;

        // Constructor to initialize the context
        public TraitService(Data.DbContexts.RatAppDbContext context)
        {
            _context = context;
            _speciesService = new SpeciesService(context);
        }

        public async Task<int?> GetTraitTypeIdByNameAsync(string name)
        {
            // Query the TraitType by Name and return the Id (nullable to handle non-existing types)
            var traitType = await _context.TraitType
                .FirstOrDefaultAsync(tt => tt.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            return traitType?.Id; // Returns null if no matching trait type is found
        }

        //get all traits by type and species 
        public async Task<IEnumerable<Trait>> GetTraitsByTypeAndSpeciesAsync(string type, string species)
        {
            // Query the TraitType by Name and return the Id (nullable to handle non-existing types)
            var traitType = await _context.TraitType
                .FirstOrDefaultAsync(tt => tt.Name.Equals(type, StringComparison.OrdinalIgnoreCase));
            if (traitType == null)
            {
                throw new InvalidOperationException($"Trait type '{type}' does not exist.");
            }
            // Query the Traits by TraitTypeId and SpeciesID
            return await _context.Trait
                .Where(t => t.TraitTypeId == traitType.Id && t.SpeciesID == (species == "Rat" ? 0 : 1))
                .ToListAsync();
        }

        //get all traits by species
        public async Task<IEnumerable<Trait>> GetTraitsBySpeciesAsync(string species)
        {
            List<Trait> traitList = new List<Trait>();

            // Query the Species by CommonName and return the Id
            try
            {
                var speciesID = await _speciesService.GetSpeciesIdByCommonNameAsync(species); // Get the SpeciesID
                traitList = await _context.Trait
                    .Where(t => t.SpeciesID == speciesID)
                    .ToListAsync();
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException($"Species '{species}' does not exist.");
            }
            // Query the Traits by SpeciesID
            return traitList; // Return the list of traits for the specified species
        }

        //create a new trait type i.e. each trait has a type (e.g. color, pattern, etc.) so this is not species specific 
        public async Task<TraitType> CreateTraitTypeAsync(string name, string? description = null)
        {
            // Check if the trait type already exists
            var existingTraitType = await _context.TraitType
                .FirstOrDefaultAsync(tt => tt.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (existingTraitType != null)
            {
                throw new InvalidOperationException($"Trait type '{name}' already exists.");
            }
            // Create the new trait type
            var newTraitType = new TraitType
            {
                Name = name,
                Description = description
            };
            _context.TraitType.Add(newTraitType);
            await _context.SaveChangesAsync();
            return newTraitType;
        }

        //Create a new trait i.e. a trait within a trait type, so this is species specific 
        public async Task<Trait> CreateTraitAsync(string name, int traitTypeId, string species, string? description = null)
        {
            //get species based on species passed in TODO: implement this
            if (species == null)
            {
                throw new InvalidOperationException("Species must be provided.");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Trait name must be provided.");
            }
            if (string.IsNullOrWhiteSpace(description)) { description = "N/A"; }
            if (traitTypeId <= 0)
            {
                throw new InvalidOperationException("TraitTypeId must be provided.");
            }

            // get species id from the database 
            // TODO may be better to look for either the common name or scientific name 
            var speciesID = await _context.Species
                .Where(s => s.CommonName.Equals(species, StringComparison.OrdinalIgnoreCase))
                .Select(s => s.Id)
                .FirstOrDefaultAsync();

            // Check if the trait already exists
            var existingTrait = await _context.Trait
                .FirstOrDefaultAsync(t => t.CommonName.Equals(name, StringComparison.OrdinalIgnoreCase) && t.TraitTypeId == traitTypeId);
            if (existingTrait != null)
            {
                throw new InvalidOperationException($"Trait '{name}' already exists for TraitTypeId '{traitTypeId}'.");
            }

            // Create the new trait
            var newTrait = new Trait
            {
                CommonName = name,
                TraitTypeId = traitTypeId,
                Genotype = "N/A", //TODO: Implement genotype generation based on trait
                SpeciesID = speciesID,
                //Genotype = genotype.GenerateGenotype(name), eventually this is the goal to generate the genotype based on the trait but for not its just a string that the user enters 

            };
            _context.Trait.Add(newTrait);
            await _context.SaveChangesAsync();
            return newTrait;
        }

        // Get all traits available in the database (for all species)
        public async Task<IEnumerable<Trait>> GetAllTraitsAsync()
        {
            return await _context.Trait.ToListAsync();  // Get all traits from the database
        }

        // Get traits by TraitTypeId
        public async Task<IEnumerable<Trait>> GetTraitsByTraitTypeIdAsync(int traitTypeId)
        {
            return await _context.Trait
                .Where(t => t.TraitTypeId == traitTypeId)  // Filter by TraitTypeId
                .ToListAsync();
        }

        // Get all trait types available in the database
        public async Task<IEnumerable<TraitType>> GetAllTraitTypesAsync()
        {
            return await _context.TraitType.ToListAsync();  // Get all trait types from the database
        }

        // Get specific trait by Id
        public async Task<Trait> GetTraitByIdAsync(int id)
        {
            return await _context.Trait
                .FirstOrDefaultAsync(t => t.Id == id);  // Get a trait by its Id
        }

        //create new entry in the animal trait table
        public async Task<AnimalTrait> CreateAnimalTraitAsync(int animalId, int traitId)
        {
            // Check if the animal trait already exists
            var existingAnimalTrait = await _context.AnimalTrait
                .FirstOrDefaultAsync(at => at.AnimalId == animalId && at.TraitId == traitId);
            if (existingAnimalTrait != null)
            {
                throw new InvalidOperationException($"AnimalTrait for AnimalId '{animalId}' and TraitId '{traitId}' already exists.");
            }
            // Create the new animal trait
            var newAnimalTrait = new AnimalTrait
            {
                AnimalId = animalId,
                TraitId = traitId
            };
            _context.AnimalTrait.Add(newAnimalTrait);
            await _context.SaveChangesAsync();
            return newAnimalTrait;
        }

        // Get all traits for a specific animal by AnimalId (from the AnimalTrait table)
        public async Task<IEnumerable<Trait>> GetTraitsByAnimalIdAsync(int animalId)
        {
            return await _context.AnimalTrait
                .Where(at => at.AnimalId == animalId)
                .Select(at => at.Trait)
                .ToListAsync();
        }

        //get all traits for a specific animal by animal id and trait id 
        //this is used to get all trait types for a single animal
        // eventually this will be used to get/create the genotype for the animal
        // and to "stack" traits together to make new traits, for example angora + rex = texel
        public async Task<Trait> GetTraitByAnimalIdAndTraitIdAsync(int animalId, int traitId)
        {
            return await _context.AnimalTrait
                .Where(at => at.AnimalId == animalId && at.TraitId == traitId)
                .Select(at => at.Trait)
                .FirstOrDefaultAsync();
        }
    }
}
