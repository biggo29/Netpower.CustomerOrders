using Microsoft.EntityFrameworkCore;
using Netpower.CustomerOrders.Application.Common.Interfaces;
using Netpower.CustomerOrders.Application.Dtos;
using Netpower.CustomerOrders.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.Infrastructure.Repositories
{
    public sealed class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _db;

        public CustomerRepository(AppDbContext db) => _db = db;

        public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Customers
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new CustomerDto(
                    x.Id,
                    x.FirstName,
                    x.LastName,
                    x.Email
                ))
                .FirstOrDefaultAsync(ct);
        }
    }
}
