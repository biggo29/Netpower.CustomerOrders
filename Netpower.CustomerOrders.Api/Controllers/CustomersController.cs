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

        public CustomersController(IMediator mediator) => _mediator = mediator;

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            var customer = await _mediator.Send(new GetCustomerByIdQuery(id), ct);

            return customer is null ? NotFound() : Ok(customer);
        }
    }
}
