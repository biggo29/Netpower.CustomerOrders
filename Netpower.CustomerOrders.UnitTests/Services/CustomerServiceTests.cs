using Moq;
using Netpower.CustomerOrders.Application.Common.Interfaces;
using Netpower.CustomerOrders.Application.Dtos.Requests;
using Netpower.CustomerOrders.Application.Services;
using Netpower.CustomerOrders.Domain.Entities;

namespace Netpower.CustomerOrders.UnitTests.Services
{
    /// <summary>
    /// Unit tests for CustomerService with mocked dependencies
    /// </summary>
    public sealed class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _mockRepository;
        private readonly CustomerService _sut; // System Under Test

        public CustomerServiceTests()
        {
            _mockRepository = new Mock<ICustomerRepository>();
            _sut = new CustomerService(_mockRepository.Object);
        }

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidRequest_CreatesCustomerAndReturnsDto()
        {
            // Arrange
            var request = new CreateCustomerRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "1234567890"
            };

            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<Customers>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockRepository
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _sut.CreateAsync(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.Email.Should().Be("john.doe@example.com");
            result.Id.Should().NotBeEmpty();

            _mockRepository.Verify(
                r => r.AddAsync(It.Is<Customers>(c => 
                    c.FirstName == "John" && 
                    c.LastName == "Doe" && 
                    c.Email == "john.doe@example.com"),
                It.IsAny<CancellationToken>()),
                Times.Once);

            _mockRepository.Verify(
                r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithWhitespaceInNames_TrimsValues()
        {
            // Arrange
            var request = new CreateCustomerRequest
            {
                FirstName = "  John  ",
                LastName = "  Doe  ",
                Email = "  john.doe@example.com  ",
                PhoneNumber = "1234567890"
            };

            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<Customers>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockRepository
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _sut.CreateAsync(request, CancellationToken.None);

            // Assert
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.Email.Should().Be("john.doe@example.com");
        }

        [Fact]
        public async Task CreateAsync_CallsAddAsyncAndSaveChangesAsync()
        {
            // Arrange
            var request = new CreateCustomerRequest
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com"
            };

            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<Customers>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockRepository
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _sut.CreateAsync(request, CancellationToken.None);

            // Assert
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Customers>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithExistingCustomer_ReturnsCustomerDto()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customers
            {
                Id = customerId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IsDeleted = false
            };

            _mockRepository
                .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer);

            // Act
            var result = await _sut.GetByIdAsync(customerId, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(customerId);
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.Email.Should().Be("john@example.com");

            _mockRepository.Verify(
                r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentCustomer_ReturnsNull()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _mockRepository
                .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Customers?)null);

            // Act
            var result = await _sut.GetByIdAsync(customerId, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WithDeletedCustomer_ReturnsNull()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customers
            {
                Id = customerId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IsDeleted = true,
                DeletedAtUtc = DateTime.UtcNow
            };

            _mockRepository
                .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer);

            // Act
            var result = await _sut.GetByIdAsync(customerId, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithMultipleActiveCustomers_ReturnsAllNonDeletedCustomers()
        {
            // Arrange
            var customers = new List<Customers>
            {
                new Customers
                {
                    Id = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john@example.com",
                    IsDeleted = false
                },
                new Customers
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane@example.com",
                    IsDeleted = false
                },
                new Customers
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Bob",
                    LastName = "Johnson",
                    Email = "bob@example.com",
                    IsDeleted = true // Should be filtered out
                }
            };

            _mockRepository
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(customers);

            // Act
            var result = await _sut.GetAllAsync(CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.Should().NotContain(c => c.FirstName == "Bob");
            result.Should().Contain(c => c.FirstName == "John");
            result.Should().Contain(c => c.FirstName == "Jane");
        }

        [Fact]
        public async Task GetAllAsync_WithNoCustomers_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Customers>());

            // Act
            var result = await _sut.GetAllAsync(CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_FiltersOutDeletedCustomers()
        {
            // Arrange
            var customers = new List<Customers>
            {
                new Customers { Id = Guid.NewGuid(), FirstName = "Active", LastName = "User", Email = "active@example.com", IsDeleted = false },
                new Customers { Id = Guid.NewGuid(), FirstName = "Deleted", LastName = "User", Email = "deleted@example.com", IsDeleted = true }
            };

            _mockRepository
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(customers);

            // Act
            var result = await _sut.GetAllAsync(CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result.First().FirstName.Should().Be("Active");
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithExistingCustomer_UpdatesAndReturnsTrue()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var existingCustomer = new Customers
            {
                Id = customerId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IsDeleted = false
            };

            var updateRequest = new UpdateCustomerRequest
            {
                FirstName = "Jonathan",
                LastName = "Doe-Smith",
                Email = "jonathan@example.com"
            };

            _mockRepository
                .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCustomer);

            _mockRepository
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _sut.UpdateAsync(customerId, updateRequest, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            existingCustomer.FirstName.Should().Be("Jonathan");
            existingCustomer.LastName.Should().Be("Doe-Smith");
            existingCustomer.Email.Should().Be("jonathan@example.com");
            existingCustomer.UpdatedAtUtc.Should().NotBeNull();

            _mockRepository.Verify(
                r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentCustomer_ReturnsFalse()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var updateRequest = new UpdateCustomerRequest
            {
                FirstName = "Jonathan",
                LastName = "Doe-Smith",
                Email = "jonathan@example.com"
            };

            _mockRepository
                .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Customers?)null);

            // Act
            var result = await _sut.UpdateAsync(customerId, updateRequest, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(
                r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WithDeletedCustomer_ReturnsFalse()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var deletedCustomer = new Customers
            {
                Id = customerId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IsDeleted = true
            };

            var updateRequest = new UpdateCustomerRequest
            {
                FirstName = "Jonathan",
                LastName = "Doe-Smith",
                Email = "jonathan@example.com"
            };

            _mockRepository
                .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(deletedCustomer);

            // Act
            var result = await _sut.UpdateAsync(customerId, updateRequest, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(
                r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_TrimsInputValues()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var existingCustomer = new Customers
            {
                Id = customerId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IsDeleted = false
            };

            var updateRequest = new UpdateCustomerRequest
            {
                FirstName = "  Jonathan  ",
                LastName = "  Smith  ",
                Email = "  jonathan@example.com  "
            };

            _mockRepository
                .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCustomer);

            _mockRepository
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _sut.UpdateAsync(customerId, updateRequest, CancellationToken.None);

            // Assert
            existingCustomer.FirstName.Should().Be("Jonathan");
            existingCustomer.LastName.Should().Be("Smith");
            existingCustomer.Email.Should().Be("jonathan@example.com");
        }

        #endregion

        #region SoftDeleteAsync Tests

        [Fact]
        public async Task SoftDeleteAsync_WithExistingCustomer_MarkAsDeletedAndReturnsTrue()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customers
            {
                Id = customerId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IsDeleted = false
            };

            _mockRepository
                .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer);

            _mockRepository
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _sut.SoftDeleteAsync(customerId, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            customer.IsDeleted.Should().BeTrue();
            customer.DeletedAtUtc.Should().NotBeNull();
            customer.UpdatedAtUtc.Should().NotBeNull();

            _mockRepository.Verify(
                r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SoftDeleteAsync_WithNonExistentCustomer_ReturnsFalse()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _mockRepository
                .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Customers?)null);

            // Act
            var result = await _sut.SoftDeleteAsync(customerId, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(
                r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task SoftDeleteAsync_WithAlreadyDeletedCustomer_ReturnsFalse()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customers
            {
                Id = customerId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                IsDeleted = true,
                DeletedAtUtc = DateTime.UtcNow
            };

            _mockRepository
                .Setup(r => r.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customer);

            // Act
            var result = await _sut.SoftDeleteAsync(customerId, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _mockRepository.Verify(
                r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion

        #region ExistsAndActiveAsync Tests

        [Fact]
        public async Task ExistsAndActiveAsync_WithExistingCustomer_ReturnsTrue()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _mockRepository
                .Setup(r => r.ExistsAndActiveAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.ExistsAndActiveAsync(customerId, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(
                r => r.ExistsAndActiveAsync(customerId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ExistsAndActiveAsync_WithNonExistentCustomer_ReturnsFalse()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            _mockRepository
                .Setup(r => r.ExistsAndActiveAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.ExistsAndActiveAsync(customerId, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        #endregion
    }
}