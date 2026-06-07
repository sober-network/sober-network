using SoberNetwork.Domain.Enums;

namespace SoberNetwork.Domain.Entities;

/// <summary>
/// An AA service role held by a member within a group.
/// A member can hold multiple roles; roles are elected by group conscience (T2).
/// </summary>
public class GroupServiceRole
{
    /// <summary>Unique identifier for the service role assignment.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>Owning group identifier.</summary>
    public Guid GroupId { get; set; }
    /// <summary>Assigned member identifier.</summary>
    public Guid UserId { get; set; }
    /// <summary>Standardized role type.</summary>
    public GroupServiceRoleType RoleType { get; set; }
    /// <summary>Used when RoleType is Other.</summary>
    public string? CustomTitle { get; set; }
    /// <summary>Controls display ordering in the UI.</summary>
    public int DisplayOrder { get; set; } = 0;
    /// <summary>UTC timestamp when the assignment was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>UTC timestamp when the assignment was last updated.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Soft-delete marker for removed assignments.</summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>Assigned member navigation.</summary>
    public ApplicationUser User { get; set; } = null!;
    /// <summary>Owning group navigation.</summary>
    public Group Group { get; set; } = null!;
}
