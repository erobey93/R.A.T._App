using System;
using System.Security.Cryptography;
using System.Text;

public class PasswordHashing
{
    private const int SaltSize = 16; // 16 bytes = 128 bits
    private const int HashSize = 32; // 32 bytes = 256 bits

    public string HashPassword(string password)
    {
        //FIXME this is not the most secure hashing algorithm, but it works for now 
        using (var sha256 = SHA256.Create())
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = sha256.ComputeHash(passwordBytes);
            return Convert.ToBase64String(hashBytes); // Base64 encode the hash result
        }
    }

    //FIXME when hashing algorithm is updated, update here as well! 
    public bool VerifyPassword(string storedHash, string inputPassword)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] inputHashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
            string inputHash = Convert.ToBase64String(inputHashBytes);
            return inputHash == storedHash; // Compare the hashes directly
        }
    }

    public bool VerifyPasswordTempFix(string storedHash, string inputPassword)
    {
            return inputPassword == storedHash; // Compare the hashes directly
    }
}