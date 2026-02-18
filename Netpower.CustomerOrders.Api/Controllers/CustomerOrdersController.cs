using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Netpower.CustomerOrders.Application.Query;

namespace Netpower.CustomerOrders.Api.Controllers
{
    [Route("api/customers/{customerId:guid}/orders")]
    [ApiController]
    public sealed class CustomerOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerOrdersController> _logger;

        public CustomerOrdersController(IMediator mediator, ILogger<CustomerOrdersController> logger)
            => (_mediator, _logger) = (mediator, logger);

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
            _logger.LogInformation(
                "Received request to get orders for customer {CustomerId} with status {Status}, from {FromUtc} to {ToUtc}, page {PageNumber}, pageSize {PageSize}",
                customerId, status, fromUtc, toUtc, pageNumber, pageSize);

            // Guard: if user gives both and flips them, we normalize
            var originalFrom = fromUtc;
            var originalTo = toUtc;
            if (fromUtc.HasValue && toUtc.HasValue && fromUtc.Value > toUtc.Value)
            {
                (fromUtc, toUtc) = (toUtc, fromUtc);

                _logger.LogDebug(
                    "Normalized date range for customer {CustomerId} from {OriginalFromUtc}..{OriginalToUtc} to {FromUtc}..{ToUtc}",
                    customerId, originalFrom, originalTo, fromUtc, toUtc);
            }

            var result = await _mediator.Send(
                new GetCustomerOrdersQuery(customerId, status, fromUtc, toUtc, pageNumber, pageSize),
                ct);

            if (result.TotalCount == 0)
            {
                _logger.LogWarning(
                    "No orders found for customer {CustomerId} with filters status={Status}, from={FromUtc}, to={ToUtc}",
                    customerId, status, fromUtc, toUtc);
            }
            else
            {
                _logger.LogInformation(
                    "Returning {TotalCount} orders for customer {CustomerId} (page {PageNumber}, pageSize {PageSize})",
                    result.TotalCount, customerId, pageNumber, pageSize);
            }

            return Ok(result);
        }
    }
}
