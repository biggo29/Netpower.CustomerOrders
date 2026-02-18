namespace Netpower.CustomerOrders.Api.Authentication
{
    public interface IJwtTokenService
    {
        string GenerateToken(string userId, string email, IEnumerable<string> roles = null!);
        bool ValidateToken(string token);
    }
}