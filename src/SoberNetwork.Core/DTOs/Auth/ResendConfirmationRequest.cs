using System.ComponentModel.DataAnnotations;

namespace SoberNetwork.Core.DTOs.Auth;

public record ResendConfirmationRequest(
    [Required, EmailAddress] string Email
);
