using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;
using RATAPPLibrary.Data.Models.Animal_Management;

namespace RATAPPLibrary.Services
{
    /// <summary>
    /// Service for managing animal-related operations in the R.A.T. App.
    /// Handles CRUD operations for animals, including trait management, lineage tracking, and species associations.
    /// 
    /// Key Features:
    /// - Create and update animals with traits, lineage, and species information
    /// - Manage animal relationships (dam/sire)
    /// - Track animal traits (color, markings, ear type, coat type)
    /// - Query animals by various criteria (sex, species, registration number)
    /// 
    /// Known Limitations:
    /// - Breeder information is currently hardcoded (TODO)
    /// - Some trait management features need refinement
    /// - Error handling for missing traits needs improvement
    /// 
    /// Dependencies:
    /// - LineService: Manages animal line/variety information
    /// - TraitService: Handles animal trait management
    /// - LineageService: Manages ancestry relationships
    /// </summary>
    public class AnimalService : BaseService
    {
        //private readonly RatAppDbContext _context;
        private readonly LineService _lineService;
        private readonly TraitService _traitService;
        private readonly LineageService _lineageService;

        public AnimalService(RatAppDbContextFactory contextFactory) : base(contextFactory)
        {
            //_context = context;
            _lineService = new LineService(contextFactory);
            _traitService = new TraitService(contextFactory);
            _lineageService = new LineageService(contextFactory);
        }

        /// <summary>
        /// Creates a new animal in the database with associated traits and lineage information.
        /// 
        /// Process:
        /// 1. Validates animal doesn't already exist
        /// 2. Creates/retrieves line and species associations
        /// 3. Creates the animal record
        /// 4. Sets dam/sire relationships
        /// 5. Assigns animal traits
        /// 
        /// Required AnimalDto fields:
        /// - Id: Unique identifier
        /// - regNum: Registration number
        /// - Line: Line/variety identifier
        /// - species: Common name of species
        /// - Basic info (name, sex, DOB, etc.)
        /// 
        /// Optional fields:
        /// - damId/sireId: Parent identifiers
        /// - traits (color, markings, ear type, variety)
        /// - comment, imageUrl
        /// 
        /// Throws:
        /// - InvalidOperationException: If animal already exists or required entities not found
        /// </summary>
        /// <param name="animalDto">Data transfer object containing animal information</param>
        /// <returns>The created Animal entity</returns>
        public async Task<Animal> CreateAnimalAsync(AnimalDto animalDto)
        {
            return await ExecuteInTransactionAsync(async _context =>
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

                    //Find the stock id based on the line 
                    int stockId = line.StockId;

                    // Map the AnimalDto to the Animal database model
                    //todo make mapping method 
                    var newAnimal = new Animal
                    {
                        registrationNumber = animalDto.regNum,
                        StockId = stockId,
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
            });
        }

        //add additional images for animal
        //public async Task AddAdditionalAnimalImages(int animalId, string[] additionalImageURl)
        //{
        //    await ExecuteInContextAsync(async _context =>
        //    {
        //        //for each image path, add new entry in AnimalImage table 
        //        foreach (string imageUrl in additionalImageURl)
        //        {
        //            //make entry
        //            //_context.AnimalIma
        //            //no errors, return true
        //            //else, return false 
        //        }
        //    });
                
        //}

        //complete create process TODO just trying this out given the new re-factor 
        public async Task<Animal> CreateAnimalFullProcess(AnimalDto animal) {
            //first, create the animal
            var animal1 = await CreateAnimalAsync(animal);

            //get the animal back from the db so that we have the id
            var animalDtoWithReg = await GetAnimalByRegAsync(animal.regNum);

            //set dam and sire
            await SetAnimalDamAndSire(animalDtoWithReg);

            //set available animal traits 
            await SetAnimalTraits(animalDtoWithReg);

            //get the animal object back with all new data attached (?) 
            //animalDtoWithReg = await 

            return animal1;

            //then, get the id back
            //then, add dam and sire
            //finally, set traits


        }
        //this returns an animal object without creating the animal in the database as I'm currently assuming that the animal exists FIXME because I should check, but there COULD be instances where this may make sense not sure though
        public async Task <Animal> MapAnimalDtoBackToAnimal(AnimalDto animalDto)
        {
            return await ExecuteInTransactionAsync(async _context =>
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

                //Find the stock id based on the line 
                int stockId = line.StockId;
                var newAnimal = new Animal
                {
                    registrationNumber = animalDto.regNum,
                    StockId = stockId,
                    Name = animalDto.name,
                    DateOfBirth = animalDto.DateOfBirth,
                    DateOfDeath = animalDto.DateOfDeath,
                    Sex = animalDto.sex,
                    LineId = line.Id,
                    comment = animalDto.comment,
                    imageUrl = animalDto.imageUrl,
                };

                return newAnimal;
            });
        }

        /// <summary>
        /// Sets or updates an animal's traits based on the provided DTO.
        /// Handles color, variety, ear type, and markings.
        /// 
        /// Note: Traits are optional - if a trait doesn't exist in the database,
        /// the operation will silently continue without throwing an error.
        /// 
        /// TODO: Improve error handling for missing traits
        /// </summary>
        /// <param name="animalDto">DTO containing trait information</param>
        public async Task SetAnimalTraits(AnimalDto animalDto)
        {
            await ExecuteInTransactionAsync(async _context =>
            {
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
                if (animalDto.variety != null)
                {
                    try
                    {
                        await CreateAnimalTraitAsync(animalDto.Id, animalDto.variety);
                    }
                    catch (Exception e)
                    {
                        //variety isn't required so if it doesn't exist, just continue? FIXME 
                    }

                }
                if (animalDto.earType != null)
                {
                    try
                    {
                        await CreateAnimalTraitAsync(animalDto.Id, animalDto.earType);
                    }
                    catch (Exception e)
                    {
                        //ear type isn't required so if it doesn't exist, just continue? FIXME 
                    }

                }
                if (animalDto.markings != null)
                {
                    try
                    {
                        await CreateAnimalTraitAsync(animalDto.Id, animalDto.markings);
                    }
                    catch (Exception e)
                    {
                        //marking isn't required so if it doesn't exist, just continue? FIXME 
                    }

                }
            });
        }

        /// <summary>
        /// Establishes parental relationships for an animal by creating lineage connections.
        /// Creates maternal (dam) and paternal (sire) connections in the lineage table.
        /// 
        /// Note: If a connection attempt fails, it will be silently caught and ignored.
        /// This behavior may need review for better error handling.
        /// </summary>
        /// <param name="animalDto">DTO containing dam and sire IDs</param>
        public async Task SetAnimalDamAndSire(AnimalDto animalDto)
        {
            await ExecuteInContextAsync(async _context =>
            {
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
                        await _lineageService.AddLineageConnection(animalDto.Id, (int)animalDto.sireId, 1, 2, "Paternal");
                    }
                    catch (Exception e) { } //for now, just catch, do nothing 
                }
            });
        }

        //create new animal trait
        //this method assumes that any trait added is already in the db 
        public async Task CreateAnimalTraitAsync(int animalId, string traitName)
        {
            await ExecuteInTransactionAsync(async _context =>
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
            });
        }

        /// <summary>
        /// [Obsolete] Use GetAllAnimalsAsync with species and sex parameters instead.
        /// Gets animals filtered by both sex and species.
        /// </summary>
        [Obsolete("Use GetAllAnimalsAsync(species, sex) instead")]
        public async Task<AnimalDto[]> GetAnimalInfoBySexAndSpecies(string sex, string species)
        {
            return await ExecuteInTransactionAsync(async _context =>
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
            });
        }

        /// <summary>
        /// [Obsolete] Use GetAllAnimalsAsync with sex parameter instead.
        /// Gets animals filtered by sex.
        /// </summary>
        [Obsolete("Use GetAllAnimalsAsync(sex: sex) instead")]
        public async Task<AnimalDto[]> GetAnimalsBySex(string sex)
        {
            return await ExecuteInTransactionAsync(async _context =>
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
            });
        }

        /// <summary>
        /// [Obsolete] Use GetAllAnimalsAsync with species parameter instead.
        /// Gets animals filtered by species.
        /// </summary>
        [Obsolete("Use GetAllAnimalsAsync(species: species) instead")]
        public async Task<AnimalDto[]> GetAnimalInfoBySpecies(string species)
        {
            return await ExecuteInTransactionAsync(async _context =>
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
            });
        }

        //get animal species
        //this must come via line -> stock -> species so putting in an easy way to access this here 
        public async Task<string> GetAnimalSpecies(int id)
        {
            return await ExecuteInTransactionAsync(async _context =>
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
            });
        }

        /// <summary>
        /// Updates an existing animal's information, including traits and lineage.
        /// 
        /// Process:
        /// 1. Validates animal exists
        /// 2. Updates basic information
        /// 3. Updates/adds traits
        /// 4. Updates/adds lineage connections
        /// 
        /// Note: 
        /// - Existing traits are preserved
        /// - New traits are added
        /// - Lineage connections are only added if they don't exist
        /// 
        /// TODO: Consider changing return type to bool for success/failure indication
        /// </summary>
        /// <param name="animalDto">Updated animal information</param>
        /// <returns>Updated Animal entity</returns>
        public async Task<Animal> UpdateAnimalAsync(AnimalDto animalDto)
        {
            return await ExecuteInTransactionAsync(async _context =>
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
                await SetAnimalTraits(animalDto);

                int? damId = animalDto.damId;
                int? sireId = animalDto.sireId;
                int animalId = animalDto.Id;

                //if ancestry connection doesn't exist, make it 
                if (damId != 0)
                {
                    var doesExist = await _lineageService.DoesAncestryConnectionExist(animalId, (int)damId);
                    bool connectionExists = doesExist;

                    //check if it exists
                    if (!connectionExists)
                    {
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
                            await _lineageService.AddLineageConnection(animalId, (int)sireId, 1, 2, "Paternal"); //TODO this is hardcoded for a dam 
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
            });
        }

        //check if animal is already in database 
        public async Task<bool> DoesAnimalExist(int id)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                var existingAnimal = await _context.Animal.FirstOrDefaultAsync(a => a.Id == id);
                if (existingAnimal != null)
                {
                    return true;
                }

                return false;
            });
        }

        /// <summary>
        /// Gets a specific animal by its ID
        /// </summary>
        /// <param name="id">The unique identifier of the animal</param>
        /// <returns>Animal details as AnimalDto</returns>
        /// <exception cref="KeyNotFoundException">Thrown when animal with specified ID is not found</exception>
        public async Task<AnimalDto> GetAnimalByIdAsync(int id)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                var animal = await _context.Animal.FirstOrDefaultAsync(a => a.Id == id);
                if (animal == null)
                {
                    throw new KeyNotFoundException($"Animal with ID {id} not found.");
                }

                return await MapSingleAnimaltoDto(animal);
            });
        }

        /// <summary>
        /// Get all animals with optional filtering by species, sex, and search term
        /// </summary>
        /// <param name="species">Optional species filter</param>
        /// <param name="sex">Optional sex filter (Male/Female/Unknown)</param>
        /// <param name="searchTerm">Optional search term for name or ID</param>
        /// <returns>Array of animals matching the specified criteria</returns>
        public async Task<AnimalDto[]> GetAllAnimalsAsync(string? species = null, string? sex = null, string? searchTerm = null)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                // Start with base query
                var animals = await _context.Animal
                    .Include(a => a.Line)
                    .ToListAsync();

                List<AnimalDto> animalDtos = new List<AnimalDto>();

                // Map to DTOs
                foreach (var animal in animals)
                {
                    animalDtos.Add(await MapSingleAnimaltoDto(animal));
                }

                // Apply filters
                var filteredAnimals = animalDtos.AsEnumerable();

                if (!string.IsNullOrEmpty(species))
                {
                    filteredAnimals = filteredAnimals.Where(a => 
                        a.species.Equals(species, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(sex))
                {
                    filteredAnimals = filteredAnimals.Where(a => 
                        a.sex.Equals(sex, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    filteredAnimals = filteredAnimals.Where(a =>
                        a.name.ToLower().Contains(searchTerm) ||
                        a.Id.ToString().Contains(searchTerm));
                }

                return filteredAnimals.ToArray();
            });
        }
        //convert from Animal to AnimalDto
        public async Task<AnimalDto> MapSingleAnimaltoDto(Animal a)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                if (a != null)
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
                        breeder = "TLDR", // TODO: Implement breeder lookup
                        species = speciesObj.CommonName,
                        imageUrl = a.imageUrl,
                        color = animalTraits.ContainsKey("Color") ? animalTraits["Color"].LastOrDefault() : null,
                        markings = animalTraits.ContainsKey("Marking") ? animalTraits["Marking"].LastOrDefault() : null,
                        earType = animalTraits.ContainsKey("Ear Type") ? animalTraits["Ear Type"].LastOrDefault() : null,
                        variety = animalTraits.ContainsKey("Coat Type") ? animalTraits["Coat Type"].LastOrDefault() : null,
                        damId = damId != 0 ? damId : (int?)null,
                        sireId = sireId != 0 ? sireId : (int?)null,
                    };

                    return result;
                }
                else
                {
                    return null;
                }
            });
        }

        /// <summary>
        /// Retrieves all traits associated with an animal.
        /// 
        /// Returns a dictionary with trait categories as keys:
        /// - Color
        /// - Marking
        /// - Ear Type
        /// - Coat Type
        /// 
        /// Note: If traits are not found, returns default "No X found" values
        /// rather than throwing an exception.
        /// 
        /// TODO: Consider if this error handling approach is optimal
        /// </summary>
        /// <param name="id">Animal ID</param>
        /// <returns>Dictionary mapping trait categories to lists of trait values</returns>
        public async Task<Dictionary<string, List<string>>> GetAnimalTraits(int id)
        {
            return await ExecuteInTransactionAsync(async _context =>
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
                catch (Exception e) //for now just catch exceptions and set default trait values to avoid breaking the world FIXME 
                {
                    //throw new KeyNotFoundException($"Traits for animal with ID {id} not found.");
                    //animal doesn't have to have traits, at least not right now TODO 
                    Console.WriteLine($"Traits for animal with ID {id} not found.");
                    traitMap["Color"] = new List<string> { "No color found" };
                    traitMap["Marking"] = new List<string> { "No markings found" };
                    traitMap["Ear Type"] = new List<string> { "No ear type found" };
                    traitMap["Coat Type"] = new List<string> { "No coat type found" };
                }

                if (traitMap.Count < 4) //FIXME this is assuming we have 4 traits just to auto set traits for now for now just make sure we have the 4 traits we're looking for
                {
                    if (traitMap.ContainsKey("Color") == false)
                    {
                        traitMap["Color"] = new List<string> { "No color found" };
                    }
                    if (traitMap.ContainsKey("Marking") == false)
                    {
                        traitMap["Marking"] = new List<string> { "No markings found" };
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
            });
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
            await ExecuteInContextAsync(async _context =>
            {
                var animal = await _context.Animal.FirstOrDefaultAsync(a => a.Id == id);
                if (animal == null)
                {
                    throw new KeyNotFoundException($"Animal with ID {id} not found.");
                }
                _context.Animal.Remove(animal);
                await _context.SaveChangesAsync();
            });
        }

        //get animal by registration number
        //FIXME reg has to be unique then and now I have an id and a registration number....
        public async Task<AnimalDto> GetAnimalByRegAsync(string registrationNum)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                var animal = await _context.Animal.FirstOrDefaultAsync(a => a.registrationNumber == registrationNum);
                if (animal == null)
                {
                    throw new KeyNotFoundException($"Animal with Registration Number {registrationNum} not found.");
                }

                return await MapSingleAnimaltoDto(animal);
            });
        }

        //update animal main image FIXME 
        public async Task<bool> UpdateAnimalImageByRegAsync(string regNum, string newImageUrl)
        {
            return await ExecuteInContextAsync(async _context =>
            {
                // Find the animal in the database by registration number
                var animal = await _context.Animal
                    .FirstOrDefaultAsync(a => a.registrationNumber == regNum);

                // Check if the animal was found
                if (animal == null)
                {
                    throw new KeyNotFoundException($"Animal with Registration Number {regNum} not found.");
                }

                // Update the image URL
                animal.imageUrl = newImageUrl;

                // Save changes to the database
                await _context.SaveChangesAsync();

                return true;
            });
        }

        //update animal carousal FIXME 

        //public async Task<bool> UpdateAnimalAdditionalImagesByRegAsync(string regNum, string[] newImageUrl)
        //{
        //    return await ExecuteInContextAsync(async _context =>
        //    {
        //        // Update the image URL
        //        foreach(var image in newImageUrl)
        //        {
        //            //look for the image + reg number pair
        //            //var image = await _context.AnimalImage; 
        //            // .FirstOrDefaultAsync(a => a.registrationNumber == regNum);
        //            //if it doesn't exist, add it
        //            //if it does exist, don't add it
        //        }

        //        // Save changes to the database
        //        await _context.SaveChangesAsync();

        //        return true;
        //    });
        //}
    }
}
