using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace RATAPPLibrary.Services
{
    public class SpeciesService
    {
        private readonly AnimalDbContext _context;

        public SpeciesService(AnimalDbContext context)
        {
            _context = context;
        }

        public async Task<Species> GetOrCreateSpeciesAsync(string scientificName)
        {
            // Find the correct Species based on the common name
            var species = await _context.Species.FirstOrDefaultAsync(s => s.ScientificName == scientificName);

            if (species == null)
            {
                //// Create a new Species if it doesn't exist
                //species = new Species
                //{
                   // Throw an exception if the species is not found and creation is not implemented yet will be a feature in a later release 
                throw new KeyNotFoundException($"Species with scientific name '{scientificName}' not found.");
                //};

                //_context.Species.Add(species);
                //await _context.SaveChangesAsync();
            }

            return species;
        }
    }
}
