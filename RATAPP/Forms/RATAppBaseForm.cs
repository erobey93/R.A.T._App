using Azure;
using RATAPP.Panels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace RATAPP.Forms
{
    public partial class RATAppBaseForm : Form
    {
        // Class-level fields for controls
        private Panel topNavPanel;
        private Panel sideNavPanel;
        private Panel panelContent; // Placeholder content panel to hold dynamic content
        private Label appNameLabel;
        private Label userNameLabel;
        private Button utilitiesButton;
        private Button logoutButton;
        private Button createViewDocsButton;
        private Button homeButton;
        private Button breedingButton;
        private Button businessButton;
        private Button geneticsButton;

        // Panels for specific views
        private Panel homePanel;
        private Panel pairingsAndLittersPanel;

        // Property to hold UserName
        public string UserName { get; set; }

        // contentPanel is the panel where content like HomePanel or GeneticsPanel will be displayed
        public Panel contentPanel { get { return this.panelContent; } }  // panelContent is the placeholder panel for content

        public RATAppBaseForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Initialize the form properties
            this.Text = "R.A.T. APP";
            this.Size = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // Initialize components
            InitializePanels();
            InitializeLabels();
            InitializeButtons();

            // Add controls to form
            this.Controls.Add(contentPanel);
            this.Controls.Add(sideNavPanel);
            this.Controls.Add(topNavPanel);
        }

        private void InitializePanels()
        {
            // Top navigation panel
            topNavPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.DarkBlue,
            };

            // Side navigation panel
            sideNavPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = Color.Navy
            };

            // Content panel
            panelContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
        }

        // Method to set the username
        public void SetUserName(string username)
        {
            UserName = username;
            userNameLabel.Text = UserName;  // Update the label with the username
        }

        private void InitializeLabels()
        {
            // Adjusted Y position for the labels
            int topMargin = 10;  // This is the distance from the top of the panel to the app name label

            // App name label
            appNameLabel = new Label
            {
                Text = "R.A.T. APP",
                ForeColor = Color.White,
                Font = new Font("Arial", 14F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, topMargin)  // Set the Y position higher
            };
            topNavPanel.Controls.Add(appNameLabel);

            // User name label (Positioned below the app name label)
            userNameLabel = new Label
            {
                Text = UserName ?? "User Name", // Default text if no name is set
                ForeColor = Color.White,
                Font = new Font("Arial", 10F),
                AutoSize = true,
                Location = new Point(10, appNameLabel.Bottom + 5) // Positioning just below the app name label
            };
            topNavPanel.Controls.Add(userNameLabel);
        }

        private void InitializeButtons()
        {
            // Utilities button
            utilitiesButton = CreateTopNavButton("Utilities", 10, UtilitiesButton_Click);
            utilitiesButton.Location = new Point(200, 40); // Set the Y-position manually for proper alignment
            topNavPanel.Controls.Add(utilitiesButton);

            // Log out button
            logoutButton = CreateTopNavButton("Log Out", 10, LogoutButton_Click);
            logoutButton.Location = new Point(utilitiesButton.Right + 10, 40); // Positioning next to the utilities button
            topNavPanel.Controls.Add(logoutButton);

            // Create/View Documents button
            createViewDocsButton = CreateTopNavButton("Create/View Docs", 10, CreateViewDocsButton_Click);
            createViewDocsButton.Location = new Point(logoutButton.Right + 10, 40); // Positioning next to the logout button
            topNavPanel.Controls.Add(createViewDocsButton);

            // Side nav buttons
            homeButton = CreateSideNavButton("Home", HomeButton_Click);
            breedingButton = CreateSideNavButton("Breeding", BreedingButton_Click);
            businessButton = CreateSideNavButton("Business", BusinessButton_Click);
            geneticsButton = CreateSideNavButton("Genetics & Ancestry", GeneticsButton_Click);

            // Add buttons to side panel
            sideNavPanel.Controls.AddRange(new Control[] { breedingButton, businessButton, geneticsButton, homeButton });
        }

        private Button CreateTopNavButton(string text, int xPosition, EventHandler onClick)
        {
            var button = new Button
            {
                Text = text,
                ForeColor = Color.White,
                BackColor = Color.Navy,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 12F, FontStyle.Bold),
                Height = 60, // Ensure the buttons are tall enough
                Width = 180, // Ensure buttons are wide enough to fit the text
                FlatAppearance = { BorderSize = 3 },
                Padding = new Padding(10)
            };

            button.Click += onClick;
            return button;
        }

        private Button CreateSideNavButton(string text, EventHandler onClick)
        {
            var button = new Button
            {
                Text = text,
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                BackColor = Color.DarkBlue,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 12F, FontStyle.Bold),
                Height = 60,
                FlatAppearance = { BorderSize = 3 },
                Padding = new Padding(10)
            };

            button.Click += onClick;
            return button;
        }

        // Event handlers for buttons
        private void UtilitiesButton_Click(object sender, EventArgs e)
        {
            // Handle Utilities button click
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            // Show the message box with Yes/No buttons
            DialogResult result = MessageBox.Show(
                "Are you sure you want to exit?", // Message text
                "Exit Confirmation",             // Title of the message box
                MessageBoxButtons.YesNo,         // Options for the message box buttons
                MessageBoxIcon.Question          // Icon to show (optional)
            );

            // Check the result of the message box
            if (result == DialogResult.Yes)
            {
                // If the user clicks "Yes", close the application
                Application.Exit(); // This will close the app
            }
            else
            {
                // If the user clicks "No", just close the message box and continue
                return; // Continue the normal flow
            }
        }

        private void CreateViewDocsButton_Click(object sender, EventArgs e)
        {
            // Handle Create/View Docs button click
        }

        private void HomeButton_Click(object sender, EventArgs e)
        {
            var homePanel = new HomePanel(UserName, "role - TODO");
            ShowPanel(homePanel);  // Show the home panel
        }

        private void BreedingButton_Click(object sender, EventArgs e)
        {
            var pairingsAndLittersPanel = new PairingsAndLittersPanel();
            ShowPanel(pairingsAndLittersPanel); // Show the breeding panel (pairings and litters)
        }

        private void BusinessButton_Click(object sender, EventArgs e)
        {
            // Handle Business button click
        }

        private void GeneticsButton_Click(object sender, EventArgs e)
        {
            // Handle Genetics button click
        }

        // Method to switch panels
        private void ShowPanel(Panel panelToShow)
        {
            //// Show the selected panel

            // Clear the existing content and add the new panel
            contentPanel.Controls.Clear();
            contentPanel.Controls.Add(panelToShow);
        }
    }
}