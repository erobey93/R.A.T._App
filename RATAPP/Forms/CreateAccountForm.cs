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

        // UI Constants
        private readonly Color PRIMARY_COLOR = Color.FromArgb(0, 120, 215);
        private readonly Color SECONDARY_COLOR = Color.FromArgb(0, 150, 136);
        private readonly Color NEUTRAL_COLOR = Color.FromArgb(158, 158, 158);
        private readonly Font TITLE_FONT = new Font("Segoe UI", 16, FontStyle.Bold);
        private readonly Font INPUT_FONT = new Font("Segoe UI", 12);

        public CreateAccountForm(RATAPPLibrary.Data.DbContexts.RatAppDbContextFactory contextFactory, IConfigurationRoot configuration, PasswordHashing passwordHashing)
        {
            //_context = context;
            _contextFactory = contextFactory; //FIXME I don't think I actually need to be passing this around, but keeping the current pattern for now 
            _configuration = configuration;
            _passwordHashing = passwordHashing;
            _accountService = new AccountService(contextFactory, _configuration, _passwordHashing);

            CreateAcctForm();
        }


        private void CreateAcctForm()
        {
            // Set form properties
            this.Text = "RAT App - Create Account";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(650, 900);
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Center position calculation
            int centerX = this.ClientSize.Width / 2;

            // Add logo at the top
            var logo = new PictureBox
            {
                Name = "logo",
                Size = new Size(200, 200),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Image.FromFile("C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\RATAPPLogo.png"),
            };
            logo.Location = new Point((this.ClientSize.Width - logo.Width) / 2, 20);
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
            this.Controls.Add(lblTitle);

            // Adjust title position after adding to the form
            lblTitle.Location = new Point((this.ClientSize.Width - lblTitle.Width) / 2, logo.Bottom + 10);

            // Initialize layout variables
            int currentY = lblTitle.Bottom + 20;
            int fieldSpacing = 50;
            int fieldWidth = 300;

            // Helper function for creating centered textboxes
            TextBox CreateCenteredTextBox(string name, string placeholder, int y)
            {
                var textBox = CreateTextBox(name, placeholder, y, fieldWidth);
                textBox.Location = new Point(centerX - (fieldWidth / 2), y);
                return textBox;
            }

            // Create textboxes
            this.Controls.Add(CreateCenteredTextBox("txtFirstName", "First Name", currentY));
            currentY += fieldSpacing;
            this.Controls.Add(CreateCenteredTextBox("txtLastName", "Last Name", currentY));
            currentY += fieldSpacing;
            this.Controls.Add(CreateCenteredTextBox("txtPhone", "Phone Number", currentY));
            currentY += fieldSpacing;
            this.Controls.Add(CreateCenteredTextBox("txtEmail", "Email Address", currentY));
            currentY += fieldSpacing;
            this.Controls.Add(CreateCenteredTextBox("txtCity", "City", currentY));
            currentY += fieldSpacing;
            this.Controls.Add(CreateCenteredTextBox("txtState", "State", currentY));
            currentY += fieldSpacing;
            this.Controls.Add(CreateCenteredTextBox("txtUsername", "Username", currentY));
            currentY += fieldSpacing;

            // Password field
            var txtPassword = CreateCenteredTextBox("txtPassword", "Password", currentY);
            txtPassword.PasswordChar = '•';
            this.Controls.Add(txtPassword);
            currentY += fieldSpacing;

            // Account Type dropdown
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
            cmbAccountType.SelectedIndex = 1;
            this.Controls.Add(cmbAccountType);

            currentY += fieldSpacing;

            // Submit button
            var btnSubmit = CreateButton("btnSubmit", "Create Account", currentY, fieldWidth, PRIMARY_COLOR);
            btnSubmit.Location = new Point(centerX - (fieldWidth / 2), currentY);
            btnSubmit.Click += BtnCreateAccount_Click;
            this.Controls.Add(btnSubmit);

            // Cancel button below the Submit button
            var btnCancel = CreateButton("btnCancel", "Cancel", currentY + 60, fieldWidth, NEUTRAL_COLOR);
            btnCancel.Location = new Point(centerX - (fieldWidth / 2), currentY + 60);
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
                Font = new Font("Segoe UI", 12),
                Size = new Size(width, 50),
                Location = new Point((this.ClientSize.Width - width) / 2, y),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
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
