namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Request to assign an AA service role to a group member.</summary>
public record AssignServiceRoleRequest(
    Guid UserId,
    string RoleType,
    string? CustomTitle,
    int DisplayOrder = 0
);
