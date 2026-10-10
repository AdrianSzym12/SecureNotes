
using SecureNotes.Application.DTOs.Auth;
using SecureNotes.Application.Interfaces.Security;
using SecureNotes.Domain.Repositories;
using SecureNotes.Domain.ValueObjects;

namespace SecureNotes.Application.Services
{
    public class LoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public LoginService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<Guid?> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return null;
            }

            Email email;

            try
            {
                email = new Email(request.Email);
            }
            catch (ArgumentException)
            {
                return null;
            }

            var user = await _userRepository.GetByEmailAsync(
                email,
                cancellationToken);

            if (user == null)
            {
                return null;
            }

            var validPassword = _passwordHasher.VerifyPassword(
                request.Password,
                user.PasswordHash);

            if (!validPassword)
            {
                return null;
            }

            return user.Id;
        }
    }
}
