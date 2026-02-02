using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class OrderFormDtoValidator : AbstractValidator<OrderFormDto>
{
    public OrderFormDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Müşteri seçilmelidir");

        RuleFor(x => x.AddressId)
            .Must((dto, addressId) => addressId > 0 || dto.ShippingAddress != null)
            .WithMessage("Teslimat adresi seçilmelidir veya yeni adres bilgileri girilmelidir");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0).When(x => x.CompanyId.HasValue).WithMessage("Şirket seçilmelidir");

        RuleFor(x => x.Items)
            .NotEmpty().When(x => !x.Id.HasValue).WithMessage("Yeni sipariş en az bir ürün içermelidir");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemFormDtoValidator());

        RuleFor(x => x.ShippingAddress)
            .SetValidator(new ShippingAddressDtoValidator())
            .When(x => x.ShippingAddress != null);
    }
}

public class ShippingAddressDtoValidator : AbstractValidator<ShippingAddressDto>
{
    public ShippingAddressDtoValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Sokak adresi gereklidir");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Şehir gereklidir");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("İlçe/Bölge gereklidir");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("Posta kodu gereklidir");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Ülke gereklidir");
    }
}

public class OrderItemFormDtoValidator : AbstractValidator<OrderItemFormDto>
{
    public OrderItemFormDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Ürün seçilmelidir");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(1000).WithMessage("Miktar 1000'den fazla olamaz");
    }
}
