using SoberNetwork.Core.DTOs.Members;

namespace SoberNetwork.Core.Interfaces;

public interface IMemberService
{
    // ── Own profile ────────────────────────────────────────────────────────────

    Task<MemberProfileResponse?> GetMyProfileAsync(string userId);

    Task<(MemberProfileResponse? Profile, string? Error)> UpdateProfileAsync(
        string userId, UpdateProfileRequest request);

    /// <summary>
    /// Soft-deletes the account. Requires current password for confirmation.
    /// Cascades: revokes all refresh tokens, soft-deletes all group memberships.
    /// </summary>
    Task<(bool Success, string? Error)> DeleteAccountAsync(string userId, string password);

    // ── Credentials ────────────────────────────────────────────────────────────

    Task<(bool Success, string? Error)> ChangePasswordAsync(
        string userId, ChangePasswordRequest request);

    /// <summary>
    /// Changes email. Requires current password. New email must be unique.
    /// Triggers re-confirmation — user is effectively logged out until confirmed.
    /// </summary>
    Task<(bool Success, string? Error)> ChangeEmailAsync(
        string userId, ChangeEmailRequest request);

    // ── Sobriety date ──────────────────────────────────────────────────────────

    Task<(bool Success, string? Error)> SetSobrietyDateAsync(string userId, DateOnly date);

    Task<bool> RemoveSobrietyDateAsync(string userId);

    /// <summary>
    /// Updates both visibility toggles independently (T3 — granular opt-in).
    /// </summary>
    Task<bool> UpdateSobrietyVisibilityAsync(string userId, bool isDatePublic, bool isDaysPublic);

    // ── Phone ──────────────────────────────────────────────────────────────────

    Task<(bool Success, string? Error)> SetPhoneAsync(string userId, string phoneNumber);

    Task<bool> RemovePhoneAsync(string userId);

    /// <summary>
    /// Toggles per-group phone sharing. Caller must be an active member of the group (T4, T12).
    /// </summary>
    Task<(bool Success, string? Error)> SetGroupPhoneVisibilityAsync(
        string userId, string groupSlug, bool isShared);

    // ── Group-scoped member views ──────────────────────────────────────────────

    /// <summary>
    /// Returns the phone list for a group — only members with IsPhoneShared=true for this group.
    /// Caller must be an active member (T12 — auth-only).
    /// </summary>
    Task<(IReadOnlyList<PhoneListEntryResponse>? List, string? Error)> GetGroupPhoneListAsync(
        string requestingUserId, string groupSlug);

    /// <summary>
    /// Returns the group-scoped profile of another member.
    /// Respects IsSobrietyDatePublic, IsDaysSoberPublic, and IsPhoneShared for this group (T3, T12).
    /// Caller must be an active member of the same group (T4).
    /// </summary>
    Task<(MemberDetailResponse? Member, string? Error)> GetMemberInGroupContextAsync(
        string requestingUserId, string groupSlug, string targetUserId);

    // ── SuperAdmin ─────────────────────────────────────────────────────────────

    /// <summary>Returns all users on the platform. SuperAdmin only.</summary>
    Task<IReadOnlyList<AdminMemberResponse>> GetAllMembersAsync();

    /// <summary>Returns the full admin view of any user. SuperAdmin only.</summary>
    Task<AdminMemberResponse?> GetUserByIdAsync(string targetUserId);

    /// <summary>
    /// Force-deactivates a user account. SuperAdmin only.
    /// Same cascade as self-deletion: revoke tokens, remove memberships.
    /// </summary>
    Task<(bool Success, string? Error)> DeactivateUserAsync(string adminUserId, string targetUserId);
}
