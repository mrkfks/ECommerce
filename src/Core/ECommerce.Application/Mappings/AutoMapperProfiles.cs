using AutoMapper;
using ECommerce.Domain.Entities;
using ECommerce.Application.DTOs;

namespace ECommerce.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User ↔ UserDto
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<User, UserCreateDto>().ReverseMap();
            CreateMap<User, UserUpdateDto>().ReverseMap();
            CreateMap<User, UserDto>()
                .ForMember(d => d.CompanyName, opt => opt.MapFrom(s => s.Company.Name))
                .ForMember(d => d.Roles, opt => opt.MapFrom(s => s.UserRoles.Select(r => r.RoleName).ToList()));
            CreateMap<UserCreateDto, User>();
            CreateMap<UserUpdateDto, User>();

            // Customer ↔ CustomerDto
            CreateMap<Customer, CustomerDto>().ReverseMap();
            CreateMap<Customer, CustomerCreateDto>().ReverseMap();
            CreateMap<Customer, CustomerUpdateDto>().ReverseMap();
            CreateMap<Customer, CustomerSummaryDto>();

            // Address ↔ AddressDto
            CreateMap<Address, AddressDto>().ReverseMap();

            // Product ↔ ProductDto
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Product, ProductCreateDto>().ReverseMap();
            CreateMap<Product, ProductUpdateDto>().ReverseMap();

            // Brand ↔ BrandDto
            CreateMap<Brand, BrandDto>().ReverseMap();

            // Category ↔ CategoryDto
            CreateMap<Category, CategoryDto>().ReverseMap();

            // Order ↔ OrderDto
            CreateMap<Order, OrderDto>().ReverseMap();
            CreateMap<Order, OrderCreateDto>().ReverseMap();
            CreateMap<Order, OrderUpdateDto>().ReverseMap();

            // OrderItem ↔ OrderItemDto
            CreateMap<OrderItem, OrderItemDto>().ReverseMap();
            CreateMap<OrderItem, OrderItemCreateDto>().ReverseMap();

            // Review ↔ ReviewDto
            CreateMap<Review, ReviewDto>().ReverseMap();
            CreateMap<Review, ReviewCreateDto>().ReverseMap();
            CreateMap<Review, ReviewUpdateDto>().ReverseMap();

            // Company ↔ CompanyDto
            CreateMap<Company, CompanyDto>().ReverseMap();
            CreateMap<Company, CompanyCreateDto>().ReverseMap();
            CreateMap<Company, CompanyUpdateDto>().ReverseMap();
        }
    }
}
