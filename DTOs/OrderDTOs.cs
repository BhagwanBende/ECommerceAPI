using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs;

public record CreateOrderDto(
    [Required(ErrorMessage = "Shipping address is required.")]
    [StringLength(200, MinimumLength = 10,
        ErrorMessage = "Address must be between 10 and 200 characters.")]
    string ShippingAddress,

    [Required(ErrorMessage = "Order must have at least one item.")]
    [MinLength(1, ErrorMessage = "Order must have at least one item.")]
    List<OrderItemDto> Items);

public record OrderItemDto(
    [Required(ErrorMessage = "Product is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid product.")]
    int ProductId,

    [Required(ErrorMessage = "Quantity is required.")]
    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100.")]
    int Quantity);

public record OrderResponseDto(
    int Id,
    int UserId,
    string Status,
    decimal TotalAmount,
    string ShippingAddress,
    DateTime OrderDate,
    List<OrderItemResponseDto> Items);

public record OrderItemResponseDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal);

public record UpdateOrderStatusDto(
    [Required(ErrorMessage = "Status is required.")]
    string Status);