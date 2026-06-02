using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;
using SoberNetwork.Core.Interfaces;

namespace SoberNetwork.Infrastructure.Services;

public class EmailService(
    IResend resend,
    IConfiguration config,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly string _from = config["Resend:FromAddress"]
        ?? throw new InvalidOperationException("Resend:FromAddress is not configured.");

    public async Task SendEmailConfirmationAsync(string toEmail, string displayName, string confirmationLink)
    {
        await SendAsync(
            to: toEmail,
            subject: "Confirm your Sober Network email",
            html: $"""
                <h2>Welcome to Sober Network, {displayName}!</h2>
                <p>Please confirm your email address to complete registration.</p>
                <p><a href="{confirmationLink}" style="background:#4f46e5;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">Confirm Email</a></p>
                <p>This link expires in 24 hours.</p>
                <p>If you didn't create this account, you can ignore this email.</p>
                """
        );
    }

    public async Task SendPasswordResetAsync(string toEmail, string displayName, string resetLink)
    {
        await SendAsync(
            to: toEmail,
            subject: "Reset your Sober Network password",
            html: $"""
                <h2>Password Reset Request</h2>
                <p>Hi {displayName}, we received a request to reset your password.</p>
                <p><a href="{resetLink}" style="background:#4f46e5;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">Reset Password</a></p>
                <p>This link expires in 1 hour.</p>
                <p>If you didn't request a password reset, please ignore this email.</p>
                """
        );
    }

    public async Task SendGroupJoinApprovedAsync(string toEmail, string displayName, string groupName)
    {
        await SendAsync(
            to: toEmail,
            subject: $"You've been approved to join {groupName}",
            html: $"""
                <h2>Welcome to {groupName}!</h2>
                <p>Hi {displayName}, your request to join <strong>{groupName}</strong> on Sober Network has been approved.</p>
                <p>You can now log in and access the group.</p>
                """
        );
    }

    public async Task SendGroupJoinRequestAsync(string adminEmail, string applicantName, string groupName, string approvalLink)
    {
        await SendAsync(
            to: adminEmail,
            subject: $"New join request for {groupName}",
            html: $"""
                <h2>New Membership Request</h2>
                <p><strong>{applicantName}</strong> has requested to join <strong>{groupName}</strong>.</p>
                <p><a href="{approvalLink}" style="background:#4f46e5;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">Review Request</a></p>
                """
        );
    }

    public async Task SendGroupJoinRejectedAsync(string toEmail, string displayName, string groupName)
    {
        await SendAsync(
            to: toEmail,
            subject: $"Your request to join {groupName}",
            html: $"""
                <h2>Membership Request Update</h2>
                <p>Hi {displayName}, your request to join <strong>{groupName}</strong> on Sober Network was not approved at this time.</p>
                <p>If you have questions, please contact the group administrator directly.</p>
                """
        );
    }

    private async Task SendAsync(string to, string subject, string html)
    {
        try
        {
            var message = new EmailMessage
            {
                From = _from,
                Subject = subject,
                HtmlBody = html
            };
            message.To.Add(to);

            await resend.EmailSendAsync(message);
            logger.LogInformation("Email sent to {To} — subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To} — subject: {Subject}", to, subject);
            throw;
        }
    }
}
