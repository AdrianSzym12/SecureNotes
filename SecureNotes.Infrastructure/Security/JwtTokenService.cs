
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SecureNotes.Application.Interfaces.Security;
using SecureNotes.Infrastructure.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecureNotes.Infrastructure.Security
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtConfiguration _configuration;

        public JwtTokenService(
            IOptions<JwtConfiguration> options)
        {
            _configuration = options.Value;
        }

        public string GenerateToken(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException(
                    "User ID cannot be empty.",
                    nameof(userId));
            }

            var keyBytes = Encoding.UTF8.GetBytes(
                _configuration.SecretKey);

            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException(
                    "JWT signing key must contain at least 32 bytes.");
            }

            if (string.IsNullOrWhiteSpace(_configuration.Issuer) ||
                string.IsNullOrWhiteSpace(_configuration.Audience) ||
                _configuration.ExpirationMinutes <= 0)
            {
                throw new InvalidOperationException(
                    "Invalid JWT configuration.");
            }

            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    userId.ToString()),

                new Claim(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()),

                new Claim(
                    JwtRegisteredClaimNames.Iat,
                    EpochTime.GetIntDate(now).ToString(),
                    ClaimValueTypes.Integer64)
            };

            var securityKey = new SymmetricSecurityKey(keyBytes);

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(
                    _configuration.ExpirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}
