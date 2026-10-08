
using SecureNotes.Domain.Entities;
using SecureNotes.Domain.ValueObjects;
using Xunit;

namespace SecureNotes.Domain.Tests.Entities;

public sealed class UserTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var email = new Email("user@example.com");
        const string passwordHash = "test-password-hash";

        // Act
        var user = new User(email, passwordHash);

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(email, user.Email);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.True(user.CreatedAtUtc <= DateTime.UtcNow);
        Assert.True(user.CreatedAtUtc > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void Constructor_WithNullEmail_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new User(null!, "test-password-hash"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidPasswordHash_ShouldThrowArgumentException(
        string? passwordHash)
    {
        // Arrange
        var email = new Email("user@example.com");

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => new User(email, passwordHash!));
    }

    [Fact]
    public void Constructor_WithDifferentUsers_ShouldGenerateUniqueIds()
    {
        // Arrange
        var email1 = new Email("user1@example.com");
        var email2 = new Email("user2@example.com");

        // Act
        var user1 = new User(email1, "hash1");
        var user2 = new User(email2, "hash2");

        // Assert
        Assert.NotEqual(user1.Id, user2.Id);
    }
}
