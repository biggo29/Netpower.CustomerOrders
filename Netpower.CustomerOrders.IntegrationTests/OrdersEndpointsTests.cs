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
    public sealed class OrdersEndpointsTests
    {
        private const string Conn =
            "Server=DESKTOP-SHEVKKB\\SQLEXPRESS;Database=Netpower_IT;User Id=sa;Password=SqlServer;TrustServerCertificate=True;Encrypt=False;";

        [Fact]
        public async Task GetCustomerOrders_returns_200_and_paged_payload()
        {
            await using var factory = new CustomWebApplicationFactory(Conn);
            var client = factory.CreateClient();

            Guid customerId;
            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                (customerId, _) = await TestDataSeeder.SeedAsync(db);
            }

            var response = await client.GetAsync($"/api/customers/{customerId}/orders?pageNumber=1&pageSize=10");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var json = await response.Content.ReadAsStringAsync();
            json.Should().Contain("\"items\"");
            json.Should().Contain("\"totalCount\"");
            json.Should().Contain("\"pageNumber\"");
            json.Should().Contain("\"pageSize\"");
        }

        [Fact]
        public async Task GetCustomerOrders_returns_400_for_invalid_pageSize()
        {
            await using var factory = new CustomWebApplicationFactory(Conn);
            var client = factory.CreateClient();

            var response = await client.GetAsync($"/api/customers/{Guid.NewGuid()}/orders?pageNumber=1&pageSize=9999");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
