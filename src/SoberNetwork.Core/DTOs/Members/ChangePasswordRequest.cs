using System.ComponentModel.DataAnnotations;

namespace SoberNetwork.Core.DTOs.Members;

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, MinLength(10)] string NewPassword,
    [Required] string ConfirmNewPassword
);
