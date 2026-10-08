
using Microsoft.EntityFrameworkCore;
using SecureNotes.Infrastructure.Persistence;

namespace SecureNotes.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Rejestracja us³ug
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddPersistence(builder.Configuration);

            var app = builder.Build();

            // Automatyczne migracje bazy danych
            // Wy³¹cznie w œrodowisku Development
            if (app.Environment.IsDevelopment())
            {
                using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider
                        .GetRequiredService<PersistenceContext>();

                    await dbContext.Database.MigrateAsync();
                }
            }

            // Konfiguracja HTTP
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
