using System;
using System.Drawing;
using System.Windows.Forms;
using RATAPP.Helpers;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Services;
using RATAPPLibrary.Services.Genetics;

namespace RATAPP.Forms
{
    public partial class AddTraitForm : Form
    {
        // Services
        private readonly TraitService _traitService;
        private readonly GeneService _geneService;
        private readonly ChromosomeService _chromosomeService;
        private readonly RatAppDbContextFactory _contextFactory;
        private readonly LoadingSpinnerHelper _spinner;

        // UI Components
        private TabControl mainTabControl;
        private TabPage traitDetailsTab;

        // Trait Info Components
        private TextBox traitNameTextBox;
        private ComboBox traitTypeComboBox;
        private TextBox descriptionTextBox;

        // Genetic Info Components
        private ComboBox chromosomePairComboBox;
        private Label speciesLabel;
        private TextBox genotypeTextBox;

        // Buttons
        private Button saveButton;
        private Button cancelButton;

        private AddTraitForm(RatAppDbContextFactory contextFactory, TraitService traitService, GeneService geneService)
        {
            _contextFactory = contextFactory;
            _traitService = traitService;
            _geneService = geneService;
            _chromosomeService = new ChromosomeService(contextFactory.CreateDbContext());
            _spinner = new LoadingSpinnerHelper(this, "Loading.gif");

            InitializeComponents();
            RegisterEventHandlers();
        }

        public static async Task<AddTraitForm> CreateAsync(
            RatAppDbContextFactory contextFactory,
            TraitService traitService,
            GeneService geneService)
        {
            var form = new AddTraitForm(contextFactory, traitService, geneService);
            await form.LoadInitialDataAsync();
            return form;
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                _spinner.Show();
                await LoadTraitTypes();
                await LoadChromosomePairs();
            }
            finally
            {
                _spinner.Hide();
            }
        }

        private void InitializeComponents()
        {
            // Form properties
            this.Text = "Add Trait";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Create header
            var headerPanel = FormComponentFactory.CreateHeaderPanel("Add Trait");
            headerPanel.Height = 50;

            var headerLabel = headerPanel.Controls[0] as Label;
            if (headerLabel != null)
            {
                headerLabel.Location = new Point(25, 10);
            }

            var descriptionLabel = new Label
            {
                Text = "Add a new genetic trait with chromosome and genotype information",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(headerLabel.Right + 10, headerLabel.Top + 5)
            };
            headerPanel.Controls.Add(descriptionLabel);

            // Create main container
            var mainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Create trait info group
            var traitInfoGroup = FormComponentFactory.CreateFormSection("Trait Information", DockStyle.Top, 150);
            
            // Create trait name field
            traitNameTextBox = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(traitNameTextBox);
            var traitNameField = CreateRequiredFormField("Trait Name:", traitNameTextBox);

            // Create trait type field
            traitTypeComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(traitTypeComboBox);
            var traitTypeField = CreateRequiredFormField("Trait Type:", traitTypeComboBox);

            // Create description field
            descriptionTextBox = new TextBox { Multiline = true, Height = 60 };
            FormStyleHelper.ApplyTextBoxStyle(descriptionTextBox);
            var descriptionField = FormComponentFactory.CreateFormField("Description:", descriptionTextBox);

            traitInfoGroup.Controls.AddRange(new Control[] {
                traitNameField,
                traitTypeField,
                descriptionField
            });

            // Create genetic info group
            var geneticInfoGroup = FormComponentFactory.CreateFormSection("Genetic Information", DockStyle.Top, 150);

            // Create chromosome pair field
            chromosomePairComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(chromosomePairComboBox);
            var chromosomePairField = CreateRequiredFormField("Chromosome Pair:", chromosomePairComboBox);

            // Create species display
            speciesLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };
            var speciesField = FormComponentFactory.CreateFormField("Species:", speciesLabel);

            // Create genotype field
            genotypeTextBox = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(genotypeTextBox);
            var genotypeField = CreateRequiredFormField("Genotype:", genotypeTextBox);

            geneticInfoGroup.Controls.AddRange(new Control[] {
                chromosomePairField,
                speciesField,
                genotypeField
            });

            // Create buttons
            saveButton = new Button { Text = "Save Trait" };
            FormStyleHelper.ApplyButtonStyle(saveButton, true);

            cancelButton = new Button { Text = "Cancel" };
            FormStyleHelper.ApplyButtonStyle(cancelButton, false);

            var buttonPanel = FormComponentFactory.CreateButtonPanel(saveButton, cancelButton);

            // Create info panel
            var infoPanel = FormComponentFactory.CreateInfoPanel("Important Information",
                "• All fields marked with * are required\n" +
                "• Chromosome pair must exist or be created\n" +
                "• Genotype must follow the correct format\n" +
                "• Trait name must be unique");

            // Add all components to main container
            mainContainer.Controls.AddRange(new Control[] {
                traitInfoGroup,
                geneticInfoGroup,
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
            saveButton.Click += async (s, e) => await HandleSaveClick();
            cancelButton.Click += (s, e) => HandleCancelClick();
            chromosomePairComboBox.SelectedIndexChanged += async (s, e) => await HandleChromosomePairChanged();
        }

        private async Task LoadTraitTypes()
        {
            try
            {
                var traitTypes = await _traitService.GetAllTraitTypesAsync();
                traitTypeComboBox.Items.Clear();

                foreach (var type in traitTypes)
                {
                    traitTypeComboBox.Items.Add(type);
                }

                traitTypeComboBox.DisplayMember = "Name";
                traitTypeComboBox.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading trait types: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadChromosomePairs()
        {
            try
            {
                chromosomePairComboBox.Items.Clear();
                chromosomePairComboBox.Items.Add("Create New Pair");

                // Load existing pairs
                using (var context = _contextFactory.CreateDbContext())
                {
                    var pairs = await _chromosomeService.GetAllChromosomePairsAsync();
                    foreach (var pair in pairs)
                    {
                        chromosomePairComboBox.Items.Add(pair);
                    }
                }

                chromosomePairComboBox.DisplayMember = "Name";
                chromosomePairComboBox.ValueMember = "PairId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading chromosome pairs: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task HandleChromosomePairChanged()
        {
            if (chromosomePairComboBox.SelectedItem == null) return;

            if (chromosomePairComboBox.SelectedIndex == 0) // "Create New Pair"
            {
                var createPairForm = await CreateChromosomePairForm.CreateAsync(_contextFactory);
                if (createPairForm.ShowDialog() == DialogResult.OK)
                {
                    await LoadChromosomePairs();
                    // Select the newly created pair
                    chromosomePairComboBox.SelectedItem = createPairForm.CreatedPair;
                }
            }
            else
            {
                var selectedPair = chromosomePairComboBox.SelectedItem as ChromosomePair;
                if (selectedPair?.MaternalChromosome?.Species != null)
                {
                    speciesLabel.Text = selectedPair.MaternalChromosome.Species.CommonName;
                }
            }
        }

        private async Task HandleSaveClick()
        {
            if (!ValidateForm()) return;

            try
            {
                _spinner.Show();

                var selectedPair = chromosomePairComboBox.SelectedItem as ChromosomePair;
                var selectedType = traitTypeComboBox.SelectedItem as TraitType;

                var trait = new Trait
                {
                    CommonName = traitNameTextBox.Text.Trim(),
                    TraitTypeId = selectedType.Id,
                    Genotype = genotypeTextBox.Text.Trim(),
                    SpeciesID = selectedPair.MaternalChromosome.SpeciesId
                };

                // Create GenericGenotype
                var genericGenotype = new GenericGenotype
                {
                    GenotypeCode = genotypeTextBox.Text.Trim(),
                    ChromosomePairId = selectedPair.PairId,
                    TraitId = trait.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _traitService.CreateTraitAsync(trait);

                MessageBox.Show("Trait created successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving trait: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _spinner.Hide();
            }
        }

        private void HandleCancelClick()
        {
            if (MessageBox.Show("Are you sure you want to cancel? Any unsaved changes will be lost.",
                "Confirm Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(traitNameTextBox.Text))
            {
                MessageBox.Show("Please enter a trait name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (traitTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a trait type.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (chromosomePairComboBox.SelectedItem == null || chromosomePairComboBox.SelectedIndex == 0)
            {
                MessageBox.Show("Please select or create a chromosome pair.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(genotypeTextBox.Text))
            {
                MessageBox.Show("Please enter a genotype.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}
