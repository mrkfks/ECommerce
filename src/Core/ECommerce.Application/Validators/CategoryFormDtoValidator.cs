using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class CategoryFormDtoValidator : AbstractValidator<CategoryFormDto>
{
    public CategoryFormDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kategori adı zorunludur")
            .MaximumLength(100).WithMessage("Kategori adı en fazla 100 karakter olabilir");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir");
    }
}
