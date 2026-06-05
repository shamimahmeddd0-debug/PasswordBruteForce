using PasswordBruteForce.Models;

namespace PasswordBruteForce.Services
{
    public class PasswordValidator
    {
        private readonly string _hash;
        public PasswordValidator(string hash) { _hash = hash; }

        public bool IsMatch(string candidate)
            => PasswordHasher.Verify(candidate, _hash);
    }
}