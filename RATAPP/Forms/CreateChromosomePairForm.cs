using System;
using System.Drawing;
using System.Windows.Forms;
using RATAPP.Helpers;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Services.Genetics;
using RATAPPLibrary.Services;

namespace RATAPP.Forms
{
    public partial class CreateChromosomePairForm : Form
    {
        private readonly ChromosomeService _chromosomeService;
        private readonly RatAppDbContextFactory _contextFactory;
        private readonly LoadingSpinnerHelper _spinner;

        // UI Components
        private ComboBox maternalChromosomeComboBox;
        private ComboBox paternalChromosomeComboBox;
        private ComboBox inheritancePatternComboBox;
        private ComboBox speciesComboBox;
        private Button createButton;
        private Button cancelButton;

        // Public property to access the created pair
        public ChromosomePair CreatedPair { get; private set; }

        private CreateChromosomePairForm(RatAppDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
            _chromosomeService = new ChromosomeService(contextFactory.CreateDbContext());
            _spinner = new LoadingSpinnerHelper(this, "Loading.gif");

            InitializeComponents();
            RegisterEventHandlers();
        }

        public static async Task<CreateChromosomePairForm> CreateAsync(RatAppDbContextFactory contextFactory)
        {
            var form = new CreateChromosomePairForm(contextFactory);
            await form.LoadInitialDataAsync();
            return form;
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                _spinner.Show();
                await LoadSpecies();
                await LoadChromosomes();
                LoadInheritancePatterns();
            }
            finally
            {
                _spinner.Hide();
            }
        }

        private void InitializeComponents()
        {
            // Form properties
            this.Text = "Create Chromosome Pair";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Create header
            var headerPanel = FormComponentFactory.CreateHeaderPanel("Create Chromosome Pair");
            headerPanel.Height = 50;

            // Create main container
            var mainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Create species selection
            speciesComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(speciesComboBox);
            var speciesField = CreateRequiredFormField("Species:", speciesComboBox);

            // Create chromosome selections
            maternalChromosomeComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(maternalChromosomeComboBox);
            var maternalField = CreateRequiredFormField("Maternal Chromosome:", maternalChromosomeComboBox);

            paternalChromosomeComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(paternalChromosomeComboBox);
            var paternalField = CreateRequiredFormField("Paternal Chromosome:", paternalChromosomeComboBox);

            // Create inheritance pattern selection
            inheritancePatternComboBox = new ComboBox();
            FormStyleHelper.ApplyComboBoxStyle(inheritancePatternComboBox);
            var inheritanceField = CreateRequiredFormField("Inheritance Pattern:", inheritancePatternComboBox);

            // Create buttons
            createButton = new Button { Text = "Create Pair" };
            FormStyleHelper.ApplyButtonStyle(createButton, true);

            cancelButton = new Button { Text = "Cancel" };
            FormStyleHelper.ApplyButtonStyle(cancelButton, false);

            var buttonPanel = FormComponentFactory.CreateButtonPanel(createButton, cancelButton);

            // Create info panel
            var infoPanel = FormComponentFactory.CreateInfoPanel("Important Information",
                "• All fields marked with * are required\n" +
                "• Select species first to load available chromosomes\n" +
                "• Click 'Create New' to add a new chromosome\n" +
                "• Maternal and paternal chromosomes must be from same species");

            // Add all components to main container
            mainContainer.Controls.AddRange(new Control[] {
                speciesField,
                maternalField,
                paternalField,
                inheritanceField,
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
            createButton.Click += async (s, e) => await HandleCreateClick();
            cancelButton.Click += (s, e) => HandleCancelClick();
            speciesComboBox.SelectedIndexChanged += async (s, e) => await HandleSpeciesChanged();
            maternalChromosomeComboBox.SelectedIndexChanged += async (s, e) => await HandleChromosomeSelectionChanged(true);
            paternalChromosomeComboBox.SelectedIndexChanged += async (s, e) => await HandleChromosomeSelectionChanged(false);
        }

        private async Task LoadSpecies()
        {
            try
            {
                var speciesService = new SpeciesService(_contextFactory);
                var species = await speciesService.GetAllSpeciesObjectsAsync();

                speciesComboBox.Items.Clear();
                foreach (var s in species)
                {
                    speciesComboBox.Items.Add(s);
                }

                speciesComboBox.DisplayMember = "CommonName";
                speciesComboBox.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading species: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadInheritancePatterns()
        {
            inheritancePatternComboBox.Items.Clear();
            inheritancePatternComboBox.Items.AddRange(new string[] {
                "Autosomal Dominant",
                "Autosomal Recessive",
                "Polygenic"
            });
        }

        private async Task LoadChromosomes()
        {
            if (speciesComboBox.SelectedItem == null) return;

            try
            {
                var species = speciesComboBox.SelectedItem as Species;
                var chromosomes = await _chromosomeService.GetChromosomesBySpeciesAsync(species.Id);

                maternalChromosomeComboBox.Items.Clear();
                paternalChromosomeComboBox.Items.Clear();

                maternalChromosomeComboBox.Items.Add("Create New");
                paternalChromosomeComboBox.Items.Add("Create New");

                foreach (var chromosome in chromosomes)
                {
                    maternalChromosomeComboBox.Items.Add(chromosome);
                    paternalChromosomeComboBox.Items.Add(chromosome);
                }

                maternalChromosomeComboBox.DisplayMember = "Name";
                maternalChromosomeComboBox.ValueMember = "ChromosomeId";
                paternalChromosomeComboBox.DisplayMember = "Name";
                paternalChromosomeComboBox.ValueMember = "ChromosomeId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading chromosomes: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task HandleSpeciesChanged()
        {
            await LoadChromosomes();
        }

        private async Task HandleChromosomeSelectionChanged(bool isMaternal)
        {
            var comboBox = isMaternal ? maternalChromosomeComboBox : paternalChromosomeComboBox;
            
            if (comboBox.SelectedIndex == 0) // "Create New"
            {
                var createForm = await CreateChromosomeForm.CreateAsync(_contextFactory, speciesComboBox.SelectedItem as Species);
                if (createForm.ShowDialog() == DialogResult.OK)
                {
                    await LoadChromosomes();
                    // Select the newly created chromosome
                    if (isMaternal)
                        maternalChromosomeComboBox.SelectedItem = createForm.CreatedChromosome;
                    else
                        paternalChromosomeComboBox.SelectedItem = createForm.CreatedChromosome;
                }
                else
                {
                    comboBox.SelectedIndex = -1;
                }
            }
        }

        private async Task HandleCreateClick()
        {
            if (!ValidateForm()) return;

            try
            {
                _spinner.Show();

                var maternalChromosome = maternalChromosomeComboBox.SelectedItem as Chromosome;
                var paternalChromosome = paternalChromosomeComboBox.SelectedItem as Chromosome;
                var inheritancePattern = inheritancePatternComboBox.SelectedItem.ToString().ToLower().Replace(" ", "-");

                CreatedPair = await _chromosomeService.CreateChromosomePairAsync(
                    maternalChromosome.ChromosomeId,
                    paternalChromosome.ChromosomeId,
                    inheritancePattern);

                MessageBox.Show("Chromosome pair created successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating chromosome pair: {ex.Message}", "Error",
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
            if (speciesComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a species.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (maternalChromosomeComboBox.SelectedItem == null || maternalChromosomeComboBox.SelectedIndex == 0)
            {
                MessageBox.Show("Please select or create a maternal chromosome.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (paternalChromosomeComboBox.SelectedItem == null || paternalChromosomeComboBox.SelectedIndex == 0)
            {
                MessageBox.Show("Please select or create a paternal chromosome.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (inheritancePatternComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select an inheritance pattern.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}
