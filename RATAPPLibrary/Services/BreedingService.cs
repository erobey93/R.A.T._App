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

        //pairings 
        //get all pairings

        //add new pairing 
        public async Task CreatePairingAsync(string pairingId, int damId, int sireId, int projectId, DateTime? startDate, DateTime? endDate)
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
        //update pairing 
        //find pairing 
        //get all pairings (by species for now, but I will be breaking the db into species specific dbs to reduce complexity) 

        //litters 
        //add new litter 
        //when a litter is added there should be pups associated with it which implies that new animals will be created 
        //delete litter
        //update litter 

    }
}