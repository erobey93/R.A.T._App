using System;
using System.Drawing;
using System.Windows.Forms;
using RATAPP.Helpers;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Services.Genetics;

namespace RATAPP.Forms
{
    public partial class CreateChromosomeForm : Form
    {
        private readonly ChromosomeService _chromosomeService;
        private readonly RatAppDbContextFactory _contextFactory;
        private readonly LoadingSpinnerHelper _spinner;
        private readonly Species _species;

        // UI Components
        private TextBox nameTextBox;
        private NumericUpDown numberInput;
        private RadioButton pArmRadio;
        private RadioButton qArmRadio;
        private NumericUpDown regionInput;
        private NumericUpDown bandInput;
        private TextBox descriptionTextBox;
        private Button createButton;
        private Button cancelButton;

        // Public property to access the created chromosome
        public Chromosome CreatedChromosome { get; private set; }

        private CreateChromosomeForm(RatAppDbContextFactory contextFactory, Species species)
        {
            _contextFactory = contextFactory;
            _chromosomeService = new ChromosomeService(contextFactory.CreateContext());
            _species = species;
            _spinner = new LoadingSpinnerHelper(this, "C:\\Users\\earob\\source\\repos\\R.A.T._APP\\RATAPP\\Resources\\Loading_2.gif");

            InitializeComponents();
            RegisterEventHandlers();
        }

        public static async Task<CreateChromosomeForm> CreateAsync(RatAppDbContextFactory contextFactory, Species species)
        {
            var form = new CreateChromosomeForm(contextFactory, species);
            return form;
        }

        private void InitializeComponents()
        {
            // Form properties
            this.Text = "Create Chromosome";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Create header
            var headerPanel = FormComponentFactory.CreateHeaderPanel("Create Chromosome");
            headerPanel.Height = 50;

            // Create main container
            var mainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Create required fields group
            var requiredGroup = FormComponentFactory.CreateFormSection("Required Information", DockStyle.Top, 150);

            // Create name field
            nameTextBox = new TextBox();
            FormStyleHelper.ApplyTextBoxStyle(nameTextBox);
            var nameField = CreateRequiredFormField("Name:", nameTextBox);

            // Create number field
            numberInput = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 100,
                Width = 100
            };
            var numberField = CreateRequiredFormField("Number:", numberInput);

            // Create species display
            var speciesLabel = new Label
            {
                Text = _species.CommonName,
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };
            var speciesField = FormComponentFactory.CreateFormField("Species:", speciesLabel);

            requiredGroup.Controls.AddRange(new Control[] {
                nameField,
                numberField,
                speciesField
            });

            // Create optional fields group
            var optionalGroup = FormComponentFactory.CreateFormSection("Optional Information", DockStyle.Top, 250);

            // Create arm selection
            var armPanel = new Panel { Height = 30, Dock = DockStyle.Top };
            var armLabel = new Label
            {
                Text = "Arm:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(0, 5)
            };

            pArmRadio = new RadioButton
            {
                Text = "p (short)",
                Location = new Point(100, 3),
                Font = new Font("Segoe UI", 10)
            };

            qArmRadio = new RadioButton
            {
                Text = "q (long)",
                Location = new Point(200, 3),
                Font = new Font("Segoe UI", 10)
            };

            armPanel.Controls.AddRange(new Control[] { armLabel, pArmRadio, qArmRadio });

            // Create region field
            regionInput = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 100,
                Width = 100
            };
            var regionField = FormComponentFactory.CreateFormField("Region:", regionInput);

            // Create band field
            bandInput = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 100,
                Width = 100
            };
            var bandField = FormComponentFactory.CreateFormField("Band:", bandInput);

            // Create description field
            descriptionTextBox = new TextBox { Multiline = true, Height = 60 };
            FormStyleHelper.ApplyTextBoxStyle(descriptionTextBox);
            var descriptionField = FormComponentFactory.CreateFormField("Description:", descriptionTextBox);

            optionalGroup.Controls.AddRange(new Control[] {
                armPanel,
                regionField,
                bandField,
                descriptionField
            });

            // Create buttons
            createButton = new Button { Text = "Create Chromosome" };
            FormStyleHelper.ApplyButtonStyle(createButton, true);

            cancelButton = new Button { Text = "Cancel" };
            FormStyleHelper.ApplyButtonStyle(cancelButton, false);

            var buttonPanel = FormComponentFactory.CreateButtonPanel(createButton, cancelButton);

            // Create info panel
            var infoPanel = FormComponentFactory.CreateInfoPanel("Important Information",
                "• Name and number are required\n" +
                "• Number must be unique for the species\n" +
                "• Arm can be p (short) or q (long)\n" +
                "• Region and band are optional location identifiers");

            // Add all components to main container
            mainContainer.Controls.AddRange(new Control[] {
                requiredGroup,
                optionalGroup,
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
        }

        private async Task HandleCreateClick()
        {
            if (!ValidateForm()) return;

            try
            {
                _spinner.Show();

                var chromosome = new Chromosome
                {
                    ChromosomeId = Guid.NewGuid(),
                    Name = nameTextBox.Text.Trim(),
                    Number = (int)numberInput.Value,
                    SpeciesId = _species.Id,
                    Arm = GetSelectedArm(),
                    Region = regionInput.Value > 0 ? (int)regionInput.Value : null,
                    Band = bandInput.Value > 0 ? bandInput.Value.ToString() : null,
                    Description = descriptionTextBox.Text.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _chromosomeService.ValidateChromosomeNumberAsync(chromosome.Number, chromosome.SpeciesId);
                CreatedChromosome = await _chromosomeService.CreateChromosomeAsync(
                    chromosome.Name,
                    chromosome.Number,
                    chromosome.SpeciesId);

                MessageBox.Show("Chromosome created successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating chromosome: {ex.Message}", "Error",
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

        private char? GetSelectedArm()
        {
            if (pArmRadio.Checked) return 'p';
            if (qArmRadio.Checked) return 'q';
            return null;
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Please enter a name for the chromosome.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (numberInput.Value < 1)
            {
                MessageBox.Show("Please enter a valid chromosome number.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}
