namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Request body for creating a new group.</summary>
public record CreateGroupRequest(
    /// <summary>Group name shown throughout the application. Required, maximum 100 characters.</summary>
    string Name,
    /// <summary>Permanent URL slug. Required, lowercase letters, numbers, and hyphens only, maximum 50 characters.</summary>
    string Slug,
    /// <summary>Optional group description. Maximum 500 characters.</summary>
    string? Description = null,
    /// <summary>Optional time zone label. Maximum 100 characters.</summary>
    string? TimeZone = null,
    /// <summary>Whether the group is listed in public discovery views.</summary>
    bool IsPublic = true,
    /// <summary>Whether join requests require approval from a group admin.</summary>
    bool RequiresApproval = true
);
