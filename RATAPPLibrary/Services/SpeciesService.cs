using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace RATAPPLibrary.Services
{
    public class SpeciesService
    {
        private readonly RatAppDbContext _context;

        public SpeciesService(RatAppDbContext context)
        {
            _context = context;
        }

        public async Task<Species> GetSpeciesAsync(string commonName)
        {
            // Find the correct Species based on the common name
            var species = await _context.Species.FirstOrDefaultAsync(s => s.CommonName == commonName);

            if (species == null)
            {
                // Throw an exception if the species is not found and creation is not implemented yet will be a feature in a later release 
                throw new KeyNotFoundException($"Species with scientific name '{commonName}' not found.");
            }

            return species;
        }

        //get all species in the database 
        //returned as a list of species common names
        public async Task<IEnumerable<string>> GetAllSpeciesAsync()
        {
            // Query all Species and return the common names
            return await _context.Species
                .Select(s => s.CommonName)
                .ToListAsync();
        }

        //create a new species 
        public async Task<Species> CreateSpeciesAsync(string commonName, string scientificName)
        {
            // Check if the species already exists
            var existingSpecies = await _context.Species.FirstOrDefaultAsync(s => s.ScientificName == scientificName);
            if (existingSpecies != null)
            {
                throw new InvalidOperationException($"Species with scientific name '{scientificName}' already exists.");
            }
            // Create the new species
            var newSpecies = new Species
            {
                CommonName = commonName,
                ScientificName = scientificName
            };
            _context.Species.Add(newSpecies);
            await _context.SaveChangesAsync();
            return newSpecies;
        }

        //edit a species
        public async Task<Species> EditSpeciesAsync(int id, string commonName, string scientificName)
        {
            // Find the species to edit
            var species = await _context.Species.FirstOrDefaultAsync(s => s.Id == id);
            if (species == null)
            {
                throw new KeyNotFoundException($"Species with ID '{id}' not found.");
            }
            // Update the species
            species.CommonName = commonName;
            species.ScientificName = scientificName;
            await _context.SaveChangesAsync();
            return species;
        }

        //get species by id
        public async Task<Species> GetSpeciesByIdAsync(int id)
        {
            // Find the correct Species based on the ID
            var species = await _context.Species.FirstOrDefaultAsync(s => s.Id == id);
            if (species == null)
            {
                // Throw an exception if the species is not found
                throw new KeyNotFoundException($"Species with ID '{id}' not found.");
            }
            return species;
        }

        //get species by common name
        public async Task<Species> GetSpeciesByCommonNameAsync(string commonName)
        {
            // Find the correct Species based on the common name
            var species = await _context.Species.FirstOrDefaultAsync(s => s.CommonName == commonName);
            if (species == null)
            {
                // Throw an exception if the species is not found
                throw new KeyNotFoundException($"Species with common name '{commonName}' not found.");
            }
            return species;
        }

        //get species by scientific name

        //get species id by common name - don't return the whole object if we don't need it, this will de-clutter the code in other classes/services 
        public async Task<int> GetSpeciesIdByCommonNameAsync(string commonName)
        {
            // Find the correct Species based on the common name and return the ID
            var species = await _context.Species.FirstOrDefaultAsync(s => s.CommonName == commonName);
            if (species == null)
            {
                // Throw an exception if the species is not found
                throw new KeyNotFoundException($"Species with common name '{commonName}' not found.");
            }
            return species.Id;
        }
    }
}
