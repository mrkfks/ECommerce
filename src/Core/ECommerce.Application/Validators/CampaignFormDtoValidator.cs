using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class CampaignFormDtoValidator : AbstractValidator<CampaignFormDto>
{
    public CampaignFormDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kampanya adı zorunludur")
            .MaximumLength(200).WithMessage("Kampanya adı en fazla 200 karakter olabilir");

        RuleFor(x => x.DiscountPercent)
            .InclusiveBetween(1, 100).WithMessage("İndirim oranı 1 ile 100 arasında olmalıdır");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Başlangıç tarihi zorunludur");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Bitiş tarihi zorunludur")
            .GreaterThan(x => x.StartDate).WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır");
    }
}
