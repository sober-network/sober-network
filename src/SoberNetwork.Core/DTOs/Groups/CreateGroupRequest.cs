using System.ComponentModel.DataAnnotations;

namespace SoberNetwork.Core.DTOs.Groups;

public record CreateGroupRequest(
    [Required, MaxLength(100)] string Name,

    /// <summary>URL-safe identifier, e.g. "earlybird". Becomes the subdomain slug.</summary>
    [Required, MaxLength(50), RegularExpression("^[a-z0-9-]+$",
        ErrorMessage = "Slug may only contain lowercase letters, numbers, and hyphens.")]
    string Slug,

    [MaxLength(500)] string? Description,
    [MaxLength(1000)] string? MeetingSchedule,
    [MaxLength(500)] string? ZoomLink,
    [MaxLength(100)] string? TimeZone
);
