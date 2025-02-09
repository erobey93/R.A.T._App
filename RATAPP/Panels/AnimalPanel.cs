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
        }

        // Method to initialize labels
        private void InitializeLabels()
        {
            animalNameLabel = CreateLabel("Animal Name", 10, 10);
            idLabel = CreateLabel("ID", 10, 40);
            speciesLabel = CreateLabel("Species", 10, 70);
            sexLabel = CreateLabel("Sex", 10, 100);
            varietyLabel = CreateLabel("Variety", 10, 130);
            colorLabel = CreateLabel("Color", 10, 160);
            geneticsLabel = CreateLabel("Genetics", 10, 190);
            ancestryLabel = CreateLabel("Ancestry", 10, 220);
            breederInfoLabel = CreateLabel("Breeder Info", 10, 250);
            commentsLabel = CreateLabel("Comments", 10, 280);

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

        // Method to initialize textboxes for information
        private void InitializeTextBoxes()
        {
            animalNameTextBox = CreateTextBox(100, 10);
            idTextBox = CreateTextBox(100, 40);
            speciesTextBox = CreateTextBox(100, 70);
            sexTextBox = CreateTextBox(100, 100);
            varietyTextBox = CreateTextBox(100, 130);
            colorTextBox = CreateTextBox(100, 160);
            geneticsTextBox = CreateTextBox(100, 190);
            ancestryTextBox = CreateTextBox(100, 220);
            breederInfoTextBox = CreateTextBox(100, 250);
            commentsTextBox = CreateTextBox(100, 280);

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
        private void InitializePhotoBox()
        {
            animalPhotoBox = new PictureBox
            {
                Location = new Point(10, 320),
                Size = new Size(200, 200),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Add picture box to panel
            this.Controls.Add(animalPhotoBox);
        }
    }
}
