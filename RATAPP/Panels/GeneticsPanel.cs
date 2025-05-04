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
using RATAPP.Helpers;

namespace RATAPP.Panels
{
    public class GeneticsPanel : ResponsivePanel
    {
        private readonly TraitService _traitService;
        private readonly GeneService _geneService;
        private readonly BreedingCalculationService _breedingService;
        private readonly AnimalService _animalService; 
        private readonly RatAppDbContext _context;

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

        // Event handler for Load event
        private EventHandler loadHandler; //what is the difference between this and the implementation below this? 

        public Action<object, EventArgs> Load { get; private set; }

        //TODO look at this implementation vs. what I've been doing i.e. with the services 
        //This is likely not set up correctly, which is why I'm not seeing data 
        public GeneticsPanel(RATAppBaseForm baseForm, TraitService traitService, GeneService geneService,
                            BreedingCalculationService breedingService, AnimalService animalService, RatAppDbContext context)
            : base(baseForm)
        {
            _traitService = traitService;
            _geneService = geneService;
            _breedingService = breedingService;
            _animalService = animalService;
            _context = context;

            InitializeComponents();
            RegisterEventHandlers();
            _initialized = true;
        }

        private void InitializeComponents()
        {
            // Main panel setup
            this.BackColor = Color.White;

            // Create header panel
            InitializeHeaderPanel();

            // Initialize main tab control
            mainTabControl = ResponsiveFormComponentFactory.CreateResponsiveTabControl();

            // Initialize tabs
            traitManagementTab = ResponsiveFormComponentFactory.CreateResponsiveTabPage("Trait Management");
            analysisTab = ResponsiveFormComponentFactory.CreateResponsiveTabPage("Genetic Analysis");
            breedingCalculatorTab = ResponsiveFormComponentFactory.CreateResponsiveTabPage("Breeding Calculator");
            pedigreeTab = ResponsiveFormComponentFactory.CreateResponsiveTabPage("Pedigree Viewer");

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
        }

        private void InitializeHeaderPanel()
        {
            headerPanel = ResponsiveFormComponentFactory.CreateResponsiveHeaderPanel("Genetics Management");

            Label descriptionLabel = new Label
            {
                Text = "Manage genetic traits, analyze breeding outcomes, and view pedigrees",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(25, 40)
            };

            headerPanel.Controls.Add(descriptionLabel);
        }

        private void InitializeTraitManagementTab()
        {
            // Create container panel with padding
            Panel containerPanel = ResponsiveFormComponentFactory.CreateResponsivePanel();
            containerPanel.Padding = new Padding(20);

            // Create filter section
            Panel filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                AutoSize = true
            };

            filterLabel = new Label
            {
                Text = "Filter by:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };

            traitTypeFilter = new ComboBox
            {
                Width = 200,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };

            Label speciesLabel = new Label
            {
                Text = "Species:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };

            speciesFilter = new ComboBox
            {
                Width = 200,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };

            // Use TableLayoutPanel for responsive layout
            TableLayoutPanel filterTableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                AutoSize = true
            };

            filterTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            filterTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            filterTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            filterTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            filterTableLayout.Controls.Add(filterLabel, 0, 0);
            filterTableLayout.Controls.Add(traitTypeFilter, 1, 0);
            filterTableLayout.Controls.Add(speciesLabel, 2, 0);
            filterTableLayout.Controls.Add(speciesFilter, 3, 0);

            filterPanel.Controls.Add(filterTableLayout);

            // Create trait registry grid
            traitRegistryGrid = ResponsiveFormComponentFactory.CreateResponsiveDataGridView();
            traitRegistryGrid.AutoGenerateColumns = false;
            traitRegistryGrid.MultiSelect = false;
            traitRegistryGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            traitRegistryGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "CommonName", HeaderText = "Name", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "TraitType", HeaderText = "Type", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "Genotype", HeaderText = "Genotype", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "Species", HeaderText = "Species", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", Width = 200 }
            });

            // Create button panel
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 0)
            };

            addTraitButton = CreateStyledButton("Add Trait");
            editTraitButton = CreateStyledButton("Edit Trait");
            deleteTraitButton = CreateStyledButton("Delete Trait");
            assignTraitButton = CreateStyledButton("Assign to Animal");

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
            Panel containerPanel = ResponsiveFormComponentFactory.CreateResponsivePanel();
            containerPanel.Padding = new Padding(20);

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
            TableLayoutPanel filterPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 3,
                RowCount = 1,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };

            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            filterPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            Label filterLabel = new Label
            {
                Text = "Analysis Type:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top, // Or just AnchorStyles.Left, depending on layout FIXME if UI is off come back here 
                TextAlign = ContentAlignment.MiddleLeft // Align text to the middle left
            };

            analysisFilterCombo = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Left | AnchorStyles.Top, // Or just AnchorStyles.Left, depending on layout FIXME if UI is off come back here 
                //TextAlign = ContentAlignment.MiddleLeft // Align text to the middle left FIXME if UI is off, come back here for combo boxes 
            };

            analysisFilterCombo.Items.AddRange(new object[] {
                "Colony Trait Distribution",
                "Breeding Line Analysis",
                "Inheritance Patterns",
                "Trait Frequency by Generation"
            });
            analysisFilterCombo.SelectedIndex = 0;

            generateReportButton = CreateStyledButton("Generate Report");
            generateReportButton.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            generateReportButton.TextAlign = ContentAlignment.MiddleLeft; 

            filterPanel.Controls.Add(filterLabel, 0, 0);
            filterPanel.Controls.Add(analysisFilterCombo, 1, 0);
            filterPanel.Controls.Add(generateReportButton, 2, 0);

            // Create stats panel
            colonyStatsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 200,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 10, 0, 10),
                AutoSize = true
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
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
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
            Panel containerPanel = ResponsiveFormComponentFactory.CreateResponsivePanel();
            containerPanel.Padding = new Padding(20);

            // Create description label
            breedingDescriptionLabel = new Label
            {
                Text = "Calculate potential offspring traits based on parent genetics",
                Font = new Font("Segoe UI", 12),
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 20)
            };

            // Create parent selection panel using TableLayoutPanel for responsive layout
            TableLayoutPanel parentPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 3,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };

            // Set column and row styles
            parentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            parentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            parentPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            parentPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            parentPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Dam selection
            dam1Label = new Label
            {
                Text = "Dam (Female):",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top, // Or just AnchorStyles.Left, depending on layout FIXME if UI is off come back here 
                TextAlign = ContentAlignment.MiddleLeft // Align text to the middle left
            };

            dam1Combo = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Left | AnchorStyles.Top //FIXME come back here if comboBox UI is off 
            };

            // Sire selection
            sire1Label = new Label
            {
                Text = "Sire (Male):",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top, // Or just AnchorStyles.Left, depending on layout FIXME if UI is off come back here 
                TextAlign = ContentAlignment.MiddleLeft // Align text to the middle left
            };

            sire1Combo = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Left | AnchorStyles.Top //FIXME come back here if comboBox UI is off 
            };

            calculateButton = CreateStyledButton("Calculate Offspring");
            calculateButton.Anchor = AnchorStyles.Left;

            // Add controls to the table layout
            parentPanel.Controls.Add(dam1Label, 0, 0);
            parentPanel.Controls.Add(dam1Combo, 1, 0);
            parentPanel.Controls.Add(sire1Label, 0, 1);
            parentPanel.Controls.Add(sire1Combo, 1, 1);
            parentPanel.Controls.Add(calculateButton, 1, 2);

            // Create results panel
            resultPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
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
            Panel containerPanel = ResponsiveFormComponentFactory.CreateResponsivePanel();
            containerPanel.Padding = new Padding(20);

            // Create description label
            pedigreeDescriptionLabel = new Label
            {
                Text = "View and analyze animal pedigrees and ancestry",
                Font = new Font("Segoe UI", 12),
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 20)
            };

            // Create animal selection panel using TableLayoutPanel
            TableLayoutPanel selectionPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 3,
                RowCount = 1,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };

            selectionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            selectionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            selectionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            Label animalLabel = new Label
            {
                Text = "Select Animal:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top, // Or just AnchorStyles.Left, depending on layout FIXME if UI is off come back here 
                TextAlign = ContentAlignment.MiddleLeft // Align text to the middle left
            };

            animalSelector = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Left | AnchorStyles.Top 
            };

            viewPedigreeButton = CreateStyledButton("View Pedigree");
            viewPedigreeButton.Anchor = AnchorStyles.Right | AnchorStyles.Top; //FIXME
            viewPedigreeButton.TextAlign = ContentAlignment.MiddleLeft; //FIXME

            selectionPanel.Controls.Add(animalLabel, 0, 0);
            selectionPanel.Controls.Add(animalSelector, 1, 0);
            selectionPanel.Controls.Add(viewPedigreeButton, 2, 0);

            // Create pedigree display panel
            pedigreeDisplayPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
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
            infoPanel.Margin = new Padding(0, 0, 0, 10);

            // Add all components to container
            containerPanel.Controls.Add(pedigreeDisplayPanel);
            containerPanel.Controls.Add(selectionPanel);
            containerPanel.Controls.Add(infoPanel);
            containerPanel.Controls.Add(pedigreeDescriptionLabel);

            pedigreeTab.Controls.Add(containerPanel);
        }

        private Button CreateStyledButton(string text)
        {
            Button button = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Margin = new Padding(0, 0, 10, 0),
                Padding = new Padding(10, 5, 10, 5),
                MinimumSize = new Size(100, 30)
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
                Padding = new Padding(10),
                AutoSize = true
            };

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                AutoSize = true
            };

            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Dock = DockStyle.Fill
            };

            Label contentLabel = new Label
            {
                Text = content,
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Dock = DockStyle.Fill,
                MaximumSize = new Size(800, 0)
            };

            layout.Controls.Add(titleLabel, 0, 0);
            layout.Controls.Add(contentLabel, 0, 1);

            panel.Controls.Add(layout);
            return panel;
        }

        private void RegisterEventHandlers()
        {
            loadHandler = new EventHandler(GeneticsPanel_Load);
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
                var speciesService = new SpeciesService(_context);
                var species = await speciesService.GetAllSpeciesAsync();

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
                var animalService = new AnimalService(_context);
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

                traitRegistryGrid.DataSource = null;
                traitRegistryGrid.DataSource = traits;

                // Set column display properties
                if (traitRegistryGrid.Columns.Contains("TraitType"))
                {
                    traitRegistryGrid.Columns["TraitType"].DataPropertyName = "Name";
                }

                if (traitRegistryGrid.Columns.Contains("Species"))
                {
                    traitRegistryGrid.Columns["Species"].DataPropertyName = "CommonName";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing traits: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddTraitButton_Click(object sender, EventArgs e)
        {
            using (var form = new AddTraitForm(_traitService, _geneService, _context))
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
            using (var form = new AddTraitForm(_traitService, _geneService, _context, selectedTrait))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    RefreshTraitGrid();
                }
            }
        }

        private void DeleteTraitButton_Click(object sender, EventArgs e)
        {
            if (traitRegistryGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a trait to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedTrait = (Trait)traitRegistryGrid.SelectedRows[0].DataBoundItem;

            DialogResult result = MessageBox.Show(
                $"Are you sure you want to delete the trait '{selectedTrait.CommonName}'?",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    _traitService.DeleteTraitAsync(selectedTrait.Id);
                    RefreshTraitGrid();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting trait: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AssignTraitButton_Click(object sender, EventArgs e)
        {
            using (var form = new AssignTraitForm(_traitService, _geneService, new AnimalService(_context), _context))
            {
                form.ShowDialog();
            }
        }

        private async void Filter_Changed(object sender, EventArgs e)
        {
            await RefreshTraitGrid();
        }

        private void CalculateButton_Click(object sender, EventArgs e)
        {
            if (dam1Combo.SelectedItem == null || sire1Combo.SelectedItem == null)
            {
                MessageBox.Show("Please select both a dam and sire for breeding calculation.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dam = (Animal)dam1Combo.SelectedItem;
            var sire = (Animal)sire1Combo.SelectedItem;

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
                // Calculate breeding results
                var results = _breedingService.CalculateBreedingOutcomes(dam, sire);

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

                // Use FlowLayoutPanel for responsive results display
                FlowLayoutPanel flowPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    Width = resultsContainer.Width
                };
                resultsContainer.Controls.Add(flowPanel);

                foreach (var result in results)
                {
                    Panel traitPanel = new Panel
                    {
                        BorderStyle = BorderStyle.FixedSingle,
                        Width = flowPanel.Width - 20,
                        AutoSize = true,
                        Margin = new Padding(0, 0, 0, 10)
                    };

                    TableLayoutPanel layout = new TableLayoutPanel
                    {
                        ColumnCount = 1,
                        RowCount = 3,
                        Dock = DockStyle.Fill,
                        AutoSize = true,
                        Padding = new Padding(10)
                    };

                    layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                    Label traitLabel = new Label
                    {
                        Text = result.ToString(), //poss FIXME here 
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        AutoSize = true,
                        Dock = DockStyle.Fill
                    };

                    Label probabilityLabel = new Label
                    {
                        Text = $"Probability: {result.ToString():P}", //FIXME no clue what's going on here was Probability
                        AutoSize = true,
                        Dock = DockStyle.Fill
                    };

                    //Label genotypeLabel = new Label
                    //{
                    //    Text = $"Genotype: {result.ToString():P}", //THESE DON"T SEEM RIGHT FIXME 
                    //    Font = new Font("Segoe UI", 9),
                    //    AutoSize = true,
                    //    Dock = DockStyle.Fill
                    //};

                    Label genotypeLabel = new Label
                    {
                        Text = $"Possible Genotypes: {result.ToString()}",
                        Font = new Font("Segoe UI", 9),
                        AutoSize = true,
                        Dock = DockStyle.Fill
                    };

                    layout.Controls.Add(traitLabel, 0, 0);
                    layout.Controls.Add(probabilityLabel, 0, 1);
                    layout.Controls.Add(genotypeLabel, 0, 2);

                    traitPanel.Controls.Add(layout);
                    flowPanel.Controls.Add(traitPanel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating breeding outcomes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                report.AppendLine($"Generated: {DateTime.Now}");
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
                    report.AppendLine($"Trait Type: {trait.TraitType.Name ?? "None"}");
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

        private void ViewPedigreeButton_Click(object sender, EventArgs e)
        {
            if (animalSelector.SelectedItem == null)
            {
                MessageBox.Show("Please select an animal to view its pedigree.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedAnimal = (Animal)animalSelector.SelectedItem;

            // Clear previous pedigree
            pedigreeDisplayPanel.Controls.Clear();

            Label pedigreeTitle = new Label
            {
                Text = $"Pedigree for {selectedAnimal.Name}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };
            pedigreeDisplayPanel.Controls.Add(pedigreeTitle);

            try
            {

                var animal = _animalService.MapSingleAnimaltoDto(selectedAnimal);
                var animalResult = animal.Result;
           
                // Create a new form to display the pedigree in a larger view
                var pedigreeForm = IndividualAnimalAncestryForm.Create(_baseForm, _context, animalResult);

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

        // Override the ResizeUI method from ResponsivePanel
        public override void ResizeUI()
        {
            // Adjust the layout of components based on the current size
            if (mainTabControl != null)
            {
                // Ensure the tab control fills the available space
                mainTabControl.Size = new Size(this.Width, this.Height - headerPanel.Height);

                // Adjust data grid columns
                if (traitRegistryGrid != null && traitRegistryGrid.Columns.Count > 0)
                {
                    int totalWidth = traitRegistryGrid.Width - 20; // Account for scrollbar

                    // Adjust column widths proportionally
                    traitRegistryGrid.Columns["Id"].Width = (int)(totalWidth * 0.08);
                    traitRegistryGrid.Columns["CommonName"].Width = (int)(totalWidth * 0.20);
                    traitRegistryGrid.Columns["TraitType"].Width = (int)(totalWidth * 0.15);
                    traitRegistryGrid.Columns["Genotype"].Width = (int)(totalWidth * 0.15);
                    traitRegistryGrid.Columns["Species"].Width = (int)(totalWidth * 0.15);
                    traitRegistryGrid.Columns["Description"].Width = (int)(totalWidth * 0.27);
                }

                // Adjust result panel layout if needed
                if (resultPanel != null && resultPanel.Controls.Count > 0)
                {
                    foreach (Control control in resultPanel.Controls)
                    {
                        if (control is Panel resultsContainer)
                        {
                            resultsContainer.Width = resultPanel.Width - 20;
                        }
                    }
                }
            }
        }

        // Implement the RefreshDataAsync method from INavigable
        public override async Task RefreshDataAsync()
        {
            await LoadTraitTypes();
            await LoadSpecies();
            await LoadAnimals();
            await RefreshTraitGrid();
        }
    }
}



//using System;
//using System.Drawing;
//using System.Windows.Forms;
//using System.Text;
//using System.Linq;
//using System.IO;
//using System.Threading.Tasks;
//using System.Collections.Generic;
//using RATAPPLibrary.Services;
//using RATAPPLibrary.Services.Genetics;
//using RATAPPLibrary.Data.Models;
//using RATAPPLibrary.Data.Models.Genetics;
//using RATAPPLibrary.Data.DbContexts;
//using RATAPP.Forms;

//namespace RATAPP.Panels
//{
//    public class GeneticsPanel : Panel, INavigable
//    {
//        private readonly TraitService _traitService;
//        private readonly GeneService _geneService;
//        private readonly BreedingCalculationService _breedingService;
//        private readonly AnimalService _animalService;
//        private readonly RatAppDbContext _context;
//        private readonly RATAppBaseForm _baseForm;

//        // UI Components
//        private Panel headerPanel;
//        private TabControl mainTabControl;
//        private TabPage traitManagementTab;
//        private TabPage analysisTab;
//        private TabPage breedingCalculatorTab;
//        private TabPage pedigreeTab;

//        // Trait Management Components
//        private DataGridView traitRegistryGrid;
//        private Button addTraitButton;
//        private Button editTraitButton;
//        private Button deleteTraitButton;
//        private Button assignTraitButton;
//        private ComboBox traitTypeFilter;
//        private ComboBox speciesFilter;
//        private Label filterLabel;

//        // Analysis Components
//        private Panel colonyStatsPanel;
//        private Panel litterAnalysisPanel;
//        private ComboBox analysisFilterCombo;
//        private Button generateReportButton;
//        private Label analysisDescriptionLabel;

//        // Breeding Calculator Components
//        private ComboBox dam1Combo;
//        private ComboBox sire1Combo;
//        private Button calculateButton;
//        private Panel resultPanel;
//        private Label breedingDescriptionLabel;
//        private Label dam1Label;
//        private Label sire1Label;

//        // Pedigree Components
//        private ComboBox animalSelector;
//        private Panel pedigreeDisplayPanel;
//        private Button viewPedigreeButton;
//        private Label pedigreeDescriptionLabel;

//        public Action<object, EventArgs> Load { get; private set; }

//        public GeneticsPanel(RATAppBaseForm baseForm, TraitService traitService, GeneService geneService,
//                            BreedingCalculationService breedingService, RatAppDbContext context)
//        {
//            _baseForm = baseForm;
//            _traitService = traitService;
//            _geneService = geneService;
//            _breedingService = breedingService;
//            _animalService = new AnimalService(context); //TODO look at the difference between this implementation and how I've been implementing the service 
//            _context = context;

//            InitializeComponents();
//            RegisterEventHandlers();
//        }

//        private void InitializeComponents()
//        {
//            // Main panel setup
//            this.Dock = DockStyle.Fill;
//            this.BackColor = Color.White;

//            // Create header panel
//            InitializeHeaderPanel();

//            // Initialize main tab control
//            mainTabControl = new TabControl
//            {
//                Dock = DockStyle.Fill,
//                Font = new Font("Segoe UI", 11),
//                Padding = new Point(12, 4)
//            };

//            // Initialize tabs
//            traitManagementTab = new TabPage("Trait Management");
//            analysisTab = new TabPage("Genetic Analysis");
//            breedingCalculatorTab = new TabPage("Breeding Calculator");
//            pedigreeTab = new TabPage("Pedigree Viewer");

//            InitializeTraitManagementTab();
//            InitializeAnalysisTab();
//            InitializeBreedingCalculatorTab();
//            InitializePedigreeTab();

//            mainTabControl.TabPages.Add(traitManagementTab);
//            mainTabControl.TabPages.Add(analysisTab);
//            mainTabControl.TabPages.Add(breedingCalculatorTab);
//            mainTabControl.TabPages.Add(pedigreeTab);

//            // Add controls to main panel
//            this.Controls.Add(mainTabControl);
//            this.Controls.Add(headerPanel);
//        }

//        private void InitializeHeaderPanel()
//        {
//            headerPanel = new Panel
//            {
//                Dock = DockStyle.Top,
//                Height = 70,
//                BackColor = Color.FromArgb(0, 120, 212)
//            };

//            Label titleLabel = new Label
//            {
//                Text = "Genetics Management",
//                ForeColor = Color.White,
//                Font = new Font("Segoe UI", 18, FontStyle.Bold),
//                AutoSize = true,
//                Location = new Point(25, 10)
//            };

//            Label descriptionLabel = new Label
//            {
//                Text = "Manage genetic traits, analyze breeding outcomes, and view pedigrees",
//                ForeColor = Color.White,
//                Font = new Font("Segoe UI", 10),
//                AutoSize = true,
//                Location = new Point(25, 40)
//            };

//            headerPanel.Controls.Add(titleLabel);
//            headerPanel.Controls.Add(descriptionLabel);
//        }

//        private void InitializeTraitManagementTab()
//        {
//            // Create container panel with padding
//            Panel containerPanel = new Panel
//            {
//                Dock = DockStyle.Fill,
//                Padding = new Padding(20)
//            };

//            // Create filter section
//            Panel filterPanel = new Panel
//            {
//                Dock = DockStyle.Top,
//                Height = 60
//            };

//            filterLabel = new Label
//            {
//                Text = "Filter by:",
//                Font = new Font("Segoe UI", 10),
//                AutoSize = true,
//                Location = new Point(0, 10)
//            };

//            traitTypeFilter = new ComboBox
//            {
//                Width = 200,
//                Font = new Font("Segoe UI", 10),
//                DropDownStyle = ComboBoxStyle.DropDownList,
//                Location = new Point(100, 8)
//            };

//            Label speciesLabel = new Label
//            {
//                Text = "Species:",
//                Font = new Font("Segoe UI", 10),
//                AutoSize = true,
//                Location = new Point(320, 10)
//            };

//            speciesFilter = new ComboBox
//            {
//                Width = 200,
//                Font = new Font("Segoe UI", 10),
//                DropDownStyle = ComboBoxStyle.DropDownList,
//                Location = new Point(380, 8)
//            };

//            filterPanel.Controls.Add(filterLabel);
//            filterPanel.Controls.Add(traitTypeFilter);
//            filterPanel.Controls.Add(speciesLabel);
//            filterPanel.Controls.Add(speciesFilter);

//            // Create trait registry grid
//            traitRegistryGrid = new DataGridView
//            {
//                Dock = DockStyle.Fill,
//                AutoGenerateColumns = false,
//                MultiSelect = false,
//                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
//                BackgroundColor = Color.White,
//                BorderStyle = BorderStyle.None,
//                RowHeadersVisible = false,
//                AllowUserToAddRows = false,
//                AllowUserToDeleteRows = false,
//                ReadOnly = true,
//                Font = new Font("Segoe UI", 9)
//            };

//            traitRegistryGrid.Columns.AddRange(new DataGridViewColumn[]
//            {
//                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 60 },
//                new DataGridViewTextBoxColumn { Name = "CommonName", HeaderText = "Name", Width = 150 },
//                new DataGridViewTextBoxColumn { Name = "TraitType", HeaderText = "Type", Width = 120 },
//                new DataGridViewTextBoxColumn { Name = "Genotype", HeaderText = "Genotype", Width = 120 },
//                new DataGridViewTextBoxColumn { Name = "Species", HeaderText = "Species", Width = 120 },
//                new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", Width = 200 }
//            });

//            // Create button panel
//            Panel buttonPanel = new Panel
//            {
//                Dock = DockStyle.Bottom,
//                Height = 50,
//                Padding = new Padding(0, 10, 0, 0)
//            };

//            addTraitButton = CreateButton("Add Trait", 0);
//            editTraitButton = CreateButton("Edit Trait", 110);
//            deleteTraitButton = CreateButton("Delete Trait", 220);
//            assignTraitButton = CreateButton("Assign to Animal", 330);

//            buttonPanel.Controls.Add(addTraitButton);
//            buttonPanel.Controls.Add(editTraitButton);
//            buttonPanel.Controls.Add(deleteTraitButton);
//            buttonPanel.Controls.Add(assignTraitButton);

//            // Add info panel
//            Panel infoPanel = CreateInfoPanel(
//                "Trait Management",
//                "• Add, edit, and delete genetic traits in your colony\n" +
//                "• Assign traits to individual animals\n" +
//                "• Filter traits by type and species\n" +
//                "• Traits are used in breeding calculations and pedigree analysis"
//            );
//            infoPanel.Dock = DockStyle.Top;
//            infoPanel.Height = 120;
//            infoPanel.Margin = new Padding(0, 0, 0, 10);

//            // Add all components to container
//            containerPanel.Controls.Add(traitRegistryGrid);
//            containerPanel.Controls.Add(buttonPanel);
//            containerPanel.Controls.Add(filterPanel);
//            containerPanel.Controls.Add(infoPanel);

//            traitManagementTab.Controls.Add(containerPanel);
//        }

//        private void InitializeAnalysisTab()
//        {
//            // Create container panel with padding
//            Panel containerPanel = new Panel
//            {
//                Dock = DockStyle.Fill,
//                Padding = new Padding(20)
//            };

//            // Create description label
//            analysisDescriptionLabel = new Label
//            {
//                Text = "Analyze genetic traits across your colony and breeding lines",
//                Font = new Font("Segoe UI", 12),
//                AutoSize = true,
//                Dock = DockStyle.Top,
//                Padding = new Padding(0, 0, 0, 10)
//            };

//            // Create filter panel
//            Panel filterPanel = new Panel
//            {
//                Dock = DockStyle.Top,
//                Height = 50
//            };

//            Label filterLabel = new Label
//            {
//                Text = "Analysis Type:",
//                Font = new Font("Segoe UI", 10),
//                AutoSize = true,
//                Location = new Point(0, 15)
//            };

//            analysisFilterCombo = new ComboBox
//            {
//                Width = 250,
//                Font = new Font("Segoe UI", 10),
//                DropDownStyle = ComboBoxStyle.DropDownList,
//                Location = new Point(100, 12)
//            };
//            analysisFilterCombo.Items.AddRange(new object[] {
//                "Colony Trait Distribution",
//                "Breeding Line Analysis",
//                "Inheritance Patterns",
//                "Trait Frequency by Generation"
//            });
//            analysisFilterCombo.SelectedIndex = 0;

//            generateReportButton = new Button
//            {
//                Text = "Generate Report",
//                Font = new Font("Segoe UI", 10),
//                Width = 150,
//                Height = 30,
//                Location = new Point(370, 10),
//                BackColor = Color.FromArgb(0, 120, 212),
//                ForeColor = Color.White,
//                FlatStyle = FlatStyle.Flat
//            };
//            generateReportButton.FlatAppearance.BorderSize = 0;

//            filterPanel.Controls.Add(filterLabel);
//            filterPanel.Controls.Add(analysisFilterCombo);
//            filterPanel.Controls.Add(generateReportButton);

//            // Create stats panel
//            colonyStatsPanel = new Panel
//            {
//                Dock = DockStyle.Top,
//                Height = 200,
//                BorderStyle = BorderStyle.FixedSingle,
//                Margin = new Padding(0, 10, 0, 10)
//            };

//            Label statsTitle = new Label
//            {
//                Text = "Colony Statistics",
//                Font = new Font("Segoe UI", 12, FontStyle.Bold),
//                AutoSize = true,
//                Location = new Point(10, 10)
//            };

//            // Placeholder stats (would be populated with real data)
//            Label statsContent = new Label
//            {
//                Text = "Total Animals: 0\nTotal Traits: 0\nMost Common Trait: N/A",
//                Font = new Font("Segoe UI", 10),
//                AutoSize = true,
//                Location = new Point(10, 40)
//            };

//            colonyStatsPanel.Controls.Add(statsTitle);
//            colonyStatsPanel.Controls.Add(statsContent);

//            // Create analysis panel
//            litterAnalysisPanel = new Panel
//            {
//                Dock = DockStyle.Fill,
//                BorderStyle = BorderStyle.FixedSingle
//            };

//            Label analysisTitle = new Label
//            {
//                Text = "Detailed Analysis",
//                Font = new Font("Segoe UI", 12, FontStyle.Bold),
//                AutoSize = true,
//                Location = new Point(10, 10)
//            };

//            // Placeholder for analysis content
//            Label analysisContent = new Label
//            {
//                Text = "Select an analysis type and click 'Generate Report' to view detailed analysis.",
//                Font = new Font("Segoe UI", 10),
//                AutoSize = true,
//                Location = new Point(10, 40)
//            };

//            litterAnalysisPanel.Controls.Add(analysisTitle);
//            litterAnalysisPanel.Controls.Add(analysisContent);

//            // Add all components to container
//            containerPanel.Controls.Add(litterAnalysisPanel);
//            containerPanel.Controls.Add(colonyStatsPanel);
//            containerPanel.Controls.Add(filterPanel);
//            containerPanel.Controls.Add(analysisDescriptionLabel);

//            analysisTab.Controls.Add(containerPanel);
//        }

//        private void InitializeBreedingCalculatorTab()
//        {
//            // Create container panel with padding
//            Panel containerPanel = new Panel
//            {
//                Dock = DockStyle.Fill,
//                Padding = new Padding(20)
//            };

//            // Create description label
//            breedingDescriptionLabel = new Label
//            {
//                Text = "Calculate potential offspring traits based on parent genetics",
//                Font = new Font("Segoe UI", 12),
//                AutoSize = true,
//                Dock = DockStyle.Top,
//                Padding = new Padding(0, 0, 0, 20)
//            };

//            // Create parent selection panel
//            Panel parentPanel = new Panel
//            {
//                Dock = DockStyle.Top,
//                Height = 120
//            };

//            // Dam selection
//            dam1Label = new Label
//            {
//                Text = "Dam (Female):",
//                Font = new Font("Segoe UI", 10),
//                AutoSize = true,
//                Location = new Point(0, 15)
//            };

//            dam1Combo = new ComboBox
//            {
//                Width = 250,
//                Font = new Font("Segoe UI", 10),
//                DropDownStyle = ComboBoxStyle.DropDownList,
//                Location = new Point(120, 12)
//            };

//            // Sire selection
//            sire1Label = new Label
//            {
//                Text = "Sire (Male):",
//                Font = new Font("Segoe UI", 10),
//                AutoSize = true,
//                Location = new Point(0, 55)
//            };

//            sire1Combo = new ComboBox
//            {
//                Width = 250,
//                Font = new Font("Segoe UI", 10),
//                DropDownStyle = ComboBoxStyle.DropDownList,
//                Location = new Point(120, 52)
//            };

//            calculateButton = new Button
//            {
//                Text = "Calculate Offspring",
//                Font = new Font("Segoe UI", 10),
//                Width = 150,
//                Height = 30,
//                Location = new Point(120, 90),
//                BackColor = Color.FromArgb(0, 120, 212),
//                ForeColor = Color.White,
//                FlatStyle = FlatStyle.Flat
//            };
//            calculateButton.FlatAppearance.BorderSize = 0;

//            parentPanel.Controls.Add(dam1Label);
//            parentPanel.Controls.Add(dam1Combo);
//            parentPanel.Controls.Add(sire1Label);
//            parentPanel.Controls.Add(sire1Combo);
//            parentPanel.Controls.Add(calculateButton);

//            // Create results panel
//            resultPanel = new Panel
//            {
//                Dock = DockStyle.Fill,
//                BorderStyle = BorderStyle.FixedSingle
//            };

//            Label resultTitle = new Label
//            {
//                Text = "Breeding Results",
//                Font = new Font("Segoe UI", 12, FontStyle.Bold),
//                AutoSize = true,
//                Location = new Point(10, 10)
//            };

//            // Placeholder for results
//            Label resultContent = new Label
//            {
//                Text = "Select parents and click 'Calculate Offspring' to view potential offspring traits.",
//                Font = new Font("Segoe UI", 10),
//                AutoSize = true,
//                Location = new Point(10, 40)
//            };

//            resultPanel.Controls.Add(resultTitle);
//            resultPanel.Controls.Add(resultContent);

//            // Add info panel
//            Panel infoPanel = CreateInfoPanel(
//                "Breeding Calculator",
//                "• Select dam and sire to calculate potential offspring traits\n" +
//                "• Results show probability of each trait appearing in offspring\n" +
//                "• Calculations are based on Mendelian inheritance\n" +
//                "• Only animals with assigned traits will show accurate results"
//            );
//            infoPanel.Dock = DockStyle.Top;
//            infoPanel.Height = 120;
//            infoPanel.Margin = new Padding(0, 0, 0, 10);

//            // Add all components to container
//            containerPanel.Controls.Add(resultPanel);
//            containerPanel.Controls.Add(parentPanel);
//            containerPanel.Controls.Add(infoPanel);
//            containerPanel.Controls.Add(breedingDescriptionLabel);

//            breedingCalculatorTab.Controls.Add(containerPanel);
//        }

//        private void InitializePedigreeTab()
//        {
//            // Create container panel with padding
//            Panel containerPanel = new Panel
//            {
//                Dock = DockStyle.Fill,
//                Padding = new Padding(20)
//            };

//            // Create description label
//            pedigreeDescriptionLabel = new Label
//            {
//                Text = "View and analyze animal pedigrees and ancestry",
//                Font = new Font("Segoe UI", 12),
//                AutoSize = true,
//                Dock = DockStyle.Top,
//                Padding = new Padding(0, 0, 0, 20)
//            };

//            // Create animal selection panel
//            Panel selectionPanel = new Panel
//            {
//                Dock = DockStyle.Top,
//                Height = 60
//            };

//            Label animalLabel = new Label
//            {
//                Text = "Select Animal:",
//                Font = new Font("Segoe UI", 10),
//                AutoSize = true,
//                Location = new Point(0, 15)
//            };

//            animalSelector = new ComboBox
//            {
//                Width = 250,
//                Font = new Font("Segoe UI", 10),
//                DropDownStyle = ComboBoxStyle.DropDownList,
//                Location = new Point(100, 12)
//            };

//            viewPedigreeButton = new Button
//            {
//                Text = "View Pedigree",
//                Font = new Font("Segoe UI", 10),
//                Width = 120,
//                Height = 30,
//                Location = new Point(370, 10),
//                BackColor = Color.FromArgb(0, 120, 212),
//                ForeColor = Color.White,
//                FlatStyle = FlatStyle.Flat
//            };
//            viewPedigreeButton.FlatAppearance.BorderSize = 0;

//            selectionPanel.Controls.Add(animalLabel);
//            selectionPanel.Controls.Add(animalSelector);
//            selectionPanel.Controls.Add(viewPedigreeButton);

//            // Create pedigree display panel
//            pedigreeDisplayPanel = new Panel
//            {
//                Dock = DockStyle.Fill,
//                BorderStyle = BorderStyle.FixedSingle
//            };

//            Label pedigreeTitle = new Label
//            {
//                Text = "Pedigree View",
//                Font = new Font("Segoe UI", 12, FontStyle.Bold),
//                AutoSize = true,
//                Location = new Point(10, 10)
//            };

//            // Placeholder for pedigree
//            Label pedigreeContent = new Label
//            {
//                Text = "Select an animal and click 'View Pedigree' to display the pedigree chart.",
//                Font = new Font("Segoe UI", 10),
//                AutoSize = true,
//                Location = new Point(10, 40)
//            };

//            pedigreeDisplayPanel.Controls.Add(pedigreeTitle);
//            pedigreeDisplayPanel.Controls.Add(pedigreeContent);

//            // Add info panel
//            Panel infoPanel = CreateInfoPanel(
//                "Pedigree Viewer",
//                "• View ancestry information for any animal in your colony\n" +
//                "• Trace genetic traits through multiple generations\n" +
//                "• Identify common ancestors and breeding lines\n" +
//                "• Export pedigree charts for documentation"
//            );
//            infoPanel.Dock = DockStyle.Top;
//            infoPanel.Height = 120;
//            infoPanel.Margin = new Padding(0, 0, 0, 10);

//            // Add all components to container
//            containerPanel.Controls.Add(pedigreeDisplayPanel);
//            containerPanel.Controls.Add(selectionPanel);
//            containerPanel.Controls.Add(infoPanel);
//            containerPanel.Controls.Add(pedigreeDescriptionLabel);

//            pedigreeTab.Controls.Add(containerPanel);
//        }

//        private Button CreateButton(string text, int x)
//        {
//            Button button = new Button
//            {
//                Text = text,
//                Font = new Font("Segoe UI", 10),
//                Location = new Point(x, 0),
//                Width = 100,
//                Height = 30,
//                BackColor = Color.FromArgb(0, 120, 212),
//                ForeColor = Color.White,
//                FlatStyle = FlatStyle.Flat
//            };
//            button.FlatAppearance.BorderSize = 0;
//            return button;
//        }

//        private Panel CreateInfoPanel(string title, string content)
//        {
//            Panel panel = new Panel
//            {
//                BorderStyle = BorderStyle.FixedSingle,
//                BackColor = Color.FromArgb(240, 240, 240),
//                Padding = new Padding(10)
//            };

//            Label titleLabel = new Label
//            {
//                Text = title,
//                Font = new Font("Segoe UI", 11, FontStyle.Bold),
//                AutoSize = true,
//                Location = new Point(10, 10)
//            };

//            Label contentLabel = new Label
//            {
//                Text = content,
//                Font = new Font("Segoe UI", 9),
//                AutoSize = true,
//                Location = new Point(10, 35),
//                MaximumSize = new Size(800, 0)
//            };

//            panel.Controls.Add(titleLabel);
//            panel.Controls.Add(contentLabel);
//            return panel;
//        }

//        private void RegisterEventHandlers()
//        {
//            this.Load += GeneticsPanel_Load;
//            addTraitButton.Click += AddTraitButton_Click;
//            editTraitButton.Click += EditTraitButton_Click;
//            deleteTraitButton.Click += DeleteTraitButton_Click;
//            assignTraitButton.Click += AssignTraitButton_Click;
//            calculateButton.Click += CalculateButton_Click;
//            generateReportButton.Click += GenerateReportButton_Click;
//            viewPedigreeButton.Click += ViewPedigreeButton_Click;
//            traitTypeFilter.SelectedIndexChanged += Filter_Changed;
//            speciesFilter.SelectedIndexChanged += Filter_Changed;
//        }

//        private async void GeneticsPanel_Load(object sender, EventArgs e)
//        {
//            await LoadTraitTypes();
//            await LoadSpecies();
//            await LoadAnimals();
//            await RefreshTraitGrid();
//        }

//        private async Task LoadTraitTypes()
//        {
//            try
//            {
//                var traitTypes = await _traitService.GetAllTraitTypesAsync();
//                traitTypeFilter.Items.Clear();
//                traitTypeFilter.Items.Add("All Types");

//                foreach (var type in traitTypes)
//                {
//                    traitTypeFilter.Items.Add(type);
//                }

//                traitTypeFilter.SelectedIndex = 0;
//                traitTypeFilter.DisplayMember = "Name";
//                traitTypeFilter.ValueMember = "Id";
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Error loading trait types: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }

//        private async Task LoadSpecies()
//        {
//            try
//            {
//                var speciesService = new SpeciesService(_context);
//                var species = await speciesService.GetAllSpeciesAsync();

//                speciesFilter.Items.Clear();
//                speciesFilter.Items.Add("All Species");

//                foreach (var s in species)
//                {
//                    speciesFilter.Items.Add(s);
//                }

//                speciesFilter.SelectedIndex = 0;
//                speciesFilter.DisplayMember = "CommonName";
//                speciesFilter.ValueMember = "Id";
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Error loading species: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }

//        private async Task LoadAnimals()
//        {
//            try
//            {
//                var animalService = new AnimalService(_context);
//                var animals = await animalService.GetAllAnimalsAsync();

//                // Populate dam and sire combos for breeding calculator
//                dam1Combo.Items.Clear();
//                sire1Combo.Items.Clear();
//                animalSelector.Items.Clear();

//                // Add all animals to the pedigree selector
//                foreach (var animal in animals)
//                {
//                    animalSelector.Items.Add(animal);

//                    // Add females to dam combo
//                    if (animal.sex == "Female")
//                    {
//                        dam1Combo.Items.Add(animal);
//                    }
//                    // Add males to sire combo
//                    else if (animal.sex == "Male")
//                    {
//                        sire1Combo.Items.Add(animal);
//                    }
//                }

//                dam1Combo.DisplayMember = "Name";
//                dam1Combo.ValueMember = "Id";
//                sire1Combo.DisplayMember = "Name";
//                sire1Combo.ValueMember = "Id";
//                animalSelector.DisplayMember = "Name";
//                animalSelector.ValueMember = "Id";
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Error loading animals: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }

//        private async Task RefreshTraitGrid()
//        {
//            try
//            {
//                var traits = await _traitService.GetAllTraitsAsync();

//                // Apply filters if selected
//                if (traitTypeFilter.SelectedIndex > 0)
//                {
//                    var selectedTraitType = (TraitType)traitTypeFilter.SelectedItem;
//                    traits = traits.Where(t => t.TraitTypeId == selectedTraitType.Id).ToList();
//                }

//                if (speciesFilter.SelectedIndex > 0)
//                {
//                    var selectedSpecies = (Species)speciesFilter.SelectedItem;
//                    traits = traits.Where(t => t.SpeciesID == selectedSpecies.Id).ToList();
//                }

//                traitRegistryGrid.DataSource = null;
//                traitRegistryGrid.DataSource = traits;

//                // Set column display properties
//                if (traitRegistryGrid.Columns.Contains("TraitType"))
//                {
//                    traitRegistryGrid.Columns["TraitType"].DataPropertyName = "Name";
//                }

//                if (traitRegistryGrid.Columns.Contains("Species"))
//                {
//                    traitRegistryGrid.Columns["Species"].DataPropertyName = "CommonName";
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Error refreshing traits: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }

//        private void AddTraitButton_Click(object sender, EventArgs e)
//        {
//            using (var form = new AddTraitForm(_traitService, _geneService, _context))
//            {
//                if (form.ShowDialog() == DialogResult.OK)
//                {
//                    RefreshTraitGrid();
//                }
//            }
//        }

//        private void EditTraitButton_Click(object sender, EventArgs e)
//        {
//            if (traitRegistryGrid.SelectedRows.Count == 0)
//            {
//                MessageBox.Show("Please select a trait to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//                return;
//            }

//            var selectedTrait = (Trait)traitRegistryGrid.SelectedRows[0].DataBoundItem;
//            using (var form = new AddTraitForm(_traitService, _geneService, _context)) //FIXME took , selectedTrait out
//            {
//                if (form.ShowDialog() == DialogResult.OK)
//                {
//                    RefreshTraitGrid();
//                }
//            }
//        }

//        private void DeleteTraitButton_Click(object sender, EventArgs e)
//        {
//            if (traitRegistryGrid.SelectedRows.Count == 0)
//            {
//                MessageBox.Show("Please select a trait to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//                return;
//            }

//            var selectedTrait = (Trait)traitRegistryGrid.SelectedRows[0].DataBoundItem;

//            DialogResult result = MessageBox.Show(
//                $"Are you sure you want to delete the trait '{selectedTrait.CommonName}'?",
//                "Confirm Deletion",
//                MessageBoxButtons.YesNo,
//                MessageBoxIcon.Question
//            );

//            if (result == DialogResult.Yes)
//            {
//                try
//                {
//                    _traitService.DeleteTraitAsync(selectedTrait.Id);
//                    RefreshTraitGrid();
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show($"Error deleting trait: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                }
//            }
//        }

//        private void AssignTraitButton_Click(object sender, EventArgs e)
//        {
//            using (var form = new AssignTraitForm(_traitService, _geneService, new AnimalService(_context), _context))
//            {
//                form.ShowDialog();
//            }
//        }

//        private async void Filter_Changed(object sender, EventArgs e)
//        {
//            await RefreshTraitGrid();
//        }

//        private void CalculateButton_Click(object sender, EventArgs e)
//        {
//            if (dam1Combo.SelectedItem == null || sire1Combo.SelectedItem == null)
//            {
//                MessageBox.Show("Please select both a dam and sire for breeding calculation.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//                return;
//            }

//            var dam = (Animal)dam1Combo.SelectedItem;
//            var sire = (Animal)sire1Combo.SelectedItem;

//            // Clear previous results
//            resultPanel.Controls.Clear();

//            Label resultTitle = new Label
//            {
//                Text = "Breeding Results",
//                Font = new Font("Segoe UI", 12, FontStyle.Bold),
//                AutoSize = true,
//                Location = new Point(10, 10)
//            };
//            resultPanel.Controls.Add(resultTitle);

//            try
//            {
//                // Calculate breeding results
//                var results = _breedingService.CalculateBreedingOutcomes(dam, sire);

//                if (results == null || !results.Any())
//                {
//                    Label noResultsLabel = new Label
//                    {
//                        Text = "No genetic traits found for these animals or they are incompatible for breeding.",
//                        Font = new Font("Segoe UI", 10),
//                        AutoSize = true,
//                        Location = new Point(10, 40)
//                    };
//                    resultPanel.Controls.Add(noResultsLabel);
//                    return;
//                }

//                // Display results in a formatted panel
//                Panel resultsContainer = new Panel
//                {
//                    AutoScroll = true,
//                    Location = new Point(10, 40),
//                    Size = new Size(resultPanel.Width - 20, resultPanel.Height - 50),
//                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
//                };
//                resultPanel.Controls.Add(resultsContainer);

//                int yPos = 10;
//                foreach (var result in results)
//                {
//                    Panel traitPanel = new Panel
//                    {
//                        BorderStyle = BorderStyle.FixedSingle,
//                        Location = new Point(0, yPos),
//                        Size = new Size(resultsContainer.Width - 20, 100),
//                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
//                    };

//                    Label traitLabel = new Label
//                    {
//                        Text = result.ToString(),
//                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
//                        AutoSize = true,
//                        Location = new Point(10, 10)
//                    };

//                    Label probabilityLabel = new Label
//                    {
//                        Text = $"Probability: {result.ToString():P}", //FIXME no clue what's going on here was Probability
//                        Font = new Font("Segoe UI", 9),
//                        AutoSize = true,
//                        Location = new Point(10, 35)
//                    };

//                    Label genotypeLabel = new Label
//                    {
//                        Text = $"Possible Genotypes: {result.ToString()}", //was PossibleGenoType FIXME not sure what the inteniton was
//                        Font = new Font("Segoe UI", 9),
//                        AutoSize = true,
//                        Location = new Point(10, 60)
//                    };

//                    traitPanel.Controls.Add(traitLabel);
//                    traitPanel.Controls.Add(probabilityLabel);
//                    traitPanel.Controls.Add(genotypeLabel);

//                    resultsContainer.Controls.Add(traitPanel);
//                    yPos += 110; // Add spacing between trait panels
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Error calculating breeding outcomes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }

//        private async void GenerateReportButton_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                var traits = await _traitService.GetAllTraitsAsync();
//                var traitTypes = await _traitService.GetAllTraitTypesAsync();

//                var report = new StringBuilder();
//                report.AppendLine("Genetics Report");
//                report.AppendLine("==============");
//                report.AppendLine($"Generated: {DateTime.Now}");
//                report.AppendLine();

//                // Trait Type Summary
//                report.AppendLine("Trait Types Summary");
//                report.AppendLine("-----------------");
//                foreach (var traitType in traitTypes)
//                {
//                    var typeTraits = traits.Where(t => t.TraitTypeId == traitType.Id).ToList();
//                    report.AppendLine($"{traitType.Name}: {typeTraits.Count} traits");
//                }
//                report.AppendLine();

//                // Trait Details
//                report.AppendLine("Trait Details");
//                report.AppendLine("-------------");
//                foreach (var trait in traits)
//                {
//                    report.AppendLine($"Name: {trait.CommonName}");
//                    report.AppendLine($"Type: {trait.TraitType?.Name ?? "Unknown"}");
//                    report.AppendLine($"Genotype: {trait.Genotype ?? "Not specified"}");
//                    report.AppendLine($"Description: {trait.TraitType.Name ?? "None"}"); //FIXME
//                    report.AppendLine();
//                }

//                // Save report
//                using (var saveDialog = new SaveFileDialog())
//                {
//                    saveDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
//                    saveDialog.FilterIndex = 1;
//                    saveDialog.DefaultExt = "txt";
//                    saveDialog.FileName = $"GeneticsReport_{DateTime.Now:yyyyMMdd}";

//                    if (saveDialog.ShowDialog() == DialogResult.OK)
//                    {
//                        await File.WriteAllTextAsync(saveDialog.FileName, report.ToString());
//                        MessageBox.Show("Report generated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }

//        private void ViewPedigreeButton_Click(object sender, EventArgs e)
//        {
//            if (animalSelector.SelectedItem == null)
//            {
//                MessageBox.Show("Please select an animal to view its pedigree.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
//                return;
//            }

//            var selectedAnimal = (Animal)animalSelector.SelectedItem;

//            // Clear previous pedigree
//            pedigreeDisplayPanel.Controls.Clear();

//            Label pedigreeTitle = new Label
//            {
//                Text = $"Pedigree for {selectedAnimal.Name}",
//                Font = new Font("Segoe UI", 12, FontStyle.Bold),
//                AutoSize = true,
//                Location = new Point(10, 10)
//            };
//            pedigreeDisplayPanel.Controls.Add(pedigreeTitle);

//            try
//            {
//                var animal = _animalService.MapSingleAnimaltoDto(selectedAnimal);
//                var animalResult = animal.Result;

//                // Create a new form to display the pedigree in a larger view
//                var pedigreeForm = IndividualAnimalAncestryForm.Create(_baseForm, _context, animalResult);

//                // Add the form to the pedigree panel
//                pedigreeForm.TopLevel = false;
//                pedigreeForm.FormBorderStyle = FormBorderStyle.None;
//                pedigreeForm.Dock = DockStyle.Fill;

//                pedigreeDisplayPanel.Controls.Add(pedigreeForm);
//                pedigreeForm.Show();
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Error displaying pedigree: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }

//        public async Task RefreshDataAsync()
//        {
//            await LoadTraitTypes();
//            await LoadSpecies();
//            await LoadAnimals();
//            await RefreshTraitGrid();
//        }
//    }
//}






////using System;
////using System.Drawing;
////using System.Windows.Forms;
////using System.Text;
////using System.Linq;
////using System.IO;
////using System.Threading.Tasks;
////using System.Collections.Generic;
////using RATAPPLibrary.Services;
////using RATAPPLibrary.Services.Genetics;
////using RATAPPLibrary.Data.Models;
////using RATAPPLibrary.Data.Models.Genetics;
////using RATAPPLibrary.Data.DbContexts;
////using RATAPP.Forms;

////namespace RATAPP.Panels
////{
////    public class GeneticsPanel : Panel, INavigable
////    {
////        private readonly TraitService _traitService;
////        private readonly GeneService _geneService;
////        private readonly BreedingCalculationService _breedingService;
////        private readonly RatAppDbContext _context;

////        // UI Components
////        private TabControl mainTabControl;
////        private TabPage traitManagementTab;
////        private TabPage analysisTab;
////        private TabPage breedingTab;

////        // Trait Management Components
////        private DataGridView traitRegistryGrid;
////        private Button addTraitButton;
////        private Button editTraitButton;
////        private Button assignTraitButton;
////        private ComboBox traitTypeFilter;
////        private ComboBox speciesFilter;

////        // Analysis Components
////        private Panel colonyStatsPanel;
////        private Panel litterAnalysisPanel;
////        private ComboBox analysisFilterCombo;
////        private Button generateReportButton;

////        // Breeding Components
////        private ComboBox dam1Combo;
////        private ComboBox dam2Combo;
////        private Button calculateButton;
////        private Panel resultPanel;

////        public Action<object, EventArgs> Load { get; private set; }

////        public GeneticsPanel(TraitService traitService, GeneService geneService, BreedingCalculationService breedingService, RatAppDbContext context)
////        {
////            _traitService = traitService;
////            _geneService = geneService;
////            _breedingService = breedingService;
////            _context = context;

////            InitializeComponents();
////            SetupLayout();
////            RegisterEventHandlers();
////        }

////        private void InitializeComponents()
////        {
////            // Initialize main tab control
////            mainTabControl = new TabControl
////            {
////                Dock = DockStyle.Fill
////            };

////            // Initialize tabs
////            traitManagementTab = new TabPage("Trait Management");
////            analysisTab = new TabPage("Analysis");
////            breedingTab = new TabPage("Breeding");

////            mainTabControl.TabPages.Add(traitManagementTab);
////            mainTabControl.TabPages.Add(analysisTab);
////            mainTabControl.TabPages.Add(breedingTab);

////            // Initialize Trait Management components
////            traitRegistryGrid = new DataGridView
////            {
////                Dock = DockStyle.Fill,
////                AutoGenerateColumns = false,
////                MultiSelect = false,
////                SelectionMode = DataGridViewSelectionMode.FullRowSelect
////            };

////            traitRegistryGrid.Columns.AddRange(new DataGridViewColumn[]
////            {
////                new DataGridViewTextBoxColumn { Name = "CommonName", HeaderText = "Name", DataPropertyName = "CommonName" },
////                new DataGridViewTextBoxColumn { Name = "TraitType", HeaderText = "Type", DataPropertyName = "TraitType.Name" },
////                new DataGridViewTextBoxColumn { Name = "Genotype", HeaderText = "Genotype", DataPropertyName = "Genotype" },
////                new DataGridViewTextBoxColumn { Name = "Species", HeaderText = "Species", DataPropertyName = "Species.CommonName" }
////            });

////            addTraitButton = new Button
////            {
////                Text = "Add Trait",
////                Width = 100,
////                Height = 30
////            };

////            editTraitButton = new Button
////            {
////                Text = "Edit Trait",
////                Width = 100,
////                Height = 30
////            };

////            assignTraitButton = new Button
////            {
////                Text = "Assign Trait",
////                Width = 100,
////                Height = 30
////            };

////            traitTypeFilter = new ComboBox
////            {
////                Width = 150,
////                DropDownStyle = ComboBoxStyle.DropDownList
////            };

////            speciesFilter = new ComboBox
////            {
////                Width = 150,
////                DropDownStyle = ComboBoxStyle.DropDownList
////            };

////            // Initialize Analysis components
////            colonyStatsPanel = new Panel
////            {
////                Dock = DockStyle.Top,
////                Height = 300
////            };

////            litterAnalysisPanel = new Panel
////            {
////                Dock = DockStyle.Fill
////            };

////            analysisFilterCombo = new ComboBox
////            {
////                Width = 150,
////                DropDownStyle = ComboBoxStyle.DropDownList
////            };

////            generateReportButton = new Button
////            {
////                Text = "Generate Report",
////                Width = 120,
////                Height = 30
////            };

////            // Initialize Breeding components
////            dam1Combo = new ComboBox
////            {
////                Width = 200,
////                DropDownStyle = ComboBoxStyle.DropDownList
////            };

////            dam2Combo = new ComboBox
////            {
////                Width = 200,
////                DropDownStyle = ComboBoxStyle.DropDownList
////            };

////            calculateButton = new Button
////            {
////                Text = "Calculate Offspring",
////                Width = 150,
////                Height = 30
////            };

////            resultPanel = new Panel
////            {
////                Dock = DockStyle.Bottom,
////                Height = 300
////            };
////        }

////        private void SetupLayout()
////        {
////            // Main panel setup
////            this.Dock = DockStyle.Fill;
////            this.Controls.Add(mainTabControl);

////            // Trait Management tab layout
////            var traitManagementLayout = new TableLayoutPanel
////            {
////                Dock = DockStyle.Fill,
////                ColumnCount = 2,
////                RowCount = 3
////            };

////            var filterPanel = new FlowLayoutPanel
////            {
////                Dock = DockStyle.Top,
////                Height = 40,
////                FlowDirection = FlowDirection.LeftToRight,
////                Padding = new Padding(5)
////            };

////            filterPanel.Controls.AddRange(new Control[]
////            {
////                new Label { Text = "Type:", AutoSize = true },
////                traitTypeFilter,
////                new Label { Text = "Species:", AutoSize = true },
////                speciesFilter
////            });

////            var buttonPanel = new FlowLayoutPanel
////            {
////                Dock = DockStyle.Bottom,
////                Height = 40,
////                FlowDirection = FlowDirection.LeftToRight,
////                Padding = new Padding(5)
////            };

////            buttonPanel.Controls.AddRange(new Control[]
////            {
////                addTraitButton,
////                editTraitButton,
////                assignTraitButton
////            });

////            traitManagementTab.Controls.Add(traitManagementLayout);
////            traitManagementLayout.Controls.Add(filterPanel, 0, 0);
////            traitManagementLayout.Controls.Add(traitRegistryGrid, 0, 1);
////            traitManagementLayout.Controls.Add(buttonPanel, 0, 2);

////            // Analysis tab layout
////            analysisTab.Controls.Add(litterAnalysisPanel);
////            analysisTab.Controls.Add(colonyStatsPanel);

////            var analysisToolbar = new FlowLayoutPanel
////            {
////                Dock = DockStyle.Top,
////                Height = 40,
////                FlowDirection = FlowDirection.LeftToRight,
////                Padding = new Padding(5)
////            };

////            analysisToolbar.Controls.AddRange(new Control[]
////            {
////                new Label { Text = "Filter:", AutoSize = true },
////                analysisFilterCombo,
////                generateReportButton
////            });

////            analysisTab.Controls.Add(analysisToolbar);

////            // Breeding tab layout
////            var breedingLayout = new TableLayoutPanel
////            {
////                Dock = DockStyle.Fill,
////                ColumnCount = 2,
////                RowCount = 3,
////                Padding = new Padding(10)
////            };

////            breedingLayout.Controls.Add(new Label { Text = "Parent 1:", AutoSize = true }, 0, 0);
////            breedingLayout.Controls.Add(dam1Combo, 1, 0);
////            breedingLayout.Controls.Add(new Label { Text = "Parent 2:", AutoSize = true }, 0, 1);
////            breedingLayout.Controls.Add(dam2Combo, 1, 1);
////            breedingLayout.Controls.Add(calculateButton, 1, 2);
////            breedingLayout.Controls.Add(resultPanel, 0, 3);
////            breedingLayout.SetColumnSpan(resultPanel, 2);

////            breedingTab.Controls.Add(breedingLayout);
////        }

////        private void RegisterEventHandlers()
////        {
////            this.Load += GeneticsPanel_Load;
////            addTraitButton.Click += AddTraitButton_Click;
////            editTraitButton.Click += EditTraitButton_Click;
////            assignTraitButton.Click += AssignTraitButton_Click;
////            calculateButton.Click += CalculateButton_Click;
////            generateReportButton.Click += GenerateReportButton_Click;
////            traitTypeFilter.SelectedIndexChanged += Filter_Changed;
////            speciesFilter.SelectedIndexChanged += Filter_Changed;
////        }

////        private async void GeneticsPanel_Load(object sender, EventArgs e)
////        {
////            await LoadTraitTypes();
////            await LoadSpecies();
////            await RefreshTraitGrid();
////        }

////        private async Task LoadTraitTypes()
////        {
////            var traitTypes = await _traitService.GetAllTraitTypesAsync();
////            traitTypeFilter.DataSource = traitTypes;
////            traitTypeFilter.DisplayMember = "Name";
////            traitTypeFilter.ValueMember = "Id";
////        }

////        private async Task LoadSpecies()
////        {
////            try
////            {
////                var speciesService = new SpeciesService(_context);
////                var species = await speciesService.GetAllSpeciesAsync();

////                speciesFilter.DataSource = species;
////                speciesFilter.DisplayMember = "CommonName";
////                speciesFilter.ValueMember = "Id";
////            }
////            catch (Exception ex)
////            {
////                MessageBox.Show($"Error loading species: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
////            }
////        }

////        private async Task RefreshTraitGrid()
////        {
////            try
////            {
////                var traits = await _traitService.GetAllTraitsAsync();

////                // Apply filters if selected
////                if (traitTypeFilter.SelectedItem != null)
////                {
////                    var selectedTraitType = (TraitType)traitTypeFilter.SelectedItem;
////                    traits = traits.Where(t => t.TraitTypeId == selectedTraitType.Id).ToList();
////                }

////                if (speciesFilter.SelectedItem != null)
////                {
////                    var selectedSpecies = (Species)speciesFilter.SelectedItem;
////                    traits = traits.Where(t => t.SpeciesID == selectedSpecies.Id).ToList();
////                }

////                traitRegistryGrid.DataSource = traits;
////            }
////            catch (Exception ex)
////            {
////                MessageBox.Show($"Error refreshing traits: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
////            }
////        }

////        private void AddTraitButton_Click(object sender, EventArgs e)
////        {
////            using (var form = new AddTraitForm(_traitService, _geneService, _context))
////            {
////                if (form.ShowDialog() == DialogResult.OK)
////                {
////                    RefreshTraitGrid();
////                }
////            }
////        }

////        private void EditTraitButton_Click(object sender, EventArgs e)
////        {
////            if (traitRegistryGrid.SelectedRows.Count == 0)
////            {
////                MessageBox.Show("Please select a trait to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
////                return;
////            }

////            var selectedTrait = (Trait)traitRegistryGrid.SelectedRows[0].DataBoundItem;
////            using (var form = new AddTraitForm(_traitService, _geneService, _context))
////            {
////                // TODO: Implement edit mode in AddTraitForm
////                if (form.ShowDialog() == DialogResult.OK)
////                {
////                    RefreshTraitGrid();
////                }
////            }
////        }

////        private void AssignTraitButton_Click(object sender, EventArgs e)
////        {
////            using (var form = new AssignTraitForm(_traitService, _geneService, new AnimalService(_context), _context))
////            {
////                if (form.ShowDialog() == DialogResult.OK)
////                {
////                    RefreshTraitGrid();
////                }
////            }
////        }

////        private async void Filter_Changed(object sender, EventArgs e)
////        {
////            await RefreshTraitGrid();
////        }

////        private void CalculateButton_Click(object sender, EventArgs e)
////        {
////            using (var form = new BreedingCalculatorForm(
////                _traitService,
////                _geneService,
////                _breedingService,
////                new AnimalService(_context),
////                _context))
////            {
////                form.ShowDialog();
////            }
////        }

////        private async void GenerateReportButton_Click(object sender, EventArgs e)
////        {
////            try
////            {
////                var traits = await _traitService.GetAllTraitsAsync();
////                var traitTypes = await _traitService.GetAllTraitTypesAsync();

////                var report = new StringBuilder();
////                report.AppendLine("Genetics Report");
////                report.AppendLine("==============");
////                report.AppendLine();

////                // Trait Type Summary
////                report.AppendLine("Trait Types Summary");
////                report.AppendLine("-----------------");
////                foreach (var traitType in traitTypes)
////                {
////                    var typeTraits = traits.Where(t => t.TraitTypeId == traitType.Id).ToList();
////                    report.AppendLine($"{traitType.Name}: {typeTraits.Count} traits");
////                }
////                report.AppendLine();

////                // Trait Details
////                report.AppendLine("Trait Details");
////                report.AppendLine("-------------");
////                foreach (var trait in traits)
////                {
////                    report.AppendLine($"Name: {trait.CommonName}");
////                    report.AppendLine($"Type: {trait.TraitType?.Name ?? "Unknown"}");
////                    report.AppendLine($"Genotype: {trait.Genotype ?? "Not specified"}");
////                    report.AppendLine();
////                }

////                // Save report
////                using (var saveDialog = new SaveFileDialog())
////                {
////                    saveDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
////                    saveDialog.FilterIndex = 1;
////                    saveDialog.DefaultExt = "txt";
////                    saveDialog.FileName = $"GeneticsReport_{DateTime.Now:yyyyMMdd}";

////                    if (saveDialog.ShowDialog() == DialogResult.OK)
////                    {
////                        await File.WriteAllTextAsync(saveDialog.FileName, report.ToString());
////                        MessageBox.Show("Report generated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
////                    }
////                }
////            }
////            catch (Exception ex)
////            {
////                MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
////            }
////        }

////        public Task RefreshDataAsync()
////        {
////            throw new NotImplementedException();
////        }
////    }
////}
