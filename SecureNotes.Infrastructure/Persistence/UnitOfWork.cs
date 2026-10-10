
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using SecureNotes.Application.Exceptions;
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
            try
            {
                return await _context.SaveChangesAsync(
                    cancellationToken);
            }
            catch (DbUpdateException ex)
                when (IsDuplicateEmailError(ex))
            {
                throw new DuplicateEmailException();
            }
        }

        private static bool IsDuplicateEmailError(
            DbUpdateException exception)
        {
            if (exception.InnerException is not MySqlException mysqlException)
            {
                return false;
            }

            return mysqlException.Number == 1062 &&
                   mysqlException.Message.Contains(
                       "IX_Users_Email",
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}
