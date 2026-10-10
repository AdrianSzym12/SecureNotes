
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SecureNotes.Infrastructure.Configuration;
using System.Text;

namespace SecureNotes.API.Extensions
{
    public static class JwtAuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwt = configuration
                .GetSection("Jwt")
                .Get<JwtConfiguration>()
                ?? throw new InvalidOperationException(
                    "JWT configuration not found.");

            if (string.IsNullOrWhiteSpace(jwt.Issuer) ||
                string.IsNullOrWhiteSpace(jwt.Audience) ||
                jwt.ExpirationMinutes <= 0 ||
                string.IsNullOrWhiteSpace(jwt.SecretKey))
            {
                throw new InvalidOperationException(
                    "Invalid JWT configuration.");
            }

            var keyBytes = Encoding.UTF8.GetBytes(jwt.SecretKey);

            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException(
                    "JWT signing key must contain at least 32 bytes.");
            }

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = jwt.Issuer,

                            ValidateAudience = true,
                            ValidAudience = jwt.Audience,

                            ValidateLifetime = true,
                            RequireExpirationTime = true,

                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey =
                                new SymmetricSecurityKey(keyBytes),

                            RequireSignedTokens = true,

                            ValidAlgorithms = new[]
                            {
                                SecurityAlgorithms.HmacSha256
                            },

                            ClockSkew = TimeSpan.Zero
                        };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // JWT będzie przechowywany w HttpOnly Cookie.
                            context.Token = context.Request.Cookies[
                                "SecureNotes.Auth"];

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}
