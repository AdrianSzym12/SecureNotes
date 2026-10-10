
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SecureNotes.Infrastructure.Configuration;
using SecureNotes.Infrastructure.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecureNotes.Infrastructure.Tests.Security
{
    public class JwtTokenServiceTests
    {
        // Wyłącznie klucz testowy. Nie używamy tutaj User Secrets.
        private const string TestSecretKey =
            "SecureNotes-TEST-KEY-Only-For-Unit-Tests-2026-123456";

        private const string TestIssuer = "SecureNotes.Tests";
        private const string TestAudience = "SecureNotes.Tests.Client";

        private JwtTokenService CreateService()
        {
            var configuration = new JwtConfiguration
            {
                Issuer = TestIssuer,
                Audience = TestAudience,
                ExpirationMinutes = 30,
                SecretKey = TestSecretKey
            };

            return new JwtTokenService(
                Options.Create(configuration));
        }

        [Fact]
        public void GenerateToken_ShouldContainCorrectUserId()
        {
            // Arrange
            var service = CreateService();
            var userId = Guid.NewGuid();

            // Act
            var token = service.GenerateToken(userId);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Assert
            Assert.Equal(
                userId.ToString(),
                jwt.Subject);

            Assert.Equal(TestIssuer, jwt.Issuer);
            Assert.Contains(TestAudience, jwt.Audiences);

            Assert.NotNull(jwt.Id);
            Assert.NotEmpty(jwt.Id);

            Assert.True(jwt.ValidTo > DateTime.UtcNow);
        }

        [Fact]
        public void GenerateToken_ShouldHaveValidSignature()
        {
            // Arrange
            var service = CreateService();
            var token = service.GenerateToken(Guid.NewGuid());

            var validationParameters = CreateValidationParameters(
                TestSecretKey);

            // Act
            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(
                token,
                validationParameters,
                out var validatedToken);

            // Assert
            Assert.NotNull(principal);
            Assert.IsType<JwtSecurityToken>(validatedToken);

            Assert.Contains(
                principal.Claims,
                claim => claim.Type == ClaimTypes.NameIdentifier ||
                         claim.Type == JwtRegisteredClaimNames.Sub);
        }


        [Fact]
        public void GenerateToken_ShouldRejectInvalidSigningKey()
        {
            // Arrange
            var service = CreateService();
            var token = service.GenerateToken(Guid.NewGuid());

            const string wrongKey =
                "SecureNotes-WRONG-KEY-Only-For-Unit-Tests-2026-12345";

            var validationParameters = CreateValidationParameters(
                wrongKey);

            var handler = new JwtSecurityTokenHandler();

            // Act & Assert
            var exception = Record.Exception(() =>
                handler.ValidateToken(
                    token,
                    validationParameters,
                    out _));

            Assert.NotNull(exception);

            Assert.True(
                exception is SecurityTokenInvalidSignatureException
                    or SecurityTokenSignatureKeyNotFoundException,
                $"Unexpected exception type: {exception.GetType().Name}");
        }


        [Fact]
        public void GenerateToken_ShouldRejectEmptyUserId()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => service.GenerateToken(Guid.Empty));
        }

        private static TokenValidationParameters CreateValidationParameters(
            string secretKey)
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = TestIssuer,

                ValidateAudience = true,
                ValidAudience = TestAudience,

                ValidateLifetime = true,
                RequireExpirationTime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secretKey)),

                RequireSignedTokens = true,

                ClockSkew = TimeSpan.Zero
            };
        }
    }
}
