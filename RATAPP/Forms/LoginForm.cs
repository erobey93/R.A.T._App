using Microsoft.Extensions.Configuration;
using RATAPP.Forms;
using RATAPPLibrary.Services;
using RATAPPLibrary.Data;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.Models;
using RATAPP.Panels;

namespace RATAPP
{
    public partial class LoginForm : Form
    {
        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
        private IConfigurationRoot _configuration;
        private PasswordHashing _passwordHashing;

        public LoginForm(RATAPPLibrary.Data.DbContexts.RatAppDbContext context, Microsoft.Extensions.Configuration.IConfigurationRoot configuration, PasswordHashing passwordHashing)
        {
            _context = context;
            _configuration = configuration;
            _passwordHashing = passwordHashing;

            CreateLoginForm();
        }

        private void CreateLoginForm()
        {
            this.Text = "RAT App Login";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(600, 650);
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var logo = new PictureBox
            {
                Size = new Size(220, 220),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Image.FromFile("C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\RATAPPLogo.png"),
                Location = new Point((this.ClientSize.Width - 220) / 2, 20)
            };

            var lblUsername = CreateLabel("Username", 300);
            var lblPassword = CreateLabel("Password", 370);

            var txtUsername = CreateTextBox("txtUsername", "Username", 260);
            var txtPassword = CreateTextBox("txtPassword", "Password", 330);
            txtPassword.PasswordChar = '•';

            var btnLogin = CreateButton("Login", 410, Color.FromArgb(0, 120, 215));
            var btnCreateAccount = CreateButton("Create Account", 460, Color.FromArgb(0, 150, 136));
            var btnChangePassword = CreateButton("Change Password", 510, Color.FromArgb(158, 158, 158));

            btnLogin.Click += BtnLogin_Click;
            btnCreateAccount.Click += BtnCreateAccount_Click;
            btnChangePassword.Click += BtnUpdatePassword_Click;

            //TODO took label out for now 
            this.Controls.AddRange(new Control[] { logo, txtUsername, txtPassword, btnLogin, btnCreateAccount, btnChangePassword, lblUsername, lblPassword });
        }

        // Login to existing account
        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            // Check if the username and password text boxes are empty
            if (string.IsNullOrWhiteSpace(((TextBox)this.Controls["txtUsername"]).Text) || string.IsNullOrWhiteSpace(((TextBox)this.Controls["txtPassword"]).Text))
            {
                MessageBox.Show("Please enter a username and password.", "Login Failed");
                return;
            }

            // If boxes are populated, get the username and password from the text boxes
            string username = ((TextBox)this.Controls["txtUsername"]).Text;
            string password = ((TextBox)this.Controls["txtPassword"]).Text;

            try
            {
                // Create an instance of your login service and call the async Login method
                var loginService = new LoginService(_context, _configuration, _passwordHashing); // Inject dependencies
                var response = await loginService.Login(new RATAPPLibrary.Data.Models.Requests.LoginRequest { Username = username, Password = password });

                // Get the instance of the main form (RatAppBaseForm)
                var baseForm = Application.OpenForms.OfType<RATAppBaseForm>().FirstOrDefault();

                if (baseForm == null)
                {
                    // If for some reason the base form doesn't exist, create it here
                    baseForm = new RATAppBaseForm(_context);
                }

                // Set user info
                baseForm.SetUserName(response.Username);

                // Depending on the role, show the appropriate panel
                Panel contentPanelToShow = null;

                var homePanel = await HomePanel.CreateAsync(baseForm, _context, response.Username, response.Role); // You will need to create this panel
                if (response.Role == "Admin")
                {
                    // Create and show admin-specific panel
                    //TODO decide if I want to do anything differently if logged in as admin, for now logic is the same 
                    homePanel = await HomePanel.CreateAsync(baseForm, _context, response.Username, response.Role); // You will need to create this panel
                    contentPanelToShow = homePanel;
                }
                else if (response.Role == "User")
                {
                    // Create and show user-specific panel
                    homePanel = await HomePanel.CreateAsync(baseForm, _context, response.Username, response.Role); // You will need to create this panel
                    contentPanelToShow = homePanel;
                }

                // Clear current controls in the content panel and add the new panel
                baseForm.contentPanel.Controls.Clear();
                baseForm.contentPanel.Controls.Add(contentPanelToShow);  // Add the new panel
                baseForm.SetActivePanel(homePanel);  // Set the new panel as the active panel

                // Show the base form with the correct content panel
                baseForm.Show();
                this.Hide();
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Invalid username or password.", "Login Failed");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error");
            }
        }

        private Label CreateLabel(string text, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                Location = new Point((this.ClientSize.Width - 100) / 2, y),
                AutoSize = true
            };
        }

        //generic text box to be used for all login text boxes
        private TextBox CreateTextBox(string name, string placeholder, int y)
        {
            var textBox = new TextBox
            {
                Name = name,
                Font = new Font("Segoe UI", 12),
                Size = new Size(300, 30),
                Location = new Point((this.ClientSize.Width - 300) / 2, y)
            };

            textBox.Enter += (s, e) => { if (textBox.Text == placeholder) { textBox.Text = ""; textBox.ForeColor = System.Drawing.Color.Black; } };
            textBox.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(textBox.Text)) { textBox.Text = placeholder; textBox.ForeColor = System.Drawing.Color.Gray; } };
            textBox.Text = placeholder;
            textBox.ForeColor = Color.Gray;

            return textBox;
        }

        //generic button to be used for all login buttons
        private Button CreateButton(string text, int y, System.Drawing.Color color)
        {
            return new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 12),
                Size = new Size(300, 50),
                Location = new Point((this.ClientSize.Width - 300) / 2, y),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        // check the user wants to quit before quitting 
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing)
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to close the application?",
                    "Confirm Exit",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        //create a new account 
        private void BtnCreateAccount_Click(object sender, EventArgs e)
        {
            // Open the CreateAccountForm
            var createAccountForm = new CreateAccountForm(_context, _configuration, _passwordHashing);
            createAccountForm.Show();
            this.Hide();
        }

        // allow the user to cancel the login
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            // Display a confirmation dialog with Yes and No options
            DialogResult result = MessageBox.Show(
                "Are you sure you want to close the application?",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            // If the user selects "Yes," close the application
            if (result == DialogResult.Yes)
            {
                Application.Exit(); // Properly exits the application
            }
            // If the user selects "No," simply return to the login form (do nothing)
        }

        //TODO need to do assertion testing but wanted basic formatting to be there 
        //TODO need to implement still 
        private void BtnUpdatePassword_Click(object sender, EventArgs e)
        {
            //if valid credentials, open update form
            //update form is still TODO just getting basic outline there for now 
            var updateCredentialsForm = new UpdateCredentialsForm(_context);
            updateCredentialsForm.Show();
            this.Hide();
        }
    }

}
