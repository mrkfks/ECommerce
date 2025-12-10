using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class OrderCreateDtoValidator : AbstractValidator<OrderCreateDto>
{
    public OrderCreateDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Müşteri seçilmelidir");

        RuleFor(x => x.AddressId)
            .GreaterThan(0).WithMessage("Teslimat adresi seçilmelidir");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0).WithMessage("Şirket seçilmelidir");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Sipariş en az bir ürün içermelidir");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemCreateDtoValidator());
    }
}

public class OrderItemCreateDtoValidator : AbstractValidator<OrderItemCreateDto>
{
    public OrderItemCreateDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Ürün seçilmelidir");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(1000).WithMessage("Miktar 1000'den fazla olamaz");
    }
}
