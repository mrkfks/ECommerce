using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.Customers.Commands;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Customers.Handlers;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCustomerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.Id);
        if (customer == null)
        {
            throw new NotFoundException("Müşteri bulunamadı");
        }

        _unitOfWork.Customers.Delete(customer);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Müşteri başarıyla silindi");
    }
}
