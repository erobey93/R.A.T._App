using System;
using System.Drawing;
using System.Windows.Forms;
using RATAPP.Helpers;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Services;

namespace RATAPP.Forms
{
    /// <summary>
    /// Form for adding new pairings, either single or multiple.
    /// Uses helper classes to manage UI, data, and events.
    /// </summary>
    public partial class AddPairingForm : Form
    {
        // Helper classes
        private readonly PairingDataManager _dataManager;
        private readonly PairingEventHandler _eventHandler;
        private readonly LoadingSpinnerHelper _spinner;

        // UI Controls
        private TabControl tabControl;
        private ComboBox damComboBox;
        private ComboBox sireComboBox;
        private ComboBox speciesComboBox;
        private TextBox pairingIdTextBox;
        private ComboBox projectComboBox;
        private DateTimePicker pairingDatePicker;
        private Button addButton;
        private Button cancelButton;
        private DataGridView multiplePairingsGrid;
        private Button addToGridButton;
        private Button saveAllButton;
        private Button importButton;

        public AddPairingForm(RATAPPLibrary.Data.DbContexts.RatAppDbContext context)
        {
            // Initialize services
            var breedingService = new BreedingService(context);
            var speciesService = new SpeciesService(context);
            var animalService = new AnimalService(context);
            var projectService = new ProjectService(context);

            // Initialize helper classes
            _dataManager = new PairingDataManager(
                breedingService,
                speciesService,
                projectService,
                animalService);

            _spinner = new LoadingSpinnerHelper(this, "RATAPP/Resources/Loading_2.gif");
            _eventHandler = new PairingEventHandler(_dataManager, _spinner);

            InitializeComponents();
            InitializeEventHandlers();
        }

        private void InitializeComponents()
        {
            // Set form properties
            this.Text = "Add Pairing";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Create header
            var headerPanel = new Panel();
            FormStyleHelper.ApplyHeaderStyle(headerPanel, "Add Pairing");
            this.Controls.Add(headerPanel);

            // Create container panel for tabControl
            var tabContainerPanel = new Panel
            {
                Location = new Point(0, headerPanel.Bottom),
                Width = this.ClientSize.Width,
                Height = this.ClientSize.Height - headerPanel.Height,
                Dock = DockStyle.Fill
            };

            // Create and style tab control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                Padding = new Point(12, 4)
            };

            // Initialize tabs
            var singlePairingTab = PairingFormComponentFactory.CreatePairingTabPage("Single Pairing");
            InitializeSinglePairingTab(singlePairingTab);
            tabControl.TabPages.Add(singlePairingTab);

            var multiplePairingsTab = PairingFormComponentFactory.CreatePairingTabPage("Multiple Pairings");
            InitializeMultiplePairingsTab(multiplePairingsTab);
            tabControl.TabPages.Add(multiplePairingsTab);

            tabContainerPanel.Controls.Add(tabControl);
            this.Controls.Add(tabContainerPanel);
        }

        private void InitializeSinglePairingTab(TabPage tab)
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // Create pairing information group
            var pairingGroup = PairingFormComponentFactory.CreateFormSection("Pairing Information", DockStyle.Top, 300);

            // Create form fields
            speciesComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(speciesComboBox);
            var speciesField = PairingFormComponentFactory.CreateFormField("Species:", speciesComboBox);

            pairingIdTextBox = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(pairingIdTextBox);
            var pairingIdField = PairingFormComponentFactory.CreateFormField("Pairing ID:", pairingIdTextBox);

            projectComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(projectComboBox);
            var projectField = PairingFormComponentFactory.CreateFormField("Project:", projectComboBox);

            damComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(damComboBox);
            var damField = PairingFormComponentFactory.CreateFormField("Dam (Female):", damComboBox);

            sireComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(sireComboBox);
            var sireField = PairingFormComponentFactory.CreateFormField("Sire (Male):", sireComboBox);

            pairingDatePicker = new DateTimePicker { Format = DateTimePickerFormat.Short };
            var dateField = PairingFormComponentFactory.CreateFormField("Pairing Date:", pairingDatePicker);

            // Create buttons
            addButton = new Button { Text = "Add Pairing" };
            cancelButton = new Button { Text = "Cancel" };
            var buttonPanel = PairingFormComponentFactory.CreateButtonPanel(addButton, cancelButton);

            // Add all controls to the group
            var formPanel = new Panel { Dock = DockStyle.Fill };
            formPanel.Controls.AddRange(new Control[] {
                speciesField, pairingIdField, projectField,
                damField, sireField, dateField, buttonPanel
            });

            pairingGroup.Controls.Add(formPanel);
            mainPanel.Controls.Add(pairingGroup);

            // Add information panel
            var infoPanel = PairingFormComponentFactory.CreateInfoPanel("Information",
                "• Pairing ID should be unique for each pairing\n" +
                "• Ensure the dam and sire are of appropriate age for breeding\n" +
                "• The pairing date will be used to calculate expected litter dates\n" +
                "• You can view all pairings in the Breeding History section");

            mainPanel.Controls.Add(infoPanel);
            tab.Controls.Add(mainPanel);
        }

        private void InitializeMultiplePairingsTab(TabPage tab)
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // Create input group
            var inputGroup = PairingFormComponentFactory.CreateFormSection("New Pairing", DockStyle.Top, 275);

            // Create form fields (similar to single pairing tab)
            var multiPairingIdTextBox = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(multiPairingIdTextBox);
            var pairingIdField = PairingFormComponentFactory.CreateFormField("Pairing ID:", multiPairingIdTextBox);

            var multiProjectComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(multiProjectComboBox);
            var projectField = PairingFormComponentFactory.CreateFormField("Project:", multiProjectComboBox);

            var multiDamComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(multiDamComboBox);
            var damField = PairingFormComponentFactory.CreateFormField("Dam (Female):", multiDamComboBox);

            var multiSireComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(multiSireComboBox);
            var sireField = PairingFormComponentFactory.CreateFormField("Sire (Male):", multiSireComboBox);

            var multiPairingDatePicker = new DateTimePicker { Format = DateTimePickerFormat.Short };
            var dateField = PairingFormComponentFactory.CreateFormField("Pairing Date:", multiPairingDatePicker);

            // Create Add to Grid button
            addToGridButton = new Button { Text = "Add to List" };
            var addToGridPanel = PairingFormComponentFactory.CreateButtonPanel(addToGridButton, null);

            // Add all controls to the group
            var formPanel = new Panel { Dock = DockStyle.Fill };
            formPanel.Controls.AddRange(new Control[] {
                pairingIdField, projectField, damField,
                sireField, dateField, addToGridPanel
            });

            inputGroup.Controls.Add(formPanel);
            mainPanel.Controls.Add(inputGroup);

            // Create grid
            multiplePairingsGrid = PairingFormComponentFactory.CreatePairingsGrid();
            var gridGroup = PairingFormComponentFactory.CreateFormSection("Pairings to Add", DockStyle.Fill, 0);
            gridGroup.Controls.Add(multiplePairingsGrid);

            // Create bottom buttons
            saveAllButton = new Button { Text = "Save All Pairings" };
            importButton = new Button { Text = "Import Data" };
            var cancelMultiButton = new Button { Text = "Cancel" };

            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(0, 10, 0, 0)
            };

            FormStyleHelper.ApplyButtonStyle(saveAllButton, true);
            FormStyleHelper.ApplyButtonStyle(importButton, true);
            FormStyleHelper.ApplyButtonStyle(cancelMultiButton, false);

            saveAllButton.Location = new Point(300, 10);
            importButton.Location = new Point(140, 10);
            cancelMultiButton.Location = new Point(460, 10);

            bottomPanel.Controls.AddRange(new Control[] { saveAllButton, importButton, cancelMultiButton });

            mainPanel.Controls.Add(bottomPanel);
            mainPanel.Controls.Add(gridGroup);
            tab.Controls.Add(mainPanel);
        }

        private void InitializeEventHandlers()
        {
            this.Load += async (s, e) => await _eventHandler.HandleFormLoadAsync(
                speciesComboBox, damComboBox, sireComboBox, projectComboBox);

            speciesComboBox.SelectedIndexChanged += async (s, e) => await _eventHandler.HandleSpeciesSelectionChangedAsync(
                speciesComboBox, damComboBox, sireComboBox);

            addButton.Click += async (s, e) => await _eventHandler.HandleAddPairingClickAsync(
                pairingIdTextBox.Text, damComboBox, sireComboBox, projectComboBox, pairingDatePicker);

            addToGridButton.Click += (s, e) => _eventHandler.HandleAddToGridClick(
                pairingIdTextBox.Text, damComboBox, sireComboBox, projectComboBox, pairingDatePicker, multiplePairingsGrid);

            saveAllButton.Click += async (s, e) => await _eventHandler.HandleSaveAllPairingsAsync(multiplePairingsGrid);

            importButton.Click += (s, e) => MessageBox.Show("TODO - bulk import from excel logic goes here");

            cancelButton.Click += (s, e) => _eventHandler.HandleCancelClick(this);

            multiplePairingsGrid.CellContentClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == multiplePairingsGrid.Columns["Remove"].Index)
                {
                    multiplePairingsGrid.Rows.RemoveAt(e.RowIndex);
                }
            };
        }
    }
}
