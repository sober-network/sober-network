using System.ComponentModel.DataAnnotations;

namespace SoberNetwork.Core.DTOs.Members;

public record ChangeEmailRequest(
    [Required] string CurrentPassword,
    [Required, EmailAddress, MaxLength(256)] string NewEmail
);
