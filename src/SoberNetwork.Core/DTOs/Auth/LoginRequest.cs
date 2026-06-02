using System.ComponentModel.DataAnnotations;

namespace SoberNetwork.Core.DTOs.Auth;

public record LoginRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(256)] string Password
);
