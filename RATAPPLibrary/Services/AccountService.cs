using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Requests;

namespace RATAPPLibrary.Services
{
    /// <summary>
    /// Service for managing user accounts and profiles in the R.A.T. App.
    /// Handles account creation, breeder profile management, and credential updates.
    /// 
    /// Key Features:
    /// - Account Management:
    ///   * User account creation
    ///   * Breeder profile creation
    ///   * Credential updates
    /// 
    /// Data Structure:
    /// - User accounts contain:
    ///   * Credentials (username/password)
    ///   * Individual info (name, contact details)
    ///   * Account type
    ///   * Optional breeder profile
    /// 
    /// Known Limitations:
    /// - Basic password storage (no hashing)
    /// - Fixed profile image ("xxxx")
    /// - Limited account type management
    /// - Basic error handling
    /// 
    /// Planned Improvements:
    /// - Implement password hashing
    /// - Add proper profile image handling
    /// - Enhance account type management
    /// - Improve error handling and logging
    /// 
    /// Dependencies:
    /// - IConfiguration: For app settings
    /// - PasswordHashing: For credential management
    /// - Inherits from BaseService for database operations
    /// </summary>
    public class AccountService : BaseService
    {
        //private readonly RatAppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHashing _passwordHashing;

        public AccountService(RatAppDbContextFactory contextFactory, IConfiguration configuration, PasswordHashing passwordHashing) : base(contextFactory)
        {
            //_context = context;
            _configuration = configuration;
            _passwordHashing = passwordHashing;
        }

        /// <summary>
        /// Creates a new user account with associated profile information.
        /// 
        /// Process:
        /// 1. Validates username availability
        /// 2. Creates credentials
        /// 3. Creates individual profile
        /// 4. Creates/associates account type
        /// 5. Creates user record
        /// 
        /// Required Information:
        /// - Username and password
        /// - Personal details (name, contact info)
        /// - Location (city, state)
        /// - Account type
        /// 
        /// Note: Currently stores password as plain text
        /// TODO: Implement password hashing before storage
        /// 
        /// Returns: True if account created successfully
        /// </summary>
        public async Task<bool> CreateAccountAsync(string username, string password, string email, string firstName, string lastName, string phone, string city, string state, string accountTypeName)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                // Check if username already exists
                var existingUser = await _context.Users
                .Where(u => u.Credentials.Username == username)
                .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    // Username already exists
                    Console.WriteLine("Username already exists");
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
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = phone,
                    Email = email,
                    City = city,
                    State = state
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
            });
        } 

        /// <summary>
        /// Creates a new breeder profile for an existing user.
        /// 
        /// Process:
        /// 1. Finds user by username
        /// 2. Checks if breeder profile exists
        /// 3. Creates new breeder profile if none exists
        /// 
        /// Note: Currently only supports one breeder profile per user
        /// TODO: Consider supporting multiple breeder profiles
        /// 
        /// Returns: True if breeder profile created successfully
        /// </summary>
        /// <param name="userName">Username of the user to create breeder profile for</param>
        public async Task<bool> CreateNewBreeder(string userName)
        {
            return await ExecuteInTransactionAsync(async _context =>
            {
                //get the new user id 
                var userId = await _context.Users
                .Where(u => u.Credentials.Username == userName)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

                //FIXME this should likely be elsewhere but for now 
                //check if breeder exists, if not create new breeder
                var breeder = await _context.Breeder
                    .Where(b => b.UserId == userId)
                    .FirstOrDefaultAsync();
                if (breeder == null)
                {
                    //create new breeder 
                    breeder = new Data.Models.Breeding.Breeder
                    {
                        UserId = userId
                    };

                    //add breeder to the database
                    _context.Breeder.Add(breeder);
                    return true;
                }
                else
                {
                    //breeder already exists
                    Console.WriteLine("Breeder already exists");
                    return false;
                }
            });

        }
        /// <summary>
        /// Updates user credentials (password).
        /// 
        /// Process:
        /// 1. Validates user exists
        /// 2. Verifies current password
        /// 3. Updates to new password
        /// 
        /// Security Notes:
        /// - Currently uses plain text password comparison
        /// - No password complexity requirements
        /// - No username update support
        /// 
        /// TODO:
        /// - Implement password hashing
        /// - Add password complexity validation
        /// - Add username update capability
        /// - Improve error handling
        /// 
        /// Returns: True if credentials updated successfully
        /// </summary>
        public async Task<bool> UpdateCredentialsAsync(UpdateCredentialsRequest request)
        {
            return await ExecuteInTransactionAsync(async _context =>
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
                if (!passwordHashing.VerifyPasswordTempFix(user.Credentials.Password, request.CurrentPassword))
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
            });
        }
    }
}
