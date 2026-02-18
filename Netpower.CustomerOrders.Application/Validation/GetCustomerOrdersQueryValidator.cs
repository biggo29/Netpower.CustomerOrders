using FluentValidation;
using Netpower.CustomerOrders.Application.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.Application.Validation
{
    public sealed class GetCustomerOrdersQueryValidator : AbstractValidator<GetCustomerOrdersQuery>
    {
        public GetCustomerOrdersQueryValidator()
        {
            RuleFor(x => x.CustomerId).NotEmpty();

            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 200);

            RuleFor(x => x)
                .Must(x => !(x.FromUtc.HasValue && x.ToUtc.HasValue && x.FromUtc.Value > x.ToUtc.Value))
                .WithMessage("'FromUtc' must be <= 'ToUtc'.");
        }
    }
}
