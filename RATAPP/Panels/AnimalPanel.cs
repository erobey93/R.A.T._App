﻿using Microsoft.Azure.Amqp.Framing;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Internal;
using RATAPP.Forms;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Migrations;
using RATAPPLibrary.Services;
using RATAPPLibrary.Services.Genetics;
using System.Runtime.Serialization;

namespace RATAPP.Panels
{
    /// <summary>
    /// Panel for managing individual animal records in the R.A.T. App.
    /// Provides a comprehensive interface for viewing, creating, and editing animal information.
    /// 
    /// Key Features:
    /// - Animal Information Display/Edit:
    ///   * Basic details (name, ID, species)
    ///   * Physical characteristics
    ///   * Lineage information
    ///   * Image management
    /// 
    /// UI Components:
    /// - Text fields for data entry
    /// - Combo boxes for selections
    /// - Image handling with thumbnails
    /// - Navigation and action buttons
    /// 
    /// Modes:
    /// - View Mode: Display only
    /// - Edit Mode: Allow modifications
    /// - New Animal Mode: Create records
    /// 
    /// State Management:
    /// - Tracks current animal data
    /// - Manages parent relationships
    /// - Handles image references
    /// - Maintains edit state
    /// 
    /// Known Limitations:
    /// - Hardcoded image paths
    /// - Basic error handling
    /// - Limited data validation
    /// - Some TODO implementations
    /// 
    /// Dependencies:
    /// - Various services (Animal, Lineage, etc.)
    /// - Windows Forms components
    /// - Database context
    /// </summary>
    public partial class AnimalPanel : Panel, INavigable
    {
        // Services
        private readonly RatAppDbContextFactory _contextFactory;
        private readonly AnimalService _animalService;
        private readonly LineageService _lineageService;
        private readonly TraitService _traitService;
        private readonly SpeciesService _speciesService;
        private readonly BreedingCalculationService _geneticsService; 
        private readonly SemaphoreSlim _loadingSemaphore = new SemaphoreSlim(1, 1);

        // State
        private bool _isEditMode = false;
        private AnimalDto _animal;
        private AnimalDto _dam;
        private AnimalDto _sire;
        private AnimalDto[] _dams;
        private AnimalDto[] _sires;
        private AnimalDto[] _allAnimals;
        private readonly RATAppBaseForm _parentForm;

        // UI Controls
        private Label animalNameLabel, regNumLabel, speciesLabel, sexLabel, varietyLabel;
        private Label colorLabel, genotypeLabel, breederInfoLabel, commentsLabel;
        private Label damLabel, sireLabel, inbredLabel, earTypeLabel, markingsLabel;
        private Label weightLabel, cageNumberLabel, dobLabel, dodLabel, numGensLabel;

        private TextBox animalNameTextBox, regNumTextBox, speciesTextBox, sexTextBox;
        private TextBox varietyTextBox, colorTextBox, genotypeTextBox, breederInfoTextBox;
        private TextBox commentsTextBox, damTextBox, sireTextBox, inbredTextBox;
        private TextBox earTypeTextBox, markingsTextBox, dobTextBox, dodTextBox;
        private TextBox weightTextBox, cageNumberTextBox, numGensTextBox;
        private DateTimePicker dobPicker;
        private DateOnly dobDate; 

        private ComboBox speciesComboBox, sexComboBox, varietyComboBox, colorComboBox;
        private ComboBox genotypeComboBox, ancestryComboBox, damComboBox, sireComboBox;
        private ComboBox earTypeComboBox, markingComboBox, breederInfoComboBox;
        private ComboBox cageNumberComboBox;

        private PictureBox animalPhotoBox;
        private PictureBox loadingSpinner;
        private Button inbredButton, saveButton, updateButton, cancelButton;
        private Button documentsButton, healthButton, indAncestryButton;
        private Button prevButton, nextButton, geneticsButton, breedingHistoryButton;

        private FlowLayoutPanel thumbnailPanel;
        private TableLayoutPanel mainContainer;
        private Panel dataEntryPanel;
        private Panel imagePanel;
        private Panel actionButtonPanel;
        private Panel featureButtonPanel;

        private List<string> animalImageUrls;

        //private List<string> coatType;
        //private List<string> color;
        //private List<string> earType; 
        //private List<string> markingType;


        public AnimalPanel(RATAppBaseForm parentForm, RatAppDbContextFactory contextFactory, AnimalDto[] allAnimals,AnimalDto currAnimal)
        {
            _contextFactory = contextFactory;
            _animalService = new AnimalService(contextFactory);
            _traitService = new TraitService(contextFactory);
            _speciesService = new SpeciesService(contextFactory);
            _lineageService = new LineageService(contextFactory);
            _geneticsService = new BreedingCalculationService(contextFactory);

            parentForm.SetActivePanel(this);
            _parentForm = parentForm;
            _allAnimals = allAnimals;

            InitializeComponent(currAnimal);
            LoadSpinner();
        }

        /// <summary>
        /// Initializes the panel's UI components and loads animal data.
        /// 
        /// Process:
        /// 1. Sets up panel properties
        /// 2. Initializes UI components
        /// 3. Loads animal data if provided
        /// 4. Configures edit mode
        /// 
        /// Components Created:
        /// - Labels and text fields
        /// - Combo boxes for selections
        /// - Image display areas
        /// - Action buttons
        /// 
        /// State Configuration:
        /// - Edit mode determined by animal parameter
        /// - UI elements enabled/disabled accordingly
        /// - Data loaded from services if animal exists
        /// </summary>
        /// <param name="animal">Animal data to display, or null for new animal</param>
        private void InitializeComponent(AnimalDto animal)
        {
            _animal = animal;
            if (_animal == null)
            {
                _isEditMode = true;
            }

            this.Size = new Size(1200, 800);
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            IntializeButtons();
            InitializeBottomButtons();

            if (animal != null && !_isEditMode)
            {
                LoadAnimalDataAsync(animal.Id);
            }
            else
            {
                InitializeComboBoxes();
                //hide most buttons in edit mode 
                documentsButton.Hide();
                healthButton.Hide();
                indAncestryButton.Hide();
                prevButton.Hide();
                nextButton.Hide();
                geneticsButton.Hide();
                breedingHistoryButton.Hide();

            }

            InitializeLabels();
            InitializePhotoBox();
            InitializeThumbnailPanel();
        }

        // initialize combo boxes
        private async void InitializeComboBoxes()
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
            string dob = "";
            string earType = "";
            string dam = "";
            string sire = "";
            string imageUrl = "";


            //check if animal exists, this is the update case 
            //if it does, then we need to get the data from the database and populate the text boxes with the values
            if (_animal != null)
            {
                //don't allow reg # to be changed once set right now 
                regNumTextBox.ReadOnly = true; 
                await GetInbredCoEfficient(); //get and set inbred coefficient

                //TODO resetting dam and sire here but this won't always need to happen, so re-think this logic to be more efficient 
                var defaultAnimal = await GetOrCreateDefaultAnimal(0);
                _dam = defaultAnimal;
                _sire = defaultAnimal;

                //get most recent dam and sire 
                await GetAnimalDamAndSire();

                regNum = _animal.regNum.ToString();
                name = _animal.name;
                species = _animal.species;
                sex = _animal.sex;
                variety = _animal.variety;
                color = _animal.color;
                genotype = _animal.genotype;
                marking = _animal.markings;
                breeder = _animal.breeder;
                earType = _animal.earType;
                dam = _dam.name;
                sire = _sire.name;
                imageUrl = _animal.imageUrl;
                dob = _animal.DateOfBirth.Date.ToString(); 

                InitializePhotoBox();
                AddImageToAnimalTextbox(_animal);
                Refresh();
            }
           
            // First column (left side)
            regNumTextBox = CreateTextBox(150, 20, regNum);
            animalNameTextBox = CreateTextBox(150, 60, name);
            animalNameTextBox.Name = "nameTextBox";
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
            //breederInfoComboBox = CreateComboBox(490, 140, breeder);
            //breederInfoComboBox.Name = "breederInfoComboBox";
            dobPicker = new DateTimePicker();
            dobPicker.Name = "dobPicker";
            dobPicker.Format = DateTimePickerFormat.Short; // Display date as MM/DD/YYYY
            dobPicker.Value = DateTime.Now; // Default to today's date

            // Optional: Set the range to ensure valid dates for DOB
            dobPicker.MinDate = new DateTime(1900, 1, 1); // Minimum DOB
            dobPicker.MaxDate = DateTime.Now; // Maximum DOB is today
            dobPicker.Location = new Point(490, 140); // Set location on the form
            dobDate = DateOnly.FromDateTime(dobPicker.Value);
            earTypeComboBox = CreateComboBox(490, 180, earType);
            earTypeComboBox.Name = "earTypeComboBox";
            sireComboBox = CreateComboBox(490, 220, sire);
            sireComboBox.Name = "sireComboBox";

            inbredTextBox = CreateTextBox(150, 450, "0");
            inbredTextBox.ReadOnly = true;
            await GetInbredCoEfficient(); //get and set inbred coefficient
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
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Add comboboxes to panel
            this.Controls.Add(animalNameTextBox);
            this.Controls.Add(regNumTextBox);
            this.Controls.Add(speciesComboBox);
            this.Controls.Add(sexComboBox);
            this.Controls.Add(varietyComboBox);
            this.Controls.Add(colorComboBox);
            //this.Controls.Add(genotypeComboBox);
            this.Controls.Add(ancestryComboBox);
            this.Controls.Add(breederInfoComboBox);
            this.Controls.Add(commentsTextBox);
            this.Controls.Add(damComboBox);
            this.Controls.Add(sireComboBox);
            this.Controls.Add(earTypeComboBox);
            this.Controls.Add(inbredTextBox);
            this.Controls.Add(markingComboBox);
            this.Controls.Add(dobPicker);

            genotypeComboBox.AllowDrop = false;


            NewAnimal(); //new animal settings
            if(_animal == null)
            {
                SetComboBoxes("Rat"); //FIXME this may break the world 
            }
            else
            {
                SetComboBoxes(_animal.species);
            }
           
        }

        private async void AnimalImageClicked(object sender, EventArgs e)
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

                            //add image to database? FIXME there is something wrong with this flow I need to go through it and re-organize it
                            bool updateImage = await _animalService.UpdateAnimalImageByRegAsync(_animal.regNum, _animal.imageUrl);
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

        // Add image to textbox
        private void AddImageToAnimalTextbox(AnimalDto animal)
        {
            //check that the image url exists 
            if (animal.AdditionalImages != null)
            {
                try
                {
                    //get the correctly formatted image url and set the image 
                    string imageUrl = CleanFilePath(animal.AdditionalImages.First());
                    animalPhotoBox.Image = Image.FromFile(imageUrl);
                }
                catch (Exception e)
                {
                    //animalPhotoBox.Image = Image.FromFile("C:\\Users\\earob\\source\\repos\\R.A.T._APP\\RATAPP\\Resources\\RATAPPLogo.png");  //FIXME - for now, show the error but continue with the rest of the logic 
                }
            }
            else
            {
              //leave blank
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

            //hide save & cancel buttons
            saveButton.Hide();
            cancelButton.Hide();

            //show update button
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
            nextButton.Hide();
            prevButton.Hide();
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
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
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
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
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
            if (_animal != null)
            {
                AddImageToAnimalTextbox(_animal); //FIXME might be an issue for new animals 
            }
            Refresh();
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
                FlowDirection = FlowDirection.LeftToRight, // Arrange thumbnails horizontally
                BorderStyle = BorderStyle.FixedSingle // Add border for visual clarity
            };

            // Add "Add Image" button at the start of thumbnail panel
            Button addImageButton = new Button
            {
                Size = new Size(50, 50),
                Text = "+",
                Font = new Font("Segoe UI", 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(5),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat
            };
            addImageButton.Click += AddThumbnailImage;
            thumbnailPanel.Controls.Add(addImageButton);

            this.Controls.Add(thumbnailPanel);

            // Add existing thumbnails to the panel
            if (_animal != null && _animal.AdditionalImages != null)
            {
                foreach (var imagePath in _animal.AdditionalImages)
                {
                    AddThumbnailToPanel(imagePath);
                }
            }
        }

        // Handle adding a new thumbnail image
        private async void AddThumbnailImage(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Image Files (*.jpg, *.jpeg, *.png, *.gif, *.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp",
                Title = "Select an Image"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string selectedFilePath = dialog.FileName;
                    if (ValidateAndProcessImage(selectedFilePath))
                    {
                        string normalizedPath = CleanFilePath(selectedFilePath);
                        AddThumbnailToPanel(normalizedPath);

                        // Update animal's additional images in database
                        if (_animal != null)
                        {
                            if (_animal.AdditionalImages == null)
                                _animal.AdditionalImages = new List<string>();

                            _animal.AdditionalImages.Add(normalizedPath);
                            await _animalService.AddAdditionalAnimalImagesAsync(_animal.Id, _animal.AdditionalImages.ToArray());
                            Refresh(); //FIXME just trying this 
                        }
                        else
                        {
                            if (animalImageUrls == null)
                                animalImageUrls = new List<string>();
                            //add to local variable so that we still have the images 
                            animalImageUrls.Add(normalizedPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage("Error adding thumbnail image: " + ex.Message);
                }
            }
        }

        // Add a single thumbnail to the panel
        private void AddThumbnailToPanel(string imagePath)
        {
            try
            {
                if (File.Exists(imagePath))
                {
                    PictureBox thumbnail = new PictureBox
                    {
                        Image = Image.FromFile(imagePath),
                        Size = new Size(50, 50),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Margin = new Padding(5),
                        Cursor = Cursors.Hand
                    };

                    // Create context menu for thumbnail
                    ContextMenuStrip menu = new ContextMenuStrip();
                    menu.Items.Add("View", null, (s, e) => OnThumbnailClick(imagePath));
                    menu.Items.Add("Delete", null, (s, e) => DeleteThumbnail(thumbnail, imagePath));
                    thumbnail.ContextMenuStrip = menu;

                    // Add click handler
                    thumbnail.Click += (s, e) => OnThumbnailClick(imagePath);

                    // Add to panel after the add button
                    thumbnailPanel.Controls.Add(thumbnail);

                    //refresh?
                    Refresh();
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading thumbnail: " + ex.Message);
            }
        }

        // Handle the click event on a thumbnail
        private void OnThumbnailClick(string imagePath)
        {
            try
            {
                if (File.Exists(imagePath))
                {
                    // Change the main image box to the selected thumbnail's image
                    if (animalPhotoBox.Image != null)
                    {
                        animalPhotoBox.Image.Dispose();
                    }
                    animalPhotoBox.Image = Image.FromFile(imagePath);
                    _animal.imageUrl = imagePath; //reset the main image here
                }
                else
                {
                    ShowMessage("Image file not found: " + imagePath);
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error displaying image: " + ex.Message);
            }
        }

        // Delete a thumbnail
        private async void DeleteThumbnail(PictureBox thumbnail, string imagePath)
        {
            if (MessageBox.Show("Are you sure you want to remove this image?", "Confirm Delete",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    // Remove from animal's additional images
                    if (_animal != null && _animal.AdditionalImages != null)
                    {
                        if(thumbnail != null)
                        {
                            thumbnail.Dispose();
                            animalPhotoBox.Image.Dispose();
                            _animal.AdditionalImages.Remove(imagePath);
                            if(_animal.AdditionalImages.Count > 0)
                            {
                                // Assuming _animal.AdditionalImages is a collection of image URLs or paths
                                string firstImagePath = _animal.AdditionalImages.First();
                                animalPhotoBox.Image = Image.FromFile(firstImagePath);
                                _animal.imageUrl = firstImagePath;

                            }
                            else
                            {
                                _animal.AdditionalImages = null;
                                animalPhotoBox.Image = null;
                                _animal.imageUrl = null;
                            }
                            
                        }

                       
                        await _animalService.RemoveAdditionalAnimalImageAsync(_animal.Id, imagePath);

                        _animal = await _animalService.GetAnimalByIdAsync(_animal.Id);
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage("Error removing thumbnail: " + ex.Message);
                }
            }
        }

        // Validate and process an image file
        private bool ValidateAndProcessImage(string filePath)
        {
            if (!File.Exists(filePath))
            {
                ShowMessage("Error: The selected file does not exist.");
                return false;
            }

            string[] validImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            string fileExtension = Path.GetExtension(filePath).ToLower();

            if (!validImageExtensions.Contains(fileExtension))
            {
                ShowMessage("Error: The selected file is not a valid image. Please choose a valid image file.");
                return false;
            }

            try
            {
                // Verify the file is a valid image by attempting to load it
                using (Image img = Image.FromFile(filePath)) { }
                return true;
            }
            catch (Exception)
            {
                ShowMessage("Error: The selected file is not a valid image.");
                return false;
            }
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
            // Make sure that we're in edit mode. If we aren't, we shouldn't be here anyways, just in case, check.
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

        private async void SetComboBoxes(string species)
        {
            if (species == "Rat")
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

                        // Subscribe to the SelectedIndexChanged event
                        comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
                    }
                }
            }
            else //for now assume mouse 
            {
                // Iterate through all controls on the form
                foreach (Control control in this.Controls)
                {
                    if (control is ComboBox comboBox)
                    {
                        //get the list of values from the db 
                        List<string> items = await GetComboBoxItemsFromDatabase_Mouse(control); // Add database items

                        // Add "Create New" as an option TODO need to set Create New as a button 
                        comboBox.Items.Clear(); // Clear existing items (optional, based on your use case)
                        comboBox.Items.AddRange(items.ToArray()); // Add database items

                        // Subscribe to the SelectedIndexChanged event
                        comboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
                    }
                }
            }
            
        }

        // Event handler for "Create New" functionality
        private async void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //create new option
            if (sender is ComboBox comboBox)
            {
                string name = comboBox.Name; 

                if(name == "speciesComboBox")
                {
                    SetComboBoxes(comboBox.Text);
                }

                //if (speciesComboBox.SelectedIndex < 0) //no species means we don't what phenotypes and dams/sires to choose
                //{
                //    MessageBox.Show("Please enter a species");
                //}
                //if dam get the selected dam object and store to global dam variable FIXME I need to implement data binding here
                if (comboBox.Name == "damComboBox")
                {
                    //get the animal object from the list of animal objects stored earlier 
                    foreach (var animal in _dams)
                    {
                        //compare the name? I guess this is the easiest approach for now but is problematic if there are duplicate names  
                        if (comboBox.SelectedItem?.ToString() == animal.name)
                        {
                            _dam = animal;
                        }
                    }
                }
                //if sire, get the selected sire object and store to global sire variable 
                else if (comboBox.Name == "sireComboBox")
                {
                    //get the animal object from the list of animal objects stored earlier 
                    foreach (var animal in _sires)
                    {
                        //compare the name? I guess this is the easiest approach for now but is problematic if there are duplicate names  
                        if (comboBox.SelectedItem?.ToString() == animal.name)
                        {
                            _sire = animal;
                        }
                    }
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
            Console.WriteLine($"New item added to the database logic TODO: {newItem}");
        }

        private async void LoadAnimalDataAsync(int animalId)
        {
            try
            {
                //await _loadingSemaphore.WaitAsync();
                ShowLoadingIndicator();

                var animal = await _animalService.GetAnimalByIdAsync(animalId);
                _animal = animal;

                await SetDefaultDamAndSire();
                //_loadingSemaphore.Release();
                await GetAnimalDamAndSire();
                InitializeTextBoxes(_animal);

            }
            catch (Exception ex)
            {
                LogError(ex);
                ShowMessage("Error loading animal data. Please try again.");
            }
            finally
            {
                HideLoadingIndicator();
               
            }
        }

        private async Task SetDefaultDamAndSire()
        {
            var defaultAnimal = await GetOrCreateDefaultAnimal(0);
            _dam = defaultAnimal;
            _sire = defaultAnimal;
        }

        private async Task GetAnimalDamAndSire()
        {
            if (_animal != null)
            {
                try
                {
                    //await _loadingSemaphore.WaitAsync();
                    var dam = await _lineageService.GetDamByAnimalId(_animal.Id);
                    if (dam != null)
                    {
                        _dam = await _animalService.MapSingleAnimaltoDto(dam);
                    }

                    var sire = await _lineageService.GetSireByAnimalId(_animal.Id);
                    if (sire != null)
                    {
                        _sire = await _animalService.MapSingleAnimaltoDto(sire);
                    }
                }
                finally
                {
                    //_loadingSemaphore.Release();
                }
            }
        }

        private async Task<AnimalDto> GetOrCreateDefaultAnimal(int? id)
        {
            if (id.HasValue && id.Value != 0)
            {
                try
                {
                    //await _loadingSemaphore.WaitAsync();
                    var existingAnimal = await _animalService.GetAnimalByIdAsync(id.Value);
                    if (existingAnimal != null)
                    {
                        return existingAnimal;
                    }
                }
                finally
                {
                    //_loadingSemaphore.Release();
                }
            }

            return new AnimalDto
            {
                Id = 0,
                regNum = "0",
                name = "Unknown",
                sex = "Unknown",
                DateOfBirth = DateTime.Now,
                DateOfDeath = DateTime.Now,
                species = "Unknown",
                comment = "Unknown",
                imageUrl = "",
                Line = "1",
                damId = null,
                sireId = null,
                variety = "Unknown",
                color = "Unknown",
                breeder = "TLDR",
                genotype = "Xx/Yy/Zz"
            };
        }

        private async Task<List<string>> GetComboBoxItemsFromDatabase(Control control)
        {
            try
            {
                //await _loadingSemaphore.WaitAsync();
                var options = new List<string>();
                //string animalSpecies = GetAnimalSpecies();
                string animalSpecies = "Rat"; 

                switch (control.Name)
                {
                    case "speciesComboBox":
                        var species = await _speciesService.GetAllSpeciesAsync();
                        //bind this data to the species combo box items
                        options.AddRange((List<string>)species);
                        break;

                    case "sexComboBox":
                        options.AddRange(new[] { "Male", "Female", "Unknown", "Other" });
                        break;

                    case "varietyComboBox":
                        //if (speciesComboBox.SelectedIndex < 0)
                        //{
                        //    break;
                        //}
                        var animalCoats = await GetSpeciesTraitsByType(animalSpecies, "Coat Type");
                        options.AddRange(animalCoats);
                        break;

                    case "colorComboBox":
                        //if (speciesComboBox.SelectedIndex < 0)
                        //{
                        //    break;
                        //}
                        var animalColors = await GetSpeciesTraitsByType(animalSpecies, "Color");
                        options.AddRange(animalColors);
                        break;

                    case "damComboBox":
                        //if (speciesComboBox.SelectedIndex < 0)
                        //{
                        //    break;
                        //}
                        if (animalSpecies != "Unknown")
                        {
                            _dams = await _animalService.GetAnimalInfoBySexAndSpecies("Female", animalSpecies);
                        }
                        else
                        {
                            _dams = await _animalService.GetAnimalsBySex("Female");
                        }
                        foreach (var dam in _dams)
                        {
                            options.Add(dam.name);
                        }
                        break;

                    case "sireComboBox":
                        //if (speciesComboBox.SelectedIndex < 0)
                        //{
                        //    break;
                        //}
                        if (animalSpecies != "Unknown")
                        {
                            _sires = await _animalService.GetAnimalInfoBySexAndSpecies("Male", animalSpecies);
                        }
                        else
                        {
                            _sires = await _animalService.GetAnimalsBySex("Male");
                        }
                        foreach (var sire in _sires)
                        {
                            options.Add(sire.name);
                        }
                        break;

                    case "earTypeComboBox":
                        //if (speciesComboBox.SelectedIndex < 0)
                        //{
                        //    break;
                        //}
                        var animalEars = await GetSpeciesTraitsByType(animalSpecies, "Ear Type");
                        options.AddRange(animalEars);
                        break;

                    case "markingComboBox":
                        //if (speciesComboBox.SelectedIndex < 0)
                        //{
                        //    break;
                        //}
                        var animalMarkings = await GetSpeciesTraitsByType(animalSpecies, "Marking");
                        options.AddRange(animalMarkings);
                        break;

                    default:
                        options.AddRange(new[] { "Option1", "Option2", "Option3" });
                        break;
                }

                return options;
            }
            finally
            {
                //_loadingSemaphore.Release();
            }
        }

        private async Task<List<string>> GetComboBoxItemsFromDatabase_Mouse(Control control)
        {
            try
            {
                //await _loadingSemaphore.WaitAsync();
                var options = new List<string>();
                //string animalSpecies = GetAnimalSpecies();
                string animalSpecies = "Mouse";

                switch (control.Name)
                {
                    case "speciesComboBox":
                        var species = await _speciesService.GetAllSpeciesAsync();
                        //bind this data to the species combo box items
                        options.AddRange((List<string>)species);
                        break;

                    case "sexComboBox":
                        options.AddRange(new[] { "Male", "Female", "Unknown", "Other" });
                        break;

                    case "varietyComboBox":
                        //if (speciesComboBox.SelectedIndex < 0)
                        //{
                        //    break;
                        //}
                        var animalCoats = await GetSpeciesTraitsByType(animalSpecies, "Coat Type");
                        options.AddRange(animalCoats);
                        break;

                    case "colorComboBox":
                        //if (speciesComboBox.SelectedIndex < 0)
                        //{
                        //    break;
                        //}
                        var animalColors = await GetSpeciesTraitsByType(animalSpecies, "Color");
                        options.AddRange(animalColors);
                        break;

                    case "damComboBox":
                        //if (speciesComboBox.SelectedIndex < 0)
                        //{
                        //    break;
                        //}
                        if (animalSpecies != "Unknown")
                        {
                            _dams = await _animalService.GetAnimalInfoBySexAndSpecies("Female", animalSpecies);
                        }
                        else
                        {
                            _dams = await _animalService.GetAnimalsBySex("Female");
                        }
                        foreach (var dam in _dams)
                        {
                            options.Add(dam.name);
                        }
                        break;

                    case "sireComboBox":
                        //if (speciesComboBox.SelectedIndex < 0)
                        //{
                        //    break;
                        //}
                        if (animalSpecies != "Unknown")
                        {
                            _sires = await _animalService.GetAnimalInfoBySexAndSpecies("Male", animalSpecies);
                        }
                        else
                        {
                            _sires = await _animalService.GetAnimalsBySex("Male");
                        }
                        foreach (var sire in _sires)
                        {
                            options.Add(sire.name);
                        }
                        break;

                    case "earTypeComboBox":
                        //if (speciesComboBox.SelectedIndex < 0)
                        //{
                        //    break;
                        //}
                        var animalEars = await GetSpeciesTraitsByType(animalSpecies, "Ear Type");
                        options.AddRange(animalEars);
                        break;

                    case "markingComboBox":
                        //if (speciesComboBox.SelectedIndex < 0)
                        //{
                        //    break;
                        //}
                        var animalMarkings = await GetSpeciesTraitsByType(animalSpecies, "Marking");
                        options.AddRange(animalMarkings);
                        break;

                    default:
                        options.AddRange(new[] { "Option1", "Option2", "Option3" });
                        break;
                }

                return options;
            }
            finally
            {
                //_loadingSemaphore.Release();
            }
        }

        //find species
        //FIXME - have to hack together some logic to make this work, but will refactor after IdeaFest 
        //private string GetAnimalSpecies()
        //{
        //    //first, check if _animal is populated
        //    if (_animal != null)
        //    {
        //        return _animal.species;
        //    }
        //    else if (speciesComboBox.SelectedItem != null && speciesComboBox.Text != "Add New") //FIXME make sure this is the correct text
        //    {
        //        return speciesComboBox.SelectedItem.ToString(); //or .Text? FIXME 
        //    }
        //    else
        //    {
        //        //return unknown, use to tell the user that they need to fill out the species field first
        //        return "Unknown";
        //        //TODO handle case where species is unknown, this should be a pop up box that says "Please fill out the species field first"  MessageBox.Show("Please fill out the species field first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

        private void IntializeButtons()
        {
            InbredButton();
            UpdateButton();
            SaveButton();
            CancelButton();
            //InitializeNavigationButtons(); //initialize the navigation buttons FIXME working on formatting 

            // Add the button to the panel
            this.Controls.Add(inbredButton);
            this.Controls.Add(updateButton);
            this.Controls.Add(saveButton);
            this.Controls.Add(cancelButton);
            //this.Controls.Add(nextButton);
            //this.Controls.Add(prevButton);
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
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };
            inbredButton.Click += async (sender, e) =>
            {
                await GetInbredCoEfficient();
                MessageBox.Show("Inbreeding Coefficient Updated"); 
            };
        }

        private async Task GetInbredCoEfficient()
        {
            if(_animal != null)
            { 
                // Logic to calculate inbreeding coefficient
                double inbredCo = await _geneticsService.CalculateInbreedingCoefficientAsync(_dam.Id, _sire.Id);
                inbredTextBox.Text = inbredCo.ToString();
            }
            else
            {
                inbredTextBox.Text = "No animal found";
            }
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
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };
            saveButton.Click += async (sender, e) => //NOTE: putting this inside of an async lambda is what allows us to use await without setting the entire method to async! 
            {
                await SaveButtonClick(sender, e);
            };
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
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
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
                Location = new Point(170, 630),
                Width = 150,
                Height = 40,
                Text = "Cancel",
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 1 },
                Cursor = Cursors.Hand
            };
            //this should be a call to the library to calculate the % inbred
            cancelButton.Click += async (sender, e) =>
            {
                await CancelButtonClick(sender, e);
            };
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
            //bool success = int.TryParse(regNumTextBox.Text, out id);
            bool success = true; 
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

        //parse textbox into animal dto object
        private async Task <AnimalDto> ParseAnimalData()
        {
            //FIXME just for now 
            DateTime dob = DateTime.Now;

            //FIXME breeder logic is in the works
            //there needs to be some logic to find the breeder in the db and pass that id to the animal object
            // for now just getting this to work
            string breeder = "0"; //this is the user's breeder id there is currently a bug that isn't storing the user as a breeder that needs to be fixed before this will actually work

            //FIXME this is just for testing right now need a way to manage lines i.e. "add new" or "select from list" 
            string line = "3";
            if (speciesComboBox.Text == "Mouse" || speciesComboBox.Text == "mouse")
            {
                line = "2";
            }
           
            if (_animal != null)
            {
                 _animal.genotype = await _animalService.GetGenotypesAsStringAsync(_animal.Id);
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
                    imageUrl = _animal?.AdditionalImages?.FirstOrDefault(), //FIXME this is being set inside of the click image box method so I think this should be fine like this TODO just hardcoded right now 
                    Line = line, // Assuming there's a TextBox for line
                    damId = _dam != null ? _dam.Id : (int?)null,//damComboBox.Text, // Assuming there's a TextBox for dam
                    sireId = _sire != null ? _sire.Id : (int?)null,//sireComboBox.Text, // Assuming there's a TextBox for sire
                    variety = varietyComboBox.Text, // Assuming there's a TextBox for variety
                    color = colorComboBox.Text, // Assuming there's a TextBox for color TODO
                    markings = markingComboBox.Text,
                    earType = earTypeComboBox.Text,
                    breeder = "TLDR",//breeder, // Assuming there's a TextBox for breeder TODO this should take in a text name of the breeder, it should be converted to a breeder id in the backend and then that should be used to seach the database or create a new breeder if not found 
                    genotype = _animal.genotype, // Assuming there's a TextBox for genotype TODO FIXME this may not work correctly, we'll see 
                    AdditionalImages = _animal?.AdditionalImages,
                };
                return animal;
            }
            else
            {
                AnimalDto animal = new AnimalDto
                {
                    //id will have to be created for a new animal 
                    regNum = regNumTextBox.Text,
                    name = animalNameTextBox.Text,
                    sex = sexComboBox.Text, // Assuming there's a TextBox for sex
                    DateOfBirth = dob,  //dobTextBox.Text : DateTime.Parse(dobTextBox.Text), // Assuming dobTextBox contains the Date of Birth FIXME 
                    DateOfDeath = DateTime.Now,//string.IsNullOrWhiteSpace(dodTextBox.Text) //FIXME getting an error on NULL value for DateOfDeath should be allowed 
                                               //? null
                                               //: DateTime.Parse(dodTextBox.Text), // Assuming dodTextBox contains the Date of Death
                    species = speciesComboBox.Text,
                    comment = commentsTextBox.Text,
                    imageUrl = animalImageUrls.First(),//"C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\RATAPPLogo.png", //FIXME this is being set inside of the click image box method so I think this should be fine like this TODO just hardcoded right now 
                    Line = line, // Assuming there's a TextBox for line
                    damId = _dam != null ? _dam.Id : (int?)null,//damComboBox.Text, // Assuming there's a TextBox for dam
                    sireId = _sire != null ? _sire.Id : (int?)null,//sireComboBox.Text, // Assuming there's a TextBox for sire
                    variety = varietyComboBox.Text, // Assuming there's a TextBox for variety
                    color = colorComboBox.Text, // Assuming there's a TextBox for color TODO
                    markings = markingComboBox.Text,
                    earType = earTypeComboBox.Text,
                    breeder = "TLDR",//breeder, // Assuming there's a TextBox for breeder TODO this should take in a text name of the breeder, it should be converted to a breeder id in the backend and then that should be used to seach the database or create a new breeder if not found 
                    genotype = "XYZ",//genotypeTextBox.Text, // Assuming there's a TextBox for genotype TODO this should come from the phenotypes entered for now 
                    AdditionalImages = animalImageUrls,
                };
                return animal;
            }

        }

        //save and update animal logic below 
        /// <summary>
        /// Handles saving animal data to the database.
        /// 
        /// Process:
        /// 1. Validates form data
        /// 2. Creates/updates animal record
        /// 3. Handles success/failure scenarios
        /// 
        /// Validation:
        /// - Required fields
        /// - Data format
        /// - Unique constraints
        /// 
        /// Error Handling:
        /// - Shows user-friendly messages
        /// - Logs technical errors
        /// - Maintains data integrity
        /// 
        /// State Updates:
        /// - Refreshes UI after save
        /// - Updates edit mode
        /// - Reloads data if needed
        /// </summary>
        private async Task SaveButtonClick(object sender, EventArgs e)
        {
            AnimalDto animalDto = await ParseAnimalData(); //first, parse the data from the text boxes this doesn't work now because the animal doesn't exist yet, so the animal needs to exist first

            
            if (animalDto == null) //if the data is invalid, show an error message and return
            {
                ShowMessage("Error: Animal data is invalid. Please check required fields.");
                return;
            }

            try
            {
                Animal animal = await _animalService.CreateAnimalFullProcess(animalDto); //start by creating the animal not using the method I created as it returns an error TODO on this 
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

        private async Task<List<string>> GetSpeciesTraitsByType(string species, string type)
        {
            try
            {
                //await _loadingSemaphore.WaitAsync();
                return (List<string>)await _traitService.GetTraitsByTypeAndSpeciesAsync(type, species);
            }
            catch (Exception ex)
            {
                LogError(ex);
                ShowMessage("Failed to get traits for the species. Please try again.");
                return new List<string>();
            }
            finally
            {
                //_loadingSemaphore.Release();
            }
        }

        //Button UI logic below 
        //TODO need to better organize and work on set navigation for this panel  
        // Initialize the row of buttons at the bottom
        private void InitializeBottomButtons()
        {
            // Create a panel for the feature buttons
            Panel featureButtonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(230, 230, 230)
            };

            // Button 2: Ancestry
            indAncestryButton = CreateFeatureButton("Ancestry");
            indAncestryButton.Click += (sender, e) =>
            {
                var ancestryForm = new IndividualAnimalAncestryForm(_parentForm, _contextFactory, _animal);
                ancestryForm.Show();
            };

            // Button 3: Genetics
            geneticsButton = CreateFeatureButton("Genetics");

            // Button 4: Breeding History
            breedingHistoryButton = CreateFeatureButton("Breeding History");

            // Button 5: Documents
            documentsButton = CreateFeatureButton("Documents");
            documentsButton.Click += (sender, e) =>
            {
                var documentForm = new DocumentsForm(_parentForm, _contextFactory);
                documentForm.Show();
            };

            // Button 6: Health
            healthButton = CreateFeatureButton("Health");
            healthButton.Click += (sender, e) =>
            {
                MessageBox.Show("Health Record Form Will Show Here"); 
                //TODO
                //var healthForm = new HealthRecordForm(_parentForm, _contextFactory, _animal);
                //healthForm.Show();
            };

            //navigation buttons
            // Remove any existing buttons with the same name
            var buttonsToRemove = this.Controls.OfType<Button>()
                .Where(b => b.Name == "Previous" || b.Name == "Next").ToList();
            foreach (var button in buttonsToRemove)
            {
                this.Controls.Remove(button);
            }

            // Create and re-add buttons
            prevButton = CreateFeatureButton("Previous");
            prevButton.BackColor = Color.FromArgb(0, 120, 212);
            prevButton.ForeColor = Color.White;
            prevButton.Click += PreviousButtonClick;

            nextButton = CreateFeatureButton("Next");
            nextButton.BackColor = Color.FromArgb(0, 120, 212);
            nextButton.ForeColor = Color.White;
            nextButton.Click += NextButtonClick;

            //this.Controls.Add(prevButton);
            //this.Controls.Add(nextButton);

            // Add buttons to the panel with proper spacing
            int buttonX = 10;
            foreach (Button btn in new[] { indAncestryButton, geneticsButton, breedingHistoryButton, documentsButton, healthButton, prevButton, nextButton })
            {
                btn.Location = new Point(buttonX, 10);
                featureButtonPanel.Controls.Add(btn);
                buttonX += btn.Width + 10;
            }
            this.Controls.Add(featureButtonPanel);
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
            genotypeLabel = CreateLabel("Genotype", 380, 60, labelFont);
            markingsLabel = CreateLabel("Markings", 380, 100, labelFont);
            //breederInfoLabel = CreateLabel("Breeder", 380, 140, labelFont); TODO
            dobLabel = CreateLabel("DOB", 380, 140, labelFont);
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
            this.Controls.Add(dobLabel);
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
        //TODO I need to go through and get rid of most of my async voids and turn them into async Tasks and handle them with lambdas 
        private async void InitializeTextBoxes(AnimalDto animal)
        {
            // First column (left side)
            regNumTextBox = CreateTextBox(150, 20, _animal.regNum);
            animalNameTextBox = CreateTextBox(150, 60, _animal.name);
            speciesTextBox = CreateTextBox(150, 100, _animal.species);
            sexTextBox = CreateTextBox(150, 140, _animal.sex);
            varietyTextBox = CreateTextBox(150, 180, _animal.variety);
            damTextBox = CreateTextBox(150, 220, _dam.name);

            // Second column (right side)
            colorTextBox = CreateTextBox(490, 20, _animal.color);
            genotypeTextBox = CreateTextBox(490, 60, _animal.genotype); //TODO will likely be removing this value eventually and using the appropriate model 
            markingsTextBox = CreateTextBox(490, 100, _animal.markings);
            //breederInfoTextBox = CreateTextBox(490, 140, _animal.breeder);
            dobTextBox = CreateTextBox(490, 140, _animal.DateOfBirth.Date.ToString("yyyy-MM-dd"));
            earTypeTextBox = CreateTextBox(490, 180, _animal.earType);
            sireTextBox = CreateTextBox(490, 220, _sire.name);

            inbredTextBox = CreateTextBox(150, 450, "0"); //start with zero, set actual value below FIXME weird logic but works for now 
            //set the % inbred, if any 
            await GetInbredCoEfficient();

            genotypeTextBox.ReadOnly = true;

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
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
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
            //this.Controls.Add(breederInfoTextBox);

            this.Controls.Add(dobTextBox);
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

        private Button CreateFeatureButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = 140,
                Height = 40,
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.FromArgb(250, 250, 250),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderColor = Color.FromArgb(200, 200, 200) },
                Cursor = Cursors.Hand
            };
        }



        private void ShowLoadingIndicator()
        {
            if (loadingSpinner != null)
            {
                loadingSpinner.Visible = true;
                CenterLoadingSpinner();
                Refresh();
            }
        }

        private void HideLoadingIndicator()
        {
            if (loadingSpinner != null)
            {
                loadingSpinner.Visible = false;
                Refresh();
            }
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

        private void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        private void LogError(Exception ex)
        {
            // TODO: Implement proper logging
            Console.WriteLine(ex.Message);
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
    }
}
