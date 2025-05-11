using Microsoft.Extensions.Configuration;
using RATAPPLibrary.Services;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RATAPP.Forms
{
    public partial class CreateAccountForm : Form
    {
        //private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
        private RATAPPLibrary.Data.DbContexts.RatAppDbContextFactory _contextFactory; 
        private Microsoft.Extensions.Configuration.IConfigurationRoot _configuration;
        private PasswordHashing _passwordHashing;
        private RATAPPLibrary.Services.AccountService _accountService;

        // Store screen dimensions for responsive sizing
        private int screenWidth;
        private int screenHeight;

        // Define form size as percentage of screen
        private const double FORM_WIDTH_PERCENT = 0.5; // 50% of screen width
        private const double FORM_HEIGHT_PERCENT = 0.8; // 80% of screen height

        // UI Constants
        private readonly Color PRIMARY_COLOR = Color.FromArgb(0, 120, 215);
        private readonly Color SECONDARY_COLOR = Color.FromArgb(0, 150, 136);
        private readonly Color NEUTRAL_COLOR = Color.FromArgb(158, 158, 158);
        private readonly Color BACKGROUND_COLOR = Color.FromArgb(240, 240, 240);
        private readonly Font TITLE_FONT = new Font("Segoe UI", 16, FontStyle.Bold);
        private readonly Font INPUT_FONT = new Font("Segoe UI", 12);
        private readonly Font BUTTON_FONT = new Font("Segoe UI", 12);

        public CreateAccountForm(RATAPPLibrary.Data.DbContexts.RatAppDbContextFactory contextFactory, IConfigurationRoot configuration, PasswordHashing passwordHashing)
        {
            //_context = context;
            _contextFactory = contextFactory; //FIXME I don't think I actually need to be passing this around, but keeping the current pattern for now 
            _configuration = configuration;
            _passwordHashing = passwordHashing;
            _accountService = new AccountService(contextFactory, _configuration, _passwordHashing);

            // Get screen dimensions
            screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            screenHeight = Screen.PrimaryScreen.WorkingArea.Height;

            CreateAcctForm();

            // Handle resize events to maintain proportions
            this.Resize += CreateAccountForm_Resize;
        }

        private void CreateAccountForm_Resize(object sender, EventArgs e)
        {
            // Recalculate and reposition controls when form is resized
            RepositionControls();
        }

        private void RepositionControls()
        {
            // Calculate center positions
            int centerX = this.ClientSize.Width / 2;

            // Reposition logo
            if (this.Controls["logo"] != null)
            {
                var logo = (PictureBox)this.Controls["logo"];
                logo.Location = new Point(centerX - (logo.Width / 2), 20);
            }

            // Reposition title
            if (this.Controls["lblTitle"] != null)
            {
                var lblTitle = (Label)this.Controls["lblTitle"];
                lblTitle.Location = new Point(centerX - (lblTitle.Width / 2), 210);
            }

            // Reposition all text boxes and the combo box
            string[] controlNames = {
                "txtFirstName", "txtLastName", "txtPhone", "txtEmail",
                "txtCity", "txtState", "txtUsername", "txtPassword",
                "cmbAccountType", "btnSubmit", "btnCancel"
            };

            foreach (string name in controlNames)
            {
                if (this.Controls[name] != null)
                {
                    var control = this.Controls[name];
                    control.Location = new Point(centerX - (control.Width / 2), control.Location.Y);
                }
            }

            // Reposition placeholder for combo box
            if (this.Controls["accountTypePlaceholder"] != null && this.Controls["cmbAccountType"] != null)
            {
                var placeholder = (Label)this.Controls["accountTypePlaceholder"];
                var comboBox = this.Controls["cmbAccountType"];
                placeholder.Location = new Point(comboBox.Left + 10, comboBox.Top + 3);
            }
        }

        private void CreateAcctForm()
        {
            // Calculate form size based on screen dimensions
            int formWidth = (int)(screenWidth * FORM_WIDTH_PERCENT);
            int formHeight = (int)(screenHeight * FORM_HEIGHT_PERCENT);

            // Set form properties to match login form style
            this.Text = "RAT App - Create Account";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(formWidth, formHeight);
            this.MinimumSize = new Size(500, 650); // Set minimum size to ensure controls fit
            this.BackColor = BACKGROUND_COLOR;
            this.FormBorderStyle = FormBorderStyle.Sizable; // Allow resizing
            this.MaximizeBox = true;

            // Calculate center position
            int centerX = this.ClientSize.Width / 2;

            // Add logo at the top
            var logo = new PictureBox
            {
                Name = "logo",
                Size = new Size(180, 180),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Image.FromFile("C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\RATAPPLogo.png"),
                Location = new Point(centerX - 90, 20)
            };
            this.Controls.Add(logo);

            // Create title label
            var lblTitle = new Label
            {
                Name = "lblTitle",
                Text = "Create New Account",
                Font = TITLE_FONT,
                ForeColor = PRIMARY_COLOR,
                AutoSize = true
            };
            lblTitle.Location = new Point(centerX - (lblTitle.Width / 2), 210);
            this.Controls.Add(lblTitle);

            // Create personal information section
            int startY = 260;
            int fieldSpacing = 50;
            int currentY = startY;
            int fieldWidth = 300;

            // First Name
            var txtFirstName = CreateTextBox("txtFirstName", "First Name", currentY, fieldWidth);
            this.Controls.Add(txtFirstName);
            currentY += fieldSpacing;

            // Last Name
            var txtLastName = CreateTextBox("txtLastName", "Last Name", currentY, fieldWidth);
            this.Controls.Add(txtLastName);
            currentY += fieldSpacing;

            // Phone
            var txtPhone = CreateTextBox("txtPhone", "Phone Number", currentY, fieldWidth);
            this.Controls.Add(txtPhone);
            currentY += fieldSpacing;

            // Email
            var txtEmail = CreateTextBox("txtEmail", "Email Address", currentY, fieldWidth);
            this.Controls.Add(txtEmail);
            currentY += fieldSpacing;

            // City
            var txtCity = CreateTextBox("txtCity", "City", currentY, fieldWidth);
            this.Controls.Add(txtCity);
            currentY += fieldSpacing;

            // State
            var txtState = CreateTextBox("txtState", "State", currentY, fieldWidth);
            this.Controls.Add(txtState);
            currentY += fieldSpacing;

            // Username
            var txtUsername = CreateTextBox("txtUsername", "Username", currentY, fieldWidth);
            this.Controls.Add(txtUsername);
            currentY += fieldSpacing;

            // Password
            var txtPassword = CreateTextBox("txtPassword", "Password", currentY, fieldWidth);
            txtPassword.PasswordChar = '•';
            this.Controls.Add(txtPassword);
            currentY += fieldSpacing;

            // Account Type
            var cmbAccountType = new ComboBox
            {
                Name = "cmbAccountType",
                Font = INPUT_FONT,
                Size = new Size(fieldWidth, 30),
                Location = new Point(centerX - (fieldWidth / 2), currentY),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.White
            };
            cmbAccountType.Items.AddRange(new string[] { "Admin", "User", "Guest" });
            cmbAccountType.SelectedIndex = 1; // Default to "User"

            // Add placeholder text effect for ComboBox
            var accountTypePlaceholder = new Label
            {
                Name = "accountTypePlaceholder",
                Text = "Account Type",
                Font = INPUT_FONT,
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(cmbAccountType.Left + 10, cmbAccountType.Top + 3),
                BackColor = Color.Transparent
            };

            cmbAccountType.DropDown += (s, e) => { accountTypePlaceholder.Visible = false; };
            cmbAccountType.DropDownClosed += (s, e) =>
            {
                if (cmbAccountType.SelectedIndex == -1)
                    accountTypePlaceholder.Visible = true;
                else
                    accountTypePlaceholder.Visible = false;
            };

            this.Controls.Add(cmbAccountType);
            this.Controls.Add(accountTypePlaceholder);

            currentY += fieldSpacing;

            // Submit button (Create Account)
            var btnSubmit = CreateButton("btnSubmit", "Create Account", currentY, fieldWidth, PRIMARY_COLOR);
            btnSubmit.Click += BtnCreateAccount_Click;
            this.Controls.Add(btnSubmit);
            currentY += 60;

            // Cancel button
            var btnCancel = CreateButton("btnCancel", "Cancel", currentY, fieldWidth, NEUTRAL_COLOR);
            btnCancel.Click += BtnCancel_Click;
            this.Controls.Add(btnCancel);
        }

        private TextBox CreateTextBox(string name, string placeholder, int y, int width)
        {
            var textBox = new TextBox
            {
                Name = name,
                Font = INPUT_FONT,
                Size = new Size(width, 30),
                Location = new Point((this.ClientSize.Width - width) / 2, y),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Add placeholder text effect
            textBox.Text = placeholder;
            textBox.ForeColor = Color.Gray;

            textBox.Enter += (s, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.ForeColor = Color.Black;
                }
            };

            textBox.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = Color.Gray;
                }
            };

            return textBox;
        }

        private Button CreateButton(string name, string text, int y, int width, Color color)
        {
            return new Button
            {
                Name = name,
                Text = text,
                Font = BUTTON_FONT,
                Size = new Size(width, 40),
                Location = new Point((this.ClientSize.Width - width) / 2, y),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };
        }

        private async void BtnCreateAccount_Click(object sender, EventArgs e)
        {
            // Show "processing" cursor
            this.Cursor = Cursors.WaitCursor;

            // Disable the submit button to prevent multiple submissions
            var btnSubmit = (Button)this.Controls["btnSubmit"];
            btnSubmit.Enabled = false;
            btnSubmit.Text = "Creating Account...";

            try
            {
                // Validate required fields
                if (!ValidateFields())
                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Call method to create the account
                bool accountCreated = await CreateAccountAsync();

                // Handle success or failure
                if (accountCreated)
                {
                    await _accountService.CreateNewBreeder(GetTextBoxValue("txtUsername"));
                    MessageBox.Show("Account created successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    BackToLogin();
                }
                else
                {
                    MessageBox.Show("Account creation failed. Please try again.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Restore cursor and button state
                this.Cursor = Cursors.Default;
                btnSubmit.Enabled = true;
                btnSubmit.Text = "Create Account";
            }
        }

        private bool ValidateFields()
        {
            // Check if required fields are filled
            string[] requiredFields = { "txtFirstName", "txtLastName", "txtEmail", "txtUsername", "txtPassword" };

            foreach (string fieldName in requiredFields)
            {
                string value = GetTextBoxValue(fieldName);
                if (string.IsNullOrWhiteSpace(value))
                    return false;
            }

            // Check if account type is selected
            ComboBox cmbAccountType = (ComboBox)this.Controls["cmbAccountType"];
            if (cmbAccountType.SelectedIndex == -1)
                return false;

            return true;
        }

        private string GetTextBoxValue(string controlName)
        {
            TextBox textBox = (TextBox)this.Controls[controlName];
            string placeholder = textBox.ForeColor == Color.Gray ? textBox.Text : "";

            return textBox.Text != placeholder ? textBox.Text : "";
        }

        private async Task<bool> CreateAccountAsync()
        {
            // Gather data from the form
            string firstName = GetTextBoxValue("txtFirstName");
            string lastName = GetTextBoxValue("txtLastName");
            string phone = GetTextBoxValue("txtPhone");
            string email = GetTextBoxValue("txtEmail");
            string city = GetTextBoxValue("txtCity");
            string state = GetTextBoxValue("txtState");
            string username = GetTextBoxValue("txtUsername");
            string password = GetTextBoxValue("txtPassword");
            string accountTypeName = ((ComboBox)this.Controls["cmbAccountType"]).Text;

            return await _accountService.CreateAccountAsync(
                username, password, email, firstName, lastName,
                phone, city, state, accountTypeName);
        }

        private void BackToLogin()
        {
            // Navigate back to the login form
            this.Hide();
            var loginForm = new LoginForm(_contextFactory, _configuration, _passwordHashing);
            loginForm.Show();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            BackToLogin();
        }

        // Handle form closing
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Instead of asking to confirm exit, just go back to login
                e.Cancel = true;
                BackToLogin();
            }
        }
    }
}