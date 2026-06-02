using System.ComponentModel.DataAnnotations;

namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>
/// Only updateable fields — Name and Slug are identity fields and cannot be changed after creation.
/// </summary>
public record UpdateGroupRequest(
    [MaxLength(500)] string? Description,
    [MaxLength(1000)] string? MeetingSchedule,
    [MaxLength(500)] string? ZoomLink,
    [MaxLength(100)] string? TimeZone
);
