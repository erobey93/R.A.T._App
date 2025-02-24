using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace RATAPPLibrary.Services
{
    public class SpeciesService
    {
        private readonly Data.DbContexts.RatAppDbContext _context;

        public SpeciesService(Data.DbContexts.RatAppDbContext context)
        {
            _context = context;
        }

        public async Task<Species> GetSpeciesAsync(string scientificName)
        {
            // Find the correct Species based on the common name
            var species = await _context.Species.FirstOrDefaultAsync(s => s.ScientificName == scientificName);

            if (species == null)
            {
                // Throw an exception if the species is not found and creation is not implemented yet will be a feature in a later release 
                throw new KeyNotFoundException($"Species with scientific name '{scientificName}' not found.");
            }

            return species;
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
    }
}
