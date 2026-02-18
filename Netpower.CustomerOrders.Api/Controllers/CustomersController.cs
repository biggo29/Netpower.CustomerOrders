using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Netpower.CustomerOrders.Application.Common.Interfaces;
using Netpower.CustomerOrders.Application.Dtos.Requests;

namespace Netpower.CustomerOrders.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all endpoints
    [Produces("application/json")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
            => (_customerService, _logger) = (customerService, logger);

        /// <summary>
        /// Get all customers (requires authentication)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var userId = GetUserIdFromToken();
            _logger.LogInformation("User {UserId} requested all customers", userId);

            var customers = await _customerService.GetAllAsync(ct);

            _logger.LogInformation("User {UserId} retrieved {Count} customers", userId, customers.Count);
            return Ok(customers);
        }

        /// <summary>
        /// Get customer by ID (requires authentication)
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            var userId = GetUserIdFromToken();
            _logger.LogInformation("User {UserId} requested customer {CustomerId}", userId, id);

            var customer = await _customerService.GetByIdAsync(id, ct);

            if (customer is null)
            {
                _logger.LogWarning("User {UserId} requested non-existent customer {CustomerId}", userId, id);
                return NotFound();
            }

            return Ok(customer);
        }

        /// <summary>
        /// Create a new customer (requires authentication)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken ct)
        {
            var userId = GetUserIdFromToken();
            _logger.LogInformation("User {UserId} creating customer with email {Email}", userId, MaskEmail(request.Email));

            var customer = await _customerService.CreateAsync(request, ct);

            _logger.LogInformation("User {UserId} created customer {CustomerId}", userId, customer.Id);
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }

        /// <summary>
        /// Update an existing customer (requires authentication)
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken ct)
        {
            var userId = GetUserIdFromToken();
            _logger.LogInformation("User {UserId} updating customer {CustomerId}", userId, id);

            var success = await _customerService.UpdateAsync(id, request, ct);

            if (!success)
            {
                _logger.LogWarning("User {UserId} attempted to update non-existent customer {CustomerId}", userId, id);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} updated customer {CustomerId}", userId, id);
            return NoContent();
        }

        /// <summary>
        /// Delete a customer - soft delete for GDPR compliance (requires authentication)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
        {
            var userId = GetUserIdFromToken();
            _logger.LogInformation("User {UserId} deleting customer {CustomerId}", userId, id);

            var success = await _customerService.SoftDeleteAsync(id, ct);

            if (!success)
            {
                _logger.LogWarning("User {UserId} attempted to delete non-existent customer {CustomerId}", userId, id);
                return NotFound();
            }

            _logger.LogInformation("User {UserId} deleted customer {CustomerId}", userId, id);
            return NoContent();
        }

        /// <summary>
        /// Export customer data (GDPR - Right to be forgotten/data portability)
        /// </summary>
        [HttpGet("{id:guid}/export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportData([FromRoute] Guid id, CancellationToken ct)
        {
            var userId = GetUserIdFromToken();
            _logger.LogInformation("User {UserId} requested data export for customer {CustomerId}", userId, id);

            var customer = await _customerService.GetByIdAsync(id, ct);
            if (customer is null)
            {
                return NotFound();
            }

            return Ok(new { data = customer, exportedAt = DateTime.UtcNow });
        }

        /// <summary>
        /// Helper method to extract user ID from JWT token
        /// </summary>
        private string GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim?.Value ?? "Unknown";
        }

        /// <summary>
        /// Mask email for logging (GDPR - data minimization)
        /// </summary>
        private static string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                return "***";

            var parts = email.Split('@');
            var localPart = parts[0];
            var visibleChars = Math.Max(1, localPart.Length / 3);
            var maskedLocal = localPart.Substring(0, visibleChars) + new string('*', localPart.Length - visibleChars);
            return $"{maskedLocal}@{parts[1]}";
        }
    }
}
