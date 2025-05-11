using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Ancestry;

namespace RATAPPLibrary.Services
{
    public class LineageService : BaseService
    {
        //private readonly RatAppDbContext _context;

        public LineageService(RatAppDbContextFactory contextFactory) : base(contextFactory)
        {
            //_context = context;
        }

        //get dam via animal id
        //Find the animal id
        //Find gen 1, seq 1 for dam
        public async Task<Animal> GetDamByAnimalId(int animalId)
        {
            return await ExecuteInContextAsync(async _context =>
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
            });
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
                    .FirstOrDefaultAsync(l => l.AnimalId == animalId && l.Generation == 1 && l.Sequence == 2 && l.RelationshipType == "Paternal"); // Assuming you have a maternal identifier.

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

        //add a new lineage connection
        //Add a new lineage connection between an animal and its ancestor
        //Lineage connection addition must be followed by adding in any new connections with the updated data 
        public async Task<bool> AddLineageConnection(int animalId, int ancestorId, int generation, int sequence, string relationshipType)
        {
            try
            {
                var newLineage = new Lineage
                {
                    AnimalId = animalId,
                    AncestorId = ancestorId,
                    Generation = generation,
                    Sequence = sequence,
                    RelationshipType = relationshipType,
                    RecordedAt = DateTime.UtcNow
                };
                _context.Lineages.Add(newLineage);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Handle exceptions (logging, etc.)
                Console.WriteLine($"Error in AddLineageConnection: {ex.Message}");
                return false; // Return false in case of error
            }
        }

        //check for matching animalId and ancestorId 
        public async Task<bool> DoesAncestryConnectionExist(int animalId, int ancestorId)
        {
            var existingConnection = await _context.Lineages.FirstOrDefaultAsync(l =>
                l.AnimalId == animalId && l.AncestorId == ancestorId);

            return existingConnection != null;
        }


        // update connections
        // when a new lineage connection is added
        // the connections must be updated to reflect the new data
        // this means searching for any connections 

    }
}
