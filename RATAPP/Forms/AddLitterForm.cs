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
            this.Size = new Size(1200, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.AutoScroll = true;

            // Create header with description
            var headerPanel = FormComponentFactory.CreateHeaderPanel("Add Litter");
            var descriptionLabel = new Label
            {
                Text = "Create new litters and manage breeding records",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(22, 40)
            };
            headerPanel.Controls.Add(descriptionLabel);
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

            // Create two columns for better organization
            var leftColumn = new Panel
            {
                Dock = DockStyle.Left,
                Width = 550,
                Padding = new Padding(0, 0, 20, 0)
            };

            var rightColumn = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 0, 0, 0)
            };

            // Create groups for related fields
            var basicInfoGroup = FormComponentFactory.CreateFormSection("Basic Information", DockStyle.Top, 280);
            var breedingInfoGroup = FormComponentFactory.CreateFormSection("Breeding Information", DockStyle.Top, 280);
            var litterDetailsGroup = FormComponentFactory.CreateFormSection("Litter Details", DockStyle.Top, 350);

            // Create and configure form fields with validation indicators
            speciesComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(speciesComboBox);
            var speciesField = CreateRequiredFormField("Species:", speciesComboBox);

            litterIdTextBox = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(litterIdTextBox);
            var litterIdField = CreateRequiredFormField("Litter ID:", litterIdTextBox);

            litterNameTextBox = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(litterNameTextBox);
            var litterNameField = CreateRequiredFormField("Litter Name:", litterNameTextBox);

            projectComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(projectComboBox);
            var projectField = CreateRequiredFormField("Project:", projectComboBox);

            damComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(damComboBox);
            var damField = CreateRequiredFormField("Dam (Female):", damComboBox);

            sireComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(sireComboBox);
            var sireField = CreateRequiredFormField("Sire (Male):", sireComboBox);

            pairComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(pairComboBox);
            var pairField = CreateRequiredFormField("Pair:", pairComboBox);

            litterDatePicker = new DateTimePicker { Format = DateTimePickerFormat.Short };
            var dateField = CreateRequiredFormField("Birth Date:", litterDatePicker);

            numPups = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(numPups);
            numPups.KeyPress += (s, e) => ValidateNumericInput(e);
            var numPupsField = CreateRequiredFormField("Number of Pups:", numPups);

            numMales = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(numMales);
            numMales.KeyPress += (s, e) => ValidateNumericInput(e);
            var numMalesField = CreateRequiredFormField("Number of Males:", numMales);

            numFemales = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(numFemales);
            numFemales.KeyPress += (s, e) => ValidateNumericInput(e);
            var numFemalesField = CreateRequiredFormField("Number of Females:", numFemales);

            litterNotes = new TextBox { Multiline = true, Height = 80 };
            FormStyleHelper.ApplyTextBoxStyle(litterNotes, 400);
            var notesField = FormComponentFactory.CreateFormField("Notes:", litterNotes);

            // Organize fields into groups
            var basicInfoPanel = new Panel { Dock = DockStyle.Fill };
            basicInfoPanel.Controls.AddRange(new Control[] {
                speciesField, litterIdField, litterNameField, projectField
            });
            basicInfoGroup.Controls.Add(basicInfoPanel);

            var breedingInfoPanel = new Panel { Dock = DockStyle.Fill };
            breedingInfoPanel.Controls.AddRange(new Control[] {
                damField, sireField, pairField, dateField
            });
            breedingInfoGroup.Controls.Add(breedingInfoPanel);

            var litterDetailsPanel = new Panel { Dock = DockStyle.Fill };
            litterDetailsPanel.Controls.AddRange(new Control[] {
                numPupsField, numMalesField, numFemalesField, notesField
            });
            litterDetailsGroup.Controls.Add(litterDetailsPanel);

            // Create buttons with improved styling
            addButton = new Button { Text = "Add Litter" };
            FormStyleHelper.ApplyButtonStyle(addButton, true);
            addButton.Margin = new Padding(0, 20, 0, 0);

            cancelButton = new Button { Text = "Cancel" };
            FormStyleHelper.ApplyButtonStyle(cancelButton, false);
            cancelButton.Margin = new Padding(10, 20, 0, 0);

            var buttonPanel = FormComponentFactory.CreateButtonPanel(addButton, cancelButton);

            // Organize panels
            leftColumn.Controls.AddRange(new Control[] { basicInfoGroup, breedingInfoGroup });
            rightColumn.Controls.AddRange(new Control[] { litterDetailsGroup });

            mainPanel.Controls.AddRange(new Control[] { leftColumn, rightColumn, buttonPanel });

            // Add enhanced information panel
            var infoPanel = FormComponentFactory.CreateInfoPanel("Important Information",
                "• Litter ID must be unique for each litter (required)\n" +
                "• All fields marked with * are required\n" +
                "• Ensure the dam and sire are of appropriate age for breeding\n" +
                "• The birth date will be used to track litter development\n" +
                "• Numbers must be positive integers\n" +
                "• You can view all litters in the Breeding History section");

            mainPanel.Controls.Add(infoPanel);

            // Set tab order
            SetTabOrder();
            tab.Controls.Add(mainPanel);
        }

        private void InitializeMultipleLittersTab(TabPage tab)
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // Create input group with improved layout
            var inputGroup = FormComponentFactory.CreateFormSection("Add New Litter", DockStyle.Top, 250);
            inputGroup.Margin = new Padding(0, 0, 0, 20);

            // Create form fields with validation indicators
            var multiLitterIdTextBox = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(multiLitterIdTextBox);
            var litterIdField = CreateRequiredFormField("Litter ID:", multiLitterIdTextBox);

            var multiProjectComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(multiProjectComboBox);
            var projectField = CreateRequiredFormField("Project:", multiProjectComboBox);

            var multiDamComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(multiDamComboBox);
            var damField = CreateRequiredFormField("Dam (Female):", multiDamComboBox);

            var multiSireComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(multiSireComboBox);
            var sireField = CreateRequiredFormField("Sire (Male):", multiSireComboBox);

            var multiLitterDatePicker = new DateTimePicker { Format = DateTimePickerFormat.Short };
            var dateField = CreateRequiredFormField("Birth Date:", multiLitterDatePicker);

            // Create Add to Grid button with improved styling
            addToGridButton = new Button { Text = "Add to List" };
            FormStyleHelper.ApplyButtonStyle(addToGridButton, true);
            addToGridButton.Margin = new Padding(0, 10, 0, 0);
            var addToGridPanel = FormComponentFactory.CreateButtonPanel(addToGridButton, null);

            // Organize fields into two columns
            var leftColumn = new Panel
            {
                Dock = DockStyle.Left,
                Width = 400,
                Padding = new Padding(0, 0, 10, 0)
            };

            var rightColumn = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 0, 0, 0)
            };

            leftColumn.Controls.AddRange(new Control[] {
                litterIdField, projectField, damField
            });

            rightColumn.Controls.AddRange(new Control[] {
                sireField, dateField, addToGridPanel
            });

            var formPanel = new Panel { Dock = DockStyle.Fill };
            formPanel.Controls.AddRange(new Control[] { leftColumn, rightColumn });
            inputGroup.Controls.Add(formPanel);

            // Create grid with improved styling
            multipleLittersGrid = FormComponentFactory.CreateDataGrid();
            FormStyleHelper.ApplyDataGridViewStyle(multipleLittersGrid);
            multipleLittersGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            multipleLittersGrid.Margin = new Padding(0, 10, 0, 10);

            var gridGroup = FormComponentFactory.CreateFormSection("Litters to Add", DockStyle.Fill, 0);
            gridGroup.Controls.Add(multipleLittersGrid);

            // Add columns to grid with improved formatting
            multipleLittersGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "LitterID", HeaderText = "Litter ID", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "PairID", HeaderText = "Pair ID", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "BirthDate", HeaderText = "Birth Date", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "NumPups", HeaderText = "# Pups", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "NumMales", HeaderText = "# Males", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "NumFemales", HeaderText = "# Females", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Notes", HeaderText = "Notes", Width = 200 },
                new DataGridViewButtonColumn { 
                    Name = "Remove", 
                    HeaderText = "", 
                    Text = "Remove", 
                    UseColumnTextForButtonValue = true,
                    Width = 80
                }
            });

            // Create bottom buttons with improved styling
            saveAllButton = new Button { Text = "Save All Litters" };
            FormStyleHelper.ApplyButtonStyle(saveAllButton, true);
            
            var cancelMultiButton = new Button { Text = "Cancel" };
            FormStyleHelper.ApplyButtonStyle(cancelMultiButton, false);
            
            var bottomPanel = FormComponentFactory.CreateButtonPanel(saveAllButton, cancelMultiButton);
            bottomPanel.Margin = new Padding(0, 10, 0, 0);

            // Add help text
            var helpText = new Label
            {
                Text = "Add multiple litters by filling in the details above and clicking 'Add to List'. " +
                      "Click 'Save All Litters' when you're ready to save all entries.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(100, 100, 100),
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 10)
            };

            mainPanel.Controls.AddRange(new Control[] {
                inputGroup,
                helpText,
                gridGroup,
                bottomPanel
            });

            tab.Controls.Add(mainPanel);
        }

        private Panel CreateRequiredFormField(string label, Control control)
        {
            var panel = FormComponentFactory.CreateFormField(label + " *", control);
            panel.Margin = new Padding(0, 0, 0, 10);
            return panel;
        }

        private void ValidateNumericInput(KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void SetTabOrder()
        {
            int tabIndex = 0;
            foreach (Control control in this.Controls)
            {
                if (control is ComboBox || control is TextBox || control is DateTimePicker || control is Button)
                {
                    control.TabIndex = tabIndex++;
                }
            }
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
