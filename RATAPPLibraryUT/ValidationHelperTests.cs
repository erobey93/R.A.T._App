//using System;
//using System.Windows.Forms;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using RATAPP.Helpers;
//using RATAPPLibrary.Data.Models.Animal_Management;
//using RATAPPLibrary.Data.Models.Breeding;

//TODO
//namespace RATAPPLibraryUT
//{
//    [TestClass]
//    public class ValidationHelperTests
//    {
//        [TestMethod]
//        public void ValidateRequiredFields_WithAllFieldsFilled_ShouldReturnTrue()
//        {
//            // Arrange
//            var textBox = new TextBox { Text = "Test" };
//            var comboBox = new ComboBox();
//            comboBox.Items.Add("Item");
//            comboBox.SelectedIndex = 0;

//            // Act
//            var result = ValidationHelper.ValidateRequiredFields(textBox, comboBox);

//            // Assert
//            Assert.IsTrue(result);
//        }

//        [TestMethod]
//        public void ValidateRequiredFields_WithEmptyTextBox_ShouldReturnFalse()
//        {
//            // Arrange
//            var textBox = new TextBox { Text = "" };
//            var comboBox = new ComboBox();
//            comboBox.Items.Add("Item");
//            comboBox.SelectedIndex = 0;

//            // Act
//            var result = ValidationHelper.ValidateRequiredFields(textBox, comboBox);

//            // Assert
//            Assert.IsFalse(result);
//        }

//        [TestMethod]
//        public void ValidateRequiredFields_WithNoComboBoxSelection_ShouldReturnFalse()
//        {
//            // Arrange
//            var textBox = new TextBox { Text = "Test" };
//            var comboBox = new ComboBox();
//            comboBox.Items.Add("Item");
//            comboBox.SelectedIndex = -1;

//            // Act
//            var result = ValidationHelper.ValidateRequiredFields(textBox, comboBox);

//            // Assert
//            Assert.IsFalse(result);
//        }

//        [TestMethod]
//        public void ValidateRequiredFields_WithNullControls_ShouldReturnFalse()
//        {
//            // Act
//            var result = ValidationHelper.ValidateRequiredFields(null);

//            // Assert
//            Assert.IsFalse(result);
//        }

//        [TestMethod]
//        public void ValidatePairingInputs_WithValidData_ShouldReturnTrue()
//        {
//            // Arrange
//            var dam = new AnimalDto { Id = "DAM001", SpeciesId = 1 };
//            var sire = new AnimalDto { Id = "SIRE001", SpeciesId = 1 };
//            var project = new Project { Id = 1, Name = "Test Project" };
//            string errorMessage;

//            // Act
//            var result = ValidationHelper.ValidatePairingInputs(
//                "PAIR001",
//                dam,
//                sire,
//                project,
//                out errorMessage);

//            // Assert
//            Assert.IsTrue(result);
//            Assert.IsTrue(string.IsNullOrEmpty(errorMessage));
//        }

//        [TestMethod]
//        public void ValidatePairingInputs_WithEmptyPairingId_ShouldReturnFalse()
//        {
//            // Arrange
//            var dam = new AnimalDto { Id = "DAM001", SpeciesId = 1 };
//            var sire = new AnimalDto { Id = "SIRE001", SpeciesId = 1 };
//            var project = new Project { Id = 1, Name = "Test Project" };
//            string errorMessage;

//            // Act
//            var result = ValidationHelper.ValidatePairingInputs(
//                "",
//                dam,
//                sire,
//                project,
//                out errorMessage);

//            // Assert
//            Assert.IsFalse(result);
//            Assert.AreEqual("Pairing ID is required", errorMessage);
//        }

//        [TestMethod]
//        public void ValidatePairingInputs_WithNullDam_ShouldReturnFalse()
//        {
//            // Arrange
//            var sire = new AnimalDto { Id = "SIRE001", SpeciesId = 1 };
//            var project = new Project { Id = 1, Name = "Test Project" };
//            string errorMessage;

//            // Act
//            var result = ValidationHelper.ValidatePairingInputs(
//                "PAIR001",
//                null,
//                sire,
//                project,
//                out errorMessage);

//            // Assert
//            Assert.IsFalse(result);
//            Assert.AreEqual("Dam selection is required", errorMessage);
//        }

//        [TestMethod]
//        public void ValidatePairingInputs_WithNullSire_ShouldReturnFalse()
//        {
//            // Arrange
//            var dam = new AnimalDto { Id = "DAM001", SpeciesId = 1 };
//            var project = new Project { Id = 1, Name = "Test Project" };
//            string errorMessage;

//            // Act
//            var result = ValidationHelper.ValidatePairingInputs(
//                "PAIR001",
//                dam,
//                null,
//                project,
//                out errorMessage);

//            // Assert
//            Assert.IsFalse(result);
//            Assert.AreEqual("Sire selection is required", errorMessage);
//        }

//        [TestMethod]
//        public void ValidatePairingInputs_WithNullProject_ShouldReturnFalse()
//        {
//            // Arrange
//            var dam = new AnimalDto { Id = "DAM001", SpeciesId = 1 };
//            var sire = new AnimalDto { Id = "SIRE001", SpeciesId = 1 };
//            string errorMessage;

//            // Act
//            var result = ValidationHelper.ValidatePairingInputs(
//                "PAIR001",
//                dam,
//                sire,
//                null,
//                out errorMessage);

//            // Assert
//            Assert.IsFalse(result);
//            Assert.AreEqual("Project selection is required", errorMessage);
//        }

//        [TestMethod]
//        public void ValidatePairingInputs_WithSameDamAndSire_ShouldReturnFalse()
//        {
//            // Arrange
//            var animal = new AnimalDto { Id = "ANIMAL001", SpeciesId = 1 };
//            var project = new Project { Id = 1, Name = "Test Project" };
//            string errorMessage;

//            // Act
//            var result = ValidationHelper.ValidatePairingInputs(
//                "PAIR001",
//                animal,
//                animal,
//                project,
//                out errorMessage);

//            // Assert
//            Assert.IsFalse(result);
//            Assert.AreEqual("Dam and Sire cannot be the same animal", errorMessage);
//        }

//        [TestMethod]
//        public void ValidatePairingInputs_WithDifferentSpecies_ShouldReturnFalse()
//        {
//            // Arrange
//            var dam = new AnimalDto { Id = "DAM001", SpeciesId = 1 };
//            var sire = new AnimalDto { Id = "SIRE001", SpeciesId = 2 }; // Different species
//            var project = new Project { Id = 1, Name = "Test Project" };
//            string errorMessage;

//            // Act
//            var result = ValidationHelper.ValidatePairingInputs(
//                "PAIR001",
//                dam,
//                sire,
//                project,
//                out errorMessage);

//            // Assert
//            Assert.IsFalse(result);
//            Assert.AreEqual("Dam and Sire must be of the same species", errorMessage);
//        }

//        [TestMethod]
//        public void ValidateMultiplePairings_WithValidData_ShouldReturnTrue()
//        {
//            // Arrange
//            var grid = new DataGridView();
//            grid.Columns.Add("PairingID", "Pairing ID");
//            grid.Rows.Add("PAIR001");
//            grid.Rows.Add("PAIR002");
//            string errorMessage;

//            // Act
//            var result = ValidationHelper.ValidateMultiplePairings(grid, out errorMessage);

//            // Assert
//            Assert.IsTrue(result);
//            Assert.IsTrue(string.IsNullOrEmpty(errorMessage));
//        }

//        [TestMethod]
//        public void ValidateMultiplePairings_WithEmptyGrid_ShouldReturnFalse()
//        {
//            // Arrange
//            var grid = new DataGridView();
//            grid.Columns.Add("PairingID", "Pairing ID");
//            string errorMessage;

//            // Act
//            var result = ValidationHelper.ValidateMultiplePairings(grid, out errorMessage);

//            // Assert
//            Assert.IsFalse(result);
//            Assert.AreEqual("At least one pairing is required", errorMessage);
//        }

//        [TestMethod]
//        public void ValidateMultiplePairings_WithDuplicatePairingIds_ShouldReturnFalse()
//        {
//            // Arrange
//            var grid = new DataGridView();
//            grid.Columns.Add("PairingID", "Pairing ID");
//            grid.Rows.Add("PAIR001");
//            grid.Rows.Add("PAIR001"); // Duplicate ID
//            string errorMessage;

//            // Act
//            var result = ValidationHelper.ValidateMultiplePairings(grid, out errorMessage);

//            // Assert
//            Assert.IsFalse(result);
//            Assert.AreEqual("Duplicate Pairing IDs found", errorMessage);
//        }
//    }
//}
