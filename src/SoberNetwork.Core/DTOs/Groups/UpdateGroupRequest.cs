namespace SoberNetwork.Core.DTOs.Groups;

/// <summary>Request body for updating mutable group fields.</summary>
public record UpdateGroupRequest(
    /// <summary>Optional replacement group name. Maximum 100 characters.</summary>
    string? Name,
    /// <summary>Optional replacement description. Maximum 500 characters.</summary>
    string? Description,
    /// <summary>Optional replacement time zone label. Maximum 100 characters.</summary>
    string? TimeZone,
    /// <summary>Optional replacement public discoverability flag.</summary>
    bool? IsPublic,
    /// <summary>Optional replacement approval requirement flag.</summary>
    bool? RequiresApproval,
    /// <summary>Optional flag to require admin approval before posts appear in the news feed.</summary>
    bool? RequiresPostApproval = null,
    /// <summary>Optional district name (e.g. "District 5"). Maximum 100 characters.</summary>
    string? DistrictName = null,
    /// <summary>Optional district website URL.</summary>
    string? DistrictWebsiteUrl = null,
    /// <summary>Optional area name (e.g. "Area 11"). Maximum 100 characters.</summary>
    string? AreaName = null,
    /// <summary>Optional area website URL.</summary>
    string? AreaWebsiteUrl = null,
    /// <summary>Optional U.S. state or region abbreviation (e.g. "NY").</summary>
    string? State = null,
    /// <summary>Optional latitude for district map center. Used as fallback for online meeting search.</summary>
    double? DistrictLatitude = null,
    /// <summary>Optional longitude for district map center. Used as fallback for online meeting search.</summary>
    double? DistrictLongitude = null
);
