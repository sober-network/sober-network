namespace SoberNetwork.Core.Interfaces;

/// <summary>Sends transactional emails used by authentication and group workflows.</summary>
public interface IEmailService
{
    /// <summary>Sends an email confirmation message to a newly registered or changing member.</summary>
    Task SendEmailConfirmationAsync(string toEmail, string displayName, string confirmationLink);

    /// <summary>Sends a password reset message to a member.</summary>
    Task SendPasswordResetAsync(string toEmail, string displayName, string resetLink);

    /// <summary>Sends an approval notice after a member is admitted to a group.</summary>
    Task SendGroupJoinApprovedAsync(string toEmail, string displayName, string groupName);

    /// <summary>Sends a pending join request notification to a group administrator.</summary>
    Task SendGroupJoinRequestAsync(string adminEmail, string applicantName, string groupName, string approvalLink);

    /// <summary>Sends a rejection notice for a group join request.</summary>
    Task SendGroupJoinRejectedAsync(string toEmail, string displayName, string groupName);
}
