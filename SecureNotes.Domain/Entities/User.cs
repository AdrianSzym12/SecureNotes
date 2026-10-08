using SecureNotes.Domain.ValueObjects;

namespace SecureNotes.Domain.Entities
{
    public sealed class User
    {
        public Guid Id { get; private set; }
        public Email Email{ get; private set; }
        public string PasswordHash { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        private User()
        {
            Email = null!;
            PasswordHash = null!;
        }
        public User(Email email, string passwordHash)
        {
            ArgumentNullException.ThrowIfNull(email);
            
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException("Password hash cannot be null or empty.", nameof(passwordHash));
            }
            Id = Guid.NewGuid();
            Email = email;
            PasswordHash = passwordHash;
            CreatedAtUtc = DateTime.UtcNow;
        }
    }
}
