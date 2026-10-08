using Microsoft.EntityFrameworkCore;
using SecureNotes.Infrastructure.Persistence;
namespace SecureNotes.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
            builder.Services.AddPersistence(builder.Configuration);

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider
                    .GetRequiredService<PersistenceContext>();
                var migrations = dbContext.Database.GetMigrations().ToList();

                Console.WriteLine($"Liczba migracji: {migrations.Count}");

                foreach (var migration in migrations)
                {
                    Console.WriteLine($"Migracja: {migration}");
                }
                try
                {
                    await dbContext.Database.MigrateAsync();
                    await dbContext.Database.OpenConnectionAsync();
                    Console.WriteLine("MariaDB connection: True");
                    await dbContext.Database.CloseConnectionAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

    }
}
