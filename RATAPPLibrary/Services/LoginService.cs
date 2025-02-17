namespace RATAPPLibrary.Services
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using RATAPPLibrary.Data.DbContexts;
    using RATAPPLibrary.Data.Models;
    using RATAPPLibrary.Data.Models.Requests;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;

    //TODO need to add interface for testing purposes 
    //public interface ILoginService
    //{
    //    public async Task<LoginResponse> Login(LoginRequest request);

    //}

    public class LoginService
    {
        private readonly RatAppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHashing _passwordHashing;

        public LoginService(RatAppDbContext context, IConfiguration configuration, PasswordHashing passwordHashing)
        {
            _context = context;
            _configuration = configuration;
            _passwordHashing = passwordHashing;
        }

        public async Task<LoginResponse> Login (LoginRequest request)
        {
            //Retrieve the user with their credentials
            var user = await _context.Users
                .Include(u => u.Credentials)  // Ensure the credentials are included in the user query
                .FirstOrDefaultAsync(u => u.Credentials.Username == request.Username);

            if (user == null)
            {
                // User not found
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            //Hash the input password using the same hashing method
            //string hashedInputPassword = _passwordHashing.HashPassword(request.Password);

            //verify password FIXME currently just verifying text to text needs to have hash (salt algorithm) 
            if (!_passwordHashing.VerifyPasswordTempFix(request.Password, user.Credentials.Password)) //FIXME 
            {
                // Password is incorrect
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            //Verify the password hash
            //if (!_passwordHashing.VerifyPassword(hashedInputPassword, user.Credentials.Password))
            //{
            //    // Password is incorrect
            //    throw new UnauthorizedAccessException("Invalid credentials");
            //}

            // If authentication is successful, generate a JWT token
            var token = GenerateJwtToken(user);

            // Create and return the response
            return new LoginResponse
            {
                Token = token,
                Username = user.Credentials.Username,
                Role = "Admin"//user.AccountType.Name // Example: assuming AccountType has a 'Name' property FIXME roles will need to be handled in a seperate story 
            };
        }

        public string GenerateJwtToken(User user)
        {
            // Temporarily hard-code the JWT values for now
            var secretKey = "your_256bit_secret_key_which_is_at_least_32_bytes_long"; // Set your secret key here
            var issuer = "your_issuer_here";         // Set your issuer here
            var audience = "your_audience_here";     // Set your audience here

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, user.Credentials.Username),
            new Claim(ClaimTypes.Role, "Admin" ), // Assuming you have roles FIXME this is not setup yet user.AccountType.Name
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            //var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //var token = new JwtSecurityToken(
            //    issuer: _configuration["Jwt:Issuer"],
            //    audience: _configuration["Jwt:Audience"],
            //    claims: claims,
            //    expires: DateTime.Now.AddHours(1),
            //    signingCredentials: creds);

            var token = new JwtSecurityToken(
      issuer: issuer,
      audience: audience,
      claims: claims,
      expires: DateTime.Now.AddHours(1),
      signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
    }
