namespace Netpower.CustomerOrders.Api.Authentication
{
    public sealed class AuthenticationResponse
    {
        public string Token { get; init; } = default!;
        public string Email { get; init; } = default!;
        public DateTime ExpiresAt { get; init; }
    }
}