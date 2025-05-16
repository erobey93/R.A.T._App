using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Genetics;
using System.Security.AccessControl;
using Trait = RATAPPLibrary.Data.Models.Genetics.Trait;

namespace RATAPPLibrary.Services
{
    /// <summary>
    /// Service for managing animal traits and characteristics in the R.A.T. App.
    /// Handles the creation, organization, and tracking of physical traits and their
    /// associations with animals.
    /// 
    /// Key Features:
    /// - Trait Management:
    ///   * Create and track trait types (color, pattern, etc.)
    ///   * Manage species-specific traits
    ///   * Associate traits with animals
    /// 
    /// Data Structure:
    /// - TraitType: Categories of traits (e.g., color, markings, ear type)
    /// - Trait: Specific characteristics within a type
    /// - AnimalTrait: Links traits to specific animals
    /// 
    /// Relationships:
    /// - Traits belong to TraitTypes
    /// - Traits are species-specific
    /// - Animals can have multiple traits
    /// 
    /// Known Limitations:
    /// - Basic genotype support ("N/A" placeholder)
    /// - No trait inheritance calculations
    /// - Limited trait combination logic
    /// - No validation for trait compatibility
    /// 
    /// Future Enhancements:
    /// - Implement genotype generation
    /// - Add trait stacking (e.g., angora + rex = texel)
    /// - Improve trait inheritance tracking
    /// 
    /// Dependencies:
    /// - SpeciesService: For species validation
    /// - Inherits from BaseService for database operations
    /// </summary>
    {
        //get all phenotypes - aka traits
        //private readonly Data.DbContexts.RatAppDbContext _context;
        private SpeciesService _speciesService;

        public TraitService(RatAppDbContextFactory contextFactory) : base(contextFactory)
        {
            //_context = context;
            _speciesService = new SpeciesService(contextFactory);
        }

        // Get TraitTypeId by Name
        public async Task<int?> GetTraitTypeIdByNameAsync(string name)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Trait type name cannot be null or empty.", nameof(name));
                }

                // search traitType by name
                var traitType = await _context.TraitType
                    .FirstOrDefaultAsync(tt => tt.Name.ToLower() == name.ToLower());

                if (traitType == null)
                {
                    throw new InvalidOperationException($"Trait type with name '{name}' does not exist.");
                }

                return traitType.Id; // Return the Id if found
            });
        }

        // Get Trait by Name
        public async Task<Trait> GetTraitByNameAsync(string name)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Trait name cannot be null or empty.", nameof(name));
                }

                // search trait table by name 
                var trait = await _context.Trait
                    .FirstOrDefaultAsync(t => t.CommonName.ToLower() == name.ToLower());

                if (trait == null)
                {
                    throw new InvalidOperationException($"Trait with name '{name}' does not exist.");
                }

                return trait;
            });
        }

        // Get a list of trait objects based on the traitType and species names converted into ID for searching the database
        public async Task<List<Trait>> GetTraitObjectsByTypeAndSpeciesAsync(string type, string species)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                List<string> traitList = new List<string>();

                // search traitType by name 
                var traitType = await _context.TraitType
                    .FirstOrDefaultAsync(tt => tt.Name.ToLower() == type.ToLower());
                if (traitType == null)
                {
                    throw new InvalidOperationException($"Trait type '{type}' does not exist.");
                }

                // search traits by traitTypeId and speciesID
                var speciesId = _context.Species
                    .Where(s => s.CommonName.ToLower() == species.ToLower())
                    .Select(s => s.Id)
                    .FirstOrDefault();

                var traits = await _context.Trait
                    .Where(t => t.TraitTypeId == traitType.Id && t.SpeciesID == speciesId)
                    .ToListAsync();

                return traits;
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

                if(species != null && species != "Unknown")
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

        //get all traits by species - return a trait object for each trait associated with a specific species 
        public async Task<IEnumerable<Trait>> GetTraitsBySpeciesAsync(string species)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                List<Trait> traitList = new List<Trait>();

                // search Species by CommonName and return the Id
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

                return traitList;
            });
        }

        //create a new trait type i.e. each trait has a type (e.g. color, pattern, etc.) so this is not species specific 
        public async Task<TraitType> CreateTraitTypeAsync(string name, string? description = null)
        {
            return await ExecuteInTransactionAsync(async _context =>
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
            });
        }

        /// <summary>
        /// Creates a new trait for a specific species and trait type.
        /// 
        /// Process:
        /// 1. Validates inputs (name, species, trait type)
        /// 2. Checks for existing traits
        /// 3. Creates trait with default genotype
        /// 
        /// Required Fields:
        /// - name: Trait identifier
        /// - traitTypeId: Category of trait
        /// - species: Associated species
        /// 
        /// Note: Currently uses "N/A" for genotype.
        /// TODO: Implement proper genotype generation based on trait
        /// 
        /// Throws:
        /// - InvalidOperationException for validation failures
        /// </summary>
        public async Task<Trait> CreateTraitAsync(string name, int traitTypeId, string species, string? description = null)
        {
            return await ExecuteInTransactionAsync(async _context =>
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
            });
        }

        // Get all traits available in the database (for all species)
        public async Task<IEnumerable<Trait>> GetAllTraitsAsync()
        {
            return await ExecuteInContextAsync(async _context =>
            {
                return await _context.Trait
           .Include(t => t.TraitType)   // Include TraitType navigation property
           .Include(t => t.Species)    // Include Species navigation property
           .ToListAsync();             // Get all traits with their related objects  // Get all traits from the database
            });
        }

        // Get traits by TraitTypeId
        public async Task<IEnumerable<Trait>> GetTraitsByTraitTypeIdAsync(int traitTypeId)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                return await _context.Trait
                .Where(t => t.TraitTypeId == traitTypeId)  // Filter by TraitTypeId
                .ToListAsync();
            });
        }

        // Get all trait types available in the database
        public async Task<IEnumerable<TraitType>> GetAllTraitTypesAsync()
        {
            return await ExecuteInContextAsync(async _context =>
            {
                return await _context.TraitType.ToListAsync();
            });
        }

        // Get specific trait by Id
        public async Task<Trait> GetTraitByIdAsync(int id)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                // Check if the trait exists
                var trait = await _context.Trait.FindAsync(id);
                if (trait == null)
                {
                    throw new InvalidOperationException($"Trait with id '{id}' does not exist.");
                }

                return trait; // Return the trait if found
            });
        }

        /// <summary>
        /// Retrieves all traits associated with a specific animal.
        /// Returns a mapping of trait types to trait names.
        /// 
        /// Example Output:
        /// {
        ///   "Color": ["Black", "White"],
        ///   "Ear Type": ["Standard"],
        ///   "Coat Type": ["Rex"]
        /// }
        /// 
        /// Note: Returns empty trait lists if animal has no traits,
        /// rather than throwing an exception.
        /// 
        /// TODO: Consider if this error handling approach is optimal
        /// </summary>
        /// <param name="animalId">ID of animal to get traits for</param>
        /// <returns>Dictionary mapping trait types to lists of trait names</returns>
        public async Task<Dictionary<string, List<string>>> GetTraitMapForSingleAnimal(int animalId)
        {
            return await ExecuteInTransactionAsync(async _context =>
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
            });
        }

        ////map trait type and name
        private async Task<Dictionary<string, List<string>>> MapTraitTypeAndNameAsync(IEnumerable<Data.Models.Genetics.Trait> traits)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                Dictionary<string, List<string>> traitMap = new Dictionary<string, List<string>>();
                foreach (var trait in traits)
                {
                    var traitType = await _context.TraitType.FindAsync(trait.TraitTypeId);

                    // Check if traitType is null
                    if (traitType != null)
                    {
                        if (traitMap.ContainsKey(traitType.Name))
                        {
                            traitMap[traitType.Name].Add(trait.CommonName);
                        }
                        else
                        {
                            List<string> traitList = new List<string>();
                            traitList.Add(trait.CommonName);
                            traitMap.Add(traitType.Name, traitList);
                        }
                    }
                    else
                    {
                        // Handle the case where the TraitType is not found.
                        // You might want to log an error, throw an exception, or skip the trait.
                        // Example:
                        Console.WriteLine($"TraitType with ID {trait.TraitTypeId} not found for trait {trait.CommonName}.");
                        continue; //TODO 
                    }
                }
                return traitMap;
            });
        }

        /// <summary>
        /// Retrieves traits of a specific type for an animal.
        /// Currently used primarily for color traits, but supports any trait type.
        /// 
        /// Process:
        /// 1. Gets all traits of specified type for species
        /// 2. Filters for traits associated with animal
        /// 
        /// Note: Currently assumes one trait per type per animal.
        /// TODO: Consider supporting multiple traits per type
        /// </summary>
        /// <param name="animalId">Animal to get traits for</param>
        /// <param name="traitType">Type of traits to retrieve</param>
        /// <param name="species">Species to filter traits by</param>
        /// <returns>Collection of matching traits</returns>
        public async Task<IEnumerable<Trait>> GetColorTraitsForSingleAnimal(int animalId, string traitType, string species)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                // Search the AnimalTrait by animal id, get all traits
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

                    //Search the AnimalTrait by animal id and trait id
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
            });
        }

        //create new entry in the animal trait table
        public async Task<AnimalTrait> CreateAnimalTraitAsync(int traitId, int animalId)
        {
            return await ExecuteInContextAsync(async _context =>
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

                    // FIXME : remove once testing is complete 
                    if (dbEx.InnerException is SqlException sqlEx)
                    {
                        Console.WriteLine($"SQL Error: {sqlEx.Message}");
                        Console.WriteLine($"SQL Error Code: {sqlEx.Number}");
                    }
                    throw;
                }
            });
        }


        //get all traits for a specific animal by animal id and trait id 
        //this is used to get all trait types for a single animal
        // eventually this will be used to get/create the genotype for the animal
        // and to "stack" traits together to make new traits, for example angora + rex = texel
        public async Task<Trait> GetTraitByAnimalIdAndTraitIdAsync(int animalId, int traitId)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                //check that the trait exists 
                var trait = await _context.AnimalTrait
                .Where(at => at.AnimalId == animalId && at.TraitId == traitId)
                .Select(at => at.Trait)
                .FirstOrDefaultAsync();

                //if the trait doesn't exist throw an exception
                if (trait == null)
                {
                    throw new InvalidOperationException($"Trait with id '{traitId}' does not exist.");
                }

                return trait;
            });
        }

        //get trait type name by id 
        private async Task<string> GetTraitTypeNameByIdAsync(int traitTypeId)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                var traitType = await _context.TraitType.FindAsync(traitTypeId);
                return traitType.Name;
            });
        }

        // Delete trait
        //TODO may need to account for dependencies on the trait i.e. in animal trait? 
        public async Task<bool> DeleteTraitAsync(int traitId)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                // Check if the trait exists
                var trait = await _context.Trait.FirstOrDefaultAsync(t => t.Id == traitId);

                if (trait != null)
                {
                    // Delete the trait
                    _context.Trait.Remove(trait);
                    await _context.SaveChangesAsync(); // Save changes to the database

                    return true; // Indicate successful deletion
                }

                // Return false if the trait does not exist
                return false;
            });
        }

        //this should maybe go elsewhere, but this will return the trait with all of its information i.e. 
        //trait, 
    }
}
