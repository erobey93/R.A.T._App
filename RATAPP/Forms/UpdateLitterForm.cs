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
    /// Form for updating existing litters in the R.A.T. App.
    /// Provides litter data modification with validation.
    /// 
    /// Key Features:
    /// - Litter Information Update:
    ///   * Basic litter information
    ///   * Breeding pair selection
    ///   * Pup count tracking
    ///   * Notes and details
    /// 
    /// Data Validation:
    /// - Required field checking
    /// - Numeric input validation
    /// - Date range verification
    /// 
    /// UI Components:
    /// - Form validation
    /// - Loading indicators
    /// - Error handling
    /// 
    /// Dependencies:
    /// - BreedingService
    /// - AnimalService
    /// - ProjectService
    /// - SpeciesService
    /// - Form helper classes
    /// </summary>
    public partial class UpdateLitterForm : Form
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
        private TextBox numLivePups;
        private TextBox numMales;
        private TextBox numFemales;
        private TextBox litterNotes;
        private ComboBox projectComboBox;
        private DateTimePicker litterDatePicker;
        private ComboBox pairComboBox;
        private Button updateButton;
        private Button cancelButton;

        //services
        private BreedingService _breedingService;
        private SpeciesService _speciesService;
        private AnimalService _animalService;
        private ProjectService _projectService;

        //state
        bool pairSelected = false;
        private Litter _currentLitter;

        private UpdateLitterForm(RatAppDbContextFactory contextFactory, Litter litter)
        {
            _contextFactory = contextFactory;
            _currentLitter = litter;

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
        /// Static factory method that initializes the form and loads required data.
        /// Uses async initialization pattern for proper data loading.
        /// 
        /// Process:
        /// 1. Creates form instance
        /// 2. Loads species data
        /// 3. Loads animal records
        /// 4. Loads active pairs
        /// 5. Loads projects
        /// 6. Populates form with existing litter data
        /// 
        /// Error Handling:
        /// - Shows loading indicator
        /// - Manages async failures
        /// - Provides user feedback
        /// 
        /// Usage:
        /// var form = await UpdateLitterForm.CreateAsync(context, litter);
        /// </summary>
        public static async Task<UpdateLitterForm> CreateAsync(RatAppDbContextFactory contextFactory, Litter litter)
        {
            var form = new UpdateLitterForm(contextFactory, litter);
            await form.LoadInitialDataAsync();
            return form;
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                _spinner.Show();
                await LoadSpecies();
                await LoadAnimals();
                await LoadPairs();
                await LoadProjects();
                PopulateExistingLitterData();
            }
            finally
            {
                _spinner.Hide();
            }
        }

        private void PopulateExistingLitterData()
        {
            if (_currentLitter == null) return;

            // Set basic litter information
            litterIdTextBox.Text = _currentLitter.LitterId;
            litterIdTextBox.ReadOnly = true; // Make litter ID read-only for updates
            litterNameTextBox.Text = _currentLitter.Name;
            litterDatePicker.Value = (DateTime)_currentLitter.DateOfBirth;
            numPups.Text = _currentLitter.NumPups?.ToString() ?? "";
            numLivePups.Text = _currentLitter.NumLivePups?.ToString() ?? "";
            numMales.Text = _currentLitter.NumMale?.ToString() ?? "";
            numFemales.Text = _currentLitter.NumFemale?.ToString() ?? "";
            litterNotes.Text = _currentLitter.Notes;

            // Set selected pair
            if (_currentLitter.Pair != null)
            {
                pairComboBox.SelectedItem = _currentLitter.Pair;
                
                // Set dam and sire
                if (_currentLitter.Pair.Dam != null)
                {
                    damComboBox.SelectedItem = _currentLitter.Pair.Dam;
                }
                if (_currentLitter.Pair.Sire != null)
                {
                    sireComboBox.SelectedItem = _currentLitter.Pair.Sire;
                }

                // Set project
                if (_currentLitter.Pair.Project != null)
                {
                    projectComboBox.SelectedItem = _currentLitter.Pair.Project;

                    // Set species based on project's stock
                    if (_currentLitter.Pair.Project.Line?.Stock?.Species != null)
                    {
                        speciesComboBox.SelectedItem = _currentLitter.Pair.Project.Line.Stock.Species;
                    }
                }
            }
        }

        private void InitializeComponents()
        {
            //Properties
            this.Text = "Update Litter";
            this.Size = new Size(1200, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.AutoScroll = true;

            // Header with description
            var headerPanel = FormComponentFactory.CreateHeaderPanel("Update Litter");
            headerPanel.Height = 50;
            var headerLabel = headerPanel.Controls[0] as Label;
            if (headerLabel != null)
            {
                headerLabel.Location = new Point(25, 10);
            }
            var descriptionLabel = new Label
            {
                Text = "Update existing litter information",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                AutoSize = true,
            };

            int descriptionLabelX = headerLabel.Location.X + headerLabel.Width + 10;
            int descriptionLabelY = headerLabel.Location.Y;

            descriptionLabel.Location = new Point(descriptionLabelX, descriptionLabelY);
            headerPanel.Controls.Add(descriptionLabel);
            this.Controls.Add(headerPanel);

            // Create container panel for form content
            var contentPanel = new Panel
            {
                Location = new Point(0, headerPanel.Bottom),
                Width = this.ClientSize.Width,
                Height = this.ClientSize.Height - headerPanel.Height,
                Dock = DockStyle.Fill
            };

            // Create two columns for better organization
            var leftColumn = new Panel
            {
                Dock = DockStyle.Left,
                Width = 550,
                Padding = new Padding(0, 60, 20, 0)
            };

            var rightColumn = new Panel
            {
                Dock = DockStyle.Right,
                Width = 550,
                Padding = new Padding(10, 60, 0, 0)
            };

            // Create groups for related fields
            var basicInfoGroup = FormComponentFactory.CreateFormSection("Basic Information", DockStyle.Top, 250);
            var breedingInfoGroup = FormComponentFactory.CreateFormSection("Breeding Information", DockStyle.Top, 250);
            var litterDetailsGroup = FormComponentFactory.CreateFormSection("Litter Details", DockStyle.Top, 250);

            // Create and configure form fields with validation indicators
            speciesComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(speciesComboBox);
            var speciesField = CreateRequiredFormField("Species:", speciesComboBox);

            litterIdTextBox = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(litterIdTextBox);
            litterIdTextBox.ReadOnly = true; // Make litter ID read-only for updates
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

            numLivePups = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(numLivePups);
            numLivePups.KeyPress += (s, e) => ValidateNumericInput(e);
            var numLivePupsField = CreateRequiredFormField("Number of Live Pups:", numLivePups);

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
                numPupsField, numLivePupsField, numMalesField, numFemalesField, notesField
            });
            litterDetailsGroup.Controls.Add(litterDetailsPanel);

            // Create buttons with improved styling
            updateButton = new Button { Text = "Update Litter" };
            FormStyleHelper.ApplyButtonStyle(updateButton, true);
            updateButton.Margin = new Padding(0, 20, 0, 0);

            cancelButton = new Button { Text = "Cancel" };
            FormStyleHelper.ApplyButtonStyle(cancelButton, false);
            cancelButton.Margin = new Padding(10, 20, 0, 0);

            var buttonPanel = FormComponentFactory.CreateButtonPanel(updateButton, cancelButton);
            buttonPanel.Dock = DockStyle.Bottom;

            // Organize panels
            leftColumn.Controls.AddRange(new Control[] { basicInfoGroup, breedingInfoGroup });
            rightColumn.Controls.AddRange(new Control[] { litterDetailsGroup });

            contentPanel.Controls.AddRange(new Control[] { leftColumn, rightColumn });

            // Add enhanced information panel
            var infoPanel = FormComponentFactory.CreateInfoPanel("Important Information",
                "• Litter ID cannot be changed\n" +
                "• All fields marked with * are required\n" +
                "• Ensure the dam and sire are of appropriate age for breeding\n" +
                "• The birth date will be used to track litter development\n" +
                "• Numbers must be positive integers\n" +
                "• You can view all litters in the Breeding History section");

            contentPanel.Controls.Add(infoPanel);
            contentPanel.Controls.Add(buttonPanel);

            this.Controls.Add(contentPanel);
            SetTabOrder();
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

        private async Task LoadAnimals()
        {
            try
            {
                var animals = await _animalService.GetAllAnimalsAsync();

                damComboBox.Items.Clear();
                sireComboBox.Items.Clear();

                foreach (var animal in animals)
                {
                    if (animal.sex == "Female")
                    {
                        damComboBox.Items.Add(animal);
                    }
                    else if (animal.sex == "Male")
                    {
                        sireComboBox.Items.Add(animal);
                    }
                }

                damComboBox.DisplayMember = "Name";
                damComboBox.ValueMember = "Id";
                sireComboBox.DisplayMember = "Name";
                sireComboBox.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading animals: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadPairs()
        {
            try
            {
                var pairs = await _breedingService.GetAllActivePairingsAsync();

                pairComboBox.Items.Clear();
                pairComboBox.Items.Add("All Pairings");
                pairComboBox.Items.Add("Add New Pairing");

                foreach (var p in pairs)
                {
                    pairComboBox.Items.Add(p);
                }

                pairComboBox.SelectedIndex = 0;
                pairComboBox.DisplayMember = "DisplayText";
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

        public async Task HandleSireSelectionChangedAsync()
        {
            if (sireComboBox.SelectedItem == null) return;
            if (pairSelected == true) return;

            try
            {
                _spinner.Show();
                var selectedSire = sireComboBox.SelectedItem as AnimalDto;

                pairComboBox.Items.Clear();

                if (damComboBox.SelectedItem != null)
                {
                    var selectedDam = damComboBox.SelectedItem as AnimalDto;
                    var pairs = await _breedingService.GetAllActivePairingsByDamandSireIdAsync(selectedDam.Id, selectedSire.Id);
                    foreach (var pair in pairs)
                    {
                        pairComboBox.Items.Add(pair);
                    }
                }
                else
                {
                    var pairs = await _breedingService.GetAllActivePairingsByAnimalIdAsync(selectedSire.Id);
                    foreach (var pair in pairs)
                    {
                        pairComboBox.Items.Add(pair);
                    }
                }
            }
            finally
            {
                pairComboBox.SelectedIndex = 0;
                _spinner.Hide();
            }
        }

        public async Task HandleDamSelectionChangedAsync()
        {
            if (damComboBox.SelectedItem == null) return;
            if (pairSelected == true) return;

            try
            {
                _spinner.Show();
                var selectedDam = damComboBox.SelectedItem as AnimalDto;

                pairComboBox.Items.Clear();

                if (sireComboBox.SelectedItem != null)
                {
                    var selectedSire = sireComboBox.SelectedItem as AnimalDto;
                    var pairs = await _breedingService.GetAllActivePairingsByDamandSireIdAsync(selectedDam.Id, selectedSire.Id);
                    foreach (var pair in pairs)
                    {
                        pairComboBox.Items.Add(pair);
                    }
                }
                else
                {
                    var pairs = await _breedingService.GetAllActivePairingsByAnimalIdAsync(selectedDam.Id);
                    foreach (var pair in pairs)
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
            var selectedText = pairComboBox.Text;

            if (selectedText == "Add New Pairing")
            {
                AddPairingForm addPairing = await AddPairingForm.CreateAsync(_contextFactory);
                addPairing.ShowDialog();
                return;
            }
            else if (selectedPair == null && pairSelected == true)
            {
                LoadInitialDataAsync();
                pairSelected = false;
                return;
            }
            else if (selectedPair == null) return;
            else if (pairComboBox.SelectedItem == null) return;

            try
            {
                _spinner.Show();

                if (selectedPair.Dam != null)
                {
                    pairSelected = true;
                    damComboBox.Items.Clear();
                    sireComboBox.Items.Clear();
                    projectComboBox.Items.Clear();
                    speciesComboBox.Items.Clear();

                    var dam = selectedPair.Dam;
                    var sire = selectedPair.Sire;
                    var project = selectedPair.Project;

                    if (dam != null)
                    {
                        damComboBox.Items.Add(dam);
                    }
                    if (sire != null)
                    {
                        sireComboBox.Items.Add(sire);
                    }
                    if (project != null)
                    {
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
                    }
                    damComboBox.SelectedIndex = 0;
                    sireComboBox.SelectedIndex = 0;
                    projectComboBox.SelectedIndex = 0;

                    damComboBox.DisplayMember = "Name";
                    damComboBox.ValueMember = "Id";
                    sireComboBox.DisplayMember = "Name";
                    sireComboBox.ValueMember = "Id";
                    projectComboBox.DisplayMember = "Name";
                    projectComboBox.ValueMember = "Id";
                }
            }
            finally
            {
                _spinner.Hide();
            }
        }

        public async Task UpdateLitterClick()
        {
            try
            {
                _spinner.Show();

                if (string.IsNullOrWhiteSpace(litterNameTextBox.Text))
                {
                    MessageBox.Show("Please fill in all required fields", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var pair = pairComboBox.SelectedItem as Pairing;
                var updatedLitter = new Litter
                {
                    LitterId = litterIdTextBox.Text,
                    Name = litterNameTextBox.Text,
                    PairId = (int)pair.Id,
                    DateOfBirth = litterDatePicker.Value,
                    NumPups = !string.IsNullOrWhiteSpace(numPups.Text) ? int.Parse(numPups.Text) : null,
                    NumLivePups = !string.IsNullOrWhiteSpace(numLivePups.Text) ? int.Parse(numLivePups.Text) : null,
                    NumMale = !string.IsNullOrWhiteSpace(numMales.Text) ? int.Parse(numMales.Text) : null,
                    NumFemale = !string.IsNullOrWhiteSpace(numFemales.Text) ? int.Parse(numFemales.Text) : null,
                    Notes = litterNotes.Text,
                    LastUpdated = DateTime.Now
                };

                bool litterUpdated = await _breedingService.UpdateLitterAsync(updatedLitter);

                if (litterUpdated)
                {
                    MessageBox.Show("Litter updated successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("An error occurred. Litter not updated!", "Failure",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                _spinner.Hide();
            }
        }

        public void HandleCancelClick()
        {
            if (MessageBox.Show("Are you sure you want to cancel? Any unsaved changes will be lost.",
                "Confirm Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void InitializeEventHandlers()
        {
            updateButton.Click += async (s, e) => await UpdateLitterClick();
            cancelButton.Click += (s, e) => HandleCancelClick();
            sireComboBox.SelectedIndexChanged += async (s, e) => await HandleSireSelectionChangedAsync();
            damComboBox.SelectedIndexChanged += async (s, e) => await HandleDamSelectionChangedAsync();
            pairComboBox.SelectedIndexChanged += async (s, e) => await HandlePairSelectionChangedAsync();
        }
    }
}
