using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class BrandFormDtoValidator : AbstractValidator<BrandFormDto>
{
    public BrandFormDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Marka adı zorunludur")
            .MaximumLength(100).WithMessage("Marka adı en fazla 100 karakter olabilir");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir");
    }
}
