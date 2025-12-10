using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class ProductUpdateDtoValidator : AbstractValidator<ProductUpdateDto>
{
    public ProductUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ürün adı zorunludur")
            .MaximumLength(200).WithMessage("Ürün adı en fazla 200 karakter olabilir");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Ürün açıklaması zorunludur")
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır")
            .LessThan(1000000).WithMessage("Fiyat 1.000.000'dan küçük olmalıdır");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stok miktarı negatif olamaz");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Kategori seçilmelidir");

        RuleFor(x => x.BrandId)
            .GreaterThan(0).WithMessage("Marka seçilmelidir");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Resim URL'si en fazla 500 karakter olabilir");

        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("Aktiflik durumu belirtilmelidir");
    }
}
