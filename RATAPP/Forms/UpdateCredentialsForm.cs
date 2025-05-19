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
        //private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
        private Microsoft.Extensions.Configuration.IConfigurationRoot _configuration;
        private PasswordHashing _passwordHashing;
        private RatAppDbContextFactory _contextFactory;

        public UpdateCredentialsForm(RatAppDbContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
            InitializeComponent();
            CreateUpdateCredentialsForm();

            // Add form closing confirmation
            this.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    DialogResult result = MessageBox.Show(
                        "Are you sure you want to cancel changing your password?",
                        "Confirm Exit",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (result == DialogResult.No)
                    {
                        e.Cancel = true;
                    }
                }
            };
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

        private TextBox CreateTextBox(string name, string placeholder, int y)
        {
            var textBox = new TextBox
            {
                Name = name,
                Font = new Font("Segoe UI", 12),
                Size = new Size(300, 30),
                Location = new Point((this.ClientSize.Width - 300) / 2, y)
            };

            textBox.Enter += (s, e) => { if (textBox.Text == placeholder) { textBox.Text = ""; textBox.ForeColor = Color.Black; } };
            textBox.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(textBox.Text)) { textBox.Text = placeholder; textBox.ForeColor = Color.Gray; } };
            textBox.Text = placeholder;
            textBox.ForeColor = Color.Gray;

            return textBox;
        }

        private Button CreateButton(string text, int y, Color color)
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

        private void CreateUpdateCredentialsForm()
        {
            // Set form properties
            this.Text = "Change Password";
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

            // Create labels
            var lblCurrentUsername = CreateLabel("Current Username", 300);
            var lblCurrentPassword = CreateLabel("Current Password", 370);
            var lblNewPassword = CreateLabel("New Password", 440);
            var lblConfirmPassword = CreateLabel("Confirm New Password", 510);

            // Create textboxes
            var txtCurrentUsername = CreateTextBox("txtCurrentUsername", "Current Username", 260);
            var txtCurrentPassword = CreateTextBox("txtCurrentPassword", "Current Password", 330);
            txtCurrentPassword.PasswordChar = '•';
            var txtNewPassword = CreateTextBox("txtNewPassword", "New Password", 400);
            txtNewPassword.PasswordChar = '•';
            var txtConfirmPassword = CreateTextBox("txtConfirmPassword", "Confirm New Password", 470);
            txtConfirmPassword.PasswordChar = '•';

            // Create buttons
            var btnSave = CreateButton("Save Changes", 540, Color.FromArgb(0, 120, 215));
            btnSave.Click += (sender, e) => SaveChanges(txtCurrentUsername.Text, txtCurrentPassword.Text, txtNewPassword.Text, txtConfirmPassword.Text);

            var btnCancel = CreateButton("Cancel", 590, Color.FromArgb(158, 158, 158));
            btnCancel.Click += (sender, e) => this.Close();

            // Add controls to the form
            this.Controls.AddRange(new Control[] { 
                logo,
                lblCurrentUsername, txtCurrentUsername,
                lblCurrentPassword, txtCurrentPassword,
                lblNewPassword, txtNewPassword,
                lblConfirmPassword, txtConfirmPassword,
                btnSave, btnCancel
            });
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

            var accountService = new AccountService(_contextFactory, _configuration, _passwordHashing); // _context should be your DbContext instance

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
