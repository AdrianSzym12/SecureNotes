namespace SecureNotes.Domain.Entities
{
    public sealed class Note
    {
        public const int MaxTitleLength = 150;
        public const int MaxContentLength = 10000;

        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        public string Title { get; private set; }

        public string Content { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }

        public DateTime? UpdatedAtUtc { get; private set; }

        private Note()
        {
            Title = null!;
            Content = null!;
        }

        public Note(Guid userId, string title, string content)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException(
                    "User ID cannot be empty.",
                    nameof(userId));
            }

            Id = Guid.NewGuid();
            UserId = userId;
            Title = ValidateTitle(title);
            Content = ValidateContent(content);
            CreatedAtUtc = DateTime.UtcNow;
        }

        public void Update(string title, string content)
        {
            var validatedTitle = ValidateTitle(title);
            var validatedContent = ValidateContent(content);

            Title = validatedTitle;
            Content = validatedContent;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        private static string ValidateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(
                    "Note title cannot be empty.",
                    nameof(title));
            }

            title = title.Trim();

            if (title.Length > MaxTitleLength)
            {
                throw new ArgumentException(
                    $"Note title cannot exceed {MaxTitleLength} characters.",
                    nameof(title));
            }

            return title;
        }

        private static string ValidateContent(string content)
        {
            ArgumentNullException.ThrowIfNull(content);

            if (content.Length > MaxContentLength)
            {
                throw new ArgumentException(
                    $"Note content cannot exceed {MaxContentLength} characters.",
                    nameof(content));
            }

            return content;
        }
    }
}
