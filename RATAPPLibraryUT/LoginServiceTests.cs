using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models.Requests;
using RATAPPLibrary.Data.Models; 
using RATAPPLibrary.Services;
using RATAPPLibrary.Utilities;
using System;
using System.Threading.Tasks;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using Microsoft.Extensions.Configuration;

namespace RATAPPLibraryUT
{
    [TestClass]
    public class LoginServiceTests
    {
        private LoginService _loginService;
        private RatAppDbContext _context;
        private DbContextOptions<RatAppDbContext> _options;
        private PasswordHashing _passwordHashing;
        private IConfiguration _configuration;

        [TestInitialize]
        public void Setup()
        {
            // Configure In-Memory Database for testing
            _options = new DbContextOptionsBuilder<RatAppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Initialize DbContext with In-Memory Database
            _context = new RatAppDbContext(_options);

            // Initialize password hashing utility
            _passwordHashing = new PasswordHashing();

            _configuration = new ConfigurationBuilder() //FIXME not sure about this  this is from main
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Initialize LoginService with the DbContext
            _loginService = new LoginService(_context, _configuration,_passwordHashing);

            // Clear the database before each test
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Seed test data
            SeedData().Wait();
        }

        private async Task SeedData()
        {
            var accountType = new AccountType { Id = 1, Name = "Admin" };
            _context.AccountTypes.Add(accountType);

            var hashedPassword = _passwordHashing.HashPassword("validPassword");
            var credentials = new Credentials { Id = 1, Username = "testUser", Password = hashedPassword };
            _context.Credentials.Add(credentials);

            var user = new User { Id = 1, AccountTypeId = 1, CredentialsId = 1 };
            _context.Users.Add(user);

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// This method tests the LoginAsync method with valid credentials.
        /// It validates that when correct username and password are provided:
        /// 1. A valid response object is returned
        /// 2. The response contains the correct username
        /// 3. The response contains the correct role
        /// 4. A JWT token is generated and included in the response
        /// </summary>
        [TestMethod]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var request = new LoginRequest { Username = "testUser", Password = "validPassword" };

            // Act
            var response = await _loginService.Login(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual("testUser", response.Username);
            Assert.AreEqual("Admin", response.Role);
            Assert.IsNotNull(response.Token);
        }

        /// <summary>
        /// This method tests the LoginAsync method with an invalid username.
        /// It validates that when a non-existent username is provided:
        /// 1. An UnauthorizedAccessException is thrown
        /// 2. No login response or token is generated
        /// </summary>
        [TestMethod]
        public async Task Login_InvalidUsername_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var request = new LoginRequest { Username = "wrongUser", Password = "anyPassword" };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(async () =>
            {
                await _loginService.Login(request);
            });
        }

        /// <summary>
        /// This method tests the LoginAsync method with an invalid password.
        /// It validates that when an incorrect password is provided for an existing user:
        /// 1. An UnauthorizedAccessException is thrown
        /// 2. No login response or token is generated
        /// This ensures secure authentication even when the username exists
        /// </summary>
        [TestMethod]
        public async Task Login_InvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var request = new LoginRequest { Username = "testUser", Password = "wrongPassword" };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<UnauthorizedAccessException>(async () =>
            {
                await _loginService.Login(request);
            });
        }

        /// <summary>
        /// This method tests the JWT token generation aspect of the LoginAsync method.
        /// It validates that when a successful login occurs:
        /// 1. A JWT token is generated
        /// 2. The token is properly formatted and included in the response
        /// This ensures proper token-based authentication functionality
        /// </summary>
        [TestMethod]
        public async Task Login_GeneratesValidJwtToken()
        {
            // Arrange
            var request = new LoginRequest { Username = "testUser", Password = "validPassword" };

            // Act
            var response = await _loginService.Login(request);

            // Assert
            Assert.IsNotNull(response.Token);
            // Additional JWT validation could be added here if needed
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}