//using Netpower.CustomerOrders.Application.Common.Interfaces;
//using Netpower.CustomerOrders.Application.Dtos;
//using Netpower.CustomerOrders.Application.Dtos.Requests;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Netpower.CustomerOrders.Application.Services
//{
//    public sealed class CustomerService : ICustomerService
//    {
//        private readonly ICustomerRepository _customerRepository;

//        public CustomerService(ICustomerRepository customerRepository)
//        {
//            _customerRepository = customerRepository;
//        }

//        public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct)
//        {
//            // (Optional) business validations can go here (e.g., unique email) later.

//            var customer = new Customers
//            {
//                Id = Guid.NewGuid(),
//                FirstName = request.FirstName.Trim(),
//                LastName = request.LastName.Trim(),
//                Email = request.Email.Trim(),
//                PhoneNumber = request.PhoneNumber?.Trim(),

//                // Soft-delete fields (assumed to exist in entity)
//                IsDeleted = false,
//                DeletedAtUtc = null
//            };

//            await _customerRepository.AddAsync(customer, ct);
//            await _customerRepository.SaveChangesAsync(ct);

//            return MapToDto(customer);
//        }

//        public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken ct)
//        {
//            var customers = await _customerRepository.GetAllAsync(ct);

//            // Usually you do NOT return deleted customers from "GetAll"
//            return customers
//                .Where(c => !c.IsDeleted)
//                .Select(MapToDto)
//                .ToList();
//        }

//        public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken ct)
//        {
//            var customer = await _customerRepository.GetByIdAsync(id, ct);

//            if (customer is null || customer.IsDeleted)
//                return null;

//            return MapToDto(customer);
//        }

//        public async Task<bool> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct)
//        {
//            var customer = await _customerRepository.GetByIdAsync(id, ct);

//            if (customer is null || customer.IsDeleted)
//                return false;

//            customer.FirstName = request.FirstName.Trim();
//            customer.LastName = request.LastName.Trim();
//            customer.Email = request.Email.Trim();
//            customer.PhoneNumber = request.PhoneNumber?.Trim();

//            await _customerRepository.SaveChangesAsync(ct);
//            return true;
//        }

//        public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct)
//        {
//            var customer = await _customerRepository.GetByIdAsync(id, ct);

//            if (customer is null || customer.IsDeleted)
//                return false;

//            customer.IsDeleted = true;
//            customer.DeletedAtUtc = DateTime.UtcNow;

//            await _customerRepository.SaveChangesAsync(ct);
//            return true;
//        }

//        public Task<bool> ExistsAndActiveAsync(Guid id, CancellationToken ct)
//            => _customerRepository.ExistsAndActiveAsync(id, ct);

//        private static CustomerDto MapToDto(Customer customer)
//            => new CustomerDto
//            {
//                Id = customer.Id,
//                FirstName = customer.FirstName,
//                LastName = customer.LastName,
//                Email = customer.Email,
//                PhoneNumber = customer.PhoneNumber
//            };
//    }
//}
