
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SecureNotes.Application.Interfaces.Security;
using SecureNotes.Infrastructure.Security;
using MySqlConnector;
using SecureNotes.Application.Interfaces.Persistence;
using SecureNotes.Application.Services;
using SecureNotes.Domain.Repositories;
using SecureNotes.Infrastructure.Configuration;
using SecureNotes.Infrastructure.Persistence.Repositories;


namespace SecureNotes.Infrastructure.Persistence
{
    public static class PersistenceService
    {
        public static IServiceCollection AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var config = configuration
                .GetSection("Api")
                .Get<ApiConfiguration>()
                ?? throw new InvalidOperationException(
                    "Api configuration not found.");


            if (string.IsNullOrWhiteSpace(config.Server) ||
                string.IsNullOrWhiteSpace(config.Database) ||
                string.IsNullOrWhiteSpace(config.User) ||
                string.IsNullOrWhiteSpace(config.Password) ||
                config.Port is < 1 or > 65535)
            {
                throw new InvalidOperationException(
                    "Invalid database configuration.");
            }

            var connectionString = new MySqlConnectionStringBuilder
            {
                Server = config.Server,
                Database = config.Database,
                Port = (uint)config.Port,
                UserID = config.User,
                Password = config.Password,
                SslMode = MySqlSslMode.Preferred
            }.ConnectionString;

            services.AddDbContext<PersistenceContext>(options =>
                options.UseMySql(
                    connectionString,
                    new MariaDbServerVersion(
                        new Version(10, 4, 32))));

            services.Configure<JwtConfiguration>(
            configuration.GetSection("Jwt"));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<RegisterService>();
            services.AddScoped<LoginService>();

            services.AddScoped<IJwtTokenService, JwtTokenService>();


            return services;
        }
    }
}
