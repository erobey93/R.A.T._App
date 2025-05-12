using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Breeding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Drawing;
using Microsoft.Identity.Client;
using PdfSharp.Charting;
using RATAPPLibrary.Data.Models.Genetics;
using System.Reflection.Metadata.Ecma335;

namespace RATAPPLibrary.Services
{
    /// <summary>
    /// Service for managing breeding-related operations in the R.A.T. App.
    /// Handles pairing and litter management, including tracking active breeding pairs,
    /// recording litter information, and managing breeding projects.
    /// 
    /// Key Features:
    /// - Pairing Management:
    ///   * Create and track breeding pairs
    ///   * Monitor active, upcoming, and past pairings
    ///   * Filter pairings by animal, line, or species
    /// 
    /// - Litter Management:
    ///   * Record and track litters
    ///   * Manage litter details (DOB, size, etc.)
    ///   * Associate pups with litters
    /// 
    /// Dependencies:
    /// - LineService: For line/variety management
    /// - TraitService: For trait tracking
    /// - LineageService: For ancestry management
    /// - AnimalService: For animal record management
    /// </summary>
    public class BreedingService : BaseService
    {
        private readonly LineService _lineService;
        private readonly TraitService _traitService;
        private readonly LineageService _lineageService;
        private readonly AnimalService _animalService;

        public BreedingService(RatAppDbContextFactory contextFactory) : base(contextFactory)
        {
            // Initialize other services with the same context factory
            _lineService = new LineService(contextFactory);
            _traitService = new TraitService(contextFactory);
            _lineageService = new LineageService(contextFactory);
            _animalService = new AnimalService(contextFactory);
        }

        #region Pairing Management

        /// <summary>
        /// Retrieves all pairings from the database, including associated dam, sire,
        /// and project information.
        /// </summary>
        /// <returns>List of all pairings, or empty list if none found</returns>
        public async Task<List<Pairing>> GetAllPairingsAsync()
        {
            return await ExecuteInContextAsync(async context =>
            {
                var pairings = await context.Pairing
                    .Include(p => p.Dam)
                    .Include(p => p.Sire)
                    .Include(p => p.Project)
                    .ToListAsync();
                return pairings ?? new List<Pairing>();
            });
        }

        /// <summary>
        /// Retrieves all currently active pairings (those with a start date but no end date).
        /// Active pairings represent breeding pairs that are currently together.
        /// </summary>
        /// <returns>List of active pairings</returns>
        public async Task<List<Pairing>> GetAllActivePairingsAsync()
        {
            return await ExecuteInContextAsync(async context =>
            {
                return await context.Pairing
                    .Include(p => p.Dam)
                    .Include(p => p.Sire)
                    .Include(p => p.Project)
                    .Where(p => p.PairingEndDate == null && p.PairingStartDate != null)
                    .ToListAsync();
            });
        }

        //get all upcoming pairings (no pairing start, or end date)
        public async Task<List<Pairing>> GetAllUpcomingPairingsAsync()
        {
            return await ExecuteInContextAsync(async context =>
            {
                return await context.Pairing
                    .Include(p => p.Dam)
                    .Include(p => p.Sire)
                    .Include(p => p.Project)
                    .Where(p => p.PairingEndDate == null && p.PairingStartDate == null)
                    .ToListAsync();
            });
        }

        //get all past pairings (pairing start and end date)
        public async Task<List<Pairing>> GetAllPastPairingsAsync()
        {
            return await ExecuteInContextAsync(async context =>
            {
                return await context.Pairing
                    .Include(p => p.Dam)
                    .Include(p => p.Sire)
                    .Include(p => p.Project)
                    .Where(p => p.PairingEndDate != null && p.PairingStartDate != null)
                    .ToListAsync();
            });
        }

        //get all pairings for dam and sire
        public async Task<List<Pairing>> GetAllActivePairingsByAnimalIdAsync(int animalID)
        {
            return await ExecuteInContextAsync(async context =>
            {
                return await context.Pairing
                    .Include(p => p.Dam)
                    .Include(p => p.Sire)
                    .Include(p => p.Project)
                    .Where(p => p.PairingEndDate == null && p.PairingStartDate != null && 
                               (p.SireId == animalID || p.DamId == animalID))
                    .ToListAsync();
            });
        }

        //get all pairings for dam and sire with matching IDs
        public async Task<List<Pairing>> GetAllActivePairingsByDamandSireIdAsync(int damID, int sireID)
        {
            return await ExecuteInContextAsync(async context =>
            {
                return await context.Pairing
                    .Include(p => p.Dam)
                    .Include(p => p.Sire)
                    .Include(p => p.Project)
                    .Where(p => p.PairingEndDate == null && p.PairingStartDate != null && 
                               p.SireId == sireID && p.DamId == damID)
                    .ToListAsync();
            });
        }

        //get all pairings for line
        public async Task<List<Pairing>> GetAllPairingsByLineIdAsync(int lineID)
        {
            return await ExecuteInContextAsync(async context =>
            {
                return await context.Pairing
                    .Include(p => p.Project)
                    .Include(p => p.Dam)
                    .Include(p => p.Sire)
                    .Where(p => p.Project.LineId == lineID)
                    .ToListAsync();
            });
        }

        //get pairings by species 
        public async Task<List<Pairing>> GetAllPairingsBySpeciesAsync(string species)
        {
            return await ExecuteInContextAsync(async context =>
            {
                return await context.Pairing
                    .Include(p => p.Project)
                        .ThenInclude(proj => proj.Line)
                            .ThenInclude(line => line.Stock)
                                .ThenInclude(stock => stock.Species)
                    .Include(p => p.Dam)
                    .Include(p => p.Sire)
                    .Where(p => p.Project.Line.Stock.Species.CommonName == species)
                    .ToListAsync();
            });
        }

        /// <summary>
        /// Creates a new breeding pair with the specified details.
        /// 
        /// Required Parameters:
        /// - pairingId: Unique identifier for the pairing
        /// - damId: ID of the female animal
        /// - sireId: ID of the male animal
        /// - projectId: Associated breeding project
        /// 
        /// Optional Parameters:
        /// - startDate: When the pairing begins
        /// - endDate: When the pairing ends
        /// 
        /// Throws:
        /// - InvalidOperationException if pairing ID already exists
        /// </summary>
        public async Task<bool> CreatePairingAsync(string pairingId, int damId, int sireId, int projectId, DateTime? startDate, DateTime? endDate)
        {
            return await ExecuteInTransactionAsync(async context =>
            {
                var existingPairing = await context.Pairing.FirstOrDefaultAsync(p => p.pairingId == pairingId);
                if (existingPairing != null)
                {
                    throw new InvalidOperationException($"Pairing with ID {pairingId} already exists.");
                }

                var pairing = mapToPairingObject(pairingId, damId, sireId, projectId, DateTime.Now, DateTime.Now, startDate, endDate);
                context.Pairing.Add(pairing);
                await context.SaveChangesAsync();
                return true;
            });
        }

        //add new pairing with pairing object
        public async Task<bool> CreatePairingAsync(Pairing pair)
        {
            return await ExecuteInTransactionAsync(async context =>
            {
                var existingPairing = await context.Pairing.FirstOrDefaultAsync(p => p.pairingId == pair.pairingId);
                if (existingPairing != null)
                {
                    throw new InvalidOperationException($"Pairing with ID {pair.pairingId} already exists.");
                }

                context.Pairing.Add(pair);
                await context.SaveChangesAsync();
                return true;
            });
        }

        //map to pairing object 
        private Pairing mapToPairingObject(string pairingId, int damId, int sireId, int projectId, 
            DateTime createdOn, DateTime lastUpdated, DateTime? startDate, DateTime? endDate)
        {
            return new Pairing
            {
                pairingId = pairingId,
                DamId = damId,
                SireId = sireId,
                ProjectId = projectId,
                PairingStartDate = startDate,
                PairingEndDate = endDate,
                CreatedOn = createdOn,
                LastUpdated = lastUpdated,
            };
        }

        #region Litter Management

        /// <summary>
        /// Retrieves all litters from the database, including associated pairing
        /// and parent information.
        /// </summary>
        /// <returns>List of all litters, or empty list if none found</returns>
        public async Task<List<Litter>> GetAllLittersAsync()
        {
            return await ExecuteInContextAsync(async context =>
            {
                var litters = await context.Litter
                    .Include(l => l.Pair)
                        .ThenInclude(p => p.Dam)
                    .Include(l => l.Pair)
                        .ThenInclude(p => p.Sire)
                    .Include(l => l.Pair)
                        .ThenInclude(p => p.Project)
                    .ToListAsync();
                return litters ?? new List<Litter>();
            });
        }

        /// <summary>
        /// Creates a new litter record in the database.
        /// 
        /// Required Litter Information:
        /// - ID: Unique identifier
        /// - Associated pairing
        /// - Date of birth
        /// - Number of pups
        /// 
        /// Note: CreatedOn and LastUpdated timestamps are automatically set
        /// 
        /// Throws:
        /// - InvalidOperationException if litter ID already exists
        /// </summary>
        /// <param name="litter">Litter object containing all required information</param>
        /// <returns>True if creation successful</returns>
        public async Task<bool> CreateLitterAsync(Litter litter)
        {
            return await ExecuteInTransactionAsync(async context =>
            {
                var existingLitter = await context.Litter.FirstOrDefaultAsync(l => l.Id == litter.Id);
                if (existingLitter != null)
                {
                    throw new InvalidOperationException($"Litter with ID {litter.Id} already exists.");
                }

                litter.CreatedOn = DateTime.Now;
                litter.LastUpdated = DateTime.Now;

                context.Litter.Add(litter);
                await context.SaveChangesAsync();
                return true;
            });
        }

        /// <summary>
        /// Updates an existing litter's information.
        /// 
        /// Updateable Fields:
        /// - Name: Litter identifier/name
        /// - DOB: Date of birth
        /// - NumPups: Number of pups in litter
        /// 
        /// Note: LastUpdated timestamp is automatically updated
        /// 
        /// Throws:
        /// - KeyNotFoundException if litter not found
        /// </summary>
        /// <returns>Updated Litter object</returns>
        public async Task<Litter> UpdateLitterAsync(int litterId, string name, DateTime dob, int numPups)
        {
            return await ExecuteInTransactionAsync(async context =>
            {
                var litter = await context.Litter.FindAsync(litterId);
                if (litter == null) 
                {
                    throw new KeyNotFoundException($"Litter {litterId} not found");
                }

                litter.Name = name;
                litter.DateOfBirth = dob;
                litter.NumPups = numPups;
                litter.LastUpdated = DateTime.Now;

                await context.SaveChangesAsync();
                return litter;
            });
        }

        /// <summary>
        /// Deletes a litter record if it has no associated pups.
        /// 
        /// Safety Checks:
        /// - Verifies litter exists
        /// - Ensures no pups are associated with the litter
        /// 
        /// Throws:
        /// - KeyNotFoundException if litter not found
        /// - InvalidOperationException if litter has associated pups
        /// </summary>
        public async Task DeleteLitterAsync(int litterId)
        {
            await ExecuteInTransactionAsync(async context =>
            {
                var litter = await context.Litter
                    .Include(l => l.Animals)
                    .FirstOrDefaultAsync(l => l.Id == litterId);

                if (litter == null)
                {
                    throw new KeyNotFoundException($"Litter {litterId} not found");
                }

                if (litter.Animals?.Any() == true)
                {
                    throw new InvalidOperationException($"Litter {litterId} has pups associated with it so it cannot be deleted");
                }

                context.Litter.Remove(litter);
                await context.SaveChangesAsync();
            });
        }
    }
}
