namespace Netpower.CustomerOrders.Api.Settings
{
    public sealed class JwtSettings
    {
        public string SecretKey { get; set; } = default!;
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public int ExpirationMinutes { get; set; } = 60;
    }
}