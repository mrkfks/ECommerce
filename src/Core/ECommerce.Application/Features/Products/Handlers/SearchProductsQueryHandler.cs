using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Features.Products.Queries;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Products.Handlers;

public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, ApiResponse<List<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<ProductDto>>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.SearchAsync(request.SearchTerm);
        var productDtos = _mapper.Map<List<ProductDto>>(products);

        return ApiResponse<List<ProductDto>>.SuccessResponse(productDtos);
    }
}
