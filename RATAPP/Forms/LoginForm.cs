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
        // Declare the fields to store the dependencies
        private RATAPPLibrary.Data.DbContexts.UserDbContext _context;
        private Microsoft.Extensions.Configuration.IConfigurationRoot _configuration;
        private PasswordHashing _passwordHashing;


        public LoginForm(RATAPPLibrary.Data.DbContexts.UserDbContext context, Microsoft.Extensions.Configuration.IConfigurationRoot configuration, PasswordHashing passwordHashing)
        {
            InitializeComponent();

            // Store the dependencies
            _context = context;
            _configuration = configuration;
            _passwordHashing = passwordHashing;

            // Create the login form controls
            CreateLoginForm();
        }

        private void CreateLoginForm()
        {
            // Set form properties
            this.Text = "RAT App Login";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new System.Drawing.Size(450, 350);
            this.BackColor = System.Drawing.Color.WhiteSmoke;

            // Create and configure the username label
            Label lblUsername = new Label
            {
                Text = "Username:",
                Location = new System.Drawing.Point(50, 50),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            // Create and configure the username textbox
            TextBox txtUsername = new TextBox
            {
                Name = "txtUsername",
                Location = new System.Drawing.Point(150, 50),
                Width = 200,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            // Create and configure the password label
            Label lblPassword = new Label
            {
                Text = "Password:",
                Location = new System.Drawing.Point(50, 100),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            // Create and configure the password textbox
            TextBox txtPassword = new TextBox
            {
                Name = "txtPassword",
                Location = new System.Drawing.Point(150, 100),
                Width = 200,
                PasswordChar = '*',
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            // Create and configure the login button
            Button btnLogin = new Button
            {
                Text = "Login",
                Location = new System.Drawing.Point(150, 150),
                Width = 90,
                Height = 30,
                Font = new System.Drawing.Font("Segoe UI", 9)
            };
            btnLogin.Click += BtnLogin_Click;

            // Create and configure the cancel button
            Button btnCancel = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(260, 150),
                Width = 90,
                Height = 30,
                Font = new System.Drawing.Font("Segoe UI", 9)
            };
            btnCancel.Click += BtnCancel_Click;

            // Create and configure the Create Account button
            Button btnCreateAccount = new Button
            {
                Text = "Create Account",
                Location = new System.Drawing.Point(150, 200),
                Width = 200,
                Height = 30,
                Font = new System.Drawing.Font("Segoe UI", 9)
            };
            btnCreateAccount.Click += BtnCreateAccount_Click;

            // Create and configure the Change Password button
            Button btnChangePassword = new Button
            {
                Text = "Change Password",
                Location = new System.Drawing.Point(150, 250),
                Width = 200,
                Height = 30,
                Font = new System.Drawing.Font("Segoe UI", 9)
            };
            btnChangePassword.Click += BtnUpdatePassword_Click;

            // Add controls to the form
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
            this.Controls.Add(btnCancel);
            this.Controls.Add(btnCreateAccount);
            this.Controls.Add(btnChangePassword);
        }

        // Login to existing account
        // Make the event handler async
        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            // Get the username and password from the text boxes
            string username = ((TextBox)this.Controls["txtUsername"]).Text;
            string password = ((TextBox)this.Controls["txtPassword"]).Text;

            try
            {
                // Create an instance of your login service and call the async Login method
                var loginService = new LoginService(_context, _configuration, _passwordHashing); // Inject dependencies
                var response = await loginService.Login(new RATAPPLibrary.Data.Models.LoginRequest { Username = username, Password = password });

                // On successful login, show a message and navigate to the next form
                MessageBox.Show($"Welcome {response.Username}! Role: {response.Role}", "Login Successful");

                // Get the instance of the main form (RatAppBaseForm)
                var baseForm = Application.OpenForms.OfType<RATAppBaseForm>().FirstOrDefault();

                if (baseForm == null)
                {
                    // If for some reason the base form doesn't exist, create it here
                    baseForm = new RATAppBaseForm();
                }

                // Set user info
                baseForm.SetUserName(response.Username);

                // Depending on the role, show the appropriate panel
                Panel contentPanelToShow = null;

                if (response.Role == "Admin")
                {
                    // Create and show admin-specific panel
                    //TODO decide if I want to do anything differently if logged in as admin, for now logic is the same 
                    var homePanel = new HomePanel(baseForm, response.Username, response.Role); // You will need to create this panel
                    contentPanelToShow = homePanel;
                }
                else if (response.Role == "User")
                {
                    // Create and show user-specific panel
                    var homePanel = new HomePanel(baseForm, response.Username, response.Role); // You will need to create this panel
                    contentPanelToShow = homePanel;
                }

                // Clear current controls in the content panel and add the new panel
                baseForm.contentPanel.Controls.Clear();
                baseForm.contentPanel.Controls.Add(contentPanelToShow);  // Add the new panel

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

        //create a new account 
        private void BtnCreateAccount_Click(object sender, EventArgs e)
        {
            // Open the CreateAccountForm
            var createAccountForm = new CreateAccountForm(_context, _configuration, _passwordHashing);
            createAccountForm.Show();
            this.Hide();
        }

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
        private async void BtnUpdatePassword_Click(object sender, EventArgs e)
        {
            //first, make sure that they've entered valid credentials
            //text boxes should be populated
            

            //if valid credentials, open update form
            //updat form is still TODO just getting basic outline there for now 
            var updateCredentialsForm = new UpdateCredentialsForm(_context);
            updateCredentialsForm.Show();
            this.Hide();
        }
    }
}
