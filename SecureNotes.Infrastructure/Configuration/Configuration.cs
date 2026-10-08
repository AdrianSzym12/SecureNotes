
namespace SecureNotes.Infrastructure.Configuration
{
    public class Configuration
    {
        public ApiConfiguration Api { get; set; } = new();
    }

    public class ApiConfiguration
    {
        public string Server { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public int? Port { get; set; }
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
