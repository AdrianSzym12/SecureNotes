
using SecureNotes.Domain.Entities;
using Xunit;

namespace SecureNotes.Domain.Tests.Entities;

public sealed class NoteTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidData_ShouldCreateNote()
    {
        // Act
        var note = new Note(
            _userId,
            "My first note",
            "This is the note content.");

        // Assert
        Assert.NotEqual(Guid.Empty, note.Id);
        Assert.Equal(_userId, note.UserId);
        Assert.Equal("My first note", note.Title);
        Assert.Equal("This is the note content.", note.Content);
        Assert.True(note.CreatedAtUtc <= DateTime.UtcNow);
        Assert.Null(note.UpdatedAtUtc);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new Note(Guid.Empty, "Title", "Content"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithInvalidTitle_ShouldThrowArgumentException(
        string title)
    {
        Assert.Throws<ArgumentException>(
            () => new Note(_userId, title, "Content"));
    }

    [Fact]
    public void Constructor_WithTooLongTitle_ShouldThrowArgumentException()
    {
        var title = new string('A', Note.MaxTitleLength + 1);

        Assert.Throws<ArgumentException>(
            () => new Note(_userId, title, "Content"));
    }

    [Fact]
    public void Constructor_WithTooLongContent_ShouldThrowArgumentException()
    {
        var content = new string('A', Note.MaxContentLength + 1);

        Assert.Throws<ArgumentException>(
            () => new Note(_userId, "Title", content));
    }

    [Fact]
    public void Constructor_WithEmptyContent_ShouldCreateNote()
    {
        var note = new Note(_userId, "Title", "");

        Assert.Equal("", note.Content);
    }

    [Fact]
    public void Constructor_ShouldTrimTitle()
    {
        var note = new Note(_userId, "  My note  ", "Content");

        Assert.Equal("My note", note.Title);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateNote()
    {
        // Arrange
        var note = new Note(_userId, "Old title", "Old content");

        // Act
        note.Update("New title", "New content");

        // Assert
        Assert.Equal("New title", note.Title);
        Assert.Equal("New content", note.Content);
        Assert.NotNull(note.UpdatedAtUtc);
        Assert.True(note.UpdatedAtUtc >= note.CreatedAtUtc);
    }

    [Fact]
    public void Update_WithInvalidTitle_ShouldPreserveExistingData()
    {
        // Arrange
        var note = new Note(_userId, "Original title", "Original content");

        // Act
        Assert.Throws<ArgumentException>(
            () => note.Update("", "Changed content"));

        // Assert
        Assert.Equal("Original title", note.Title);
        Assert.Equal("Original content", note.Content);
        Assert.Null(note.UpdatedAtUtc);
    }

    [Fact]
    public void Update_WithInvalidContent_ShouldPreserveExistingData()
    {
        // Arrange
        var note = new Note(_userId, "Original title", "Original content");
        var invalidContent = new string(
            'A',
            Note.MaxContentLength + 1);

        // Act
        Assert.Throws<ArgumentException>(
            () => note.Update("New title", invalidContent));

        // Assert
        Assert.Equal("Original title", note.Title);
        Assert.Equal("Original content", note.Content);
        Assert.Null(note.UpdatedAtUtc);
    }

    [Fact]
    public void Constructor_WithHtmlContent_ShouldPreserveText()
    {
        // Arrange
        const string htmlContent = "<script>alert('XSS')</script>";

        // Act
        var note = new Note(_userId, "HTML example", htmlContent);

        // Assert
        Assert.Equal(htmlContent, note.Content);
    }
}
