using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs;

public record RegisterDto(
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(50, MinimumLength = 2,
        ErrorMessage = "Name must be between 2 and 50 characters.")]
    string Name,

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    string Email,

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Password must be at least 8 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).+$",
        ErrorMessage = "Password must have uppercase, lowercase, number and special character.")]
    string Password);

public record LoginDto(
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    string Email,

    [Required(ErrorMessage = "Password is required.")]
    string Password);

public record AuthResponseDto(
    string Token,
    string Name,
    string Email,
    string Role);