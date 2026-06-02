using System.ComponentModel.DataAnnotations;

namespace SoberNetwork.Core.DTOs.Auth;

public record RefreshTokenRequest(
    [Required] string RefreshToken
);
