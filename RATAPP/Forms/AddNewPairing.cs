using System;
using System.Drawing;
using System.Windows.Forms;
using RATAPP.Helpers;
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
        private readonly FormDataManager _dataManager;
        private readonly FormEventHandler _eventHandler;
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

        private void InitializeComponents()
        {
            // Set form properties
            this.Text = "Add Pairing";
            this.Size = new Size(1200, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.AutoScroll = true;

            // Create header with description
            var headerPanel = FormComponentFactory.CreateHeaderPanel("Add Pairing");
            var descriptionLabel = new Label
            {
                Text = "Create new breeding pairs and manage pairing records",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(22, 40)
            };

            // Calculate the X position to place descriptionLabel to the right of headerLabel
            int descriptionLabelX = headerLabel.Location.X + headerLabel.Width + 10;
            // Calculate the Y position to vertically align descriptionLabel with headerLabel
            int descriptionLabelY = headerLabel.Location.Y;

            descriptionLabel.Location = new Point(descriptionLabelX, descriptionLabelY);
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
            var singlePairingTab = FormComponentFactory.CreateTabPage("Single Pairing");
            InitializeSinglePairingTab(singlePairingTab);
            tabControl.TabPages.Add(singlePairingTab);

            var multiplePairingsTab = FormComponentFactory.CreateTabPage("Multiple Pairings");
            InitializeMultiplePairingsTab(multiplePairingsTab);
            tabControl.TabPages.Add(multiplePairingsTab);

            tabContainerPanel.Controls.Add(tabControl);
            this.Controls.Add(tabContainerPanel);
        }

        private void InitializeSinglePairingTab(TabPage tab)
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // Create two columns for better organization
            var leftColumn = new Panel
            {
                Dock = DockStyle.Left,
                Width = 550,
                Padding = new Padding(0, 0, 10, 0)
            };

            var rightColumn = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 0, 0, 0)
            };

            // Create groups for related fields
            var basicInfoGroup = FormComponentFactory.CreateFormSection("Basic Information", DockStyle.Top, 250);
            var breedingInfoGroup = FormComponentFactory.CreateFormSection("Breeding Information", DockStyle.Top, 250);

            // Create and configure form fields with validation indicators
            speciesComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(speciesComboBox);
            var speciesField = CreateRequiredFormField("Species:", speciesComboBox);

            pairingIdTextBox = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(pairingIdTextBox);
            var pairingIdField = CreateRequiredFormField("Pairing ID:", pairingIdTextBox);

            projectComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(projectComboBox);
            var projectField = CreateRequiredFormField("Project:", projectComboBox);

            damComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(damComboBox);
            var damField = CreateRequiredFormField("Dam (Female):", damComboBox);

            sireComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(sireComboBox);
            var sireField = CreateRequiredFormField("Sire (Male):", sireComboBox);

            pairingDatePicker = new DateTimePicker { Format = DateTimePickerFormat.Short };
            var dateField = CreateRequiredFormField("Pairing Date:", pairingDatePicker);

            // Organize fields into groups
            var basicInfoPanel = new Panel { Dock = DockStyle.Fill };
            basicInfoPanel.Controls.AddRange(new Control[] {
                speciesField, pairingIdField, projectField
            });
            basicInfoGroup.Controls.Add(basicInfoPanel);

            var breedingInfoPanel = new Panel { Dock = DockStyle.Fill };
            breedingInfoPanel.Controls.AddRange(new Control[] {
                damField, sireField, dateField
            });
            breedingInfoGroup.Controls.Add(breedingInfoPanel);

            // Create buttons with improved styling
            addButton = new Button { Text = "Add Pairing" };
            FormStyleHelper.ApplyButtonStyle(addButton, true);
            addButton.Margin = new Padding(0, 20, 0, 0);

            cancelButton = new Button { Text = "Cancel" };
            FormStyleHelper.ApplyButtonStyle(cancelButton, false);
            cancelButton.Margin = new Padding(10, 20, 0, 0);

            var buttonPanel = FormComponentFactory.CreateButtonPanel(addButton, cancelButton);

            // Organize panels
            leftColumn.Controls.Add(basicInfoGroup);
            rightColumn.Controls.Add(breedingInfoGroup);

            mainPanel.Controls.AddRange(new Control[] { leftColumn, rightColumn, buttonPanel });

            // Add enhanced information panel
            var infoPanel = FormComponentFactory.CreateInfoPanel("Important Information",
                "• Pairing ID must be unique for each pairing (required)\n" +
                "• All fields marked with * are required\n" +
                "• Ensure the dam and sire are of appropriate age for breeding\n" +
                "• The pairing date will be used to calculate expected litter dates\n" +
                "• Dam and sire must be of the same species\n" +
                "• You can view all pairings in the Breeding History section");

            mainPanel.Controls.Add(infoPanel);

            // Set tab order
            SetTabOrder();
            tab.Controls.Add(mainPanel);
        }

        private void InitializeMultiplePairingsTab(TabPage tab)
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // Create input group with improved layout
            var inputGroup = FormComponentFactory.CreateFormSection("Add New Pairing", DockStyle.Top, 250);
            inputGroup.Margin = new Padding(0, 0, 0, 20);

            // Create form fields with validation indicators
            var multiPairingIdTextBox = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(multiPairingIdTextBox);
            var pairingIdField = CreateRequiredFormField("Pairing ID:", multiPairingIdTextBox);

            var multiProjectComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(multiProjectComboBox);
            var projectField = CreateRequiredFormField("Project:", multiProjectComboBox);

            var multiDamComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(multiDamComboBox);
            var damField = CreateRequiredFormField("Dam (Female):", multiDamComboBox);

            var multiSireComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(multiSireComboBox);
            var sireField = CreateRequiredFormField("Sire (Male):", multiSireComboBox);

            var multiPairingDatePicker = new DateTimePicker { Format = DateTimePickerFormat.Short };
            var dateField = CreateRequiredFormField("Pairing Date:", multiPairingDatePicker);

            // Create Add to Grid button with improved styling
            addToGridButton = new Button { Text = "Add to List" };
            FormStyleHelper.ApplyButtonStyle(addToGridButton, true);
            addToGridButton.Margin = new Padding(0, 10, 0, 0);
            var addToGridPanel = FormComponentFactory.CreateButtonPanel(addToGridButton, null);

            // Organize fields into two columns
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

            leftColumn.Controls.AddRange(new Control[] {
                pairingIdField, projectField, damField
            });

            rightColumn.Controls.AddRange(new Control[] {
                sireField, dateField, addToGridPanel
            });

            var formPanel = new Panel { Dock = DockStyle.Fill };
            formPanel.Controls.AddRange(new Control[] { leftColumn, rightColumn });
            inputGroup.Controls.Add(formPanel);

            // Create grid with improved styling
            multiplePairingsGrid = FormComponentFactory.CreateDataGrid();
            FormStyleHelper.ApplyDataGridViewStyle(multiplePairingsGrid);
            multiplePairingsGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            multiplePairingsGrid.Margin = new Padding(0, 10, 0, 10);

            var gridGroup = FormComponentFactory.CreateFormSection("Pairings to Add", DockStyle.Fill, 0);
            gridGroup.Controls.Add(multiplePairingsGrid);

            // Add columns to grid with improved formatting
            multiplePairingsGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Dam", HeaderText = "Dam", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "Sire", HeaderText = "Sire", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "PairingID", HeaderText = "Pairing ID", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Project", HeaderText = "Project", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "PairingDate", HeaderText = "Pairing Date", Width = 100 },
                new DataGridViewButtonColumn { 
                    Name = "Remove", 
                    HeaderText = "", 
                    Text = "Remove", 
                    UseColumnTextForButtonValue = true,
                    Width = 80
                }
            });

            // Create bottom buttons with improved styling
            saveAllButton = new Button { Text = "Save All Pairings" };
            FormStyleHelper.ApplyButtonStyle(saveAllButton, true);
            
            importButton = new Button { Text = "Import Data" };
            FormStyleHelper.ApplyButtonStyle(importButton, true);
            
            var cancelMultiButton = new Button { Text = "Cancel" };
            FormStyleHelper.ApplyButtonStyle(cancelMultiButton, false);
            
            var bottomPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0)
            };
            
            bottomPanel.Controls.AddRange(new Control[] { saveAllButton, importButton, cancelMultiButton });
            foreach (Control button in bottomPanel.Controls)
            {
                button.Margin = new Padding(0, 0, 10, 0);
            }

            // Add help text
            var helpText = new Label
            {
                Text = "Add multiple pairings by filling in the details above and clicking 'Add to List'. " +
                      "Click 'Save All Pairings' when you're ready to save all entries. Use 'Import Data' to bulk import from Excel.",
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

            addButton.Click += async (s, e) => await _eventHandler.HandleAddPairingClickAsync(
                pairingIdTextBox.Text, damComboBox, sireComboBox, projectComboBox, pairingDatePicker);

            addToGridButton.Click += (s, e) => _eventHandler.HandleAddPairingToGridClick(
                pairingIdTextBox.Text, damComboBox, sireComboBox, projectComboBox, pairingDatePicker, multiplePairingsGrid);

            saveAllButton.Click += async (s, e) => await _eventHandler.HandleSaveAllPairingsAsync(
                multiplePairingsGrid, damComboBox, sireComboBox, projectComboBox);

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
