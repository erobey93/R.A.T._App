using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Breeding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Drawing;
using Microsoft.Identity.Client;

namespace RATAPPLibrary.Services
{
    public class AnimalService
    {
        private readonly Data.DbContexts.RatAppDbContext _context;
        private readonly LineService _lineService;

        public AnimalService(Data.DbContexts.RatAppDbContext context)
        {
            _context = context;
            _lineService = new LineService(context);
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
        //public async Task<Animal> CreateAnimalAsync(int id, string variety, DateTime dateOfBirth, string sex, string commonSpecies, string? name = null, DateTime? dateOfDeath = null)
        //{
        //    //first, check if the animal exists
        //    bool exists = await DoesAnimalExist(id);

        //    if (!exists)
        //    {
        //        // Get or create the line for the animal based on the variety
        //        var line = await _lineService.GetOrCreateLineAsync(variety);
        //        var species = await _context.Species.FirstOrDefaultAsync(s => s.ScientificName == commonSpecies); //TODO Need to "create a species", but will add mouse and rat to the database first as creating a new species is another feature

        //        var newAnimal = new Animal
        //        {
        //            Id = id,
        //            DateOfBirth = dateOfBirth,
        //            Sex = sex,
        //            LineId = line.Id,
        //            StockId = line.StockId, //FIXME this should not be here but EF if fing up so leaving for now
        //            Name = name, // Can be null; DisplayName will handle defaulting
        //        };

        //        // if the animal isn't already in the database (has to be based on ID + name match because technically the same name isn't that important 
        //        _context.Animal.Add(newAnimal);
        //        await _context.SaveChangesAsync();

        //        return newAnimal;
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException($"An animal with the ID {id} already exists.");
        //    }

        //}
        //TODO switched to passing an animalDto object instead of individual parameters
        public async Task<Animal> CreateAnimalAsync(AnimalDto animalDto)
        {
            // Check if the animal already exists based on the provided ID
            bool exists = await DoesAnimalExist(animalDto.Id);

            if (!exists)
            {
                // Get or create the line for the animal based on the variety
                var line = await _lineService.GetOrCreateLineAsync_ByName(animalDto.Variety);

                // Find the species in the database by its scientific name
                var species = await _context.Species.FirstOrDefaultAsync(s => s.ScientificName == animalDto.Species);
                if (species == null)
                {
                    throw new InvalidOperationException($"Species '{animalDto.Species}' not found. Please ensure it exists in the database.");
                }

                // Map the AnimalDto to the Animal database model
                var newAnimal = new Animal
                {
                    Id = animalDto.Id,
                    Name = animalDto.Name,
                    DateOfBirth = animalDto.DateOfBirth,
                    DateOfDeath = animalDto.DateOfDeath,
                    Sex = animalDto.Sex,
                    LineId = line.Id,
                    //StockId = line.StockId, // FIXME: Leaving this here as noted even though stock id should not be in the animal object (EF won't let me update this)
                    //S = species.Id, // Assuming a SpeciesId FK exists in the Animal table
                    //Dam = animalDto.Dam,
                    //Sire = animalDto.Sire,
                    //Variety = animalDto.Variety, line is being set based on variety so this is not needed
                    //Color = animalDto.Color, //TODO missing color and markings I guess? Gonna get the db working first then I'll update this, maybe I was planning on storing this in a phenotype object but idk
                    //Breeder = animalDto.Breeder, //TODO not yet implemented 
                    //Genotype = animalDto.Genotype //TODO not yet implemented
                };

                // Add the new animal to the database
                _context.Animal.Add(newAnimal);
                await _context.SaveChangesAsync();

                return newAnimal;
            }
            else
            {
                throw new InvalidOperationException($"An animal with the ID {animalDto.Id} already exists.");
            }
        }


        //check if animal is already in database 
        private async Task<bool> DoesAnimalExist(int id)
        {
            var existingAnimal = await _context.Animal.FirstOrDefaultAsync(a => a.Id == id);
            if (existingAnimal != null)
            {
                return true;
            }

            return false;
        }

        //get animal by id
        public async Task<AnimalDto> GetAnimalByIdAsync(int id)
        {
            var animal = await _context.Animal.FirstOrDefaultAsync(a => a.Id == id);
            if (animal == null)
            {
                throw new KeyNotFoundException($"Animal with ID {id} not found.");
            }

            return MapSingleAnimaltoDto(animal);
        }

        //get all animals - TODO just basic data right now as all relationships are not built out yet
        public async Task<AnimalDto[]> GetAllAnimalsAsync()
        {
            // Fetch the animals and include related entities for species, line, dam, sire, and variety
            //this is fetching all animals so must be dealt with as a group 
            var animals = await _context.Animal
                .Include(a => a.Line)  
                                      //.Include(a => a.Litters).ThenInclude(l => l.Pair) // Assuming Litters is a navigation property and Dam is a navigation property on Litter TODO still need to set up ancestry and genetics so these won't be super functional right now 
                .ToListAsync();

            List<AnimalDto> animalDto = new List<AnimalDto>();

            //map each individual animal and add it to the array to be returned 
            foreach (var animal in animals)
            {
                animalDto.Add(MapSingleAnimaltoDto(animal));
            }

           return animalDto.ToArray(); //return the array of animals or a list need to research why one over the other given my use case TODO 
        }

        //convert from Animal to AnimalDto
        public AnimalDto MapSingleAnimaltoDto(Animal a)
        {
            var line = a.LineId;

            // Map the animals to include string values for the related entities
            var result = new AnimalDto
            {
                Id = a.Id,
                Name = a.Name,
                DateOfBirth = a.DateOfBirth,
                Sex = a.Sex,
                Breeder = a.Line.Stock.Breeder.User.Individual.Name,
                Species = a.Line.Stock.Species.CommonName, // Assuming Species has a Name property
                //Line = a.Line?.Name, // Assuming Line has a Name property
                //                     // Dam = a.Litters, // Assuming Dam has a Name property TODO 
                //                     // Sire = a.Sire?.Name, // Assuming Sire has a Name property TODO 
                //                     // Variety = a.Variety?.Name, // Assuming Variety has a Name property TODO
            };

            return result;
        }

        //Map array of animals to array of AnimalDto
        public AnimalDto[] MapMultiAnimalsToDto(Animal[] animals)
        {
            // Map the animals to include string values for the related entities
            AnimalDto[] result = animals.Select(a => new AnimalDto
            {
                Id = a.Id,
                Name = a.Name,
                DateOfBirth = a.DateOfBirth,
                Sex = a.Sex,
                Breeder = a.Line.Stock.Breeder.User.Individual.Name,
                Species = a.Line.Stock.Species.CommonName, // Assuming Species has a Name property
                //Line = a.Line?.Name, // Assuming Line has a Name property
                //                     // Dam = a.Litters, // Assuming Dam has a Name property TODO 
                //                     // Sire = a.Sire?.Name, // Assuming Sire has a Name property TODO 
                //                     // Variety = a.Variety?.Name, // Assuming Variety has a Name property TODO
            }).ToArray();

            return result;

            

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
    }
}
