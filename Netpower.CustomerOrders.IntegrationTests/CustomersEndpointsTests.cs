using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Netpower.CustomerOrders.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.IntegrationTests
{
    public sealed class CustomersEndpointsTests
    {
        private const string Conn =
            "Server=DESKTOP-SHEVKKB\\SQLEXPRESS;Database=Netpower_IT;User Id=sa;Password=SqlServer;TrustServerCertificate=True;Encrypt=False;";

        [Fact]
        public async Task GetCustomerById_returns_200_for_existing_customer()
        {
            await using var factory = new CustomWebApplicationFactory(Conn);
            var client = factory.CreateClient();

            // Seed
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var seeded = await TestDataSeeder.SeedAsync(db);

                var response = await client.GetAsync($"/api/customers/{seeded.customerId}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task GetCustomerById_returns_404_for_missing_customer()
        {
            await using var factory = new CustomWebApplicationFactory(Conn);
            var client = factory.CreateClient();

            var response = await client.GetAsync($"/api/customers/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
