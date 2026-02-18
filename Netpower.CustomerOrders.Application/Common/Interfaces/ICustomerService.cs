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

        /// <summary>
        /// Returns true if update succeeded, false if customer not found (or deleted).
        /// </summary>
        Task<bool> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct);

        /// <summary>
        /// Soft-delete (logical delete). Returns true if succeeded, false if not found (or already deleted).
        /// </summary>
        Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct);

        /// <summary>
        /// Used by order creation: validates the customer exists and is not soft-deleted.
        /// </summary>
        Task<bool> ExistsAndActiveAsync(Guid id, CancellationToken ct);
    }
}
