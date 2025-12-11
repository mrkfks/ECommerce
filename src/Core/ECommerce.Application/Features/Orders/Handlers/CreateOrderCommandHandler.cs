using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.Orders.Commands;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using MediatR;

namespace ECommerce.Application.Features.Orders.Handlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, ApiResponse<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Müşteri kontrolü
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.Order.CustomerId);
        if (customer == null)
        {
            throw new NotFoundException("Müşteri bulunamadı");
        }

        // Adres kontrolü
        var address = await _unitOfWork.Addresses.GetByIdAsync(request.Order.AddressId);
        if (address == null)
        {
            throw new NotFoundException("Adres bulunamadı");
        }

        // Stok kontrolü ve toplam hesaplama
        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in request.Order.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            if (product == null)
            {
                throw new NotFoundException($"Ürün bulunamadı: {item.ProductId}");
            }

            if (product.StockQuantity < item.Quantity)
            {
                throw new BusinessException($"Yetersiz stok: {product.Name}");
            }

            // Stok düşür
            product.StockQuantity -= item.Quantity;
            _unitOfWork.Products.Update(product);

            var orderItem = new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            };

            orderItems.Add(orderItem);
            totalAmount += orderItem.Quantity * orderItem.UnitPrice;
        }

        // Sipariş oluştur
        var order = new Order
        {
            CustomerId = request.Order.CustomerId,
            AddressId = request.Order.AddressId,
            CompanyId = request.Order.CompanyId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending,
            Items = orderItems
        };

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        var orderDto = _mapper.Map<OrderDto>(order);

        return ApiResponse<OrderDto>.SuccessResponse(orderDto, "Sipariş başarıyla oluşturuldu");
    }
}
