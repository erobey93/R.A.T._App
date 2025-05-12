using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace RATAPPLibrary.Services
{
    /// <summary>
    /// Service for managing species information in the R.A.T. App.
    /// Handles the creation, retrieval, and management of species records.
    /// 
    /// Key Features:
    /// - Species Management:
    ///   * Store and retrieve species information
    ///   * Track both common and scientific names
    ///   * Support for species lookups by various criteria
    /// 
    /// Data Structure:
    /// - Species records contain:
    ///   * Common name (e.g., "Norway Rat")
    ///   * Scientific name (e.g., "Rattus norvegicus")
    ///   * Unique identifier
    /// 
    /// Known Limitations:
    /// - No automatic species creation when not found
    /// - No validation of scientific name format
    /// - No support for subspecies
    /// - Missing implementation for scientific name lookup
    /// 
    /// Dependencies:
    /// - Inherits from BaseService for database operations
    /// - Requires RatAppDbContext for data access
    /// </summary>
    public class SpeciesService : BaseService
    {
        //private readonly RatAppDbContext _context;

        public SpeciesService(RatAppDbContextFactory contextFactory) : base(contextFactory)
        {
            //_context = context;
        }

        /// <summary>
        /// Retrieves a species by its common name.
        /// 
        /// Note: Species creation is not implemented in this version.
        /// A future release will add the ability to create new species.
        /// 
        /// Throws:
        /// - KeyNotFoundException if species not found
        /// </summary>
        /// <param name="commonName">Common name of the species to retrieve</param>
        /// <returns>Species object if found</returns>
        public async Task<Species> GetSpeciesAsync(string commonName)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                // Find the correct Species based on the common name
                var species = await _context.Species.FirstOrDefaultAsync(s => s.CommonName == commonName);

                if (species == null)
                {
                    // Throw an exception if the species is not found and creation is not implemented yet will be a feature in a later release 
                    throw new KeyNotFoundException($"Species with scientific name '{commonName}' not found.");
                }

                return species;
            });
        }

        /// <summary>
        /// Retrieves all species common names from the database.
        /// Useful for populating dropdowns or species selection lists.
        /// </summary>
        /// <returns>List of species common names</returns>
        public async Task<IEnumerable<string>> GetAllSpeciesAsync()
        {
            return await ExecuteInContextAsync(async _context =>
            {
                // Query all Species and return the common names
                return await _context.Species
                .Select(s => s.CommonName)
                .ToListAsync();
            });
        }


        //get all species in the database 
        //returned as a list of species common names
        public async Task<IEnumerable<Species>> GetAllSpeciesObjectsAsync()
        {
            return await ExecuteInContextAsync(async _context =>
            {
                // Query all Species and return the common names
                return await _context.Species
                .ToListAsync();
            });
        }

        /// <summary>
        /// Creates a new species record in the database.
        /// 
        /// Validation:
        /// - Checks for existing species with same scientific name
        /// - Both common and scientific names required
        /// 
        /// Throws:
        /// - InvalidOperationException if species already exists
        /// 
        /// TODO: Add validation for scientific name format
        /// </summary>
        /// <param name="commonName">Common name for the species</param>
        /// <param name="scientificName">Scientific name (binomial nomenclature)</param>
        /// <returns>Newly created Species object</returns>
        public async Task<Species> CreateSpeciesAsync(string commonName, string scientificName)
        {
            return await ExecuteInTransactionAsync(async _context =>
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
            });
        }

        //edit a species
        public async Task<Species> EditSpeciesAsync(int id, string commonName, string scientificName)
        {
            return await ExecuteInContextAsync(async _context =>
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
            }); 
        }

        //get species by id
        public async Task<Species> GetSpeciesByIdAsync(int id)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                // Find the correct Species based on the ID
                var species = await _context.Species.FirstOrDefaultAsync(s => s.Id == id);
                if (species == null)
                {
                    throw new KeyNotFoundException($"Species with ID '{id}' not found.");
                }
                return species;
            }); 
        }

        //get species by common name
        public async Task<Species> GetSpeciesByCommonNameAsync(string commonName)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                // Find the correct Species based on the common name
                var species = await _context.Species.FirstOrDefaultAsync(s => s.CommonName == commonName);
                if (species == null)
                {
                    throw new KeyNotFoundException($"Species with common name '{commonName}' not found.");
                }
                return species;
            });
        }

        /// <summary>
        /// TODO: Implement species lookup by scientific name
        /// 
        /// Planned functionality:
        /// - Case-insensitive search
        /// - Support for partial matches
        /// - Validation of scientific name format
        /// </summary>
        //get species id by common name - don't return the whole object if we don't need it, this will de-clutter the code in other classes/services 
        public async Task<int> GetSpeciesIdByCommonNameAsync(string commonName)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                // Find the correct Species based on the common name and return the ID
                var species = await _context.Species.FirstOrDefaultAsync(s => s.CommonName == commonName);
                if (species == null)
                {
                    throw new KeyNotFoundException($"Species with common name '{commonName}' not found.");
                }
                return species.Id;
            });
        }
    }
}
