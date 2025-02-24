using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Requests;
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

namespace RATAPP.Forms
{
    public partial class UpdateCredentialsForm : Form
    {
        // Declare the fields to store the dependencies
        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
        private Microsoft.Extensions.Configuration.IConfigurationRoot _configuration;
        private PasswordHashing _passwordHashing;

        public UpdateCredentialsForm(RatAppDbContext context)
        {
            _context = context;
            InitializeComponent();
            CreateUpdateCredentialsForm(); 
        }

        private void CreateUpdateCredentialsForm()
        {
            // Set form properties
            this.Text = "Change Password";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new System.Drawing.Size(400, 300);
            this.BackColor = System.Drawing.Color.WhiteSmoke;

            Label lblCurrentUsername = new Label
            {
                Text = "Current Username:",
                Location = new System.Drawing.Point(50, 50),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            TextBox txtCurrentUsername = new TextBox
            {
                Name = "txtCurrentUsername",
                Location = new System.Drawing.Point(200, 50),
                Width = 150,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            // Create and configure labels and textboxes
            Label lblCurrentPassword = new Label
            {
                Text = "Current Password:",
                Location = new System.Drawing.Point(50, 100),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            TextBox txtCurrentPassword = new TextBox
            {
                Name = "txtCurrentPassword",
                Location = new System.Drawing.Point(200, 100),
                Width = 150,
                PasswordChar = '*',
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            Label lblNewPassword = new Label
            {
                Text = "New Password:",
                Location = new System.Drawing.Point(50, 150),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            TextBox txtNewPassword = new TextBox
            {
                Name = "txtNewPassword",
                Location = new System.Drawing.Point(200, 150),
                Width = 150,
                PasswordChar = '*',
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            Label lblConfirmPassword = new Label
            {
                Text = "Confirm New Password:",
                Location = new System.Drawing.Point(50, 200),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            TextBox txtConfirmPassword = new TextBox
            {
                Name = "txtConfirmPassword",
                Location = new System.Drawing.Point(200, 200),
                Width = 150,
                PasswordChar = '*',
                Font = new System.Drawing.Font("Segoe UI", 10)
            };

            // Create Save button
            Button btnSave = new Button
            {
                Text = "Save",
                Location = new System.Drawing.Point(200, 250),
                Width = 80,
                Height = 30,
                Font = new System.Drawing.Font("Segoe UI", 9)
            };
            btnSave.Click += (sender, e) => SaveChanges(txtCurrentUsername.Text, txtCurrentPassword.Text, txtNewPassword.Text, txtConfirmPassword.Text);

            // Create Cancel button
            Button btnCancel = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(300, 250),
                Width = 80,
                Height = 30,
                Font = new System.Drawing.Font("Segoe UI", 9)
            };
            btnCancel.Click += (sender, e) => this.Close();

            // Add controls to the form
            this.Controls.Add(lblCurrentUsername);
            this.Controls.Add(txtCurrentUsername);
            this.Controls.Add(lblCurrentPassword);
            this.Controls.Add(txtCurrentPassword);
            this.Controls.Add(lblNewPassword);
            this.Controls.Add(txtNewPassword);
            this.Controls.Add(lblConfirmPassword);
            this.Controls.Add(txtConfirmPassword);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private async void SaveChanges(string username, string currentPassword, string newPassword, string confirmPassword)
        {

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("All fields are required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("New Password and Confirm Password do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
          
            var accountService = new AccountService(_context, _configuration, _passwordHashing); // _context should be your DbContext instance

            UpdateCredentialsRequest request = new UpdateCredentialsRequest(username, currentPassword, newPassword);
            bool success = await accountService.UpdateCredentialsAsync(request);

            if (success)
            {
                MessageBox.Show("Password updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Failed to update password. Ensure the username and old password are correct.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
