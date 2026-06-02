namespace SoberNetwork.Core.Interfaces;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string toEmail, string displayName, string confirmationLink);
    Task SendPasswordResetAsync(string toEmail, string displayName, string resetLink);
    Task SendGroupJoinApprovedAsync(string toEmail, string displayName, string groupName);
    Task SendGroupJoinRequestAsync(string adminEmail, string applicantName, string groupName, string approvalLink);
    Task SendGroupJoinRejectedAsync(string toEmail, string displayName, string groupName);
}
