using System.ComponentModel.DataAnnotations;

namespace SoberNetwork.Core.DTOs.Members;

/// <summary>
/// Password is required to confirm account deletion — prevents accidental/unauthorized removal.
/// </summary>
public record DeleteAccountRequest(
    [Required] string Password
);
