using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Interfaces
{
    public interface IOrderDomainService
    {
        bool CanTransition(OrderStatus current, OrderStatus next);
        decimal CalculateTotal(Order order);
    }
}