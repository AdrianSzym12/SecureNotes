
using Microsoft.EntityFrameworkCore;
using SecureNotes.Domain.Entities;
using SecureNotes.Domain.Repositories;
using SecureNotes.Domain.ValueObjects;

namespace SecureNotes.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly PersistenceContext _context;

        public UserRepository(PersistenceContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Id == id,
                    cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(
            Email email,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email == email,
                    cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(
            Email email,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AnyAsync(
                    u => u.Email == email,
                    cancellationToken);
        }

        public async Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(
                user,
                cancellationToken);
        }
    }
}
