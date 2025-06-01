using System;
using System.Drawing;
using System.Windows.Forms;
using RATAPP.Helpers;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Services;
using RATAPPLibrary.Services.Genetics;

namespace RATAPP.Forms
{
    public partial class AssignTraitForm : Form
    {
        private readonly TraitService _traitService;
        private readonly GeneService _geneService;
        private readonly AnimalService _animalService;
        private readonly RatAppDbContextFactory _contextFactory;
        private readonly LoadingSpinnerHelper _spinner;
        private readonly Trait _selectedTrait;

        // UI Components
        private Label traitNameLabel;
        private Label speciesLabel;
        private Label genotypeLabel;
        private ComboBox animalComboBox;
        private Button assignButton;
        private Button cancelButton;

        public AssignTraitForm(TraitService traitService, GeneService geneService, AnimalService animalService, 
            RatAppDbContextFactory contextFactory, Trait selectedTrait)
        {
            _traitService = traitService;
            _geneService = geneService;
            _animalService = animalService;
            _contextFactory = contextFactory;
            _selectedTrait = selectedTrait;
            _spinner = new LoadingSpinnerHelper(this, "C:\\Users\\earob\\source\\repos\\R.A.T._APP\\RATAPP\\Resources\\Loading_2.gif");

            InitializeComponents();
            RegisterEventHandlers();
            LoadAnimals();
        }

        private void InitializeComponents()
        {
            // Form properties
            this.Text = "Assign Trait";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Create header
            var headerPanel = FormComponentFactory.CreateHeaderPanel("Assign Trait");
            headerPanel.Height = 50;

            // Create main container
            var mainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Create trait info group
            var traitInfoGroup = FormComponentFactory.CreateFormSection("Trait Information", DockStyle.Top, 150);

            // Create trait name display
            traitNameLabel = new Label
            {
                Text = $"Trait: {_selectedTrait.CommonName}",
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
            var traitNameField = FormComponentFactory.CreateFormField("", traitNameLabel);

            // Create species display
            speciesLabel = new Label
            {
                Text = $"Species: {_selectedTrait.Species?.CommonName ?? "Unknown"}",
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
            var speciesField = FormComponentFactory.CreateFormField("", speciesLabel);

            // Create genotype display
            genotypeLabel = new Label
            {
                Text = $"Genotype: {_selectedTrait.Genotype ?? "Not specified"}",
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
            var genotypeField = FormComponentFactory.CreateFormField("", genotypeLabel);

            traitInfoGroup.Controls.AddRange(new Control[] {
                traitNameField,
                speciesField,
                genotypeField
            });

            // Create animal selection group
            var animalGroup = FormComponentFactory.CreateFormSection("Select Animal", DockStyle.Top, 100);

            // Create animal selection
            animalComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(animalComboBox);
            var animalField = CreateRequiredFormField("Animal:", animalComboBox);

            animalGroup.Controls.Add(animalField);

            // Create buttons
            assignButton = new Button { Text = "Assign Trait" };
            FormStyleHelper.ApplyButtonStyle(assignButton, true);

            cancelButton = new Button { Text = "Cancel" };
            FormStyleHelper.ApplyButtonStyle(cancelButton, false);

            var buttonPanel = FormComponentFactory.CreateButtonPanel(assignButton, cancelButton);

            // Create info panel
            var infoPanel = FormComponentFactory.CreateInfoPanel("Important Information",
                "• Only animals of the correct species are shown\n" +
                "• Assignment cannot be undone\n" +
                "• The trait's genotype will be assigned to the animal");

            // Add all components to main container
            mainContainer.Controls.AddRange(new Control[] {
                traitInfoGroup,
                animalGroup,
                infoPanel,
                buttonPanel
            });

            // Add components to form
            this.Controls.AddRange(new Control[] {
                headerPanel,
                mainContainer
            });
        }

        private Panel CreateRequiredFormField(string label, Control control)
        {
            var panel = FormComponentFactory.CreateFormField(label + " *", control);
            panel.Margin = new Padding(0, 0, 0, 10);
            return panel;
        }

        private void RegisterEventHandlers()
        {
            assignButton.Click += async (s, e) => await HandleAssignClick();
            cancelButton.Click += (s, e) => HandleCancelClick();
        }

        private async void LoadAnimals()
        {
            try
            {
                _spinner.Show();

                // Get animals filtered by species
                var animals = await _animalService.GetAllAnimalsAsync(
                    species: _selectedTrait.Species?.CommonName);

                animalComboBox.Items.Clear();
                foreach (var animal in animals)
                {
                    animalComboBox.Items.Add(animal);
                }

                animalComboBox.DisplayMember = "name";
                animalComboBox.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading animals: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _spinner.Hide();
            }
        }

        private async Task HandleAssignClick()
        {
            if (!ValidateForm()) return;

            try
            {
                _spinner.Show();

                var selectedAnimal = animalComboBox.SelectedItem as AnimalDto;

                // Create trait-animal association
                await _traitService.CreateAnimalTraitAsync(_selectedTrait.Id, selectedAnimal.Id);

                // Assign genotype
                await _geneService.AssignGenericGenotypeToAnimalAsync(selectedAnimal.Id, _selectedTrait.Id);

                MessageBox.Show("Trait assigned successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error assigning trait: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _spinner.Hide();
            }
        }

        private void HandleCancelClick()
        {
            this.Close();
        }

        private bool ValidateForm()
        {
            if (animalComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select an animal.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}
