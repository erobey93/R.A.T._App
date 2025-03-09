using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Genetics;
using System.Security.AccessControl;

namespace RATAPPLibrary.Services
{
    public class TraitService
    {
        //get all phenotypes - aka traits
        private readonly Data.DbContexts.RatAppDbContext _context;
        private SpeciesService _speciesService;

        // Constructor to initialize the context
        public TraitService(Data.DbContexts.RatAppDbContext context)
        {
            _context = context;
            _speciesService = new SpeciesService(context);
        }

        // Get TraitTypeId by Name
        public async Task<int?> GetTraitTypeIdByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Trait type name cannot be null or empty.", nameof(name));
            }

            // Query the TraitType by Name
            var traitType = await _context.TraitType
                .FirstOrDefaultAsync(tt => tt.Name.ToLower() == name.ToLower());

            if (traitType == null)
            {
                throw new InvalidOperationException($"Trait type with name '{name}' does not exist.");
            }

            return traitType.Id; // Return the Id if found
        }

        // Get Trait by Name
        public async Task<Trait> GetTraitByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Trait name cannot be null or empty.", nameof(name));
            }

            // Query the Trait by Name
            var trait = await _context.Trait
                .FirstOrDefaultAsync(t => t.CommonName.ToLower() == name.ToLower());

            if (trait == null)
            {
                throw new InvalidOperationException($"Trait with name '{name}' does not exist.");
            }

            return trait; // Return the Trait if found
        }

        // Get a list of trait objects based on the traitType and species names converted into ID for searching the database
        public async Task<List<Trait>> GetTraitObjectsByTypeAndSpeciesAsync(string type, string species)
        {
            List<string> traitList = new List<string>();
            // Query the TraitType by Name without using StringComparison
            var traitType = await _context.TraitType
                .FirstOrDefaultAsync(tt => tt.Name.ToLower() == type.ToLower());
            if (traitType == null)
            {
                throw new InvalidOperationException($"Trait type '{type}' does not exist.");
            }

            // Query the Traits by TraitTypeId and SpeciesID
            var speciesId = _context.Species
                .Where(s => s.CommonName.ToLower() == species.ToLower())
                .Select(s => s.Id)
                .FirstOrDefault();

            var traits = await _context.Trait
                .Where(t => t.TraitTypeId == traitType.Id && t.SpeciesID == speciesId)
                .ToListAsync();

            return traits;
        }

        //get all trait names by type and species - don't return full object just the names
        public async Task<List<string>> GetTraitsByTypeAndSpeciesAsync(string type, string species)
        {
            List<string> traitList = new List<string>();
            // Query the TraitType by Name without using StringComparison
            var traitType = await _context.TraitType
                .FirstOrDefaultAsync(tt => tt.Name.ToLower() == type.ToLower());
            if (traitType == null)
            {
                throw new InvalidOperationException($"Trait type '{type}' does not exist.");
            }

            // Query the Traits by TraitTypeId and SpeciesID
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

            return traitList; 
        }

        //get all traits by species - return a trait object for each trait associated with a specific species 
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

        // get all traits for a specific animal by animal id
        // returns a map of trait type and trait name 
        public async Task<Dictionary<string, List<string>>> GetTraitMapForSingleAnimal(int animalId)
        {
            IEnumerable<Trait> traits = new List<Trait>();

            //check that animal exists 
            var animal = await _context.Animal.FindAsync(animalId);
            if (animal == null)
            {
                throw new InvalidOperationException($"Animal with id '{animalId}' does not exist.");
            }
            else
            {
                // query the AnimalTrait table by animal id
                // if the animal has traits associated with it
                //return the list of traits
                if (_context.AnimalTrait.Any(at => at.AnimalId == animalId))
                {
                    traits = await _context.AnimalTrait
                        .Where(at => at.AnimalId == animalId)
                        .Select(at => at.Trait)
                        .ToListAsync();

                }
                else
                {
                    //throw new InvalidOperationException($"Animal with id '{animalId}' does not have any traits."); FIXME, for now do nothing 
                }
            }

            //now that we have the traits create our map of trait type and trait name
            Dictionary<string, List<string>> traitMap = new Dictionary<string, List<string>>();

            //then set that to the corresponding trait name
            await MapTraitTypeAndNameAsync(traits).ContinueWith(t => traitMap = t.Result);

            return traitMap; // Return the list of traits for the specified animal

        }

        //map trait type and name
        private async Task<Dictionary<string, List<string>>> MapTraitTypeAndNameAsync(IEnumerable<Trait> traits)
        {
            Dictionary<string, List<string>> traitMap = new Dictionary<string, List<string>>();
            foreach (var trait in traits)
            {
                //get the trait type name
                var traitType = await _context.TraitType.FindAsync(trait.TraitTypeId);
                //if the trait type name is already in the map
                if (traitMap.ContainsKey(traitType.Name))
                {
                    //add the trait name to the list of traits for that trait type
                    traitMap[traitType.Name].Add(trait.CommonName);
                }
                else
                {
                    //create a new list of traits for the trait type
                    List<string> traitList = new List<string>();
                    //add the trait name to the list
                    traitList.Add(trait.CommonName);
                    //add the trait type and list of traits to the map
                    traitMap.Add(traitType.Name, traitList);
                }
            }
            return traitMap;
        }

        //get colors for animal
        public async Task<IEnumerable<Trait>> GetColorTraitsForSingleAnimal(int animalId, string traitType, string species)
        {
            // Query the AnimalTrait by animal id, get all traits
            List<int> traitIds = new List<int>();
            // return the list of color traits (TODO for right now, this will just be 1 trait) 
            try
            {
                //get all traits for the given species and trait type 
                var allTraitsofType = await GetTraitObjectsByTypeAndSpeciesAsync(traitType, species);
                foreach (var trait in allTraitsofType)
                {
                    //get all traits for the animal
                    traitIds.Add(trait.Id);
                }

                // Query the AnimalTrait by animal id and trait id
                //this should return all the traitType traits for the animal
                var animalTraits = await _context.AnimalTrait
                    .Where(at => at.AnimalId == animalId && traitIds.Contains(at.TraitId))
                    .Select(at => at.Trait)
                    .ToListAsync();

                //return the list of color traits for the animal
                return animalTraits;
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException($"Species '{species}' does not exist.");
            }
        }

        //create new entry in the animal trait table
        public async Task<AnimalTrait> CreateAnimalTraitAsync( int traitId, int animalId)
        {
            try
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

                // Add and save to the database
                _context.AnimalTrait.Add(newAnimalTrait);
                await _context.SaveChangesAsync();

                return newAnimalTrait;
            }
            catch (DbUpdateException dbEx)
            {
                // Log the detailed error message
                Console.WriteLine($"DbUpdateException: {dbEx.Message}");

                // Check if there's an inner exception with more details
                if (dbEx.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
                }

                // Optional: You can also log the SQL error if available
                if (dbEx.InnerException is SqlException sqlEx)
                {
                    Console.WriteLine($"SQL Error: {sqlEx.Message}");
                    Console.WriteLine($"SQL Error Code: {sqlEx.Number}");
                }
                throw;
            }
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

        //get trait type name by id 
        private async Task<string> GetTraitTypeNameByIdAsync(int traitTypeId)
        {
            var traitType = await _context.TraitType.FindAsync(traitTypeId);
            return traitType.Name;
        }
    }
}
