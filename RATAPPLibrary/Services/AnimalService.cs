using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Breeding;
using Microsoft.EntityFrameworkCore;
using System;

namespace RATAPPLibrary.Services
{
    public class AnimalService
    {
        private readonly AnimalDbContext _context;
        private readonly LineService _lineService;

        public AnimalService(AnimalDbContext context, LineService lineService)
        {
            _context = context;
            _lineService = lineService;
        }

        public async Task<Animal> CreateAnimalAsync(string variety, DateTime dateOfBirth, string sex, string commonSpecies, string? name = null)
        {
            // Get or create the line for the animal based on the variety
            var line = await _lineService.GetOrCreateLineAsync(variety);
            var species = await _context.Species.FirstOrDefaultAsync(s => s.ScientificName == commonSpecies);

            var newAnimal = new Animal
            {
                DateOfBirth = dateOfBirth,
                Sex = sex,
                LineId = line.Id,
                Name = name // Can be null; DisplayName will handle defaulting
            };

            _context.Animals.Add(newAnimal);
            await _context.SaveChangesAsync();

            return newAnimal;
        }
    }
}
