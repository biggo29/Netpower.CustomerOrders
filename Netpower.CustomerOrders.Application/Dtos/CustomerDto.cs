using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.Application.Dtos
{
    public sealed class CustomerDto
    {
        public CustomerDto()
        {
        }

        public CustomerDto(Guid id, string firstName, string lastName, string email)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }

        public Guid Id { get; init; }
        public string FirstName { get; init; } = default!;
        public string LastName { get; init; } = default!;
        public string Email { get; init; } = default!;
    }
}
