using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using RATAPP.Helpers;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Services;

namespace RATAPP.Forms
{
    /// <summary>
    /// Form for adding new litters in the R.A.T. App.
    /// Provides single and batch litter creation with validation.
    /// 
    /// Key Features:
    /// - Single Litter Entry:
    ///   * Basic litter information
    ///   * Breeding pair selection
    ///   * Pup count tracking
    ///   * Notes and details
    /// 
    /// - Multiple Litter Entry:
    ///   * Batch creation interface
    ///   * Data grid for review
    ///   * Bulk save operations
    /// 
    /// Data Validation:
    /// - Required field checking
    /// - Numeric input validation
    /// - Date range verification
    /// - Duplicate ID prevention
    /// 
    /// UI Components:
    /// - Tabbed interface
    /// - Form validation
    /// - Loading indicators
    /// - Error handling
    /// 
    /// Known Limitations:
    /// - Basic error handling
    /// - Limited bulk operations
    /// - Fixed validation rules
    /// - Some TODO implementations
    /// 
    /// Dependencies:
    /// - BreedingService
    /// - AnimalService
    /// - ProjectService
    /// - Form helper classes
    /// </summary>
    public partial class AddLitterForm : Form
    {
        // Helper classes
        private readonly FormDataManager _dataManager;
        private readonly FormEventHandler _eventHandler;
        private readonly LoadingSpinnerHelper _spinner;
        private readonly RatAppDbContextFactory _contextFactory;

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

        //services
        private BreedingService _breedingService;
        private SpeciesService _speciesService;
        private AnimalService _animalService;
        private ProjectService _projectService;

        //state
        bool pairSelected = false; 

        private AddLitterForm(RatAppDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;

            // Initialize services with context factory
            _breedingService = new BreedingService(contextFactory);
            _speciesService = new SpeciesService(contextFactory);
            _animalService = new AnimalService(contextFactory);
            _projectService = new ProjectService(contextFactory);

            _spinner = new LoadingSpinnerHelper(this, "C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\Loading_2.gif");

            InitializeComponents();
            InitializeEventHandlers();
        }

        /// <summary>
        /// Static factory method that initializes the form and ensures the data is loaded.
        /// Use like: var form = await AddLitterForm.CreateAsync(context);
        /// </summary>
        /// <summary>
        /// Static factory method that initializes the form and loads required data.
        /// Uses async initialization pattern for proper data loading.
        /// 
        /// Process:
        /// 1. Creates form instance
        /// 2. Loads species data
        /// 3. Loads animal records
        /// 4. Loads active pairs
        /// 5. Loads projects
        /// 
        /// Error Handling:
        /// - Shows loading indicator
        /// - Manages async failures
        /// - Provides user feedback
        /// 
        /// Usage:
        /// var form = await AddLitterForm.CreateAsync(context);
        /// </summary>
        public static async Task<AddLitterForm> CreateAsync(RatAppDbContextFactory contextFactory)
        {
            var form = new AddLitterForm(contextFactory);
            await form.LoadInitialDataAsync();
            return form;
        }

        /// <summary>
        /// Loads all required data for the form asynchronously.
        /// Shows loading indicator during data fetch operations.
        /// 
        /// Data Loaded:
        /// - Species list
        /// - Available animals
        /// - Active breeding pairs
        /// - Current projects
        /// 
        /// Dependencies:
        /// - Species service
        /// - Animal service
        /// - Breeding service
        /// - Project service
        /// 
        /// Error Handling:
        /// - Manages service exceptions
        /// - Shows loading state
        /// - Ensures UI responsiveness
        /// </summary>
        private async Task LoadInitialDataAsync()
        {
            try
            {
                _spinner.Show();
                await LoadSpecies();
                await LoadAnimals();
                await LoadPairs();
                await LoadProjects();
            }
            finally
            {
                _spinner.Hide();
            }
        }

        private void InitializeComponents()
        {
            //Properties
            this.Text = "Add Litter";
            this.Size = new Size(1200, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.AutoScroll = true;

            // Header with description
            var headerPanel = FormComponentFactory.CreateHeaderPanel("Add Litter");
            headerPanel.Height = 50;
            var headerLabel = headerPanel.Controls[0] as Label;
            if (headerLabel != null)
            {
                headerLabel.Location = new Point(25, 10);
            }
            var descriptionLabel = new Label
            {
                Text = "Create new litters and manage breeding records",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                AutoSize = true,
            };

            // Calculate the X position to place descriptionLabel to the right of headerLabel
            int descriptionLabelX = headerLabel.Location.X + headerLabel.Width + 10; // Add some spacing (10)
                                                                                     // Calculate the Y position to vertically align descriptionLabel with headerLabel
            int descriptionLabelY = headerLabel.Location.Y;

            descriptionLabel.Location = new Point(descriptionLabelX, descriptionLabelY);

            headerPanel.Controls.Add(descriptionLabel); // Add the descriptionLabel to the headerPanel

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

        //this is the original way that I was managing data, but I have moved to using my helpers for clarity + to prevent my controller classes from getting too large
        private async void LitterPanel_Load(object sender, EventArgs e)
        {
            await LoadInitialDataAsync();
        }

        //What data do I need before anything else happens?
        //Animals
        //bind animal data to combo box options 
        private async Task LoadAnimals()
        {
            try
            {
                var animals = await _animalService.GetAllAnimalsAsync();

                // Populate dam and sire combos for breeding calculator
                damComboBox.Items.Clear();
                sireComboBox.Items.Clear();
                //animalSelector.Items.Clear();

                // Add all animals to the pedigree selector
                foreach (var animal in animals)
                {
                    //animalSelector.Items.Add(animal); TODO not sure if I need this for this form 

                    // Add females to dam combo
                    if (animal.sex == "Female")
                    {
                        damComboBox.Items.Add(animal);
                    }
                    // Add males to sire combo
                    else if (animal.sex == "Male")
                    {
                        sireComboBox.Items.Add(animal);
                    }
                }

                damComboBox.DisplayMember = "Name"; //display member is what the user sees 
                damComboBox.ValueMember = "Id"; //value member is what we use when we're trying to work with the obect i.e. how the backend identifies the object 
                sireComboBox.DisplayMember = "Name";
                sireComboBox.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading animals: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //species
        //load species objects to species combo box items 
        private async Task LoadSpecies()
        {
            try
            {
                var species = await _speciesService.GetAllSpeciesAsync();

                speciesComboBox.Items.Clear();
                speciesComboBox.Items.Add("All Species");

                foreach (var s in species)
                {
                    speciesComboBox.Items.Add(s);
                }

                speciesComboBox.SelectedIndex = 0;
                speciesComboBox.DisplayMember = "CommonName";
                speciesComboBox.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading species: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadPairs()
        {
            try
            {
                var pairs = await _breedingService.GetAllActivePairingsAsync();

                pairComboBox.Items.Clear();
                pairComboBox.Items.Add("All Pairings");

                foreach (var p in pairs)
                {
                    pairComboBox.Items.Add(p);
                }

                pairComboBox.SelectedIndex = 0;
                pairComboBox.DisplayMember = "pairingId"; //FIXME I may need a way to make it easier for user to distinguish pairs i.e. dam+sire+date? 
                pairComboBox.ValueMember = "pairingId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading pairings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadProjects()
        {
            try
            {
                var projects = await _projectService.GetAllProjectsAsync();

                projectComboBox.Items.Clear();
                projectComboBox.Items.Add("All Species");

                foreach (var p in projects)
                {
                    projectComboBox.Items.Add(p);
                }

                projectComboBox.SelectedIndex = 0;
                projectComboBox.DisplayMember = "Name";
                projectComboBox.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading projects: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            var basicInfoGroup = FormComponentFactory.CreateFormSection("Basic Information", DockStyle.Top, 250);
            var breedingInfoGroup = FormComponentFactory.CreateFormSection("Breeding Information", DockStyle.Top, 250);
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
            buttonPanel.Dock = DockStyle.Bottom; // Dock the button panel to the bottom

            // Organize panels
            leftColumn.Controls.AddRange(new Control[] { basicInfoGroup, breedingInfoGroup });
            rightColumn.Controls.AddRange(new Control[] { litterDetailsGroup });

            mainPanel.Controls.AddRange(new Control[] { leftColumn, rightColumn });

            // Add enhanced information panel
            var infoPanel = FormComponentFactory.CreateInfoPanel("Important Information",
                "• Litter ID must be unique for each litter (required)\n" +
                "• All fields marked with * are required\n" +
                "• Ensure the dam and sire are of appropriate age for breeding\n" +
                "• The birth date will be used to track litter development\n" +
                "• Numbers must be positive integers\n" +
                "• You can view all litters in the Breeding History section");

            mainPanel.Controls.Add(infoPanel);
            mainPanel.Controls.Add(buttonPanel); // Add buttonPanel *after* infoPanel

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

        //species selected index changed
        //only show pairs, dams and sires for appropriate species 
        //private async 

        //sire combo box changed
        //set pairs accordingly 
        public async Task HandleSireSelectionChangedAsync()
        {
            if (sireComboBox.SelectedItem == null) return;
            if (pairSelected == true) return;

            try
            {
                _spinner.Show();
                var selectedSire = sireComboBox.SelectedItem as AnimalDto;

                // Clear existing items
                pairComboBox.Items.Clear();

                //check if the sire combo box is populated
                if (damComboBox.SelectedItem != null)
                {
                    var selectedDam = damComboBox.SelectedItem as AnimalDto;

                    var pairs = await _breedingService.GetAllActivePairingsByDamandSireIdAsync(selectedDam.Id, selectedSire.Id);


                    foreach (var pair in pairs) //FIXME IDs don't really make sense it should find the dam and sire based on pair or find the pair based on dam and sire i.e. it should be filtering the pairs based on dam and sire if they exist 
                    {
                        pairComboBox.Items.Add(pair);
                    }

                }
                else //dam isn't populated so just get the pairs for the sire
                {
                    // Load active pairings for selected dam
                    var pairs = await _breedingService.GetAllActivePairingsByAnimalIdAsync(selectedSire.Id);

                    foreach (var pair in pairs) //FIXME IDs don't really make sense it should find the dam and sire based on pair or find the pair based on dam and sire i.e. it should be filtering the pairs based on dam and sire if they exist 
                    {
                        pairComboBox.Items.Add(pair);
                    }
                }
            }
            finally
            {
                pairComboBox.SelectedIndex = 0;
                pairComboBox.DisplayMember = "pairId"; //display member is what the user sees 
                pairComboBox.ValueMember = "pairId"; //value member is what we use when we're trying to work with the obect i.e. how the backend identifies the object 
                _spinner.Hide();
            }
        }

        //dam combo box changed
        //set pairs accordingly 
        public async Task HandleDamSelectionChangedAsync()
        {
            if (damComboBox.SelectedItem == null) return;
            if (pairSelected == true) return;

            try
            {
                _spinner.Show();
                var selectedDam = damComboBox.SelectedItem as AnimalDto;

                // Clear existing items
                pairComboBox.Items.Clear();

                //check if the dam combo box is populated
                if (sireComboBox.SelectedItem != null)
                {
                    var selectedSire = sireComboBox.SelectedItem as AnimalDto;

                    var pairs = await _breedingService.GetAllActivePairingsByDamandSireIdAsync(selectedDam.Id, selectedSire.Id);


                    foreach (var pair in pairs) //FIXME IDs don't really make sense it should find the dam and sire based on pair or find the pair based on dam and sire i.e. it should be filtering the pairs based on dam and sire if they exist 
                    {
                        pairComboBox.Items.Add(pair);
                    }

                }
                else //sire isn't populated so just get the pairs for the dam
                {
                    // Load active pairings for selected dam
                    var pairs = await _breedingService.GetAllActivePairingsByAnimalIdAsync(selectedDam.Id);

                    foreach (var pair in pairs) //FIXME IDs don't really make sense it should find the dam and sire based on pair or find the pair based on dam and sire i.e. it should be filtering the pairs based on dam and sire if they exist 
                    {
                        pairComboBox.Items.Add(pair);
                    }
                }
            }
            finally
            {
                _spinner.Hide();
            }
        }

        public async Task HandlePairSelectionChangedAsync()
        {
            var selectedPair = pairComboBox.SelectedItem as Pairing;
            if (selectedPair == null) return; //when first loaded, the object will be null
            if (pairComboBox.SelectedItem == null) return; //but the initial value will not be 

            try
            {
                _spinner.Show();

                if (selectedPair.Dam != null)
                {
                    pairSelected = true; 
                    // Clear existing items
                    damComboBox.Items.Clear();
                    sireComboBox.Items.Clear();
                    projectComboBox.Items.Clear();
                    speciesComboBox.Items.Clear(); 

                    var dam = selectedPair.Dam;
                    var sire = selectedPair.Sire;
                    
                    var project = selectedPair.Project;

                    if (dam != null) {
                        damComboBox.Items.Add(dam);
                 
                    }
                    if (sire != null) {
                        sireComboBox.Items.Add(sire);
                    }
                    if (project != null) {
                        projectComboBox.Items.Add(project); 
                    }
                    if (project != null)
                    {
                        var _stockService = new StockService(_contextFactory);
                        int stockId = 1;
                        if (project.Id == 2)
                        {
                            stockId = 2;
                           
                        }
                        var stockObj = await _stockService.GetStockAsync_ById(stockId);
                        var stock = stockObj.Species;
                        if (stock != null)
                        {
                            speciesComboBox.Items.Add(stock);
                            speciesComboBox.SelectedIndex = 0;
                            speciesComboBox.ValueMember = "Id";
                            speciesComboBox.DisplayMember = "CommonName";
                        }
                        //TODO why is line and stock coming back as empty when the data does exist? 
                        //int stockId = project.Line.StockId; FIXME just patching this to work until I figure out what's wrong with line and other objects that are supposed to exist  this was working before the re-factor
                        //var _stockService = new StockService(_contextFactory);
                        //var stockObj = await _stockService.GetStockAsync_ById(stockId);
                        //var stock = stockObj.Species; 
                        //if (stock != null) {
                        //    speciesComboBox.Items.Add(stock);
                        //    speciesComboBox.SelectedIndex = 0;
                        //    speciesComboBox.ValueMember = "Id";
                        //    speciesComboBox.DisplayMember = "CommonName";
                        //}
                        //if (dam.Line.Stock != null) FIXME have to figure out why the Stock object is always null here but putting a bandaid fix for now since I do have stockID
                        //{
                        //    if (dam.Line.Stock.Species != null)
                        //    {
                        //        speciesComboBox.Items.Add(dam.Line.Stock.Species);
                        //    }
                        //}
                    }
                    damComboBox.SelectedIndex = 0; //FIXME is all of this needed? 
                    sireComboBox.SelectedIndex = 0;
                    projectComboBox.SelectedIndex = 0;
                    
                    damComboBox.DisplayMember = "Name"; //display member is what the user sees 
                    damComboBox.ValueMember = "Id"; //value member is what we use when we're trying to work with the object i.e. how the backend identifies the object 
                    sireComboBox.DisplayMember = "Name";
                    sireComboBox.ValueMember = "Id";
                    projectComboBox.DisplayMember = "Name";
                    projectComboBox.ValueMember = "Id";
                    
                }
            }
            finally
            {
                _spinner.Hide();
                //pairSelected = false; // put flag back to false so that it auto swaps after this is complete 
            }
        }

        public async Task AddLitterClick(string litterId, string litterName, int pairId,
          DateTimePicker datePicker, TextBox numPups, TextBox numMales, TextBox numFemales, TextBox notes)
        {
            try
            {
                _spinner.Show();

                // Validate inputs
                if (string.IsNullOrWhiteSpace(litterId) || string.IsNullOrWhiteSpace(litterName))
                {
                    //MessageBox.Show("Please fill in all required fields", "Validation Error",
                    //    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var litter = new Litter
                {
                    LitterId = litterId,
                    Name = litterName,
                    PairId = pairId,
                    DateOfBirth = datePicker.Value,
                    NumPups = !string.IsNullOrWhiteSpace(numPups.Text) ? int.Parse(numPups.Text) : null,
                    NumMale = !string.IsNullOrWhiteSpace(numMales.Text) ? int.Parse(numMales.Text) : null,
                    NumFemale = !string.IsNullOrWhiteSpace(numFemales.Text) ? int.Parse(numFemales.Text) : null,
                    Notes = notes.Text,
                    CreatedOn = DateTime.Now,
                    LastUpdated = DateTime.Now
                };

                bool litterCreated = await _breedingService.CreateLitterAsync(litter);

                if (litterCreated)
                {
                    MessageBox.Show("Litter added successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("An error occurred. Litter not created!", "Failure",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                
            }
            finally
            {
                _spinner.Hide();
            }
        }

        //cancel = close form 
        public void HandleCancelClick(Form form)
        {
            if (MessageBox.Show("Are you sure you want to cancel? Any unsaved data will be lost.",
                "Confirm Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                form.Close();
            }
        }


        public void HandleAddLitterToGridClick(string litterId, string litterName, int pairId,
            DateTimePicker datePicker, TextBox numPups, TextBox numMales, TextBox numFemales, TextBox notes,
            DataGridView grid)
        {
            try
            {
                _spinner.Show();

                // Validate inputs
                if (string.IsNullOrWhiteSpace(litterId) || string.IsNullOrWhiteSpace(litterName))
                {
                    MessageBox.Show("Please fill in all required fields", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                grid.Rows.Add(
                    litterId,
                    litterName,
                    pairId.ToString(),
                    datePicker.Value.ToShortDateString(),
                    numPups.Text,
                    numMales.Text,
                    numFemales.Text,
                    notes.Text
                );
            }
            finally
            {
                _spinner.Hide();
            }
        }


        //FIXME below is how I was previously doing it 
        private void InitializeEventHandlers()
        {
            // Remove LitterPanel_Load since we're using LoadInitialDataAsync
            addButton.Click += (s,e) => AddLitterClick(
                        litterIdTextBox.Text,
                        litterNameTextBox.Text,
                        pairComboBox.SelectedIndex + 1,
                        litterDatePicker,
                        numPups,
                        numMales,
                        numFemales,
                        litterNotes);

            addToGridButton.Click += (s, e) => HandleAddLitterToGridClick(
                litterIdTextBox.Text,
                litterNameTextBox.Text,
                pairComboBox.SelectedIndex + 1,
                litterDatePicker,
                numPups,
                numMales,
                numFemales,
                litterNotes,
                multipleLittersGrid);

            //saveAllButton.Click += async (s, e) => await HandleSaveAllLittersAsync(
            //    multipleLittersGrid); TODO this is for adding multiple litters which I am not doing yet 

            cancelButton.Click += (s, e) => HandleCancelClick(this);

            sireComboBox.SelectedIndexChanged += async (s, e) => await HandleSireSelectionChangedAsync();

            damComboBox.SelectedIndexChanged += async (s, e) => await HandleDamSelectionChangedAsync();

            pairComboBox.SelectedIndexChanged += async (s, e) => await HandlePairSelectionChangedAsync();

            multipleLittersGrid.CellContentClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == multipleLittersGrid.Columns["Remove"].Index)
                {
                    multipleLittersGrid.Rows.RemoveAt(e.RowIndex);
                }
            };
        }

        //this is an example of how I could organize using helper classes, but I am finding this a bit confusing...
        //    private async void InitializeEventHandlers()
        //    {
        //        this.Load += async (s, e) => await _eventHandler.HandleFormLoadAsyncLitter(
        //            speciesComboBox, damComboBox, sireComboBox, projectComboBox, pairComboBox);

        //        speciesComboBox.SelectedIndexChanged += async (s, e) => await _eventHandler.HandleSpeciesSelectionChangedDamAndSireAsync(
        //            speciesComboBox, damComboBox, sireComboBox);


        //        damComboBox.SelectedIndexChanged += async (s, e) => await _eventHandler.HandleDamSelectionChangedAsync(
        //            damComboBox, sireComboBox, pairComboBox);

        //        sireComboBox.SelectedIndexChanged += async (s, e) => await _eventHandler.HandleSireSelectionChangedAsync(
        //            sireComboBox, damComboBox, pairComboBox);

        //        addButton.Click += async (s, e) => await _eventHandler.HandleAddLitterClickAsync(
        //            litterIdTextBox.Text,
        //            litterNameTextBox.Text,
        //            pairComboBox.SelectedIndex + 1,
        //            litterDatePicker,
        //            numPups,
        //            numMales,
        //            numFemales,
        //            litterNotes);

        //        addToGridButton.Click += (s, e) => _eventHandler.HandleAddLitterToGridClick(
        //            litterIdTextBox.Text,
        //            litterNameTextBox.Text,
        //            pairComboBox.SelectedIndex + 1,
        //            litterDatePicker,
        //            numPups,
        //            numMales,
        //            numFemales,
        //            litterNotes,
        //            multipleLittersGrid);

        //        saveAllButton.Click += async (s, e) => await _eventHandler.HandleSaveAllLittersAsync(
        //            multipleLittersGrid);

        //        cancelButton.Click += (s, e) => _eventHandler.HandleCancelClick(this);

        //        //multipleLittersGrid.CellContentClick += (s, e) =>
        //        //{
        //        //    if (e.RowIndex >= 0 && e.ColumnIndex == multipleLittersGrid.Columns["Remove"].Index)
        //        //    {
        //        //        multipleLittersGrid.Rows.RemoveAt(e.RowIndex);
        //        //    }
        //        //};
        //    }





        //public async Task HandleSaveAllLittersAsync(DataGridView grid)
        //{
        //    try
        //    {
        //        _spinner.Show();

        //        if (grid.Rows.Count == 0)
        //        {
        //            MessageBox.Show("Please add at least one litter to the list", "Validation Error",
        //                MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            return;
        //        }

        //        foreach (DataGridViewRow row in grid.Rows)
        //        {
        //            var litter = new Litter
        //            {
        //                LitterId = row.Cells["LitterID"].Value.ToString(),
        //                Name = row.Cells["Name"].Value.ToString(),
        //                PairId = int.Parse(row.Cells["PairID"].Value.ToString()),
        //                DateOfBirth = DateTime.Parse(row.Cells["BirthDate"].Value.ToString()),
        //                NumPups = !string.IsNullOrWhiteSpace(row.Cells["NumPups"].Value?.ToString())
        //                    ? int.Parse(row.Cells["NumPups"].Value.ToString()) : null,
        //                NumMale = !string.IsNullOrWhiteSpace(row.Cells["NumMales"].Value?.ToString())
        //                    ? int.Parse(row.Cells["NumMales"].Value.ToString()) : null,
        //                NumFemale = !string.IsNullOrWhiteSpace(row.Cells["NumFemales"].Value?.ToString())
        //                    ? int.Parse(row.Cells["NumFemales"].Value.ToString()) : null,
        //                Notes = row.Cells["Notes"].Value?.ToString(),
        //                CreatedOn = DateTime.Now,
        //                LastUpdated = DateTime.Now
        //            };

        //            await _breedingService.CreateLitterAsync(litter);
        //        }

        //        MessageBox.Show($"Successfully added {grid.Rows.Count} litters!", "Success",
        //            MessageBoxButtons.OK, MessageBoxIcon.Information);

        //        grid.Rows.Clear();
        //    }
        //    finally
        //    {
        //        _spinner.Hide();
        //    }
        //}
    }
}
