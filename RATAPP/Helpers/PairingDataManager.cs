using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Data.Models.Animal_Management;
using RATAPPLibrary.Services;

namespace RATAPP.Helpers
{
    /// <summary>
    /// Manages all data operations for the pairing form.
    /// This pattern can be applied to other forms that need similar data management.
    /// </summary>
    public class PairingDataManager
    {
        private readonly BreedingService _breedingService;
        private readonly SpeciesService _speciesService;
        private readonly ProjectService _projectService;
        private readonly AnimalService _animalService;

        /// <summary>
        /// Data transfer object for pairing information
        /// </summary>
        public class PairingData
        {
            public string PairingId { get; set; }
            public AnimalDto Dam { get; set; }
            public AnimalDto Sire { get; set; }
            public Project Project { get; set; }
            public DateTime PairingDate { get; set; }
            public Species Species { get; set; }
        }

        public PairingDataManager(
            BreedingService breedingService,
            SpeciesService speciesService,
            ProjectService projectService,
            AnimalService animalService)
        {
            _breedingService = breedingService ?? throw new ArgumentNullException(nameof(breedingService));
            _speciesService = speciesService ?? throw new ArgumentNullException(nameof(speciesService));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _animalService = animalService ?? throw new ArgumentNullException(nameof(animalService));
        }

        /// <summary>
        /// Loads all available species
        /// </summary>
        public async Task<List<Species>> LoadSpeciesAsync()
        {
            try
            {
                var species = await _speciesService.GetAllSpeciesObjectsAsync();
                return species?.ToList() ?? new List<Species>();
            }
            catch (Exception ex)
            {
                ValidationHelper.ShowError($"Error loading species: {ex.Message}");
                return new List<Species>();
            }
        }

        /// <summary>
        /// Loads animals based on sex and species
        /// </summary>
        public async Task<List<AnimalDto>> LoadAnimalsAsync(string sex, string species)
        {
            try
            {
                if (string.IsNullOrEmpty(species) || species == "Unknown")
                {
                    var animals = await _animalService.GetAnimalsBySex(sex);
                    return animals?.ToList() ?? new List<AnimalDto>();
                }
                else
                {
                    var animals = await _animalService.GetAnimalInfoBySexAndSpecies(sex, species);
                    return animals?.ToList() ?? new List<AnimalDto>();
                }
            }
            catch (Exception ex)
            {
                ValidationHelper.ShowError($"Error loading animals: {ex.Message}");
                return new List<AnimalDto>();
            }
        }

        /// <summary>
        /// Loads all available projects
        /// </summary>
        public async Task<List<Project>> LoadProjectsAsync()
        {
            try
            {
                var projects = await _projectService.GetAllProjectsAsync();
                return projects ?? new List<Project>();
            }
            catch (Exception ex)
            {
                ValidationHelper.ShowError($"Error loading projects: {ex.Message}");
                return new List<Project>();
            }
        }

        /// <summary>
        /// Saves a single pairing
        /// </summary>
        public async Task<bool> SavePairingAsync(PairingData data)
        {
            try
            {
                // Validate inputs
                string errorMessage;
                if (!ValidationHelper.ValidatePairingInputs(
                    data.PairingId,
                    data.Dam,
                    data.Sire,
                    data.Project,
                    out errorMessage))
                {
                    ValidationHelper.ShowError(errorMessage);
                    return false;
                }

                // Create pairing
                await _breedingService.CreatePairingAsync(
                    data.PairingId,
                    data.Dam.Id,
                    data.Sire.Id,
                    data.Project.Id,
                    data.PairingDate,
                    null // End date is optional
                );

                ValidationHelper.ShowSuccess("Pairing added successfully!");
                return true;
            }
            catch (Exception ex)
            {
                ValidationHelper.ShowError($"Error saving pairing: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Saves multiple pairings in bulk
        /// </summary>
        public async Task<bool> SaveMultiplePairingsAsync(List<PairingData> pairings)
        {
            try
            {
                foreach (var pairing in pairings)
                {
                    // Validate each pairing
                    string errorMessage;
                    if (!ValidationHelper.ValidatePairingInputs(
                        pairing.PairingId,
                        pairing.Dam,
                        pairing.Sire,
                        pairing.Project,
                        out errorMessage))
                    {
                        ValidationHelper.ShowError($"Error in pairing {pairing.PairingId}: {errorMessage}");
                        return false;
                    }

                    // Create each pairing
                    await _breedingService.CreatePairingAsync(
                        pairing.PairingId,
                        pairing.Dam.Id,
                        pairing.Sire.Id,
                        pairing.Project.Id,
                        pairing.PairingDate,
                        null // End date is optional
                    );
                }

                ValidationHelper.ShowSuccess($"Successfully added {pairings.Count} pairings!");
                return true;
            }
            catch (Exception ex)
            {
                ValidationHelper.ShowError($"Error saving pairings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets a species by its common name
        /// </summary>
        public Species GetSpeciesByCommonName(List<Species> allSpecies, string commonName)
        {
            return allSpecies?.FirstOrDefault(s => s.CommonName == commonName);
        }
    }
}
