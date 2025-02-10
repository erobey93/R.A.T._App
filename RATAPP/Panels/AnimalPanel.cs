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
            this.Size = new Size(1200, 800); // Increased panel size for better spacing
            this.BackColor = Color.White;
            this.Padding = new Padding(20); // Add padding around the panel for better spacing

            // Initialize controls
            InitializeLabels();
            InitializeTextBoxes();
            InitializePhotoBox();
            InitializeNavigationButtons();
            InitializeBottomButtons();
        }

        // Initialize labels take 1
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
            sireLabel = CreateLabel("Sire", 380, 220, labelFont);

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

        // initialize textboxes
        private void InitializeTextBoxes()
        {
            // First column (left side)
            idTextBox = CreateTextBox(150, 20);
            animalNameTextBox = CreateTextBox(150, 60);
            speciesTextBox = CreateTextBox(150, 100);
            sexTextBox = CreateTextBox(150, 140);
            varietyTextBox = CreateTextBox(150, 180);
            damTextBox = CreateTextBox(150, 220);

            // Second column (right side)
            colorTextBox = CreateTextBox(490, 20);
            geneticsTextBox = CreateTextBox(490, 60);
            ancestryTextBox = CreateTextBox(490, 100);
            breederInfoTextBox = CreateTextBox(490, 140);
            sireTextBox = CreateTextBox(490, 220);

            // Move commentsTextBox below everything else and make it larger
            commentsTextBox = new TextBox
            {
                Location = new Point(10, 290),
                Width = 680,
                Height = 120,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10F) // Make it consistent with labels
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
                Width = 200,
                Font = new Font("Segoe UI", 10F)
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

        // Initialize the row of buttons at the bottom
        private void InitializeBottomButtons()
        {
            // Button 1: Update Animal Details
            Button updateButton = CreateButton("Update Animal Details", 50, 630);

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

            this.Controls.Add(updateButton);
            this.Controls.Add(ancestryButton);
            this.Controls.Add(geneticsButton);
            this.Controls.Add(breedingHistoryButton);
            this.Controls.Add(documentsButton);
            this.Controls.Add(healthButton);
        }
    }
}