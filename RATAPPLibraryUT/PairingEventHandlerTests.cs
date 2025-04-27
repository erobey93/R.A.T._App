using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RATAPP.Helpers;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Animal_Management;
using RATAPPLibrary.Data.Models.Breeding;

namespace RATAPPLibraryUT
{
    [TestClass]
    public class PairingEventHandlerTests : TestBase
    {
        private PairingEventHandler _eventHandler;
        private PairingDataManager _dataManager;
        private LoadingSpinnerHelper _spinner;
        private Form _testForm;

        private ComboBox _speciesComboBox;
        private ComboBox _damComboBox;
        private ComboBox _sireComboBox;
        private ComboBox _projectComboBox;
        private TextBox _pairingIdTextBox;
        private DateTimePicker _pairingDatePicker;
        private DataGridView _pairingsGrid;

        [TestInitialize]
        public void Setup()
        {
            var context = CreateInMemoryContext();
            _testForm = new Form();
            _spinner = new LoadingSpinnerHelper(_testForm, "RATAPP/Resources/Loading_2.gif");
            _dataManager = new PairingDataManager(
                new RATAPPLibrary.Services.BreedingService(context),
                new RATAPPLibrary.Services.SpeciesService(context),
                new RATAPPLibrary.Services.ProjectService(context),
                new RATAPPLibrary.Services.AnimalService(context));
            _eventHandler = new PairingEventHandler(_dataManager, _spinner);

            InitializeControls();
        }

        private void InitializeControls()
        {
            _speciesComboBox = new ComboBox();
            _damComboBox = new ComboBox();
            _sireComboBox = new ComboBox();
            _projectComboBox = new ComboBox();
            _pairingIdTextBox = new TextBox();
            _pairingDatePicker = new DateTimePicker();
            _pairingsGrid = PairingFormComponentFactory.CreatePairingsGrid();
        }

        [TestMethod]
        public async Task HandleFormLoadAsync_ShouldPopulateAllComboBoxes()
        {
            // Act
            await _eventHandler.HandleFormLoadAsync(
                _speciesComboBox,
                _damComboBox,
                _sireComboBox,
                _projectComboBox);

            // Assert
            Assert.IsTrue(_speciesComboBox.Items.Count > 0, "Species combo box should be populated");
            Assert.IsTrue(_damComboBox.Items.Count > 0, "Dam combo box should be populated");
            Assert.IsTrue(_sireComboBox.Items.Count > 0, "Sire combo box should be populated");
            Assert.IsTrue(_projectComboBox.Items.Count > 0, "Project combo box should be populated");
        }

        [TestMethod]
        public async Task HandleSpeciesSelectionChangedAsync_ShouldUpdateAnimalComboBoxes()
        {
            // Arrange
            await _eventHandler.HandleFormLoadAsync(
                _speciesComboBox,
                _damComboBox,
                _sireComboBox,
                _projectComboBox);

            _speciesComboBox.SelectedIndex = 0;

            // Act
            await _eventHandler.HandleSpeciesSelectionChangedAsync(
                _speciesComboBox,
                _damComboBox,
                _sireComboBox);

            // Assert
            Assert.IsTrue(_damComboBox.Items.Count > 0, "Dam combo box should be updated");
            Assert.IsTrue(_sireComboBox.Items.Count > 0, "Sire combo box should be updated");
        }

        [TestMethod]
        public async Task HandleAddPairingClickAsync_WithValidData_ShouldSavePairing()
        {
            // Arrange
            await _eventHandler.HandleFormLoadAsync(
                _speciesComboBox,
                _damComboBox,
                _sireComboBox,
                _projectComboBox);

            _speciesComboBox.SelectedIndex = 0;
            _damComboBox.SelectedIndex = 0;
            _sireComboBox.SelectedIndex = 0;
            _projectComboBox.SelectedIndex = 0;
            _pairingIdTextBox.Text = "TEST001";
            _pairingDatePicker.Value = DateTime.Today;

            // Act
            await _eventHandler.HandleAddPairingClickAsync(
                _pairingIdTextBox.Text,
                _damComboBox,
                _sireComboBox,
                _projectComboBox,
                _pairingDatePicker);

            // Assert
            Assert.AreEqual(string.Empty, _pairingIdTextBox.Text, "Pairing ID should be cleared");
            Assert.AreEqual(-1, _damComboBox.SelectedIndex, "Dam selection should be cleared");
            Assert.AreEqual(-1, _sireComboBox.SelectedIndex, "Sire selection should be cleared");
            Assert.AreEqual(-1, _projectComboBox.SelectedIndex, "Project selection should be cleared");
        }

        [TestMethod]
        public void HandleAddToGridClick_WithValidData_ShouldAddRowToGrid()
        {
            // Arrange
            _pairingIdTextBox.Text = "TEST001";
            _damComboBox.Items.Add(new DropdownHelper.ListItem<AnimalDto>
            {
                Display = "DAM001 - Test Dam",
                Value = new AnimalDto { Id = "DAM001", Name = "Test Dam" }
            });
            _sireComboBox.Items.Add(new DropdownHelper.ListItem<AnimalDto>
            {
                Display = "SIRE001 - Test Sire",
                Value = new AnimalDto { Id = "SIRE001", Name = "Test Sire" }
            });
            _projectComboBox.Items.Add(new DropdownHelper.ListItem<Project>
            {
                Display = "Test Project",
                Value = new Project { Id = 1, Name = "Test Project" }
            });

            _damComboBox.SelectedIndex = 0;
            _sireComboBox.SelectedIndex = 0;
            _projectComboBox.SelectedIndex = 0;

            // Act
            _eventHandler.HandleAddToGridClick(
                _pairingIdTextBox.Text,
                _damComboBox,
                _sireComboBox,
                _projectComboBox,
                _pairingDatePicker,
                _pairingsGrid);

            // Assert
            Assert.AreEqual(1, _pairingsGrid.Rows.Count, "Grid should have one row");
            Assert.AreEqual("TEST001", _pairingsGrid.Rows[0].Cells["PairingID"].Value);
            Assert.AreEqual(string.Empty, _pairingIdTextBox.Text, "Pairing ID should be cleared");
        }

        [TestMethod]
        public async Task HandleSaveAllPairingsAsync_WithValidData_ShouldSaveAndClearGrid()
        {
            // Arrange
            _pairingsGrid.Rows.Add("TEST001", "Test Project", "DAM001 - Test Dam", "SIRE001 - Test Sire", DateTime.Today.ToShortDateString());
            _pairingsGrid.Rows.Add("TEST002", "Test Project", "DAM002 - Test Dam", "SIRE002 - Test Sire", DateTime.Today.ToShortDateString());

            // Act
            await _eventHandler.HandleSaveAllPairingsAsync(
                _pairingsGrid,
                _damComboBox,
                _sireComboBox,
                _projectComboBox);

            // Assert
            Assert.AreEqual(0, _pairingsGrid.Rows.Count, "Grid should be cleared after saving");
        }

        [TestMethod]
        public void HandleCancelClick_WithUserConfirmation_ShouldCloseForm()
        {
            // Arrange
            bool formClosed = false;
            _testForm.FormClosed += (s, e) => formClosed = true;

            // Act
            _eventHandler.HandleCancelClick(_testForm);

            // Assert
            Assert.IsTrue(formClosed, "Form should be closed");
        }
    }
}
