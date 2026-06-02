using System.ComponentModel.DataAnnotations;

namespace SoberNetwork.Core.DTOs.Auth;

public record ResetPasswordRequest(
    [Required] string UserId,
    [Required] string Token,
    [Required, MinLength(10)] string NewPassword
);
