using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Features.Customers.Commands;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Features.Customers.Handlers;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, ApiResponse<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCustomerCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = _mapper.Map<Customer>(request.Customer);
        customer.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        var customerDto = _mapper.Map<CustomerDto>(customer);

        return ApiResponse<CustomerDto>.SuccessResponse(customerDto, "Müşteri başarıyla oluşturuldu");
    }
}
