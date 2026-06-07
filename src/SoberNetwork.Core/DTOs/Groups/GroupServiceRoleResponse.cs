namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>An AA service role held by a member, returned to authenticated group members.</summary>
public record GroupServiceRoleResponse(
    Guid Id,
    Guid UserId,
    string DisplayName,
    /// <summary>Email if the member has opted in to email sharing for this group.</summary>
    string? Email,
    /// <summary>Phone if the member has opted in to phone sharing for this group.</summary>
    string? PhoneNumber,
    string RoleType,
    string? CustomTitle,
    int DisplayOrder
);
