using System.ComponentModel.DataAnnotations;

namespace SoberNetwork.Core.DTOs.Auth;

public record ForgotPasswordRequest(
    [Required, EmailAddress] string Email
);
