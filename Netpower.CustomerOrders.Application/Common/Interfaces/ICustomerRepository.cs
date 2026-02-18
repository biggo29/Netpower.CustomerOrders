using Netpower.CustomerOrders.Application.Dtos;
using Netpower.CustomerOrders.Domain.Entities;
//using Netpower.CustomerOrders.Infrastructure.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.Application.Common.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customers?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task AddAsync(Customers customer, CancellationToken ct);
        //Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<IReadOnlyList<Customers>> GetAllAsync(CancellationToken ct);

        Task<bool> ExistsAndActiveAsync(Guid id, CancellationToken ct);

        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
