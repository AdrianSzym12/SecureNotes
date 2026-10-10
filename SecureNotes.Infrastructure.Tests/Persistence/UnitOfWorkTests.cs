
using Microsoft.EntityFrameworkCore;
using SecureNotes.Application.Exceptions;
using SecureNotes.Domain.Entities;
using SecureNotes.Domain.ValueObjects;
using SecureNotes.Infrastructure.Persistence;
using SecureNotes.Infrastructure.Persistence.Repositories;
using SecureNotes.Infrastructure.Tests.Fixtures;

namespace SecureNotes.Infrastructure.Tests.Persistence
{
    public class UnitOfWorkTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public UnitOfWorkTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldThrowDuplicateEmailException_WhenEmailAlreadyExists()
        {
            // Arrange
            var context = _fixture.Context;
            var repository = new UserRepository(context);
            var unitOfWork = new UnitOfWork(context);

            var email = new Email(
                $"duplicate-index-{Guid.NewGuid():N}@example.com");

            var firstUser = new User(
                email,
                "test-password-hash");

            await repository.AddAsync(firstUser);
            await unitOfWork.SaveChangesAsync();

            context.ChangeTracker.Clear();

            // Tworzymy drugiego użytkownika z tym samym e-mailem,
            // ale innym identyfikatorem.
            var secondUser = new User(
                email,
                "another-test-password-hash");

            await repository.AddAsync(secondUser);

            // Act & Assert
            await Assert.ThrowsAsync<DuplicateEmailException>(
                () => unitOfWork.SaveChangesAsync());

            // Usuwamy z ChangeTrackera niezapisany obiekt.
            context.ChangeTracker.Clear();

            // Dodatkowe sprawdzenie: w bazie jest tylko pierwszy użytkownik.
            var savedUser = await repository.GetByEmailAsync(email);

            Assert.NotNull(savedUser);
            Assert.Equal(firstUser.Id, savedUser.Id);
        }
    }
}
