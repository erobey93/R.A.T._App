using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using RATAPPLibrary.Services;
using RATAPPLibrary.Services.Genetics;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Data.DbContexts;

namespace RATAPP.Panels
{
    public class GeneticsPanel : Panel
    {
        private readonly TraitService _traitService;
        private readonly GeneService _geneService;
        private readonly BreedingCalculationService _breedingService;
        private readonly RatAppDbContext _context;
        
        // UI Components
        private TabControl mainTabControl;
        private TabPage traitManagementTab;
        private TabPage analysisTab;
        private TabPage breedingTab;

        // Trait Management Components
        private DataGridView traitRegistryGrid;
        private Button addTraitButton;
        private Button editTraitButton;
        private Button assignTraitButton;
        private ComboBox traitTypeFilter;
        private ComboBox speciesFilter;

        // Analysis Components
        private Panel colonyStatsPanel;
        private Panel litterAnalysisPanel;
        private ComboBox analysisFilterCombo;
        private Button generateReportButton;

        // Breeding Components
        private ComboBox dam1Combo;
        private ComboBox dam2Combo;
        private Button calculateButton;
        private Panel resultPanel;

        public GeneticsPanel(TraitService traitService, GeneService geneService, BreedingCalculationService breedingService, RatAppDbContext context)
        {
            _traitService = traitService;
            _geneService = geneService;
            _breedingService = breedingService;
            _context = context;

            InitializeComponents();
            SetupLayout();
            RegisterEventHandlers();
        }

        private void InitializeComponents()
        {
            // Initialize main tab control
            mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Initialize tabs
            traitManagementTab = new TabPage("Trait Management");
            analysisTab = new TabPage("Analysis");
            breedingTab = new TabPage("Breeding");

            mainTabControl.TabPages.Add(traitManagementTab);
            mainTabControl.TabPages.Add(analysisTab);
            mainTabControl.TabPages.Add(breedingTab);

            // Initialize Trait Management components
            traitRegistryGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            traitRegistryGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "CommonName", HeaderText = "Name", DataPropertyName = "CommonName" },
                new DataGridViewTextBoxColumn { Name = "TraitType", HeaderText = "Type", DataPropertyName = "TraitType.Name" },
                new DataGridViewTextBoxColumn { Name = "Genotype", HeaderText = "Genotype", DataPropertyName = "Genotype" },
                new DataGridViewTextBoxColumn { Name = "Species", HeaderText = "Species", DataPropertyName = "Species.CommonName" }
            });

            addTraitButton = new Button
            {
                Text = "Add Trait",
                Width = 100,
                Height = 30
            };

            editTraitButton = new Button
            {
                Text = "Edit Trait",
                Width = 100,
                Height = 30
            };

            assignTraitButton = new Button
            {
                Text = "Assign Trait",
                Width = 100,
                Height = 30
            };

            traitTypeFilter = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            speciesFilter = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            // Initialize Analysis components
            colonyStatsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 300
            };

            litterAnalysisPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            analysisFilterCombo = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            generateReportButton = new Button
            {
                Text = "Generate Report",
                Width = 120,
                Height = 30
            };

            // Initialize Breeding components
            dam1Combo = new ComboBox
            {
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            dam2Combo = new ComboBox
            {
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            calculateButton = new Button
            {
                Text = "Calculate Offspring",
                Width = 150,
                Height = 30
            };

            resultPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 300
            };
        }

        private void SetupLayout()
        {
            // Main panel setup
            this.Dock = DockStyle.Fill;
            this.Controls.Add(mainTabControl);

            // Trait Management tab layout
            var traitManagementLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3
            };

            var filterPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };

            filterPanel.Controls.AddRange(new Control[]
            {
                new Label { Text = "Type:", AutoSize = true },
                traitTypeFilter,
                new Label { Text = "Species:", AutoSize = true },
                speciesFilter
            });

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };

            buttonPanel.Controls.AddRange(new Control[]
            {
                addTraitButton,
                editTraitButton,
                assignTraitButton
            });

            traitManagementTab.Controls.Add(traitManagementLayout);
            traitManagementLayout.Controls.Add(filterPanel, 0, 0);
            traitManagementLayout.Controls.Add(traitRegistryGrid, 0, 1);
            traitManagementLayout.Controls.Add(buttonPanel, 0, 2);

            // Analysis tab layout
            analysisTab.Controls.Add(litterAnalysisPanel);
            analysisTab.Controls.Add(colonyStatsPanel);

            var analysisToolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5)
            };

            analysisToolbar.Controls.AddRange(new Control[]
            {
                new Label { Text = "Filter:", AutoSize = true },
                analysisFilterCombo,
                generateReportButton
            });

            analysisTab.Controls.Add(analysisToolbar);

            // Breeding tab layout
            var breedingLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10)
            };

            breedingLayout.Controls.Add(new Label { Text = "Parent 1:", AutoSize = true }, 0, 0);
            breedingLayout.Controls.Add(dam1Combo, 1, 0);
            breedingLayout.Controls.Add(new Label { Text = "Parent 2:", AutoSize = true }, 0, 1);
            breedingLayout.Controls.Add(dam2Combo, 1, 1);
            breedingLayout.Controls.Add(calculateButton, 1, 2);
            breedingLayout.Controls.Add(resultPanel, 0, 3);
            breedingLayout.SetColumnSpan(resultPanel, 2);

            breedingTab.Controls.Add(breedingLayout);
        }

        private void RegisterEventHandlers()
        {
            this.Load += GeneticsPanel_Load;
            addTraitButton.Click += AddTraitButton_Click;
            editTraitButton.Click += EditTraitButton_Click;
            assignTraitButton.Click += AssignTraitButton_Click;
            calculateButton.Click += CalculateButton_Click;
            generateReportButton.Click += GenerateReportButton_Click;
            traitTypeFilter.SelectedIndexChanged += Filter_Changed;
            speciesFilter.SelectedIndexChanged += Filter_Changed;
        }

        private async void GeneticsPanel_Load(object sender, EventArgs e)
        {
            await LoadTraitTypes();
            await LoadSpecies();
            await RefreshTraitGrid();
        }

        private async Task LoadTraitTypes()
        {
            var traitTypes = await _traitService.GetAllTraitTypesAsync();
            traitTypeFilter.DataSource = traitTypes;
            traitTypeFilter.DisplayMember = "Name";
            traitTypeFilter.ValueMember = "Id";
        }

        private async Task LoadSpecies()
        {
            try
            {
                var speciesService = new SpeciesService(_context);
                var species = await speciesService.GetAllSpeciesAsync();
                
                speciesFilter.DataSource = species;
                speciesFilter.DisplayMember = "CommonName";
                speciesFilter.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading species: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RefreshTraitGrid()
        {
            try
            {
                var traits = await _traitService.GetAllTraitsAsync();
                
                // Apply filters if selected
                if (traitTypeFilter.SelectedItem != null)
                {
                    var selectedTraitType = (TraitType)traitTypeFilter.SelectedItem;
                    traits = traits.Where(t => t.TraitTypeId == selectedTraitType.Id).ToList();
                }

                if (speciesFilter.SelectedItem != null)
                {
                    var selectedSpecies = (Species)speciesFilter.SelectedItem;
                    traits = traits.Where(t => t.SpeciesID == selectedSpecies.Id).ToList();
                }

                traitRegistryGrid.DataSource = traits;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing traits: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddTraitButton_Click(object sender, EventArgs e)
        {
            using (var form = new AddTraitForm(_traitService, _geneService))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    RefreshTraitGrid();
                }
            }
        }

        private void EditTraitButton_Click(object sender, EventArgs e)
        {
            if (traitRegistryGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a trait to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedTrait = (Trait)traitRegistryGrid.SelectedRows[0].DataBoundItem;
            using (var form = new AddTraitForm(_traitService, _geneService))
            {
                // TODO: Implement edit mode in AddTraitForm
                if (form.ShowDialog() == DialogResult.OK)
                {
                    RefreshTraitGrid();
                }
            }
        }

        private void AssignTraitButton_Click(object sender, EventArgs e)
        {
            using (var form = new AssignTraitForm(_traitService, _geneService, new AnimalService(_context)))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    RefreshTraitGrid();
                }
            }
        }

        private async void Filter_Changed(object sender, EventArgs e)
        {
            await RefreshTraitGrid();
        }

        private void CalculateButton_Click(object sender, EventArgs e)
        {
            using (var form = new BreedingCalculatorForm(
                _traitService,
                _geneService,
                _breedingService,
                new AnimalService(_context)))
            {
                form.ShowDialog();
            }
        }

        private async void GenerateReportButton_Click(object sender, EventArgs e)
        {
            try
            {
                var traits = await _traitService.GetAllTraitsAsync();
                var traitTypes = await _traitService.GetAllTraitTypesAsync();

                var report = new StringBuilder();
                report.AppendLine("Genetics Report");
                report.AppendLine("==============");
                report.AppendLine();

                // Trait Type Summary
                report.AppendLine("Trait Types Summary");
                report.AppendLine("-----------------");
                foreach (var traitType in traitTypes)
                {
                    var typeTraits = traits.Where(t => t.TraitTypeId == traitType.Id).ToList();
                    report.AppendLine($"{traitType.Name}: {typeTraits.Count} traits");
                }
                report.AppendLine();

                // Trait Details
                report.AppendLine("Trait Details");
                report.AppendLine("-------------");
                foreach (var trait in traits)
                {
                    report.AppendLine($"Name: {trait.CommonName}");
                    report.AppendLine($"Type: {trait.TraitType?.Name ?? "Unknown"}");
                    report.AppendLine($"Genotype: {trait.Genotype ?? "Not specified"}");
                    report.AppendLine();
                }

                // Save report
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    saveDialog.FilterIndex = 1;
                    saveDialog.DefaultExt = "txt";
                    saveDialog.FileName = $"GeneticsReport_{DateTime.Now:yyyyMMdd}";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        await File.WriteAllTextAsync(saveDialog.FileName, report.ToString());
                        MessageBox.Show("Report generated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
