using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Netpower.CustomerOrders.Api.Authentication;
using Netpower.CustomerOrders.Api.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Netpower.CustomerOrders.UnitTests.Authentication
{
    /// <summary>
    /// Unit tests for JwtTokenService with mocked dependencies
    /// </summary>
    public sealed class JwtTokenServiceTests
    {
        private readonly JwtTokenService _sut;
        private readonly Mock<IOptions<JwtSettings>> _mockJwtOptions;
        private readonly Mock<ILogger<JwtTokenService>> _mockLogger;
        private readonly JwtSettings _jwtSettings;

        public JwtTokenServiceTests()
        {
            _jwtSettings = new JwtSettings
            {
                SecretKey = "your-256-bit-secret-key-min-32-chars-required-for-HS256",
                Issuer = "Netpower.CustomerOrders.Api",
                Audience = "NetpowerClients",
                ExpirationMinutes = 60
            };

            _mockJwtOptions = new Mock<IOptions<JwtSettings>>();
            _mockJwtOptions.Setup(x => x.Value).Returns(_jwtSettings);

            _mockLogger = new Mock<ILogger<JwtTokenService>>();

            _sut = new JwtTokenService(_mockJwtOptions.Object, _mockLogger.Object);
        }

        #region GenerateToken Tests

        [Fact]
        public void GenerateToken_WithValidInput_ReturnsValidJwtToken()
        {
            // Arrange
            var userId = "test-user-123";
            var email = "user@example.com";
            var roles = new[] { "User", "Admin" };

            // Act
            var token = _sut.GenerateToken(userId, email, roles);

            // Assert
            token.Should().NotBeNullOrWhiteSpace();

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            jwtToken.Should().NotBeNull();
            jwtToken!.Issuer.Should().Be(_jwtSettings.Issuer);
            jwtToken.Audiences.Should().Contain(_jwtSettings.Audience);
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId);
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }

        [Fact]
        public void GenerateToken_WithoutRoles_ReturnsTokenWithoutRoleClaims()
        {
            // Arrange
            var userId = "test-user-123";
            var email = "user@example.com";

            // Act
            var token = _sut.GenerateToken(userId, email);

            // Assert
            token.Should().NotBeNullOrWhiteSpace();

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            jwtToken!.Claims.Where(c => c.Type == ClaimTypes.Role).Should().BeEmpty();
        }

        [Fact]
        public void GenerateToken_WithSingleRole_ReturnsTokenWithRoleClaim()
        {
            // Arrange
            var userId = "test-user-123";
            var email = "user@example.com";
            var roles = new[] { "Admin" };

            // Act
            var token = _sut.GenerateToken(userId, email, roles);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            jwtToken!.Claims.Where(c => c.Type == ClaimTypes.Role && c.Value == "Admin").Should().HaveCount(1);
        }

        [Fact]
        public void GenerateToken_SetsCorrectExpiration()
        {
            // Arrange
            var userId = "test-user-123";
            var email = "user@example.com";
            var beforeGeneration = DateTime.UtcNow;

            // Act
            var token = _sut.GenerateToken(userId, email);
            var afterGeneration = DateTime.UtcNow;

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            var expectedExpiration = beforeGeneration.AddMinutes(_jwtSettings.ExpirationMinutes);
            jwtToken!.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void GenerateToken_IncludesAudienceClaim()
        {
            // Arrange
            var userId = "test-user-123";
            var email = "user@example.com";

            // Act
            var token = _sut.GenerateToken(userId, email);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            jwtToken!.Claims.Should().Contain(c => c.Type == "aud" && c.Value == _jwtSettings.Audience);
        }

        #endregion

        #region ValidateToken Tests

        [Fact]
        public void ValidateToken_WithValidToken_ReturnsTrue()
        {
            // Arrange
            var userId = "test-user-123";
            var email = "user@example.com";
            var token = _sut.GenerateToken(userId, email);

            // Act
            var result = _sut.ValidateToken(token);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateToken_WithInvalidToken_ReturnsFalse()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var result = _sut.ValidateToken(invalidToken);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateToken_WithTamperedToken_ReturnsFalse()
        {
            // Arrange
            var userId = "test-user-123";
            var email = "user@example.com";
            var token = _sut.GenerateToken(userId, email);
            
            // Tamper with the token by changing a character
            var tamperedToken = token.Substring(0, token.Length - 5) + "XXXXX";

            // Act
            var result = _sut.ValidateToken(tamperedToken);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateToken_WithEmptyToken_ReturnsFalse()
        {
            // Arrange
            var emptyToken = string.Empty;

            // Act
            var result = _sut.ValidateToken(emptyToken);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateToken_WithNullToken_ReturnsFalse()
        {
            // Arrange
            string? nullToken = null;

            // Act
            var result = _sut.ValidateToken(nullToken!);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Round-trip Tests

        [Fact]
        public void GenerateAndValidateToken_WithValidInput_SucceedsRoundTrip()
        {
            // Arrange
            var userId = "test-user-123";
            var email = "user@example.com";
            var roles = new[] { "User" };

            // Act
            var token = _sut.GenerateToken(userId, email, roles);
            var isValid = _sut.ValidateToken(token);

            // Assert
            isValid.Should().BeTrue();

            // Verify token content
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            jwtToken!.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId);
            jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
        }

        #endregion
    }
}