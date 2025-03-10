using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;

namespace RATAPPLibrary.Services
{
    public class LineageService
    {
        private readonly RatAppDbContext _context;

        public LineageService(RatAppDbContext context)
        {
            _context = context;
        }

        //get dam via animal id
        //Find the animal id
        //Find gen 1, seq 1 for dam
        public async Task<Animal> GetDamByAnimalId(int animalId)
        {
            try
            {
                var damAncestryRecord = await _context.Lineages
                    .Include(l => l.Ancestor)
                    .FirstOrDefaultAsync(l => l.AnimalId == animalId && l.Generation == 1 && l.Sequence == 1 && l.RelationshipType == "Maternal"); // Assuming you have a maternal identifier.

                if (damAncestryRecord != null && damAncestryRecord.Ancestor != null)
                {
                    return damAncestryRecord.Ancestor;
                }

                return null; // Dam not found
            }
            catch (Exception ex)
            {
                // Handle exceptions (logging, etc.)
                Console.WriteLine($"Error in GetDamByAnimalId: {ex.Message}");
                return null; // Return null in case of error
            }
        }


        //get sire via animal id
        //Find the animal id
        //Find gen 1, seq 2 for sire 
        public async Task<Animal> GetSireByAnimalId(int animalId)
        {
            try
            {
                var sireAncestryRecord = await _context.Lineages
                    .Include(l => l.Ancestor)
                    .FirstOrDefaultAsync(l => l.AnimalId == animalId && l.Generation == 1 && l.Sequence == 1 && l.RelationshipType == "Maternal"); // Assuming you have a maternal identifier.

                if (sireAncestryRecord != null && sireAncestryRecord.Ancestor != null)
                {
                    return sireAncestryRecord.Ancestor;
                }

                return null; // Dam not found
            }
            catch (Exception ex)
            {
                // Handle exceptions (logging, etc.)
                Console.WriteLine($"Error in GetDamByAnimalId: {ex.Message}");
                return null; // Return null in case of error
            }
        }

        //return dam and sire as strings via animal id
        public async Task<(string dam, string sire)> GetDamAndSireByAnimalId(int animalId)
        {
            var dam = await GetDamByAnimalId(animalId);
            var sire = await GetSireByAnimalId(animalId);
            return (dam?.DisplayName ?? "Unknown", sire?.DisplayName ?? "Unknown");
        }
    }
}
