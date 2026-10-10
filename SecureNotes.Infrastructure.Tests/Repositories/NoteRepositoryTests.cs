
using SecureNotes.Domain.Entities;
using SecureNotes.Domain.ValueObjects;
using SecureNotes.Infrastructure.Persistence;
using SecureNotes.Infrastructure.Persistence.Repositories;
using SecureNotes.Infrastructure.Tests.Fixtures;

namespace SecureNotes.Infrastructure.Tests.Repositories
{
    public class NoteRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public NoteRepositoryTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task AddAsync_ShouldSaveNoteToDatabase()
        {
            // Arrange
            var context = _fixture.Context;

            var userRepository = new UserRepository(context);
            var noteRepository = new NoteRepository(context);

            var email = new Email(
                $"note-test-{Guid.NewGuid():N}@example.com");

            var user = new User(email, "test-password-hash");

            await userRepository.AddAsync(user);
            await context.SaveChangesAsync();

            var note = new Note(
                user.Id,
                "Testowa notatka",
                "Treść testowej notatki");

            // Act
            await noteRepository.AddAsync(note);
            await context.SaveChangesAsync();

            // Czyścimy śledzone encje, żeby odczyt nastąpił z bazy.
            context.ChangeTracker.Clear();

            var savedNote = await noteRepository.GetByIdAndUserIdAsync(
                note.Id,
                user.Id);

            // Assert
            Assert.NotNull(savedNote);
            Assert.Equal(note.Id, savedNote.Id);
            Assert.Equal(user.Id, savedNote.UserId);
            Assert.Equal("Testowa notatka", savedNote.Title);
            Assert.Equal("Treść testowej notatki", savedNote.Content);
        }

        [Fact]
        public async Task GetByIdAndUserIdAsync_ShouldReturnNull_WhenUserIsNotOwner()
        {
            // Arrange
            var context = _fixture.Context;

            var userRepository = new UserRepository(context);
            var noteRepository = new NoteRepository(context);

            var owner = new User(
                new Email($"owner-{Guid.NewGuid():N}@example.com"),
                "test-password-hash");

            var otherUser = new User(
                new Email($"other-{Guid.NewGuid():N}@example.com"),
                "test-password-hash");

            await userRepository.AddAsync(owner);
            await userRepository.AddAsync(otherUser);
            await context.SaveChangesAsync();

            var note = new Note(
                owner.Id,
                "Prywatna notatka",
                "Tylko właściciel powinien ją odczytać");

            await noteRepository.AddAsync(note);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();

            // Act
            var result = await noteRepository.GetByIdAndUserIdAsync(
                note.Id,
                otherUser.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnOnlyOwnerNotes()
        {
            // Arrange
            var context = _fixture.Context;

            var userRepository = new UserRepository(context);
            var noteRepository = new NoteRepository(context);

            var owner = new User(
                new Email($"owner-{Guid.NewGuid():N}@example.com"),
                "test-password-hash");

            var otherUser = new User(
                new Email($"other-{Guid.NewGuid():N}@example.com"),
                "test-password-hash");

            await userRepository.AddAsync(owner);
            await userRepository.AddAsync(otherUser);
            await context.SaveChangesAsync();

            var firstNote = new Note(
                owner.Id,
                "Notatka A1",
                "Treść pierwszej notatki");

            var secondNote = new Note(
                owner.Id,
                "Notatka A2",
                "Treść drugiej notatki");

            var otherNote = new Note(
                otherUser.Id,
                "Notatka B1",
                "Treść cudzej notatki");

            await noteRepository.AddAsync(firstNote);
            await noteRepository.AddAsync(secondNote);
            await noteRepository.AddAsync(otherNote);

            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();

            // Act
            var result = await noteRepository.GetByUserIdAsync(owner.Id);

            // Assert
            Assert.Equal(2, result.Count);

            Assert.Contains(result, n => n.Id == firstNote.Id);
            Assert.Contains(result, n => n.Id == secondNote.Id);

            Assert.DoesNotContain(result, n => n.Id == otherNote.Id);

            Assert.All(result, n =>
                Assert.Equal(owner.Id, n.UserId));
        }

        [Fact]
        public async Task Remove_ShouldDeleteNoteFromDatabase()
        {
            // Arrange
            var context = _fixture.Context;

            var userRepository = new UserRepository(context);
            var noteRepository = new NoteRepository(context);
            var unitOfWork = new UnitOfWork(context);

            var user = new User(
                new Email($"delete-{Guid.NewGuid():N}@example.com"),
                "test-password-hash");

            await userRepository.AddAsync(user);
            await unitOfWork.SaveChangesAsync();

            var note = new Note(
                user.Id,
                "Notatka do usunięcia",
                "Ta notatka powinna zostać usunięta");

            await noteRepository.AddAsync(note);
            await unitOfWork.SaveChangesAsync();

            // Odczyt z bazy i usunięcie notatki
            context.ChangeTracker.Clear();

            var savedNote = await noteRepository.GetByIdAndUserIdAsync(
                note.Id,
                user.Id);

            Assert.NotNull(savedNote);

            // Act
            noteRepository.Remove(savedNote);
            await unitOfWork.SaveChangesAsync();

            context.ChangeTracker.Clear();

            var deletedNote = await noteRepository.GetByIdAndUserIdAsync(
                note.Id,
                user.Id);

            // Assert
            Assert.Null(deletedNote);
        }

        [Fact]
        public async Task Update_ShouldSaveChangesToDatabase()
        {
            // Arrange
            var context = _fixture.Context;

            var userRepository = new UserRepository(context);
            var noteRepository = new NoteRepository(context);
            var unitOfWork = new UnitOfWork(context);

            var user = new User(
                new Email($"update-{Guid.NewGuid():N}@example.com"),
                "test-password-hash");

            await userRepository.AddAsync(user);
            await unitOfWork.SaveChangesAsync();

            var note = new Note(
                user.Id,
                "Stary tytuł",
                "Stara treść");

            await noteRepository.AddAsync(note);
            await unitOfWork.SaveChangesAsync();

            context.ChangeTracker.Clear();

            var savedNote = await noteRepository.GetByIdAndUserIdAsync(
                note.Id,
                user.Id);

            Assert.NotNull(savedNote);

            // Act
            savedNote.Update(
                "Nowy tytuł",
                "Nowa treść");

            await unitOfWork.SaveChangesAsync();

            context.ChangeTracker.Clear();

            var updatedNote = await noteRepository.GetByIdAndUserIdAsync(
                note.Id,
                user.Id);

            // Assert
            Assert.NotNull(updatedNote);
            Assert.Equal("Nowy tytuł", updatedNote.Title);
            Assert.Equal("Nowa treść", updatedNote.Content);
            Assert.NotNull(updatedNote.UpdatedAtUtc);
        }

    }
}
