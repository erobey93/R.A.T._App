
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RATAPP.Panels;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Services;
using RATAPPLibrary.Services.Genetics;

namespace RATAPP.Forms
{
    public interface INavigable
    {
        Task RefreshDataAsync();
    }

    public partial class RATAppBaseForm : Form
    {
        private Panel topNavPanel;
        private Panel sideNavPanel;
        public Panel contentPanel;
        private Label appNameLabel;
        private PictureBox logoPictureBox;
        private INavigable _activePanel;
        private RatAppDbContextFactory _contextFactory; 

        public string UserName { get; set; }
        public Panel ContentPanel => contentPanel;

        public RATAppBaseForm(RatAppDbContextFactory contextFactory)
        {
            //_context = context;
            _contextFactory = contextFactory;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.Text = "R.A.T. APP";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            InitializePanels();
            InitializeTopNav();
            InitializeSideNav();

            this.Controls.Add(contentPanel);
            this.Controls.Add(sideNavPanel);
            this.Controls.Add(topNavPanel);

            this.ResumeLayout(false);
        }

        private void InitializePanels()
        {
            topNavPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(0, 120, 212)
            };

            sideNavPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 200,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
        }

        private void InitializeTopNav()
        {
            logoPictureBox = new PictureBox
            {
                Image = Image.FromFile("C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\RATAPPLogo.png"), // Replace with actual path
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(40, 40),
                Location = new Point(10, 10)
            };

            appNameLabel = new Label
            {
                Text = "R.A.T. APP",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(60, 15)
            };

            var refreshButton = CreateTopNavButton("Refresh", 0, RefreshButton_Click);
            var utilitiesButton = CreateTopNavButton("Utilities", 1, UtilitiesButton_Click);
            var logoutButton = CreateTopNavButton("Log Out", 2, LogoutButton_Click);
            var settingsButton = CreateTopNavButton("Settings", 3, SettingsButton_Click);

            topNavPanel.Controls.AddRange(new Control[] { logoPictureBox, appNameLabel, refreshButton, utilitiesButton, logoutButton });
        }

        private void InitializeSideNav()
        {
            var buttons = new[]
             {
                CreateSideNavButton("Research", ResearchButton_Click),
                CreateSideNavButton("Financial", FinancialButton_Click),
                CreateSideNavButton("Reports", ReportsButton_Click),
                CreateSideNavButton("Adopter Management", AdopterButton_Click),
                CreateSideNavButton("Genetics", GeneticsButton_Click),
                CreateSideNavButton("Ancestry", AncestryButton_Click),
                CreateSideNavButton("Breeding", BreedingButton_Click),
                CreateSideNavButton("Home", HomeButton_Click),
            };

            sideNavPanel.Controls.AddRange(buttons);
        }

        private Button CreateTopNavButton(string text, int count, EventHandler onClick)
        {
            var button = new Button
            {
                Text = text,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Size = new Size(100, 40),
                Location = new Point(count * 110 + 200, 10),
                FlatAppearance = { BorderSize = 0 }
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
                ForeColor = Color.Black,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12),
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                FlatAppearance = { BorderSize = 0 }
            };
            button.Click += onClick;
            return button;
        }

        public void SetActivePanel(INavigable panel)
        {
            _activePanel = panel;
        }

        public void SetUserName(string username)
        {
            UserName = username;
        }

        public void ShowPanel(Panel panelToShow)
        {
            contentPanel.Controls.Clear();
            contentPanel.Controls.Add(panelToShow);
        }

        private async void RefreshButton_Click(object sender, EventArgs e)
        {
            if (_activePanel != null)
            {
                await _activePanel.RefreshDataAsync();
            }
        }

        private void UtilitiesButton_Click(object sender, EventArgs e)
        {
            Button utilitiesButton = (Button)sender; // cast sender as button 

            ContextMenuStrip utilitiesContextMenu = new ContextMenuStrip();

            ToolStripMenuItem settingsItem = new ToolStripMenuItem("Settings");
            settingsItem.Click += SettingsItem_Click;
            utilitiesContextMenu.Items.Add(settingsItem);

            ToolStripMenuItem styleItem = new ToolStripMenuItem("Style");
            styleItem.Click += StyleItem_Click;
            utilitiesContextMenu.Items.Add(styleItem);

            ToolStripMenuItem errorLogsItem = new ToolStripMenuItem("Error Logs");
            errorLogsItem.Click += ErrorLogsItem_Click;
            utilitiesContextMenu.Items.Add(errorLogsItem);

            ToolStripMenuItem bulkImportItem = new ToolStripMenuItem("Bulk Import Animals");
            bulkImportItem.Click += BulkImportItem_Click;
            utilitiesContextMenu.Items.Add(bulkImportItem);

            // Show the context menu below the Utilities button
            utilitiesContextMenu.Show(utilitiesButton, new Point(0, utilitiesButton.Height));
        }

        private void SettingsItem_Click(object sender, EventArgs e)
        {
            // Handle Settings menu item click
            MessageBox.Show("Settings clicked");
            // Add your settings logic here
        }

        private void StyleItem_Click(object sender, EventArgs e)
        {
            // Handle Style menu item click
            MessageBox.Show("Style clicked");
            // Add your style logic here
        }

        private void ErrorLogsItem_Click(object sender, EventArgs e)
        {
            // Handle Error Logs menu item click
            MessageBox.Show("Error Logs clicked");
            // Add your error logs logic here
        }

        private void BulkImportItem_Click(object sender, EventArgs e)
        {
            // Handle Error Logs menu item click
            MessageBox.Show("Bulk Import clicked");
            // Add your error logs logic here
        }

        private void AncestryButton_Click(object sender, EventArgs e)
        {
            // Handle Ancestry button click
            var ancestryPanel = new AncestryPanel(this, _contextFactory); // Create a new instance of the ReportsPanel
            _activePanel = ancestryPanel;
            ShowPanel(ancestryPanel);
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            // Handle Settings button click
            MessageBox.Show("Settings button clicked");
        }

        private void AdopterButton_Click(object sender, EventArgs e)
        {
            // Handle Adopter Management button click
            var adopterPanel = new AdopterManagementPanel(this, _contextFactory); // Create a new instance of the ReportsPanel
            _activePanel = adopterPanel;
            ShowPanel(adopterPanel);
        }

        private void ResearchButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
            //var researchService = new ResearchService(_contextFactory);
            //var researchPanel = new ResearchPanel(this, researchService);
            //_activePanel = researchPanel;
            //ShowPanel(researchPanel);
        }

        private void ReportsButton_Click(object sender, EventArgs e)
        {
            var reportsPanel = new ReportsPanel(_contextFactory); // Create a new instance of the ReportsPanel
            _activePanel = reportsPanel;
            ShowPanel(reportsPanel);
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to exit?",
                "Exit Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        public async void HomeButton_Click(object sender, EventArgs e)
        {
            var homePanel = await HomePanel.CreateAsync(this, _contextFactory, UserName, "role - TODO");
            _activePanel = homePanel;
            ShowPanel(homePanel);
        }

        private void BreedingButton_Click(object sender, EventArgs e)
        {
            var pairingsAndLittersPanel = new PairingsAndLittersPanel(this, _contextFactory);
            _activePanel = pairingsAndLittersPanel;
            ShowPanel(pairingsAndLittersPanel);
        }

        private void FinancialButton_Click(object sender, EventArgs e)
        {
            // Financial button click
            var financialPanel = new FinancialPanel(this, _contextFactory);
            _activePanel = financialPanel;
            ShowPanel(financialPanel);
        }

        private void GeneticsButton_Click(object sender, EventArgs e)
        {
            var geneticsPanel = new GeneticsPanel(
                this,
                new TraitService(_contextFactory),
                new GeneService(_contextFactory),
                new BreedingCalculationService(_contextFactory),
                _contextFactory);
            _activePanel = geneticsPanel;
            ShowPanel(geneticsPanel);
        }
    }
}
//using Azure;
//using Microsoft.AspNetCore.Rewrite;
//using Microsoft.EntityFrameworkCore;
//using RATAPP.Panels;
//using RATAPP.Properties;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//namespace RATAPP.Forms
//{
//    //require all forms to implement an interface, or "contract" for all of the side panel and top nav functionality
//    public interface INavigable
//    {
//        Task RefreshDataAsync();
//        //void NavigateToHome();
//        //void NavigateToBreeding(); these are already working, but this is an example TODO think through logic more 
//        //void NavigateToBusiness(); TODO add as implemented in the future
//        //void NavigateToGenetics();
//        //void Utilities(); 

//    }

//    public partial class RATAppBaseForm : Form
//    {
//        // Class-level fields for controls
//        private Panel topNavPanel;
//        private Panel sideNavPanel;
//        private Panel panelContent; // Placeholder content panel to hold dynamic content
//        private Label appNameLabel;
//        private Label userNameLabel;
//        private Button utilitiesButton;
//        private Button logoutButton;
//        private Button createViewDocsButton;
//        private Button homeButton;
//        private Button breedingButton;
//        private Button businessButton;
//        private Button geneticsButton;

//        // Panels for specific views FIXME I don't think that I'm using anymore 
//        //private Panel homePanel;
//        //private Panel pairingsAndLittersPanel;
//        private INavigable _activePanel;

//        // Database context
//        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;

//        //picture box for R.A.T. logo
//        private PictureBox logoPictureBox;

//        // Property to hold UserName
//        public string UserName { get; set; }

//        // contentPanel is the panel where content like HomePanel or GeneticsPanel will be displayed
//        public Panel contentPanel { get { return this.panelContent; } }  // panelContent is the placeholder panel for content

//        public RATAppBaseForm(RATAPPLibrary.Data.DbContexts.RatAppDbContext context)
//        {
//            _context = context;
//            InitializeComponent();
//        }

//        private void InitializeComponent()
//        {
//            // Initialize the form properties
//            this.Text = "R.A.T. APP";
//            this.Size = new Size(1024, 768);
//            this.StartPosition = FormStartPosition.CenterScreen;
//            this.WindowState = FormWindowState.Maximized;

//            // Initialize components
//            InitializePanels();
//            InitializeLabels();
//            InitializeButtons();
//            InitializeLogoPictureBox();

//            // Add controls to form
//            this.Controls.Add(contentPanel);
//            this.Controls.Add(sideNavPanel);
//            this.Controls.Add(topNavPanel);
//        }

//        private void InitializePanels()
//        {
//            // Top navigation panel
//            topNavPanel = new Panel
//            {
//                Dock = DockStyle.Top,
//                Height = 200, // Increased height to accommodate the logo and buttons
//                BackColor = Color.DarkBlue,
//            };

//            // Side navigation panel
//            sideNavPanel = new Panel
//            {
//                Dock = DockStyle.Left,
//                Width = 200,
//                BackColor = Color.Navy
//            };

//            // Content panel
//            panelContent = new Panel
//            {
//                Dock = DockStyle.Fill,
//                BackColor = Color.White
//            };
//        }

//        //kind of a hack, but for now this fixes the issue of having to click buttons to set the active panel 
//        //this will set the active panel to the home panel when the form is loaded
//        public void SetActivePanel(INavigable panel)
//        {
//            _activePanel = panel; 
//        }

//        // Method to set the username
//        public void SetUserName(string username)
//        {
//            UserName = username;
//            userNameLabel.Text = UserName;  // Update the label with the username
//        }

//        private void InitializeLabels()
//        {
//            // Adjusted Y position for the labels
//            int topMargin = 10;  // This is the distance from the top of the panel to the app name label

//            // App name label
//            appNameLabel = new Label
//            {
//                Text = "R.A.T. APP",
//                ForeColor = Color.White,
//                Font = new Font("Arial", 14F, FontStyle.Bold),
//                AutoSize = true,
//                Location = new Point(10, topMargin)  // Set the Y position higher
//            };
//            topNavPanel.Controls.Add(appNameLabel);

//            //// User name label (Positioned to the far right)
//            userNameLabel = new Label
//            {
//                Text = UserName ?? "User Name", // Default text if no name is set
//                ForeColor = Color.White,
//                Font = new Font("Arial", 10F),
//                AutoSize = true,
//                Location = new Point(topNavPanel.Width - 110, topMargin) // Positioning to the far right
//            };
//            //topNavPanel.Controls.Add(userNameLabel);
//        }

//        private void InitializeButtons()
//        {
//            // Utilities button
//            utilitiesButton = CreateTopNavButton("Utilities", 10, UtilitiesButton_Click);
//            utilitiesButton.Location = new Point(200, 120); // Set the Y-position manually for proper alignment below the logo
//            topNavPanel.Controls.Add(utilitiesButton);

//            // Log out button
//            logoutButton = CreateTopNavButton("Log Out", 10, LogoutButton_Click);
//            logoutButton.Location = new Point(utilitiesButton.Right + 10, 120); // Positioning next to the utilities button
//            topNavPanel.Controls.Add(logoutButton);

//            // Create/View Documents button
//            createViewDocsButton = CreateTopNavButton("Refresh", 10, RefreshButton_Click);
//            createViewDocsButton.Location = new Point(logoutButton.Right + 10, 120); // Positioning next to the logout button
//            topNavPanel.Controls.Add(createViewDocsButton);

//            // Side nav buttons
//            homeButton = CreateSideNavButton("Home", HomeButton_Click);
//            breedingButton = CreateSideNavButton("Breeding", BreedingButton_Click);
//            businessButton = CreateSideNavButton("Business", BusinessButton_Click);
//            geneticsButton = CreateSideNavButton("Genetics & Ancestry", GeneticsButton_Click);

//            // Add buttons to side panel
//            sideNavPanel.Controls.AddRange(new Control[] { breedingButton, businessButton, geneticsButton, homeButton });
//        }

//        private Button CreateTopNavButton(string text, int xPosition, EventHandler onClick)
//        {
//            var button = new Button
//            {
//                Text = text,
//                ForeColor = Color.White,
//                BackColor = Color.Navy,
//                FlatStyle = FlatStyle.Flat,
//                Font = new Font("Arial", 12F, FontStyle.Bold),
//                Height = 60, // Ensure the buttons are tall enough
//                Width = 180, // Ensure buttons are wide enough to fit the text
//                FlatAppearance = { BorderSize = 3 },
//                Padding = new Padding(10)
//            };

//            button.Click += onClick;
//            return button;
//        }

//        private Button CreateSideNavButton(string text, EventHandler onClick)
//        {
//            var button = new Button
//            {
//                Text = text,
//                Dock = DockStyle.Top,
//                ForeColor = Color.White,
//                BackColor = Color.DarkBlue,
//                FlatStyle = FlatStyle.Flat,
//                Font = new Font("Arial", 12F, FontStyle.Bold),
//                Height = 60,
//                FlatAppearance = { BorderSize = 3 },
//                Padding = new Padding(10)
//            };

//            button.Click += onClick;
//            return button;
//        }

//        // Event handlers for buttons
//        private void UtilitiesButton_Click(object sender, EventArgs e)
//        {
//            // Handle Utilities button click
//        }

//        private void LogoutButton_Click(object sender, EventArgs e)
//        {
//            // Show the message box with Yes/No buttons
//            DialogResult result = MessageBox.Show(
//                "Are you sure you want to exit?", // Message text
//                "Exit Confirmation",             // Title of the message box
//                MessageBoxButtons.YesNo,         // Options for the message box buttons
//                MessageBoxIcon.Question          // Icon to show (optional)
//            );

//            // Check the result of the message box
//            if (result == DialogResult.Yes)
//            {
//                // If the user clicks "Yes", close the application
//                Application.Exit(); // This will close the app
//            }
//            else
//            {
//                // If the user clicks "No", just close the message box and continue
//                return; // Continue the normal flow
//            }
//        }

//        private void RefreshButton_Click(object sender, EventArgs e)
//        {
//            //each panel will have its own specific data that needs to be refreshed
//            //so we will need to know what panel is currently being displayed and what data is associated with it
//            //or we could refresh all data for all panels but I'm not sure how to do that yet

//            //couple of options
//            //go through and refresh all
//            //foreach (Control control in this.Controls)
//            //{
//            //    if (control is Panel panel)
//            //    {
//            //        panel.RefreshData(); // Call RefreshData for each panel
//            //    }
//            //}

//            //just refresh the current panel - this is likely a better option since I am grabbing data live from the db currently, if I use caching, the above option may make more sense
//            _activePanel?.RefreshDataAsync();

//        }

//        private async void HomeButton_Click(object sender, EventArgs e)
//        {
//            //var homePanel = new HomePanel(this, _context, UserName, "role - TODO");
//            var homePanel = await HomePanel.CreateAsync(this, _context, UserName, "role - TODO"); //switched to using an async factory pattern for creating the home panel, this should be for all panels 
//            _activePanel = homePanel; //set the active panel to the home panel FIXME need to think through this better this is an issue because it only gets set when the home BUTTON is clicked 
//            ShowPanel(homePanel);  // Show the home panel
//        }

//        private void BreedingButton_Click(object sender, EventArgs e)
//        {
//            var pairingsAndLittersPanel = new PairingsAndLittersPanel();
//            _activePanel = pairingsAndLittersPanel;
//            ShowPanel(pairingsAndLittersPanel); // Show the breeding panel (pairings and litters)
//        }

//        private void BusinessButton_Click(object sender, EventArgs e)
//        {
//            // Handle Business button click
//        }

//        private void GeneticsButton_Click(object sender, EventArgs e)
//        {
//            // Handle Genetics button click
//        }

//        private void InitializeLogoPictureBox()
//        {
//            try
//            {
//                // Create and configure the PictureBox for the logo
//                logoPictureBox = new PictureBox
//                {
//                    //TODO - replace with the actual path to your logo image or get from database
//                    Image = Image.FromFile("C:\\Users\\earob\\source\\repos\\RATAPP\\RATAPPLibrary\\RATAPP\\Resources\\RATAPPLogo.png"),
//                    SizeMode = PictureBoxSizeMode.StretchImage,
//                    Location = new Point(15, appNameLabel.Bottom + 10), // Positioning just below the app name label
//                    Size = new Size(100, 100) // Adjust the size as needed
//                };

//                // Add the PictureBox to the top navigation panel
//                topNavPanel.Controls.Add(logoPictureBox);
//            }
//            catch (FileNotFoundException ex)
//            {
//                MessageBox.Show($"Logo image file not found: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }


//        // Method to switch panels
//        public void ShowPanel(Panel panelToShow)
//        {
//            // Clear the existing content and add the new panel
//            contentPanel.Controls.Clear();

//            //set the current panel contents to the panel to show
//            contentPanel.Controls.Add(panelToShow);
//        }
//    }
//}
