using Microsoft.AspNetCore.Mvc;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Services;
using System.ComponentModel.DataAnnotations;

namespace RATAPP_WEB.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnimalController : ControllerBase
    {
        private readonly AnimalService _animalService;
        private readonly ILogger<AnimalController> _logger;

        public AnimalController(RatAppDbContextFactory contextFactory, ILogger<AnimalController> logger)
        {
            _animalService = new AnimalService(contextFactory);
            _logger = logger;
        }

        /// <summary>
        /// Get all animals with optional filtering
        /// </summary>
        /// <param name="species">Filter by species name</param>
        /// <param name="sex">Filter by sex</param>
        /// <param name="searchTerm">Search by name or ID</param>
        /// <returns>Array of animals matching the criteria</returns>
        [HttpGet]
        public async Task<ActionResult<AnimalDto[]>> GetAnimals(
            [FromQuery] string? species = null,
            [FromQuery] string? sex = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                AnimalDto[] animals;

                if (!string.IsNullOrEmpty(species) && !string.IsNullOrEmpty(sex))
                {
                    animals = await _animalService.GetAnimalInfoBySexAndSpecies(sex, species);
                }
                else if (!string.IsNullOrEmpty(species))
                {
                    animals = await _animalService.GetAnimalInfoBySpecies(species);
                }
                else if (!string.IsNullOrEmpty(sex))
                {
                    animals = await _animalService.GetAnimalsBySex(sex);
                }
                else
                {
                    animals = await _animalService.GetAllAnimalsAsync();
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    animals = animals.Where(a => 
                        a.name.ToLower().Contains(searchTerm) || 
                        a.Id.ToString().Contains(searchTerm)
                    ).ToArray();
                }

                return Ok(animals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting animals");
                return StatusCode(500, "An error occurred while retrieving animals");
            }
        }

        /// <summary>
        /// Get a specific animal by ID
        /// </summary>
        /// <param name="id">Animal ID</param>
        /// <returns>Animal details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<AnimalDto>> GetAnimal(int id)
        {
            try
            {
                var animal = await _animalService.GetAnimalByIdAsync(id);
                if (animal == null)
                {
                    return NotFound($"Animal with ID {id} not found");
                }
                return Ok(animal);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Animal with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting animal {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the animal");
            }
        }

        /// <summary>
        /// Get animals by species
        /// </summary>
        /// <param name="speciesName">Species name</param>
        /// <returns>Array of animals of the specified species</returns>
        [HttpGet("species/{speciesName}")]
        public async Task<ActionResult<AnimalDto[]>> GetAnimalsBySpecies(string speciesName)
        {
            try
            {
                var animals = await _animalService.GetAnimalInfoBySpecies(speciesName);
                return Ok(animals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting animals for species {SpeciesName}", speciesName);
                return StatusCode(500, "An error occurred while retrieving animals");
            }
        }

        /// <summary>
        /// Get animals by sex
        /// </summary>
        /// <param name="sex">Sex (Male/Female/Unknown)</param>
        /// <returns>Array of animals of the specified sex</returns>
        [HttpGet("sex/{sex}")]
        public async Task<ActionResult<AnimalDto[]>> GetAnimalsBySex(string sex)
        {
            try
            {
                var animals = await _animalService.GetAnimalsBySex(sex);
                return Ok(animals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting animals for sex {Sex}", sex);
                return StatusCode(500, "An error occurred while retrieving animals");
            }
        }
    }
}
