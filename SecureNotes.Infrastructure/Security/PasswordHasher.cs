
using Microsoft.AspNetCore.Identity;
using SecureNotes.Application.Interfaces.Security;

namespace SecureNotes.Infrastructure.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<object> _passwordHasher;

        public PasswordHasher()
        {
            _passwordHasher = new PasswordHasher<object>();
        }

        public string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(
                null!,
                password);
        }

        public bool VerifyPassword(
            string password,
            string passwordHash)
        {
            var result = _passwordHasher.VerifyHashedPassword(
                null!,
                passwordHash,
                password);

            return result != PasswordVerificationResult.Failed;
        }
    }
}
