using System.ComponentModel.DataAnnotations;

namespace SoberNetwork.Core.DTOs.Members;

public record UpdateProfileRequest(
    [MaxLength(100)] string? DisplayName,
    [MaxLength(50)]  string? FirstName,
    [MaxLength(100)] string? TimeZone
);
