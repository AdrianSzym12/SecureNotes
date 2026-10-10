
using Microsoft.EntityFrameworkCore;
using SecureNotes.Domain.Entities;
using SecureNotes.Domain.Repositories;

namespace SecureNotes.Infrastructure.Persistence.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly PersistenceContext _context;

        public NoteRepository(PersistenceContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Note>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notes
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task<Note?> GetByIdAndUserIdAsync(
            Guid noteId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Notes
                .FirstOrDefaultAsync(
                    n => n.Id == noteId &&
                         n.UserId == userId,
                    cancellationToken);
        }

        public async Task AddAsync(
            Note note,
            CancellationToken cancellationToken = default)
        {
            await _context.Notes.AddAsync(
                note,
                cancellationToken);
        }

        public void Remove(Note note)
        {
            _context.Notes.Remove(note);
        }
    }
}
