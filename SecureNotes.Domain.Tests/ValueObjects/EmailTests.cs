
using SecureNotes.Domain.ValueObjects;
using Xunit;

namespace SecureNotes.Domain.Tests.ValueObjects;

public sealed class EmailTests
{
    [Fact]
    public void Constructor_WithValidEmail_ShouldCreateEmail()
    {
        // Arrange
        const string emailAddress = "user@example.com";

        // Act
        var email = new Email(emailAddress);

        // Assert
        Assert.Equal(emailAddress, email.Value);
    }

    [Fact]
    public void Constructor_WithUppercaseEmail_ShouldNormalizeToLowercase()
    {
        // Arrange
        const string emailAddress = "USER@EXAMPLE.COM";

        // Act
        var email = new Email(emailAddress);

        // Assert
        Assert.Equal("user@example.com", email.Value);
    }

    [Fact]
    public void Constructor_WithWhitespace_ShouldTrimEmail()
    {
        // Arrange
        const string emailAddress = "  user@example.com  ";

        // Act
        var email = new Email(emailAddress);

        // Assert
        Assert.Equal("user@example.com", email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    public void Constructor_WithInvalidEmail_ShouldThrowArgumentException(
        string invalidEmail)
    {
        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => new Email(invalidEmail));

        // Assert
        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void TwoEmails_WithSameNormalizedValue_ShouldBeEqual()
    {
        // Arrange
        var first = new Email("USER@example.com");
        var second = new Email("user@example.com");

        // Act & Assert
        Assert.Equal(first, second);
    }

    [Fact]
    public void Constructor_WithTooLongEmail_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidEmail =
            new string('a', Email.MaxLength) + "@example.com";

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => new Email(invalidEmail));
    }
}
