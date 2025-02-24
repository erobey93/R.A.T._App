using Moq;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Services;
using RATAPPLibrary.Data.Models;
using Xunit;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace RATAPPLibraryUT
{
    public class LoginServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<RatAppDbContext> _mockDbContext;
        private readonly Mock<PasswordHashing> _mockPasswordHashing;
        private readonly LoginService _loginService;
        private readonly string _jwtSecretKey = "secretkey12345";
        private readonly string _jwtIssuer = "http://issuer.com";
        private readonly string _jwtAudience = "http://audience.com";

        public LoginServiceTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(config => config["Jwt:SecretKey"]).Returns(_jwtSecretKey);
            _mockConfig.Setup(config => config["Jwt:Issuer"]).Returns(_jwtIssuer);
            _mockConfig.Setup(config => config["Jwt:Audience"]).Returns(_jwtAudience);

            _mockPasswordHashing = new Mock<PasswordHashing>();
            _mockPasswordHashing.Setup(ph => ph.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            _mockDbContext = new Mock<RatAppDbContext>();
            _loginService = new LoginService(_mockDbContext.Object, _mockConfig.Object, _mockPasswordHashing.Object);
        }

        //[Fact]
        //public void Login_ValidCredentials_ReturnsToken()
        //{
        //    // Arrange
        //    var request = new LoginRequest { Username = "testUser", Password = "validPassword" };

        //    var user = new User
        //    {
        //        Id = 1,
        //        Credentials = new Credentials { Username = "testUser", Password = "validPassword" },
        //        AccountType = new AccountType { Name = "Admin" }
        //    };

        //    _mockDbContext.Setup(db => db.Users.FirstOrDefault(It.IsAny<Func<User, bool>>()))
        //        .Returns(user);

        //    // Mock password verification to return true
        //    _mockPasswordHashing.Setup(ph => ph.VerifyPassword(request.Password, user.Credentials.Password)).Returns(true);

        //    // Act
        //    var response = _loginService.Login(request);

        //    // Assert
        //    Assert.NotNull(response);
        //    Assert.Equal("testUser", response.Username);
        //    Assert.Equal("Admin", response.Role);
        //    Assert.NotNull(response.Token);
        //}

        //[Fact]
        //public void Login_InvalidUsername_ThrowsUnauthorizedAccessException()
        //{
        //    // Arrange
        //    var request = new LoginRequest { Username = "wrongUser", Password = "anyPassword" };

        //    _mockDbContext.Setup(db => db.Users.FirstOrDefault(It.IsAny<Func<User, bool>>()))
        //        .Returns((User)null); // User not found

        //    // Act & Assert
        //    Assert.Throws<UnauthorizedAccessException>(() => _loginService.Login(request));
        //}

        //[Fact]
        //public void Login_InvalidPassword_ThrowsUnauthorizedAccessException()
        //{
        //    // Arrange
        //    var request = new LoginRequest { Username = "testUser", Password = "wrongPassword" };

        //    var user = new User
        //    {
        //        Id = 1,
        //        Credentials = new Credentials { Username = "testUser", Password = "validPassword" },
        //        AccountType = new AccountType { Name = "Admin" }
        //    };

        //    _mockDbContext.Setup(db => db.Users.FirstOrDefault(It.IsAny<Func<User, bool>>()))
        //        .Returns(user);

        //    // Mock password verification to return false
        //    _mockPasswordHashing.Setup(ph => ph.VerifyPassword(request.Password, user.Credentials.Password)).Returns(false);

        //    // Act & Assert
        //    Assert.Throws<UnauthorizedAccessException>(() => _loginService.Login(request));
        //}

        //[Fact]
        //public void Login_GeneratesValidJwtToken()
        //{
        //    // Arrange
        //    var request = new LoginRequest { Username = "testUser", Password = "validPassword" };

        //    var user = new User
        //    {
        //        Id = 1,
        //        Credentials = new Credentials { Username = "testUser", Password = "validPassword" },
        //        AccountType = new AccountType { Name = "Admin" }
        //    };

        //    _mockDbContext.Setup(db => db.Users.FirstOrDefault(It.IsAny<Func<User, bool>>()))
        //        .Returns(user);

        //    // Mock password verification to return true
        //    _mockPasswordHashing.Setup(ph => ph.VerifyPassword(request.Password, user.Credentials.Password)).Returns(true);

        //    // Act
        //    var response = _loginService.Login(request);

        //    // Assert JWT Token structure
        //    var handler = new JwtSecurityTokenHandler();
        //    var jsonToken = handler.ReadToken(response.Token) as JwtSecurityToken;

        //    Assert.NotNull(jsonToken);
        //    Assert.Equal("testUser", jsonToken?.Claims.First(c => c.Type == ClaimTypes.Name).Value);
        //    Assert.Equal("Admin", jsonToken?.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        //}

        [Fact]
        public void GenerateJwtToken_ValidUser_CreatesValidToken()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Credentials = new Credentials { Username = "testUser", Password = "validPassword" },
                AccountType = new AccountType { Name = "Admin" }
            };

            var token = _loginService.GenerateJwtToken(user);

            // Act & Assert
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            Assert.NotNull(jsonToken);
            Assert.Equal("testUser", jsonToken?.Claims.First(c => c.Type == ClaimTypes.Name).Value);
            Assert.Equal("Admin", jsonToken?.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        }
    }
}