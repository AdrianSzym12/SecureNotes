
using SecureNotes.Application.DTOs.Auth;
using SecureNotes.Application.Services;
using SecureNotes.Infrastructure.Persistence;
using SecureNotes.Infrastructure.Persistence.Repositories;
using SecureNotes.Infrastructure.Security;
using SecureNotes.Infrastructure.Tests.Fixtures;

namespace SecureNotes.Infrastructure.Tests.Services
{
    public class LoginServiceTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public LoginServiceTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        private LoginService CreateService()
        {
            return new LoginService(
                new UserRepository(_fixture.Context),
                new PasswordHasher());
        }

        private async Task<(Guid UserId, string Email, string Password)>
            CreateTestUserAsync()
        {
            var email = $"login-{Guid.NewGuid():N}@example.com";
            var password = "SecureNotes!2026Test";

            var registerService = new RegisterService(
                new UserRepository(_fixture.Context),
                new PasswordHasher(),
                new UnitOfWork(_fixture.Context));

            var userId = await registerService.RegisterAsync(
                new RegisterRequest(email, password));

            _fixture.Context.ChangeTracker.Clear();

            return (userId, email, password);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnUserId_ForCorrectCredentials()
        {
            // Arrange
            var (userId, email, password) =
                await CreateTestUserAsync();

            var service = CreateService();

            // Act
            var result = await service.LoginAsync(
                new LoginRequest(email, password));

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Value);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_ForWrongPassword()
        {
            // Arrange
            var (_, email, _) = await CreateTestUserAsync();
            var service = CreateService();

            // Act
            var result = await service.LoginAsync(
                new LoginRequest(email, "WrongPassword!123"));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_ForUnknownEmail()
        {
            // Arrange
            var service = CreateService();

            var email =
                $"unknown-{Guid.NewGuid():N}@example.com";

            // Act
            var result = await service.LoginAsync(
                new LoginRequest(
                    email,
                    "SecureNotes!2026Test"));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_ForInvalidEmail()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.LoginAsync(
                new LoginRequest(
                    "invalid-email",
                    "SecureNotes!2026Test"));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_ForEmptyPassword()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.LoginAsync(
                new LoginRequest(
                    "test@example.com",
                    ""));

            // Assert
            Assert.Null(result);
        }
    }
}
