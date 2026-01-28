using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class ReviewFormDtoValidator : AbstractValidator<ReviewFormDto>
{
    public ReviewFormDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Ürün seçilmelidir");

        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("Müşteri seçilmelidir");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0).WithMessage("Şirket seçilmelidir");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Değerlendirme 1 ile 5 arasında olmalıdır");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Yorum en fazla 1000 karakter olabilir");
    }
}
