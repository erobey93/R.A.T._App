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
using RATAPP.Forms;

namespace RATAPP.Panels
{
    public class GeneticsPanel : Panel, INavigable
    {
        private readonly TraitService _traitService;
        private readonly GeneService _geneService;
        private readonly BreedingCalculationService _breedingService;
        private readonly AnimalService _animalService;
        private readonly RatAppDbContextFactory _contextFactory;
        private readonly RATAppBaseForm _baseForm;

        // UI Components
        private Panel headerPanel;
        private TabControl mainTabControl;
        private TabPage traitManagementTab;
        private TabPage analysisTab;
        private TabPage breedingCalculatorTab;
        private TabPage pedigreeTab;

        // Trait Management Components
        private DataGridView traitRegistryGrid;
        private Button addTraitButton;
        private Button editTraitButton;
        private Button deleteTraitButton;
        private Button assignTraitButton;
        private ComboBox traitTypeFilter;
        private ComboBox speciesFilter;
        private Label filterLabel;

        // Analysis Components
        private Panel colonyStatsPanel;
        private Panel litterAnalysisPanel;
        private ComboBox analysisFilterCombo;
        private Button generateReportButton;
        private Label analysisDescriptionLabel;

        // Breeding Calculator Components
        private ComboBox dam1Combo;
        private ComboBox sire1Combo;
        private Button calculateButton;
        private Panel resultPanel;
        private Label breedingDescriptionLabel;
        private Label dam1Label;
        private Label sire1Label;

        // Pedigree Components
        private ComboBox animalSelector;
        private Panel pedigreeDisplayPanel;
        private Button viewPedigreeButton;
        private Label pedigreeDescriptionLabel;

        public Action<object, EventArgs> Load { get; private set; }

        public GeneticsPanel(RATAppBaseForm baseForm, TraitService traitService, GeneService geneService,
                            BreedingCalculationService breedingService, RatAppDbContextFactory contextFactory)
        {
            _baseForm = baseForm;
            _traitService = traitService;
            _geneService = geneService;
            _breedingService = breedingService;
            _animalService = new AnimalService(contextFactory); //TODO look at the difference between this implementation and how I've been implementing the service 
            _contextFactory = contextFactory;

            InitializeComponents();
            RegisterEventHandlers();
        }

        private async void InitializeComponents()
        {
            // Main panel setup
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // Create header panel
            InitializeHeaderPanel();

            // Initialize main tab control
            mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11),
                Padding = new Point(12, 4)
            };

            // Initialize tabs
            traitManagementTab = new TabPage("Trait Management");
            analysisTab = new TabPage("Genetic Analysis");
            breedingCalculatorTab = new TabPage("Breeding Calculator");
            pedigreeTab = new TabPage("Pedigree Viewer");

            InitializeTraitManagementTab();
            InitializeAnalysisTab();
            InitializeBreedingCalculatorTab();
            InitializePedigreeTab();

            mainTabControl.TabPages.Add(traitManagementTab);
            mainTabControl.TabPages.Add(analysisTab);
            mainTabControl.TabPages.Add(breedingCalculatorTab);
            mainTabControl.TabPages.Add(pedigreeTab);

            // Add controls to main panel
            this.Controls.Add(mainTabControl);
            this.Controls.Add(headerPanel);

            //get the data when the page loads
            await RefreshDataAsync();
        }

        private void InitializeHeaderPanel()
        {
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(0, 120, 212)
            };

            Label titleLabel = new Label
            {
                Text = "Genetics Management",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 10)
            };

            Label descriptionLabel = new Label
            {
                Text = "Manage genetic traits, analyze breeding outcomes, and view pedigrees",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(25, 40)
            };

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(descriptionLabel);
        }

        private void InitializeTraitManagementTab()
        {
            // Create container panel with padding
            Panel containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Create filter section
            Panel filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60
            };

            filterLabel = new Label
            {
                Text = "Filter by:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(0, 10)
            };

            traitTypeFilter = new ComboBox
            {
                Width = 200,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(100, 8)
            };

            Label speciesLabel = new Label
            {
                Text = "Species:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(320, 10)
            };

            speciesFilter = new ComboBox
            {
                Width = 200,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(380, 8)
            };

            filterPanel.Controls.Add(filterLabel);
            filterPanel.Controls.Add(traitTypeFilter);
            filterPanel.Controls.Add(speciesLabel);
            filterPanel.Controls.Add(speciesFilter);

            // Create trait registry grid
            traitRegistryGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                Font = new Font("Segoe UI", 9)
            };
            traitRegistryGrid.Columns.AddRange(new DataGridViewColumn[]
            {
    new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "Id", HeaderText = "ID", Width = 60 },
    new DataGridViewTextBoxColumn { Name = "CommonName", DataPropertyName = "CommonName", HeaderText = "Name", Width = 150 },
    new DataGridViewTextBoxColumn { Name = "TraitType", DataPropertyName = "TraitType", HeaderText = "Type", Width = 120 },
    new DataGridViewTextBoxColumn { Name = "Genotype", DataPropertyName = "Genotype", HeaderText = "Genotype", Width = 120 },
    new DataGridViewTextBoxColumn { Name = "Species", DataPropertyName = "Species", HeaderText = "Species", Width = 120 },
    new DataGridViewTextBoxColumn { Name = "Description", DataPropertyName = "Description", HeaderText = "Description", Width = 200 }
            });

            // Create button panel
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(0, 10, 0, 0)
            };

            addTraitButton = CreateButton("Add Trait", 0);
            editTraitButton = CreateButton("Edit Trait", 110);
            deleteTraitButton = CreateButton("Delete Trait", 220);
            assignTraitButton = CreateButton("Assign to Animal", 330);

            buttonPanel.Controls.Add(addTraitButton);
            buttonPanel.Controls.Add(editTraitButton);
            buttonPanel.Controls.Add(deleteTraitButton);
            buttonPanel.Controls.Add(assignTraitButton);

            // Add info panel
            Panel infoPanel = CreateInfoPanel(
                "Trait Management",
                "• Add, edit, and delete genetic traits in your colony\n" +
                "• Assign traits to individual animals\n" +
                "• Filter traits by type and species\n" +
                "• Traits are used in breeding calculations and pedigree analysis"
            );
            infoPanel.Dock = DockStyle.Top;
            infoPanel.Height = 120;
            infoPanel.Margin = new Padding(0, 0, 0, 10);

            // Add all components to container
            containerPanel.Controls.Add(traitRegistryGrid);
            containerPanel.Controls.Add(buttonPanel);
            containerPanel.Controls.Add(filterPanel);
            containerPanel.Controls.Add(infoPanel);

            traitManagementTab.Controls.Add(containerPanel);
        }

        private void InitializeAnalysisTab()
        {
            // Create container panel with padding
            Panel containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Create description label
            analysisDescriptionLabel = new Label
            {
                Text = "Analyze genetic traits across your colony and breeding lines",
                Font = new Font("Segoe UI", 12),
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 10)
            };

            // Create filter panel
            Panel filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50
            };

            Label filterLabel = new Label
            {
                Text = "Analysis Type:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(0, 15)
            };

            analysisFilterCombo = new ComboBox
            {
                Width = 250,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(100, 12)
            };
            analysisFilterCombo.Items.AddRange(new object[] {
                "Colony Trait Distribution",
                "Breeding Line Analysis",
                "Inheritance Patterns",
                "Trait Frequency by Generation"
            });
            analysisFilterCombo.SelectedIndex = 0;

            generateReportButton = new Button
            {
                Text = "Generate Report",
                Font = new Font("Segoe UI", 10),
                Width = 150,
                Height = 30,
                Location = new Point(370, 10),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            generateReportButton.FlatAppearance.BorderSize = 0;

            filterPanel.Controls.Add(filterLabel);
            filterPanel.Controls.Add(analysisFilterCombo);
            filterPanel.Controls.Add(generateReportButton);

            // Create stats panel
            colonyStatsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 200,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 10, 0, 10)
            };

            Label statsTitle = new Label
            {
                Text = "Colony Statistics",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            // Placeholder stats (would be populated with real data)
            Label statsContent = new Label
            {
                Text = "Total Animals: 0\nTotal Traits: 0\nMost Common Trait: N/A",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(10, 40)
            };

            colonyStatsPanel.Controls.Add(statsTitle);
            colonyStatsPanel.Controls.Add(statsContent);

            // Create analysis panel
            litterAnalysisPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label analysisTitle = new Label
            {
                Text = "Detailed Analysis",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            // Placeholder for analysis content
            Label analysisContent = new Label
            {
                Text = "Select an analysis type and click 'Generate Report' to view detailed analysis.",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(10, 40)
            };

            litterAnalysisPanel.Controls.Add(analysisTitle);
            litterAnalysisPanel.Controls.Add(analysisContent);

            // Add all components to container
            containerPanel.Controls.Add(litterAnalysisPanel);
            containerPanel.Controls.Add(colonyStatsPanel);
            containerPanel.Controls.Add(filterPanel);
            containerPanel.Controls.Add(analysisDescriptionLabel);

            analysisTab.Controls.Add(containerPanel);
        }

        private void InitializeBreedingCalculatorTab()
        {
            // Create container panel with padding
            Panel containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Create description label
            breedingDescriptionLabel = new Label
            {
                Text = "Calculate potential offspring traits based on parent genetics",
                Font = new Font("Segoe UI", 12),
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 20)
            };

            // Create parent selection panel
            Panel parentPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120
            };

            // Dam selection
            dam1Label = new Label
            {
                Text = "Dam (Female):",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(0, 15)
            };

            dam1Combo = new ComboBox
            {
                Width = 250,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(120, 12)
            };

            // Sire selection
            sire1Label = new Label
            {
                Text = "Sire (Male):",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(0, 55)
            };

            sire1Combo = new ComboBox
            {
                Width = 250,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(120, 52)
            };

            calculateButton = new Button
            {
                Text = "Calculate Offspring",
                Font = new Font("Segoe UI", 10),
                Width = 150,
                Height = 30,
                Location = new Point(120, 90),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            calculateButton.FlatAppearance.BorderSize = 0;

            parentPanel.Controls.Add(dam1Label);
            parentPanel.Controls.Add(dam1Combo);
            parentPanel.Controls.Add(sire1Label);
            parentPanel.Controls.Add(sire1Combo);
            parentPanel.Controls.Add(calculateButton);

            // Create results panel
            resultPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label resultTitle = new Label
            {
                Text = "Breeding Results",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            // Placeholder for results
            Label resultContent = new Label
            {
                Text = "Select parents and click 'Calculate Offspring' to view potential offspring traits.",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(10, 40)
            };

            resultPanel.Controls.Add(resultTitle);
            resultPanel.Controls.Add(resultContent);

            // Add info panel
            Panel infoPanel = CreateInfoPanel(
                "Breeding Calculator",
                "• Select dam and sire to calculate potential offspring traits\n" +
                "• Results show probability of each trait appearing in offspring\n" +
                "• Calculations are based on Mendelian inheritance\n" +
                "• Only animals with assigned traits will show accurate results"
            );
            infoPanel.Dock = DockStyle.Top;
            infoPanel.Height = 120;
            infoPanel.Margin = new Padding(0, 0, 0, 10);

            // Add all components to container
            containerPanel.Controls.Add(resultPanel);
            containerPanel.Controls.Add(parentPanel);
            containerPanel.Controls.Add(infoPanel);
            containerPanel.Controls.Add(breedingDescriptionLabel);

            breedingCalculatorTab.Controls.Add(containerPanel);
        }

        private void InitializePedigreeTab()
        {
            // Create container panel with padding
            Panel containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Create description label
            pedigreeDescriptionLabel = new Label
            {
                Text = "View and analyze animal pedigrees and ancestry",
                Font = new Font("Segoe UI", 12),
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 20)
            };

            // Create animal selection panel
            Panel selectionPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60
            };

            Label animalLabel = new Label
            {
                Text = "Select Animal:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(0, 15)
            };

            animalSelector = new ComboBox
            {
                Width = 250,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(100, 12)
            };

            viewPedigreeButton = new Button
            {
                Text = "View Pedigree",
                Font = new Font("Segoe UI", 10),
                Width = 120,
                Height = 30,
                Location = new Point(370, 10),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            viewPedigreeButton.FlatAppearance.BorderSize = 0;

            selectionPanel.Controls.Add(animalLabel);
            selectionPanel.Controls.Add(animalSelector);
            selectionPanel.Controls.Add(viewPedigreeButton);

            // Create pedigree display panel
            pedigreeDisplayPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label pedigreeTitle = new Label
            {
                Text = "Pedigree View",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            // Placeholder for pedigree
            Label pedigreeContent = new Label
            {
                Text = "Select an animal and click 'View Pedigree' to display the pedigree chart.",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(10, 40)
            };

            pedigreeDisplayPanel.Controls.Add(pedigreeTitle);
            pedigreeDisplayPanel.Controls.Add(pedigreeContent);

            // Add info panel
            Panel infoPanel = CreateInfoPanel(
                "Pedigree Viewer",
                "• View ancestry information for any animal in your colony\n" +
                "• Trace genetic traits through multiple generations\n" +
                "• Identify common ancestors and breeding lines\n" +
                "• Export pedigree charts for documentation"
            );
            infoPanel.Dock = DockStyle.Top;
            infoPanel.Height = 120;
            infoPanel.Margin = new Padding(0, 0, 0, 10);

            // Add all components to container
            containerPanel.Controls.Add(pedigreeDisplayPanel);
            containerPanel.Controls.Add(selectionPanel);
            containerPanel.Controls.Add(infoPanel);
            containerPanel.Controls.Add(pedigreeDescriptionLabel);

            pedigreeTab.Controls.Add(containerPanel);
        }

        private Button CreateButton(string text, int x)
        {
            Button button = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                Location = new Point(x, 0),
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private Panel CreateInfoPanel(string title, string content)
        {
            Panel panel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };

            Label titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            Label contentLabel = new Label
            {
                Text = content,
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Location = new Point(10, 35),
                MaximumSize = new Size(800, 0)
            };

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(contentLabel);
            return panel;
        }

        private void RegisterEventHandlers()
        {
            this.Load += GeneticsPanel_Load;
            addTraitButton.Click += AddTraitButton_Click;
            editTraitButton.Click += EditTraitButton_Click;
            deleteTraitButton.Click += DeleteTraitButton_Click;
            assignTraitButton.Click += AssignTraitButton_Click;
            calculateButton.Click += CalculateButton_Click;
            generateReportButton.Click += GenerateReportButton_Click;
            viewPedigreeButton.Click += ViewPedigreeButton_Click;
            traitTypeFilter.SelectedIndexChanged += Filter_Changed;
            speciesFilter.SelectedIndexChanged += Filter_Changed;
        }

        private async void GeneticsPanel_Load(object sender, EventArgs e)
        {
            await LoadTraitTypes();
            await LoadSpecies();
            await LoadAnimals();
            await RefreshTraitGrid();
        }

        private async Task LoadTraitTypes()
        {
            try
            {
                var traitTypes = await _traitService.GetAllTraitTypesAsync();
                traitTypeFilter.Items.Clear();
                traitTypeFilter.Items.Add("All Types");

                foreach (var type in traitTypes)
                {
                    traitTypeFilter.Items.Add(type);
                }

                traitTypeFilter.SelectedIndex = 0;
                traitTypeFilter.DisplayMember = "Name";
                traitTypeFilter.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading trait types: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadSpecies()
        {
            try
            {
                var speciesService = new SpeciesService(_contextFactory);
                var species = await speciesService.GetAllSpeciesObjectsAsync();

                speciesFilter.Items.Clear();
                speciesFilter.Items.Add("All Species");

                foreach (var s in species)
                {
                    speciesFilter.Items.Add(s);
                }

                speciesFilter.SelectedIndex = 0;
                speciesFilter.DisplayMember = "CommonName";
                speciesFilter.ValueMember = "Id";
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
                var animalService = new AnimalService(_contextFactory);
                var animals = await animalService.GetAllAnimalsAsync();

                // Populate dam and sire combos for breeding calculator
                dam1Combo.Items.Clear();
                sire1Combo.Items.Clear();
                animalSelector.Items.Clear();

                // Add all animals to the pedigree selector
                foreach (var animal in animals)
                {
                    animalSelector.Items.Add(animal);

                    // Add females to dam combo
                    if (animal.sex == "Female")
                    {
                        dam1Combo.Items.Add(animal);
                    }
                    // Add males to sire combo
                    else if (animal.sex == "Male")
                    {
                        sire1Combo.Items.Add(animal);
                    }
                }

                dam1Combo.DisplayMember = "Name";
                dam1Combo.ValueMember = "Id";
                sire1Combo.DisplayMember = "Name";
                sire1Combo.ValueMember = "Id";
                animalSelector.DisplayMember = "Name";
                animalSelector.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading animals: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RefreshTraitGrid()
        {
            try
            {
                var traits = await _traitService.GetAllTraitsAsync();

                // Apply filters if selected
                if (traitTypeFilter.SelectedIndex > 0)
                {
                    var selectedTraitType = (TraitType)traitTypeFilter.SelectedItem;
                    traits = traits.Where(t => t.TraitTypeId == selectedTraitType.Id).ToList();
                }

                if (speciesFilter.SelectedIndex > 0)
                {
                    var selectedSpecies = (Species)speciesFilter.SelectedItem;
                    traits = traits.Where(t => t.SpeciesID == selectedSpecies.Id).ToList();
                }

                // Flatten the data for display
                var displayData = traits.Select(t => new
                {
                    t.Id,
                    t.CommonName,
                    TraitType = t.TraitType?.Name ?? "Unknown", // Display the name or "Unknown" if null
                    t.Genotype,
                    Species = t.Species?.CommonName ?? "Unknown", // Display the common name or "Unknown" if null
                }).ToList();

                // Bind the flattened data to the grid
                traitRegistryGrid.DataSource = null;
                traitRegistryGrid.DataSource = displayData;

                // Ensure the column display names match the new properties
                if (traitRegistryGrid.Columns.Contains("TraitType"))
                {
                    traitRegistryGrid.Columns["TraitType"].HeaderText = "Type";
                }

                if (traitRegistryGrid.Columns.Contains("Species"))
                {
                    traitRegistryGrid.Columns["Species"].HeaderText = "Species";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing traits: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private async void AddTraitButton_Click(object sender, EventArgs e)
        {
            using (var form = await AddTraitForm.CreateAsync(_contextFactory, _traitService, _geneService))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    await RefreshTraitGrid();
                }
            }
        }

        //TODO
        private void EditTraitButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Edit Trait Functionality Not Yet Implemented"); 
            //if (traitRegistryGrid.SelectedRows.Count == 0)
            //{
            //    MessageBox.Show("Please select a trait to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            //var selectedTrait = (Trait)traitRegistryGrid.SelectedRows[0].DataBoundItem;
            //using (var form = new AddTraitForm(_traitService, _geneService, _contextFactory)) //FIXME took , selectedTrait out
            //{
            //    if (form.ShowDialog() == DialogResult.OK)
            //    {
            //        RefreshTraitGrid();
            //    }
            //}
        }

        //TODO
        private void DeleteTraitButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Delete Trait Functionality Not Yet Implemented");
            //if (traitRegistryGrid.SelectedRows.Count == 0)
            //{
            //    MessageBox.Show("Please select a trait to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            //var selectedTrait = (Trait)traitRegistryGrid.SelectedRows[0].DataBoundItem;

            //DialogResult result = MessageBox.Show(
            //    $"Are you sure you want to delete the trait '{selectedTrait.CommonName}'?",
            //    "Confirm Deletion",
            //    MessageBoxButtons.YesNo,
            //    MessageBoxIcon.Question
            //);

            //if (result == DialogResult.Yes)
            //{
            //    try
            //    {
            //        _traitService.DeleteTraitAsync(selectedTrait.Id);
            //        RefreshTraitGrid();
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show($"Error deleting trait: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    }
            //}
        }

        private async void AssignTraitButton_Click(object sender, EventArgs e)
        {
            if (traitRegistryGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a trait to assign.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRow = traitRegistryGrid.SelectedRows[0];
            var traitId = (int)selectedRow.Cells["Id"].Value;
            
            try
            {
                var trait = await _traitService.GetTraitByIdAsync(traitId);
                if (trait == null)
                {
                    MessageBox.Show("Selected trait not found.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (var form = new AssignTraitForm(_traitService, _geneService, 
                    new AnimalService(_contextFactory), _contextFactory, trait))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        await RefreshTraitGrid();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading trait: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void Filter_Changed(object sender, EventArgs e)
        {
            await RefreshTraitGrid();
        }

        private async void CalculateButton_Click(object sender, EventArgs e)
        {
            if (dam1Combo.SelectedItem == null || sire1Combo.SelectedItem == null)
            {
                MessageBox.Show("Please select both a dam and sire for breeding calculation.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dam = dam1Combo.SelectedItem as AnimalDto;
            var sire = sire1Combo.SelectedItem as AnimalDto;

            // Clear previous results
            resultPanel.Controls.Clear();

            Label resultTitle = new Label
            {
                Text = "Breeding Results",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            resultPanel.Controls.Add(resultTitle);

            try
            {
                //get the animalDto objects for dam and sire back as Animal objects
                var getDamAsAnimalObj = await _animalService.MapAnimalDtoBackToAnimal(dam);
                var getSireAsAnimalObj = await _animalService.MapAnimalDtoBackToAnimal(sire);

                var damAsAnimalObj = getDamAsAnimalObj;
                var sireAsAnimalObj = getSireAsAnimalObj;

                // Calculate Possible Outcomes
                var results = _breedingService.CalculateBreedingOutcomes(damAsAnimalObj, sireAsAnimalObj); //IDK why this was here because we want the possible phenotype/genotype outcomes 
                //var results = await _breedingService.CalculateOffspringProbabilitiesAsync();

                if (results == null || !results.Any())
                {
                    Label noResultsLabel = new Label
                    {
                        Text = "No genetic traits found for these animals or they are incompatible for breeding.",
                        Font = new Font("Segoe UI", 10),
                        AutoSize = true,
                        Location = new Point(10, 40)
                    };
                    resultPanel.Controls.Add(noResultsLabel);
                    return;
                }

                // Display results in a formatted panel
                Panel resultsContainer = new Panel
                {
                    AutoScroll = true,
                    Location = new Point(10, 40),
                    Size = new Size(resultPanel.Width - 20, resultPanel.Height - 50),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
                };
                resultPanel.Controls.Add(resultsContainer);

                int yPos = 10;
                foreach (var result in results)
                {
                    Panel traitPanel = new Panel
                    {
                        BorderStyle = BorderStyle.FixedSingle,
                        Location = new Point(0, yPos),
                        Size = new Size(resultsContainer.Width - 20, 100),
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    };

                    Label traitLabel = new Label
                    {
                        Text = result.ToString(),
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        AutoSize = true,
                        Location = new Point(10, 10)
                    };

                    Label probabilityLabel = new Label
                    {
                        Text = $"Probability: {result.ToString():P}", //FIXME no clue what's going on here was Probability
                        Font = new Font("Segoe UI", 9),
                        AutoSize = true,
                        Location = new Point(10, 35)
                    };

                    Label genotypeLabel = new Label
                    {
                        Text = $"Possible Genotypes: {result.ToString()}", //was PossibleGenoType FIXME not sure what the inteniton was
                        Font = new Font("Segoe UI", 9),
                        AutoSize = true,
                        Location = new Point(10, 60)
                    };

                    traitPanel.Controls.Add(traitLabel);
                    traitPanel.Controls.Add(probabilityLabel);
                    traitPanel.Controls.Add(genotypeLabel);

                    resultsContainer.Controls.Add(traitPanel);
                    yPos += 110; // Add spacing between trait panels
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating breeding outcomes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //TODO
        private async void GenerateReportButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Generate Report Functionality Not Yet Implemented");
            //try
            //{
            //    var traits = await _traitService.GetAllTraitsAsync();
            //    var traitTypes = await _traitService.GetAllTraitTypesAsync();

            //    var report = new StringBuilder();
            //    report.AppendLine("Genetics Report");
            //    report.AppendLine("==============");
            //    report.AppendLine($"Generated: {DateTime.Now}");
            //    report.AppendLine();

            //    // Trait Type Summary
            //    report.AppendLine("Trait Types Summary");
            //    report.AppendLine("-----------------");
            //    foreach (var traitType in traitTypes)
            //    {
            //        var typeTraits = traits.Where(t => t.TraitTypeId == traitType.Id).ToList();
            //        report.AppendLine($"{traitType.Name}: {typeTraits.Count} traits");
            //    }
            //    report.AppendLine();

            //    // Trait Details
            //    report.AppendLine("Trait Details");
            //    report.AppendLine("-------------");
            //    foreach (var trait in traits)
            //    {
            //        report.AppendLine($"Name: {trait.CommonName}");
            //        report.AppendLine($"Type: {trait.TraitType?.Name ?? "Unknown"}");
            //        report.AppendLine($"Genotype: {trait.Genotype ?? "Not specified"}");
            //        report.AppendLine($"Description: {trait.TraitType.Name ?? "None"}"); //FIXME
            //        report.AppendLine();
            //    }

            //    // Save report
            //    using (var saveDialog = new SaveFileDialog())
            //    {
            //        saveDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            //        saveDialog.FilterIndex = 1;
            //        saveDialog.DefaultExt = "txt";
            //        saveDialog.FileName = $"GeneticsReport_{DateTime.Now:yyyyMMdd}";

            //        if (saveDialog.ShowDialog() == DialogResult.OK)
            //        {
            //            await File.WriteAllTextAsync(saveDialog.FileName, report.ToString());
            //            MessageBox.Show("Report generated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        private void ViewPedigreeButton_Click(object sender, EventArgs e)
        {
            if (animalSelector.SelectedItem == null)
            {
                MessageBox.Show("Please select an animal to view its pedigree.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedAnimal = animalSelector.SelectedItem as AnimalDto;
           

            // Clear previous pedigree
            pedigreeDisplayPanel.Controls.Clear();

            Label pedigreeTitle = new Label
            {
                Text = $"Pedigree for {selectedAnimal.name}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            pedigreeDisplayPanel.Controls.Add(pedigreeTitle);

            try
            {
                //var animal = _animalService.MapSingleAnimaltoDto(selectedAnimal);
                //var animalResult = animal.Result;
                var animal = animalSelector.SelectedItem as RATAPPLibrary.Data.Models.AnimalDto; 

                // Create a new form to display the pedigree in a larger view
                var pedigreeForm = PedigreeForm.Create( _contextFactory, animal);

                // Add the form to the pedigree panel
                pedigreeForm.TopLevel = false;
                pedigreeForm.FormBorderStyle = FormBorderStyle.None;
                pedigreeForm.Dock = DockStyle.Fill;

                pedigreeDisplayPanel.Controls.Add(pedigreeForm);
                pedigreeForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying pedigree: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task RefreshDataAsync()
        {
            await LoadTraitTypes();
            await LoadSpecies();
            await LoadAnimals();
            await RefreshTraitGrid();
        }
    }
}
