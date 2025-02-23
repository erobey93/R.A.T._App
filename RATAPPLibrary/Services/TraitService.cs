using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.Models.Genetics;

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

        // Constructor to initialize the context
        public TraitService(Data.DbContexts.RatAppDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetTraitTypeIdByNameAsync(string name)
        {
            // Query the TraitType by Name and return the Id (nullable to handle non-existing types)
            var traitType = await _context.TraitType
                .FirstOrDefaultAsync(tt => tt.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            return traitType?.Id; // Returns null if no matching trait type is found
        }

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

        //Create a new trait
        public async Task<Trait> CreateTraitAsync(string name, int traitTypeId, string species, string? description = null)
        {
            int speciesID = 1; // 1 is mouse, 0 is rat 

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

            //this should be dynamically grabbed from db but for now I'm just going to hard code it TODO
            if(species == "Rat")
            {
                speciesID = 0; 
            }

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

        // Get all traits available in the database
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
    }
}
