using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Services;
using System.Drawing;
using System.Windows.Forms;

namespace RATAPP.Forms
{
    public partial class PairingDetailsForm : RATAppBaseForm
    {
        private readonly RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
        private readonly string _pairingId;
        private readonly BreedingService _breedingService;
        private readonly TraitService _traitService;
        private readonly LineageService _lineageService;
        private readonly AnimalService _animalService;
        private readonly ProjectService _projectService;

        private TabControl mainTabControl;
        private TabPage basicInfoTab;
        private TabPage animalsTab;
        private TabPage littersTab;
        private TabPage geneticsTab;

        private Label pairingIdLabel;
        private Label projectLabel;
        private Label startDateLabel;
        private Label endDateLabel;
        private Label notesLabel;
        private TextBox pairingIdTextBox;
        private TextBox projectTextBox;
        private TextBox startDateTextBox;
        private TextBox endDateTextBox;
        private TextBox notesTextBox;

        private Panel animalsPanel;
        private DataGridView littersGridView;
        private Panel geneticsPanel;

        public PairingDetailsForm(RATAPPLibrary.Data.DbContexts.RatAppDbContext context, string pairingId)
        {
            _context = context;
            _pairingId = pairingId;
            _breedingService = new BreedingService(_context);
            _traitService = new TraitService(_context);
            _lineageService = new LineageService(_context);
            _animalService = new AnimalService(_context);
            _projectService = new ProjectService(_context);

            InitializeComponents();
            LoadPairingData();
        }

        private void InitializeComponents()
        {
            this.Text = "Pairing Details";
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
            animalsTab = new TabPage("Animals");
            littersTab = new TabPage("Litters");
            geneticsTab = new TabPage("Genetics");

            InitializeBasicInfoTab();
            InitializeAnimalsTab();
            InitializeLittersTab();
            InitializeGeneticsTab();

            mainTabControl.TabPages.AddRange(new TabPage[] {
                basicInfoTab,
                animalsTab,
                littersTab,
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
            pairingIdLabel = new Label { Text = "Pairing ID:", Dock = DockStyle.Fill };
            projectLabel = new Label { Text = "Project:", Dock = DockStyle.Fill };
            startDateLabel = new Label { Text = "Start Date:", Dock = DockStyle.Fill };
            endDateLabel = new Label { Text = "End Date:", Dock = DockStyle.Fill };
            notesLabel = new Label { Text = "Notes:", Dock = DockStyle.Fill };

            // Create and add read-only textboxes
            pairingIdTextBox = new TextBox { ReadOnly = true, Dock = DockStyle.Fill };
            projectTextBox = new TextBox { ReadOnly = true, Dock = DockStyle.Fill };
            startDateTextBox = new TextBox { ReadOnly = true, Dock = DockStyle.Fill };
            endDateTextBox = new TextBox { ReadOnly = true, Dock = DockStyle.Fill };
            notesTextBox = new TextBox { ReadOnly = true, Dock = DockStyle.Fill, Multiline = true, Height = 100 };

            // Add controls to layout
            layout.Controls.Add(pairingIdLabel, 0, 0);
            layout.Controls.Add(pairingIdTextBox, 1, 0);
            layout.Controls.Add(projectLabel, 0, 1);
            layout.Controls.Add(projectTextBox, 1, 1);
            layout.Controls.Add(startDateLabel, 0, 2);
            layout.Controls.Add(startDateTextBox, 1, 2);
            layout.Controls.Add(endDateLabel, 0, 3);
            layout.Controls.Add(endDateTextBox, 1, 3);
            layout.Controls.Add(notesLabel, 0, 4);
            layout.Controls.Add(notesTextBox, 1, 4);

            basicInfoTab.Controls.Add(layout);
        }

        private void InitializeAnimalsTab()
        {
            animalsPanel = new Panel { Dock = DockStyle.Fill };
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10)
            };

            // Create panels for dam and sire info
            Panel damInfoPanel = CreateAnimalInfoPanel("Dam Information");
            Panel sireInfoPanel = CreateAnimalInfoPanel("Sire Information");

            // Create panels for dam and sire lineage
            Panel damLineagePanel = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
            Panel sireLineagePanel = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };

            Label damLineageLabel = new Label
            {
                Text = "Dam's Lineage",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Height = 30
            };

            Label sireLineageLabel = new Label
            {
                Text = "Sire's Lineage",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Height = 30
            };

            damLineagePanel.Controls.Add(damLineageLabel);
            sireLineagePanel.Controls.Add(sireLineageLabel);

            layout.Controls.Add(damInfoPanel, 0, 0);
            layout.Controls.Add(sireInfoPanel, 1, 0);
            layout.Controls.Add(damLineagePanel, 0, 1);
            layout.Controls.Add(sireLineagePanel, 1, 1);

            animalsPanel.Controls.Add(layout);
            animalsTab.Controls.Add(animalsPanel);
        }

        private Panel CreateAnimalInfoPanel(string title)
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

        private void InitializeLittersTab()
        {
            littersGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };

            littersGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "LitterId", HeaderText = "Litter ID" },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name/Theme" },
                new DataGridViewTextBoxColumn { Name = "DOB", HeaderText = "Date of Birth" },
                new DataGridViewTextBoxColumn { Name = "NumPups", HeaderText = "Number of Pups" },
                new DataGridViewButtonColumn { Name = "Details", HeaderText = "Details", Text = "Go", UseColumnTextForButtonValue = true }
            });

            littersGridView.CellClick += LittersGridView_CellClick;
            littersTab.Controls.Add(littersGridView);
        }

        private void InitializeGeneticsTab()
        {
            geneticsPanel = new Panel { Dock = DockStyle.Fill };
            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };

            // Create sections for parent traits and potential offspring traits
            Panel parentTraitsPanel = CreateGeneticsSection("Parent Traits");
            Panel potentialTraitsPanel = CreateGeneticsSection("Potential Offspring Traits");

            layout.Controls.Add(parentTraitsPanel, 0, 0);
            layout.Controls.Add(potentialTraitsPanel, 0, 1);

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

        private async void LoadPairingData()
        {
            try
            {
                // Get pairing details
                var pairings = await _breedingService.GetAllPairingsAsync();
                var pairing = pairings.FirstOrDefault(p => p.pairingId == _pairingId);

                if (pairing == null)
                {
                    MessageBox.Show("Pairing not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                // Load basic information
                LoadBasicInfo(pairing);

                // Load animals information
                await LoadAnimalsInfo(pairing);

                // Load litters information
                await LoadLittersInfo(pairing);

                // Load genetics information
                await LoadGeneticsInfo(pairing);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading pairing data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadBasicInfo(Pairing pairing)
        {
            pairingIdTextBox.Text = pairing.pairingId;
            projectTextBox.Text = pairing.Project?.Name ?? "Unknown";
            startDateTextBox.Text = pairing.PairingStartDate?.ToShortDateString() ?? "Unknown";
            endDateTextBox.Text = pairing.PairingEndDate?.ToShortDateString() ?? "Ongoing";
            notesTextBox.Text = pairing.Notes ?? "";
        }

        private async Task LoadAnimalsInfo(Pairing pairing)
        {
            if (pairing.Dam != null)
            {
                var damTraits = await _traitService.GetTraitMapForSingleAnimal(pairing.Dam.Id);
                var damPanel = (Panel)animalsPanel.Controls[0].Controls[0];
                var damGrid = (DataGridView)damPanel.Controls[0];
                PopulateTraitsGrid(damGrid, damTraits);

                // Draw dam's lineage
                var damLineagePanel = (Panel)animalsPanel.Controls[0].Controls[2];
                await DrawLineage(damLineagePanel, pairing.Dam);
            }

            if (pairing.Sire != null)
            {
                var sireTraits = await _traitService.GetTraitMapForSingleAnimal(pairing.Sire.Id);
                var sirePanel = (Panel)animalsPanel.Controls[0].Controls[1];
                var sireGrid = (DataGridView)sirePanel.Controls[0];
                PopulateTraitsGrid(sireGrid, sireTraits);

                // Draw sire's lineage
                var sireLineagePanel = (Panel)animalsPanel.Controls[0].Controls[3];
                await DrawLineage(sireLineagePanel, pairing.Sire);
            }
        }

        private async Task DrawLineage(Panel panel, Animal animal)
        {
            using (var visualizer = new Helpers.LineageVisualizer(panel, _lineageService))
            {
                await visualizer.DrawLineage(
                    animal,
                    panel.Width / 2 - 75,  // Center horizontally
                    panel.Height - 100     // Near bottom
                );
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

        private async Task LoadLittersInfo(Pairing pairing)
        {
            littersGridView.Rows.Clear();
            var litters = await _breedingService.GetAllLittersAsync();
            var pairingLitters = litters.Where(l => l.PairId == pairing.Id);

            foreach (var litter in pairingLitters)
            {
                littersGridView.Rows.Add(
                    litter.LitterId,
                    litter.Name,
                    litter.DateOfBirth?.ToShortDateString() ?? "Unknown",
                    litter.NumPups ?? 0
                );
            }
        }

        private void LittersGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == littersGridView.Columns["Details"].Index && e.RowIndex >= 0)
            {
                string litterId = littersGridView.Rows[e.RowIndex].Cells["LitterId"].Value.ToString();
                LitterDetailsForm detailsForm = new LitterDetailsForm(_context, litterId);
                detailsForm.ShowDialog();
            }
        }

        private async Task LoadGeneticsInfo(Pairing pairing)
        {
            if (pairing.Dam == null || pairing.Sire == null) return;

            var parentTraitsPanel = (Panel)geneticsPanel.Controls[0].Controls[0];
            var potentialTraitsPanel = (Panel)geneticsPanel.Controls[0].Controls[1];

            // Get traits for both parents
            var damTraits = await _traitService.GetTraitMapForSingleAnimal(pairing.Dam.Id);
            var sireTraits = await _traitService.GetTraitMapForSingleAnimal(pairing.Sire.Id);

            // Display parent traits
            var parentList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details
            };
            parentList.Columns.Add("Trait Type", 150);
            parentList.Columns.Add("Dam", 150);
            parentList.Columns.Add("Sire", 150);

            // Combine trait types from both parents
            var traitTypes = new HashSet<string>();
            foreach (var type in damTraits.Keys.Concat(sireTraits.Keys))
                traitTypes.Add(type);

            foreach (var traitType in traitTypes)
            {
                string damValue = damTraits.ContainsKey(traitType) ? string.Join(", ", damTraits[traitType]) : "N/A";
                string sireValue = sireTraits.ContainsKey(traitType) ? string.Join(", ", sireTraits[traitType]) : "N/A";
                parentList.Items.Add(new ListViewItem(new[] { traitType, damValue, sireValue }));
            }
            parentTraitsPanel.Controls.Add(parentList);

            // Display potential offspring traits
            var potentialList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details
            };
            potentialList.Columns.Add("Trait Type", 200);
            potentialList.Columns.Add("Possible Combinations", 300);

            foreach (var traitType in traitTypes)
            {
                var damValues = damTraits.ContainsKey(traitType) ? damTraits[traitType] : new List<string> { "Unknown" };
                var sireValues = sireTraits.ContainsKey(traitType) ? sireTraits[traitType] : new List<string> { "Unknown" };

                var combinations = new List<string>();
                foreach (var dValue in damValues)
                    foreach (var sValue in sireValues)
                        combinations.Add($"{dValue} x {sValue}");

                potentialList.Items.Add(new ListViewItem(new[] {
                    traitType,
                    string.Join(", ", combinations)
                }));
            }
            potentialTraitsPanel.Controls.Add(potentialList);
        }
    }
}
