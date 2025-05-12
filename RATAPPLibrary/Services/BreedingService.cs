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

        //PAIRINGS
        //get all pairings
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

        //get all current pairings (no pairing end date, so "active")
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

        //add new pairing with individual variables
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

        //LITTERS
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

        //add new litter 
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

        //update litter
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

        //delete litter
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
