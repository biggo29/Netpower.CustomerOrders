using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.Application.Dtos
{
    public sealed record OrderDto(
        Guid Id,
        string OrderNumber,
        byte Status,
        DateTime OrderDateUtc,
        decimal TotalAmount,
        Guid CustomerId
    );
}
