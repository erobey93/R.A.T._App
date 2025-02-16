using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Requests;

namespace RATAPPLibrary.Services
{
    public class AccountService
    {
        private readonly UserDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHashing _passwordHashing;

        public AccountService(UserDbContext context, IConfiguration configuration, PasswordHashing passwordHashing)
        {
            _context = context;
            _configuration = configuration;
            _passwordHashing = passwordHashing;
        }

        // Method to create a new user account
        public async Task<bool> CreateAccountAsync(string username, string password, string email, string name, string phone, string location, string accountTypeName)
        {
            // Check if username already exists
            var existingUser = await _context.Users
                .Where(u => u.Credentials.Username == username)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                // Username already exists
                Console.WriteLine("Username already existst");
                return false;
            }

            // Create new credentials
            var credentials = new Credentials
            {
                Username = username,
                Password = password // Ideally, hash this password
            };

            // Create new individual user data
            var individual = new Individual
            {
                Name = name,
                Phone = phone,
                Email = email,
                Location = location
            };

            // Create new account type (if not exists, you might want to have logic for creating account types as well)
            var accountType = await _context.AccountTypes
                .Where(at => at.Name == accountTypeName)
                .FirstOrDefaultAsync();

            if (accountType == null)
            {
                // Account type doesn't exist, create new account type
                accountType = new AccountType
                {
                    Name = accountTypeName
                };

                _context.AccountTypes.Add(accountType);
                await _context.SaveChangesAsync(); // Save the new account type to the database
            }

            // Create new user
            var user = new User
            {
                Credentials = credentials,
                AccountType = accountType,
                Individual = individual,
                ImagePath = "xxxx" //FIXME probably use the RAT logo or something 
            };

            _context.Users.Add(user);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log or handle exception
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdateCredentialsAsync(UpdateCredentialsRequest request)
        {
            PasswordHashing passwordHashing = new PasswordHashing();

            //Retrieve the user with their credentials
            var user = await _context.Users
                .Include(u => u.Credentials)  // Ensure the credentials are included in the user query
                .FirstOrDefaultAsync(u => u.Credentials.Username == request.Username);

            if (user == null)
            {
                // User not found
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            // Verify the old password matches the stored password
            if (passwordHashing.VerifyPasswordTempFix(user.Credentials.Password, request.CurrentPassword))
            {
                // Old password is incorrect
                Console.WriteLine("Old password is incorrect");
                return false;
            }

            // Update the password (ensure you hash the new password)
            //TODO should also include a username update option 
            user.Credentials.Password = request.NewPassword;//_passwordHashing.HashPassword(request.NewPassword); TODO implement hashing logic/secure log in

            try
            {
                // Save changes to the database
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log or handle exception
                Console.WriteLine($"Error updating credentials: {ex.Message}");
                return false;
            }
        }
    }
}
