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
        private readonly RatAppDbContext _context;
        private readonly LineService _lineService;

        public AnimalService(RatAppDbContext context)
        {
            _context = context;
            _lineService = new LineService(context);
        }

        //TODO switched to passing an animalDto object instead of individual parameters
        public async Task<Animal> CreateAnimalAsync(AnimalDto animalDto)
        {
            // Check if the animal already exists based on the provided ID
            bool exists = await DoesAnimalExist(animalDto.Id);

            if (!exists)
            {
                // Get or create the line for the animal based on the variety
                var line = await _lineService.GetOrCreateLineAsync_ByName(int.Parse(animalDto.Line));

                // Find the species in the database by its scientific name
                var species = await _context.Species.FirstOrDefaultAsync(s => s.CommonName == animalDto.Species);
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
                    comment = animalDto.Comment,
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

        //update animal async
        //TODO return type should probably be bool but think through this more 
        public async Task<Animal> UpdateAnimalAsync(AnimalDto animalDto)
        {
            // Check if the animal already exists based on the provided ID
            bool exists = await DoesAnimalExist(animalDto.Id);

            if (exists)
            {
                // Retrieve the existing animal from the context
                var existingAnimal = await _context.Animal.FindAsync(animalDto.Id);
                if (existingAnimal == null)
                {
                    throw new InvalidOperationException($"Animal with ID '{animalDto.Id}' not found.");
                }

                // Get or create the line for the animal based on the variety
                var line = await _lineService.GetOrCreateLineAsync_ByName(int.Parse(animalDto.Line));

                // Find the species in the database by its common  name
                var species = await _context.Species.FirstOrDefaultAsync(s => s.CommonName == animalDto.Species);
                if (species == null)
                {
                    throw new InvalidOperationException($"Species '{animalDto.Species}' not found. Please ensure it exists in the database.");
                }

                // Update the existing animal's properties
                existingAnimal.Name = animalDto.Name;
                existingAnimal.DateOfBirth = animalDto.DateOfBirth;
                existingAnimal.DateOfDeath = animalDto.DateOfDeath;
                existingAnimal.Sex = animalDto.Sex;
                existingAnimal.LineId = line.Id;
                existingAnimal.comment = animalDto.Comment;

                // Add the new animal to the database
                _context.Animal.Update(existingAnimal);
                await _context.SaveChangesAsync();

                return existingAnimal;
            }
            else
            {
                throw new InvalidOperationException($"Animal does not exist, please create animal before attempting to update.");
            }
        }

        //check if animal is already in database 
        public async Task<bool> DoesAnimalExist(int id)
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

            return await MapSingleAnimaltoDto(animal);
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
                animalDto.Add(await MapSingleAnimaltoDto(animal));
            }

           return animalDto.ToArray(); //return the array of animals or a list need to research why one over the other given my use case TODO 
        }

        //convert from Animal to AnimalDto
        public async Task<AnimalDto> MapSingleAnimaltoDto(Animal a)
        {
            int lineId = a.LineId;

            var lineObj = await _lineService.GetLineAsync_ById(lineId); //pick back up here debug and figure out why it isn't working i.e. why is the object "null" 

            var stockId = a.StockId;

            var speciesObj = await _context.Species.FirstOrDefaultAsync(s => s.Id == stockId); //FIXME this is a placeholder until I fix/implement species logic

            int breederId = 5; //FIXME this is a placeholder until I fix/implement breeder logic 

            // Map the animals to include string values for the related entities
            var result = new AnimalDto
            {
                Id = a.Id,
                Name = a.Name,
                DateOfBirth = a.DateOfBirth,
                Sex = a.Sex,
                Line = lineId.ToString(),
                Comment = a.comment,

                Breeder = breederId.ToString(),//lineObj.Stock.Breeder.User.Individual.Name,
                Species = speciesObj.CommonName, // Assuming Species has a Name property
                imageUrl = a.imageUrl
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
                Line = a.Line?.Name, // Assuming Line has a Name property
                Comment = a.comment,
                //                     // Dam = a.Litters, // Assuming Dam has a Name property TODO 
                //                     // Sire = a.Sire?.Name, // Assuming Sire has a Name property TODO 
                //                     // Variety = a.Variety?.Name, // Assuming Variety has a Name property TODO
            }).ToArray();

            return result;

        }
    }
}
