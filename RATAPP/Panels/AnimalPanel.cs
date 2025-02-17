using RATAPP.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RATAPP.Panels
{
    public partial class AnimalPanel : Panel
    {
        // Class-level fields for controls specific to Animal Page
        private Label animalNameLabel;
        private Label idLabel;
        private Label speciesLabel;
        private Label sexLabel;
        private Label varietyLabel;
        private Label colorLabel;
        private Label geneticsLabel;
        private Label ancestryLabel;
        private Label breederInfoLabel;
        private Label commentsLabel;
        private Label damLabel;
        private Label sireLabel;
        private Label inbredLabel;
        private Label genomeLabel;

        private TextBox animalNameTextBox;
        private TextBox idTextBox;
        private TextBox speciesTextBox;
        private TextBox sexTextBox;
        private TextBox varietyTextBox;
        private TextBox colorTextBox;
        private TextBox geneticsTextBox;
        private TextBox ancestryTextBox;
        private TextBox breederInfoTextBox;
        private TextBox commentsTextBox;
        private TextBox damTextBox;
        private TextBox sireTextBox;
        private TextBox inbredTextBox;
        private TextBox genomeTextBox;

        private PictureBox animalPhotoBox;
        private Button inbredButton;
        private Button saveButton;
        private Button updateButton;

        //TODO
        //need to add logic to get the values from the database


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

        // Constructor for initializing the panel
        public AnimalPanel(RATAppBaseForm parentForm, string animalName, string animalID)
        {
            InitializeComponent(animalName, animalID);
        }

        private void InitializeComponent(string animalName, string animalID)
        {
            // Set panel properties
            this.Size = new Size(1200, 800); // Increased panel size for better spacing
            this.BackColor = Color.White;
            this.Padding = new Padding(20); // Add padding around the panel for better spacing

            //Initialize the controls for the Animal Panel, buttons first as others are dependent on them (this is probably a bad idea, but works for now) 
            IntializeButtons();
            InitializeBottomButtons();
            InitializeTextBoxes(animalID, animalName);
           
            InitializeLabels();
            InitializePhotoBox();
            InitializeNavigationButtons();
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
            geneticsLabel = CreateLabel("Genetics", 380, 60, labelFont);
            ancestryLabel = CreateLabel("Ancestry", 380, 100, labelFont);
            breederInfoLabel = CreateLabel("Breeder", 380, 140, labelFont);
            genomeLabel = CreateLabel("Genome", 380, 180, labelFont);
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
            this.Controls.Add(geneticsLabel);
            this.Controls.Add(ancestryLabel);
            this.Controls.Add(breederInfoLabel);
            this.Controls.Add(commentsLabel);
            this.Controls.Add(damLabel);
            this.Controls.Add(sireLabel);
            this.Controls.Add(genomeLabel);
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
        private void InitializeTextBoxes(string id, string name)
        {
            string TODO = "TODO - should come from db"; //FIXME
            // First column (left side)
            idTextBox = CreateTextBox(150, 20, id);
            animalNameTextBox = CreateTextBox(150, 60, name);
            speciesTextBox = CreateTextBox(150, 100, TODO);
            sexTextBox = CreateTextBox(150, 140, TODO);
            varietyTextBox = CreateTextBox(150, 180, TODO);
            damTextBox = CreateTextBox(150, 220, TODO);

            // Second column (right side)
            colorTextBox = CreateTextBox(490, 20, TODO);
            geneticsTextBox = CreateTextBox(490, 60, TODO);
            ancestryTextBox = CreateTextBox(490, 100, TODO);
            breederInfoTextBox = CreateTextBox(490, 140, TODO);
            genomeTextBox = CreateTextBox(490, 180, TODO);
            sireTextBox = CreateTextBox(490, 220, TODO);

            inbredTextBox = CreateTextBox(150, 450, "");
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


            // Add textboxes to panel
            this.Controls.Add(animalNameTextBox);
            this.Controls.Add(idTextBox);
            this.Controls.Add(speciesTextBox);
            this.Controls.Add(sexTextBox);
            this.Controls.Add(varietyTextBox);
            this.Controls.Add(colorTextBox);
            this.Controls.Add(geneticsTextBox);
            this.Controls.Add(ancestryTextBox);
            this.Controls.Add(breederInfoTextBox);
            this.Controls.Add(commentsTextBox);
            this.Controls.Add(damTextBox);
            this.Controls.Add(sireTextBox);
            this.Controls.Add(genomeTextBox);
            this.Controls.Add(inbredTextBox);

            //if ID is present than it is an existing animal
            //show existing animal versions = no editing, Update Data button, animal's associated data
            if (id != "")
            {
                AnimalExists();
            }
            //if ID is not present than the user is creating a new animal
            // show new animal version = editing, save button, blank boxes 
            else
            {
                NewAnimal();
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
            updateButton.Show();

            //display animals data TODO
        }

        //Method for new animals
        private void NewAnimal()
        {
            //enable textboxes
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
            previousButton.Click += (sender, e) => {/* Logic for Previous button */};

            // Next button
            Button nextButton = CreateButton("Next", 600, 700);
            nextButton.Click += (sender, e) => {/* Logic for Next button */};

            this.Controls.Add(previousButton);
            this.Controls.Add(nextButton);
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
            //create calc % inbred button 
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
            //this should be a call to the library to calculate the % inbred
            saveButton.Click += (sender, e) =>
            {
                MessageBox.Show("Changes Saved - Not really, just testing");
                //inbredTextBox.Text = TODO;
            };
        }

        private void SaveButtonClick(object sender, EventArgs e)
        {
            //save the data to the database
            //TODO method in library to save animal data

            //enable the update button
            saveButton.Hide();
            updateButton.Show(); 
         
            //display the animals data (refresh the page) TODO what about when we go back to home page, how do we refresh the data?
            //TODO method to refresh (in library?) 
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
                MessageBox.Show("Panel should switch to editable");
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

    }
}