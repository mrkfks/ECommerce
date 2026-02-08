using ECommerce.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs;

/// <summary>
/// İade talebi bilgisi DTO
/// </summary>
public record ReturnRequestDto
{
    public int Id { get; init; }
    public int OrderId { get; init; }
    public int OrderItemId { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public int CompanyId { get; init; }
    public int Quantity { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Comments { get; init; }
    public ReturnRequestStatus Status { get; init; }
    public string StatusText { get; init; } = string.Empty;
    public string? AdminResponse { get; init; }
    public DateTime RequestDate { get; init; }
    public DateTime? ResolutionDate { get; init; }
}

/// <summary>
/// İade talebi oluşturma DTO
/// </summary>
public record CreateReturnRequestDto
{
    [Required(ErrorMessage = "OrderId gerekli")]
    public int OrderId { get; init; }

    [Required(ErrorMessage = "OrderItemId gerekli")]
    public int OrderItemId { get; init; }

    [Required(ErrorMessage = "ProductId gerekli")]
    public int ProductId { get; init; }

    [Required(ErrorMessage = "Quantity gerekli")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity en az 1 olmalı")]
    public int Quantity { get; init; }

    [Required(ErrorMessage = "Reason gerekli")]
    [StringLength(500, MinimumLength = 3, ErrorMessage = "Reason 3 ile 500 karakter arasında olmalı")]
    public string Reason { get; init; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Comments en fazla 1000 karakter olmalı")]
    public string? Comments { get; init; }
}

/// <summary>
/// İade talebi güncelleme (admin) DTO
/// </summary>
public record UpdateReturnRequestDto
{
    public ReturnRequestStatus Status { get; init; }
    public string? AdminResponse { get; init; }
}
