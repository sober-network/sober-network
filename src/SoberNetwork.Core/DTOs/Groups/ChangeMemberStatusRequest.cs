using SoberNetwork.Domain.Enums;



namespace SoberNetwork.Core.DTOs.Groups;



/// <summary>Request body for changing a group member's status.</summary>

public record ChangeMemberStatusRequest(

    /// <summary>New membership status to assign to the member.</summary>

    MemberStatus NewStatus

);


