
using SecureNotes.Domain.Entities;

namespace SecureNotes.Domain.Repositories;

public interface INoteRepository
{
    Task<IReadOnlyList<Note>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Note?> GetByIdAndUserIdAsync(
        Guid noteId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Note note,
        CancellationToken cancellationToken = default);

    void Remove(Note note);
}
