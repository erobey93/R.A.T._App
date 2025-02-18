using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace RATAPP.Forms
{
    public partial class CreateAccountForm : Form
    {
        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
        private Microsoft.Extensions.Configuration.IConfigurationRoot _configuration;
        private PasswordHashing _passwordHashing;

        public CreateAccountForm(RATAPPLibrary.Data.DbContexts.RatAppDbContext context, Microsoft.Extensions.Configuration.IConfigurationRoot configuration, PasswordHashing passwordHashing)
        {
            InitializeComponent();
            CreateAcctForm();
            _context = context;
            _configuration = configuration;
            _passwordHashing = passwordHashing;
        }

        private void CreateAcctForm()
        {
            // Create and configure the Name label
            Label lblName = new Label();
            lblName.Text = "Name:";
            lblName.Location = new System.Drawing.Point(50, 50);
            lblName.AutoSize = true;

            // Create and configure the Name textbox
            TextBox txtName = new TextBox();
            txtName.Name = "txtName";
            txtName.Location = new System.Drawing.Point(150, 50);
            txtName.Width = 200;

            // Create and configure the Phone label
            Label lblPhone = new Label();
            lblPhone.Text = "Phone:";
            lblPhone.Location = new System.Drawing.Point(50, 100);
            lblPhone.AutoSize = true;

            // Create and configure the Phone textbox
            TextBox txtPhone = new TextBox();
            txtPhone.Name = "txtPhone";
            txtPhone.Location = new System.Drawing.Point(150, 100);
            txtPhone.Width = 200;

            // Create and configure the Email label
            Label lblEmail = new Label();
            lblEmail.Text = "Email:";
            lblEmail.Location = new System.Drawing.Point(50, 150);
            lblEmail.AutoSize = true;

            // Create and configure the Email textbox
            TextBox txtEmail = new TextBox();
            txtEmail.Name = "txtEmail";
            txtEmail.Location = new System.Drawing.Point(150, 150);
            txtEmail.Width = 200;

            // Create and configure the Location label
            Label lblLocation = new Label();
            lblLocation.Text = "Location:";
            lblLocation.Location = new System.Drawing.Point(50, 200);
            lblLocation.AutoSize = true;

            // Create and configure the Location textbox
            TextBox txtLocation = new TextBox();
            txtLocation.Name = "txtLocation";
            txtLocation.Location = new System.Drawing.Point(150, 200);
            txtLocation.Width = 200;

            // Create and configure the Username label
            Label lblUsername = new Label();
            lblUsername.Text = "Username:";
            lblUsername.Location = new System.Drawing.Point(50, 250);
            lblUsername.AutoSize = true;

            // Create and configure the Username textbox
            TextBox txtUsername = new TextBox();
            txtUsername.Name = "txtUsername";
            txtUsername.Location = new System.Drawing.Point(150, 250);
            txtUsername.Width = 200;

            // Create and configure the Password label
            Label lblPassword = new Label();
            lblPassword.Text = "Password:";
            lblPassword.Location = new System.Drawing.Point(50, 300);
            lblPassword.AutoSize = true;

            // Create and configure the Password textbox
            TextBox txtPassword = new TextBox();
            txtPassword.Name = "txtPassword";
            txtPassword.Location = new System.Drawing.Point(150, 300);
            txtPassword.Width = 200;
            txtPassword.PasswordChar = '*'; // Hide password input

            // Create and configure the Account Type label
            Label lblAccountType = new Label();
            lblAccountType.Text = "Account Type:";
            lblAccountType.Location = new System.Drawing.Point(50, 350);
            lblAccountType.AutoSize = true;

            // Create and configure the Account Type textbox
            ComboBox cmbAccountType = new ComboBox();
            cmbAccountType.Name = "cmbAccountType";
            cmbAccountType.Location = new System.Drawing.Point(150, 350);
            cmbAccountType.Width = 200;
            cmbAccountType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAccountType.Items.AddRange(new string[] { "Admin", "User", "Guest" }); // Example options TODO this will be breeder or user, maybe admin

            // Create and configure the Submit button
            Button btnSubmit = new Button();
            btnSubmit.Text = "Create Account";
            btnSubmit.Location = new System.Drawing.Point(150, 400);
            btnSubmit.Click += BtnCreateAccount_Click;

            // Create and configure the Cancel button
            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Location = new System.Drawing.Point(270, 400);
            btnCancel.Click += BtnCancel_Click;

            // Add controls to the form
            this.Controls.Add(lblName);
            this.Controls.Add(txtName);
            this.Controls.Add(lblPhone);
            this.Controls.Add(txtPhone);
            this.Controls.Add(lblEmail);
            this.Controls.Add(txtEmail);
            this.Controls.Add(lblLocation);
            this.Controls.Add(txtLocation);
            this.Controls.Add(lblUsername);
            this.Controls.Add(txtUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(lblAccountType);
            this.Controls.Add(cmbAccountType);
            this.Controls.Add(btnSubmit);
            this.Controls.Add(btnCancel);

            // Optional: Set form properties
            this.Text = "Create Account";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new System.Drawing.Size(450, 500);
        }

        private async void BtnCreateAccount_Click(object sender, EventArgs e)
        {
            // Call method to create the account (passing form controls)
            bool accountCreated = await CreateAccountAsync();

            // Handle success or failure
            if (accountCreated)
            {
                MessageBox.Show("Account created successfully!", "Success");
                BackToLogin();
            }
            else
            {
                MessageBox.Show("Account creation failed. Please try again.", "Error");
            }
        }

        private async Task<bool> CreateAccountAsync()
        {
            // Gather data from the form (directly inside this method)
            // Retrieve user inputs
            string name = ((TextBox)this.Controls["txtName"]).Text;
            string phone = ((TextBox)this.Controls["txtPhone"]).Text;
            string email = ((TextBox)this.Controls["txtEmail"]).Text;
            string location = ((TextBox)this.Controls["txtLocation"]).Text;
            string username = ((TextBox)this.Controls["txtUsername"]).Text;
            string password = ((TextBox)this.Controls["txtPassword"]).Text;
            string accountTypeName = ((ComboBox)this.Controls["cmbAccountType"]).Text;

            // Check if username already exists
            var accountService = new AccountService(_context, _configuration, _passwordHashing); // _context should be your DbContext instance

            //FIXME not using hashing for now as I need to get the app built before wasting more time on password handling 
            // Hash the password before sending it to the account service
            //string hashedPassword = _passwordHashing.HashPassword(password);

            return await accountService.CreateAccountAsync(username, password, email, name, phone, location, accountTypeName);
        }

        private void BackToLogin()
        {
            // Navigate back to the login form
            this.Hide();
            var loginForm = new LoginForm(_context, _configuration, _passwordHashing);
            loginForm.Show();
        }
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            BackToLogin(); 
        }
    }
}
