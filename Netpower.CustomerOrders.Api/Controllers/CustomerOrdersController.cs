using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Netpower.CustomerOrders.Application.Query;

namespace Netpower.CustomerOrders.Api.Controllers
{
    [Route("api/customers/{customerId:guid}/orders")]
    [ApiController]
    public sealed class CustomerOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomerOrdersController(IMediator mediator)
            => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromRoute] Guid customerId,
            [FromQuery] byte? status,
            [FromQuery] DateTime? fromUtc,
            [FromQuery] DateTime? toUtc,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            // Guard: if user gives both and flips them, we normalize
            if (fromUtc.HasValue && toUtc.HasValue && fromUtc.Value > toUtc.Value)
            {
                (fromUtc, toUtc) = (toUtc, fromUtc);
            }

            var result = await _mediator.Send(
                new GetCustomerOrdersQuery(customerId, status, fromUtc, toUtc, pageNumber, pageSize),
                ct);

            return Ok(result);
        }
    }
}
