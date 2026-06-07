using SoberNetwork.Core.DTOs.Members;

namespace SoberNetwork.Core.Interfaces;

/// <summary>Provides member profile, credential, and group-scoped member workflows.</summary>
public interface IMemberService
{
    /// <summary>Returns the authenticated member's full profile, or null if the account is unavailable.</summary>
    Task<MemberProfileResponse?> GetMyProfileAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Updates editable profile fields for the authenticated member.</summary>
    Task<(MemberProfileResponse? Profile, string? Error)> UpdateProfileAsync(
        Guid userId, UpdateProfileRequest request, CancellationToken ct = default);

    /// <summary>Soft-deletes the account and cascades the change to memberships and refresh tokens.</summary>
    Task<(bool Success, string? Error)> DeleteAccountAsync(Guid userId, string password, CancellationToken ct = default);

    /// <summary>Changes the authenticated member's password.</summary>
    Task<(bool Success, string? Error)> ChangePasswordAsync(
        Guid userId, ChangePasswordRequest request, CancellationToken ct = default);

    /// <summary>Starts an email change flow for the authenticated member.</summary>
    Task<(bool Success, string? Error)> ChangeEmailAsync(
        Guid userId, ChangeEmailRequest request, CancellationToken ct = default);

    /// <summary>Sets or updates the authenticated member's sobriety date.</summary>
    Task<(bool Success, string? Error)> SetSobrietyDateAsync(Guid userId, DateOnly date, CancellationToken ct = default);

    /// <summary>Removes the authenticated member's sobriety date and related visibility flags.</summary>
    Task<bool> RemoveSobrietyDateAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Updates sobriety visibility for the authenticated member. Controls date and days-sober count as a unit.</summary>
    Task<bool> UpdateSobrietyVisibilityAsync(Guid userId, bool isPublic, CancellationToken ct = default);

    /// <summary>Returns the authenticated member's mailing address, or null if none is set.</summary>
    Task<MailingAddressResponse?> GetMailingAddressAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Updates the authenticated member's mailing address and caches geocoded coordinates.</summary>
    Task<(bool Success, string? Error)> UpdateMailingAddressAsync(Guid userId, UpdateMailingAddressRequest request, CancellationToken ct = default);

    /// <summary>Sets or replaces the authenticated member's phone number.</summary>
    Task<(bool Success, string? Error)> SetPhoneAsync(Guid userId, string phoneNumber, CancellationToken ct = default);

    /// <summary>Removes the authenticated member's phone number.</summary>
    Task<bool> RemovePhoneAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Updates whether the authenticated member shares their phone number within a specific group.</summary>
    Task<(bool Success, string? Error)> SetGroupPhoneVisibilityAsync(
        Guid userId, string groupSlug, bool isShared, CancellationToken ct = default);

    /// <summary>Updates whether the authenticated member shares their email address within a specific group.</summary>
    Task<(bool Success, string? Error)> SetGroupEmailVisibilityAsync(
        Guid userId, string groupSlug, bool isShared, CancellationToken ct = default);
 
    /// <summary>Returns the group phone list for members who opted in to sharing.</summary>
    Task<(IReadOnlyList<PhoneListEntryResponse>? List, string? Error)> GetGroupPhoneListAsync(
        Guid requestingUserId, string groupSlug, CancellationToken ct = default);

    /// <summary>Returns active group admins with contact details the group is allowed to see.</summary>
    Task<(IReadOnlyList<GroupAdminContactResponse>? List, string? Error)> GetGroupAdminContactsAsync(
        Guid requestingUserId, string groupSlug, CancellationToken ct = default);

    /// <summary>Returns a group-scoped profile view for another member.</summary>
    Task<(MemberDetailResponse? Member, string? Error)> GetMemberInGroupContextAsync(
        Guid requestingUserId, string groupSlug, Guid targetUserId, CancellationToken ct = default);

    /// <summary>Returns all members on the platform for SuperAdmin use.</summary>
    Task<IReadOnlyList<AdminMemberResponse>> GetAllMembersAsync(CancellationToken ct = default);

    /// <summary>Returns the full admin view of a specific user.</summary>
    Task<AdminMemberResponse?> GetUserByIdAsync(Guid targetUserId, CancellationToken ct = default);

    /// <summary>Force-deactivates a user account as a SuperAdmin operation.</summary>
    Task<(bool Success, string? Error)> DeactivateUserAsync(Guid adminUserId, Guid targetUserId, CancellationToken ct = default);
}
