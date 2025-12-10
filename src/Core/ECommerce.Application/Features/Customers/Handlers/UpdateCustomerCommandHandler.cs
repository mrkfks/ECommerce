using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.Customers.Commands;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Customers.Handlers;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, ApiResponse<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateCustomerCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<CustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id);
        if (customer == null)
        {
            throw new NotFoundException("Müşteri bulunamadı");
        }

        _mapper.Map(request.Customer, customer);

        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        var customerDto = _mapper.Map<CustomerDto>(customer);

        return ApiResponse<CustomerDto>.SuccessResponse(customerDto, "Müşteri başarıyla güncellendi");
    }
}
