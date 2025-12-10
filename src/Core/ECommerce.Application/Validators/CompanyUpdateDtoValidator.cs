using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class CompanyUpdateDtoValidator : AbstractValidator<CompanyUpdateDto>
{
    public CompanyUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Şirket adı zorunludur")
            .MaximumLength(200).WithMessage("Şirket adı en fazla 200 karakter olabilir");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email zorunludur")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz")
            .MaximumLength(200).WithMessage("Email en fazla 200 karakter olabilir");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarası zorunludur")
            .Matches(@"^(\+90|0)?[0-9]{10}$").WithMessage("Geçerli bir telefon numarası giriniz");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Adres zorunludur")
            .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir");

        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("Aktiflik durumu belirtilmelidir");
    }
}
