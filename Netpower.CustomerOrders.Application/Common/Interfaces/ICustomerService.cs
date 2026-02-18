using Netpower.CustomerOrders.Application.Dtos;
using Netpower.CustomerOrders.Application.Dtos.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.Application.Common.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct);
        Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken ct);
        Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<bool> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct);
        Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct);
        Task<bool> ExistsAndActiveAsync(Guid id, CancellationToken ct);
    }
}
