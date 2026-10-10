
using SecureNotes.Application.DTOs.Auth;
using SecureNotes.Application.Services;
using SecureNotes.Infrastructure.Persistence;
using SecureNotes.Infrastructure.Persistence.Repositories;
using SecureNotes.Infrastructure.Security;
using SecureNotes.Infrastructure.Tests.Fixtures;
using SecureNotes.Domain.ValueObjects;
using SecureNotes.Application.Exceptions;

namespace SecureNotes.Infrastructure.Tests.Services
{
    public class RegisterServiceTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public RegisterServiceTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        private RegisterService CreateService()
        {
            var context = _fixture.Context;

            return new RegisterService(
                new UserRepository(context),
                new PasswordHasher(),
                new UnitOfWork(context));
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUser()
        {
            // Arrange
            var service = CreateService();

            var email = $"register-{Guid.NewGuid():N}@example.com";
            var request = new RegisterRequest(
                email,
                "SecureNotes!123");

            // Act
            var userId = await service.RegisterAsync(request);

            _fixture.Context.ChangeTracker.Clear();

            var repository = new UserRepository(_fixture.Context);
            var savedUser = await repository.GetByIdAsync(userId);

            // Assert
            Assert.NotNull(savedUser);
            Assert.Equal(new Email(email), savedUser.Email);
        }

        [Fact]
        public async Task RegisterAsync_ShouldStorePasswordHash()
        {
            // Arrange
            var service = CreateService();

            var password = "SecureNotes!123";
            var request = new RegisterRequest(
                $"hash-{Guid.NewGuid():N}@example.com",
                password);

            // Act
            var userId = await service.RegisterAsync(request);

            _fixture.Context.ChangeTracker.Clear();

            var repository = new UserRepository(_fixture.Context);
            var savedUser = await repository.GetByIdAsync(userId);

            // Assert
            Assert.NotNull(savedUser);
            Assert.NotEqual(password, savedUser.PasswordHash);

            var hasher = new PasswordHasher();

            Assert.True(hasher.VerifyPassword(
                password,
                savedUser.PasswordHash));
        }

        [Fact]
        public async Task RegisterAsync_ShouldRejectDuplicateEmail()
        {
            // Arrange
            var service = CreateService();

            var email = $"duplicate-{Guid.NewGuid():N}@example.com";
            var request = new RegisterRequest(
                email,
                "SecureNotes!123");

            await service.RegisterAsync(request);

            _fixture.Context.ChangeTracker.Clear();

            // Act & Assert
            await Assert.ThrowsAsync<DuplicateEmailException>(
                () => service.RegisterAsync(request));
        }

        [Fact]
        public async Task RegisterAsync_ShouldRejectShortPassword()
        {
            // Arrange
            var service = CreateService();

            var request = new RegisterRequest(
                $"short-{Guid.NewGuid():N}@example.com",
                "Short123!");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.RegisterAsync(request));
        }
    }
}
