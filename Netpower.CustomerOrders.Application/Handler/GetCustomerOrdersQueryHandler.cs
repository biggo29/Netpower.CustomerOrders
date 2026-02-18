using MediatR;
using Netpower.CustomerOrders.Application.Common.Interfaces;
using Netpower.CustomerOrders.Application.Common.Models;
using Netpower.CustomerOrders.Application.Dtos;
using Netpower.CustomerOrders.Application.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.Application.Handler
{
    public sealed class GetCustomerOrdersQueryHandler
        : IRequestHandler<GetCustomerOrdersQuery, PagedResult<OrderDto>>
    {
        private readonly IOrderRepository _orders;

        public GetCustomerOrdersQueryHandler(IOrderRepository orders)
            => _orders = orders;

        public async Task<PagedResult<OrderDto>> Handle(GetCustomerOrdersQuery request, CancellationToken ct)
        {
            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

            // Optional: enforce a max page size to protect the DB
            if (pageSize > 200) pageSize = 200;

            var total = await _orders.CountByCustomerAsync(
                request.CustomerId,
                request.Status,
                request.FromUtc,
                request.ToUtc,
                ct);

            var items = total == 0
                ? Array.Empty<OrderDto>()
                : await _orders.GetByCustomerAsync(
                    request.CustomerId,
                    request.Status,
                    request.FromUtc,
                    request.ToUtc,
                    pageNumber,
                    pageSize,
                    ct);

            return new PagedResult<OrderDto>(items, total, pageNumber, pageSize);
        }
    }
}
