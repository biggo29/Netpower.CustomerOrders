using Netpower.CustomerOrders.Application.Common.Interfaces;
using Netpower.CustomerOrders.Application.Dtos;
using Netpower.CustomerOrders.Application.Dtos.Requests;
using Netpower.CustomerOrders.Domain.Entities;

//using Netpower.CustomerOrders.Infrastructure.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netpower.CustomerOrders.Application.Services
{
    public sealed class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repo;

        public CustomerService(ICustomerRepository repo)
        {
            _repo = repo;
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct)
        {
            var customer = new Customers
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = request.Email.Trim(),
                //PhoneNumber = request.PhoneNumber?.Trim(),
                IsDeleted = false,
                DeletedAtUtc = null,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _repo.AddAsync(customer, ct);
            await _repo.SaveChangesAsync(ct);

            return Map(customer);
        }

        public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken ct)
        {
            var customers = await _repo.GetAllAsync(ct);

            return customers
                .Where(x => !x.IsDeleted)
                .Select(Map)
                .ToList();
        }

        public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var customer = await _repo.GetByIdAsync(id, ct);

            if (customer is null || customer.IsDeleted)
                return null;

            return Map(customer);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct)
        {
            var customer = await _repo.GetByIdAsync(id, ct);

            if (customer is null || customer.IsDeleted)
                return false;

            customer.FirstName = request.FirstName.Trim();
            customer.LastName = request.LastName.Trim();
            customer.Email = request.Email.Trim();
            //customer.PhoneNumber = request.PhoneNumber?.Trim();
            customer.UpdatedAtUtc = DateTime.UtcNow;

            await _repo.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct)
        {
            var customer = await _repo.GetByIdAsync(id, ct);

            if (customer is null || customer.IsDeleted)
                return false;

            customer.IsDeleted = true;
            customer.DeletedAtUtc = DateTime.UtcNow;
            customer.UpdatedAtUtc = DateTime.UtcNow;

            await _repo.SaveChangesAsync(ct);
            return true;
        }

        public Task<bool> ExistsAndActiveAsync(Guid id, CancellationToken ct)
            => _repo.ExistsAndActiveAsync(id, ct);

        private static CustomerDto Map(Customers c) => new()
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email
        };
    }
}
