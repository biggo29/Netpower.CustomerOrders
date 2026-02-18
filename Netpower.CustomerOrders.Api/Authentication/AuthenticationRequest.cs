using System.ComponentModel.DataAnnotations;

namespace Netpower.CustomerOrders.Api.Authentication
{
    public sealed class AuthenticationRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; init; } = default!;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; init; } = default!;
    }
}