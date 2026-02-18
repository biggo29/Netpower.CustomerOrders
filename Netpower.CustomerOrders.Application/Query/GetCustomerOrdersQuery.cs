using MediatR;
using Netpower.CustomerOrders.Application.Common.Models;
using Netpower.CustomerOrders.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.Application.Query
{
    public sealed record GetCustomerOrdersQuery(
        Guid CustomerId,
        byte? Status,
        DateTime? FromUtc,
        DateTime? ToUtc,
        int PageNumber = 1,
        int PageSize = 20
    ) : IRequest<PagedResult<OrderDto>>;
}
