using ECommerce.Application.DTOs;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class UserFormDtoValidator : AbstractValidator<UserFormDto>
{
    public UserFormDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Kullanıcı adı zorunludur")
            .MinimumLength(3).WithMessage("Kullanıcı adı en az 3 karakter olmalıdır")
            .MaximumLength(50).WithMessage("Kullanıcı adı en fazla 50 karakter olabilir")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Kullanıcı adı sadece harf, rakam ve alt çizgi içerebilir");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email zorunludur")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz")
            .MaximumLength(200).WithMessage("Email en fazla 200 karakter olabilir");

        // Şifre sadece yeni kullanıcı (Id null) oluştururken veya şifre alanı doluysa zorunlu olsun
        RuleFor(x => x.Password)
            .NotEmpty().When(x => !x.Id.HasValue).WithMessage("Yeni kullanıcı için şifre zorunludur")
            .MinimumLength(6).When(x => !string.IsNullOrEmpty(x.Password)).WithMessage("Şifre en az 6 karakter olmalıdır")
            .Matches(@"[A-Z]").When(x => !string.IsNullOrEmpty(x.Password)).WithMessage("Şifre en az bir büyük harf içermelidir")
            .Matches(@"[a-z]").When(x => !string.IsNullOrEmpty(x.Password)).WithMessage("Şifre en az bir küçük harf içermelidir")
            .Matches(@"[0-9]").When(x => !string.IsNullOrEmpty(x.Password)).WithMessage("Şifre en az bir rakam içermelidir");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad zorunludur")
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad zorunludur")
            .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir");

        RuleFor(x => x.CompanyId)
            .GreaterThan(0).WithMessage("Şirket seçilmelidir");
    }
}
