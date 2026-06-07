using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoberNetwork.Core.DTOs.Members;
using SoberNetwork.Domain.Entities;
using SoberNetwork.Domain.Enums;
using SoberNetwork.Core.Interfaces;
using SoberNetwork.Infrastructure.Data;

namespace SoberNetwork.Infrastructure.Services;

public class MemberService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IAuditService audit,
    IEmailService email,
    IHttpClientFactory httpClientFactory) : IMemberService
{
    // ── Helpers ────────────────────────────────────────────────────────────────

    private static SobrietyResponse? BuildSobrietyResponse(ApplicationUser user, bool isSelf)
    {
        if (user.SobrietyDate == null) return null;

        var daysSober = (DateTime.UtcNow.Date - user.SobrietyDate.Value.ToDateTime(TimeOnly.MinValue)).Days;

        return new SobrietyResponse(
            SobrietyDate: isSelf || user.IsSobrietyDatePublic ? user.SobrietyDate : null,
            DaysSober:    isSelf || user.IsSobrietyDatePublic ? daysSober          : null,
            IsPublic:     user.IsSobrietyDatePublic
        );
    }

    private static MemberProfileResponse ToProfileResponse(ApplicationUser u) => new(
        u.Id,
        u.DisplayName,
        u.FirstName,
        u.Email!,
        u.PhoneNumber,
        u.TimeZone,
        BuildSobrietyResponse(u, isSelf: true),
        u.IsSuperAdmin,
        u.CreatedAt,
        u.LastLoginAt
    );

    // ── Own profile ────────────────────────────────────────────────────────────

    public async Task<MemberProfileResponse?> GetMyProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
        return user == null || user.DeletedAt != null ? null : ToProfileResponse(user);
    }

    public async Task<(MemberProfileResponse? Profile, string? Error)> UpdateProfileAsync(
        Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.DeletedAt != null) return (null, "User not found.");

        if (request.DisplayName != null) user.DisplayName = request.DisplayName;
        if (request.FirstName   != null) user.FirstName   = request.FirstName;
        if (request.TimeZone    != null) user.TimeZone     = request.TimeZone;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded) return (null, string.Join("; ", result.Errors.Select(e => e.Description)));

        await audit.LogAsync(SecurityEventType.ProfileUpdated, userId);
        return (ToProfileResponse(user), null);
    }

    public async Task<(bool Success, string? Error)> DeleteAccountAsync(Guid userId, string password, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.DeletedAt != null) return (false, "User not found.");

        if (!await userManager.CheckPasswordAsync(user, password))
            return (false, "Incorrect password.");

        var now = DateTime.UtcNow;
        user.DeletedAt = now;
        user.UpdatedAt = now;

        // Cascade: soft-delete all group memberships
        var memberships = await db.GroupMemberships
            .Where(m => m.UserId == userId && m.DeletedAt == null)
            .ToListAsync(ct);
        foreach (var m in memberships) { m.DeletedAt = now; m.UpdatedAt = now; }

        // Cascade: revoke all refresh tokens
        var tokens = await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var t in tokens) t.RevokedAt = now;

        await userManager.UpdateAsync(user);
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(SecurityEventType.AccountDeactivated, userId, "Self-deleted account");
        return (true, null);
    }

    // ── Credentials ────────────────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(
        Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
            return (false, "New passwords do not match.");

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.DeletedAt != null) return (false, "User not found.");

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

        await audit.LogAsync(SecurityEventType.PasswordChanged, userId);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangeEmailAsync(
        Guid userId, ChangeEmailRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.DeletedAt != null) return (false, "User not found.");

        if (!await userManager.CheckPasswordAsync(user, request.CurrentPassword))
            return (false, "Incorrect password.");

        if (await userManager.FindByEmailAsync(request.NewEmail) != null)
            return (false, "That email address is already in use.");

        // Generate token and send confirmation to new address
        var token = await userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);
        // Store the pending email in a way the confirm endpoint can use it.
        // We encode both in the token — the confirmation endpoint calls ChangeEmailAsync(token, newEmail).
        // For now, send the token directly; frontend appends it to the confirmation URL.
        var confirmLink = $"confirm-email-change?userId={Uri.EscapeDataString(userId.ToString())}&token={Uri.EscapeDataString(token)}&newEmail={Uri.EscapeDataString(request.NewEmail)}";

        try
        {
            await email.SendEmailConfirmationAsync(request.NewEmail, user.DisplayName, confirmLink);
        }
        catch
        {
            return (false, "Failed to send confirmation email. Please try again.");
        }

        await audit.LogAsync(SecurityEventType.EmailChangeRequested, userId,
            $"Change requested to new address (not logged — PII)");
        return (true, null);
    }

    // ── Sobriety date ──────────────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> SetSobrietyDateAsync(Guid userId, DateOnly date, CancellationToken ct = default)
    {
        if (date > DateOnly.FromDateTime(DateTime.UtcNow))
            return (false, "Sobriety date cannot be in the future.");

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.DeletedAt != null) return (false, "User not found.");

        user.SobrietyDate = date;
        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        await audit.LogAsync(SecurityEventType.SobrietyDateSet, userId);
        return (true, null);
    }

    public async Task<bool> RemoveSobrietyDateAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.DeletedAt != null) return false;

        user.SobrietyDate = null;
        user.IsSobrietyDatePublic = false;
        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        await audit.LogAsync(SecurityEventType.SobrietyDateRemoved, userId);
        return true;
    }

    public async Task<bool> UpdateSobrietyVisibilityAsync(Guid userId, bool isPublic, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.DeletedAt != null) return false;

        user.IsSobrietyDatePublic = isPublic;
        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        await audit.LogAsync(SecurityEventType.SobrietyVisibilityChanged, userId,
            $"isPublic={isPublic}");
        return true;
    }

    // ── Phone ──────────────────────────────────────────────────────────────────

    public async Task<(bool Success, string? Error)> SetPhoneAsync(Guid userId, string phoneNumber, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.DeletedAt != null) return (false, "User not found.");

        var result = await userManager.SetPhoneNumberAsync(user, phoneNumber);
        if (!result.Succeeded)
            return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        await audit.LogAsync(SecurityEventType.PhoneSet, userId);
        return (true, null);
    }

    public async Task<bool> RemovePhoneAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.DeletedAt != null) return false;

        await userManager.SetPhoneNumberAsync(user, null);
        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        await audit.LogAsync(SecurityEventType.PhoneRemoved, userId);
        return true;
    }

    public async Task<(bool Success, string? Error)> SetGroupPhoneVisibilityAsync(
        Guid userId, string groupSlug, bool isShared, CancellationToken ct = default)
    {
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == groupSlug && g.DeletedAt == null, ct);
        if (group == null) return (false, "Group not found.");

        var membership = await db.GroupMemberships.FirstOrDefaultAsync(m =>
            m.GroupId == group.Id &&
            m.UserId == userId &&
            m.Status == MemberStatus.Active &&
            m.DeletedAt == null, ct);
        if (membership == null) return (false, "You are not an active member of this group.");

        // Cannot share phone if none is set
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (isShared && string.IsNullOrEmpty(user?.PhoneNumber))
            return (false, "Add a phone number before sharing it with a group.");

        membership.IsPhoneShared = isShared;
        membership.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(SecurityEventType.PhoneVisibilityChanged, userId,
            $"group={groupSlug} isShared={isShared}");
        return (true, null);
    }

    // ── Group-scoped member views ──────────────────────────────────────────────

    public async Task<(IReadOnlyList<PhoneListEntryResponse>? List, string? Error)> GetGroupPhoneListAsync(
        Guid requestingUserId, string groupSlug, CancellationToken ct = default)
    {
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == groupSlug && g.DeletedAt == null, ct);
        if (group == null) return (null, "Group not found.");

        // Caller must be an active member (T12 — auth-only)
        var callerMembership = await db.GroupMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m =>
            m.GroupId == group.Id &&
            m.UserId == requestingUserId &&
            m.Status == MemberStatus.Active &&
            m.DeletedAt == null, ct);
        if (callerMembership == null) return (null, "You are not a member of this group.");

        var list = await db.GroupMemberships
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m =>
                m.GroupId == group.Id &&
                m.Status == MemberStatus.Active &&
                m.IsPhoneShared &&
                m.DeletedAt == null &&
                m.User != null &&
                m.User.PhoneNumber != null)
            .OrderBy(m => m.User!.DisplayName)
            .Select(m => new PhoneListEntryResponse(m.UserId, m.User!.DisplayName, m.User.PhoneNumber!))
            .ToListAsync(ct);

        return (list, null);
    }

    public async Task<(MemberDetailResponse? Member, string? Error)> GetMemberInGroupContextAsync(
        Guid requestingUserId, string groupSlug, Guid targetUserId, CancellationToken ct = default)
    {
        var group = await db.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == groupSlug && g.DeletedAt == null, ct);
        if (group == null) return (null, "Group not found.");

        // Caller must be an active member of this group (T4)
        var callerMembership = await db.GroupMemberships
            .AsNoTracking()
            .FirstOrDefaultAsync(m =>
            m.GroupId == group.Id &&
            m.UserId == requestingUserId &&
            m.Status == MemberStatus.Active &&
            m.DeletedAt == null, ct);
        if (callerMembership == null) return (null, "You are not a member of this group.");

        var targetMembership = await db.GroupMemberships
            .AsNoTracking()
            .Include(m => m.User)
            .FirstOrDefaultAsync(m =>
            m.GroupId == group.Id &&
            m.UserId == targetUserId &&
            m.Status == MemberStatus.Active &&
            m.DeletedAt == null, ct);
        if (targetMembership == null) return (null, "Member not found in this group.");

        var u = targetMembership.User;
        var sobriety = BuildSobrietyResponse(u, isSelf: false);
        var phone = targetMembership.IsPhoneShared ? u.PhoneNumber : null;

        var detail = new MemberDetailResponse(
            u.Id,
            u.DisplayName,
            targetMembership.Role.ToString(),
            targetMembership.Status.ToString(),
            targetMembership.IsProbationary,
            sobriety,
            phone,
            targetMembership.JoinedAt
        );

        return (detail, null);
    }

    // ── SuperAdmin ─────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<AdminMemberResponse>> GetAllMembersAsync(CancellationToken ct = default)
    {
        var users = await userManager.Users
            .AsNoTracking()
            .Where(u => u.DeletedAt == null)
            .OrderBy(u => u.DisplayName)
            .ToListAsync(ct);

        var userIds = users.Select(u => u.Id).ToList();
        var groupCounts = await db.GroupMemberships
            .AsNoTracking()
            .Where(m => userIds.Contains(m.UserId) && m.Status == MemberStatus.Active && m.DeletedAt == null)
            .GroupBy(m => m.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.UserId, g => g.Count, ct);

        var result = new List<AdminMemberResponse>();
        foreach (var user in users)
        {
            var groupCount = groupCounts.GetValueOrDefault(user.Id);
            var isLockedOut = await userManager.IsLockedOutAsync(user);
            var daysSober = user.SobrietyDate.HasValue
                ? (DateTime.UtcNow.Date - user.SobrietyDate.Value.ToDateTime(TimeOnly.MinValue)).Days
                : (int?)null;

            result.Add(new AdminMemberResponse(
                user.Id,
                user.DisplayName,
                user.FirstName,
                user.Email!,
                user.PhoneNumber,
                user.TimeZone,
                user.SobrietyDate,
                daysSober,
                user.IsSobrietyDatePublic,
                user.IsSuperAdmin,
                isLockedOut,
                user.CreatedAt,
                user.UpdatedAt,
                user.DeletedAt,
                user.LastLoginAt,
                user.EmailConfirmed,
                groupCount
            ));
        }

        return result;
    }

    public async Task<AdminMemberResponse?> GetUserByIdAsync(Guid targetUserId, CancellationToken ct = default)
    {
        var user = await userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == targetUserId, ct);
        if (user == null) return null;

        var groupCount = await db.GroupMemberships
            .AsNoTracking()
            .CountAsync(m => m.UserId == user.Id && m.Status == MemberStatus.Active && m.DeletedAt == null, ct);
        var daysSober = user.SobrietyDate.HasValue
            ? (DateTime.UtcNow.Date - user.SobrietyDate.Value.ToDateTime(TimeOnly.MinValue)).Days
            : (int?)null;

        var isLockedOut = await userManager.IsLockedOutAsync(user);

        return new AdminMemberResponse(
            user.Id,
            user.DisplayName,
            user.FirstName,
            user.Email!,
            user.PhoneNumber,
            user.TimeZone,
            user.SobrietyDate,
            daysSober,
            user.IsSobrietyDatePublic,
            user.IsSuperAdmin,
            isLockedOut,
            user.CreatedAt,
            user.UpdatedAt,
            user.DeletedAt,
            user.LastLoginAt,
            user.EmailConfirmed,
            groupCount
        );
    }

    // ── Mailing Address ────────────────────────────────────────────────────────

    public async Task<MailingAddressResponse?> GetMailingAddressAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null || user.DeletedAt != null) return null;

        return new MailingAddressResponse(
            user.MailingStreet,
            user.MailingCity,
            user.MailingState,
            user.MailingPostalCode,
            user.MailingCountry,
            user.MailingLatitude,
            user.MailingLongitude
        );
    }

    public async Task<(bool Success, string? Error)> UpdateMailingAddressAsync(
        Guid userId, UpdateMailingAddressRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.DeletedAt != null) return (false, "User not found.");

        user.MailingStreet     = request.MailingStreet;
        user.MailingCity       = request.MailingCity;
        user.MailingState      = request.MailingState;
        user.MailingPostalCode = request.MailingPostalCode;
        user.MailingCountry    = request.MailingCountry;

        // Best-effort geocoding via Nominatim — clear old coords first.
        user.MailingLatitude  = null;
        user.MailingLongitude = null;

        var addressParts = new[] { request.MailingStreet, request.MailingCity, request.MailingState, request.MailingCountry };
        var query = string.Join(", ", addressParts.Where(p => !string.IsNullOrWhiteSpace(p)));

        if (!string.IsNullOrWhiteSpace(query))
        {
            try
            {
                var http = httpClientFactory.CreateClient("Nominatim");
                var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&limit=1";
                var results = await http.GetFromJsonAsync<NominatimResult[]>(url, ct);
                if (results is { Length: > 0 })
                {
                    user.MailingLatitude  = results[0].Lat;
                    user.MailingLongitude = results[0].Lon;
                }
            }
            catch
            {
                // Geocoding is best-effort — a failure must not block the address save.
            }
        }

        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
        return (true, null);
    }

    private sealed class NominatimResult
    {
        public double Lat { get; init; }
        public double Lon { get; init; }
    }

    // ── Superadmin: user deactivation ─────────────────────────────────────────

    public async Task<(bool Success, string? Error)> DeactivateUserAsync(Guid adminUserId, Guid targetUserId, CancellationToken ct = default)
    {
        if (adminUserId == targetUserId)
            return (false, "Use DELETE /api/members/me to deactivate your own account.");

        var user = await userManager.FindByIdAsync(targetUserId.ToString());
        if (user == null) return (false, "User not found.");
        if (user.DeletedAt != null) return (false, "Account is already deactivated.");

        var now = DateTime.UtcNow;
        user.DeletedAt = now;
        user.UpdatedAt = now;

        var memberships = await db.GroupMemberships
            .Where(m => m.UserId == targetUserId && m.DeletedAt == null)
            .ToListAsync(ct);
        foreach (var m in memberships) { m.DeletedAt = now; m.UpdatedAt = now; }

        var tokens = await db.RefreshTokens
            .Where(t => t.UserId == targetUserId && t.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var t in tokens) t.RevokedAt = now;

        await userManager.UpdateAsync(user);
        await db.SaveChangesAsync(ct);

        await audit.LogAsync(SecurityEventType.AccountDeactivated, adminUserId,
            $"SuperAdmin deactivated userId={targetUserId}");
        return (true, null);
    }
}
