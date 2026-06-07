namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Compact upcoming-meeting summary shown on the group hub overview.</summary>
public record NextMeetingDto(
    /// <summary>Unique identifier of the meeting.</summary>
    Guid Id,
    /// <summary>Display name of the meeting.</summary>
    string Name,
    /// <summary>UTC timestamp of the next occurrence.</summary>
    DateTime NextOccurrence,
    /// <summary>Duration in minutes.</summary>
    int DurationMinutes,
    /// <summary>Meeting type label (Online / InPerson / Hybrid).</summary>
    string MeetingType
);
