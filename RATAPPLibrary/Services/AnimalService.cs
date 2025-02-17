using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Breeding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Drawing;

namespace RATAPPLibrary.Services
{
    public class AnimalService
    {
        private readonly Data.DbContexts.RatAppDbContext _context;
        private readonly LineService _lineService;

        public AnimalService(Data.DbContexts.RatAppDbContext context, LineService lineService)
        {
            _context = context;
            _lineService = lineService;
        }

        //TODO 
        //Species should come from drop down menu
        //user needs to create a new species to use it 
        //Sex needs to be a drop down menu 
        //Variety needs to be a drop down menu 
        //Basically, when the user selects, the value has to be found via its unique id 
        //Dam and Sire comes from drop down menu
        //there should be an option for every box of either values that exist in the db already
        //or an option to "add new" which opens up a box for the user to enter a new type 
        //right now I will focus on animal specific and then move to species 
        //I think the best way to approach this is to break each one up into a task and pre-populate the database so that I have options to choose from to start, for say, species
        public async Task<Animal> CreateAnimalAsync(int id, string variety, DateTime dateOfBirth, string sex, string commonSpecies, string? name = null, DateTime? dateOfDeath = null)
        {
            //first, check if the animal exists
           bool exists = await DoesAnimalExist(id);

            if(!exists)
            {
                // Get or create the line for the animal based on the variety
                var line = await _lineService.GetOrCreateLineAsync(variety);
                var species = await _context.Species.FirstOrDefaultAsync(s => s.ScientificName == commonSpecies); //TODO Need to "create a species", but will add mouse and rat to the database first as creating a new species is another feature

                var newAnimal = new Animal
                {
                    Id = id,
                    DateOfBirth = dateOfBirth,
                    Sex = sex,
                    LineId = line.Id,
                    Name = name, // Can be null; DisplayName will handle defaulting
                };

                // if the animal isn't already in the database (has to be based on ID + name match because technically the same name isn't that important 
                _context.Animals.Add(newAnimal);
                await _context.SaveChangesAsync();

                return newAnimal;
            }
            else
            {
                throw new InvalidOperationException($"An animal with the ID {id} already exists.");
            }
           
        }

        //check if animal is already in database 
        private async Task<bool> DoesAnimalExist(int id)
        {
            var existingAnimal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id);
            if (existingAnimal != null)
            {
                return true; 
            }

            return false; 
        }

        //get animal by id
        public async Task<Animal> GetAnimalByIdAsync(int id)
        {
            var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id);
            if (animal == null)
            {
                throw new KeyNotFoundException($"Animal with ID {id} not found.");
            }
            return animal;
        }
    }

   

    //TODO: Implement other CRUD operations for Animal entity here 
    //GetAnimalByIdAsync
    //UpdateAnimalAsync
    //DeleteAnimalAsync
    //GetAllAnimalsAsync
    //GetAnimalsByLineAsync
    //GetAnimalsBySpeciesAsync
    //GetAnimalsByProjectAsync
    //GetAnimalsByPairingAsync
    //GetAnimalsByLitterAsync
    //GetAnimalsByStockAsync
    //GetAnimalsByDateOfBirthAsync
    //GetAnimalsByDateOfDeathAsync
    //GetAnimalsBySexAsync
    //GetAnimalsByNameAsync
    //GetAnimalsByLineAndSpeciesAsync
    //GetAnimalsByLineAndProjectAsync
    //GetAnimalsByLineAndPairingAsync
    //GetAnimalsByLineAndLitterAsync
    //GetAnimalsByLineAndStockAsync
    //GetAnimalsByLineAndDateOfBirthAsync
    //GetAnimalsByLineAndDateOfDeathAsync

}
