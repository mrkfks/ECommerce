using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Hubs;
using ECommerce.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ECommerce.UnitTests
{
    public class OrderServiceTests
    {
        private readonly OrderService _orderService;
        private readonly AppDbContext _context;
        private readonly Mock<ITenantService> _tenantServiceMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<ILogger<OrderService>> _loggerMock;
        private readonly Mock<IHubContext<NotificationHub>> _hubContextMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<IPaymentService> _paymentServiceMock;

        public OrderServiceTests()
        {
            // Setup InMemory DB
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            // Mocks
            _tenantServiceMock = new Mock<ITenantService>();
            _productServiceMock = new Mock<IProductService>();
            _loggerMock = new Mock<ILogger<OrderService>>();
            _hubContextMock = new Mock<IHubContext<NotificationHub>>();
            _cacheServiceMock = new Mock<ICacheService>();
            _paymentServiceMock = new Mock<IPaymentService>();

            // Setup Tenant Context
            _context = new AppDbContext(options, _tenantServiceMock.Object);

            // Setup Hub Mock
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();
            clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(clientProxyMock.Object);
            _hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);
            
            // Setup Tenant
            _tenantServiceMock.Setup(t => t.GetCompanyId()).Returns(1);

            _orderService = new OrderService(
                _context,
                _tenantServiceMock.Object,
                _productServiceMock.Object,
                _loggerMock.Object,
                _hubContextMock.Object,
                _cacheServiceMock.Object,
                _paymentServiceMock.Object
            );
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenPaymentFails()
        {
            // Arrange
            var dto = new OrderCreateDto
            {
                CustomerId = 1,
                AddressId = 1,
                CompanyId = 1,
                CardNumber = "5555", // Invalid
                CardExpiry = "12/25",
                CardCvv = "123",
                Items = new List<OrderItemCreateDto> 
                { 
                    new OrderItemCreateDto { ProductId = 1, Quantity = 1 } 
                }
            };

            // Payment fails
            _paymentServiceMock.Setup(p => p.ValidatePayment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _orderService.CreateAsync(dto));
            
            // Verify no order saved
            Assert.Equal(0, await _context.Orders.CountAsync());
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenStockIsInsufficient()
        {
            // Arrange
            var dto = new OrderCreateDto
            {
                CustomerId = 1,
                AddressId = 1,
                CompanyId = 1,
                CardNumber = "4444", // Valid
                CardExpiry = "12/25",
                CardCvv = "123",
                Items = new List<OrderItemCreateDto> 
                { 
                    new OrderItemCreateDto { ProductId = 1, Quantity = 100 } 
                }
            };

            // Setup Product in DB (referenced by id 1)
            var product = Product.Create("Test Product", "Desc", 100m, 1, 1, 1, 10, 1);
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Payment succeeds
            _paymentServiceMock.Setup(p => p.ValidatePayment(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // Stock service DecreaseStockAsync throws exception (simulating atomic failure)
            _productServiceMock.Setup(p => p.DecreaseStockAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("Yetersiz stok"));

            // Act & Assert
            // Expect exception
            await Assert.ThrowsAsync<Exception>(() => _orderService.CreateAsync(dto));
        }
    }
}
