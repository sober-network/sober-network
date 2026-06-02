using SoberNetwork.Core.Enums;

namespace SoberNetwork.Core.Entities;

// Join table: one user can belong to many groups, with a role per group (T4 — group autonomy)
public class GroupMembership
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public Guid GroupId { get; set; }
    public GroupRole Role { get; set; } = GroupRole.Member;
    public MemberStatus Status { get; set; } = MemberStatus.PendingApproval;
    public bool IsProbationary { get; set; } = true;   // posts require moderation for new members
    public int ApprovedPostCount { get; set; } = 0;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }           // soft remove — member history preserved (T12)
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByUserId { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Group Group { get; set; } = null!;
}
