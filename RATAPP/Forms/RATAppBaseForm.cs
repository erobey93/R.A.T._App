using Azure;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using RATAPP.Panels;
using RATAPP.Properties;
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
    //require all forms to implement an interface, or "contract" for all of the side panel and top nav functionality
    public interface INavigable
    {
        Task RefreshDataAsync();
        //void NavigateToHome();
        //void NavigateToBreeding(); these are already working, but this is an example TODO think through logic more 
        //void NavigateToBusiness(); TODO add as implemented in the future
        //void NavigateToGenetics();
        //void Utilities(); 

    }

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

        // Panels for specific views FIXME I don't think that I'm using anymore 
        //private Panel homePanel;
        //private Panel pairingsAndLittersPanel;
        private INavigable _activePanel;

        // Database context
        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;

        //picture box for R.A.T. logo
        private PictureBox logoPictureBox;

        // Property to hold UserName
        public string UserName { get; set; }

        // contentPanel is the panel where content like HomePanel or GeneticsPanel will be displayed
        public Panel contentPanel { get { return this.panelContent; } }  // panelContent is the placeholder panel for content

        public RATAppBaseForm(RATAPPLibrary.Data.DbContexts.RatAppDbContext context)
        {
            _context = context;
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
            InitializeLogoPictureBox();

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
                Height = 200, // Increased height to accommodate the logo and buttons
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

            //// User name label (Positioned to the far right)
            userNameLabel = new Label
            {
                Text = UserName ?? "User Name", // Default text if no name is set
                ForeColor = Color.White,
                Font = new Font("Arial", 10F),
                AutoSize = true,
                Location = new Point(topNavPanel.Width - 110, topMargin) // Positioning to the far right
            };
            //topNavPanel.Controls.Add(userNameLabel);
        }

        private void InitializeButtons()
        {
            // Utilities button
            utilitiesButton = CreateTopNavButton("Utilities", 10, UtilitiesButton_Click);
            utilitiesButton.Location = new Point(200, 120); // Set the Y-position manually for proper alignment below the logo
            topNavPanel.Controls.Add(utilitiesButton);

            // Log out button
            logoutButton = CreateTopNavButton("Log Out", 10, LogoutButton_Click);
            logoutButton.Location = new Point(utilitiesButton.Right + 10, 120); // Positioning next to the utilities button
            topNavPanel.Controls.Add(logoutButton);

            // Create/View Documents button
            createViewDocsButton = CreateTopNavButton("Refresh", 10, RefreshButton_Click);
            createViewDocsButton.Location = new Point(logoutButton.Right + 10, 120); // Positioning next to the logout button
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

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            //each panel will have its own specific data that needs to be refreshed
            //so we will need to know what panel is currently being displayed and what data is associated with it
            //or we could refresh all data for all panels but I'm not sure how to do that yet

            //couple of options
            //go through and refresh all
            //foreach (Control control in this.Controls)
            //{
            //    if (control is Panel panel)
            //    {
            //        panel.RefreshData(); // Call RefreshData for each panel
            //    }
            //}

            //just refresh the current panel - this is likely a better option since I am grabbing data live from the db currently, if I use caching, the above option may make more sense
            _activePanel?.RefreshDataAsync();

        }

        private async void HomeButton_Click(object sender, EventArgs e)
        {
            //var homePanel = new HomePanel(this, _context, UserName, "role - TODO");
            var homePanel = await HomePanel.CreateAsync(this, _context, UserName, "role - TODO"); //switched to using an async factory pattern for creating the home panel, this should be for all panels 
            _activePanel = homePanel; //set the active panel to the home panel FIXME need to think through this better this is an issue because it only gets set when the home BUTTON is clicked 
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

        private void InitializeLogoPictureBox()
        {
            try
            {
                // Create and configure the PictureBox for the logo
                logoPictureBox = new PictureBox
                {
                    //TODO - replace with the actual path to your logo image or get from database
                    Image = Image.FromFile("C:\\Users\\earob\\source\\repos\\RATAPP\\RATAPPLibrary\\RATAPP\\Resources\\RATAPPLogo.png"),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Location = new Point(15, appNameLabel.Bottom + 10), // Positioning just below the app name label
                    Size = new Size(100, 100) // Adjust the size as needed
                };

                // Add the PictureBox to the top navigation panel
                topNavPanel.Controls.Add(logoPictureBox);
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"Logo image file not found: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Method to switch panels
        public void ShowPanel(Panel panelToShow)
        {
            // Clear the existing content and add the new panel
            contentPanel.Controls.Clear();

            //set the current panel contents to the panel to show
            contentPanel.Controls.Add(panelToShow);
        }
    }
}