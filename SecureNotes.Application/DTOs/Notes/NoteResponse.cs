
namespace SecureNotes.Application.DTOs.Notes
{
    public record NoteResponse(
        Guid Id,
        string Title,
        string Content,
        DateTime CreatedAtUtc,
        DateTime? UpdatedAtUtc);
}
