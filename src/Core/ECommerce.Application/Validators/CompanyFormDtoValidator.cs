using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class CompanyFormDtoValidator : AbstractValidator<CompanyFormDto>
{
    public CompanyFormDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Şirket adı zorunludur")
            .MaximumLength(200).WithMessage("Şirket adı en fazla 200 karakter olabilir");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta zorunludur")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz")
            .MaximumLength(150).WithMessage("E-posta en fazla 150 karakter olabilir");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarası zorunludur")
            .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Adres zorunludur")
            .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir");

        RuleFor(x => x.TaxNumber)
            .NotEmpty().WithMessage("Vergi numarası zorunludur")
            .Length(10).WithMessage("Vergi numarası 10 haneli olmalıdır")
            .Matches(@"^[0-9]+$").WithMessage("Vergi numarası sadece rakamlardan oluşmalıdır");
    }
}
