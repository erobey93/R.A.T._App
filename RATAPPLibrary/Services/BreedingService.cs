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
    public class BreedingService
    {
        private readonly RatAppDbContext _context;
        private readonly LineService _lineService;
        private readonly TraitService _traitService;
        private readonly LineageService _lineageService;
        private readonly AnimalService _animalService;

        public BreedingService(RatAppDbContext context)
        {
            _context = context;
            _lineService = new LineService(context);
            _traitService = new TraitService(context);
            _lineageService = new LineageService(context);
            _animalService = new AnimalService(context);

        }

        //PAIRINGS
        //get all pairings
        public async Task<List<Pairing>> GetAllPairingsAsync()
        {
            var pairings = await _context.Pairing.ToListAsync();
            if (pairings.Count == 0)
            {
                return new List<Pairing>();
            }
            return pairings;
        }

        //get all current pairings (no pairing end date, so "active")
        //get all pairings
        public async Task<List<Pairing>> GetAllActivePairingsAsync()
        {
            // Retrieve active pairings where PairingEndDate is null
            var activePairings = await _context.Pairing
                                               .Where(p => p.PairingEndDate == null && p.PairingStartDate != null)
                                               .ToListAsync();

            // Return the list, which may be empty if no active pairings exist
            return activePairings;
        }

        //get all upcoming pairings (no pairing start, or end date)
        public async Task<List<Pairing>> GetAllUpcomingPairingsAsync()
        {
            // Retrieve active pairings where PairingEndDate is null
            var futurePairings = await _context.Pairing
                                               .Where(p => p.PairingEndDate == null && p.PairingStartDate == null)
                                               .ToListAsync();

            // Return the list, which may be empty if no future pairings exist
            return futurePairings;
        }

        //get all past pairings (pairing start and end date)
        public async Task<List<Pairing>> GetAllPastPairingsAsync()
        {
            // Retrieve active pairings where PairingEndDate is null
            var pastPairings = await _context.Pairing
                                               .Where(p => p.PairingEndDate != null && p.PairingStartDate != null)
                                               .ToListAsync();

            // Return the list, which may be empty if no past pairings exist
            return pastPairings;
        }

        //get all pairings for animal id
        //looking for dam, or sire with matching ID
        public async Task<List<Pairing>> GetAllPairingsByAnimalIdAsync(int animalID)
        {
            // Retrieve active pairings where PairingEndDate is null
            var pastPairings = await _context.Pairing
                                               .Where(p => p.SireId == animalID || p.DamId == animalID)
                                               .ToListAsync();

            // Return the list, which may be empty if id doesn't exist
            return pastPairings;
        }

        //get all pairings for line
        public async Task<List<Pairing>> GetAllPairingsByLineIdAsync(int lineID)
        {
            //probably should get pairing, check for project then get line id but TODO this works for now 

            // Retrieve active pairings where PairingEndDate is null
            var linePairings = await _context.Pairing
                                               .Where(p => p.Project.LineId == lineID)
                                               .ToListAsync();

            // Return the list, which may be empty if lineID doesn't exist
            return linePairings;
        }

        //get pairings by species 
        public async Task<List<Pairing>> GetAllPairingsBySpeciesAsync(string species)
        {
            //probably should get pairing, check for project then get line, stock, species first to check if its populated but TODO this works for now 
            // Retrieve active pairings where PairingEndDate is null
            var pastPairings = await _context.Pairing
                                               .Where(p => p.Project.Line.Stock.Species.CommonName == species)
                                               .ToListAsync();

            // Return the list, which may be empty if id doesn't exist
            return pastPairings;
        }

        //add new pairing 
        //individual variables TODO may remove this and switch to just using objects 
        public async Task<bool> CreatePairingAsync(string pairingId, int damId, int sireId, int projectId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                //check if the pairing already exists based on pairingId
                //if it exists return exception
                //if it does not exist, add it 
                //if the add fails, return an exception 
                var existingPairing = await _context.Pairing.FirstOrDefaultAsync(p => p.pairingId == pairingId);
                if (existingPairing != null)
                {
                    throw new InvalidOperationException($"Pairing with ID {pairingId} already exists.");
                }

                DateTime createdOn = DateTime.Now;
                DateTime lastUpdated = DateTime.Now;

                //map to a pairing object 
                Pairing pairing = mapToPairingObject(pairingId, damId, sireId, projectId, createdOn, lastUpdated, startDate, endDate);

                //make a new entry in the animal pairing table for the specified dam and sire 
                _context.Pairing.Add(pairing);

                return true;
            }
            catch (Exception ex)
            {
                // Log the error details to the console or a logging service
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // throw the exception again to propagate further TODO need to think through error handling more thoroughly 
                throw;
            }
        }

        //add new pairing 
        //when passed pairing object 
        public async Task<bool> CreatePairingAsync(Pairing pair)
        {
            try
            {
                //check if the pairing already exists based on pairingId
                //if it exists return exception
                //if it does not exist, add it 
                //if the add fails, return an exception 
                var existingPairing = await _context.Pairing.FirstOrDefaultAsync(p => p.pairingId == pair.pairingId);
                if (existingPairing != null)
                {
                    throw new InvalidOperationException($"Pairing with ID {pair.pairingId} already exists.");
                }

                DateTime createdOn = DateTime.Now;
                DateTime lastUpdated = DateTime.Now;

                //make a new entry in the animal pairing table for the specified dam and sire 
                _context.Pairing.Add(pair);

                return true;    
            }
            catch (Exception ex)
            {
                // Log the error details to the console or a logging service
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // throw the exception again to propagate further
                throw;
            }
        }

        //map to pairing object 
        public Pairing mapToPairingObject(string pairingId, int damId, int sireId, int projectId, DateTime createdOn, DateTime lastUpdated, DateTime? startDate, DateTime? endDate)
        {

            Pairing pairing = new Pairing
            {
                pairingId = pairingId,
                DamId = damId,
                SireId = sireId,
                ProjectId = projectId,
                PairingStartDate = startDate,
                PairingEndDate = endDate,
                CreatedOn = createdOn, //TODO probably should have created on automatically set for new entries as it should never be changed 
                LastUpdated = lastUpdated,
            };

            return pairing;

        }


        //delete pairing 
        //TODO 

        //update pairing
        //TODO 
        //cannot update pairing id as this would conflict with litters 
        //you can only update certain pieces of a pairing such as start and end date once a litter has been associated with a pairing
        //so, this must be checked for i.e. is there a litter associated with this pairing? 
        /// <summary>
       
        /// </summary>
        /// <returns></returns>
        //



        //LITTERS

        //when a litter is added there should be pups associated with it which implies that new animals will be created 

        //get all litters 
        public async Task<List<Litter>> GetAllLittersAsync()
        {
            var litters = await _context.Litter.ToListAsync();
            if (litters.Count == 0)
            {
                return new List<Litter>();
            }
            return litters;
        }

        //add new litter 
        public async Task<bool> CreateLitterAsync(Litter litter)
        {
            try
            {
                //check if the pairing already exists based on pairingId
                //if it exists return exception
                //if it does not exist, add it 
                //if the add fails, return an exception 
                var existingLitter = await _context.Pairing.FirstOrDefaultAsync(p => p.Id == litter.Id);
                if (existingLitter != null)
                {
                    throw new InvalidOperationException($"Litter with ID {litter.Id} already exists.");
                }

                DateTime createdOn = DateTime.Now;
                DateTime lastUpdated = DateTime.Now;

                _context.Litter.Add(litter);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                throw;
            }
        }

        //update litter, individual variable not object 
        //TODO - litter ID cannot be updated as this would conflict with associated pups
        //this is the basic update method I need a method for updating animals associated with litters
        //i.e. adding or deleting animals from litters 
        public async Task<Litter> UpdateLitterAsync(int litterId, string name, DateTime dob, int numPups)
        {
            var litter = await _context.Litter.FindAsync(litterId);

            if (litter == null) throw new KeyNotFoundException($"Litter {litterId} not found");

            litter.Name = name;
            litter.DateOfBirth = dob;
            litter.NumPups = numPups;
            litter.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();
            return litter;
        }

        //TODO
        //update pups in litter (actual animal objects)

        //add 
        //delete
        //an animal doesn't have to be associated with a litter, so the user should be able to "remove animal" from a litter
        //the user should also be able to "add animal" to a litter


        //delete litter
        //TODO: for now, only allow this to happen if there are no animals associated with the litter 
        public async Task DeleteLitterAsync(int litterId)
        {
            var litter = await _context.Litter.FindAsync(litterId);

            if (litter == null) throw new KeyNotFoundException($"Litter {litterId} not found");
            if (litter.Animals != null) throw new InvalidOperationException($"Litter {litterId} has pups associated with it so it cannot be deleted"); //TODO need to set this up, eventually there should just be a warning about deleting litters with pups associated 
            
            _context.Litter.Remove(litter);

            await _context.SaveChangesAsync();
        }

        //TODO when a litter is added, new animals should be created for said pups ? - future feature 
    }
}