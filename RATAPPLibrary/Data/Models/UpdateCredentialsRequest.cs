﻿namespace RATAPPLibrary.Data.Models
{
    public class UpdateCredentialsRequest
    {
        public UpdateCredentialsRequest(string username, string currentPassword, string newPassword)
        {
            Username = username;
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
        }

        public string Username { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string CurrentPassword { get; }
    }
}
