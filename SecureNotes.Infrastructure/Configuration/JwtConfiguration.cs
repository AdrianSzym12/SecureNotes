
namespace SecureNotes.Infrastructure.Configuration
{
    public class JwtConfiguration
    {
        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public int ExpirationMinutes { get; set; } = 30;

        public string SecretKey { get; set; } = string.Empty;
    }
}
