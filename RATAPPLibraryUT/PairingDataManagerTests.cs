//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using RATAPP.Helpers;
//using RATAPPLibrary.Services;
//using RATAPPLibrary.Data.Models.Animal_Management;
//using RATAPPLibrary.Data.Models.Breeding;

//TODO
//namespace RATAPPLibraryUT
//{
//    [TestClass]
//    public class PairingDataManagerTests : TestBase
//    {
//        private PairingDataManager _dataManager;
//        private BreedingService _breedingService;
//        private SpeciesService _speciesService;
//        private ProjectService _projectService;
//        private AnimalService _animalService;

//        [TestInitialize]
//        public void Setup()
//        {
//            var context = CreateInMemoryContext();
//            _breedingService = new BreedingService(context);
//            _speciesService = new SpeciesService(context);
//            _projectService = new ProjectService(context);
//            _animalService = new AnimalService(context);

//            _dataManager = new PairingDataManager(
//                _breedingService,
//                _speciesService,
//                _projectService,
//                _animalService);
//        }

//        [TestMethod]
//        public async Task LoadSpeciesAsync_ShouldReturnAllSpecies()
//        {
//            // Act
//            var species = await _dataManager.LoadSpeciesAsync();

//            // Assert
//            Assert.IsNotNull(species);
//            Assert.AreEqual(2, species.Count);
//            Assert.IsTrue(species.Any(s => s.CommonName == "Rat"));
//            Assert.IsTrue(species.Any(s => s.CommonName == "Mouse"));
//        }

//        [TestMethod]
//        public async Task LoadAnimalsAsync_WithFemaleAndRatSpecies_ShouldReturnOnlyFemaleDams()
//        {
//            // Act
//            var animals = await _dataManager.LoadAnimalsAsync("Female", "Rat");

//            // Assert
//            Assert.IsNotNull(animals);
//            Assert.AreEqual(2, animals.Count);
//            Assert.IsTrue(animals.All(a => a.Id.StartsWith("DAM")));
//        }

//        [TestMethod]
//        public async Task LoadAnimalsAsync_WithMaleAndRatSpecies_ShouldReturnOnlyMaleSires()
//        {
//            // Act
//            var animals = await _dataManager.LoadAnimalsAsync("Male", "Rat");

//            // Assert
//            Assert.IsNotNull(animals);
//            Assert.AreEqual(2, animals.Count);
//            Assert.IsTrue(animals.All(a => a.Id.StartsWith("SIRE")));
//        }

//        [TestMethod]
//        public async Task LoadProjectsAsync_ShouldReturnAllProjects()
//        {
//            // Act
//            var projects = await _dataManager.LoadProjectsAsync();

//            // Assert
//            Assert.IsNotNull(projects);
//            Assert.AreEqual(2, projects.Count);
//            Assert.IsTrue(projects.Any(p => p.Name == "Test Project 1"));
//            Assert.IsTrue(projects.Any(p => p.Name == "Test Project 2"));
//        }

//        [TestMethod]
//        public async Task SavePairingAsync_WithValidData_ShouldSaveSuccessfully()
//        {
//            // Arrange
//            var species = (await _speciesService.GetAllSpeciesAsync()).First();
//            var dam = (await _animalService.GetAnimalsBySex("Female")).First();
//            var sire = (await _animalService.GetAnimalsBySex("Male")).First();
//            var project = (await _projectService.GetAllProjectsAsync()).First();

//            var pairingData = new PairingDataManager.PairingData
//            {
//                PairingId = "TEST001",
//                Dam = dam,
//                Sire = sire,
//                Project = project,
//                PairingDate = DateTime.Now,
//                Species = species
//            };

//            // Act
//            var result = await _dataManager.SavePairingAsync(pairingData);

//            // Assert
//            Assert.IsTrue(result);
//            var savedPairing = await _breedingService.GetPairingByIdAsync("TEST001");
//            Assert.IsNotNull(savedPairing);
//            Assert.AreEqual(dam.Id, savedPairing.DamId);
//            Assert.AreEqual(sire.Id, savedPairing.SireId);
//            Assert.AreEqual(project.Id, savedPairing.ProjectId);
//        }

//        [TestMethod]
//        public async Task SavePairingAsync_WithInvalidData_ShouldReturnFalse()
//        {
//            // Arrange
//            var pairingData = new PairingDataManager.PairingData
//            {
//                PairingId = "TEST002",
//                Dam = null, // Invalid - missing dam
//                Sire = (await _animalService.GetAnimalsBySex("Male")).First(),
//                Project = (await _projectService.GetAllProjectsAsync()).First(),
//                PairingDate = DateTime.Now
//            };

//            // Act
//            var result = await _dataManager.SavePairingAsync(pairingData);

//            // Assert
//            Assert.IsFalse(result);
//            var savedPairing = await _breedingService.GetPairingByIdAsync("TEST002");
//            Assert.IsNull(savedPairing);
//        }

//        [TestMethod]
//        public async Task SaveMultiplePairingsAsync_WithValidData_ShouldSaveAllPairings()
//        {
//            // Arrange
//            var species = (await _speciesService.GetAllSpeciesAsync()).First();
//            var dams = await _animalService.GetAnimalsBySex("Female");
//            var sires = await _animalService.GetAnimalsBySex("Male");
//            var projects = await _projectService.GetAllProjectsAsync();

//            var pairings = new[]
//            {
//                new PairingDataManager.PairingData
//                {
//                    PairingId = "MULTI001",
//                    Dam = dams.First(),
//                    Sire = sires.First(),
//                    Project = projects.First(),
//                    PairingDate = DateTime.Now,
//                    Species = species
//                },
//                new PairingDataManager.PairingData
//                {
//                    PairingId = "MULTI002",
//                    Dam = dams.Last(),
//                    Sire = sires.Last(),
//                    Project = projects.Last(),
//                    PairingDate = DateTime.Now,
//                    Species = species
//                }
//            };

//            // Act
//            var result = await _dataManager.SaveMultiplePairingsAsync(pairings.ToList());

//            // Assert
//            Assert.IsTrue(result);
//            var savedPairing1 = await _breedingService.GetPairingByIdAsync("MULTI001");
//            var savedPairing2 = await _breedingService.GetPairingByIdAsync("MULTI002");
//            Assert.IsNotNull(savedPairing1);
//            Assert.IsNotNull(savedPairing2);
//        }

//        [TestMethod]
//        public async Task SaveMultiplePairingsAsync_WithInvalidData_ShouldNotSaveAnyPairings()
//        {
//            // Arrange
//            var species = (await _speciesService.GetAllSpeciesAsync()).First();
//            var dams = await _animalService.GetAnimalsBySex("Female");
//            var sires = await _animalService.GetAnimalsBySex("Male");
//            var projects = await _projectService.GetAllProjectsAsync();

//            var pairings = new[]
//            {
//                new PairingDataManager.PairingData
//                {
//                    PairingId = "MULTI003",
//                    Dam = dams.First(),
//                    Sire = sires.First(),
//                    Project = projects.First(),
//                    PairingDate = DateTime.Now,
//                    Species = species
//                },
//                new PairingDataManager.PairingData
//                {
//                    PairingId = "MULTI004",
//                    Dam = null, // Invalid - missing dam
//                    Sire = sires.Last(),
//                    Project = projects.Last(),
//                    PairingDate = DateTime.Now,
//                    Species = species
//                }
//            };

//            // Act
//            var result = await _dataManager.SaveMultiplePairingsAsync(pairings.ToList());

//            // Assert
//            Assert.IsFalse(result);
//            var savedPairing1 = await _breedingService.GetPairingByIdAsync("MULTI003");
//            var savedPairing2 = await _breedingService.GetPairingByIdAsync("MULTI004");
//            Assert.IsNull(savedPairing1);
//            Assert.IsNull(savedPairing2);
//        }

//        [TestMethod]
//        public void GetSpeciesByCommonName_ShouldReturnCorrectSpecies()
//        {
//            // Arrange
//            var allSpecies = new List<Species>
//            {
//                new Species { Id = 1, CommonName = "Rat" },
//                new Species { Id = 2, CommonName = "Mouse" }
//            };

//            // Act
//            var result = _dataManager.GetSpeciesByCommonName(allSpecies, "Rat");

//            // Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("Rat", result.CommonName);
//        }

//        [TestMethod]
//        public void GetSpeciesByCommonName_WithInvalidName_ShouldReturnNull()
//        {
//            // Arrange
//            var allSpecies = new List<Species>
//            {
//                new Species { Id = 1, CommonName = "Rat" },
//                new Species { Id = 2, CommonName = "Mouse" }
//            };

//            // Act
//            var result = _dataManager.GetSpeciesByCommonName(allSpecies, "Invalid");

//            // Assert
//            Assert.IsNull(result);
//        }
//    }
//}
