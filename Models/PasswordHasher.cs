using System;
using System.Security.Cryptography;
using System.Text;

namespace PasswordBruteForce.Models
{
    public static class PasswordHasher
    {
        private const string SALT = "SALT123";

        public static string Hash(string password)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(SALT + password);
            byte[] hash  = SHA256.HashData(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public static bool Verify(string password, string storedHash)
        {
            return Hash(password) == storedHash;
        }

        public static string GetSalt() => SALT;
    }
}