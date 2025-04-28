using System;
using System.Drawing;
using System.Windows.Forms;
using RATAPP.Helpers;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Services;

namespace RATAPP.Forms
{
    /// <summary>
    /// Form for adding new litters, either single or multiple.
    /// Uses helper classes to manage UI, data, and events.
    /// </summary>
    public partial class AddLitterForm : Form
    {
        // Helper classes
        private readonly FormDataManager _dataManager;
        private readonly FormEventHandler _eventHandler;
        private readonly LoadingSpinnerHelper _spinner;

        // UI Controls
        private TabControl tabControl;
        private ComboBox damComboBox;
        private ComboBox sireComboBox;
        private ComboBox speciesComboBox;
        private TextBox litterIdTextBox;
        private TextBox litterNameTextBox;
        private TextBox numPups;
        private TextBox numMales;
        private TextBox numFemales;
        private TextBox litterNotes;
        private ComboBox projectComboBox;
        private DateTimePicker litterDatePicker;
        private ComboBox pairComboBox;
        private Button addButton;
        private Button cancelButton;
        private DataGridView multipleLittersGrid;
        private Button addToGridButton;
        private Button saveAllButton;

        private AddLitterForm(RATAPPLibrary.Data.DbContexts.RatAppDbContext context)
        {
            // Initialize services
            var breedingService = new BreedingService(context);
            var speciesService = new SpeciesService(context);
            var animalService = new AnimalService(context);
            var projectService = new ProjectService(context);

            // Initialize helper classes
            _dataManager = new FormDataManager(
                breedingService,
                speciesService,
                projectService,
                animalService);

            _spinner = new LoadingSpinnerHelper(this, "C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\Loading_2.gif");
            _eventHandler = new FormEventHandler(_dataManager, _spinner);

            InitializeComponents();
            InitializeEventHandlers();
        }

        /// <summary>
        /// Static factory method that initializes the form and ensures the data is loaded.
        /// Use like: var form = await AddLitterForm.CreateAsync(context);
        /// </summary>
        public static async Task<AddLitterForm> CreateAsync(RATAPPLibrary.Data.DbContexts.RatAppDbContext context)
        {
            var form = new AddLitterForm(context);
            await form._eventHandler.HandleFormLoadAsync(
                form.speciesComboBox, form.damComboBox, form.sireComboBox, form.projectComboBox);
            return form;
        }

        private void InitializeComponents()
        {
            // Set form properties
            this.Text = "Add Litter";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Create header
            var headerPanel = FormComponentFactory.CreateHeaderPanel("Add Litter");
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
            var singleLitterTab = FormComponentFactory.CreateTabPage("Single Litter");
            InitializeSingleLitterTab(singleLitterTab);
            tabControl.TabPages.Add(singleLitterTab);

            var multipleLittersTab = FormComponentFactory.CreateTabPage("Multiple Litters");
            InitializeMultipleLittersTab(multipleLittersTab);
            tabControl.TabPages.Add(multipleLittersTab);

            tabContainerPanel.Controls.Add(tabControl);
            this.Controls.Add(tabContainerPanel);
        }

        private void InitializeSingleLitterTab(TabPage tab)
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // Create litter information group
            var litterGroup = FormComponentFactory.CreateFormSection("Litter Information", DockStyle.Top, 400);

            // Create form fields
            speciesComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(speciesComboBox);
            var speciesField = FormComponentFactory.CreateFormField("Species:", speciesComboBox);

            litterIdTextBox = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(litterIdTextBox);
            var litterIdField = FormComponentFactory.CreateFormField("Litter ID:", litterIdTextBox);

            litterNameTextBox = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(litterNameTextBox);
            var litterNameField = FormComponentFactory.CreateFormField("Litter Name:", litterNameTextBox);

            projectComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(projectComboBox);
            var projectField = FormComponentFactory.CreateFormField("Project:", projectComboBox);

            damComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(damComboBox);
            var damField = FormComponentFactory.CreateFormField("Dam (Female):", damComboBox);

            sireComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(sireComboBox);
            var sireField = FormComponentFactory.CreateFormField("Sire (Male):", sireComboBox);

            pairComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(pairComboBox);
            var pairField = FormComponentFactory.CreateFormField("Pair:", pairComboBox);

            litterDatePicker = new DateTimePicker { Format = DateTimePickerFormat.Short };
            var dateField = FormComponentFactory.CreateFormField("Birth Date:", litterDatePicker);

            numPups = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(numPups);
            var numPupsField = FormComponentFactory.CreateFormField("Number of Pups:", numPups);

            numMales = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(numMales);
            var numMalesField = FormComponentFactory.CreateFormField("Number of Males:", numMales);

            numFemales = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(numFemales);
            var numFemalesField = FormComponentFactory.CreateFormField("Number of Females:", numFemales);

            litterNotes = new TextBox { Multiline = true, Height = 60 };
            FormStyleHelper.ApplyTextBoxStyle(litterNotes);
            var notesField = FormComponentFactory.CreateFormField("Notes:", litterNotes);

            // Create buttons
            addButton = new Button { Text = "Add Litter" };
            cancelButton = new Button { Text = "Cancel" };
            var buttonPanel = FormComponentFactory.CreateButtonPanel(addButton, cancelButton);

            // Add all controls to the group
            var formPanel = new Panel { Dock = DockStyle.Fill };
            formPanel.Controls.AddRange(new Control[] {
                speciesField, litterIdField, litterNameField, projectField,
                damField, sireField, pairField, dateField,
                numPupsField, numMalesField, numFemalesField, notesField,
                buttonPanel
            });

            litterGroup.Controls.Add(formPanel);
            mainPanel.Controls.Add(litterGroup);

            // Add information panel
            var infoPanel = FormComponentFactory.CreateInfoPanel("Information",
                "• Litter ID should be unique for each litter\n" +
                "• Ensure the dam and sire are of appropriate age for breeding\n" +
                "• The birth date will be used to track litter development\n" +
                "• You can view all litters in the Breeding History section");

            mainPanel.Controls.Add(infoPanel);
            tab.Controls.Add(mainPanel);
        }

        private void InitializeMultipleLittersTab(TabPage tab)
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // Create input group
            var inputGroup = FormComponentFactory.CreateFormSection("New Litter", DockStyle.Top, 275);

            // Create form fields (similar to single litter tab)
            var multiLitterIdTextBox = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(multiLitterIdTextBox);
            var litterIdField = FormComponentFactory.CreateFormField("Litter ID:", multiLitterIdTextBox);

            var multiProjectComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(multiProjectComboBox);
            var projectField = FormComponentFactory.CreateFormField("Project:", multiProjectComboBox);

            var multiDamComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(multiDamComboBox);
            var damField = FormComponentFactory.CreateFormField("Dam (Female):", multiDamComboBox);

            var multiSireComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(multiSireComboBox);
            var sireField = FormComponentFactory.CreateFormField("Sire (Male):", multiSireComboBox);

            var multiLitterDatePicker = new DateTimePicker { Format = DateTimePickerFormat.Short };
            var dateField = FormComponentFactory.CreateFormField("Birth Date:", multiLitterDatePicker);

            // Create Add to Grid button
            addToGridButton = new Button { Text = "Add to List" };
            var addToGridPanel = FormComponentFactory.CreateButtonPanel(addToGridButton, null);

            // Add all controls to the group
            var formPanel = new Panel { Dock = DockStyle.Fill };
            formPanel.Controls.AddRange(new Control[] {
                litterIdField, projectField, damField,
                sireField, dateField, addToGridPanel
            });

            inputGroup.Controls.Add(formPanel);
            mainPanel.Controls.Add(inputGroup);

            // Create grid
            multipleLittersGrid = FormComponentFactory.CreateDataGrid();
            var gridGroup = FormComponentFactory.CreateFormSection("Litters to Add", DockStyle.Fill, 0);
            gridGroup.Controls.Add(multipleLittersGrid);

            // Add columns to grid
            multipleLittersGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "LitterID", HeaderText = "Litter ID" },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name" },
                new DataGridViewTextBoxColumn { Name = "PairID", HeaderText = "Pair ID" },
                new DataGridViewTextBoxColumn { Name = "BirthDate", HeaderText = "Birth Date" },
                new DataGridViewTextBoxColumn { Name = "NumPups", HeaderText = "# Pups" },
                new DataGridViewTextBoxColumn { Name = "NumMales", HeaderText = "# Males" },
                new DataGridViewTextBoxColumn { Name = "NumFemales", HeaderText = "# Females" },
                new DataGridViewTextBoxColumn { Name = "Notes", HeaderText = "Notes" },
                new DataGridViewButtonColumn { Name = "Remove", HeaderText = "Remove", Text = "Remove", UseColumnTextForButtonValue = true }
            });

            // Create bottom buttons
            saveAllButton = new Button { Text = "Save All Litters" };
            var cancelMultiButton = new Button { Text = "Cancel" };
            var bottomPanel = FormComponentFactory.CreateButtonPanel(saveAllButton, cancelMultiButton);

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

            addButton.Click += async (s, e) => await _eventHandler.HandleAddLitterClickAsync(
                litterIdTextBox.Text,
                litterNameTextBox.Text,
                pairComboBox.SelectedIndex + 1,
                litterDatePicker,
                numPups,
                numMales,
                numFemales,
                litterNotes);

            addToGridButton.Click += (s, e) => _eventHandler.HandleAddLitterToGridClick(
                litterIdTextBox.Text,
                litterNameTextBox.Text,
                pairComboBox.SelectedIndex + 1,
                litterDatePicker,
                numPups,
                numMales,
                numFemales,
                litterNotes,
                multipleLittersGrid);

            saveAllButton.Click += async (s, e) => await _eventHandler.HandleSaveAllLittersAsync(
                multipleLittersGrid);

            cancelButton.Click += (s, e) => _eventHandler.HandleCancelClick(this);

            multipleLittersGrid.CellContentClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == multipleLittersGrid.Columns["Remove"].Index)
                {
                    multipleLittersGrid.Rows.RemoveAt(e.RowIndex);
                }
            };
        }
    }
}
