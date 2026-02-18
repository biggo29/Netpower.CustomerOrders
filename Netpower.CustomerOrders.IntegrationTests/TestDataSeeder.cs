using Netpower.CustomerOrders.Infrastructure.Persistence;
using Netpower.CustomerOrders.Infrastructure.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.IntegrationTests
{
    public static class TestDataSeeder
    {
        public static async Task<(Guid customerId, Guid orderId)> SeedAsync(AppDbContext db)
        {
            // NOTE: adjust entity/property names if scaffolding differs
            var customerId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            db.Customers.Add(new Customers
            {
                Id = customerId,
                CustomerNumber = $"CUST-{Guid.NewGuid():N}".Substring(0, 12),
                FirstName = "Test",
                LastName = "Customer",
                Email = $"test{Guid.NewGuid():N}@example.com"
            });

            db.Orders.Add(new Orders
            {
                Id = orderId,
                OrderNumber = $"ORD-{Guid.NewGuid():N}".Substring(0, 12),
                CustomerId = customerId,
                Status = 2,
                OrderDateUtc = DateTime.UtcNow,
                TotalAmount = 100m
            });

            await db.SaveChangesAsync();
            return (customerId, orderId);
        }
    }
}
