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

        private PictureBox animalPhotoBox;

        // Constructor for initializing the panel
        public AnimalPanel(RATAppBaseForm parentForm, string animalName, string animalID)
        {
            InitializeComponent(animalName, animalID);
        }

        private void InitializeComponent(string animalName, string animalID)
        {
            // Set panel properties
            this.Size = new Size(1024, 768);
            this.BackColor = Color.White;

            // Initialize controls
            InitializeLabels();
            InitializeTextBoxes();
            InitializePhotoBox();
            InitializeNavigationButtons();
            InitializeBottomButtons();
        }

        // Initialize labels take 1
        // TODO - add functionality to populate labels with data
        //still need to work on UI but basic idea is there and ready for data binding
        private void InitializeLabels()
        {
            // First column (left side)
            idLabel = CreateLabel("Registration #/ID", 10, 10);
            animalNameLabel = CreateLabel("Animal Name", 10, 40);
            speciesLabel = CreateLabel("Species", 10, 70);
            sexLabel = CreateLabel("Sex", 10, 100);
            varietyLabel = CreateLabel("Variety", 10, 130);
            damLabel = CreateLabel("Dam", 10, 160);

            // Second column (right side)
            colorLabel = CreateLabel("Color", 390, 10);
            geneticsLabel = CreateLabel("Genetics", 390, 40);
            ancestryLabel = CreateLabel("Ancestry", 390, 70);
            breederInfoLabel = CreateLabel("Breeder", 390, 100);
            sireLabel = CreateLabel("Sire", 390, 160);

            // Move commentsLabel to a new position below everything else
            commentsLabel = CreateLabel("Comments", 20, 190);

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
        }

        // Method to create labels with dynamic positions
        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Arial", 10F)
            };
        }

        // initialize textboxes take 1 
        // TODO - add functionality to populate textboxes with data
        //still need to work on UI but basic idea is there and ready for data binding
        private void InitializeTextBoxes()
        {
            // First column (left side)
            idTextBox = CreateTextBox(150, 10);
            animalNameTextBox = CreateTextBox(150, 40);
            speciesTextBox = CreateTextBox(150, 70);
            sexTextBox = CreateTextBox(150, 100);
            varietyTextBox = CreateTextBox(150, 130);
            damTextBox = CreateTextBox(150, 160);

            // Second column (right side)
            colorTextBox = CreateTextBox(490, 10);
            geneticsTextBox = CreateTextBox(490, 40);
            ancestryTextBox = CreateTextBox(490, 70);
            breederInfoTextBox = CreateTextBox(490, 100);
            sireTextBox = CreateTextBox(490, 160);

            // Move commentsTextBox below everything else and make it larger
            commentsTextBox = new TextBox
            {
                Location = new Point(10, 210),  // Position it below the other fields
                Width = 200,
                Height = 100,                   // Increase the height for a larger textbox
                Multiline = true,                // Make it multiline for longer comments
                ScrollBars = ScrollBars.Vertical // Enable vertical scroll bars if needed
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
        }

        // Method to create textboxes
        private TextBox CreateTextBox(int x, int y)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Width = 200
            };
        }

        // Method to initialize the animal's photo box
        //TODO setup to actually contain a photo 
        private void InitializePhotoBox()
        {
            animalPhotoBox = new PictureBox
            {
                Location = new Point(750, 10), // Positioned right of the text boxes
                Size = new Size(200, 200),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Add picture box to the form or panel
            this.Controls.Add(animalPhotoBox);
        }

        // Method to initialize the navigation buttons (Next and Previous)
        private void InitializeNavigationButtons()
        {
            // Previous button
            Button previousButton = new Button
            {
                Text = "Previous",
                Location = new Point(200, 700), // Positioned towards the left at the bottom
                Width = 100
            };
            this.Controls.Add(previousButton);

            // Next button
            Button nextButton = new Button
            {
                Text = "Next",
                Location = new Point(600, 700), // Positioned towards the right at the bottom
                Width = 100
            };
            this.Controls.Add(nextButton);
        }

        // initialize the row of buttons at the bottom
        private void InitializeBottomButtons()
        {
            // Button 1: Update Animal Details
            Button updateButton = new Button
            {
                Text = "Update Animal Details",
                Location = new Point(50, 630), // Positioned at the left
                Width = 160
            };

            // Button 2: Ancestry
            Button ancestryButton = new Button
            {
                Text = "Ancestry",
                Location = new Point(220, 630), // Positioned next to the first button
                Width = 120
            };

            // Button 3: Genetics
            Button geneticsButton = new Button
            {
                Text = "Genetics",
                Location = new Point(360, 630), // Positioned next to the second button
                Width = 120
            };

            // Button 4: Breeding History
            Button breedingHistoryButton = new Button
            {
                Text = "Breeding History",
                Location = new Point(500, 630), // Positioned next to the third button
                Width = 140
            };

            // Button 5: Documents
            Button documentsButton = new Button
            {
                Text = "Documents",
                Location = new Point(650, 630), // Positioned next to the fourth button
                Width = 120
            };

            // Button 6: Health
            Button healthButton = new Button
            {
                Text = "Health",
                Location = new Point(790, 630), // Positioned next to the fifth button
                Width = 120
            };

            // Add all buttons to the form or panel
            this.Controls.Add(updateButton);
            this.Controls.Add(ancestryButton);
            this.Controls.Add(geneticsButton);
            this.Controls.Add(breedingHistoryButton);
            this.Controls.Add(documentsButton);
            this.Controls.Add(healthButton);
        }
    }
}
