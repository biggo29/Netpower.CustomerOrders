using Microsoft.AspNetCore.Mvc;
using Netpower.CustomerOrders.Api.Authentication;
using System.ComponentModel.DataAnnotations;

namespace Netpower.CustomerOrders.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public sealed class AuthenticationController : ControllerBase
    {
        private readonly IJwtTokenService _tokenService;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(IJwtTokenService tokenService, ILogger<AuthenticationController> logger)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Authenticate and obtain JWT token
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] AuthenticationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Login attempt with invalid credentials");
                return BadRequest(new { message = "Email and password are required" });
            }

            // TODO: Replace with actual user validation against user store/database
            // This is a placeholder - implement proper user authentication
            if (!ValidateCredentials(request.Email, request.Password))
            {
                _logger.LogWarning("Failed login attempt for email {Email}", request.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }

            try
            {
                var token = _tokenService.GenerateToken(
                    userId: request.Email, // Use email as unique identifier
                    email: request.Email,
                    roles: new[] { "User" });

                var expiresAt = DateTime.UtcNow.AddMinutes(60);

                _logger.LogInformation("User {Email} successfully authenticated", request.Email);

                return Ok(new AuthenticationResponse
                {
                    Token = token,
                    Email = request.Email,
                    ExpiresAt = expiresAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for email {Email}", request.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred during authentication" });
            }
        }

        /// <summary>
        /// Placeholder credential validation - implement your actual authentication logic
        /// </summary>
        private static bool ValidateCredentials(string email, string password)
        {
            // TODO: Replace with actual user validation
            // This is a dummy implementation for demonstration
            return !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password);
        }
    }
}