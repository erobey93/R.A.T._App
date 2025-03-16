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
    public class AnimalService
    {
        private readonly RatAppDbContext _context;
        private readonly LineService _lineService;
        private readonly TraitService _traitService;
        private readonly LineageService _lineageService;

        public AnimalService(RatAppDbContext context)
        {
            _context = context;
            _lineService = new LineService(context);
            _traitService = new TraitService(context);
            _lineageService = new LineageService(context);
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
                if (line == null)
                {
                    throw new InvalidOperationException($"Line '{animalDto.Line}' not found. Please ensure it exists in the database.");
                }

                // Find the species in the database by its scientific name
                var species = await _context.Species.FirstOrDefaultAsync(s => s.CommonName == animalDto.species);
                if (species == null)
                {
                    throw new InvalidOperationException($"Species '{animalDto.species}' not found. Please ensure it exists in the database.");
                }

                // set animal's color 
                //get the color from the string and find the id in the database
                //then store the animal id + color id in the animal_color table
                // Get the trait by name
                if(animalDto.color != null)
                {
                    try
                    {
                        await CreateAnimalTraitAsync(animalDto.Id, animalDto.color);
                    }
                    catch(Exception e)
                    {
                        //color isn't required so if it doesn't exist, just continue? FIXME 
                    }

                }

                //if there is an entry for dam and sire, make new lineage entry
                //gen 1, seq 1, animal id, need to figure out ancestor id, how do I find them? should have the parent object and store that I guess? Is that the most efficient way? TODO 
                if (animalDto.damId != null)
                {
                    try
                    {
                        //add a new dam entry into lineage table 
                        await _lineageService.AddLineageConnection(animalDto.Id, (int)animalDto.damId, 1, 1, "Maternal");
                    }
                    catch (Exception e) { } //for now, just catch, do nothing 

                }
                if (animalDto.sireId != null)
                {
                    //add a new sire entry into lineage table 
                    try
                    {
                        //add a new dam entry into lineage table 
                        await _lineageService.AddLineageConnection(animalDto.Id, (int)animalDto.sireId, 1, 1, "Paternal");
                    }
                    catch (Exception e) { } //for now, just catch, do nothing 
                }

                    // Map the AnimalDto to the Animal database model
                    var newAnimal = new Animal
                {
                    registrationNumber = animalDto.regNum,
                    StockId = 1, //FIXME this should be set automatically based on the species of the animal
                    Name = animalDto.name,
                    DateOfBirth = animalDto.DateOfBirth,
                    DateOfDeath = animalDto.DateOfDeath,
                    Sex = animalDto.sex,
                    LineId = line.Id,
                    comment = animalDto.comment,
                    imageUrl = animalDto.imageUrl,
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

        //create new animal trait
        public async Task CreateAnimalTraitAsync(int animalId, string traitName)
        {
            try
            {
                var trait = await _traitService.GetTraitByNameAsync(traitName); //FIXME: Placeholder for color logic
                if (trait == null)
                {
                    throw new InvalidOperationException($"Common name '{traitName}' for trait could not be found.");
                }

                int traitId = trait.Id;

                //check if the trait already exists for the animal
                var existingTrait = await _context.AnimalTrait.FirstOrDefaultAsync(t => t.TraitId == traitId && t.AnimalId == animalId);
                if (existingTrait != null)
                {
                    throw new InvalidOperationException($"Trait with ID {traitId} already exists for animal with ID {animalId}.");
                }

                //make a new entry in the animal trait table for the color of the animal
                await _traitService.CreateAnimalTraitAsync(traitId, animalId);
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

        //get all animal info by sex 
        public async Task<AnimalDto[]> GetAnimalInfoBySexAndSpecies(string sex, string species)
        {
            //Get all animals of the correct sex 
            var bySex = await GetAnimalsBySex(sex);

            List<AnimalDto> animalDto = new List<AnimalDto>();

            //then find all animals that match the provided sex 
            foreach (var animal in bySex)
            {
                //get the traits for that animal 
                var traits = await _traitService.GetTraitMapForSingleAnimal(animal.Id); //FIXME changed this wihtout checkout functionality 

                if (animal.species.Equals(species))
                {
                    //add to the list of animals that match the requested sex 
                    animalDto.Add(animal);
                }
            }

            return animalDto.ToArray();
        }

        //get all animals by sex 
        public async Task<AnimalDto[]> GetAnimalsBySex(string sex)
        {
            // Find all animals that match sex
            var animals = await _context.Animal.Where(a => a.Sex.Equals(sex)).ToListAsync();

            //store to a list of animalDto
            List<AnimalDto> animalDto = new List<AnimalDto>();

            //then return the array of animals
            foreach (var animal in animals)
            {
                animalDto.Add(await MapSingleAnimaltoDto(animal));
            }

            return animalDto.ToArray();
        }

        public async Task<AnimalDto[]> GetAnimalInfoBySpecies(string species)
        {
            // Find all animals that match the provided species
            var animals = await _context.Animal.Where(a => a.StockId.Equals(species)).ToListAsync();
            //get the names and ids of all animals that have that species id
            List<AnimalDto> animalDto = new List<AnimalDto>();
            //then return the array of animals
            foreach (var animal in animals)
            {
                animalDto.Add(await MapSingleAnimaltoDto(animal));
            }
            return animalDto.ToArray();
        }

        //get animal species
        //this must come via line -> stock -> species so putting in an easy way to access this here 
        public async Task<string> GetAnimalSpecies(int id)
        {
            var animal = await _context.Animal.FirstOrDefaultAsync(a => a.Id == id);
            if (animal == null)
            {
                throw new KeyNotFoundException($"Animal with ID {id} not found.");
            }
            var line = await _context.Line.FirstOrDefaultAsync(l => l.Id == animal.LineId);
            if (line == null)
            {
                throw new KeyNotFoundException($"Line with ID {animal.LineId} not found.");
            }
            var stock = await _context.Stock.FirstOrDefaultAsync(s => s.Id == line.StockId);
            if (stock == null)
            {
                throw new KeyNotFoundException($"Stock with ID {line.StockId} not found.");
            }
            var species = await _context.Species.FirstOrDefaultAsync(s => s.Id == stock.SpeciesId);
            if (species == null)
            {
                throw new KeyNotFoundException($"Species with ID {stock.SpeciesId} not found.");
            }
            return species.CommonName;
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
                if (line == null)
                {
                    throw new InvalidOperationException($"Line '{animalDto.Line}' not found. Please ensure it exists in the database.");
                }

                // Find the species in the database by its common  name
                var species = await _context.Species.FirstOrDefaultAsync(s => s.CommonName == animalDto.species);
                if (species == null)
                {
                    throw new InvalidOperationException($"Species '{animalDto.species}' not found. Please ensure it exists in the database.");
                }

                // add updated traits if they don't already exist 
                // set animal's color 
                //get the color from the string and find the id in the database
                //then store the animal id + color id in the animal_color table
                // Get the trait by name
                if (animalDto.color != null)
                {
                    try
                    {
                        await CreateAnimalTraitAsync(animalDto.Id, animalDto.color);
                    }
                    catch (Exception e)
                    {
                        //color isn't required so if it doesn't exist, just continue? FIXME 
                    }

                }

                int? damId = animalDto.damId;
                int? sireId = animalDto.sireId;
                int animalId = animalDto.Id;

                //if ancestry connection doesn't exist, make it 
                if(damId != 0)
                {
                    var doesExist = await _lineageService.DoesAncestryConnectionExist(animalId, (int)damId);
                    bool connectionExists = doesExist; 

                    //check if it exists
                    if (!connectionExists){
                        //add the connection
                        await _lineageService.AddLineageConnection(animalId, (int)damId, 1, 1, "Maternal"); //TODO this is hardcoded for a dam 
                    }
                    else
                    {
                        //connection is there so no need to do anything 
                    }
                }
                if (sireId != 0)
                {
                    var doesExist = await _lineageService.DoesAncestryConnectionExist(animalId, (int)sireId);
                    bool connectionExists = doesExist;

                    //check if it exists
                    if (!connectionExists)
                    {
                        //add the connection
                        await _lineageService.AddLineageConnection(animalId, (int)sireId, 1, 1, "Paternal"); //TODO this is hardcoded for a dam 
                    }
                    else
                    {
                        //connection is there so no need to do anything 
                    }
                }


                // Update the existing animal's properties
                existingAnimal.Name = animalDto.name;
                existingAnimal.DateOfBirth = animalDto.DateOfBirth;
                existingAnimal.DateOfDeath = animalDto.DateOfDeath;
                existingAnimal.Sex = animalDto.sex;
                existingAnimal.LineId = line.Id;
                existingAnimal.comment = animalDto.comment;
                existingAnimal.imageUrl = animalDto.imageUrl;

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
            string species = speciesObj.CommonName;

            int breederId = 5; //FIXME this is a placeholder until I fix/implement breeder logic 

            var animalTraits = await GetAnimalTraits(a.Id); //FIXME this is a placeholder until I fix/implement trait logic

            //TODO can do above logic for all traits and then just loop through them to get

            //var getSire = await _lineageService.GetDamAndSireByAnimalId(a.Id); //TODO look into how I should be handling all of this lineage stuff and generally the database calls. To me, it feels like the service should handle checks and the controller should handle the logic, but I'm not sure if that's correct.
            //int sireId = getSire.sire.;
            var getDam = await _lineageService.GetDamByAnimalId(a.Id);
            int damId = 0; 
            if (getDam != null)
            {
                damId = getDam.Id;
            }

            var getSire = await _lineageService.GetSireByAnimalId(a.Id);
            int sireId = 0;
            if (getSire != null)
            {
                sireId = getSire.Id;
            }

            // Map the animals to include string values for the related entities
            var result = new AnimalDto
            {
                Id = a.Id,
                regNum = a.registrationNumber,
                name = a.Name,
                DateOfBirth = a.DateOfBirth,
                sex = a.Sex,
                Line = lineId.ToString(),
                comment = a.comment,

                breeder = breederId.ToString(),//lineObj.Stock.Breeder.User.Individual.Name, TODO this should be grabbing the breeders name from the breeder db (actually the user db)
                species = speciesObj.CommonName, // Assuming Species has a Name property
                imageUrl = a.imageUrl,
                color = animalTraits["Color"].LastOrDefault(), //TODO this might break the world since its assuming multiple colors FIXME this is just going to print all colors as a list of strings which will work for 1, but not once i start stacking them
                markings = animalTraits["Markings"].LastOrDefault(), //TODO this might break the world since its assuming multiple markings FIXME this is just going to print all markings as a list of strings which will work for 1, but not once i start stacking them
                earType = animalTraits["Ear Type"].LastOrDefault(), //TODO this might break the world since its assuming multiple ear types FIXME this is just going to print all ear types as a list of strings which will work for 1, but not once i start stacking them
                variety = animalTraits["Coat Type"].LastOrDefault(), //TODO this might break the world since its assuming multiple coat types FIXME this is just going to print all coat types as a list of strings which will work for 1, but not once i start stacking them
                damId = damId != 0 ? damId : (int?)null,
                sireId = sireId != 0 ? sireId : (int?)null, 
            };

            return result;
        }

        //get animal's traits 
        public async Task<Dictionary<string,List<string>>> GetAnimalTraits(int id)
        {
            var animal = await _context.Animal.FirstOrDefaultAsync(a => a.Id == id);
            if (animal == null)
            {
                throw new KeyNotFoundException($"Animal with ID {id} not found.");
            }
            
            Dictionary<string, List<string>> traitMap = new Dictionary<string, List<string>>();
            try
            {
                traitMap = await _traitService.GetTraitMapForSingleAnimal(id);
            }
            catch(Exception e) //for now just catch exceptions and set default trait values to avoid breaking the world FIXME 
            {
                //throw new KeyNotFoundException($"Traits for animal with ID {id} not found.");
                //animal doesn't have to have traits, at least not right now TODO 
                Console.WriteLine($"Traits for animal with ID {id} not found.");
                traitMap["Color"] = new List<string> { "No color found" };
                traitMap["Markings"] = new List<string> { "No markings found" };
                traitMap["Ear Type"] = new List<string> { "No ear type found" };
                traitMap["Coat Type"] = new List<string> { "No coat type found" };
            }

            if (traitMap.Count < 4) //FIXME this is assuming we have 4 traits just to auto set traits for now for now just make sure we have the 4 traits we're looking for
            {
                if(traitMap.ContainsKey("Color") == false)
                {
                    traitMap["Color"] = new List<string> { "No color found" };
                }
                if (traitMap.ContainsKey("Markings") == false)
                {
                    traitMap["Markings"] = new List<string> { "No markings found" };
                }
                if (traitMap.ContainsKey("Ear Type") == false)
                {
                    traitMap["Ear Type"] = new List<string> { "No ear type found" };
                }
                if (traitMap.ContainsKey("Coat Type") == false)
                {
                    traitMap["Coat Type"] = new List<string> { "No coat type found" };
                }
            }

            return traitMap;
        }

        //Map array of animals to array of AnimalDto
        public AnimalDto[] MapMultiAnimalsToDto(Animal[] animals)
        {
            // Map the animals to include string values for the related entities
            AnimalDto[] result = animals.Select(a => new AnimalDto
            {
                Id = a.Id,
                regNum = a.registrationNumber,
                name = a.Name,
                DateOfBirth = a.DateOfBirth,
                sex = a.Sex,
                breeder = a.Line.Stock.Breeder.User.Individual.FirstName,
                species = a.Line.Stock.Species.CommonName, // Assuming Species has a Name property
                Line = a.Line?.Name, // Assuming Line has a Name property
                comment = a.comment,
                //                     // Dam = a.Litters, // Assuming Dam has a Name property TODO 
                //                     // Sire = a.Sire?.Name, // Assuming Sire has a Name property TODO 
                //                     // Variety = a.Variety?.Name, // Assuming Variety has a Name property TODO
            }).ToArray();

            return result;

        }

        // delete animal by id
        public async Task DeleteAnimalByIdAsync(int id)
        {
            var animal = await _context.Animal.FirstOrDefaultAsync(a => a.Id == id);
            if (animal == null)
            {
                throw new KeyNotFoundException($"Animal with ID {id} not found.");
            }
            _context.Animal.Remove(animal);
            await _context.SaveChangesAsync();
        }

        //get animal 

    }
}
