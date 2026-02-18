using Microsoft.EntityFrameworkCore;
using Netpower.CustomerOrders.Application.Common.Interfaces;
using Netpower.CustomerOrders.Application.Dtos;
using Netpower.CustomerOrders.Domain.Entities;
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

        public CustomerRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Customers customer, CancellationToken ct)
        {
            await _db.Customers.AddAsync(customer, ct);
        }

        public Task<Customers?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            // With global query filter, deleted customers will not be returned.
            // Without global filter, add: && !x.IsDeleted
            return _db.Customers.FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<IReadOnlyList<Customers>> GetAllAsync(CancellationToken ct)
        {
            // AsNoTracking: because service maps to DTO and doesn't modify entities here.
            return await _db.Customers
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync(ct);
        }

        public Task<bool> ExistsAndActiveAsync(Guid id, CancellationToken ct)
        {
            return _db.Customers.AnyAsync(x => x.Id == id, ct);
            // If no global filter, use:
            // return _db.Customers.AnyAsync(x => x.Id == id && !x.IsDeleted, ct);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct)
            => _db.SaveChangesAsync(ct);
    }
}
