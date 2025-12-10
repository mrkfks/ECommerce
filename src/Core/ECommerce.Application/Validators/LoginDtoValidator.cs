using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty().WithMessage("Kullanıcı adı veya email zorunludur");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur");
    }
}
