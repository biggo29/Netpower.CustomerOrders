using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Netpower.CustomerOrders.Infrastructure.Persistence;

namespace Netpower.CustomerOrders.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _testConn;

    public CustomWebApplicationFactory(string testConn) => _testConn = testConn;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((ctx, config) =>
        {
            // Strongest override: in-memory settings that win over appsettings + secrets
            var dict = new Dictionary<string, string?>
            {
                // Use the SAME key your API reads. If unsure, override multiple.
                ["ConnectionStrings:DefaultConnection"] = _testConn,
                ["ConnectionStrings:Netpower"] = _testConn,
                ["ConnectionStrings:ConnectionString"] = _testConn
            };

            config.AddInMemoryCollection(dict);
        });

        builder.ConfigureServices(services =>
        {
            // Remove ANY EF registration that could remain
            services.RemoveAll(typeof(AppDbContext));
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(IDbContextFactory<AppDbContext>));

            // Re-register using test DB
            services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(_testConn));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Fail-fast guard: never allow production DB
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        string databaseName = "DB=" + db.Database.GetDbConnection().Database;
        string connStr = "Conn=" + db.Database.GetDbConnection().ConnectionString;
        var dbName = db.Database.GetDbConnection().Database;

        if (!dbName.Equals("Netpower_IT", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"❌ Tests using '{dbName}' not 'Netpower_IT'.");

        return host;
    }
}
