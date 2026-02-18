using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Netpower.CustomerOrders.Application.Common.Interfaces;
using Netpower.CustomerOrders.Application.Dtos.Requests;
using Netpower.CustomerOrders.Application.Query;

namespace Netpower.CustomerOrders.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
            => (_customerService, _logger) = (customerService, logger);

        /// <summary>
        /// Get all customers
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            _logger.LogInformation("Request received: Get all customers");

            var customers = await _customerService.GetAllAsync(ct);

            _logger.LogInformation("Retrieved {Count} customers", customers.Count);
            return Ok(customers);
        }

        /// <summary>
        /// Get customer by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            _logger.LogInformation("Request received: Get customer by id {CustomerId}", id);

            var customer = await _customerService.GetByIdAsync(id, ct);

            if (customer is null)
            {
                _logger.LogWarning("Customer not found: {CustomerId}", id);
                return NotFound();
            }

            _logger.LogInformation("Customer retrieved: {CustomerId}", id);
            return Ok(customer);
        }

        /// <summary>
        /// Create a new customer
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Request received: Create customer {Email}", request.Email);

            var customer = await _customerService.CreateAsync(request, ct);

            _logger.LogInformation("Customer created: {CustomerId}", customer.Id);
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }

        /// <summary>
        /// Update an existing customer
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken ct)
        {
            _logger.LogInformation("Request received: Update customer {CustomerId}", id);

            var success = await _customerService.UpdateAsync(id, request, ct);

            if (!success)
            {
                _logger.LogWarning("Customer not found for update: {CustomerId}", id);
                return NotFound();
            }

            _logger.LogInformation("Customer updated: {CustomerId}", id);
            return NoContent();
        }

        /// <summary>
        /// Delete a customer (soft delete)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
        {
            _logger.LogInformation("Request received: Delete customer {CustomerId}", id);

            var success = await _customerService.SoftDeleteAsync(id, ct);

            if (!success)
            {
                _logger.LogWarning("Customer not found for deletion: {CustomerId}", id);
                return NotFound();
            }

            _logger.LogInformation("Customer deleted: {CustomerId}", id);
            return NoContent();
        }
    }
}
