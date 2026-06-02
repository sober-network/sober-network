using System.ComponentModel.DataAnnotations;

namespace SoberNetwork.Core.DTOs.Auth;

public record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(10), MaxLength(256)] string Password,
    [Required, MaxLength(100)] string DisplayName,
    [MaxLength(100)] string? FirstName
);
