
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SecureNotes.Infrastructure.Configuration;
using SecureNotes.Infrastructure.Persistence;

namespace SecureNotes.Infrastructure.Tests.Fixtures
{
    public class DatabaseFixture : IAsyncLifetime
    {
        public PersistenceContext Context { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .AddUserSecrets<DatabaseFixture>()
                .Build();

            var config = configuration
                .GetSection("Api")
                .Get<ApiConfiguration>()
                ?? throw new InvalidOperationException(
                    "Missing test database configuration.");

            // Zabezpieczenie przed użyciem głównej bazy
            if (config.Database != "securenotes_test_db")
            {
                throw new InvalidOperationException(
                    "Tests must use securenotes_test_db.");
            }

            var connectionString = new MySqlConnectionStringBuilder
            {
                Server = config.Server,
                Database = config.Database,
                Port = (uint)config.Port,
                UserID = config.User,
                Password = config.Password
            }.ConnectionString;

            var options = new DbContextOptionsBuilder<PersistenceContext>()
                .UseMySql(
                    connectionString,
                    new MariaDbServerVersion(new Version(10, 4, 32)))
                .Options;

            Context = new PersistenceContext(options);

            await Context.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await Context.DisposeAsync();
        }
    }
}
