using MediatR;
using Netpower.CustomerOrders.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.Application.Query
{

    public sealed record GetCustomerByIdQuery(Guid Id) : IRequest<CustomerDto?>;
}
