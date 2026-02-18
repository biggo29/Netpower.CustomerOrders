namespace Netpower.CustomerOrders.Api.Middleware
{
    /// <summary>
    /// Adds security headers to all HTTP responses to protect against common web vulnerabilities
    /// </summary>
    public sealed class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityHeadersMiddleware> _logger;

        public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Prevent X-Frame-Options clickjacking attacks
            context.Response.Headers.Add("X-Frame-Options", "DENY");

            // Prevent MIME type sniffing
            context.Response.Headers.Add("X-Content-Type-Options", "nosniff");

            // Enable XSS Protection
            context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");

            // Content Security Policy - restrict resource loading
            context.Response.Headers.Add("Content-Security-Policy", 
                "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' https:;");

            // Referrer Policy - control referrer information
            context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

            // Feature Policy - restrict browser features
            context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

            // HSTS - enforce HTTPS (only in production)
            if (!context.Request.IsHttps && !string.Equals(
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development",
                    StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            }

            await _next(context);
        }
    }
}