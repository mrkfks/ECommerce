using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class BannerFormDtoValidator : AbstractValidator<BannerFormDto>
{
    public BannerFormDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Banner başlığı zorunludur")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("Banner görseli zorunludur");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Sıralama 0 veya daha büyük olmalıdır");
    }
}
