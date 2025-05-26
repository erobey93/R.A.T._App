using RATAPP.Forms;
using RATAPPLibrary.Data.Models;
using System;
using System.Drawing;
using System.Windows.Forms;
using RATAPPLibrary;
using RATAPPLibrary.Data.Models.Breeding;
using System.Transactions;
using RATAPPLibrary.Services;
using PdfSharp.Charting;
using Font = System.Drawing.Font;
using Point = System.Drawing.Point;
using RATAPPLibrary.Data.DbContexts;

namespace RATAPP.Panels
{
    /// <summary>
    /// Panel for managing breeding operations in the R.A.T. App.
    /// Provides tabbed interface for handling pairings, litters, and breeding lines.
    /// 
    /// Key Features:
    /// - Breeding Management:
    ///   * Create/track breeding pairs
    ///   * Monitor litters
    ///   * Manage breeding lines
    /// 
    /// UI Components:
    /// - Tabbed interface
    /// - Data grids for each tab
    /// - Search and filter controls
    /// - Action buttons
    /// 
    /// Data Management:
    /// - Asynchronous data loading
    /// - Cached breeding data
    /// - Real-time updates
    /// 
    /// Known Limitations:
    /// - Basic search functionality
    /// - Limited filter options
    /// - No bulk operations
    /// - Some TODO implementations
    /// 
    /// Dependencies:
    /// - BreedingService
    /// - AnimalService
    /// - LineService
    /// - StockService
    /// </summary>
    public partial class PairingsAndLittersPanel : Panel, INavigable
    {
        private TabControl tabControl;
        private TabPage pairingsTab;
        private TabPage littersTab;
        private TabPage linesTab;
        private TabPage projectsTab;
        private TextBox searchBox;
        private ComboBox filterComboBox;
        private Button searchButton;
        private DataGridView pairingsGridView;
        private DataGridView littersGridView;
        private DataGridView linesGridView;
        private DataGridView projectGridView;
        private Button addButton;
        private Button updateButton;
        private Button deleteButton;
        private PictureBox loadingSpinner;
        private Panel headerPanel; 

        private Pairing[] _pairings;
        private Litter[] _litters;
        private Line[] _lines;
        private Project[] _projects; 

        private RATAppBaseForm _parentForm;
        private BreedingService _breedingService;
        private AnimalService _animalService;
        private LineService _lineService;
        private StockService _stockService;
        private ProjectService _projectService;

        private readonly RatAppDbContextFactory _contextFactory;
        private readonly SemaphoreSlim _loadingSemaphore = new SemaphoreSlim(1, 1);
        //filter states 
        private string searchState = "all"; //use this variable to decide which data to get for now. Really I should be caching the data and then filtering the cached data vs. going to the db each time, but that's for a future iteration 
        private string currentSpeciesFilter = "All";
        private string currentLineFilter = "All";
        private string currentProjectFilter = "All";

        int topPanelHeight = 160; 

        public PairingsAndLittersPanel(RATAppBaseForm parentForm, RatAppDbContextFactory contextFactory)
        {
            _parentForm = parentForm;
            _contextFactory = contextFactory;
            _animalService = new AnimalService(contextFactory);
            _breedingService = new BreedingService(contextFactory);
            _lineService = new LineService(contextFactory);
            _stockService = new StockService(contextFactory);
            _projectService = new ProjectService(contextFactory);

            InitializeComponents();
            InitializeLoadingSpinner();
            InitializeHeaderPanel(); 
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
                Text = "Breeding Management",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 10)
            };

            Label descriptionLabel = new Label
            {
                Text = "View family trees and create pedigrees",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(25, 40)
            };

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(descriptionLabel);
            this.Controls.Add(headerPanel);
        }

        private void InitializeLoadingSpinner()
        {
            loadingSpinner = new PictureBox
            {
                Size = new Size(50, 50),
                Image = Image.FromFile("C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\Loading_2.gif"),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false
            };
            this.Controls.Add(loadingSpinner);
            this.Resize += (s, e) => CenterLoadingSpinner();
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

        private void CenterLoadingSpinner()
        {
            if (loadingSpinner != null)
            {
                loadingSpinner.Location = new Point(
                    (ClientSize.Width - loadingSpinner.Width) / 2,
                    (ClientSize.Height - loadingSpinner.Height) / 2
                );
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingSpinner != null)
            {
                loadingSpinner.Visible = true;
                CenterLoadingSpinner();
                this.Refresh();
            }
        }

        private void HideLoadingIndicator()
        {
            if (loadingSpinner != null)
            {
                loadingSpinner.Visible = false;
                this.Refresh();
            }
        }

        /// <summary>
        /// Initializes all UI components of the breeding management panel.
        /// Sets up tabbed interface and common controls.
        /// 
        /// Components:
        /// - Tab Control:
        ///   * Pairings tab
        ///   * Litters tab
        ///   * Line management tab
        /// 
        /// - Common Controls:
        ///   * Search box
        ///   * Filter dropdown
        ///   * Action buttons
        /// 
        /// Layout:
        /// - Dock-fill main container
        /// - Fixed-height control panels
        /// - Responsive data grids
        /// </summary>
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
            linesGridView = new DataGridView();
            projectGridView = new DataGridView(); 

            // Initialize Pairings Tab
            pairingsTab = new TabPage("Pairings");
            InitializePairingDataGridView();

            // Initialize Litters Tab
            littersTab = new TabPage("Litters");
            InitializeLitterDataGridView();

            // Initialize Line management tab (may change the name, but fine for now)
            linesTab = new TabPage("Line Management");
            InitializeLineDataGridView();

            // Initialize Project management tab (may change the name, but fine for now)
            projectsTab = new TabPage("Project Management");
            InitializeProjectDataGridView(); //need to make InitializeProjectDataGridView

            tabControl.TabPages.Add(pairingsTab);
            tabControl.TabPages.Add(littersTab);
            tabControl.TabPages.Add(linesTab);
            tabControl.TabPages.Add(projectsTab);

            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

            this.Controls.Add(tabControl);

            // Initialize common controls
            InitializeCommonControls();
        }

        private async Task GetAllBreedingData()
        {
            //get all litters
            //if no litters, initialize litters array with empty object 
            var getLitters = await _breedingService.GetAllLittersAsync();
            _litters = getLitters.ToArray();

            //get pairings
            var getPairings = await _breedingService.GetAllPairingsAsync();
            _pairings = getPairings.ToArray();

            //get lines 
            var getLines = await _lineService.GetAllLinesAsync();
            _lines = getLines.ToArray();

            //get projects TODO 
            var getProjects = await _projectService.GetAllProjectsAsync();
            _projects = getProjects.ToArray();
        }

        private async Task GetCurrentBreedingData()
        {
            //get all litters
            //if no litters, initialize litters array with empty object 
            var getLitters = await _breedingService.GetAllLittersAsync(); //TODO I don't think I have a "current" litters option maybe do something like if the litter is between 0 days and 8 weeks they are current, else they are past? 
            _litters = getLitters.ToArray();

            //get pairings
            var getPairings = await _breedingService.GetAllActivePairingsAsync();
            _pairings = getPairings.ToArray();

            //get lines TODO...maybe? 
            var getLines = await _lineService.GetAllLinesAsync(); //TODO I don't think that I have a "current" line option because that doesn't make sense in this context 
            _lines = getLines.ToArray();

            //get projects TODO
            var getProjects = await _projectService.GetAllProjectsAsync();
            _projects = getProjects.ToArray();
        }

        private async Task GetPastBreedingData()
        {
            //get all litters
            //if no litters, initialize litters array with empty object 
            var getLitters = await _breedingService.GetAllLittersAsync(); //TODO I don't think I have a "current" litters option
            _litters = getLitters.ToArray();

            //get pairings
            var getPairings = await _breedingService.GetAllPastPairingsAsync();
            _pairings = getPairings.ToArray();

            //get lines TODO...maybe? 
            var getLines = await _lineService.GetAllLinesAsync(); //TODO I don't think that I have a "current" line option 
            _lines = getLines.ToArray();

            //get projects TODO
            var getProjects = await _projectService.GetAllProjectsAsync();
            _projects = getProjects.ToArray();
        }

        private async void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                await _loadingSemaphore.WaitAsync();
                await LoadTabDataAsync(tabControl.SelectedIndex);
            }
            finally
            {
                _loadingSemaphore.Release();
            }
        }

        /// <summary>
        /// Loads data for the selected tab asynchronously.
        /// Handles loading indicators and error states.
        /// 
        /// Process:
        /// 1. Shows loading indicator
        /// 2. Fetches relevant data
        /// 3. Updates UI visibility
        /// 4. Populates data grid
        /// 
        /// Data Loading:
        /// - Pairings: Active and historical
        /// - Litters: All associated with pairs
        /// - Lines: All breeding lines
        /// 
        /// Error Handling:
        /// - Shows loading state
        /// - Handles empty data sets
        /// - Manages async errors
        /// </summary>
        private async Task LoadTabDataAsync(int tabIndex)
        {
            try
            {
                ShowLoadingIndicator();
                await GetAllBreedingData(); 
                

                // Update visibility
                pairingsGridView.Visible = tabIndex == 0;
                littersGridView.Visible = tabIndex == 1;
                linesGridView.Visible = tabIndex == 2;
                projectGridView.Visible = tabIndex == 3; 

                // Load data based on selected tab
                switch (tabIndex)
                {
                    case 0:
                        PopulatePairingDataDisplayArea();
                        break;
                    case 1:
                        PopulateLittersDataDisplayArea();
                        break;
                    case 2:
                        PopulateLineDataDisplayArea();
                        break;
                    case 3:
                        PopulateProjectDataDisplayArea();
                        break;
                }
            }
            finally
            {
                HideLoadingIndicator();
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
            //TODO bulk import button
            //TODO export? 

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

        private async void SearchButton_Click(object sender, EventArgs e)
        {
            // Implement search functionality
            //MessageBox.Show($"Searching for: {searchBox.Text}\nFilter: {filterComboBox.SelectedItem}");

            string filter = filterComboBox.SelectedItem.ToString();
            string searchTerm = searchBox.Text.ToLower();

            //if pairs tab
            //if(tabControl.SelectedIndex == 0)
            //{
                if(filter == "Current") //get all current pairings 
                {
                    searchState = "current";
                    PopulatePairingDataDisplayArea(); 
                    
                }
                else if(filter == "Past")
            {
                searchState = "past";
                PopulatePairingDataDisplayArea();
            }else if(filter == "All"){
                searchState = "all";
                PopulatePairingDataDisplayArea();
            }
            //}
            //get pairs based on filter first
            //then get based on search term
            //if litters tab
            //if lines tab 


            //var filteredLitters = _animals.Where(animal =>
            //    (speciesFilter == "All Species" || animal.species == speciesFilter) &&
            //    (sexFilter == "All Sexes" || animal.sex == sexFilter) &&
            //    (animal.name.ToLower().Contains(searchTerm) || animal.Id.ToString().Contains(searchTerm))
            //);

            //DataGridView(sender).Rows.Clear();
            //foreach (var animal in filteredAnimals)
            //{
            //    dataDisplayArea.Rows.Add(animal.species, animal.Id, animal.name, animal.sex, animal.DateOfBirth, animal.variety);
            //}
        }

        //TODO 
        //different functionality for adding pairings vs. litters so check that first 
        private async void ActionButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            string action = clickedButton.Text;
            string currentTab = tabControl.SelectedTab.Text;

            if(clickedButton.Text == "Add")
            {
                if (currentTab == "Pairings")
                {
                    AddPairingForm addPairing = await AddPairingForm.CreateAsync(_contextFactory);
                    addPairing.ShowDialog();
                    await LoadTabDataAsync(tabControl.SelectedIndex);
                }
                else if (currentTab == "Litters")
                {
                    AddLitterForm addLitter = await AddLitterForm.CreateAsync(_contextFactory);
                    addLitter.ShowDialog();
                    await LoadTabDataAsync(tabControl.SelectedIndex);
                }
                else if (currentTab == "Line Management")
                {
                    AddLineForm addLine = new AddLineForm(_contextFactory);
                    addLine.ShowDialog();
                    await LoadTabDataAsync(tabControl.SelectedIndex);
                }
                else if(currentTab == "Project Management")
                {
                    MessageBox.Show("Add Project Form Will Be Here"); 
                    //AddProjectForm addProject = await AddProjectForm.CreateAsync(_contextFactory); 
                    //addProject.ShowDialog()
                    //await LoadTabDataAsync(tabControl.SelectedIndex)
                }
            }
            else if (clickedButton.Text == "Update")
            {
                if (currentTab == "Pairings")
                {
                    MessageBox.Show("Update Pairing Form Will Be Here");
                    //TODO update is the same as add except for the data is filled in 
                    //UpdatePairingForm updatePairing = new UpdatePairingForm(_contextFactory);
                    //updatePairing.ShowDialog();
                    //await LoadTabDataAsync(tabControl.SelectedIndex);
                }
                else if (currentTab == "Litters")
                {
                    MessageBox.Show("Update Litters Form Will Be Here");
                    //TODO update is the same as add except for the data is filled in 
                    //UpdateLitterForm updateLitter = await UpdateLitterForm.CreateAsync(_contextFactory);
                    //updateLitter.ShowDialog();
                    //await LoadTabDataAsync(tabControl.SelectedIndex);
                }
                else if (currentTab == "Line Management")
                {
                    MessageBox.Show("Update Line Form Will Be Here");
                    //TODO
                    //AddLineForm addLine = new AddLineForm(_contextFactory);
                    //addLine.ShowDialog();
                    //await LoadTabDataAsync(tabControl.SelectedIndex);
                }
                else if (currentTab == "Project Management")
                {
                    MessageBox.Show("Update Project Form Will Be Here");
                    //TODO
                    //AddLineForm addLine = new AddLineForm(_contextFactory);
                    //addLine.ShowDialog();
                    //await LoadTabDataAsync(tabControl.SelectedIndex);
                }
            }
           
            else
            {
                MessageBox.Show($"{action} {currentTab}");
            }

            // Implement add, update, or delete functionality based on the action and current tab TODO 
        }

        public async Task RefreshDataAsync()
        {
            searchState = "all"; //TODO probably need to clear out the search box/filter drop down as well 
            // Implement data refresh logic TODO
            await GetAllBreedingData();
        }

        private void InitializePairingDataGridView()
        {
            //get all pairings from db 
            //I'm not sure about getting breeding data every time the tabs change, this should happen once when the app is first loaded and then be cached FIXME need to think through this logic more 

            pairingsGridView.Location = new Point(0, topPanelHeight);
            pairingsGridView.Width = 1000;
            pairingsGridView.Height = 400;
            pairingsGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            pairingsGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            pairingsGridView.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            pairingsGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            pairingsGridView.ReadOnly = true;

            pairingsGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "PairingId", HeaderText = "Pairing ID" },
                new DataGridViewTextBoxColumn { Name = "Doe", HeaderText = "Doe" },
                new DataGridViewTextBoxColumn { Name = "Buck", HeaderText = "Buck" },
                new DataGridViewTextBoxColumn { Name = "Project", HeaderText = "Project" },
                new DataGridViewTextBoxColumn { Name = "Pairing Date", HeaderText = "Pairing Date" },
                new DataGridViewTextBoxColumn { Name = "Pairing End Date", HeaderText = "Pairing End Date" },
                new DataGridViewButtonColumn { Name = "PairingPage", HeaderText = "Pairing Page", Text = "Go", UseColumnTextForButtonValue = true }
            });

            PopulatePairingDataDisplayArea();

            this.Controls.Add(pairingsGridView);
        }

        private async void PopulatePairingDataDisplayArea()
        {
            if (searchState == "all")
            {
                await GetAllBreedingData();
            }
            else if (searchState == "current")
            {
                await GetCurrentBreedingData();
            }
            else if (searchState == "past")
            {
                await GetPastBreedingData();
            }

            string dam = "Unknown";
            string sire = "Unknown";
            string projName = "Unknown"; //TODO there should always be a project, dam and sire name....

            pairingsGridView.Rows.Clear();
            if (_pairings != null)
            {
                foreach (var pairing in _pairings)
                {
                    if (pairing.Dam.Name != null)
                    {
                        dam = pairing.Dam.Name;
                    }
                    if (pairing.Sire != null)
                    {
                        sire = pairing.Sire.Name;
                    }
                    if (pairing.Project != null)
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

        private void InitializeProjectDataGridView()
        {
            //int topPanelHeight = 90;

            //get all projects from db 
            //I'm not sure about getting breeding data every time the tabs change, this should happen once when the app is first loaded and then be cached FIXME need to think through this logic more 

            projectGridView.Location = new Point(0, topPanelHeight);
            projectGridView.Width = 1000;
            projectGridView.Height = 400;
            projectGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            projectGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            projectGridView.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            projectGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            projectGridView.ReadOnly = true;

            projectGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "Project ID" },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Project Name" },
                new DataGridViewTextBoxColumn { Name = "LineId", HeaderText = "LineId" },
                //new DataGridViewTextBoxColumn { Name = "LineName", HeaderText = "LineId" }, TODO would be nice to have Line name but need to add new variable for this 
                new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description" },
                new DataGridViewTextBoxColumn { Name = "Notes", HeaderText = "Notes" },
                new DataGridViewTextBoxColumn { Name = "CreatedOn", HeaderText = "Created On" },
                new DataGridViewTextBoxColumn { Name = "LastUpdated", HeaderText = "Last Updated" },
                new DataGridViewButtonColumn { Name = "ProjectPage", HeaderText = "Project Page", Text = "Go", UseColumnTextForButtonValue = true }
            });

            PopulateProjectDataDisplayArea();

            this.Controls.Add(projectGridView);
        }

        private async void PopulateProjectDataDisplayArea()
        {
            if (searchState == "all")
            {
                await GetAllBreedingData();
            }
            else if (searchState == "current")
            {
                await GetCurrentBreedingData();
            }
            else if (searchState == "past")
            {
                await GetPastBreedingData();
            }

            projectGridView.Rows.Clear();
            if (_projects != null)
            {
                foreach (var project in _projects)
                {
                    projectGridView.Rows.Add(project.Id, project.Name, project.LineId, project.Description, project.Notes, project.CreatedOn, project.LastUpdated);
                }
            }
            else
            {
                MessageBox.Show("There are no pairings in your database.");
            }
        }

        private void InitializeLitterDataGridView()
        {
            //int topPanelHeight = 90;

            littersGridView.Location = new Point(0, topPanelHeight);
            littersGridView.Width = 1000;
            littersGridView.Height = 400;
            littersGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            littersGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            littersGridView.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            littersGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            littersGridView.ReadOnly = true;    

            littersGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "LitterId", HeaderText = "Litter ID" },
                new DataGridViewTextBoxColumn { Name = "LitterName", HeaderText = "Name/Theme" },
                //new DataGridViewTextBoxColumn { Name = "Species", HeaderText = "Species" }, //TODO check out what's up here 
                new DataGridViewTextBoxColumn { Name = "Project", HeaderText = "Project" }, //TODO or line if there is no project? probably just default project 
                new DataGridViewTextBoxColumn { Name = "Dam", HeaderText = "Dam" },
                new DataGridViewTextBoxColumn { Name = "Sire", HeaderText = "Sire" },
                new DataGridViewTextBoxColumn { Name = "DOB", HeaderText = "DOB" },
                new DataGridViewTextBoxColumn { Name = "NumPups", HeaderText = "Num Pups" },
                new DataGridViewButtonColumn { Name = "LitterPage", HeaderText = "Litter Page", Text = "Go", UseColumnTextForButtonValue = true }
            });

            PopulateLittersDataDisplayArea();

            this.Controls.Add(littersGridView);
        }

        private async void PopulateLittersDataDisplayArea()
        {
            if (searchState == "all")
            {
                await GetAllBreedingData();
            }
            else if (searchState == "current")
            {
                await GetCurrentBreedingData();
            }
            else if (searchState == "past")
            {
                await GetPastBreedingData();
            }

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
                MessageBox.Show("There are no litters in your database."); //FIXME this is popping up before the page is loaded, I only want it to pop up once on the page add as bug 
            }
        }

        //show all data related to lines 
        private async void InitializeLineDataGridView()
        {
            linesGridView.Location = new Point(0, topPanelHeight); // Changed to linesGridView
            linesGridView.Width = 1000; // Changed to linesGridView
            linesGridView.Height = 400; // Changed to linesGridView
            linesGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Changed to linesGridView
            linesGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize; // Changed to linesGridView
            linesGridView.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single; // Changed to linesGridView
            linesGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders; // Changed to linesGridView
            linesGridView.ReadOnly = true;

            linesGridView.Columns.Clear(); // Clear existing columns before adding new ones // Changed to linesGridView

            linesGridView.Columns.AddRange(new DataGridViewColumn[] // Changed to linesGridView
            {
                new DataGridViewTextBoxColumn { Name = "LineId", HeaderText = "Line ID" },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Line Name" },
                new DataGridViewTextBoxColumn { Name = "StockId", HeaderText = "Stock ID" }, // Assuming Stock ID is relevant
                new DataGridViewTextBoxColumn { Name = "StockName", HeaderText = "Stock Name" }, // Displaying Stock Name
                new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description" },
                new DataGridViewTextBoxColumn { Name = "Notes", HeaderText = "Notes" },
                new DataGridViewButtonColumn { Name = "LinePage", HeaderText = "Line Page", Text = "Go", UseColumnTextForButtonValue = true }
            });

            PopulateLineDataDisplayArea(); // Assuming this method now populates Line data

            this.Controls.Add(linesGridView); // Changed to linesGridView
        }

        //populate line data grid view via line + stock service 
        //stock is correlated with species, so that will auto populate based on the species that the user has selected
        //for now, we are assuming rats and mice only, in the future, it will be any species 
        private async void PopulateLineDataDisplayArea()
        {
            if (searchState == "all")
            {
                await GetAllBreedingData();
            }
            else if (searchState == "current")
            {
                await GetCurrentBreedingData();
            }
            else if (searchState == "past")
            {
                await GetPastBreedingData();
            }

            linesGridView.Rows.Clear();

            if (_lineService == null)
            {
                MessageBox.Show("Line service is not initialized.");
                return;
            }

            try
            {
                var lines = await _lineService.GetAllLinesAsync();

                if (lines != null && lines.Any()) // Check for null and empty list
                {
                    foreach (var line in lines)
                    {
                        // Fetch Stock Name using StockId (assuming you have a StockService)
                        string stockName = "Unknown";
                        if (line.StockId != null)
                        {
                            var stock = await _stockService.GetStockAsync_ById(line.StockId);
                            if (stock != null)
                            {
                                stockName = stock.Species.CommonName; //stock is about species, so there is no "name" just species name 
                            }
                        }

                        linesGridView.Rows.Add(
                            line.Id,
                            line.Name,
                            line.StockId,
                            stockName,
                            line.Description,
                            line.Notes,
                            "Edit" // Placeholder for edit button text
                        );
                    }
                }
                else
                {
                    MessageBox.Show("There are no lines in your database.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error populating line data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
