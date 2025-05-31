using System;
using System.Drawing;
using System.Windows.Forms;
using RATAPP.Helpers;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Services;

namespace RATAPP.Forms
{
    /// <summary>
    /// Form for updating existing pairings.
    /// Uses helper classes to manage UI, data, and events.
    /// </summary>
    public partial class UpdatePairingForm : Form
    {
        // Helper classes
        private readonly LoadingSpinnerHelper _spinner;
        private BreedingService _breedingService;
        private AnimalService _animalService;
        private ProjectService _projectService;
        private SpeciesService _speciesService;

        // UI Controls
        private ComboBox damComboBox;
        private ComboBox sireComboBox;
        private ComboBox speciesComboBox;
        private TextBox pairingIdTextBox;
        private ComboBox projectComboBox;
        private DateTimePicker pairingDatePicker;
        private DateTimePicker pairingEndDatePicker;
        private Button updateButton;
        private Button cancelButton;

        private RatAppDbContextFactory _contextFactory;
        private Pairing _currentPairing;

        //state
        bool speciesSelected = false;
        bool projectSelected = false;

        private UpdatePairingForm(RatAppDbContextFactory contextFactory, Pairing pairing)
        {
            // Initialize services
            _breedingService = new BreedingService(contextFactory);
            _animalService = new AnimalService(contextFactory);
            _projectService = new ProjectService(contextFactory);
            _speciesService = new SpeciesService(contextFactory);

            _contextFactory = contextFactory;
            _currentPairing = pairing;

            _spinner = new LoadingSpinnerHelper(this, "C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\Loading_2.gif");

            InitializeComponents();
            InitializeEventHandlers();
        }

        public static async Task<UpdatePairingForm> CreateAsync(RatAppDbContextFactory contextFactory, Pairing pairing)
        {
            var form = new UpdatePairingForm(contextFactory, pairing);
            await form.LoadInitialDataAsync();
            return form;
        }

        private void InitializeComponents()
        {
            // Set form properties
            this.Text = "Update Pairing";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.AutoScroll = true;

            // Create header with description
            var headerPanel = FormComponentFactory.CreateHeaderPanel("Update Pairing");
            headerPanel.Height = 50;
            var headerLabel = headerPanel.Controls[0] as Label;
            if (headerLabel != null)
            {
                headerLabel.Location = new Point(25, 10);
            }
            var descriptionLabel = new Label
            {
                Text = "Update existing breeding pair information",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                AutoSize = true
            };

            int descriptionLabelX = headerLabel.Location.X + headerLabel.Width + 10;
            int descriptionLabelY = headerLabel.Location.Y;
            descriptionLabel.Location = new Point(descriptionLabelX, descriptionLabelY);
            headerPanel.Controls.Add(descriptionLabel);
            this.Controls.Add(headerPanel);

            // Create main panel
            var mainPanel = new Panel
            {
                Location = new Point(0, headerPanel.Bottom),
                Width = this.ClientSize.Width,
                Height = this.ClientSize.Height - headerPanel.Height,
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Create two columns for better organization
            var leftColumn = new Panel
            {
                Dock = DockStyle.Left,
                Width = 350,
                Padding = new Padding(0, 0, 20, 0)
            };

            var rightColumn = new Panel
            {
                Dock = DockStyle.Right,
                Width = 350,
                Padding = new Padding(10, 0, 0, 0)
            };

            // Create groups for related fields
            var basicInfoGroup = FormComponentFactory.CreateFormSection("Basic Information", DockStyle.Top, 250);
            var breedingInfoGroup = FormComponentFactory.CreateFormSection("Pairing Information", DockStyle.Top, 250);

            // Create and configure form fields with validation indicators
            speciesComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(speciesComboBox);
            var speciesField = CreateRequiredFormField("Species", speciesComboBox);

            pairingIdTextBox = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(pairingIdTextBox);
            pairingIdTextBox.ReadOnly = true;
            var pairingIdField = CreateRequiredFormField("Pairing ID", pairingIdTextBox);

            projectComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(projectComboBox);
            var projectField = CreateRequiredFormField("Project", projectComboBox);

            damComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(damComboBox);
            var damField = CreateRequiredFormField("Dam (Female)", damComboBox);

            sireComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(sireComboBox);
            var sireField = CreateRequiredFormField("Sire (Male)", sireComboBox);

            pairingDatePicker = new DateTimePicker { Format = DateTimePickerFormat.Short };
            var dateField = CreateRequiredFormField("Pairing Start Date", pairingDatePicker);

            pairingEndDatePicker = new DateTimePicker { Format = DateTimePickerFormat.Short };
            pairingEndDatePicker.ShowCheckBox = true;
            var endDateField = FormComponentFactory.CreateFormField("Pairing End Date", pairingEndDatePicker);

            // Organize fields into groups
            var basicInfoPanel = new Panel { Dock = DockStyle.Fill };
            basicInfoPanel.Controls.AddRange(new Control[] {
                speciesField, pairingIdField, projectField
            });
            basicInfoGroup.Controls.Add(basicInfoPanel);

            var breedingInfoPanel = new Panel { Dock = DockStyle.Fill };
            breedingInfoPanel.Controls.AddRange(new Control[] {
                damField, sireField, dateField, endDateField
            });
            breedingInfoGroup.Controls.Add(breedingInfoPanel);

            // Create buttons with improved styling
            updateButton = new Button { Text = "Update Pairing" };
            FormStyleHelper.ApplyButtonStyle(updateButton, true);
            updateButton.Margin = new Padding(0, 20, 0, 0);

            cancelButton = new Button { Text = "Cancel" };
            FormStyleHelper.ApplyButtonStyle(cancelButton, false);
            cancelButton.Margin = new Padding(10, 20, 0, 0);

            var buttonPanel = FormComponentFactory.CreateButtonPanel(updateButton, cancelButton);
            buttonPanel.Dock = DockStyle.Bottom;

            // Organize panels
            leftColumn.Controls.Add(basicInfoGroup);
            rightColumn.Controls.Add(breedingInfoGroup);

            mainPanel.Controls.AddRange(new Control[] { leftColumn, rightColumn });

            // Add enhanced information panel
            var infoPanel = FormComponentFactory.CreateInfoPanel("Important Information",
                "• Pairing ID cannot be changed\n" +
                "• All fields marked with * are required\n" +
                "• Ensure the dam and sire are of appropriate age for breeding\n" +
                "• The pairing date will be used to calculate expected litter dates\n" +
                "• Dam and sire must be of the same species\n" +
                "• End date is optional and can be used to mark inactive pairings");

            mainPanel.Controls.Add(infoPanel);
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);
            SetTabOrder();
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                _spinner.Show();
                await LoadAnimals();
                await LoadProjects();
                await LoadSpecies();
                PopulateExistingPairingData();
            }
            finally
            {
                _spinner.Hide();
            }
        }

        private void PopulateExistingPairingData()
        {
            if (_currentPairing == null) return;

            // Set basic pairing information
            pairingIdTextBox.Text = _currentPairing.pairingId;
            pairingDatePicker.Value = _currentPairing.PairingStartDate ?? DateTime.Now;
            
            if (_currentPairing.PairingEndDate.HasValue)
            {
                pairingEndDatePicker.Checked = true;
                pairingEndDatePicker.Value = _currentPairing.PairingEndDate.Value;
            }
            else
            {
                pairingEndDatePicker.Checked = false;
            }

            // Set dam and sire
            if (_currentPairing.Dam != null)
            {
                damComboBox.SelectedItem = _currentPairing.Dam;
            }
            if (_currentPairing.Sire != null)
            {
                sireComboBox.SelectedItem = _currentPairing.Sire;
            }

            // Set project and species
            if (_currentPairing.Project != null)
            {
                projectComboBox.SelectedItem = _currentPairing.Project;

                if (_currentPairing.Project.Line?.Stock?.Species != null)
                {
                    speciesComboBox.SelectedItem = _currentPairing.Project.Line.Stock.Species;
                }
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

        private async Task LoadProjects()
        {
            try
            {
                var projects = await _projectService.GetAllProjectsAsync();

                projectComboBox.Items.Clear();
                projectComboBox.Items.Add("All Projects");

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
            cancelButton.Click += (s, e) => HandleCancelClick(this);
            updateButton.Click += async (s, e) => await UpdatePairingClick();
            projectComboBox.SelectedIndexChanged += (s, e) => HandleProjectSelectedIndexChanged(this);
        }

        private async Task UpdatePairingClick()
        {
            try
            {
                _spinner.Show();

                if (projectComboBox.SelectedItem == null || damComboBox.SelectedItem == null || sireComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please fill in all required fields", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var damSelected = damComboBox.SelectedItem as Animal;
                var sireSelected = sireComboBox.SelectedItem as Animal;
                var projectSelected = projectComboBox.SelectedItem as Project;

                var updatedPairing = new Pairing
                {
                    pairingId = pairingIdTextBox.Text,
                    SireId = sireSelected.Id,
                    DamId = damSelected.Id,
                    ProjectId = projectSelected.Id,
                    PairingStartDate = pairingDatePicker.Value,
                    PairingEndDate = pairingEndDatePicker.Checked ? pairingEndDatePicker.Value : null,
                    LastUpdated = DateTime.Now
                };

                bool pairingUpdated = await _breedingService.UpdatePairingAsync(updatedPairing);

                if (pairingUpdated)
                {
                    MessageBox.Show("Pairing updated successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("An error occurred. Pairing not updated!", "Update Pairing Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                _spinner.Hide();
            }
        }

        private void HandleCancelClick(Form form)
        {
            if (MessageBox.Show("Are you sure you want to cancel? Any unsaved changes will be lost.",
                "Confirm Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                form.Close();
            }
        }

        private void HandleProjectSelectedIndexChanged(Form form)
        {
            Project proj = projectComboBox.SelectedItem as Project;
            if (proj != null)
            {
                Species species = proj.Line.Stock.Species;
                speciesComboBox.Items.Clear();
                speciesComboBox.Items.Add(species);
                speciesComboBox.DisplayMember = "CommonName";
                speciesComboBox.ValueMember = "Id";
                speciesComboBox.SelectedIndex = 0;

                Animal[] animalsInLine = proj.Line.Animals.ToArray();
                sireComboBox.Items.Clear();
                damComboBox.Items.Clear();
                foreach (var animal in animalsInLine)
                {
                    if (animal.Sex == "Male")
                    {
                        sireComboBox.Items.Add(animal);
                        sireComboBox.DisplayMember = "Name";
                        speciesComboBox.ValueMember = "Id";
                    }
                    else
                    {
                        damComboBox.Items.Add(animal);
                        sireComboBox.DisplayMember = "Name";
                        speciesComboBox.ValueMember = "Id";
                    }
                }
            }
        }
    }
}
