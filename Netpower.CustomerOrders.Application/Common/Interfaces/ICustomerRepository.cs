using Netpower.CustomerOrders.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.Application.Common.Interfaces
{
    public interface ICustomerRepository
    {
        Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    }
}
