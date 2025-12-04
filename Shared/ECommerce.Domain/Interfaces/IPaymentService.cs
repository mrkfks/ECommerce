namespace ECommerce.Domain.Interfaces
{
    public interface IPaymentService
    {
        bool ValidatePayment(string cardNumber, string expiry, string cvv);
    }
}