namespace SoberNetwork.Domain.Enums;

/// <summary>Indicates whether a meeting is held in-person, online, or both.</summary>
public enum MeetingType
{
    /// <summary>Physical location only.</summary>
    InPerson = 0,

    /// <summary>Online only (Zoom, Google Meet, etc.).</summary>
    Online = 1,

    /// <summary>Both physical and online attendance supported.</summary>
    Hybrid = 2
}
