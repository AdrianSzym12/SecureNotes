
namespace SecureNotes.Application.Interfaces.Security
{
    public interface IJwtTokenService
    {
        string GenerateToken(Guid userId);
    }
}
