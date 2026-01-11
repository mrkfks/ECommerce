using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Features.Products.Queries;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Products.Handlers;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, ApiResponseDto<PaginatedResult<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponseDto<PaginatedResult<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        // Ideally Repositories should support Skip/Take at DB level.
        // Assuming unitOfWork.Products returns a repository that can return IQueryable or Paged List.
        // If not available, we must implement it.
        // For now, let's check what we have. If strictly IGenericRepository, it might need updates.
        
        // TEMPORARY: Fetch all and paginate in memory if repo doesn't support it, 
        // BUT for a "Senior Architect" role, I should update the Repository.
        // Let's assume Update of Repository is next step.
        // Here I will use a Paged method that I WILL Implement.
        
        var totalCount = await _unitOfWork.Products.CountAsync();
        var products = await _unitOfWork.Products.GetPagedAsync(request.PageNumber, request.PageSize);
        var productDtos = _mapper.Map<List<ProductDto>>(products);
        
        var paginatedResult = new PaginatedResult<ProductDto>(productDtos, totalCount, request.PageNumber, request.PageSize);

        return new ApiResponseDto<PaginatedResult<ProductDto>>
        {
            Success = true,
            Data = paginatedResult,
            Message = "Products fetched successfully"
        };
    }
}
