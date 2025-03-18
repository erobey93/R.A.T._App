using RATAPP.Forms;
using RATAPPLibrary.Data.Models;
using System;
using System.Drawing;
using System.Windows.Forms;
using RATAPPLibrary;
using RATAPPLibrary.Data.Models.Breeding;
using System.Transactions;

namespace RATAPP.Panels
{
    public partial class PairingsAndLittersPanel : Panel, INavigable
    {
        private TabControl tabControl;
        private TabPage pairingsTab;
        private TabPage littersTab;
        private TextBox searchBox;
        private ComboBox filterComboBox;
        private Button searchButton;
        private DataGridView pairingsGridView;
        private DataGridView littersGridView;
        private Button addButton;
        private Button updateButton;
        private Button deleteButton;
        //private DataGridView dataDisplayArea;

        private Pairing[] _pairings;
        private Litter[] _litters;
        private bool _littersGridView; 

        private RATAppBaseForm _parentForm;
        private RATAPPLibrary.Services.BreedingService _breedingService;
        private RATAPPLibrary.Services.AnimalService _animalService;
        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;

        public PairingsAndLittersPanel(RATAppBaseForm parentForm, RATAPPLibrary.Data.DbContexts.RatAppDbContext context)
        {
            _parentForm = parentForm;
            _context = context;
            _animalService = new RATAPPLibrary.Services.AnimalService(_context);
            _breedingService = new RATAPPLibrary.Services.BreedingService(_context);
            _littersGridView = false; //start with showing pairings page, when switched will show litters page 

            InitializeComponents();
            
        }

        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // Initialize TabControl
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F, FontStyle.Regular)
            };


           

            pairingsGridView = new DataGridView();
            littersGridView = new DataGridView(); //TODO need to better organize everything, i.e. come up with a common schema for how I initialize and pass around all controls 

            // Initialize Pairings Tab
            pairingsTab = new TabPage("Pairings");
            InitializePairingDataGridView();

            // Initialize Litters Tab
            littersTab = new TabPage("Litters");
            InitializeLitterDataGridView();

            tabControl.TabPages.Add(pairingsTab);
            tabControl.TabPages.Add(littersTab);

            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged; 

            this.Controls.Add(tabControl);

            // Initialize common controls
            InitializeCommonControls();
        }

        private async Task GetPairingsAndLitters()
        {
                //get all litters
                //if no litters, initialize litters array with empty object 
                var getLitters = await _breedingService.GetAllLittersAsync();
                _litters = getLitters.ToArray();
     
              //get pairings
                var getPairings = await _breedingService.GetAllPairingsAsync();
                _pairings = getPairings.ToArray();
        }

        private async void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex == 0) // Assuming pairings tab is index 0
            {
                pairingsGridView.Visible = true;
                littersGridView.Visible = false;
            }
            else if (tabControl.SelectedIndex == 1) // Assuming litters tab is index 1
            {
                pairingsGridView.Visible = false;
                littersGridView.Visible = true;

            }
        }

        private void InitializeCommonControls()
        {
            // Search and Filter Panel
            Panel searchFilterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10)
            };

            searchBox = new TextBox
            {
                Width = 200,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(10, 15)
            };

            filterComboBox = new ComboBox
            {
                Width = 150,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(220, 15),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            filterComboBox.Items.AddRange(new object[] { "All", "Current", "Past", "Future", "Species", "Line", "Project" });
            filterComboBox.SelectedIndex = 0;

            searchButton = new Button
            {
                Text = "Search",
                Font = new Font("Segoe UI", 10F),
                Height = 30,
                Location = new Point(380, 13),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            searchButton.FlatAppearance.BorderSize = 0;
            searchButton.Click += SearchButton_Click;

            searchFilterPanel.Controls.Add(searchBox);
            searchFilterPanel.Controls.Add(filterComboBox);
            searchFilterPanel.Controls.Add(searchButton);

            // Action Buttons Panel
            Panel actionButtonsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10)
            };

            addButton = CreateActionButton("Add", 10);
            updateButton = CreateActionButton("Update", 120);
            deleteButton = CreateActionButton("Delete", 230);

            actionButtonsPanel.Controls.Add(addButton);
            actionButtonsPanel.Controls.Add(updateButton);
            actionButtonsPanel.Controls.Add(deleteButton);

            this.Controls.Add(searchFilterPanel);
            this.Controls.Add(actionButtonsPanel);
        }

        private Button CreateActionButton(string text, int x)
        {
            Button button = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(x, 13),
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += ActionButton_Click;
            return button;
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            // Implement search functionality
            MessageBox.Show($"Searching for: {searchBox.Text}\nFilter: {filterComboBox.SelectedItem}");
        }

        //TODO 
        private void ActionButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            string action = clickedButton.Text;
            string currentTab = tabControl.SelectedTab.Text;

            MessageBox.Show($"{action} {currentTab}");
            // Implement add, update, or delete functionality based on the action and current tab TODO 
        }

        public Task RefreshDataAsync()
        {
            // Implement data refresh logic TODO
            return Task.CompletedTask;
        }

        private async void InitializePairingDataGridView()
        {
            int topPanelHeight = 90;
            //get all pairings and litters from db 
            await GetPairingsAndLitters();


            pairingsGridView.Location = new Point(0, topPanelHeight);
            pairingsGridView.Width = 1000;
            pairingsGridView.Height = 400;
            pairingsGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            pairingsGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            pairingsGridView.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            pairingsGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;

            pairingsGridView.Columns.AddRange(new DataGridViewColumn[]
            {
        new DataGridViewTextBoxColumn { Name = "PairingId", HeaderText = "Pairing ID" },
        new DataGridViewTextBoxColumn { Name = "Doe", HeaderText = "Doe" },
        new DataGridViewTextBoxColumn { Name = "Buck", HeaderText = "Buck" },
        new DataGridViewTextBoxColumn { Name = "Project", HeaderText = "Project" },
        new DataGridViewTextBoxColumn { Name = "Pairing Date", HeaderText = "Pairing Date" },
        new DataGridViewTextBoxColumn { Name = "Pairing End Date", HeaderText = "Pairing End Date" },
        new DataGridViewButtonColumn { Name = "Edit", HeaderText = "Edit Pairing", Text = "Edit", UseColumnTextForButtonValue = true }
            });

            PopulatePairingDataDisplayArea();

            this.Controls.Add(pairingsGridView);
        }

        private void PopulatePairingDataDisplayArea()
        {
            string dam = "Unknown";
            string sire = "Unknown";
            string projName = "Unknown"; //TODO there should always be a project, dam and sire name....

            pairingsGridView.Rows.Clear();
            if (_pairings != null)
            {
                foreach (var pairing in _pairings)
                {
                    if(pairing.Dam.Name != null)
                    {
                        dam = pairing.Dam.Name;
                    }
                    if(pairing.Sire != null)
                    {
                        sire = pairing.Sire.Name;
                    }
                    if(pairing.Project != null)
                    {
                        projName = pairing.Project.Name;
                    }
                    pairingsGridView.Rows.Add(pairing.pairingId, dam, sire, projName, pairing.PairingStartDate, pairing.PairingEndDate, "TODO - edit pairing button?");
                }
            }
            else
            {
                MessageBox.Show("There are no pairings in your database.");
            }
        }

        private async void InitializeLitterDataGridView()
        {
            int topPanelHeight = 90;

            littersGridView.Location = new Point(0,topPanelHeight );
            littersGridView.Width = 1000;
            littersGridView.Height = 400;
            littersGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            littersGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            littersGridView.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            littersGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;

            littersGridView.Columns.AddRange(new DataGridViewColumn[]
            {
        new DataGridViewTextBoxColumn { Name = "LitterId", HeaderText = "Litter ID" },
        new DataGridViewTextBoxColumn { Name = "LitterName", HeaderText = "Name/Theme" },
        //new DataGridViewTextBoxColumn { Name = "Species", HeaderText = "Species" },
        new DataGridViewTextBoxColumn { Name = "Project", HeaderText = "Project" }, //TODO or line if there is no project? 
        new DataGridViewTextBoxColumn { Name = "Dam", HeaderText = "Dam" },
        new DataGridViewTextBoxColumn { Name = "Sire", HeaderText = "Sire" },
        new DataGridViewTextBoxColumn { Name = "DOB", HeaderText = "DOB" },
        new DataGridViewTextBoxColumn { Name = "NumPups", HeaderText = "Num Pups" },
        new DataGridViewButtonColumn { Name = "EditLitter", HeaderText = "Edit Litter", Text = "Edit", UseColumnTextForButtonValue = true }
            });

            PopulateLittersDataDisplayArea();

            this.Controls.Add(littersGridView);
        }

        private void PopulateLittersDataDisplayArea()
        {
            littersGridView.Rows.Clear();
            if (_litters != null)
            {
                foreach (var litter in _litters)
                {
                    littersGridView.Rows.Add(litter.LitterId, litter.Name, litter.Pair.Project.Name, litter.Pair.Dam, litter.Pair.Sire, litter.DateOfBirth, litter.NumPups, "TODO - edit litter button?");
                }
            }
            else
            {
                MessageBox.Show("There are no litters in your database.");
            }
        }
    }
}

