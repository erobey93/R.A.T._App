using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using RATAPPLibrary.Services;
using RATAPPLibrary.Services.Genetics;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Services.Genetics.Interfaces;

namespace RATAPP.Forms
{
    public class BreedingCalculatorForm : RATAppBaseForm
    {
        private readonly TraitService _traitService;
        private readonly GeneService _geneService;
        private readonly BreedingCalculationService _breedingService;
        private readonly AnimalService _animalService;

        // Form Controls
        private ComboBox parent1ComboBox;
        private ComboBox parent2ComboBox;
        private DataGridView traitProbabilitiesGrid;
        private DataGridView riskAnalysisGrid;
        private Button calculateButton;
        private Button closeButton;
        private Label compatibilityLabel;
        private Label inbreedingLabel;
        private Panel resultPanel;

        private BreedingCalculation currentCalculation;

        public BreedingCalculatorForm(
            TraitService traitService,
            GeneService geneService,
            BreedingCalculationService breedingService,
            AnimalService animalService,
            RatAppDbContextFactory contextFactory)
            : base(contextFactory)
        {
            _traitService = traitService;
            _geneService = geneService;
            _breedingService = breedingService;
            _animalService = animalService;

            InitializeComponents();
            SetupLayout();
            RegisterEventHandlers();
            LoadAnimals();
        }

        private void InitializeComponents()
        {
            this.Text = "Breeding Calculator";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;

            parent1ComboBox = new ComboBox
            {
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            parent2ComboBox = new ComboBox
            {
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            calculateButton = new Button
            {
                Text = "Calculate",
                Width = 100,
                Height = 30
            };

            closeButton = new Button
            {
                Text = "Close",
                Width = 100,
                Height = 30,
                DialogResult = DialogResult.Cancel
            };

            compatibilityLabel = new Label
            {
                AutoSize = false,
                Width = 400,
                Height = 60,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5)
            };

            // Trait Probabilities Grid
            traitProbabilitiesGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            traitProbabilitiesGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Trait",
                    HeaderText = "Trait",
                    DataPropertyName = "Trait",
                    Width = 150
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Probability",
                    HeaderText = "Probability",
                    DataPropertyName = "Probability",
                    Width = 100
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Genotype",
                    HeaderText = "Genotype",
                    DataPropertyName = "Genotype",
                    Width = 150
                }
            });

            // Risk Analysis Grid
            riskAnalysisGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            riskAnalysisGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Risk",
                    HeaderText = "Risk Factor",
                    DataPropertyName = "Description",
                    Width = 200
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Probability",
                    HeaderText = "Probability",
                    DataPropertyName = "Probability",
                    Width = 100
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Management",
                    HeaderText = "Management",
                    DataPropertyName = "ManagementRecommendation",
                    Width = 200
                }
            });

            resultPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false
            };
        }

        private void SetupLayout()
        {
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(10)
            };

            // Parents selection
            var parent1Panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };
            parent1Panel.Controls.AddRange(new Control[]
            {
                new Label { Text = "Parent 1:", AutoSize = true },
                parent1ComboBox
            });
            mainLayout.Controls.Add(parent1Panel, 0, 0);

            var parent2Panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };
            parent2Panel.Controls.AddRange(new Control[]
            {
                new Label { Text = "Parent 2:", AutoSize = true },
                parent2ComboBox
            });
            mainLayout.Controls.Add(parent2Panel, 1, 0);

            // Calculate button
            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };
            buttonPanel.Controls.Add(calculateButton);
            mainLayout.Controls.Add(buttonPanel, 0, 1);
            mainLayout.SetColumnSpan(buttonPanel, 2);

            // Results section
            inbreedingLabel = new Label
            {
                AutoSize = false,
                Width = 400,
                Height = 30,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5)
            };

            var resultsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Visible = false
            };

            // Compatibility status
            resultsLayout.Controls.Add(new Label { Text = "Compatibility:", AutoSize = true }, 0, 0);
            resultsLayout.Controls.Add(compatibilityLabel, 1, 0);

            // Inbreeding coefficient
            resultsLayout.Controls.Add(new Label { Text = "Inbreeding Coefficient:", AutoSize = true }, 0, 1);
            resultsLayout.Controls.Add(inbreedingLabel, 1, 1);

            // Trait probabilities
            var probabilitiesPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            probabilitiesPanel.Controls.Add(new Label { Text = "Trait Probabilities", Dock = DockStyle.Top });
            probabilitiesPanel.Controls.Add(traitProbabilitiesGrid);
            resultsLayout.Controls.Add(probabilitiesPanel, 0, 1);

            // Risk analysis
            var riskPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            riskPanel.Controls.Add(new Label { Text = "Risk Analysis", Dock = DockStyle.Top });
            riskPanel.Controls.Add(riskAnalysisGrid);
            resultsLayout.Controls.Add(riskPanel, 1, 1);

            resultPanel.Controls.Add(resultsLayout);
            mainLayout.Controls.Add(resultPanel, 0, 2);
            mainLayout.SetColumnSpan(resultPanel, 2);

            // Close button
            var closeButtonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(5)
            };
            closeButtonPanel.Controls.Add(closeButton);

            this.Controls.Add(mainLayout);
            this.Controls.Add(closeButtonPanel);
        }

        private void RegisterEventHandlers()
        {
            calculateButton.Click += CalculateButton_Click;
            closeButton.Click += (s, e) => this.Close();
            parent1ComboBox.SelectedIndexChanged += Parent_SelectedIndexChanged;
            parent2ComboBox.SelectedIndexChanged += Parent_SelectedIndexChanged;
        }

        private async void LoadAnimals()
        {
            try
            {
                var animals = await _animalService.GetAllAnimalsAsync();
                
                parent1ComboBox.DataSource = new BindingSource(animals, null);
                parent1ComboBox.DisplayMember = "DisplayName";
                parent1ComboBox.ValueMember = "Id";

                parent2ComboBox.DataSource = new BindingSource(animals, null);
                parent2ComboBox.DisplayMember = "DisplayName";
                parent2ComboBox.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading animals: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void Parent_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (parent1ComboBox.SelectedItem != null && parent2ComboBox.SelectedItem != null)
            {
                var parent1 = (Animal)parent1ComboBox.SelectedItem;
                var parent2 = (Animal)parent2ComboBox.SelectedItem;

                var compatibility = await _breedingService.ValidateBreedingPairAsync(parent1.Id, parent2.Id);
                UpdateCompatibilityDisplay(compatibility);
            }
        }

        private void UpdateCompatibilityDisplay(BreedingCompatibilityResult compatibility)
        {
            if (compatibility.IsCompatible)
            {
                compatibilityLabel.Text = "Compatible";
                compatibilityLabel.ForeColor = Color.Green;
            }
            else
            {
                compatibilityLabel.Text = "Not Compatible";
                compatibilityLabel.ForeColor = Color.Red;
            }

            if (compatibility.Warnings.Any())
            {
                compatibilityLabel.Text += $"\nWarnings: {string.Join(", ", compatibility.Warnings)}";
            }

            if (compatibility.Risks.Any())
            {
                compatibilityLabel.Text += $"\nRisks: {string.Join(", ", compatibility.Risks)}";
            }
        }

        private async void CalculateButton_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                var parent1 = (Animal)parent1ComboBox.SelectedItem;
                var parent2 = (Animal)parent2ComboBox.SelectedItem;

                // Create new breeding calculation
                currentCalculation = await _breedingService.CreateBreedingCalculationAsync(0); // TODO: Create pairing first

                // Calculate offspring probabilities
                var offspring = await _breedingService.CalculateOffspringProbabilitiesAsync(currentCalculation.CalculationId);

                // Get trait probabilities
                var traitProbabilities = await _breedingService.GetTraitProbabilitiesAsync(currentCalculation.CalculationId);

                // Analyze genetic risks
                var risks = await _breedingService.AnalyzeGeneticRisksAsync(currentCalculation.CalculationId);

                // Calculate inbreeding coefficient
                var inbreedingCoefficient = await _breedingService.CalculateInbreedingCoefficientAsync(parent1.Id, parent2.Id);

                DisplayResults(traitProbabilities, risks, inbreedingCoefficient);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating breeding results: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayResults(Dictionary<string, float> traitProbabilities, List<InheritanceRisk> risks, double inbreedingCoefficient)
        {
            // Display inbreeding coefficient
            inbreedingLabel.Text = $"{inbreedingCoefficient:P2}";
            inbreedingLabel.ForeColor = inbreedingCoefficient > 0.125 ? Color.Red : Color.Green;

            // Display trait probabilities
            var traitRows = traitProbabilities.Select(tp => new
            {
                Trait = tp.Key,
                Probability = $"{tp.Value:P0}",
                Genotype = "..." // TODO: Get actual genotype
            }).ToList();

            traitProbabilitiesGrid.DataSource = traitRows;

            // Display risks
            riskAnalysisGrid.DataSource = risks;

            // Show results
            resultPanel.Visible = true;
        }

        private bool ValidateForm()
        {
            if (parent1ComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select Parent 1.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (parent2ComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select Parent 2.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (parent1ComboBox.SelectedValue.Equals(parent2ComboBox.SelectedValue))
            {
                MessageBox.Show("Please select different animals for Parent 1 and Parent 2.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}
