using Netpower.CustomerOrders.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.Application.Common.Interfaces
{
    public interface IOrderRepository
    {
        Task<IReadOnlyList<OrderDto>> GetByCustomerAsync(
            Guid customerId,
            byte? status,
            DateTime? fromUtc,
            DateTime? toUtc,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);

        Task<int> CountByCustomerAsync(
            Guid customerId,
            byte? status,
            DateTime? fromUtc,
            DateTime? toUtc,
            CancellationToken ct = default);
    }
}
