using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Ancestry;

namespace RATAPPLibrary.Services
{
    /// <summary>
    /// Service for managing animal lineage and ancestry relationships in the R.A.T. App.
    /// Handles tracking of parental relationships and generational connections between animals.
    /// 
    /// Key Features:
    /// - Parent Tracking:
    ///   * Dam (mother) identification and retrieval
    ///   * Sire (father) identification and retrieval
    /// - Lineage Management:
    ///   * Create and verify ancestry connections
    ///   * Track generational relationships
    ///   * Support for both maternal and paternal lineages
    /// 
    /// Data Structure:
    /// - Uses generation and sequence numbers to track ancestry
    /// - Generation 1: Direct parents (dam=seq 1, sire=seq 2)
    /// - Supports relationship type tracking (Maternal/Paternal)
    /// 
    /// Known Limitations:
    /// - Update connections functionality incomplete (TODO)
    /// - Error handling returns null for some scenarios
    /// </summary>
    public class LineageService : BaseService
    {
        private readonly RatAppDbContextFactory _contextFactory;
        private readonly TraitService _traitService;

        public LineageService(RatAppDbContextFactory contextFactory) : base(contextFactory)
        {
            _contextFactory = contextFactory;
            _traitService = new TraitService(contextFactory);
        }

        /// <summary>
        /// Retrieves all traits for all ancestors of a specified animal.
        /// 
        /// Returns a dictionary mapping each ancestor to their traits, where:
        /// - Key: The ancestor animal (including generation and relationship info)
        /// - Value: Dictionary of trait types to trait lists
        /// 
        /// Example structure:
        /// {
        ///    "Dam (Gen 1)": {
        ///       "Color": ["Black", "White"],
        ///       "Ear Type": ["Standard"]
        ///    },
        ///    "Dam's Dam (Gen 2)": {
        ///       "Color": ["Agouti"],
        ///       "Coat Type": ["Rex"]
        ///    }
        /// }
        /// </summary>
        /// <param name="animalId">ID of the animal to get ancestor traits for</param>
        /// <returns>Dictionary mapping ancestor descriptions to their trait maps</returns>
        public async Task<Dictionary<string, Dictionary<string, List<string>>>> GetAncestorTraitsAsync(int animalId)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                var result = new Dictionary<string, Dictionary<string, List<string>>>();
                
                // Get all ancestors
                var ancestors = await GetAncestorsByAnimalId(animalId);
                
                // For each ancestor, get their traits
                foreach (var lineage in ancestors)
                {
                    if (lineage.Ancestor != null)
                    {
                        // Get traits for this ancestor
                        var traits = await _traitService.GetTraitMapForSingleAnimal(lineage.AncestorId);
                        
                        // Create descriptive key based on relationship and generation
                        string key = $"{lineage.RelationshipType} (Gen {lineage.Generation})";
                        
                        // Add to result dictionary
                        result[key] = traits;
                    }
                }
                
                return result;
            });
        }

        /// <summary>
        /// Retrieves the dam (mother) of a specified animal.
        /// 
        /// Process:
        /// 1. Searches lineage records for generation 1, sequence 1 (dam position)
        /// 2. Verifies maternal relationship type
        /// 3. Returns the associated ancestor animal record
        /// 
        /// Note: Returns null if dam not found or on error
        /// TODO: Consider throwing exceptions instead of returning null
        /// </summary>
        /// <param name="animalId">ID of the animal to find dam for</param>
        /// <returns>Animal object representing the dam, or null if not found</returns>
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


        /// <summary>
        /// Retrieves the sire (father) of a specified animal.
        /// 
        /// Process:
        /// 1. Searches lineage records for generation 1, sequence 2 (sire position)
        /// 2. Verifies paternal relationship type
        /// 3. Returns the associated ancestor animal record
        /// 
        /// Note: Returns null if sire not found or on error
        /// TODO: Consider throwing exceptions instead of returning null
        /// </summary>
        /// <param name="animalId">ID of the animal to find sire for</param>
        /// <returns>Animal object representing the sire, or null if not found</returns>
        public async Task<Animal> GetSireByAnimalId(int animalId)
        {
            return await ExecuteInContextAsync(async _context =>
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
            });
        }

        //return dam and sire as strings via animal id
        public async Task<(string dam, string sire)> GetDamAndSireByAnimalId(int animalId)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                return await ExecuteInContextAsync(async _context =>
            {
                var dam = await GetDamByAnimalId(animalId);
                var sire = await GetSireByAnimalId(animalId);
                return (dam?.DisplayName ?? "Unknown", sire?.DisplayName ?? "Unknown");
            });
            });
        }

        /// <summary>
        /// Creates a new lineage connection between an animal and its ancestor.
        /// 
        /// Parameters:
        /// - animalId: The descendant animal
        /// - ancestorId: The ancestor animal
        /// - generation: Generational distance (1 for parents, 2 for grandparents, etc.)
        /// - sequence: Position in generation (1=dam side, 2=sire side)
        /// - relationshipType: "Maternal" or "Paternal"
        /// 
        /// Note: After adding a connection, related connections should be updated
        /// TODO: Implement update of related connections
        /// </summary>
        /// <returns>True if connection created successfully, false if error occurs</returns>
        public async Task<bool> AddLineageConnection(int animalId, int ancestorId, int generation, int sequence, string relationshipType)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                return await ExecuteInTransactionAsync(async _context =>
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
                });
            });
        }

        /// <summary>
        /// Checks if a lineage connection exists between two animals.
        /// Used to prevent duplicate connections and verify relationships.
        /// </summary>
        /// <param name="animalId">ID of the descendant animal</param>
        /// <param name="ancestorId">ID of the ancestor animal</param>
        /// <returns>True if connection exists, false otherwise</returns>
        public async Task<bool> DoesAncestryConnectionExist(int animalId, int ancestorId)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                var existingConnection = await _context.Lineages.FirstOrDefaultAsync(l =>
                l.AnimalId == animalId && l.AncestorId == ancestorId);

                return existingConnection != null;
            });
        }

        /// <summary>
        /// Retrieves all descendants of a specified animal.
        /// 
        /// Process:
        /// 1. Searches lineage records where the specified animal is the ancestor
        /// 2. Returns all animals that are descendants
        /// 
        /// Note: Returns empty list if no descendants found or on error
        /// </summary>
        /// <param name="animalId">ID of the animal to find descendants for</param>
        /// <returns>List of Animal objects representing the descendants, or empty list if none found</returns>
        public async Task<IEnumerable<Lineage>> GetDescendantsByAnimalId(int animalId)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                try
                {
                    var descendants = await _context.Lineages
                        .Include(l => l.Animal)
                        .Where(l => l.AncestorId == animalId)
                        .ToListAsync();

                    return descendants ?? new List<Lineage>();
                }
                catch (Exception ex)
                {
                    // Handle exceptions (logging, etc.)
                    Console.WriteLine($"Error in GetDescendantsByAnimalId: {ex.Message}");
                    return new List<Lineage>();
                }
            });
        }

        /// <summary>
        /// Retrieves all ancestors of a specified animal.
        /// 
        /// Process:
        /// 1. Searches lineage records where the specified animal is the descendant
        /// 2. Returns all lineage records with their ancestor animals
        /// 
        /// Note: Returns empty list if no ancestors found or on error
        /// </summary>
        /// <param name="animalId">ID of the animal to find ancestors for</param>
        /// <returns>List of Lineage objects with their Ancestor animals included</returns>
        public async Task<IEnumerable<Lineage>> GetAncestorsByAnimalId(int animalId)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                try
                {
                    var ancestors = await _context.Lineages
                        .Include(l => l.Ancestor)
                        .Where(l => l.AnimalId == animalId)
                        .ToListAsync();

                    return ancestors ?? new List<Lineage>();
                }
                catch (Exception ex)
                {
                    // Handle exceptions (logging, etc.)
                    Console.WriteLine($"Error in GetAncestorsByAnimalId: {ex.Message}");
                    return new List<Lineage>();
                }
            });
        }

        //get x seq by animal id, or return 0 if none? 
        //this would allow me to create a family tree relatively easily 
        //just pop the animal id and make a neat document with them for now, more interesting in the future

        /// <summary>
        /// TODO: Implement connection update logic
        /// 
        /// Planned functionality:
        /// - Update existing connections when new ones are added
        /// - Maintain consistency in the lineage tree
        /// - Handle relationship changes and updates
        /// 
        /// This will ensure the integrity of the ancestry data
        /// when new connections are established.
        /// </summary>

    }
}
