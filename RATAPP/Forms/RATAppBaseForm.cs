using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using RATAPP.Helpers;
using RATAPP.Interfaces;
using RATAPP.Panels;
using RATAPPLibrary.Services;
using RATAPPLibrary.Services.Genetics;

namespace RATAPP.Forms
{
    /// <summary>
    /// Base form class for all forms in the RAT APP application.
    /// Provides common functionality for responsive layout, theming, and navigation.
    /// </summary>
    public partial class RATAppBaseForm : Form
    {
        // Events for notifying panels of form changes
        public event EventHandler FormResized;
        public event EventHandler ThemeChanged;
        public event EventHandler<UserInfoChangedEventArgs> UserInfoChanged;

        // Theme settings
        private ThemeSettings _currentTheme;
        public ThemeSettings CurrentTheme => _currentTheme;

        // User information
        private string _username;
        private string _userRole;

        /// <summary>
        /// Gets the current username.
        /// </summary>
        public string Username => _username;

        /// <summary>
        /// Gets the current user role.
        /// </summary>
        public string UserRole => _userRole;

        // Content panel for hosting active panels
        public Panel contentPanel;


        // Class-level fields for controls that will be on all pages 
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

        // Database context
       private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;

        //picture box for R.A.T. logo
        private PictureBox logoPictureBox;


        // Responsive layout settings
        private Size _originalSize;
        private Dictionary<Control, ControlProperties> _originalControlProperties;
        private bool _isResizing = false;

        // Navigation history
        private Stack<ResponsivePanel> _navigationHistory;
        private ResponsivePanel _currentPanel;

        /// <summary>
        /// Gets or sets the current active panel.
        /// </summary>
        public ResponsivePanel CurrentPanel
        {
            get { return _currentPanel; }
            set
            {
                if (_currentPanel != value)
                {
                    if (_currentPanel != null)
                    {
                        _currentPanel.SaveState();
                        _currentPanel.Visible = false;
                    }

                    _currentPanel = value;

                    if (_currentPanel != null)
                    {
                        _currentPanel.Visible = true;
                        _currentPanel.RestoreState();
                        _currentPanel.BringToFront();
                        _currentPanel.ApplyTheme();
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the RATAppBaseForm class.
        /// </summary>
        public RATAppBaseForm(RATAPPLibrary.Data.DbContexts.RatAppDbContext context)
        {
            _context = context;
            InitializeBaseForm();
        }

        /// <summary>
        /// Initializes the base form properties and event handlers.
        /// </summary>
        private void InitializeBaseForm()
        {

            // Initialize collections
            _originalControlProperties = new Dictionary<Control, ControlProperties>();
            _navigationHistory = new Stack<ResponsivePanel>();

            // Set default form properties
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);
            this.Size = new Size(1280, 720);
            this.Text = "RAT APP";

            // Initialize components
            InitializePanels();
            InitializeLabels();
            InitializeButtons();
            InitializeLogoPictureBox();

            // Add controls to form
            this.Controls.Add(contentPanel);
            this.Controls.Add(sideNavPanel);
            this.Controls.Add(topNavPanel);

            // Create content panel
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(contentPanel);

            // Store original size for scaling calculations
            _originalSize = this.Size;

            // Set default theme
            _currentTheme = ThemeSettings.DefaultLight();

            // Register event handlers
            this.Load += RATAppBaseForm_Load;
            this.Resize += RATAppBaseForm_Resize;
            this.SizeChanged += RATAppBaseForm_SizeChanged;
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
                Text = Username ?? "User Name", // Default text if no name is set
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
            RefreshCurrentPanel();

        }

        private async void HomeButton_Click(object sender, EventArgs e)
        {
            //var homePanel = new HomePanel(this, _context, UserName, "role - TODO");
            var homePanel = await HomePanel.CreateAsync(this, _context, Username, "role - TODO"); //switched to using an async factory pattern for creating the home panel, this should be for all panels 
            _currentPanel = homePanel; //set the active panel to the home panel FIXME need to think through this better this is an issue because it only gets set when the home BUTTON is clicked 
            ShowPanel(homePanel);  // Show the home panel
        }

        private void BreedingButton_Click(object sender, EventArgs e)
        {
            var pairingsAndLittersPanel = new PairingsAndLittersPanel(this, _context);
            _currentPanel = pairingsAndLittersPanel;
            ShowPanel(pairingsAndLittersPanel); // Show the breeding panel (pairings and litters)
        }

        private void BusinessButton_Click(object sender, EventArgs e)
        {
            // Handle Business button click
        }

        private void GeneticsButton_Click(object sender, EventArgs e)
        {
            //FIXME there should probably be services defined at the top level if this is the pattern I choose to go with 
            var traitService = new TraitService(_context);
            var breedingService = new BreedingCalculationService(_context);
            var animalService = new AnimalService(_context);
            var geneticsService = new GeneService(_context);   
            
            // Handle Genetics button click
            var geneticsPanel = new GeneticsPanel(this, traitService,geneticsService, breedingService, animalService, _context); 
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

        //below are the new responsive methods 

        /// <summary>
        /// Handles the Load event of the form.
        /// </summary>
        private void RATAppBaseForm_Load(object sender, EventArgs e)
        {
            // Store original properties of all controls
            StoreOriginalControlProperties(this.Controls);

            // Apply theme
            ApplyTheme();
        }

        /// <summary>
        /// Handles the Resize event of the form.
        /// </summary>
        private void RATAppBaseForm_Resize(object sender, EventArgs e)
        {
            if (!_isResizing)
            {
                _isResizing = true;
                ResizeControls();
                _isResizing = false;
            }
        }

        /// <summary>
        /// Handles the SizeChanged event of the form.
        /// </summary>
        private void RATAppBaseForm_SizeChanged(object sender, EventArgs e)
        {
            // Notify listeners that the form has been resized
            OnFormResized(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the FormResized event.
        /// </summary>
        protected virtual void OnFormResized(EventArgs e)
        {
            FormResized?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the ThemeChanged event.
        /// </summary>
        protected virtual void OnThemeChanged(EventArgs e)
        {
            ThemeChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the UserInfoChanged event.
        /// </summary>
        protected virtual void OnUserInfoChanged(UserInfoChangedEventArgs e)
        {
            UserInfoChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Sets the current username and role.
        /// </summary>
        /// <param name="username">The username to set</param>
        /// <param name="role">The user role to set</param>
        public void SetUserInfo(string username, string role = "User")
        {
            string oldUsername = _username;
            string oldRole = _userRole;

            _username = username;
            _userRole = role;

            // Update the form title to include the username
            this.Text = $"RAT APP - {_username}";

            // Notify listeners that the user info has changed
            OnUserInfoChanged(new UserInfoChangedEventArgs(oldUsername, oldRole, _username, _userRole));
        }

        /// <summary>
        /// Sets the current username.
        /// </summary>
        /// <param name="username">The username to set</param>
        public void SetUserName(string username)
        {
            SetUserInfo(username, _userRole);
        }

        /// <summary>
        /// Sets the current user role.
        /// </summary>
        /// <param name="role">The user role to set</param>
        public void SetUserRole(string role)
        {
            SetUserInfo(_username, role);
        }

        /// <summary>
        /// Stores the original properties of all controls for scaling.
        /// </summary>
        private void StoreOriginalControlProperties(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (!_originalControlProperties.ContainsKey(control))
                {
                    _originalControlProperties.Add(control, new ControlProperties
                    {
                        Size = control.Size,
                        Location = control.Location,
                        Font = control.Font
                    });
                }

                // Recursively store properties of child controls
                if (control.Controls.Count > 0)
                {
                    StoreOriginalControlProperties(control.Controls);
                }
            }
        }

        /// <summary>
        /// Resizes all controls based on the current form size.
        /// </summary>
        private void ResizeControls()
        {
            // Skip if form is minimized
            if (this.WindowState == FormWindowState.Minimized)
                return;

            // Calculate scale factors
            float widthRatio = (float)this.ClientSize.Width / _originalSize.Width;
            float heightRatio = (float)this.ClientSize.Height / _originalSize.Height;

            // Resize controls that are not ResponsivePanels
            foreach (var kvp in _originalControlProperties)
            {
                Control control = kvp.Key;
                ControlProperties originalProps = kvp.Value;

                // Skip ResponsivePanels (they handle their own resizing)
                if (control is ResponsivePanel)
                    continue;

                // Skip controls that are no longer in the form
                if (control.IsDisposed || !control.Visible)
                    continue;

                // Scale size and location
                int newWidth = (int)(originalProps.Size.Width * widthRatio);
                int newHeight = (int)(originalProps.Size.Height * heightRatio);
                control.Size = new Size(newWidth, newHeight);

                int newX = (int)(originalProps.Location.X * widthRatio);
                int newY = (int)(originalProps.Location.Y * heightRatio);
                control.Location = new Point(newX, newY);

                // Scale font if needed
                if (originalProps.Font != null)
                {
                    float scaleFactor = Math.Min(widthRatio, heightRatio);
                    float newFontSize = originalProps.Font.Size * scaleFactor;
                    // Ensure minimum font size
                    newFontSize = Math.Max(newFontSize, 8.0f);
                    control.Font = new Font(originalProps.Font.FontFamily, newFontSize, originalProps.Font.Style);
                }
            }
        }

        /// <summary>
        /// Applies the current theme to the form and all its controls.
        /// </summary>
        public void ApplyTheme()
        {
            // Apply theme to form
            this.BackColor = _currentTheme.BackgroundColor;
            this.ForeColor = _currentTheme.TextColor;

            // Apply theme to all controls
            ApplyThemeToControls(this.Controls);

            // Notify listeners that the theme has changed
            OnThemeChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Applies the current theme to the specified controls.
        /// </summary>
        private void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                // Skip ResponsivePanels (they handle their own theming)
                if (control is ResponsivePanel)
                    continue;

                // Apply theme based on control type
                if (control is Button btn)
                {
                    btn.BackColor = _currentTheme.ButtonColor;
                    btn.ForeColor = _currentTheme.ButtonTextColor;
                    if (btn.FlatStyle == FlatStyle.Flat)
                    {
                        btn.FlatAppearance.BorderColor = _currentTheme.BorderColor;
                    }
                }
                else if (control is Label lbl)
                {
                    lbl.ForeColor = _currentTheme.TextColor;
                }
                else if (control is TextBox txt)
                {
                    txt.BackColor = _currentTheme.TextBoxColor;
                    txt.ForeColor = _currentTheme.TextColor;
                    txt.BorderStyle = _currentTheme.BorderStyle;
                }
                else if (control is ComboBox cmb)
                {
                    cmb.BackColor = _currentTheme.TextBoxColor;
                    cmb.ForeColor = _currentTheme.TextColor;
                }
                else if (control is DataGridView dgv)
                {
                    dgv.BackgroundColor = _currentTheme.BackgroundColor;
                    dgv.ForeColor = _currentTheme.TextColor;
                    dgv.GridColor = _currentTheme.BorderColor;

                    // Apply to headers
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = _currentTheme.HeaderColor;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = _currentTheme.HeaderTextColor;

                    // Apply to cells
                    dgv.DefaultCellStyle.BackColor = _currentTheme.CellColor;
                    dgv.DefaultCellStyle.ForeColor = _currentTheme.TextColor;
                    dgv.DefaultCellStyle.SelectionBackColor = _currentTheme.SelectionColor;
                    dgv.DefaultCellStyle.SelectionForeColor = _currentTheme.SelectionTextColor;
                }
                else if (control is Panel panel)
                {
                    panel.BackColor = _currentTheme.BackgroundColor;
                    panel.ForeColor = _currentTheme.TextColor;
                }

                // Recursively apply to child controls
                if (control.Controls.Count > 0)
                {
                    ApplyThemeToControls(control.Controls);
                }
            }
        }

        /// <summary>
        /// Changes the application theme.
        /// </summary>
        /// <param name="theme">The new theme to apply</param>
        public void ChangeTheme(ThemeSettings theme)
        {
            _currentTheme = theme;
            ApplyTheme();
        }

        /// <summary>
        /// Toggles between light and dark themes.
        /// </summary>
        public void ToggleTheme()
        {
            if (_currentTheme.BackgroundColor.GetBrightness() > 0.5)
            {
                // Currently light theme, switch to dark
                ChangeTheme(ThemeSettings.DefaultDark());
            }
            else
            {
                // Currently dark theme, switch to light
                ChangeTheme(ThemeSettings.DefaultLight());
            }
        }

        /// <summary>
        /// Navigates to the specified panel.
        /// </summary>
        /// <param name="panel">The panel to navigate to</param>
        /// <param name="addToHistory">Whether to add the current panel to navigation history</param>
        public void NavigateTo(ResponsivePanel panel, bool addToHistory = true)
        {
            if (panel == null)
                return;

            if (addToHistory && _currentPanel != null)
            {
                _navigationHistory.Push(_currentPanel);
            }

            CurrentPanel = panel;
        }

        /// <summary>
        /// Navigates back to the previous panel.
        /// </summary>
        /// <returns>True if navigation was successful, false otherwise</returns>
        public bool NavigateBack()
        {
            if (_navigationHistory.Count > 0)
            {
                ResponsivePanel previousPanel = _navigationHistory.Pop();
                CurrentPanel = previousPanel;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Shows the specified panel and hides all others.
        /// </summary>
        /// <param name="panelToShow">The panel to show</param>
        public void ShowPanel(ResponsivePanel panelToShow)
        {
            if (panelToShow == null)
                return;

            // Hide all panels in the content panel
            foreach (Control control in contentPanel.Controls)
            {
                if (control is ResponsivePanel panel)
                {
                    panel.Visible = false;
                }
            }

            // Show the specified panel
            panelToShow.Visible = true;
            panelToShow.BringToFront();
            panelToShow.ApplyTheme();

            // Refresh the panel data
            panelToShow.RefreshDataAsync();
        }

        /// <summary>
        /// Sets the specified panel as the active panel.
        /// </summary>
        /// <param name="panel">The panel to set as active</param>
        /// <param name="refreshData">Whether to refresh the panel data</param>
        public void SetActivePanel(ResponsivePanel panel, bool refreshData = true)
        {
            if (panel == null)
                return;

            // Set as current panel
            CurrentPanel = panel;

            // Refresh data if requested
            if (refreshData)
            {
                panel.RefreshDataAsync();
            }

            // Ensure the panel is visible and in front
            panel.Visible = true;
            panel.BringToFront();
        }

        /// <summary>
        /// Gets a panel by its type.
        /// </summary>
        /// <typeparam name="T">The type of panel to get</typeparam>
        /// <returns>The panel of the specified type, or null if not found</returns>
        public T GetPanel<T>() where T : ResponsivePanel
        {
            foreach (Control control in contentPanel.Controls)
            {
                if (control is T panel)
                {
                    return panel;
                }
            }

            return null;
        }

        /// <summary>
        /// Refreshes the data in the current panel.
        /// </summary>
        public async void RefreshCurrentPanel()
        {
            if (_currentPanel != null)
            {
                await _currentPanel.RefreshDataAsync();
            }
        }

        /// <summary>
        /// Class to store original control properties for scaling.
        /// </summary>
        private class ControlProperties
        {
            public Size Size { get; set; }
            public Point Location { get; set; }
            public Font Font { get; set; }
        }
    }

    /// <summary>
    /// Event arguments for the UserInfoChanged event.
    /// </summary>
    public class UserInfoChangedEventArgs : EventArgs
    {
        public string OldUsername { get; }
        public string OldRole { get; }
        public string NewUsername { get; }
        public string NewRole { get; }

        public UserInfoChangedEventArgs(string oldUsername, string oldRole, string newUsername, string newRole)
        {
            OldUsername = oldUsername;
            OldRole = oldRole;
            NewUsername = newUsername;
            NewRole = newRole;
        }
    }
}
