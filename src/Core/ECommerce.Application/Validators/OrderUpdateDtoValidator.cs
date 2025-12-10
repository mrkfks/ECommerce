using ECommerce.Application.DTOs;
using ECommerce.Domain.Enums;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class OrderUpdateDtoValidator : AbstractValidator<OrderUpdateDto>
{
    public OrderUpdateDtoValidator()
    {
        RuleFor(x => x.OrderStatus)
            .IsInEnum().WithMessage("Geçersiz sipariş durumu");

        RuleFor(x => x.AddressId)
            .GreaterThan(0).When(x => x.AddressId.HasValue)
            .WithMessage("Geçersiz adres ID");
    }
}
