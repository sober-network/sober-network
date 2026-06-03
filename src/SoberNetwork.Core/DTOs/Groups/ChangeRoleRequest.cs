using SoberNetwork.Domain.Enums;



namespace SoberNetwork.Core.DTOs.Groups;



/// <summary>Request body for changing a group member's role.</summary>

public record ChangeRoleRequest(

    /// <summary>New group role to assign to the member.</summary>

    GroupRole NewRole

);

