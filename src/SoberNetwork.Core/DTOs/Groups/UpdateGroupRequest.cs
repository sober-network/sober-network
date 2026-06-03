namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Request body for updating mutable group fields.</summary>
public record UpdateGroupRequest(
    /// <summary>Optional replacement group name. Maximum 100 characters.</summary>
    string? Name,
    /// <summary>Optional replacement description. Maximum 500 characters.</summary>
    string? Description,
    /// <summary>Optional replacement time zone label. Maximum 100 characters.</summary>
    string? TimeZone,
    /// <summary>Optional replacement public discoverability flag.</summary>
    bool? IsPublic,
    /// <summary>Optional replacement approval requirement flag.</summary>
    bool? RequiresApproval
);
