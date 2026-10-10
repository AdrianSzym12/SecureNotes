
using SecureNotes.Infrastructure.Security;

namespace SecureNotes.Infrastructure.Tests.Security
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _passwordHasher;

        public PasswordHasherTests()
        {
            _passwordHasher = new PasswordHasher();
        }

        [Fact]
        public void HashPassword_ShouldNotReturnPlainText()
        {
            // Arrange
            var password = "SecureNotes!123";

            // Act
            var hash = _passwordHasher.HashPassword(password);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
            Assert.NotEqual(password, hash);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrue_ForCorrectPassword()
        {
            // Arrange
            var password = "SecureNotes!123";
            var hash = _passwordHasher.HashPassword(password);

            // Act
            var result = _passwordHasher.VerifyPassword(
                password,
                hash);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_ForWrongPassword()
        {
            // Arrange
            var hash = _passwordHasher.HashPassword(
                "SecureNotes!123");

            // Act
            var result = _passwordHasher.VerifyPassword(
                "WrongPassword!123",
                hash);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HashPassword_ShouldGenerateDifferentHashes_ForSamePassword()
        {
            // Arrange
            var password = "SecureNotes!123";

            // Act
            var firstHash = _passwordHasher.HashPassword(password);
            var secondHash = _passwordHasher.HashPassword(password);

            // Assert
            Assert.NotEqual(firstHash, secondHash);

            Assert.True(_passwordHasher.VerifyPassword(
                password,
                firstHash));

            Assert.True(_passwordHasher.VerifyPassword(
                password,
                secondHash));
        }
    }
}
