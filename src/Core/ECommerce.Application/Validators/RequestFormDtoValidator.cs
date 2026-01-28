using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class RequestFormDtoValidator : AbstractValidator<RequestFormDto>
{
    public RequestFormDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Talep başlığı zorunludur")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Talep açıklaması zorunludur")
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0).When(x => x.CompanyId.HasValue).WithMessage("Şirket seçilmelidir");
    }
}
