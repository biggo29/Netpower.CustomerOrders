using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.Application.Dtos.Requests
{
    public sealed class CreateCustomerRequest
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; init; } = default!;

        [Required]
        [MaxLength(100)]
        public string LastName { get; init; } = default!;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; init; } = default!;

        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; init; }
    }
}
