namespace SoberNetwork.Domain.Entities;

public class Group
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;   // e.g. "earlybird" → earlybird.sobernetwork.group
    public string? Description { get; set; }
    public string? MeetingSchedule { get; set; }
    public string? ZoomLink { get; set; }
    public string? TimeZone { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }           // soft delete — groups are never permanently removed (T4)
    public bool IsPublic { get; set; } = true;
    public bool RequiresApproval { get; set; } = true;
    public int? MeetingDay { get; set; }           // DayOfWeek 0=Sun..6=Sat, null=no fixed day
    public string? MeetingTime { get; set; }        // "HH:mm" 24-hr
    public int DurationMinutes { get; set; } = 60;
    public bool IsOpen { get; set; } = true;        // AA open (anyone) vs closed (AA members only)
    public string? Language { get; set; }           // null means English
    public string? MeetingFormats { get; set; }     // comma-separated: "Discussion","Speaker","StepStudy","BigBook","Beginners"
    public string? ZoomMeetingId { get; set; }
    public string? ZoomPasscode { get; set; }

    public ICollection<GroupMembership> Memberships { get; set; } = [];
}
