using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Netpower.CustomerOrders.Application.Query;

namespace Netpower.CustomerOrders.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerOrdersController> _logger;

        public CustomersController(IMediator mediator, ILogger<CustomerOrdersController> logger) 
            => (_mediator, _logger) = (mediator, logger);

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            _logger.LogInformation("Request received: Get customer by id {CustomerId}", id);

            var customer = await _mediator.Send(new GetCustomerByIdQuery(id), ct);

            if (customer is null)
            {
                _logger.LogWarning("Customer not found: {CustomerId}", id);
                return NotFound();
            }

            _logger.LogInformation("Customer retrieved: {CustomerId}", id);
            return Ok(customer);
        }
    }
}
