using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class CustomerFormDtoValidator : AbstractValidator<CustomerFormDto>
{
    public CustomerFormDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Müşteri adı zorunludur")
            .MaximumLength(100).WithMessage("Müşteri adı en fazla 100 karakter olabilir");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Müşteri soyadı zorunludur")
            .MaximumLength(100).WithMessage("Müşteri soyadı en fazla 100 karakter olabilir");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta zorunludur")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz")
            .MaximumLength(150).WithMessage("E-posta en fazla 150 karakter olabilir");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarası zorunludur")
            .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir");
    }
}
