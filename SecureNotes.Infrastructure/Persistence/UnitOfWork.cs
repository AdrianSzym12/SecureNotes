
using SecureNotes.Application.Interfaces.Persistence;

namespace SecureNotes.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PersistenceContext _context;

        public UnitOfWork(PersistenceContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(
                cancellationToken);
        }
    }
}
