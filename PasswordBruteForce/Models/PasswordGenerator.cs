using System;

namespace PasswordBruteForce.Models
{
    public class PasswordGenerator
    {
        public const string CharacterSet =
            "abcdefghijklmnopqrstuvwxyz0123456789";

        private readonly Random _rng = new();

        public string GeneratePassword()
        {
            int len = _rng.Next(4, 6); // 4 or 5 chars
            char[] buf = new char[len];
            for (int i = 0; i < len; i++)
                buf[i] = CharacterSet[_rng.Next(CharacterSet.Length)];
            return new string(buf);
        }
    }
}