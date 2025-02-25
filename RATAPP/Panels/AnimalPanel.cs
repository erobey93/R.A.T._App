
//using System;
//using System.Drawing;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using RATAPP.Forms;
//using RATAPPLibrary.Data.Models;
//using RATAPPLibrary.Services;

//namespace RATAPP.Panels
//{
//    public partial class AnimalPanel : Panel, INavigable
//    {
//        private RATAppBaseForm _parentForm;
//        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
//        private AnimalService _animalService;
//        private AnimalDto _animal;
//        private AnimalDto[] _allAnimals;
//        private bool _isEditMode = false;

//        private TableLayoutPanel mainLayout;
//        private FlowLayoutPanel buttonPanel;
//        private PictureBox animalPhotoBox;
//        private PictureBox loadingSpinner;

//        private Button saveButton;
//        private Button updateButton;
//        private Button calculateInbredButton;

//        private Dictionary<string, Control> formControls = new Dictionary<string, Control>();

//        public AnimalPanel(RATAppBaseForm parentForm, RATAPPLibrary.Data.DbContexts.RatAppDbContext context, AnimalDto[] allAnimals, AnimalDto currAnimal)
//        {
//            _parentForm = parentForm;
//            _context = context;
//            _animalService = new AnimalService(context);
//            _allAnimals = allAnimals;
//            _animal = currAnimal;

//            parentForm.SetActivePanel(this);
//            LoadSpinner();
//            InitializeComponent();
//        }

//        private void InitializeComponent()
//        {
//            this.SuspendLayout();

//            this.Dock = DockStyle.Fill;
//            this.BackColor = Color.White;
//            this.Padding = new Padding(20);

//            InitializeMainLayout();
//            InitializeButtonPanel();
//            InitializePhotoBox();
//            InitializeFormFields();
//            InitializeNavigationButtons();

//            if (_animal != null && !_isEditMode)
//            {
//                LoadAnimalDataAsync(_animal.Id);
//            }
//            else
//            {
//                SetEditMode(true);
//            }

//            this.ResumeLayout(false);
//        }

//        private void InitializeMainLayout()
//        {
//            mainLayout = new TableLayoutPanel
//            {
//                Dock = DockStyle.Fill,
//                ColumnCount = 3,
//                RowCount = 2
//            };

//            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
//            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
//            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
//            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
//            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));

//            this.Controls.Add(mainLayout);
//        }

//        private void InitializeButtonPanel()
//        {
//            buttonPanel = new FlowLayoutPanel
//            {
//                Dock = DockStyle.Fill,
//                FlowDirection = FlowDirection.LeftToRight,
//                WrapContents = false,
//                AutoSize = true
//            };

//            string[] buttonTexts = { "Ancestry", "Genetics", "Breeding History", "Documents", "Health" };
//            foreach (var text in buttonTexts)
//            {
//                var button = CreateButton(text, ButtonClick);
//                buttonPanel.Controls.Add(button);
//            }

//            mainLayout.SetColumnSpan(buttonPanel, 3);
//            mainLayout.Controls.Add(buttonPanel, 0, 1);
//        }

//        private void InitializePhotoBox()
//        {
//            animalPhotoBox = new PictureBox
//            {
//                Dock = DockStyle.Fill,
//                SizeMode = PictureBoxSizeMode.Zoom,
//                BorderStyle = BorderStyle.FixedSingle
//            };

//            mainLayout.Controls.Add(animalPhotoBox, 2, 0);
//        }

//        private void InitializeFormFields()
//        {
//            var leftPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8 };
//            var rightPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8 };

//            string[] leftFields = { "Registration #/ID", "Animal Name", "Species", "Sex", "Variety", "Dam", "Sire", "% Inbred" };
//            string[] rightFields = { "Color", "Genetics", "Ancestry", "Breeder", "Ear Type", "DOB", "DOD", "Comments" };

//            InitializePanel(leftPanel, leftFields, 0);
//            InitializePanel(rightPanel, rightFields, 1);

//            mainLayout.Controls.Add(leftPanel, 0, 0);
//            mainLayout.Controls.Add(rightPanel, 1, 0);
//        }

//        private void InitializePanel(TableLayoutPanel panel, string[] fields, int column)
//        {
//            for (int i = 0; i < fields.Length; i++)
//            {
//                var label = new Label { Text = fields[i], Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight };
//                panel.Controls.Add(label, 0, i);

//                Control field;
//                if (fields[i] == "Comments")
//                {
//                    field = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
//                }
//                else if (new[] { "Species", "Sex", "Variety", "Color", "Ancestry", "Ear Type" }.Contains(fields[i]))
//                {
//                    field = new ComboBox { Dock = DockStyle.Fill };
//                }
//                else
//                {
//                    field = new TextBox { Dock = DockStyle.Fill };
//                }

//                panel.Controls.Add(field, 1, i);
//                formControls[fields[i]] = field;
//            }

//            calculateInbredButton = CreateButton("Calculate % Inbred", CalculateInbred);
//            panel.Controls.Add(calculateInbredButton, 1, 7);
//            calculateInbredButton.Dock = DockStyle.Fill;
//        }

//        private void InitializeNavigationButtons()
//        {
//            var prevButton = CreateButton("Previous", PreviousButtonClick);
//            var nextButton = CreateButton("Next", NextButtonClick);

//            buttonPanel.Controls.Add(prevButton);
//            buttonPanel.Controls.Add(nextButton);
//        }

//        private Button CreateButton(string text, EventHandler clickHandler)
//        {
//            var button = new Button
//            {
//                Text = text,
//                AutoSize = true,
//                Font = new Font("Segoe UI", 10F),
//                BackColor = Color.FromArgb(0, 120, 212),
//                ForeColor = Color.White,
//                FlatStyle = FlatStyle.Flat,
//                Margin = new Padding(5)
//            };
//            button.FlatAppearance.BorderSize = 0;
//            button.Click += clickHandler;
//            return button;
//        }

//        //TODO: Implement loading spinner utility class for re-use
//        //Load the data loading user feedback spinner
//        private void LoadSpinner()
//        {
//            loadingSpinner = new PictureBox
//            {
//                Size = new Size(50, 50),
//                Image = Image.FromFile("C:\\Users\\earob\\source\\repos\\RATAPP\\RATAPPLibrary\\RATAPP\\Resources\\Loading_2.gif"),
//                SizeMode = PictureBoxSizeMode.StretchImage,
//                Visible = false
//            };
//            this.Controls.Add(loadingSpinner);
//            this.Resize += (s, e) => CenterLoadingSpinner();
//        }

//        private void CenterLoadingSpinner()
//        {
//            if (loadingSpinner != null)
//            {
//                loadingSpinner.Location = new Point(
//                    (ClientSize.Width - loadingSpinner.Width) / 2,
//                    (ClientSize.Height - loadingSpinner.Height) / 2
//                );
//            }
//        }

//        private async void LoadAnimalDataAsync(int animalId)
//        {
//            try
//            {
//                loadingSpinner.Visible = true;
//                CenterLoadingSpinner();
//                this.Refresh();

//                _animal = await _animalService.GetAnimalByIdAsync(animalId);
//                PopulateFormFields(_animal);

//                loadingSpinner.Visible = false;
//                this.Refresh();
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//            finally
//            {
//                loadingSpinner.Visible = false;
//                this.Refresh();
//            }
//        }

//        private void PopulateFormFields(AnimalDto animal)
//        {
//            if (animal != null)
//            {
//                formControls["Registration #/ID"].Text = animal.Id.ToString();
//                formControls["Animal Name"].Text = animal.Name;
//                formControls["Species"].Text = animal.Species;
//                formControls["Sex"].Text = animal.Sex;
//                formControls["Variety"].Text = animal.Variety;
//                formControls["Color"].Text = animal.Color;
//                formControls["Breeder"].Text = animal.Breeder;
//                formControls["Dam"].Text = animal.Dam;
//                formControls["Sire"].Text = animal.Sire;
//                formControls["DOB"].Text = animal.DateOfBirth.ToString("yyyy-MM-dd");
//                formControls["DOD"].Text = animal.DateOfDeath?.ToString("yyyy-MM-dd") ?? "";
//                // Populate other fields...

//                if (!string.IsNullOrEmpty(animal.imageUrl))
//                {
//                    animalPhotoBox.Image = Image.FromFile(animal.imageUrl);
//                }
//            }
//            SetEditMode(false);
//        }

//        private void SetEditMode(bool isEditMode)
//        {
//            _isEditMode = isEditMode;
//            foreach (var control in formControls.Values)
//            {
//                control.Enabled = isEditMode;
//            }
//            saveButton.Visible = isEditMode;
//            updateButton.Visible = !isEditMode;
//        }

//        private void InbredButton()
//        {
//            //create calc % inbred button 
//            calculateInbredButton = new Button
//            {
//                Location = new Point(400, 450),
//                Width = 200,
//                Height = 30,
//                Text = "Calculate % Inbred",
//                Font = new Font("Segoe UI", 10F),
//                BackColor = Color.LightGray,
//                FlatStyle = FlatStyle.Flat
//            };
//            //this should be a call to the library to calculate the % inbred
//            calculateInbredButton.Click += (sender, e) =>
//            {
//                // Logic to calculate % inbred
//                //TODO get the values from the database just for testing right now FIXME
//                string TODO = "TODO - should come from db"; //FIXME
//                calculateInbredTextBox.Text = TODO;
//            };
//        }

//        private void SaveButton()
//        {
//            //create save button
//            saveButton = new Button
//            {
//                Location = new Point(10, 630),
//                Width = 150,
//                Height = 40,
//                Text = "Save Changes",
//                Font = new Font("Segoe UI", 10F),
//                BackColor = Color.Green,
//                FlatStyle = FlatStyle.Popup
//            };
//            saveButton.Click += async (sender, e) => //NOTE: putting this inside of an async lambda is what allows us to use await without setting the entire method to async! 
//            {
//                await SaveButtonClick(sender, e);
//            };
//        }

//        private void ButtonClick(object sender, EventArgs e)
//        {
//            // Handle button clicks
//        }

//        private void CalculateInbred(object sender, EventArgs e)
//        {
//            // Implement inbreeding calculation logic
//        }

//        private void PreviousButtonClick(object sender, EventArgs e)
//        {
//            NavigateAnimals(-1);
//        }

//        private void NextButtonClick(object sender, EventArgs e)
//        {
//            NavigateAnimals(1);
//        }

//        private void NavigateAnimals(int direction)
//        {
//            if (_allAnimals == null || _allAnimals.Length == 0) return;

//            int currentIndex = Array.FindIndex(_allAnimals, a => a.Id == _animal.Id);
//            int newIndex = (currentIndex + direction + _allAnimals.Length) % _allAnimals.Length;
//            LoadAnimalDataAsync(_allAnimals[newIndex].Id);
//        }

//        public async Task RefreshDataAsync()
//        {
//            if (_animal != null)
//            {
//                await _animalService.GetAnimalByIdAsync(_animal.Id);
//            }
//        }

//        // Implement SaveButtonClick, UpdateButtonClick, and other necessary methods...
//    }
//}

using RATAPP.Forms;
using RATAPPLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

//NOTES
// * I think passing a state around is likely the best way to handle whether we're in edit mode, or not
namespace RATAPP.Panels
{
    public partial class AnimalPanel : Panel, INavigable
    {
        // Class-level fields for controls specific to Animal Page
        private Label animalNameLabel;
        private Label idLabel;
        private Label speciesLabel;
        private Label sexLabel;
        private Label varietyLabel;
        private Label colorLabel;
        private Label genotypeLabel;
        private Label ancestryLabel;
        private Label breederInfoLabel;
        private Label commentsLabel;
        private Label damLabel;
        private Label sireLabel;
        private Label inbredLabel;
        private Label earTypeLabel;
        private Label markingsLabel;

        private TextBox animalNameTextBox;
        private TextBox idTextBox;
        private TextBox speciesTextBox;
        private TextBox sexTextBox;
        private TextBox varietyTextBox;
        private TextBox colorTextBox;
        private TextBox genotypeTextBox;
        private TextBox ancestryTextBox;
        private TextBox breederInfoTextBox;
        private TextBox commentsTextBox;
        private TextBox damTextBox;
        private TextBox sireTextBox;
        private TextBox inbredTextBox;
        private TextBox earTypeTextBox;
        private TextBox markingsTextBox; //TODO
        private TextBox dobTextBox; //TODO Date of Birth this needs to be in a specific format so I need to figure that out some checks needed for that, for now just a text box
        private TextBox dodTextBox; // Date of Death

        private ComboBox speciesComboBox;
        private ComboBox sexComboBox;
        private ComboBox varietyComboBox;
        private ComboBox colorComboBox;
        private ComboBox genotypeComboBox;
        private ComboBox ancestryComboBox;
        private ComboBox damComboBox;
        private ComboBox sireComboBox;
        private ComboBox earTypeComboBox;
        private ComboBox markingsComboBox;
        private ComboBox breederInfoComboBox;


        private PictureBox animalPhotoBox;
        private Button inbredButton;
        private Button saveButton;
        private Button updateButton;

        //state of the panel
        private bool _isEditMode = false;

        //db context & services 
        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
        private RATAPPLibrary.Services.AnimalService _animalService;
        private AnimalDto _animal; //FIXME this is not correct, but I'm just getting refresh sort of functioning
        //this is likely temporary but I want to test out the idea of going to the next and previous animal
        private AnimalDto[] _allAnimals; //FIXME this should be cached data instead of passing around a potentially huge array of animals but still WIP

        private PictureBox loadingSpinner; //TODO put in some kind of utility class for re-use, just testing right now 

        //TODO
        //think through data caching of animals, right now I am passing the entire list of animals and having to navigate through an array
        //this is not efficient and I am making data base calls for the current animal instead of using the data that I already have so this is a big FIXME 
        // it may actually make sense to have a state for the panel - future work
        // edit vs non edit mode and then just check this state
        //like if a user is on an existing animal and they click update animal details

        // and then requery the database to get the updated values
        // and then disable the text boxes and enable the update button again
        // and disable the save button
        // and then if they click cancel it will just requery the database
        // but if they have made changes it will prompt them to save changes
        // and then if they click yes it will save the changes
        // and then requery the database
        // for new animals it will just save the animal to the database
        // and then requery the database
        // and then disable the text boxes and enable the update button again
        // and disable the save button


        //how do I "name" controllers? 

        // Constructor for initializing the panel
        public AnimalPanel(RATAppBaseForm parentForm, RATAPPLibrary.Data.DbContexts.RatAppDbContext context, AnimalDto[] allAnimals, AnimalDto currAnimal)
        {
            _context = context;
            _animalService = new RATAPPLibrary.Services.AnimalService(context);
            parentForm.SetActivePanel(this); //FIXME this is going to get annoying fix all of these relationships and how active panel works 
            _allAnimals = allAnimals; //FIXME this is not correct, but I'm just getting refresh sort of functioning
            _animal = currAnimal; //FIXME this is not correct, but I'm just getting refresh sort of functioning

            InitializeComponent(currAnimal); //TODO need to re-structure to account for await nope, just use async lambda! 
            LoadSpinner();
        }

        private void InitializeComponent(AnimalDto animal)
        {
            //TODO should this just be _animal? Why or why not? follow this variable through and make sure I can make that switch first 

            // Set panel properties
            this.Size = new Size(1200, 800); // Increased panel size for better spacing
            this.BackColor = Color.White;
            this.Padding = new Padding(20); // Add padding around the panel for better spacing

            //Initialize the controls for the Animal Panel, buttons first as others are dependent on them (this is probably a bad idea, but works for now) 
            IntializeButtons();
            InitializeBottomButtons();

            //if ID is present than it is an existing animal
            //show existing animal versions = no editing, Update Data button, animal's associated data
            //first get the animal data by id 
            //TODO need to set to 0 and make a rule to never allow 0 as an ID 
            //TODO work on the logic on the state, I think I need to think through "state" as a whole for the application but this is okay for now
            if (animal != null && !_isEditMode)
            {
                LoadAnimalDataAsync(animal.Id);
            }
            else
            {
                //else if ID is not present than the user is creating a new animal or updating an existing animal 
                // show new animal version = editing, save button, blank boxes
                InitializeComboBoxes();
            }

            // Initialize the labels, photo, nav for the Animal Panel
            InitializeLabels();
            InitializePhotoBox();
            InitializeNavigationButtons();
        }

        private async void LoadAnimalDataAsync(int animalID)
        {
            var animal = await _animalService.GetAnimalByIdAsync(animalID);
            _animal = animal; //FIXME 
            InitializeTextBoxes(animal);
        }

        // initialize combo boxes
        private void InitializeComboBoxes()
        {
            string id = "";
            string name = "";
            string species = "";
            string sex = "";
            string variety = "";
            string color = "";
            string genotype = "";
            string ancestry = "";
            string breeder = "";
            string earType = "";
            string dam = "";
            string sire = "";
            string inbred = "";
            string imageUrl = ""; 


            //check if animal exists, this is the update case 
            //if it does, then we need to get the data from the database and populate the text boxes with the values
            if (_animal != null)
            {
                id = _animal.Id.ToString();
                name = _animal.Name;
                species = _animal.Species;
                sex = _animal.Sex;
                variety = _animal.Variety;
                color = _animal.Color;
                genotype = "TODO";
                ancestry = "TODO";
                breeder = _animal.Breeder;
                earType = "TODO";//_animal.earType; //TODO make ear type species specific 
                dam = _animal.Dam;
                sire = _animal.Sire;
                inbred = "TODO";
                imageUrl = _animal.imageUrl;

                InitializePhotoBox();
                animalPhotoBox.Image = Image.FromFile(imageUrl); //TODO clean up this logic 
            }
            // First column (left side)
            idTextBox = CreateTextBox(150, 20, id);
            animalNameTextBox = CreateTextBox(150, 60, name);
            speciesComboBox = CreateComboBox(150, 100, species);
            speciesComboBox.Name = "speciesComboBox"; //TODO this is how you set the names 
            sexComboBox = CreateComboBox(150, 140, sex);
            sexComboBox.Name = "sexComboBox";
            varietyComboBox = CreateComboBox(150, 180, variety);

            damComboBox = CreateComboBox(150, 220, dam);

            // Second column (right side)
            colorComboBox = CreateComboBox(490, 20, color);
            genotypeComboBox = CreateComboBox(490, 60, genotype);
            ancestryComboBox = CreateComboBox(490, 100, ancestry);
            breederInfoComboBox = CreateComboBox(490, 140, breeder);
            earTypeComboBox = CreateComboBox(490, 180, earType);
            sireComboBox = CreateComboBox(490, 220, sire);

            inbredTextBox = CreateTextBox(150, 450, "TODO");
            // Move commentsTextBox below everything else and make it larger
            //FIXME should have a multi line text box option
            commentsTextBox = new TextBox
            {
                Location = new Point(10, 290),
                Width = 680,
                Height = 120,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10F), // Make it consistent with labels
                Text = "TODO - should come from db",
                BackColor = Color.LightGray
            };

            // Add comboboxes to panel
            this.Controls.Add(animalNameTextBox);
            this.Controls.Add(idTextBox);
            this.Controls.Add(speciesComboBox);
            this.Controls.Add(sexComboBox);
            this.Controls.Add(varietyComboBox);
            this.Controls.Add(colorComboBox);
            this.Controls.Add(genotypeComboBox);
            this.Controls.Add(ancestryComboBox);
            this.Controls.Add(breederInfoComboBox);
            this.Controls.Add(commentsTextBox);
            this.Controls.Add(damComboBox);
            this.Controls.Add(sireComboBox);
            this.Controls.Add(earTypeComboBox);
            this.Controls.Add(inbredTextBox);

            NewAnimal(); //new animal settings
            SetComboBoxes();
        }

        private void SetComboBoxes()
        {
            // Iterate through all controls on the form
            foreach (Control control in this.Controls)
            {
                if (control is ComboBox comboBox)
                {
                    // Add "Create New" as an option
                    comboBox.Items.Clear(); // Clear existing items (optional, based on your use case)
                    comboBox.Items.AddRange(GetComboBoxItemsFromDatabase(control).ToArray()); // Add database items
                    comboBox.Items.Add("Create New");

                    // Subscribe to the SelectedIndexChanged event
                    comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
                }
            }
        }

        // Placeholder for fetching items from the database
        //TODO
        //this is going to be getting a list of options from each table in the database
        //which means that the libary will handle most of this 
        private List<string> GetComboBoxItemsFromDatabase(Control control)
        {
            var options = new List<string>();
            //if the combo box is species, then get the species from the database
            //if the combo box is sex, then get the sex from the database (or just populate for now)

            if (control.Name == "speciesComboBox")
            {
                //get species from the database //FIXME this is just for testing
                options.AddRange(new[] { "Mouse", "Rat" });
            }
            else if (control.Name == "sexComboBox")
            {
                options.AddRange(new[] { "Male", "Female", "Unknown", "Other" }); //FIXME this is just for testing
            }
            else
            {
                options.AddRange(new[] { "Option1", "Option2", "Option3" }); //FIXME this is just for testing
            }

            return options;
        }

        // Event handler for "Create New" functionality
        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem?.ToString() == "Create New")
            {
                // Open a dialog to get the new item from the user
                string newItem = PromptForNewItem();

                if (!string.IsNullOrWhiteSpace(newItem))
                {
                    // Add the new item to the database
                    AddNewItemToDatabase(newItem);

                    // Refresh the combo box items
                    comboBox.Items.Clear();
                    comboBox.Items.AddRange(GetComboBoxItemsFromDatabase(comboBox).ToArray());
                    comboBox.Items.Add("Create New"); //TODO

                    // Optionally, select the newly added item
                    comboBox.SelectedItem = newItem;
                }
                else
                {
                    // Revert to a default selection if no new item is added
                    comboBox.SelectedIndex = -1;
                }
            }
        }

        // Placeholder for a user prompt dialog
        //this needs to run through checks for each item type but for now lets just get it working TODO
        private string PromptForNewItem() //string itemName
        {
            // TODO: Replace with actual UI or input dialog
            // For now, use a simple input box
            return Microsoft.VisualBasic.Interaction.InputBox("Enter a new item:", "Create New Item", "");
        }

        // Placeholder for database insertion
        //this is for adding a new "type" of item such as a new species so this is not in the current scope 
        private void AddNewItemToDatabase(string newItem)
        {
            // TODO: Implement database logic to save the new item
            Console.WriteLine($"New item added to the database: {newItem}");
        }

        private void InitializeLabels()
        {
            // Font settings for consistency
            Font labelFont = new Font("Segoe UI", 10F, FontStyle.Regular);

            // First column (left side)
            idLabel = CreateLabel("Registration #/ID", 10, 20, labelFont);
            animalNameLabel = CreateLabel("Animal Name", 10, 60, labelFont);
            speciesLabel = CreateLabel("Species", 10, 100, labelFont);
            sexLabel = CreateLabel("Sex", 10, 140, labelFont);
            varietyLabel = CreateLabel("Variety", 10, 180, labelFont);
            damLabel = CreateLabel("Dam", 10, 220, labelFont);

            // Second column (right side)
            colorLabel = CreateLabel("Color", 380, 20, labelFont);
            genotypeLabel = CreateLabel("Genetics", 380, 60, labelFont);
            ancestryLabel = CreateLabel("Ancestry", 380, 100, labelFont);
            breederInfoLabel = CreateLabel("Breeder", 380, 140, labelFont);
            earTypeLabel = CreateLabel("Ear Type", 380, 180, labelFont);
            sireLabel = CreateLabel("Sire", 380, 220, labelFont);
            inbredLabel = CreateLabel("% Inbred", 10, 450, labelFont);

            // Move commentsLabel to a new position below everything else
            commentsLabel = CreateLabel("Comments", 10, 260, labelFont);

            // Add labels to panel
            this.Controls.Add(animalNameLabel);
            this.Controls.Add(idLabel);
            this.Controls.Add(speciesLabel);
            this.Controls.Add(sexLabel);
            this.Controls.Add(varietyLabel);
            this.Controls.Add(colorLabel);
            this.Controls.Add(genotypeLabel);
            this.Controls.Add(ancestryLabel);
            this.Controls.Add(breederInfoLabel);
            this.Controls.Add(commentsLabel);
            this.Controls.Add(damLabel);
            this.Controls.Add(sireLabel);
            this.Controls.Add(earTypeLabel);
            this.Controls.Add(inbredLabel);
        }

        // Method to create labels with dynamic positions and fonts
        private Label CreateLabel(string text, int x, int y, Font font)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = font
            };
        }

        // initialize textboxes w/ID
        // WIP, but idea is that if there is an ID the animal exists
        // and the text boxes will be disabled
        // the user will have to click the update animal details button
        // to make changes
        //TODO get the values from the database just for testing right now FIXME 
        //TODO ID should be a string, but leaving it for now as db edits are annoying 
        private void InitializeTextBoxes(AnimalDto animal)
        {
            // First column (left side)
            idTextBox = CreateTextBox(150, 20, animal.Id.ToString());
            animalNameTextBox = CreateTextBox(150, 60, animal.Name);
            speciesTextBox = CreateTextBox(150, 100, animal.Species);
            sexTextBox = CreateTextBox(150, 140, animal.Sex);
            varietyTextBox = CreateTextBox(150, 180, animal.Variety);
            damTextBox = CreateTextBox(150, 220, animal.Dam);

            // Second column (right side)
            colorTextBox = CreateTextBox(490, 20, animal.Color);
            genotypeTextBox = CreateTextBox(490, 60, "TODO");
            ancestryTextBox = CreateTextBox(490, 100, "TODO");
            breederInfoTextBox = CreateTextBox(490, 140, animal.Breeder);
            earTypeTextBox = CreateTextBox(490, 180, "TODO");
            sireTextBox = CreateTextBox(490, 220, animal.Sire);

            inbredTextBox = CreateTextBox(150, 450, "TODO");
            // Move commentsTextBox below everything else and make it larger
            //FIXME should have a multi line text box option
            commentsTextBox = new TextBox
            {
                Location = new Point(10, 290),
                Width = 680,
                Height = 120,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10F), // Make it consistent with labels
                Text = _animal.comments, //TODO
                BackColor = Color.LightGray
            };

            // Add textboxes to panel
            this.Controls.Add(animalNameTextBox);
            this.Controls.Add(idTextBox);
            this.Controls.Add(speciesTextBox);
            this.Controls.Add(sexTextBox);
            this.Controls.Add(varietyTextBox);
            this.Controls.Add(colorTextBox);
            this.Controls.Add(genotypeTextBox);
            this.Controls.Add(ancestryTextBox);
            this.Controls.Add(breederInfoTextBox);
            this.Controls.Add(commentsTextBox);
            this.Controls.Add(damTextBox);
            this.Controls.Add(sireTextBox);
            this.Controls.Add(earTypeTextBox);
            this.Controls.Add(inbredTextBox);

            //add animal image
            //TODO this should happen elsewhere
            if (animal.imageUrl != null)
            {
                animalPhotoBox.Image = Image.FromFile(animal.imageUrl);
                this.Controls.Add(animalPhotoBox);
            }

            AnimalExists();
        }

        // Method for existing animals 
        //TODO need to handle data caching so I'm not constantly going to the db but for not, db it is 
        private void AnimalExists()
        {
            //if ID is present than it is an existing animal
            //show existing animal versions = no editing, Update Data button, animal's associated data
            //no editing 
            //disable textboxes
            foreach (Control control in this.Controls)
            {
                if (control is TextBox textBox)
                {
                    textBox.Enabled = false; // Disable all textboxes
                }
            }

            //hide save button
            //show update button
            saveButton.Hide();
            updateButton.Show();
        }

        //Method for new animals
        private void NewAnimal()
        {
            //if an controls are disabled, enable them
            foreach (Control control in this.Controls)
            {
                if (control is TextBox textBox)
                {
                    textBox.Enabled = true; // Enable all textboxes
                }
            }

            //hide update button
            //show save button
            updateButton.Hide();
            saveButton.Show();
        }

        // Method to create textboxes
        private TextBox CreateTextBox(int x, int y, string text)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Width = 200,
                Font = new Font("Segoe UI", 10F),
                Text = text,
                BackColor = Color.LightGray
            };
        }

        // Method to create combobox
        private ComboBox CreateComboBox(int x, int y, string text)
        {
            return new ComboBox
            {
                Location = new Point(x, y),
                Width = 200,
                Font = new Font("Segoe UI", 10F),
                Text = text,
                BackColor = Color.LightGray
            };
        }

        // Method to initialize the animal's photo box
        private void InitializePhotoBox()
        {
            animalPhotoBox = new PictureBox
            {
                Location = new Point(750, 20),
                Size = new Size(200, 200),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom // To fit photo inside the box
            };

            // Add picture box to the form or panel
            this.Controls.Add(animalPhotoBox);
        }

        // Method to initialize the navigation buttons (Next and Previous)
        private void InitializeNavigationButtons()
        {
            // Previous button
            Button previousButton = CreateButton("Previous", 200, 700);
            previousButton.Click += (sender, e) => { PreviousButtonClick(sender, e); };

            // Next button
            Button nextButton = CreateButton("Next", 600, 700);
            nextButton.Click += (sender, e) => { NextButtonClick(sender, e); };

            this.Controls.Add(previousButton);
            this.Controls.Add(nextButton);
        }

        //would really be helpful to just know the index 
        private void PreviousButtonClick(object sender, EventArgs e)
        {
            int i = 0;
            foreach (AnimalDto animal in _allAnimals)
            {
                //once we get to animal
                if (animal.Id == _animal.Id && i > 0) //make sure there is a previous if there's not maybe go to the end of the line like a loop? For now, just stop 
                {
                    //go to the previous animal 
                    //get the next animal based on the index of the current animal + 1
                    LoadAnimalDataAsync(_allAnimals[i - 1].Id); //FIXME this logic doesn't make sense need to think through it but just for testing purposes 
                    this.Refresh();
                    break;
                }

                //keep track of index 
                i++;
            }

        }

        //next button click 
        private void NextButtonClick(object sender, EventArgs e)
        {
            //get data for next animal in collection
            //which would be from the _animals object 
            //so we probably want to store these when they're loaded on the main page 
            //flow would be 
            //next button clicked
            //if there is another animal in the collection, go to that animal
            //another way to do it would be to get the data from the grid if it is still there
            //another thing to consider is if we have already organized by species
            //maybe next and previous should make assumptions about species based on the animal that the user is on
            //this is a future consideration
            //for now, just get next animal in collection 
            //current problem to solve is stashed data 

            //find the current animal in the collection
            //get its index
            //go to the next index 
            //get the animal at that index
            //load the animal data
            //if there is no next animal, do nothing
            //maybe a list would be better here so that I could do next and previous vs. having to find index but not sure yet 
            int i = 0;
            foreach (AnimalDto animal in _allAnimals)
            {
                if (i == 1)
                {
                    //get the next animal based on the index of the current animal + 1
                    LoadAnimalDataAsync(animal.Id); //FIXME this logic doesn't make sense need to think through it but just for testing purposes 
                    //if there is no next animal, do nothing

                    //if there is a next animal, refresh the panel with the new animal data
                    this.Refresh();
                    //and break out of the loop
                    break;
                }
                if (animal.Id == _animal.Id)
                {
                    i++;
                }
            }
        }

        // Method to create buttons
        private Button CreateButton(string text, int x, int y)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Width = 100,
                Height = 40,
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
        }

        //Button UI logic below 
        //TODO need to better organize and work on set navigation for this panel  
        // Initialize the row of buttons at the bottom
        private void InitializeBottomButtons()
        {
            // Button 1: Update Animal Details TODO made seperate methods for these buttons will likely do that for each button
            //Button updateButton = CreateButton("Update Animal Details", 50, 630);

            // Button 2: Ancestry
            Button ancestryButton = CreateButton("Ancestry", 220, 630);

            // Button 3: Genetics
            Button geneticsButton = CreateButton("Genetics", 360, 630);

            // Button 4: Breeding History
            Button breedingHistoryButton = CreateButton("Breeding History", 500, 630);

            // Button 5: Documents
            Button documentsButton = CreateButton("Documents", 650, 630);

            // Button 6: Health
            Button healthButton = CreateButton("Health", 790, 630);

            this.Controls.Add(ancestryButton);
            this.Controls.Add(geneticsButton);
            this.Controls.Add(breedingHistoryButton);
            this.Controls.Add(documentsButton);
            this.Controls.Add(healthButton);
        }

        //TODO get the animal details from the database
        // and populate the text boxes with the values
        // and set the photo box with the animal's photo
        // and set the comments text box with the comments
        // Method to initialize the button for calculating % inbred
        private void InbredButton()
        {
            //create calc % inbred button 
            inbredButton = new Button
            {
                Location = new Point(400, 450),
                Width = 200,
                Height = 30,
                Text = "Calculate % Inbred",
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            //this should be a call to the library to calculate the % inbred
            inbredButton.Click += (sender, e) =>
            {
                // Logic to calculate % inbred
                //TODO get the values from the database just for testing right now FIXME
                string TODO = "TODO - should come from db"; //FIXME
                inbredTextBox.Text = TODO;
            };
        }

        private void SaveButton()
        {
            //create save button
            saveButton = new Button
            {
                Location = new Point(10, 630),
                Width = 150,
                Height = 40,
                Text = "Save Changes",
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.Green,
                FlatStyle = FlatStyle.Popup
            };
            saveButton.Click += async (sender, e) => //NOTE: putting this inside of an async lambda is what allows us to use await without setting the entire method to async! 
            {
                await SaveButtonClick(sender, e);
            };
        }

        //parse textbox into animal dto object
        private AnimalDto ParseAnimalData()
        {
            //FIXME just for now 
            DateTime dob = DateTime.Now;

            //FIXME breeder logic is in the works
            //there needs to be some logic to find the breeder in the db and pass that id to the animal object
            // for now just getting this to work
            string breeder = "0"; //this is the user's breeder id there is currently a bug that isn't storing the user as a breeder that needs to be fixed before this will actually work

            string line = "2";
            if (speciesComboBox.Text == "Mouse")
            {
                line = "1";
            }

            AnimalDto animal = new AnimalDto
            {
                Id = int.Parse(idTextBox.Text),
                Name = animalNameTextBox.Text,
                Sex = sexComboBox.Text, // Assuming there's a TextBox for sex
                DateOfBirth = dob,  //dobTextBox.Text : DateTime.Parse(dobTextBox.Text), // Assuming dobTextBox contains the Date of Birth FIXME 
                DateOfDeath = DateTime.Now,//string.IsNullOrWhiteSpace(dodTextBox.Text) //FIXME getting an error on NULL value for DateOfDeath should be allowed 
                                           //? null
                                           //: DateTime.Parse(dodTextBox.Text), // Assuming dodTextBox contains the Date of Death
                Species = speciesComboBox.Text,
                Line = line, // Assuming there's a TextBox for line
                Dam = "aDam",//damComboBox.Text, // Assuming there's a TextBox for dam
                Sire = "aSIRE",//sireComboBox.Text, // Assuming there's a TextBox for sire
                Variety = "AVARIETY",//varietyComboBox.Text, // Assuming there's a TextBox for variety
                Color = "ACOLOR",//colorComboBox.Text, // Assuming there's a TextBox for color TODO
                Breeder = "1",//breeder, // Assuming there's a TextBox for breeder TODO this should take in a text name of the breeder, it should be converted to a breeder id in the backend and then that should be used to seach the database or create a new breeder if not found 
                Genotype = "XYZ"//genotypeTextBox.Text, // Assuming there's a TextBox for genotype TODO
            };

            return animal;
        }

        private void UpdateButton()
        {
            //create calc % inbred button 
            updateButton = new Button
            {
                Location = new Point(10, 630),
                Width = 150,
                Height = 40,
                Text = "Update Data",
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.Green,
                FlatStyle = FlatStyle.Popup
            };
            //this should be a call to the library to calculate the % inbred
            updateButton.Click += (sender, e) =>
            {
                // Logic to update animal data 
                _isEditMode = true;

                //first clear out old controls
                this.Controls.Clear();
                InitializeComponent(_animal);
            };
        }

        private void IntializeButtons()
        {
            InbredButton();
            UpdateButton();
            SaveButton();

            // Add the button to the panel
            this.Controls.Add(inbredButton);
            this.Controls.Add(updateButton);
            this.Controls.Add(saveButton);
        }

        //TODO just testing, move to utils file
        private void LoadSpinner()
        {
            // Create and configure the spinner
            loadingSpinner = new PictureBox
            {
                Size = new Size(50, 50), // Adjust size as needed "C:\Users\earob\source\repos\RATAPP\RATAPPLibrary\RATAPP\Resources\Loading_2.gif"
                Image = Image.FromFile("C:\\Users\\earob\\source\\repos\\RATAPP\\RATAPPLibrary\\RATAPP\\Resources\\Loading_2.gif"), // Add a GIF to your project resources
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false // Initially hidden
            };

            // Position the spinner in the center of the form
            loadingSpinner.Location = new Point(
                (ClientSize.Width - loadingSpinner.Width) / 2,
                (ClientSize.Height - loadingSpinner.Height) / 2
            );

            // Add the spinner to the form
            Controls.Add(loadingSpinner);

            // Handle form resize to reposition the spinner
            Resize += (s, e) =>
            {
                loadingSpinner.Location = new Point(
                    (ClientSize.Width - loadingSpinner.Width) / 2,
                    (ClientSize.Height - loadingSpinner.Height) / 2
                );
            };
        }
        //FIXME repeated code but getting interface working for now 
        public async Task RefreshDataAsync()
        {
            int id;
            bool success = int.TryParse(idTextBox.Text, out id);
            if (success)
            {
                try
                {
                    // Show spinner
                    loadingSpinner.Visible = true;
                    Refresh(); // Force UI to repaint to show spinner

                    // Fetch animals asynchronously
                    _animal = await _animalService.GetAnimalByIdAsync(id);

                    //for testing
                    // Wait asynchronously for 3 seconds
                    await Task.Delay(3000); // 3000 milliseconds = 3 seconds

                    // Hide spinner
                    loadingSpinner.Visible = false;
                    Refresh(); // Force UI to repaint to not show spinner

                    MessageBox.Show("Data refresh complete", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    //this is an emergency catch really TODO fix this logic, maybe no finally? 
                    // Hide spinner
                    loadingSpinner.Visible = false;
                    Refresh(); // Force UI to repaint to not show spinner
                }
            }
            else
            {
                MessageBox.Show("could not get animal", "", MessageBoxButtons.OK);
            }

        }

        //save and update animal logic below 

        private async Task SaveButtonClick(object sender, EventArgs e)
        {
            AnimalDto animalDto = ParseAnimalData(); //first, parse the data from the text boxes

            if (animalDto == null) //if the data is invalid, show an error message and return
            {
                ShowMessage("Error: Animal data is invalid. Please check required fields.");
                return;
            }

            try
            {
                Animal animal = await _animalService.CreateAnimalAsync(animalDto); //start by creating the animal not using the method I created as it returns an error TODO on this 
                CompleteSaveProcess(animalDto, "Animal data saved successfully!");
                await GetAndResetAnimalData(animal); //TODO do I need to be passing back an entire animal object? why, or why not? 
            }
            catch (InvalidOperationException) //FIXME check this exception type but also that it contains: already exists
            {
                if (PromptUserToUpdate())
                {
                    await HandleUpdateAsync(animalDto); //if the animal already exists, prompt the user to update it
                }
                else
                {
                    ShowMessage("IDs must be unique. Please create a new animal record."); //if user chooses no, let them know they need a unique ID to create a new animal 
                }
            }
            catch (Exception ex) //if any other exceptions occur, log the error and notify the user
            {
                // Log the error and notify the user
                LogError(ex);
                ShowMessage("An unexpected error occurred. Please try again.");
            }
        }

        // Handle the create new animal process 
        private async Task HandleCreateNewAsync(AnimalDto animalDto)
        {
            try
            {
                Animal animal = await _animalService.CreateAnimalAsync(animalDto);
                await _animalService.GetAnimalByIdAsync(animal.Id);
                CompleteSaveProcess(animalDto, "Animal data saved successfully!");
            }
            catch (Exception ex)
            {
                LogError(ex);
                ShowMessage("Failed to save the animal record. Please try again.");
            }
        }

        //get and reset animal data 
        //set _isEditMode to false 
        //get an AnimalDto object back by id
        //use this to reset the text boxes with the updated data 
        //TODO likely a more efficient way to do this but for now this is working
        private async Task GetAndResetAnimalData(Animal animal)
        {
            _isEditMode = false;
            _animal = await _animalService.GetAnimalByIdAsync(animal.Id); //get the animal to re-populate the text boxes with the updated data
            this.Controls.Clear();// clear the controls, should go back to text boxes and buttons populated with new animal data
            this.InitializeComponent(_animal);
            this.Refresh();
        }

        // Handle the update process for an existing animal
        private async Task HandleUpdateAsync(AnimalDto animalDto)
        {
            try
            {
                Animal animal = await _animalService.UpdateAnimalAsync(animalDto);
                CompleteSaveProcess(animalDto, "Animal data updated successfully!");
                await GetAndResetAnimalData(animal); //get the updated animal data and reset the text boxes
            }
            catch (Exception ex)
            {
                LogError(ex);
                ShowMessage("Failed to update the animal record. Please try again.");
            }
        }

        // Helper methods for handling the high level save process
        private void CompleteSaveProcess(AnimalDto animalDto, string successMessage)
        {
            ShowMessage(successMessage);
            ToggleButtons(false);
        }

        // Helper methods for handling user update prompt 
        private bool PromptUserToUpdate()
        {
            DialogResult result = MessageBox.Show(
                "Update existing animal?",
                "Confirmation",
                MessageBoxButtons.YesNo);
            return result == DialogResult.Yes;
        }

        private void ToggleButtons(bool isEditMode)
        {
            saveButton.Visible = isEditMode;//if edit mode is true, show the save button
            updateButton.Visible = !isEditMode; //if edit mode is false, show the update button
        }

        private void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        //logging logic, TODO will be creating a logging service for this but not yet implemented 
        private void LogError(Exception ex)
        {
            // Add logging logic here (e.g., log to a file or monitoring system)
            Console.WriteLine(ex.Message);
        }
    }
}