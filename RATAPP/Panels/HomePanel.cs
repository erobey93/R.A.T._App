using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RATAPP.Forms;

namespace RATAPP.Panels
{
    public partial class HomePanel : Panel, INavigable
    {
        private string _username;
        private string _role;
        private ComboBox speciesComboBox;
        private ComboBox sexComboBox;
        private TextBox searchBar;
        private Button searchButton;
        private Button addButton;
        private DataGridView dataDisplayArea;
        private RATAppBaseForm _parentForm;
        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
        private RATAPPLibrary.Services.AnimalService _animalService;
        private RATAPPLibrary.Data.Models.AnimalDto[] _animals;
        private PictureBox loadingSpinner;

        private HomePanel(RATAppBaseForm parentForm, RATAPPLibrary.Data.DbContexts.RatAppDbContext context, string username, string role)
        {
            _parentForm = parentForm;
            _username = username;
            _role = role;
            _context = context;
            _animalService = new RATAPPLibrary.Services.AnimalService(_context);
        }

        public static async Task<HomePanel> CreateAsync(RATAppBaseForm parentForm, RATAPPLibrary.Data.DbContexts.RatAppDbContext context, string username, string role)
        {
            var panel = new HomePanel(parentForm, context, username, role);
            await panel.InitializePanelAsync();
            return panel;
        }

        public async Task InitializePanelAsync()
        {
            await GetAnimalsAsync();
            InitializeComponents();
        }

        private async Task GetAnimalsAsync()
        {
            _animals = await _animalService.GetAllAnimalsAsync();
        }

        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(20, 20, 20, 0)
            };

            var welcomeLabel = new Label
            {
                Text = $"Welcome, {_username}!",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 212),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            headerPanel.Controls.Add(welcomeLabel);

            var roleLabel = new Label
            {
                Text = $"Role: {_role}",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, welcomeLabel.Bottom + 5)
            };
            headerPanel.Controls.Add(roleLabel);

            this.Controls.Add(headerPanel);

            InitializeFilterControls();
            InitializeDataDisplayArea();
            InitializeLoadingSpinner();
        }

        private void InitializeFilterControls()
        {
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(20, 10, 20, 10)
            };

            speciesComboBox = CreateComboBox(new string[] { "All Species", "Rats", "Mice" }, 0);
            sexComboBox = CreateComboBox(new string[] { "All Sexes", "Male", "Female" }, 190);

            searchBar = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Width = 300,
                Location = new Point(380, 5),
                PlaceholderText = "Search by Animal Name, ID..."
            };

            searchButton = CreateButton("Search", 690, SearchButton_Click);
            addButton = CreateButton("Add New Animal", 800, addButton_Click);

            filterPanel.Controls.AddRange(new Control[] { speciesComboBox, sexComboBox, searchBar, searchButton, addButton });
            this.Controls.Add(filterPanel);
        }

        private ComboBox CreateComboBox(string[] items, int x)
        {
            var comboBox = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Width = 180,
                Location = new Point(x, 15),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboBox.Items.AddRange(items);
            comboBox.SelectedIndex = 0;
            return comboBox;
        }

        private Button CreateButton(string text, int x, EventHandler clickHandler)
        {
            var button = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                Width = 100,
                Location = new Point(x, 13),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += clickHandler;
            return button;
        }

        //method to initialize the data display area which holds all the animal data from db
        //and allows for filtering and searching
        //TODO having some issues witht the data display area, need to fix this but currently it is showing up so leaving for now 
        private void InitializeDataDisplayArea()
        {
            int topPanelHeight = 180;

            dataDisplayArea = new DataGridView
            {
                Location = new Point(0, topPanelHeight),
                Width = 1000,
                Height = 400,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders
            };
            //var dataPanel = new Panel
            //{
            //    Dock = DockStyle.Fill,
            //    Padding = new Padding(20, 10, 20, 20)
            //};
            //int roleLabelHeight = 150;

            //dataDisplayArea = new DataGridView
            //{
            //    //Dock = DockStyle.Fill,
            //    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            //    BackgroundColor = Color.White,
            //    BorderStyle = BorderStyle.None,
            //    RowHeadersVisible = false,
            //    AllowUserToAddRows = false,
            //    Font = new Font("Segoe UI", 10),
            //    ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            //    RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
            //    RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders,
            //    AllowUserToResizeColumns = false,
            //    AllowUserToResizeRows = false,
            //    ReadOnly = true,


            //    Location = new Point(roleLabelHeight, 0 ),
            //                Width = 1000,
            //                Height = 400,
            //                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            //                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            //                RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
            //                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders
       // };

            dataDisplayArea.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Species", HeaderText = "Species" },
                new DataGridViewTextBoxColumn { Name = "AnimalID", HeaderText = "Animal ID" },
                new DataGridViewTextBoxColumn { Name = "AnimalName", HeaderText = "Animal Name" },
                new DataGridViewTextBoxColumn { Name = "Sex", HeaderText = "Sex" },
                new DataGridViewTextBoxColumn { Name = "DOB", HeaderText = "DOB" },
                new DataGridViewTextBoxColumn { Name = "Variety", HeaderText = "Variety" },
                new DataGridViewButtonColumn { Name = "Details", HeaderText = "Details", Text = "View", UseColumnTextForButtonValue = true }
            });

            dataDisplayArea.CellContentClick += DataGridView_CellContentClick;
            PopulateDataDisplayArea();

            this.Controls.Add(dataDisplayArea);
        }

        private void PopulateDataDisplayArea()
        {
            dataDisplayArea.Rows.Clear();
            foreach (var animal in _animals)
            {
                dataDisplayArea.Rows.Add(animal.Species, animal.Id, animal.Name, animal.Sex, animal.DateOfBirth, animal.Variety);
            }
        }

        private void InitializeLoadingSpinner()
        {
            loadingSpinner = new PictureBox
            {
                Size = new Size(50, 50),
                Image = Image.FromFile("C:\\Users\\earob\\source\\repos\\RATAPP\\RATAPPLibrary\\RATAPP\\Resources\\Loading_2.gif"),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false
            };
            this.Controls.Add(loadingSpinner);
            this.Resize += (s, e) => CenterLoadingSpinner();
        }

        private void CenterLoadingSpinner()
        {
            loadingSpinner.Location = new Point(
                (ClientSize.Width - loadingSpinner.Width) / 2,
                (ClientSize.Height - loadingSpinner.Height) / 2
            );
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            string speciesFilter = speciesComboBox.SelectedItem.ToString();
            string sexFilter = sexComboBox.SelectedItem.ToString();
            string searchTerm = searchBar.Text.ToLower();

            var filteredAnimals = _animals.Where(animal =>
                (speciesFilter == "All Species" || animal.Species == speciesFilter) &&
                (sexFilter == "All Sexes" || animal.Sex == sexFilter) &&
                (animal.Name.ToLower().Contains(searchTerm) || animal.Id.ToString().Contains(searchTerm))
            );

            dataDisplayArea.Rows.Clear();
            foreach (var animal in filteredAnimals)
            {
                dataDisplayArea.Rows.Add(animal.Species, animal.Id, animal.Name, animal.Sex, animal.DateOfBirth, animal.Variety);
            }
        }

        private void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataDisplayArea.Columns["Details"].Index && e.RowIndex >= 0)
            {
                int animalId = Convert.ToInt32(dataDisplayArea.Rows[e.RowIndex].Cells["AnimalID"].Value);
                var animal = _animals.FirstOrDefault(a => a.Id == animalId);
                if (animal != null)
                {
                    var animalDetailsPanel = new AnimalPanel(_parentForm, _context, _animals, animal);
                    _parentForm.ShowPanel(animalDetailsPanel);
                }
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            var animalPanel = new AnimalPanel(_parentForm, _context, _animals, null);
            _parentForm.ShowPanel(animalPanel);
        }

        public async Task RefreshDataAsync()
        {
            try
            {
                loadingSpinner.Visible = true;
                CenterLoadingSpinner();
                this.Refresh();

                _animals = await _animalService.GetAllAnimalsAsync();
                await Task.Delay(1000); // Simulated delay, remove in production

                PopulateDataDisplayArea();

                loadingSpinner.Visible = false;
                this.Refresh();

                MessageBox.Show("Data refresh complete", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                loadingSpinner.Visible = false;
                this.Refresh();
            }
        }
    }
}

//using Microsoft.Azure.Amqp.Framing;
//using RATAPP.Forms;
//using RATAPP.Properties;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

//namespace RATAPP.Panels
//{
//    public partial class HomePanel : Panel, INavigable
//    {
//        private string _username;
//        private string _role;

//        private ComboBox speciesComboBox;
//        private ComboBox sexComboBox;
//        private TextBox searchBar;
//        private Button searchButton;
//        private Button addButton;
//        private DataGridView dataDisplayArea;

//        private RATAppBaseForm _parentForm;  // Reference to parent form (RATAppBaseForm) this tightly couples things, but it is the easiest way to use panels
//        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;

//        private RATAPPLibrary.Services.AnimalService _animalService;
//        private RATAPPLibrary.Data.Models.AnimalDto[] _animals; //list or array, does it matter? if so, why? TODO

//        private PictureBox loadingSpinner; //TODO put in some kind of utility class for re-use, just testing right now 


//        private HomePanel(RATAppBaseForm parentForm, RATAPPLibrary.Data.DbContexts.RatAppDbContext context, string username, string role)
//        {
//            _parentForm = parentForm;
//            _username = username;
//            _role = role;
//            _context = context;
//            _animalService = new RATAPPLibrary.Services.AnimalService(_context);
//        }

//        public static async Task<HomePanel> CreateAsync(RATAppBaseForm parentForm, RATAPPLibrary.Data.DbContexts.RatAppDbContext context, string username, string role)
//        {
//            var panel = new HomePanel(parentForm, context, username, role);
//            await panel.InitializePanelAsync();
//            return panel;
//        }

//        public async Task InitializePanelAsync()
//        {
//            // Fetch animals asynchronously
//            await GetAnimalsAsync();

//            // Set the panel size to match the container (RATAppBaseForm)
//            Dock = DockStyle.Fill;
//            BackColor = Color.LightBlue;

//            // Set the title of the panel
//            var usernameLabel = new Label
//            {
//                Text = $"Welcome, {_username}!",
//                Font = new Font("Arial", 14, FontStyle.Bold),
//                AutoSize = true,
//                Location = new Point(20, 20)
//            };
//            Controls.Add(usernameLabel);

//            // Add a label to display role
//            var roleLabel = new Label
//            {
//                Text = $"Your role: {_role}",
//                Font = new Font("Arial", 12, FontStyle.Regular),
//                AutoSize = true,
//                Location = new Point(20, 60)
//            };
//            Controls.Add(roleLabel);

//            // Initialize other components in the panel
//            InitializeSpeciesToggleButton();
//            InitializeSexToggleButton();
//            InitializeSearchBar();
//            InitializeSearchButton();
//            InitializeAddButton();
//            InitializeDataDisplayArea();
//            LoadSpinner(); 
//        }

//        private async Task GetAnimalsAsync()
//        {
//            // Fetch animals asynchronously
//            _animals = await _animalService.GetAllAnimalsAsync();
//        }

//        private void InitializeSpeciesToggleButton()
//        {
//            speciesComboBox = new ComboBox
//            {
//                Font = new Font("Arial", 12F, FontStyle.Bold),
//                ForeColor = Color.White,
//                BackColor = Color.Navy,
//                DropDownStyle = ComboBoxStyle.DropDownList,
//                Width = 200,
//                Height = 40,
//                Location = new Point(20, 100)
//            };

//            speciesComboBox.Items.Add("All Species");
//            speciesComboBox.Items.Add("Rats");
//            speciesComboBox.Items.Add("Mice");
//            speciesComboBox.SelectedIndex = 0;

//            Controls.Add(speciesComboBox);
//        }

//        private void InitializeSexToggleButton()
//        {
//            sexComboBox = new ComboBox
//            {
//                Font = new Font("Arial", 12F, FontStyle.Bold),
//                ForeColor = Color.White,
//                BackColor = Color.Navy,
//                DropDownStyle = ComboBoxStyle.DropDownList,
//                Width = 150,
//                Height = 40,
//                Location = new Point(240, 100)
//            };

//            sexComboBox.Items.Add("All Sexes");
//            sexComboBox.Items.Add("Male");
//            sexComboBox.Items.Add("Female");
//            sexComboBox.SelectedIndex = 0;

//            Controls.Add(sexComboBox);
//        }

//        private void InitializeSearchBar()
//        {
//            searchBar = new TextBox
//            {
//                Font = new Font("Arial", 12F),
//                Width = 400,
//                Height = 30,
//                Location = new Point(420, 100),
//                PlaceholderText = "Search by Animal Name, ID, etc..."
//            };

//            Controls.Add(searchBar);
//        }

//        private void InitializeSearchButton()
//        {
//            searchButton = new Button
//            {
//                Text = "Search",
//                Font = new Font("Arial", 12F),
//                Width = 100,
//                Height = 30,
//                Location = new Point(830, 100)
//            };

//            searchButton.Click += SearchButton_Click;

//            Controls.Add(searchButton);
//        }

//        private void InitializeAddButton()
//        {
//            addButton = new Button
//            {
//                Text = "Add New Animal",
//                Font = new Font("Arial", 12F),
//                Width = 100,
//                Height = 30,
//                Location = new Point(1030, 100)
//            };

//            addButton.Click += addButton_Click;

//            Controls.Add(addButton);
//        }

//        private void InitializeDataDisplayArea()
//        {
//            int topPanelHeight = 150;

//            dataDisplayArea = new DataGridView
//            {
//                Location = new Point(0, topPanelHeight),
//                Width = 1000,
//                Height = 400,
//                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
//                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
//                RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
//                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders
//            };

//            // Add columns
//            dataDisplayArea.Columns.Add("Species", "Species");
//            dataDisplayArea.Columns.Add("AnimalID", "Animal ID");
//            dataDisplayArea.Columns.Add("AnimalName", "Animal Name");
//            dataDisplayArea.Columns.Add("Sex", "Sex");
//            dataDisplayArea.Columns.Add("DOB", "DOB");
//            dataDisplayArea.Columns.Add("Variety", "Variety");

//            // Add rows for all animals in database 
//            //get data from db 
//            foreach (var animal in _animals)
//            {
//                dataDisplayArea.Rows.Add(animal.Species, animal.Id, animal.Name, animal.Sex, animal.DateOfBirth, animal.Variety);
//            }

//            Controls.Add(dataDisplayArea);

//            // Add a button column for individual animal pages
//            DataGridViewButtonColumn buttonColumn = new DataGridViewButtonColumn
//            {
//                Name = "Individual Animal Page",  // Give it a name for easy reference
//                HeaderText = "Individual Animal Page",
//                Text = "Go to Page",
//                UseColumnTextForButtonValue = true
//            };
//            dataDisplayArea.Columns.Add(buttonColumn);
//            // Add an event handler for button clicks in the DataGridView
//            dataDisplayArea.CellContentClick += DataGridView_CellContentClick;
//        }

//        private void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
//        {
//            if (e.ColumnIndex == dataDisplayArea.Columns["Individual Animal Page"].Index && e.RowIndex >= 0 && dataDisplayArea.Rows[e.RowIndex].Cells["AnimalID"].Value != null)
//            {
//                string animalName = dataDisplayArea.Rows[e.RowIndex].Cells["AnimalName"].Value.ToString();
//                string animalID = dataDisplayArea.Rows[e.RowIndex].Cells["AnimalID"].Value.ToString(); //TODO fix this logic with string vs int 
//                string species = dataDisplayArea.Rows[e.RowIndex].Cells["Species"].Value.ToString();
//                string sex = dataDisplayArea.Rows[e.RowIndex].Cells["Sex"].Value.ToString();
//                string dob = dataDisplayArea.Rows[e.RowIndex].Cells["DOB"].Value.ToString();
//                //string genotype = dataDisplayArea.Rows[e.RowIndex].Cells["Genotype"].Value.ToString();

//                // Create the AnimalDetailsPanel and show it using the parent form's ShowPanel method
//                //interesting problem/question here: where should the getting be done for the single animal? Here? or in the Animal panel?
//                int animalId = int.Parse(animalID);
//                //get the animal from the list of animals
//                var animal = _animals.FirstOrDefault(a => a.Id == animalId);
//                var animalDetailsPanel = new AnimalPanel(_parentForm, _context, _animals, animal);//new AnimalPanel(animalName, animalID, species, sex, dob, genotype); TODO need to actually pass in the details for the associated animal 
//                _parentForm.ShowPanel(animalDetailsPanel);   // Use the ShowPanel method from the parent form
//            }
//        }

//        private void addButton_Click(object sender, EventArgs e)
//        {
//            //go to animal panel, but pass in an empty animal id
//            var animalPanel = new AnimalPanel(_parentForm, _context, _animals, null); // Pass in an empty animal ID for a new animal
//            _parentForm.ShowPanel(animalPanel);
//        }
//        private void SearchButton_Click(object sender, EventArgs e)
//        {
//            // Get selected filter values
//            string speciesFilter = speciesComboBox.SelectedItem.ToString();
//            string sexFilter = sexComboBox.SelectedItem.ToString();
//            string searchTerm = searchBar.Text.ToLower();

//            // Filter rows first by Species
//            var speciesFilteredRows = new List<DataGridViewRow>();
//            foreach (DataGridViewRow row in dataDisplayArea.Rows)
//            {
//                bool isVisible = true;

//                // Filter by species
//                if (speciesFilter != "All Species" && row.Cells["Species"].Value.ToString() != speciesFilter)
//                {
//                    isVisible = false;
//                }

//                // Only add to the filtered list if it matches the species filter
//                if (isVisible)
//                {
//                    speciesFilteredRows.Add(row);
//                }
//            }

//            // Now apply further filtering and sorting by Sex or Group
//            var sexFilteredRows = new List<DataGridViewRow>();
//            foreach (var row in speciesFilteredRows)
//            {
//                bool isSexVisible = true;

//                // Filter by sex/group
//                if (sexFilter != "All Sexes" && row.Cells["Sex"].Value.ToString() != sexFilter)
//                {
//                    isSexVisible = false;
//                }

//                // Apply the search filter
//                if (!row.Cells["AnimalName"].Value.ToString().ToLower().Contains(searchTerm) &&
//                    !row.Cells["AnimalID"].Value.ToString().ToLower().Contains(searchTerm))
//                {
//                    isSexVisible = false;
//                }

//                // If the row passes all filters, add it to the visible list
//                if (isSexVisible)
//                {
//                    sexFilteredRows.Add(row);
//                }
//            }

//            // Finally, update DataGridView rows based on filtering
//            dataDisplayArea.Rows.Clear();  // Clear existing rows
//            foreach (var row in sexFilteredRows)
//            {
//                dataDisplayArea.Rows.Add(row);
//            }
//        }

//        //TODO just testing, move to utils file
//        private void LoadSpinner()
//        {
//            // Create and configure the spinner
//            loadingSpinner = new PictureBox
//            {
//                Size = new Size(50, 50), // Adjust size as needed "C:\Users\earob\source\repos\RATAPP\RATAPPLibrary\RATAPP\Resources\Loading_2.gif"
//                Image = Image.FromFile("C:\\Users\\earob\\source\\repos\\RATAPP\\RATAPPLibrary\\RATAPP\\Resources\\Loading_2.gif"), // Add a GIF to your project resources
//                SizeMode = PictureBoxSizeMode.StretchImage,
//                Visible = false // Initially hidden
//            };

//            // Position the spinner in the center of the form
//            loadingSpinner.Location = new Point(
//                (ClientSize.Width - loadingSpinner.Width) / 2,
//                (ClientSize.Height - loadingSpinner.Height) / 2
//            );

//            // Add the spinner to the form
//            Controls.Add(loadingSpinner);

//            // Handle form resize to reposition the spinner
//            Resize += (s, e) =>
//            {
//                loadingSpinner.Location = new Point(
//                    (ClientSize.Width - loadingSpinner.Width) / 2,
//                    (ClientSize.Height - loadingSpinner.Height) / 2
//                );
//            };
//        }
//        //FIXME repeated code but getting interface working for now 
//        public async Task RefreshDataAsync()
//        {
//            try
//            {
//                // Show spinner
//                loadingSpinner.Visible = true;
//                Refresh(); // Force UI to repaint to show spinner

//                // Fetch animals asynchronously
//                _animals = await _animalService.GetAllAnimalsAsync();

//                //for testing
//                // Wait asynchronously for 3 seconds
//                await Task.Delay(3000); // 3000 milliseconds = 3 seconds

//                // Hide spinner
//                loadingSpinner.Visible = false;
//                Refresh(); // Force UI to repaint to not show spinner

//                MessageBox.Show("Data refresh complete", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//            finally
//            {
//                //this is an emergency catch really TODO fix this logic, maybe no finally? 
//                // Hide spinner
//                loadingSpinner.Visible = false;
//                Refresh(); // Force UI to repaint to not show spinner
//            }
//        }
//    }
//}
