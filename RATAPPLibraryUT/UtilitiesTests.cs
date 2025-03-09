//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace RATAPPLibraryUT
//{
//    public class UtilitiesTests
//    {
//        private const int SaltSize = 32;  // Example salt size
//        private const int HashSize = 32;  // Example hash size

//        private readonly PasswordHashing _passwordHashing;

//        public UtilitiesTests()
//        {
//            _passwordHashing = new PasswordHashing(); // Assuming this is your class that has the methods
//        }
        
//        [Fact]
//        public void HashPassword_ShouldReturnHash()
//        {
//            // Arrange
//            string password = "SecurePassword123!";

//            // Act
//            string hashedPassword = _passwordHashing.HashPassword(password);

//            // Assert
//            Assert.False(string.IsNullOrEmpty(hashedPassword), "Hashed password should not be null or empty.");
//            Assert.Equal(Convert.FromBase64String(hashedPassword).Length, HashSize);  // Ensure combined salt and hash size is correct
//        }

//        [Fact]
//        public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatches()
//        {
//            // Arrange
//            string password = "SecurePassword123!";
//            string hashedPassword = _passwordHashing.HashPassword(password);

//            // Act
//            bool isValid = _passwordHashing.VerifyPassword(hashedPassword, password);

//            // Assert
//            Assert.True(isValid, "Password should be valid when hashes match.");
//        }

//        [Fact]
//        public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatch()
//        {
//            // Arrange
//            string password = "SecurePassword123!";
//            string wrongPassword = "WrongPassword!123";
//            string hashedPassword = _passwordHashing.HashPassword(password);

//            // Act
//            bool isValid = _passwordHashing.VerifyPassword(hashedPassword, wrongPassword);

//            // Assert
//            Assert.False(isValid, "Password should be invalid when hashes do not match.");
//        }

//        [Fact]
//        public void HashPassword_ShouldReturnDifferentHashes_ForDifferentPasswords()
//        {
//            // Arrange
//            string password1 = "Password1!";
//            string password2 = "Password2!";

//            // Act
//            string hashedPassword1 = _passwordHashing.HashPassword(password1);
//            string hashedPassword2 = _passwordHashing.HashPassword(password2);

//            // Assert
//            Assert.NotEqual(hashedPassword1, hashedPassword2);  // Hashes should be different for different passwords
//        }

//        [Fact]
//        public void VerifyPassword_ShouldReturnFalse_WhenPasswordAndHashAreMismatched()
//        {
//            // Arrange
//            string password = "Password123";
//            string hashedPassword = _passwordHashing.HashPassword(password);

//            // Act
//            bool result = _passwordHashing.VerifyPassword(hashedPassword, "DifferentPassword");

//            // Assert
//            Assert.False(result, "Password verification should return false for mismatched password.");
//        }

//        [Fact]
//        public void HashPassword_ShouldReturnSameHash_ForSameInput()
//        {
//            // Arrange
//            string password = "TestPassword";

//            // Act
//            string hashedPassword1 = _passwordHashing.HashPassword(password);
//            string hashedPassword2 = _passwordHashing.HashPassword(password);

//            // Assert
//            Assert.Equal(hashedPassword1, hashedPassword2);  // The same password should result in the same hash
//        }
//    }
//}
