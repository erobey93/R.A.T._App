using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RATAPP.Forms;
using static System.Windows.Forms.AxHost;

namespace RATAPP.Panels
{
    public partial class HomePanel : Panel, INavigable
    {
        private string _username;
        private ComboBox speciesComboBox;
        private ComboBox sexComboBox;
        private TextBox searchBar;
        private Button searchButton;
        private Button addButton;
        private Button bulkAddButton; //TODO 
        private Panel remindersPanel;
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
                Height = 60,
                Padding = new Padding(20, 10, 20, 0)
            };

            var welcomeLabel = new Label
            {
                Text = $"{_username}",
                Font = new Font("Segoe UI", 25, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 212),
                AutoSize = true,
                Location = new Point(0, 5)
            };
            headerPanel.Controls.Add(welcomeLabel);

            this.Controls.Add(headerPanel);

            InitializeFilterControls();
            InitializeDataDisplayArea();
            InitializeRemindersPanel();
            InitializeLoadingSpinner();
        }

        private void InitializeFilterControls()
        {
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(20, 10, 20, 10),
                //Location = new Point(0, startY) // Set the Location based on startY
            };

            speciesComboBox = CreateComboBox(new string[] { "All Species", "Rat", "Mouse" }, 0); //TODO get from db 
            sexComboBox = CreateComboBox(new string[] { "All Sexes", "Male", "Female", "Unknown" }, 190); //TODO get from db 

            searchBar = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Width = 300,
                Location = new Point(380, 15),
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
                Height = 30,
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
                dataDisplayArea.Rows.Add(animal.species, animal.Id, animal.name, animal.sex, animal.DateOfBirth, animal.variety);
            }
        }

        //set up reminder panel to the right of the main data grid view panel on the home page 
        private void InitializeRemindersPanel()
        {
            int topPanelHeight = 180;

            remindersPanel = new Panel
            {
                Location = new Point(1020, topPanelHeight),
                Width = 240,
                Height = 400,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            // Reminders header
            var remindersHeader = new Label
            {
                Text = "Reminders",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 212),
                Location = new Point(10, 10),
                Size = new Size(220, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            remindersPanel.Controls.Add(remindersHeader);

            // Today's reminders section
            var todayLabel = new Label
            {
                Text = "Today",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 50),
                Size = new Size(220, 20)
            };
            remindersPanel.Controls.Add(todayLabel);

            // Sample reminders for today
            CreateReminderItem("Pup Pickup 6-7pm", 80, Color.FromArgb(255, 100, 100));
            CreateReminderItem("Respond to Adoption Applications", 110, Color.FromArgb(255, 100, 100));

            // Next week reminders section
            var nextWeekLabel = new Label
            {
                Text = "Next Week",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(10, 150),
                Size = new Size(220, 20)
            };
            remindersPanel.Controls.Add(nextWeekLabel);

            // Sample reminders for next week
            //TODO - I want some kind of calendar functionality this is just a mock up of the actual functionality 
            CreateReminderItem("Antibiotics Finished for Groups 1-3 (Monday)", 180, Color.FromArgb(100, 150, 255));
            CreateReminderItem("Vet appointment (Wednesday)", 210, Color.FromArgb(100, 150, 255));
            CreateReminderItem("Pickup Wilco Supplies (Friday)", 240, Color.FromArgb(100, 150, 255));

            // Refresh button
            var refreshButton = new Button
            {
                Text = "Refresh Reminders",
                Font = new Font("Segoe UI", 9),
                Location = new Point(10, 350),
                Size = new Size(220, 30),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Click += RefreshReminders_Click;
            remindersPanel.Controls.Add(refreshButton);

            // Refresh button
            var openCalendarButton = new Button
            {
                Text = "Open Calendar",
                Font = new Font("Segoe UI", 9),
                Location = new Point(10, 310),
                Size = new Size(220, 30),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            openCalendarButton.FlatAppearance.BorderSize = 0;
            openCalendarButton.Click += OpenCalendar_Click;
            remindersPanel.Controls.Add(openCalendarButton);

            this.Controls.Add(remindersPanel);
        }

        private void CreateReminderItem(string text, int y, Color color)
        {
            var panel = new Panel
            {
                Location = new Point(15, y),
                Size = new Size(210, 25),
                BackColor = color
            };

            var label = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                Location = new Point(5, 5),
                Size = new Size(200, 15),
                AutoEllipsis = true
            };

            panel.Controls.Add(label);
            remindersPanel.Controls.Add(panel);
        }

        private void RefreshReminders_Click(object sender, EventArgs e)
        {
            // TODO: Implement backend logic to fetch actual reminders
            MessageBox.Show("Refreshing reminders...\n\nThis would fetch the latest reminders from the database.",
                "Reminders", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OpenCalendar_Click(object sender, EventArgs e)
        {
            //TODO: do actual implementation of calendar, but this is a workable example for now 
            var calendarForm = new CalendarForm(_context);
            calendarForm.ShowDialog();

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
                (speciesFilter == "All Species" || animal.species == speciesFilter) &&
                (sexFilter == "All Sexes" || animal.sex == sexFilter) &&
                (animal.name.ToLower().Contains(searchTerm) || animal.Id.ToString().Contains(searchTerm))
            );

            dataDisplayArea.Rows.Clear();
            foreach (var animal in filteredAnimals)
            {
                dataDisplayArea.Rows.Add(animal.species, animal.Id, animal.name, animal.sex, animal.DateOfBirth, animal.variety);
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
