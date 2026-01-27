using AutoMapper;
using ECommerce.Domain.Entities;
using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Banner;
using ECommerce.Application.DTOs.Dashboard;

namespace ECommerce.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(d => d.CompanyName, opt => opt.MapFrom(s => s.Company != null ? s.Company.Name : null))
            .ForMember(d => d.Roles, opt => opt.MapFrom(s => s.UserRoles.Select(r => r.RoleName).ToList()));

        CreateMap<UserFormDto, User>()
            .ForMember(d => d.PasswordHash, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.Company, opt => opt.Ignore())
            .ForMember(d => d.UserRoles, opt => opt.Ignore())
            .ForMember(d => d.CustomerProfile, opt => opt.Ignore());

        // Login History mappings
        CreateMap<LoginHistory, LoginHistoryDto>();
        CreateMap<LoginHistoryCreateDto, LoginHistory>();

        // Customer mappings
        CreateMap<Customer, CustomerDto>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"))
            .ForMember(d => d.CompanyName, opt => opt.MapFrom(s => s.Company != null ? s.Company.Name : null))
            .ForMember(d => d.TotalOrders, opt => opt.MapFrom(s => s.Orders != null ? s.Orders.Count : 0))
            .ForMember(d => d.TotalSpent, opt => opt.MapFrom(s => s.Orders != null ? s.Orders.Sum(o => o.TotalAmount) : 0));

        CreateMap<CustomerFormDto, Customer>()
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Company, opt => opt.Ignore())
            .ForMember(d => d.User, opt => opt.Ignore())
            .ForMember(d => d.Orders, opt => opt.Ignore())
            .ForMember(d => d.Addresses, opt => opt.Ignore())
            .ForMember(d => d.Reviews, opt => opt.Ignore());

        CreateMap<Customer, CustomerSummaryDto>()
            .ForMember(d => d.Name, opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"))
            .ForMember(d => d.OrderCount, opt => opt.MapFrom(s => s.Orders != null ? s.Orders.Count : 0))
            .ForMember(d => d.ReviewCount, opt => opt.MapFrom(s => s.Reviews != null ? s.Reviews.Count : 0))
            .ForMember(d => d.TotalOrders, opt => opt.MapFrom(s => s.Orders != null ? s.Orders.Count : 0))
            .ForMember(d => d.TotalSpent, opt => opt.MapFrom(s => s.Orders != null ? s.Orders.Sum(o => o.TotalAmount) : 0));

        // Address mappings
        CreateMap<Address, AddressDto>()
            .ForMember(d => d.CompanyName, opt => opt.MapFrom(s => s.Company != null ? s.Company.Name : null))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer != null ? $"{s.Customer.FirstName} {s.Customer.LastName}" : null));
        
        CreateMap<AddressFormDto, Address>();

        // Product mappings
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.BrandName, opt => opt.MapFrom(s => s.Brand != null ? s.Brand.Name : null))
            .ForMember(d => d.CompanyName, opt => opt.MapFrom(s => s.Company != null ? s.Company.Name : null))
            .ForMember(d => d.ReviewCount, opt => opt.MapFrom(s => s.Reviews != null ? s.Reviews.Count : 0))
            .ForMember(d => d.AverageRating, opt => opt.MapFrom(s => s.Reviews != null && s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : 0));

        CreateMap<ProductImage, ProductImageDto>();

        CreateMap<ProductFormDto, Product>()
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Category, opt => opt.Ignore())
            .ForMember(d => d.Brand, opt => opt.Ignore())
            .ForMember(d => d.Company, opt => opt.Ignore())
            .ForMember(d => d.OrderItems, opt => opt.Ignore())
            .ForMember(d => d.Reviews, opt => opt.Ignore());

        // Brand mappings
        CreateMap<Brand, BrandDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : null));
        CreateMap<BrandFormDto, Brand>();

        // Model mappings
        CreateMap<Model, ModelDto>()
            .ForMember(d => d.BrandName, opt => opt.MapFrom(s => s.Brand != null ? s.Brand.Name : null));
        CreateMap<ModelFormDto, Model>();

        // Global Attribute mappings
        CreateMap<GlobalAttribute, GlobalAttributeDto>();
        CreateMap<GlobalAttributeValue, GlobalAttributeValueDto>();
        CreateMap<GlobalAttributeFormDto, GlobalAttribute>();
        CreateMap<GlobalAttributeValueFormDto, GlobalAttributeValue>();

        // Category mappings
        CreateMap<Category, CategoryDto>();
        CreateMap<CategoryFormDto, Category>();

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer != null ? $"{s.Customer.FirstName} {s.Customer.LastName}" : null))
            .ForMember(d => d.CompanyName, opt => opt.MapFrom(s => s.Company != null ? s.Company.Name : null));

        CreateMap<OrderCreateDto, Order>()
            .ForMember(d => d.OrderDate, opt => opt.Ignore())
            .ForMember(d => d.TotalAmount, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.Ignore())
            .ForMember(d => d.Customer, opt => opt.Ignore())
            .ForMember(d => d.Address, opt => opt.Ignore())
            .ForMember(d => d.Company, opt => opt.Ignore());

        CreateMap<OrderUpdateDto, Order>()
            .ForMember(d => d.CustomerId, opt => opt.Ignore())
            .ForMember(d => d.CompanyId, opt => opt.Ignore())
            .ForMember(d => d.OrderDate, opt => opt.Ignore())
            .ForMember(d => d.TotalAmount, opt => opt.Ignore())
            .ForMember(d => d.Customer, opt => opt.Ignore())
            .ForMember(d => d.Address, opt => opt.Ignore())
            .ForMember(d => d.Company, opt => opt.Ignore())
            .ForMember(d => d.Items, opt => opt.Ignore());

        // OrderItem mappings
        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product != null ? s.Product.Name : null));

        CreateMap<OrderItemCreateDto, OrderItem>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.OrderId, opt => opt.Ignore())
            .ForMember(d => d.Order, opt => opt.Ignore())
            .ForMember(d => d.Product, opt => opt.Ignore());

        // Review mappings
        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product != null ? s.Product.Name : null))
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer != null ? $"{s.Customer.FirstName} {s.Customer.LastName}" : null));

        CreateMap<ReviewFormDto, Review>()
            .ForMember(d => d.ReviewerName, opt => opt.MapFrom(s => s.ReviewerName ?? "Anonymous"))
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Product, opt => opt.Ignore())
            .ForMember(d => d.Customer, opt => opt.Ignore())
            .ForMember(d => d.Company, opt => opt.Ignore());

        // Company mappings
        CreateMap<Company, CompanyDto>();
        CreateMap<CompanyFormDto, Company>()
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Users, opt => opt.Ignore())
            .ForMember(d => d.Customers, opt => opt.Ignore())
            .ForMember(d => d.Products, opt => opt.Ignore())
            .ForMember(d => d.Orders, opt => opt.Ignore())
            .ForMember(d => d.Reviews, opt => opt.Ignore());

        // Banner mappings
        CreateMap<Banner, BannerDto>();
        CreateMap<BannerFormDto, Banner>();

        // Campaign mappings
        CreateMap<Campaign, CampaignDto>();
        CreateMap<CampaignFormDto, Campaign>();

        // Notification mappings
        CreateMap<Notification, NotificationDto>();
        CreateMap<NotificationCreateDto, Notification>();

        // Request mappings
        CreateMap<Request, RequestDto>();
        CreateMap<RequestFormDto, Request>();
    }
}
