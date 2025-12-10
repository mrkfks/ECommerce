using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class ReviewUpdateDtoValidator : AbstractValidator<ReviewUpdateDto>
{
    public ReviewUpdateDtoValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Değerlendirme 1 ile 5 arasında olmalıdır");

        RuleFor(x => x.Comment)
            .MaximumLength(1000).WithMessage("Yorum en fazla 1000 karakter olabilir");
    }
}
