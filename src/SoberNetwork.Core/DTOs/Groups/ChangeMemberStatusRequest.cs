using System.ComponentModel.DataAnnotations;
using SoberNetwork.Core.Enums;

namespace SoberNetwork.Core.DTOs.Groups;

public record ChangeMemberStatusRequest(
    [Required] MemberStatus NewStatus
);
