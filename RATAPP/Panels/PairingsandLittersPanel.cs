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
                Text = "Manage breeding pairs, litters, lines, and projects",
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
                Font = new Font("Segoe UI", 11),
                Padding = new Point(12, 4)
            };

            // Initialize DataGridViews with consistent styling
            pairingsGridView = CreateStyledDataGridView();
            littersGridView = CreateStyledDataGridView();
            linesGridView = CreateStyledDataGridView();
            projectGridView = CreateStyledDataGridView();

            // Add helper method for consistent DataGridView styling
            DataGridView CreateStyledDataGridView()
            {
                return new DataGridView
                {
                    AutoGenerateColumns = false,
                    MultiSelect = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    RowHeadersVisible = false,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    ReadOnly = true,
                    Font = new Font("Segoe UI", 9),
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                    RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                    RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders
                };
            }

            // Initialize tabs with containers and info panels
            pairingsTab = new TabPage("Pairings");
            InitializeTabWithContainer(pairingsTab, "Pairing Management", 
                "• Create and manage breeding pairs\n" +
                "• Track active and historical pairings\n" +
                "• Monitor breeding success rates\n" +
                "• Link pairings to projects and lines",
                InitializePairingDataGridView);

            littersTab = new TabPage("Litters");
            InitializeTabWithContainer(littersTab, "Litter Management",
                "• Record and track litter information\n" +
                "• Monitor litter development\n" +
                "• Track pup counts and survival rates\n" +
                "• Link litters to breeding pairs",
                InitializeLitterDataGridView);

            linesTab = new TabPage("Line Management");
            InitializeTabWithContainer(linesTab, "Line Management",
                "• Manage breeding lines and strains\n" +
                "• Track line characteristics\n" +
                "• Monitor line progression\n" +
                "• Link lines to projects",
                InitializeLineDataGridView);

            projectsTab = new TabPage("Project Management");
            InitializeTabWithContainer(projectsTab, "Project Management",
                "• Organize breeding projects\n" +
                "• Track project goals and progress\n" +
                "• Manage project timelines\n" +
                "• Link projects to lines and pairs",
                InitializeProjectDataGridView);

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
            // Initialize search and filter panel with consistent styling
            Panel searchFilterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(20)
            };

            // Create filter section with improved layout
            Panel filterSection = new Panel
            {
                Dock = DockStyle.Left,
                Width = 600
            };

            Label filterLabel = new Label
            {
                Text = "Filter by:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(0, 8)
            };

            searchBox = new TextBox
            {
                Width = 200,
                Font = new Font("Segoe UI", 10),
                Location = new Point(70, 5)
            };

            filterComboBox = new ComboBox
            {
                Width = 150,
                Font = new Font("Segoe UI", 10),
                Location = new Point(290, 5),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            filterComboBox.Items.AddRange(new object[] { "All", "Current", "Past", "Future", "Species", "Line", "Project" });
            filterComboBox.SelectedIndex = 0;

            searchButton = CreateStyledButton("Search", 460, 2);
            searchButton.Click += SearchButton_Click;

            filterSection.Controls.Add(filterLabel);
            filterSection.Controls.Add(searchBox);
            filterSection.Controls.Add(filterComboBox);
            filterSection.Controls.Add(searchButton);

            searchFilterPanel.Controls.Add(filterSection);

            // Action buttons panel with improved styling
            Panel actionButtonsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(20)
            };

            addButton = CreateStyledButton("Add", 0, 0);
            updateButton = CreateStyledButton("Update", 110, 0);
            deleteButton = CreateStyledButton("Delete", 220, 0);

            Panel buttonContainer = new Panel
            {
                Dock = DockStyle.Left,
                Width = 450
            };

            buttonContainer.Controls.Add(addButton);
            buttonContainer.Controls.Add(updateButton);
            buttonContainer.Controls.Add(deleteButton);

            actionButtonsPanel.Controls.Add(buttonContainer);

            this.Controls.Add(searchFilterPanel);
            this.Controls.Add(actionButtonsPanel);
        }

        private Button CreateStyledButton(string text, int x, int y)
        {
            Button button = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                Location = new Point(x, y),
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

        private void InitializeTabWithContainer(TabPage tabPage, string title, string description, Action initializeMethod)
        {
            Panel containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Add info panel
            Panel infoPanel = CreateInfoPanel(title, description);
            infoPanel.Dock = DockStyle.Top;
            infoPanel.Height = 120;
            infoPanel.Margin = new Padding(0, 0, 0, 10);

            // Create grid container
            Panel gridContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 130, 0, 0)
            };

            containerPanel.Controls.Add(infoPanel);
            containerPanel.Controls.Add(gridContainer);
            tabPage.Controls.Add(containerPanel);

            // Store the grid container so the initialize methods can use it
            tabPage.Tag = gridContainer;

            // Initialize the specific grid view
            initializeMethod();
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
            pairingsGridView.Dock = DockStyle.Fill;

            pairingsGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "PairingId", HeaderText = "Pairing ID", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Doe", HeaderText = "Doe", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "Buck", HeaderText = "Buck", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "Project", HeaderText = "Project", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "PairingDate", HeaderText = "Pairing Date", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "PairingEndDate", HeaderText = "End Date", Width = 120 },
                new DataGridViewButtonColumn { Name = "PairingPage", HeaderText = "Details", Text = "View", UseColumnTextForButtonValue = true, Width = 80 }
            });

            var gridContainer = pairingsTab.Tag as Panel;
            gridContainer.Controls.Add(pairingsGridView);

            PopulatePairingDataDisplayArea();
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
                MessageBox.Show("No pairings found.", "Empty Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InitializeProjectDataGridView()
        {
            projectGridView.Dock = DockStyle.Fill;

            projectGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Project Name", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "LineId", HeaderText = "Line ID", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "Notes", HeaderText = "Notes", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "CreatedOn", HeaderText = "Created", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "LastUpdated", HeaderText = "Updated", Width = 100 },
                new DataGridViewButtonColumn { Name = "ProjectPage", HeaderText = "Details", Text = "View", UseColumnTextForButtonValue = true, Width = 80 }
            });

            var gridContainer = projectsTab.Tag as Panel;
            gridContainer.Controls.Add(projectGridView);

            PopulateProjectDataDisplayArea();
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
                MessageBox.Show("No projects found.", "Empty Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InitializeLitterDataGridView()
        {
            littersGridView.Dock = DockStyle.Fill;

            littersGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "LitterId", HeaderText = "ID", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "LitterName", HeaderText = "Name/Theme", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "Project", HeaderText = "Project", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "Dam", HeaderText = "Dam", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "Sire", HeaderText = "Sire", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "DOB", HeaderText = "DOB", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "NumPups", HeaderText = "Pups", Width = 60 },
                new DataGridViewButtonColumn { Name = "LitterPage", HeaderText = "Details", Text = "View", UseColumnTextForButtonValue = true, Width = 80 }
            });

            var gridContainer = littersTab.Tag as Panel;
            gridContainer.Controls.Add(littersGridView);

            PopulateLittersDataDisplayArea();
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
                MessageBox.Show("No litters found.", "Empty Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void InitializeLineDataGridView()
        {
            linesGridView.Dock = DockStyle.Fill;

            linesGridView.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "LineId", HeaderText = "ID", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Line Name", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "StockId", HeaderText = "Stock ID", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "StockName", HeaderText = "Stock", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "Notes", HeaderText = "Notes", Width = 200 },
                new DataGridViewButtonColumn { Name = "LinePage", HeaderText = "Details", Text = "View", UseColumnTextForButtonValue = true, Width = 80 }
            });

            var gridContainer = linesTab.Tag as Panel;
            gridContainer.Controls.Add(linesGridView);

            PopulateLineDataDisplayArea();
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
                MessageBox.Show("Line service initialization failed.", "Service Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    MessageBox.Show("No breeding lines found.", "Empty Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error populating line data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
