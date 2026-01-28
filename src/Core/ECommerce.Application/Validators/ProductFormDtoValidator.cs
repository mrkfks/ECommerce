using ECommerce.Application.Constants;
using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class ProductFormDtoValidator : AbstractValidator<ProductFormDto>
{
    public ProductFormDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationMessages.ProductNameRequired)
            .MaximumLength(255).WithMessage(ValidationMessages.ProductNameMaxLength);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Ürün açıklaması zorunludur")
            .MaximumLength(1000).WithMessage(ValidationMessages.ProductDescriptionMaxLength);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage(ValidationMessages.ProductPricePositive)
            .LessThan(1000000).WithMessage("Fiyat 1.000.000'dan küçük olmalıdır");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage(ValidationMessages.ProductStockQuantityNonNegative);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage(ValidationMessages.ProductCategoryRequired);

        RuleFor(x => x.BrandId)
            .GreaterThan(0).WithMessage(ValidationMessages.ProductBrandRequired);
    }
}
