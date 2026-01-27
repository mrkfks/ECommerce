namespace ECommerce.Application.DTOs.Dashboard;

public record DailySalesDto(
    DateTime Date,
    decimal TotalAmount
);
