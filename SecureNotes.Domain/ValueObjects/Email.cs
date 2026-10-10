
using System.Net.Mail;

namespace SecureNotes.Domain.ValueObjects;

public sealed record Email
{
    public const int MaxLength = 254;

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Email cannot be empty.",
                nameof(value));
        }

        var normalizedEmail = value.Trim().ToLowerInvariant();

        if (normalizedEmail.Length > MaxLength)
        {
            throw new ArgumentException(
                $"Email cannot exceed {MaxLength} characters.",
                nameof(value));
        }

        if (!MailAddress.TryCreate(normalizedEmail, out var address)
            || address.Address != normalizedEmail
            || !normalizedEmail.Contains('@'))
        {
            throw new ArgumentException(
                "Invalid email address.",
                nameof(value));
        }

        Value = normalizedEmail;
    }

    public override string ToString() => Value;
}
