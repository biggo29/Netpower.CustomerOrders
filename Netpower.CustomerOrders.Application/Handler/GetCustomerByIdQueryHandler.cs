using MediatR;
using Netpower.CustomerOrders.Application.Common.Interfaces;
using Netpower.CustomerOrders.Application.Dtos;
using Netpower.CustomerOrders.Application.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.Application.Handler
{
    public sealed class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
    {
        private readonly ICustomerRepository _customers;

        public GetCustomerByIdQueryHandler(ICustomerRepository customers)
            => _customers = customers;

        public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var customer = await _customers.GetByIdAsync(request.Id, cancellationToken);
            if (customer is null)
            {
                return null;
            }

            return new CustomerDto
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email
            };
        }
    }
}
