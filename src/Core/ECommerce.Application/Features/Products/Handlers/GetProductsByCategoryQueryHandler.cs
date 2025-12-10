using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Features.Products.Queries;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Products.Handlers;

public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, ApiResponse<List<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductsByCategoryQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<ProductDto>>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetByCategoryAsync(request.CategoryId);
        var productDtos = _mapper.Map<List<ProductDto>>(products);

        return ApiResponse<List<ProductDto>>.SuccessResponse(productDtos);
    }
}
