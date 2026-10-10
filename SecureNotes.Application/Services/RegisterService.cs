
using SecureNotes.Application.DTOs.Auth;
using SecureNotes.Application.Interfaces.Persistence;
using SecureNotes.Application.Interfaces.Security;
using SecureNotes.Domain.Entities;
using SecureNotes.Domain.Repositories;
using SecureNotes.Domain.ValueObjects;
using SecureNotes.Application.Exceptions;

namespace SecureNotes.Application.Services
{
    public class RegisterService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.Password) ||
                request.Password.Length < 12 ||
                request.Password.Length > 128)
            {
                throw new ArgumentException(
                    "Password must contain between 12 and 128 characters.");
            }

            var email = new Email(request.Email);

            if (await _userRepository.ExistsByEmailAsync(
                email,
                cancellationToken))
            {
                throw new DuplicateEmailException();
            }

            var passwordHash = _passwordHasher.HashPassword(
                request.Password);

            var user = new User(email, passwordHash);

            await _userRepository.AddAsync(
                user,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);

            return user.Id;
        }
    }
}
