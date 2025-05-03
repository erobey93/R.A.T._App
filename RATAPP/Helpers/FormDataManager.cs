using RATAPPLibrary.Services;
using RATAPPLibrary.Data.Models.Breeding;
using System.Threading.Tasks;
using System.Collections.Generic;
using RATAPPLibrary.Data.Models;

namespace RATAPP.Helpers
{
    /// <summary>
    /// Generic data manager for forms handling breeding-related data
    /// </summary>
    public class FormDataManager
    {
        private readonly BreedingService _breedingService;
        private readonly SpeciesService _speciesService;
        private readonly ProjectService _projectService;
        private readonly AnimalService _animalService;

        public FormDataManager(
            BreedingService breedingService,
            SpeciesService speciesService,
            ProjectService projectService,
            AnimalService animalService)
        {
            _breedingService = breedingService;
            _speciesService = speciesService;
            _projectService = projectService;
            _animalService = animalService;
        }

        public async Task<AnimalDto[]> GetDamsAsync(string species)
        {
            return await _animalService.GetAnimalInfoBySexAndSpecies("Female", species);
        }

        public async Task<AnimalDto[]> GetSiresAsync(string species)
        {
            return await _animalService.GetAnimalInfoBySexAndSpecies("Male", species);
        }

        public async Task<IEnumerable<string>> GetSpeciesAsync()
        {
            return await _speciesService.GetAllSpeciesAsync();
        }

        public async Task<List<Project>> GetProjectsAsync()
        {
            return await _projectService.GetAllProjectsAsync();
        }

        public async Task<List<Pairing>> GetPairingsAsync()
        {
            return await _breedingService.GetAllPairingsAsync();
        }

        public async Task SaveLitterAsync(Litter litter)
        {
            await _breedingService.CreateLitterAsync(litter);
        }

        public async Task SavePairingAsync(Pairing pairing)
        {
            await _breedingService.CreatePairingAsync(pairing);
        }
    }
}
