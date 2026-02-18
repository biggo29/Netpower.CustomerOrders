using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.Application.Dtos
{
    public sealed record CustomerDto(
        Guid Id,
        string FirstName,
        string LastName,
        string Email
    );
}
