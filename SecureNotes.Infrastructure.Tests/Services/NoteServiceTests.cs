
using SecureNotes.Application.DTOs.Auth;
using SecureNotes.Application.DTOs.Notes;
using SecureNotes.Application.Services;
using SecureNotes.Infrastructure.Persistence;
using SecureNotes.Infrastructure.Persistence.Repositories;
using SecureNotes.Infrastructure.Security;
using SecureNotes.Infrastructure.Tests.Fixtures;

namespace SecureNotes.Infrastructure.Tests.Services
{
    public class NoteServiceTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public NoteServiceTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        private async Task<Guid> CreateTestUserAsync()
        {
            var email = $"note-service-{Guid.NewGuid():N}@example.com";

            var registerService = new RegisterService(
                new UserRepository(_fixture.Context),
                new PasswordHasher(),
                new UnitOfWork(_fixture.Context));

            var userId = await registerService.RegisterAsync(
                new RegisterRequest(
                    email,
                    "SecureNotes!2026Test"));

            _fixture.Context.ChangeTracker.Clear();

            return userId;
        }

        private NoteService CreateService()
        {
            return new NoteService(
                new NoteRepository(_fixture.Context),
                new UnitOfWork(_fixture.Context));
        }

        [Fact]
        public async Task CreateAsync_ShouldCreateNote()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var service = CreateService();

            var request = new CreateNoteRequest(
                "Test title",
                "Test content");

            // Act
            var response = await service.CreateAsync(userId, request);

            // Assert
            Assert.NotEqual(Guid.Empty, response.Id);
            Assert.Equal("Test title", response.Title);
            Assert.Equal("Test content", response.Content);

            Assert.True(response.CreatedAtUtc <= DateTime.UtcNow);
            Assert.Null(response.UpdatedAtUtc);
        }

        [Fact]
        public async Task CreateAsync_ShouldTrimTitle()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var service = CreateService();

            var request = new CreateNoteRequest(
                "   My secure note   ",
                "Some content");

            // Act
            var response = await service.CreateAsync(userId, request);

            // Assert
            Assert.Equal("My secure note", response.Title);
        }

        [Fact]
        public async Task CreateAsync_ShouldRejectEmptyTitle()
        {
            // Arrange
            var service = CreateService();

            var request = new CreateNoteRequest(
                "   ",
                "Some content");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAsync(Guid.NewGuid(), request));
        }

        [Fact]
        public async Task CreateAsync_ShouldRejectTooLongTitle()
        {
            // Arrange
            var service = CreateService();

            var request = new CreateNoteRequest(
                new string('A', 151),
                "Some content");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAsync(Guid.NewGuid(), request));
        }

        [Fact]
        public async Task CreateAsync_ShouldRejectTooLongContent()
        {
            // Arrange
            var service = CreateService();

            var request = new CreateNoteRequest(
                "Test title",
                new string('A', 10001));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAsync(Guid.NewGuid(), request));
        }

        [Fact]
        public async Task CreateAsync_ShouldRejectEmptyUserId()
        {
            // Arrange
            var service = CreateService();

            var request = new CreateNoteRequest(
                "Test title",
                "Test content");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAsync(Guid.Empty, request));
        }

        [Fact]
        public async Task CreateAsync_ShouldRejectNullRequest()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => service.CreateAsync(Guid.NewGuid(), null!));
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnOnlyUserNotes()
        {
            // Arrange
            var userA = await CreateTestUserAsync();
            var userB = await CreateTestUserAsync();

            var service = CreateService();

            var noteA1 = await service.CreateAsync(
                userA,
                new CreateNoteRequest("User A - First", "Content A1"));

            var noteA2 = await service.CreateAsync(
                userA,
                new CreateNoteRequest("User A - Second", "Content A2"));

            await service.CreateAsync(
                userB,
                new CreateNoteRequest("User B - Private", "Private content"));

            _fixture.Context.ChangeTracker.Clear();

            // Act
            var notes = await service.GetAllAsync(userA);

            // Assert
            Assert.Equal(2, notes.Count);

            Assert.Contains(notes, note => note.Id == noteA1.Id);
            Assert.Contains(notes, note => note.Id == noteA2.Id);

            Assert.DoesNotContain(
                notes,
                note => note.Title == "User B - Private");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var service = CreateService();

            // Act
            var notes = await service.GetAllAsync(userId);

            // Assert
            Assert.Empty(notes);
        }

        [Fact]
        public async Task GetAllAsync_ShouldRejectEmptyUserId()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.GetAllAsync(Guid.Empty));
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnOwnNote()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var service = CreateService();

            var createdNote = await service.CreateAsync(
                userId,
                new CreateNoteRequest(
                    "My private note",
                    "My private content"));

            _fixture.Context.ChangeTracker.Clear();

            // Act
            var note = await service.GetByIdAsync(
                createdNote.Id,
                userId);

            // Assert
            Assert.NotNull(note);
            Assert.Equal(createdNote.Id, note.Id);
            Assert.Equal("My private note", note.Title);
            Assert.Equal("My private content", note.Content);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldNotReturnOtherUserNote()
        {
            // Arrange
            var ownerId = await CreateTestUserAsync();
            var otherUserId = await CreateTestUserAsync();

            var service = CreateService();

            var privateNote = await service.CreateAsync(
                ownerId,
                new CreateNoteRequest(
                    "Owner private note",
                    "Confidential content"));

            _fixture.Context.ChangeTracker.Clear();

            // Act
            var result = await service.GetByIdAsync(
                privateNote.Id,
                otherUserId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNullForUnknownNote()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(
                Guid.NewGuid(),
                userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldRejectEmptyIds()
        {
            // Arrange
            var service = CreateService();
            var validId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.GetByIdAsync(Guid.Empty, validId));

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.GetByIdAsync(validId, Guid.Empty));
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateOwnNote()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var service = CreateService();

            var createdNote = await service.CreateAsync(
                userId,
                new CreateNoteRequest(
                    "Original title",
                    "Original content"));

            _fixture.Context.ChangeTracker.Clear();

            // Act
            var result = await service.UpdateAsync(
                createdNote.Id,
                userId,
                new UpdateNoteRequest(
                    "Updated title",
                    "Updated content"));

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdNote.Id, result.Id);
            Assert.Equal("Updated title", result.Title);
            Assert.Equal("Updated content", result.Content);
            Assert.NotNull(result.UpdatedAtUtc);

            // Sprawdzamy również dane zapisane w MariaDB.
            _fixture.Context.ChangeTracker.Clear();

            var savedNote = await service.GetByIdAsync(
                createdNote.Id,
                userId);

            Assert.NotNull(savedNote);
            Assert.Equal("Updated title", savedNote.Title);
            Assert.Equal("Updated content", savedNote.Content);
        }

        [Fact]
        public async Task UpdateAsync_ShouldTrimTitle()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var service = CreateService();

            var createdNote = await service.CreateAsync(
                userId,
                new CreateNoteRequest(
                    "Original title",
                    "Original content"));

            // Act
            var result = await service.UpdateAsync(
                createdNote.Id,
                userId,
                new UpdateNoteRequest(
                    "   Updated title   ",
                    "Updated content"));

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated title", result.Title);
        }

        [Fact]
        public async Task UpdateAsync_ShouldNotUpdateOtherUserNote()
        {
            // Arrange
            var ownerId = await CreateTestUserAsync();
            var otherUserId = await CreateTestUserAsync();

            var service = CreateService();

            var createdNote = await service.CreateAsync(
                ownerId,
                new CreateNoteRequest(
                    "Private title",
                    "Private content"));

            _fixture.Context.ChangeTracker.Clear();

            // Act
            var result = await service.UpdateAsync(
                createdNote.Id,
                otherUserId,
                new UpdateNoteRequest(
                    "Attacker title",
                    "Attacker content"));

            // Assert
            Assert.Null(result);

            // Sprawdzamy, czy oryginalna notatka nie została zmieniona.
            _fixture.Context.ChangeTracker.Clear();

            var savedNote = await service.GetByIdAsync(
                createdNote.Id,
                ownerId);

            Assert.NotNull(savedNote);
            Assert.Equal("Private title", savedNote.Title);
            Assert.Equal("Private content", savedNote.Content);
            Assert.Null(savedNote.UpdatedAtUtc);
        }

        [Fact]
        public async Task UpdateAsync_ShouldReturnNullForUnknownNote()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var service = CreateService();

            // Act
            var result = await service.UpdateAsync(
                Guid.NewGuid(),
                userId,
                new UpdateNoteRequest(
                    "Updated title",
                    "Updated content"));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldRejectInvalidDataWithoutChangingNote()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var service = CreateService();

            var createdNote = await service.CreateAsync(
                userId,
                new CreateNoteRequest(
                    "Original title",
                    "Original content"));

            _fixture.Context.ChangeTracker.Clear();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.UpdateAsync(
                    createdNote.Id,
                    userId,
                    new UpdateNoteRequest(
                        "New title",
                        new string('A', 10001))));

            // Ponownie odczytujemy notatkę z bazy.
            _fixture.Context.ChangeTracker.Clear();

            var savedNote = await service.GetByIdAsync(
                createdNote.Id,
                userId);

            Assert.NotNull(savedNote);
            Assert.Equal("Original title", savedNote.Title);
            Assert.Equal("Original content", savedNote.Content);
            Assert.Null(savedNote.UpdatedAtUtc);
        }

        [Fact]
        public async Task UpdateAsync_ShouldRejectEmptyIdsAndNullRequest()
        {
            // Arrange
            var service = CreateService();
            var validId = Guid.NewGuid();

            var request = new UpdateNoteRequest(
                "Updated title",
                "Updated content");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.UpdateAsync(
                    Guid.Empty,
                    validId,
                    request));

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.UpdateAsync(
                    validId,
                    Guid.Empty,
                    request));

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => service.UpdateAsync(
                    validId,
                    validId,
                    null!));
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteOwnNote()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var service = CreateService();

            var createdNote = await service.CreateAsync(
                userId,
                new CreateNoteRequest(
                    "Note to delete",
                    "Content to delete"));

            _fixture.Context.ChangeTracker.Clear();

            // Act
            var result = await service.DeleteAsync(
                createdNote.Id,
                userId);

            // Assert
            Assert.True(result);

            // Ponowny odczyt z bazy danych.
            _fixture.Context.ChangeTracker.Clear();

            var savedNote = await service.GetByIdAsync(
                createdNote.Id,
                userId);

            Assert.Null(savedNote);
        }

        [Fact]
        public async Task DeleteAsync_ShouldNotDeleteOtherUserNote()
        {
            // Arrange
            var ownerId = await CreateTestUserAsync();
            var otherUserId = await CreateTestUserAsync();

            var service = CreateService();

            var createdNote = await service.CreateAsync(
                ownerId,
                new CreateNoteRequest(
                    "Owner private note",
                    "Confidential content"));

            _fixture.Context.ChangeTracker.Clear();

            // Act
            var result = await service.DeleteAsync(
                createdNote.Id,
                otherUserId);

            // Assert
            Assert.False(result);

            // Sprawdzamy, czy notatka właściciela nadal istnieje.
            _fixture.Context.ChangeTracker.Clear();

            var savedNote = await service.GetByIdAsync(
                createdNote.Id,
                ownerId);

            Assert.NotNull(savedNote);
            Assert.Equal(createdNote.Id, savedNote.Id);
            Assert.Equal("Owner private note", savedNote.Title);
            Assert.Equal("Confidential content", savedNote.Content);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalseForUnknownNote()
        {
            // Arrange
            var userId = await CreateTestUserAsync();
            var service = CreateService();

            // Act
            var result = await service.DeleteAsync(
                Guid.NewGuid(),
                userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRejectEmptyNoteId()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.DeleteAsync(
                    Guid.Empty,
                    Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteAsync_ShouldRejectEmptyUserId()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.DeleteAsync(
                    Guid.NewGuid(),
                    Guid.Empty));
        }

    }
}
