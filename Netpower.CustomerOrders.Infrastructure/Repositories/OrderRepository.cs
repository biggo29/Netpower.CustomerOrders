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
    public sealed class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _db;

        public OrderRepository(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<OrderDto>> GetByCustomerAsync(
            Guid customerId,
            byte? status,
            DateTime? fromUtc,
            DateTime? toUtc,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _db.Orders.AsNoTracking().Where(o => o.CustomerId == customerId);

            if (status.HasValue) query = query.Where(o => o.Status == status.Value);
            if (fromUtc.HasValue) query = query.Where(o => o.OrderDateUtc >= fromUtc.Value);
            if (toUtc.HasValue) query = query.Where(o => o.OrderDateUtc < toUtc.Value);

            var skip = (pageNumber - 1) * pageSize;

            return await query
                .OrderByDescending(o => o.OrderDateUtc).ThenByDescending(o => o.Id) // stable paging
                .Skip(skip)
                .Take(pageSize)
                .Select(o => new OrderDto(
                    o.Id,
                    o.OrderNumber,
                    o.Status,
                    o.OrderDateUtc,
                    o.TotalAmount,
                    o.CustomerId
                ))
                .ToListAsync(ct);
        }

        public async Task<int> CountByCustomerAsync(
            Guid customerId,
            byte? status,
            DateTime? fromUtc,
            DateTime? toUtc,
            CancellationToken ct = default)
        {
            var query = _db.Orders.AsNoTracking().Where(o => o.CustomerId == customerId);

            if (status.HasValue) query = query.Where(o => o.Status == status.Value);
            if (fromUtc.HasValue) query = query.Where(o => o.OrderDateUtc >= fromUtc.Value);
            if (toUtc.HasValue) query = query.Where(o => o.OrderDateUtc < toUtc.Value);

            return await query.CountAsync(ct);
        }
    }
}
