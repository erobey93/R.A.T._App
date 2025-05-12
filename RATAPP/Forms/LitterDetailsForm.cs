using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Services;
using System.Drawing;
using System.Windows.Forms;

namespace RATAPP.Forms
{
    public partial class LitterDetailsForm : RATAppBaseForm
    {
        private readonly RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
        private readonly string _litterId;
        private readonly BreedingService _breedingService;
        private readonly TraitService _traitService;
        private readonly LineageService _lineageService;
        private readonly AnimalService _animalService;
        private readonly LineService _lineService;

        private TabControl mainTabControl;
        private TabPage basicInfoTab;
        private TabPage parentsTab;
        private TabPage lineageTab;
        private TabPage pupsTab;
        private TabPage geneticsTab;

        private Label litterIdLabel;
        private Label nameLabel;
        private Label dobLabel;
        private Label totalPupsLabel;
        private Label notesLabel;
        private TextBox litterIdTextBox;
        private TextBox nameTextBox;
        private TextBox dobTextBox;
        private TextBox totalPupsTextBox;
        private TextBox notesTextBox;

        private DataGridView pupsGridView;
        private Panel parentsPanel;
        private Panel lineagePanel;
        private Panel geneticsPanel;

        public LitterDetailsForm(RATAPPLibrary.Data.DbContexts.RatAppDbContext context, string litterId)
        {
            _context = context;
            _litterId = litterId;
            _breedingService = new BreedingService(_context);
            _traitService = new TraitService(_context);
            _lineageService = new LineageService(_context);
            _animalService = new AnimalService(_context);
            _lineService = new LineService(_context);

            InitializeComponents();
            LoadLitterData();
        }

        private void InitializeComponents()
        {
            this.Text = "Litter Details";
            this.Size = new Size(1000, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Initialize TabControl
            mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F)
            };

            // Initialize TabPages
            basicInfoTab = new TabPage("Basic Information");
            parentsTab = new TabPage("Parents");
            lineageTab = new TabPage("Lineage");
            pupsTab = new TabPage("Pups");
            geneticsTab = new TabPage("Genetics");

            InitializeBasicInfoTab();
            InitializeParentsTab();
            InitializeLineageTab();
            InitializePupsTab();
            InitializeGeneticsTab();

            mainTabControl.TabPages.AddRange(new TabPage[] {
                basicInfoTab,
                parentsTab,
                lineageTab,
                pupsTab,
                geneticsTab
            });

            this.Controls.Add(mainTabControl);
        }

        private void InitializeBasicInfoTab()
        {
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(10)
            };

            // Set column widths
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            // Create and add labels
            litterIdLabel = new Label { Text = "Litter ID:", Dock = DockStyle.Fill };
            nameLabel = new Label { Text = "Name/Theme:", Dock = DockStyle.Fill };
            dobLabel = new Label { Text = "Date of Birth:", Dock = DockStyle.Fill };
            totalPupsLabel = new Label { Text = "Total Pups:", Dock = DockStyle.Fill };
            notesLabel = new Label { Text = "Notes:", Dock = DockStyle.Fill };

            // Create and add read-only textboxes
            litterIdTextBox = new TextBox { ReadOnly = true, Dock = DockStyle.Fill };
            nameTextBox = new TextBox { ReadOnly = true, Dock = DockStyle.Fill };
            dobTextBox = new TextBox { ReadOnly = true, Dock = DockStyle.Fill };
            totalPupsTextBox = new TextBox { ReadOnly = true, Dock = DockStyle.Fill };
            notesTextBox = new TextBox { ReadOnly = true, Dock = DockStyle.Fill, Multiline = true, Height = 100 };

            // Add controls to layout
            layout.Controls.Add(litterIdLabel, 0, 0);
            layout.Controls.Add(litterIdTextBox, 1, 0);
            layout.Controls.Add(nameLabel, 0, 1);
            layout.Controls.Add(nameTextBox, 1, 1);
            layout.Controls.Add(dobLabel, 0, 2);
            layout.Controls.Add(dobTextBox, 1, 2);
            layout.Controls.Add(totalPupsLabel, 0, 3);
            layout.Controls.Add(totalPupsTextBox, 1, 3);
            layout.Controls.Add(notesLabel, 0, 4);
            layout.Controls.Add(notesTextBox, 1, 4);

            basicInfoTab.Controls.Add(layout);
        }

        private void InitializeParentsTab()
        {
            parentsPanel = new Panel { Dock = DockStyle.Fill };
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };

            // Create panels for dam and sire
            Panel damPanel = CreateParentPanel("Dam Information");
            Panel sirePanel = CreateParentPanel("Sire Information");

            layout.Controls.Add(damPanel, 0, 0);
            layout.Controls.Add(sirePanel, 1, 0);

            parentsPanel.Controls.Add(layout);
            parentsTab.Controls.Add(parentsPanel);
        }

        private Panel CreateParentPanel(string title)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Height = 30
            };

            DataGridView traitsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };

            traitsGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "TraitType", HeaderText = "Trait Type" },
                new DataGridViewTextBoxColumn { Name = "Value", HeaderText = "Value" }
            });

            panel.Controls.Add(traitsGrid);
            panel.Controls.Add(titleLabel);

            return panel;
        }

        private void InitializeLineageTab()
        {
            lineagePanel = new Panel { Dock = DockStyle.Fill };
            // Will be populated with lineage data when loading
            lineageTab.Controls.Add(lineagePanel);
        }

        private void InitializePupsTab()
        {
            pupsGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };

            pupsGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "ID", HeaderText = "ID" },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name" },
                new DataGridViewTextBoxColumn { Name = "Gender", HeaderText = "Gender" },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status" }
            });

            pupsTab.Controls.Add(pupsGridView);
        }

        private void InitializeGeneticsTab()
        {
            geneticsPanel = new Panel { Dock = DockStyle.Fill };
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };

            // Create sections for phenotypes, inherited traits, and potential genotypes
            Panel phenotypePanel = CreateGeneticsSection("Phenotype Distribution");
            Panel inheritedPanel = CreateGeneticsSection("Inherited Traits");
            Panel genotypePanel = CreateGeneticsSection("Potential Genotypes");

            layout.Controls.Add(phenotypePanel, 0, 0);
            layout.Controls.Add(inheritedPanel, 0, 1);
            layout.Controls.Add(genotypePanel, 0, 2);

            geneticsPanel.Controls.Add(layout);
            geneticsTab.Controls.Add(geneticsPanel);
        }

        private Panel CreateGeneticsSection(string title)
        {
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Height = 30
            };

            panel.Controls.Add(titleLabel);
            return panel;
        }

        private async void LoadLitterData()
        {
            try
            {
                // Get litter details
                var litters = await _breedingService.GetAllLittersAsync();
                var litter = litters.FirstOrDefault(l => l.LitterId == _litterId);

                if (litter == null)
                {
                    MessageBox.Show("Litter not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                // Load basic information
                LoadBasicInfo(litter);

                // Load parents information
                await LoadParentsInfo(litter);

                // Load lineage information
                await LoadLineageInfo(litter);

                // Load pups information
                await LoadPupsInfo(litter);

                // Load genetics information
                await LoadGeneticsInfo(litter);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading litter data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBasicInfo(Litter litter)
        {
            litterIdTextBox.Text = litter.LitterId;
            nameTextBox.Text = litter.Name;
            dobTextBox.Text = litter.DateOfBirth?.ToShortDateString() ?? "Unknown";
            totalPupsTextBox.Text = $"Total: {litter.NumPups ?? 0} (♀:{litter.NumFemale ?? 0} ♂:{litter.NumMale ?? 0})";
            notesTextBox.Text = litter.Notes ?? "";
        }

        private async Task LoadParentsInfo(Litter litter)
        {
            if (litter.Pair != null)
            {
                var damTraits = await _traitService.GetTraitMapForSingleAnimal(litter.Pair.Dam.Id);
                var sireTraits = await _traitService.GetTraitMapForSingleAnimal(litter.Pair.Sire.Id);

                // Update dam panel
                var damPanel = (Panel)parentsPanel.Controls[0].Controls[0];
                var damGrid = (DataGridView)damPanel.Controls[0];
                PopulateTraitsGrid(damGrid, damTraits);

                // Update sire panel
                var sirePanel = (Panel)parentsPanel.Controls[0].Controls[1];
                var sireGrid = (DataGridView)sirePanel.Controls[0];
                PopulateTraitsGrid(sireGrid, sireTraits);
            }
        }

        private void PopulateTraitsGrid(DataGridView grid, Dictionary<string, List<string>> traits)
        {
            grid.Rows.Clear();
            foreach (var trait in traits)
            {
                foreach (var value in trait.Value)
                {
                    grid.Rows.Add(trait.Key, value);
                }
            }
        }

        private async Task LoadLineageInfo(Litter litter)
        {
            if (litter.Pair == null) return;

            // Clear existing controls
            lineagePanel.Controls.Clear();

            // Create a panel for the lineage visualization
            Panel visualizerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            lineagePanel.Controls.Add(visualizerPanel);

            // Add paint handler for the lineage visualization
            visualizerPanel.Paint += async (sender, e) =>
            {
                using (var visualizer = new Helpers.LineageVisualizer(visualizerPanel, _lineageService))
                {
                    // Draw dam's lineage on the left side
                    if (litter.Pair.Dam != null)
                    {
                        await visualizer.DrawLineage(
                            litter.Pair.Dam,
                            visualizerPanel.Width / 4 - 75,  // Center in left half
                            visualizerPanel.Height - 100     // Near bottom
                        );
                    }

                    // Draw sire's lineage on the right side
                    if (litter.Pair.Sire != null)
                    {
                        await visualizer.DrawLineage(
                            litter.Pair.Sire,
                            (visualizerPanel.Width * 3) / 4 - 75,  // Center in right half
                            visualizerPanel.Height - 100           // Near bottom
                        );
                    }
                }
            };

            // Force the panel to redraw
            visualizerPanel.Invalidate();
        }

        private async Task LoadPupsInfo(Litter litter)
        {
            pupsGridView.Rows.Clear();
            if (litter.Animals != null)
            {
                foreach (var pup in litter.Animals)
                {
                    pupsGridView.Rows.Add(
                        pup.Id,
                        pup.Name,
                        pup.Gender,
                        pup.Status
                    );
                }
            }
        }

        private async Task LoadGeneticsInfo(Litter litter)
        {
            if (litter.Pair == null) return;

            var phenotypePanel = (Panel)geneticsPanel.Controls[0].Controls[0];
            var inheritedPanel = (Panel)geneticsPanel.Controls[0].Controls[1];
            var genotypePanel = (Panel)geneticsPanel.Controls[0].Controls[2];

            // Get traits for parents
            var damTraits = await _traitService.GetTraitMapForSingleAnimal(litter.Pair.Dam.Id);
            var sireTraits = await _traitService.GetTraitMapForSingleAnimal(litter.Pair.Sire.Id);

            // Calculate phenotype distribution
            Dictionary<string, int> phenotypeDistribution = new Dictionary<string, int>();
            if (litter.Animals != null)
            {
                foreach (var pup in litter.Animals)
                {
                    var pupTraits = await _traitService.GetTraitMapForSingleAnimal(pup.Id);
                    foreach (var traitType in pupTraits)
                    {
                        foreach (var trait in traitType.Value)
                        {
                            string key = $"{traitType.Key}: {trait}";
                            if (!phenotypeDistribution.ContainsKey(key))
                                phenotypeDistribution[key] = 0;
                            phenotypeDistribution[key]++;
                        }
                    }
                }
            }

            // Display phenotype distribution
            var phenotypeList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details
            };
            phenotypeList.Columns.Add("Trait", 200);
            phenotypeList.Columns.Add("Count", 100);
            phenotypeList.Columns.Add("Percentage", 100);

            foreach (var phenotype in phenotypeDistribution)
            {
                float percentage = (float)phenotype.Value / (litter.NumPups ?? 1) * 100;
                phenotypeList.Items.Add(new ListViewItem(new[] {
                    phenotype.Key,
                    phenotype.Value.ToString(),
                    $"{percentage:F1}%"
                }));
            }
            phenotypePanel.Controls.Add(phenotypeList);

            // Display inherited traits
            var inheritedList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details
            };
            inheritedList.Columns.Add("Trait Type", 150);
            inheritedList.Columns.Add("Dam", 150);
            inheritedList.Columns.Add("Sire", 150);

            // Combine trait types from both parents
            var traitTypes = new HashSet<string>();
            foreach (var type in damTraits.Keys.Concat(sireTraits.Keys))
                traitTypes.Add(type);

            foreach (var traitType in traitTypes)
            {
                string damValue = damTraits.ContainsKey(traitType) ? string.Join(", ", damTraits[traitType]) : "N/A";
                string sireValue = sireTraits.ContainsKey(traitType) ? string.Join(", ", sireTraits[traitType]) : "N/A";
                inheritedList.Items.Add(new ListViewItem(new[] { traitType, damValue, sireValue }));
            }
            inheritedPanel.Controls.Add(inheritedList);

            // Display potential genotypes
            var genotypeList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details
            };
            genotypeList.Columns.Add("Trait", 200);
            genotypeList.Columns.Add("Possible Genotypes", 300);

            // For now, just display the known genotypes from parents
            // In the future, this could be expanded to show Punnett squares or calculated probabilities
            foreach (var traitType in traitTypes)
            {
                string damGenotype = damTraits.ContainsKey(traitType) ? string.Join("/", damTraits[traitType]) : "Unknown";
                string sireGenotype = sireTraits.ContainsKey(traitType) ? string.Join("/", sireTraits[traitType]) : "Unknown";
                genotypeList.Items.Add(new ListViewItem(new[] {
                    traitType,
                    $"Dam: {damGenotype}, Sire: {sireGenotype}"
                }));
            }
            genotypePanel.Controls.Add(genotypeList);
        }
    }
}
