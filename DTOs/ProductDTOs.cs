using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs;

public record CreateProductDto(
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Name must be between 2 and 100 characters.")]
    string Name,

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(500, MinimumLength = 10,
        ErrorMessage = "Description must be between 10 and 500 characters.")]
    string Description,

    [Required(ErrorMessage = "Price is required.")]
    [Range(1, 10000000,
        ErrorMessage = "Price must be between 1 and 10,000,000.")]
    decimal Price,

    [Required(ErrorMessage = "Stock is required.")]
    [Range(0, 100000,
        ErrorMessage = "Stock must be between 0 and 100,000.")]
    int Stock,

    [Required(ErrorMessage = "Category is required.")]
    [StringLength(50, MinimumLength = 2,
        ErrorMessage = "Category must be between 2 and 50 characters.")]
    string Category);

public record UpdateProductDto(
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Name must be between 2 and 100 characters.")]
    string? Name,

    [StringLength(500, MinimumLength = 10,
        ErrorMessage = "Description must be between 10 and 500 characters.")]
    string? Description,

    [Range(1, 10000000,
        ErrorMessage = "Price must be between 1 and 10,000,000.")]
    decimal? Price,

    [Range(0, 100000,
        ErrorMessage = "Stock must be between 0 and 100,000.")]
    int? Stock,

    [StringLength(50, MinimumLength = 2,
        ErrorMessage = "Category must be between 2 and 50 characters.")]
    string? Category,

    bool? IsActive);

public record ProductResponseDto(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string Category,
    bool IsActive,
    DateTime CreatedAt);