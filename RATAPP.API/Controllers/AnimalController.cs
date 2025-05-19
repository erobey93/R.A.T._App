using Microsoft.AspNetCore.Mvc;
using RATAPP.API.Models;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Services;

namespace RATAPP.API.Controllers
{
    /// <summary>
    /// Provides REST endpoints for managing animal data.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AnimalController : ControllerBase
    {
        private readonly AnimalService _animalService;
        private readonly ILogger<AnimalController> _logger;

        public AnimalController(AnimalService animalService, ILogger<AnimalController> logger)
        {
            _animalService = animalService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all animals with optional filtering
        /// </summary>
        /// <param name="species">Optional species filter (e.g., "Rat", "Mouse")</param>
        /// <param name="sex">Optional sex filter (e.g., "Male", "Female")</param>
        /// <param name="searchTerm">Optional search term for name or ID</param>
        /// <returns>List of animals matching the specified criteria</returns>
        /// <response code="200">Returns the list of animals</response>
        /// <response code="400">If the filter parameters are invalid</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<AnimalDto[]>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> GetAnimals(
            [FromQuery] string? species = null,
            [FromQuery] string? sex = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var animals = await _animalService.GetAllAnimalsAsync(species, sex, searchTerm);
                return Ok(ApiResponse<AnimalDto[]>.Success(animals));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Gets a specific animal by ID
        /// </summary>
        /// <param name="id">The unique identifier of the animal</param>
        /// <returns>The requested animal's details</returns>
        /// <response code="200">Returns the requested animal</response>
        /// <response code="404">If the animal is not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AnimalDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> GetAnimal(int id)
        {
            try
            {
                var animal = await _animalService.GetAnimalByIdAsync(id);
                return Ok(ApiResponse<AnimalDto>.Success(animal));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Creates a new animal
        /// </summary>
        /// <param name="animalDto">The animal data</param>
        /// <returns>The created animal</returns>
        /// <response code="201">Returns the newly created animal</response>
        /// <response code="400">If the animal data is invalid</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Animal>), 201)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> CreateAnimal([FromBody] AnimalDto animalDto)
        {
            try
            {
                var animal = await _animalService.CreateAnimalAsync(animalDto);
                return CreatedAtAction(
                    nameof(GetAnimal),
                    new { id = animal.Id },
                    ApiResponse<Animal>.Success(animal, "Animal created successfully")
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Updates an existing animal
        /// </summary>
        /// <param name="id">The ID of the animal to update</param>
        /// <param name="animalDto">The updated animal data</param>
        /// <returns>The updated animal</returns>
        /// <response code="200">Returns the updated animal</response>
        /// <response code="400">If the animal data is invalid</response>
        /// <response code="404">If the animal is not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<Animal>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> UpdateAnimal(int id, [FromBody] AnimalDto animalDto)
        {
            if (id != animalDto.Id)
            {
                return BadRequest(ApiResponse<object>.Error("ID in URL must match ID in request body"));
            }

            try
            {
                var animal = await _animalService.UpdateAnimalAsync(animalDto);
                return Ok(ApiResponse<Animal>.Success(animal, "Animal updated successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Error(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Deletes an animal
        /// </summary>
        /// <param name="id">The ID of the animal to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">If the animal was successfully deleted</response>
        /// <response code="404">If the animal is not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public async Task<IActionResult> DeleteAnimal(int id)
        {
            try
            {
                await _animalService.DeleteAnimalByIdAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Error(ex.Message));
            }
        }
    }
}
