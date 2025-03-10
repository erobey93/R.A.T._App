using RATAPP.Forms;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Genetics;
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
using System.Windows.Forms.VisualStyles;
using static System.Runtime.InteropServices.JavaScript.JSType;

//NOTES
// * I think passing a state around is likely the best way to handle whether we're in edit mode, or not
namespace RATAPP.Panels
{
    public partial class AnimalPanel : Panel, INavigable
    {
        // Class-level fields for controls specific to Animal Page
        private Label animalNameLabel;
        private Label regNumLabel;
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
        private TextBox regNumTextBox;
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
        private ComboBox markingComboBox;
        private ComboBox breederInfoComboBox;


        private PictureBox animalPhotoBox;
        private Button inbredButton;
        private Button saveButton;
        private Button updateButton;
        private Button cancelButton;
        private Button documentsButton;
        private Button healthButton; 
        private Button indAncestryButton; // Individual Ancestry Button vs Ancestry Page
        private Button prevButton;
        private Button nextButton;

        private FlowLayoutPanel thumbnailPanel; // Panel to hold the thumbnails

        //state of the panel
        private bool _isEditMode = false;

        //db context & services 
        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
        private RATAPPLibrary.Services.AnimalService _animalService;
        private RATAPPLibrary.Services.TraitService _traitService;
        private RATAPPLibrary.Services.SpeciesService _speciesService;

        private AnimalDto _animal; //FIXME this is not correct, but I'm just getting refresh sort of functioning
        private AnimalDto[] _allAnimals; //FIXME this should be cached data instead of passing around a potentially huge array of animals but still WIP
        private RATAppBaseForm _parentForm; //FIXME this is going to get annoying fix all of these relationships and how active panel works

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

        // Test image paths TODO should be getting from an AnimalImage table in the database - future feature 
        private List<string> imagePaths = new List<string>
        {
            "C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\AnimalPics\\00S0S_e4sHXNFmkdY_0t20CI_1200x900.jpg",
            "C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\AnimalPics\\00v0v_hu4XyatVj1Q_0t20CI_1200x900.jpg",
            "C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\AnimalPics\\IMG_0214.JPG",
            "C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\AnimalPics\\IMG_5197.JPG"
        };

        // Constructor for initializing the panel
        public AnimalPanel(RATAppBaseForm parentForm, RATAPPLibrary.Data.DbContexts.RatAppDbContext context, AnimalDto[] allAnimals, AnimalDto currAnimal)
        {
            _context = context;
            _animalService = new RATAPPLibrary.Services.AnimalService(context);
            _traitService = new RATAPPLibrary.Services.TraitService(context);
            _speciesService = new RATAPPLibrary.Services.SpeciesService(context);
            parentForm.SetActivePanel(this); //FIXME this is going to get annoying fix all of these relationships and how active panel works
            _parentForm = parentForm; //FIXME this is going to get annoying fix all of these relationships and how active panel works
            _allAnimals = allAnimals; //FIXME this is not correct, but I'm just getting refresh sort of functioning
            _animal = currAnimal; //FIXME this is not correct, but I'm just getting refresh sort of functioning

            InitializeComponent(currAnimal); //TODO need to re-structure to account for await nope, just use async lambda! 
            LoadSpinner();
        }

        private void InitializeComponent(AnimalDto animal)
        {
            //TODO should this just be _animal? Why or why not? follow this variable through and make sure I can make that switch first 
            _animal = animal;

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
            InitializeThumbnailPanel(); //TODO this is for the thumbnails of the animals, not yet fully implemented
        }

        private async void LoadAnimalDataAsync(int animalID)
        {
            var animal = await _animalService.GetAnimalByIdAsync(animalID); //get the animal data from the database FIXME should just set global variable and use that instead of passing around 
            _animal = animal; //FIXME 
            InitializeTextBoxes(_animal);//probably don't need to pass anything here, but for clarity I guess 
        }

        // initialize combo boxes
        private void InitializeComboBoxes()
        {
            string regNum = "";
            string name = "";
            string species = "";
            string sex = "";
            string variety = "";
            string color = "";
            string genotype = "";
            string marking = "";
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
                regNum = _animal.regNum.ToString();
                name = _animal.name;
                species = _animal.species;
                sex = _animal.sex;
                variety = _animal.variety;
                color = _animal.color;
                genotype = "TODO";
                marking = _animal.markings;
                breeder = _animal.breeder;
                earType = _animal.earType;//_animal.earType; //TODO make ear type species specific 
                dam = _animal.dam;
                sire = _animal.sire;
                imageUrl = _animal.imageUrl;
                inbred = "TODO";

                InitializePhotoBox();
                AddImageToAnimalTextbox(_animal);
                //animalPhotoBox.Image = Image.FromFile(imageUrl); //TODO clean up this logic 

            }
            // First column (left side)
            regNumTextBox = CreateTextBox(150, 20, regNum);
            animalNameTextBox = CreateTextBox(150, 60, name);
            speciesComboBox = CreateComboBox(150, 100, species);
            speciesComboBox.Name = "speciesComboBox"; //TODO this is how you set the names 
            sexComboBox = CreateComboBox(150, 140, sex);
            sexComboBox.Name = "sexComboBox";
            varietyComboBox = CreateComboBox(150, 180, variety);
            varietyComboBox.Name = "varietyComboBox";

            damComboBox = CreateComboBox(150, 220, dam);
            damComboBox.Name = "damComboBox";

            // Second column (right side)
            colorComboBox = CreateComboBox(490, 20, color);
            colorComboBox.Name = "colorComboBox";
            genotypeComboBox = CreateComboBox(490, 60, genotype);
            genotypeComboBox.Name = "genotypeComboBox";
            markingComboBox = CreateComboBox(490, 100, marking);
            markingComboBox.Name = "markingComboBox";
            breederInfoComboBox = CreateComboBox(490, 140, breeder);
            breederInfoComboBox.Name = "breederInfoComboBox";
            earTypeComboBox = CreateComboBox(490, 180, earType);
            earTypeComboBox.Name = "earTypeComboBox";
            sireComboBox = CreateComboBox(490, 220, sire);
            sireComboBox.Name = "sireComboBox";

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
            this.Controls.Add(regNumTextBox);
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
            this.Controls.Add(markingComboBox);

            NewAnimal(); //new animal settings
            SetComboBoxes();
        }

        private async void SetComboBoxes()
        {
            // Iterate through all controls on the form
            foreach (Control control in this.Controls)
            {
                if (control is ComboBox comboBox)
                {
                    //get the list of values from the db 
                    List<string> items = await GetComboBoxItemsFromDatabase(control); // Add database items
                    
                    // Add "Create New" as an option TODO need to set Create New as a button 
                    comboBox.Items.Clear(); // Clear existing items (optional, based on your use case)
                    comboBox.Items.AddRange(items.ToArray()); // Add database items
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
        private async Task<List<string>> GetComboBoxItemsFromDatabase(Control control)
        {
            var options = new List<string>();
            string animalSpecies = GetAnimalSpecies();

            // Use switch statement to handle different ComboBox names
            switch (control.Name)
            {
                case "speciesComboBox":
                    // Get species from the database
                    List<string> species = (List<string>)await _speciesService.GetAllSpeciesAsync();
                    options.AddRange(species);
                    break;

                case "sexComboBox":
                    // Use sex options (for now just populating for testing)
                    options.AddRange(new[] { "Male", "Female", "Unknown", "Other" }); // FIXME: for testing
                    break;

                case "varietyComboBox":
                    // Handle variety-related options (update as needed)
                    options.AddRange(new[] { "Variety1", "Variety2", "Variety3" }); // FIXME: for testing
                    break;

                case "colorComboBox":
                    // Handle color-related options (update as needed)
                    //get colors by species from the db 
                    List<string> animalTraits = await GetSpeciesTraitsByType(animalSpecies, "Color");
                    options.AddRange(animalTraits.ToArray()); // FIXME: for testing
                    break;

                case "genotypeComboBox":
                    // Handle genotype-related options (update as needed)
                    options.AddRange(new[] { "Genotype1", "Genotype2", "Genotype3" }); // FIXME: for testing
                    break;

                case "damComboBox":
                    string animalSex = "Female";
                    //if we are creating a new animal show all female (dam) options 
                    //or if they've filled in the species field, get it from there 

                    //Unknown means nothing filled out, so we need to get all Female options other option is to have a pop up box that says "Please fill out the species field first"
                    if (animalSpecies != "Unknown")
                    {
                        // Handle dam-related options (fetch data from database or service as needed)
                        AnimalDto[] dams = await _animalService.GetAnimalInfoBySexAndSpecies(animalSex, animalSpecies);
                        foreach (var dam in dams)
                        {
                            options.Add(dam.name);
                        }
                    }
                    else
                    {
                        // if no species, show all for appropriate sex 
                        // TODO handle mismatch in selected species and dam/sire type 
                        AnimalDto[] dams = await _animalService.GetAnimalsBySex(animalSex);
                        foreach (var dam in dams)
                        {
                            options.Add(dam.name);
                        }
                    }

                        break;

                case "sireComboBox":
                    animalSpecies = GetAnimalSpecies();
                    animalSex = "Male";
                    //if we are creating a new animal show all female (dam) options 
                    //or if they've filled in the species field, get it from there 

                    //Unknown means nothing filled out, so we need to get all Female options other option is to have a pop up box that says "Please fill out the species field first"
                    if (animalSpecies != "Unknown")
                    {
                        // Handle dam-related options (fetch data from database or service as needed)
                        AnimalDto[] sires = await _animalService.GetAnimalInfoBySexAndSpecies(animalSex, animalSpecies);
                        foreach (var sire in sires)
                        {
                            options.Add(sire.name);
                        }
                    }
                    else
                    {
                        // if no species, show all for appropriate sex 
                        // TODO handle mismatch in selected species and dam/sire type 
                        AnimalDto[] sires = await _animalService.GetAnimalsBySex(animalSex);
                        foreach (var sire in sires)
                        {
                            options.Add(sire.name);
                        }  
                    }

                    break;

                case "earTypeComboBox":
                    // Handle ear type-related options (update as needed)
                    options.AddRange(new[] { "EarType1", "EarType2", "EarType3" }); // FIXME: for testing
                    break;

                case "markingComboBox":
                    // Handle marking-related options (update as needed)
                    options.AddRange(new[] { "Marking1", "Marking2", "Marking3" }); // FIXME: for testing
                    break;

                case "breederInfoComboBox":
                    // Handle breeder information-related options (update as needed)
                    options.AddRange(new[] { "Breeder1", "Breeder2", "Breeder3" }); // FIXME: for testing
                    break;

                default:
                    // For other ComboBoxes, just add some default options for now
                    options.AddRange(new[] { "Option1", "Option2", "Option3" }); // FIXME: for testing
                    break;
            }

            return options;
        }

        private async Task <List<string>> GetSpeciesTraitsByType(string species, string type)
        {
            try
            {
                //get the traits for the species from the database 
                List<string> traits = (List<string>)await _traitService.GetTraitsByTypeAndSpeciesAsync(type, species);
                return traits;
            }
            catch (Exception ex)
            {
                // Log the error and notify the user
                LogError(ex);
                ShowMessage("Failed to get traits for the species. Please try again.");
                return null; 
            }
        }

        //find species
        private string GetAnimalSpecies()
        {
            //first, check if _animal is populated
            if (_animal != null)
            {
                return _animal.species;
            }
            else if (speciesComboBox.SelectedItem != null && speciesComboBox.Text != "Add New") //FIXME make sure this is the correct text
            {
                return speciesComboBox.SelectedItem.ToString(); //or .Text? FIXME 
            }
            else
            {
                //return unknown, use to tell the user that they need to fill out the species field first
                return "Unknown";
                //TODO handle case where species is unknown, this should be a pop up box that says "Please fill out the species field first"  MessageBox.Show("Please fill out the species field first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event handler for "Create New" functionality
        private async void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem?.ToString() == "Create New")
            {
                // Open a dialog to get the new item from the user
                string newItem = PromptForNewItem();

                if (!string.IsNullOrWhiteSpace(newItem))
                {
                    // Add the new item to the database
                    AddNewItemToDatabase(newItem);

                    //get the updated data from the database 
                    List<string> items = await GetComboBoxItemsFromDatabase(comboBox);

                    // Refresh the combo box items
                    comboBox.Items.Clear();
                    comboBox.Items.AddRange(items.ToArray());
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

        private void AnimalImageClicked(object sender, EventArgs e)
        {
            // Open a file dialog to select an image
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image Files (*.jpg, *.jpeg, *.png, *.gif, *.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Select an Image"
            };

            // If the user selects an image, update the image box
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Get the selected file path
                    string selectedFilePath = dialog.FileName;

                    // Ensure the file exists and is a valid image
                    if (File.Exists(selectedFilePath))
                    {
                        // Check if the file is an image by looking at the file extension
                        string[] validImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" };
                        string fileExtension = Path.GetExtension(selectedFilePath).ToLower();

                        if (validImageExtensions.Contains(fileExtension))
                        {
                            // Load the image from the file
                            Image newImage = Image.FromFile(selectedFilePath);

                            // Set the image in the animalPhotoBox
                            animalPhotoBox.Image = newImage;

                            // Normalize the file path (this removes double slashes)
                            string normalizedFilePath = CleanFilePath(selectedFilePath);

                            // Update the image URL for the animal (using the normalized file path)
                            _animal.imageUrl = normalizedFilePath;

                            // Show a message confirming the image update
                            ShowMessage("Image updated successfully! Image URL: " + _animal.imageUrl);

                            // Refresh the control to ensure the new image is displayed
                            Refresh();  // This should update the UI to reflect the new image
                        }
                        else
                        {
                            ShowMessage("Error: The selected file is not a valid image. Please choose a valid image file.");
                        }
                    }
                    else
                    {
                        ShowMessage("Error: The selected file does not exist.");
                    }
                }
                catch (Exception ex)
                {
                    // Handle any errors (e.g., if the file isn't a valid image or any other exceptions)
                    ShowMessage("Error updating image: " + ex.Message);
                }
            }
        }

        // Method to normalize the file path (removes extra slashes and normalizes path separators)
        private string CleanFilePath(string filePath)
        {
            // Replace double backslashes with a single backslash
            return filePath.Replace(@"\\", @"\");
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
            regNumLabel = CreateLabel("Registration #/ID", 10, 20, labelFont);
            animalNameLabel = CreateLabel("Animal Name", 10, 60, labelFont);
            speciesLabel = CreateLabel("Species", 10, 100, labelFont);
            sexLabel = CreateLabel("Sex", 10, 140, labelFont);
            varietyLabel = CreateLabel("Variety", 10, 180, labelFont);
            damLabel = CreateLabel("Dam", 10, 220, labelFont);

            // Second column (right side)
            colorLabel = CreateLabel("Color", 380, 20, labelFont);
            genotypeLabel = CreateLabel("Genetics", 380, 60, labelFont);
            markingsLabel = CreateLabel("Markings", 380, 100, labelFont);
            breederInfoLabel = CreateLabel("Breeder", 380, 140, labelFont);
            earTypeLabel = CreateLabel("Ear Type", 380, 180, labelFont);
            sireLabel = CreateLabel("Sire", 380, 220, labelFont);
            inbredLabel = CreateLabel("% Inbred", 10, 450, labelFont);

            // Move commentsLabel to a new position below everything else
            commentsLabel = CreateLabel("Comments", 10, 260, labelFont);

            // Add labels to panel
            this.Controls.Add(animalNameLabel);
            this.Controls.Add(regNumLabel);
            this.Controls.Add(speciesLabel);
            this.Controls.Add(sexLabel);
            this.Controls.Add(varietyLabel);
            this.Controls.Add(colorLabel);
            this.Controls.Add(genotypeLabel);
            this.Controls.Add(markingsLabel);
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
            regNumTextBox = CreateTextBox(150, 20, _animal.regNum);
            animalNameTextBox = CreateTextBox(150, 60, _animal.name);
            speciesTextBox = CreateTextBox(150, 100, _animal.species);
            sexTextBox = CreateTextBox(150, 140, _animal.sex);
            varietyTextBox = CreateTextBox(150, 180, _animal.variety);
            damTextBox = CreateTextBox(150, 220, _animal.dam);

            // Second column (right side)
            colorTextBox = CreateTextBox(490, 20, _animal.color);
            genotypeTextBox = CreateTextBox(490, 60, "TODO");
            markingsTextBox = CreateTextBox(490, 100, _animal.markings);
            breederInfoTextBox = CreateTextBox(490, 140, _animal.breeder);
            earTypeTextBox = CreateTextBox(490, 180, _animal.earType);
            sireTextBox = CreateTextBox(490, 220, _animal.sire);

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
                Text = _animal.comment,
                BackColor = Color.LightGray
            };

            // Add textboxes to panel
            this.Controls.Add(animalNameTextBox);
            this.Controls.Add(regNumTextBox);
            this.Controls.Add(speciesTextBox);
            this.Controls.Add(sexTextBox);
            this.Controls.Add(varietyTextBox);
            this.Controls.Add(colorTextBox);
            this.Controls.Add(genotypeTextBox);
            this.Controls.Add(markingsTextBox);
            this.Controls.Add(breederInfoTextBox);
            this.Controls.Add(commentsTextBox);
            this.Controls.Add(damTextBox);
            this.Controls.Add(sireTextBox);
            this.Controls.Add(earTypeTextBox);
            this.Controls.Add(inbredTextBox);

            //add animal image
            AddImageToAnimalTextbox(_animal);

            //Update the state of the panel
            AnimalExists();
        }

        // Add image to textbox
        private void AddImageToAnimalTextbox(AnimalDto animal)
        {
            if (animal.imageUrl != null)
            {
                string imageUrl = CleanFilePath(animal.imageUrl);
                animalPhotoBox.Image = Image.FromFile(imageUrl);
                if(_isEditMode)
                {
                    animalPhotoBox.Click += AnimalImageClicked;
                }
            }
            else
            {
                if (_isEditMode)
                {
                    animalPhotoBox.Click += AnimalImageClicked;
                }
            }
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
            cancelButton.Hide();
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
            //TODO set up cancel here but it should return to home page 
            cancelButton.Show();
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
        //TODO work on formatting a bit more
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

        // Initialize the scrollable thumbnail panel
        private void InitializeThumbnailPanel()
        {
            // Initialize the FlowLayoutPanel (which will allow scrolling)
            thumbnailPanel = new FlowLayoutPanel
            {
                Size = new Size(200, 100), // Adjust size as needed for thumbnails
                Location = new Point(750, 240), // Adjust location to be below the main image
                AutoScroll = true, // Enable scrolling if thumbnails overflow
                FlowDirection = FlowDirection.LeftToRight // Arrange thumbnails horizontally
            };
            this.Controls.Add(thumbnailPanel);

            // Add thumbnails to the panel
            foreach (var imagePath in imagePaths)
            {
                PictureBox thumbnail = new PictureBox
                {
                    Image = Image.FromFile(imagePath), // Load the thumbnail image
                    Size = new Size(50, 50), // Set the thumbnail size
                    SizeMode = PictureBoxSizeMode.Zoom, // Scale the image to fit
                    Margin = new Padding(5), // Add spacing between thumbnails
                    Cursor = Cursors.Hand // Change cursor to hand on hover
                };

                // Event handler to update the main image when the thumbnail is clicked
                thumbnail.Click += (sender, e) => OnThumbnailClick(imagePath);

                // Add the thumbnail to the panel
                thumbnailPanel.Controls.Add(thumbnail);
            }
        }

        // Handle the click event on a thumbnail
        private void OnThumbnailClick(string imagePath)
        {
            // Change the main image box to the selected thumbnail's image
            animalPhotoBox.Image = Image.FromFile(imagePath);
        }

        // Method to initialize the navigation buttons (Next and Previous)
        private void InitializeNavigationButtons()
        {
            // Remove any existing buttons with the same name
            var buttonsToRemove = this.Controls.OfType<Button>()
                .Where(b => b.Name == "Previous" || b.Name == "Next").ToList();
            foreach (var button in buttonsToRemove)
            {
                this.Controls.Remove(button);
            }

            // Create and re-add buttons
            prevButton = CreateButton("Previous", 200, 700);
            prevButton.Click += PreviousButtonClick;

            nextButton = CreateButton("Next", 600, 700);
            nextButton.Click += NextButtonClick;

            this.Controls.Add(prevButton);
            this.Controls.Add(nextButton);
        }

        //would really be helpful to just know the index 
        private void PreviousButtonClick(object sender, EventArgs e)
        {
            if (prevButton == null)
            {
                MessageBox.Show("Previous button is not initialized!");
                return;
            }
            int i = 0;
            //_isNavMode = true; //set the navigation mode to true
            foreach (AnimalDto animal in _allAnimals)
            {
                //once we get to animal
                if (animal.Id == _animal.Id && i > 0) //make sure there is a previous if there's not maybe go to the end of the line like a loop? For now, just stop 
                {
                    //go to the previous animal 
                    //first clear out old controls
                    this.Controls.Clear();
                    InitializeComponent(_allAnimals[i - 1]);
                    break;
                }

                //keep track of index 
                i++;
            }

        }

        private void UpdateButtonClick(object sender, EventArgs e)
        {
            // Logic to update animal data 
            _isEditMode = true;

            //first clear out old controls
            this.Controls.Clear();
            InitializeComponent(_animal);
        }

        private async Task CancelButtonClick(object sender, EventArgs e)
        {
            // Make sure that we're in edit mode. If we aren't, we shouldn't be here anyways, but just in case, check.
            if (_isEditMode)
            {
                // Show message box with prompt to save changes.
                DialogResult result = MessageBox.Show("You have unsaved changes. Do you want to save them?",
                                                      "Confirm Cancel",
                                                      MessageBoxButtons.YesNoCancel,
                                                      MessageBoxIcon.Question);

                // If the user clicks 'Yes', save changes
                if (result == DialogResult.Yes)
                {
                    // Save changes (implement the actual saving logic here)
                    await SaveButtonClick(sender, e);
                }
                // If the user clicks 'No', don't save changes
                else if (result == DialogResult.No)
                {
                    // Set edit mode to false after deciding what to do with the changes
                    _isEditMode = false;

                    //clear the controls
                    this.Controls.Clear();

                    // Reload the page in non-edit mode (or update the UI to reflect the changes)
                    InitializeComponent(_animal);
                }
                // If the user clicks 'Cancel', do nothing and return to edit mode
                else if (result == DialogResult.Cancel)
                {
                    return; // Do nothing, keep the user in edit mode
                } 
            }
            else
            {
                // adding a new animal so check if there is data in the text boxes TODO
                //if //data in the text boxes 
                   // {
                    // if there is, prompt the user to save the data
                    // Show message box with prompt to save changes.
                    DialogResult result = MessageBox.Show("You have unsaved changes. Do you want to save them?",
                                                          "Confirm Cancel",
                                                          MessageBoxButtons.YesNoCancel,
                                                          MessageBoxIcon.Question);

                    // If the user clicks 'Yes', save changes
                    if (result == DialogResult.Yes)
                    {
                        // Save changes (implement the actual saving logic here)
                        await SaveButtonClick(sender, e);
                    }
                    // If the user clicks 'No', don't save changes
                    else if (result == DialogResult.No)
                    {
                        // Set edit mode to false after deciding what to do with the changes
                        _isEditMode = false;

                    //return to home page
                    _parentForm.HomeButton_Click(sender, e);
                }
                // If the user clicks 'Cancel', do nothing and return to edit mode
                else if (result == DialogResult.Cancel)
                    {
                        return; // Do nothing, keep the user in edit mode
                    }
                }
               
           // }
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
            //_isNavMode = true;
            foreach (AnimalDto animal in _allAnimals)
            {
                if (i == 1)
                {
                    //get the next animal based on the index of the current animal + 1
                    //LoadAnimalDataAsync(animal.Id); //FIXME this logic doesn't make sense need to think through it but just for testing purposes 
                    //if there is no next animal, do nothing
                    //first clear out old controls
                    this.Controls.Clear();
                    InitializeComponent(animal);
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
            // Button 2: Ancestry
            //Button ancestryButton = CreateButton("Ancestry", 220, 630);
            AncestryButton();

            // Button 3: Genetics
            Button geneticsButton = CreateButton("Genetics", 360, 630);

            // Button 4: Breeding History
            Button breedingHistoryButton = CreateButton("Breeding History", 500, 630);

            // Button 5: Documents
            //Button documentsButton = CreateButton("Documents", 650, 630);
            DocumentsButton(); //create button and set click event 

            // Button 6: Health
            HealthButton();

            this.Controls.Add(indAncestryButton);
            this.Controls.Add(geneticsButton);
            this.Controls.Add(breedingHistoryButton);
            this.Controls.Add(documentsButton);
            this.Controls.Add(healthButton);
        }

        //health button 
        private void HealthButton()
        {
            //create health button
            healthButton = CreateButton("Health", 790, 630);
            healthButton.Click += (sender, e) =>
            {
                // Logic to show health
                //TODO get the health from the database
                // and display it in a new window
                // Open the CreateAccountForm
                var healthForm = new HealthRecordForm(_parentForm, _context, _animal);
                healthForm.Show();
            };
        }

        //documents button
        //TODO just testing out 
        //I would like this to have ALL documents for the animal
        //I may end up deleting this andz
        private void DocumentsButton()
        {
            //create documents button
            documentsButton = CreateButton("Documents", 650, 630);
            documentsButton.Click += (sender, e) =>
            {
                // Logic to show documents
                //TODO get the documents from the database
                // and display them in a new window
                // Open the CreateAccountForm
                var documentForm = new DocumentsForm(_parentForm, _context);
                documentForm.Show();
            };
        }

        //ancestry button 
        //TODO just the start 
        //I would like this to have a way to update ancestry, print 
        private void AncestryButton()
        {
            //create ancestry button
            indAncestryButton = CreateButton("Ancestry", 220, 630);
            indAncestryButton.Click += (sender, e) =>
            {
                // Logic to show ancestry
                //TODO get the ancestry from the database
                // and display it in a new window
                // Open the CreateAccountForm
                var ancestryForm = new IndividualAnimalAncestryForm(_parentForm, _context, _animal); //TODO change name to form if I decide to keep it that way, maybe do a form and a panel option for different buttons and ask folks which they like best 
                ancestryForm.Show();
            };
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

            //FIXME this is just for testing right now need a way to manage lines i.e. "add new" or "select from list" 
            string line = "2";
            if (speciesComboBox.Text == "Mouse")
            {
                line = "1";
            }

            AnimalDto animal = new AnimalDto
            {
                Id = _animal.Id,
                regNum = regNumTextBox.Text,
                name = animalNameTextBox.Text,
                sex = sexComboBox.Text, // Assuming there's a TextBox for sex
                DateOfBirth = dob,  //dobTextBox.Text : DateTime.Parse(dobTextBox.Text), // Assuming dobTextBox contains the Date of Birth FIXME 
                DateOfDeath = DateTime.Now,//string.IsNullOrWhiteSpace(dodTextBox.Text) //FIXME getting an error on NULL value for DateOfDeath should be allowed 
                                           //? null
                                           //: DateTime.Parse(dodTextBox.Text), // Assuming dodTextBox contains the Date of Death
                species = speciesComboBox.Text,
                comment = commentsTextBox.Text,
                imageUrl = _animal.imageUrl, //FIXME this is being set inside of the click image box method so I think this should be fine like this TODO just hardcoded right now 
                Line = line, // Assuming there's a TextBox for line
                dam = "aDam",//damComboBox.Text, // Assuming there's a TextBox for dam
                sire = "aSIRE",//sireComboBox.Text, // Assuming there's a TextBox for sire
                variety = varietyComboBox.Text, // Assuming there's a TextBox for variety
                color = colorComboBox.Text, // Assuming there's a TextBox for color TODO
                breeder = "1",//breeder, // Assuming there's a TextBox for breeder TODO this should take in a text name of the breeder, it should be converted to a breeder id in the backend and then that should be used to seach the database or create a new breeder if not found 
                genotype = "XYZ"//genotypeTextBox.Text, // Assuming there's a TextBox for genotype TODO
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

        //allow the user to cancel making updating, or making a new animal 
        private void CancelButton()
        {
            //create calc % inbred button 
            cancelButton = new Button
            {
                Location = new Point(10, 680),
                Width = 150,
                Height = 40,
                Text = "Cancel",
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.Red,
                FlatStyle = FlatStyle.Popup
            };
            //this should be a call to the library to calculate the % inbred
            cancelButton.Click += async (sender, e) =>
            {
                await CancelButtonClick(sender, e); 
            };
        }

        private void IntializeButtons()
        {
            InbredButton();
            UpdateButton();
            SaveButton();
            CancelButton();
            InitializeNavigationButtons(); //initialize the navigation buttons

            // Add the button to the panel
            this.Controls.Add(inbredButton);
            this.Controls.Add(updateButton);
            this.Controls.Add(saveButton);
            this.Controls.Add(cancelButton);
            this.Controls.Add(nextButton);
            this.Controls.Add(prevButton);
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
            bool success = int.TryParse(regNumTextBox.Text, out id);
            if (success)
            {
                try
                {
                    // Show spinner
                    loadingSpinner.Visible = true;
                    Refresh(); // Force UI to repaint to show spinner

                    // Fetch animals asynchronously
                    _animal = await _animalService.GetAnimalByIdAsync(_animal.Id);

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
            throw new NotImplementedException();
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
            cancelButton.Visible = isEditMode; //if edit mode is true, show the cancel button
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