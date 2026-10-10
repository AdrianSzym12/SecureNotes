
using SecureNotes.Domain.Entities;
using SecureNotes.Domain.ValueObjects;
using SecureNotes.Infrastructure.Persistence.Repositories;
using SecureNotes.Infrastructure.Tests.Fixtures;

namespace SecureNotes.Infrastructure.Tests.Repositories
{
    public class UserRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public UserRepositoryTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task AddAsync_ShouldSaveUserToDatabase()
        {
            // Arrange
            var context = _fixture.Context;
            var repository = new UserRepository(context);

            var email = new Email(
                $"test-{Guid.NewGuid():N}@example.com");

            var user = new User(
                email,
                "test-password-hash");

            // Act
            await repository.AddAsync(user);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();

            var savedUser = await repository.GetByIdAsync(user.Id);

            // Assert
            Assert.NotNull(savedUser);
            Assert.Equal(user.Id, savedUser.Id);
            Assert.Equal(email, savedUser.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnExistingUser()
        {
            // Arrange
            var context = _fixture.Context;
            var repository = new UserRepository(context);

            var email = new Email(
                $"test-{Guid.NewGuid():N}@example.com");

            var user = new User(email, "test-password-hash");

            await repository.AddAsync(user);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();

            // Act
            var result = await repository.GetByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(email, result.Email);
        }

        [Fact]
        public async Task ExistsByEmailAsync_ShouldReturnTrueForExistingUser()
        {
            // Arrange
            var context = _fixture.Context;
            var repository = new UserRepository(context);

            var email = new Email(
                $"test-{Guid.NewGuid():N}@example.com");

            var user = new User(email, "test-password-hash");

            await repository.AddAsync(user);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();

            // Act
            var exists = await repository.ExistsByEmailAsync(email);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var repository = new UserRepository(_fixture.Context);
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await repository.GetByIdAsync(nonExistingId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var repository = new UserRepository(_fixture.Context);

            var email = new Email(
                $"missing-{Guid.NewGuid():N}@example.com");

            // Act
            var result = await repository.GetByEmailAsync(email);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ExistsByEmailAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Arrange
            var repository = new UserRepository(_fixture.Context);

            var email = new Email(
                $"missing-{Guid.NewGuid():N}@example.com");

            // Act
            var exists = await repository.ExistsByEmailAsync(email);

            // Assert
            Assert.False(exists);
        }

    }
}
