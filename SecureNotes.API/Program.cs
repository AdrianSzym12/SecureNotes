
using Microsoft.EntityFrameworkCore;
using SecureNotes.API.Extensions;
using SecureNotes.API.Middleware;
using SecureNotes.Infrastructure.Persistence;

namespace SecureNotes.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Rejestracja us³ug
            builder.Services.AddControllersWithViews();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddPersistence(builder.Configuration);
            builder.Services.AddJwtAuthentication(builder.Configuration);


            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";

                options.Cookie.Name = "SecureNotes.Csrf";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Path = "/";
            });


            var app = builder.Build();
            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseMiddleware<SecurityHeadersMiddleware>();

            
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

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();

        }
    }
}
