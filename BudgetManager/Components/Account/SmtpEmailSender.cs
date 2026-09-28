
using BudgetManager.Data;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using MimeKit;

namespace BudgetManager.Components.Account;

public sealed class SmtpEmailSender : IEmailSender<ApplicationUser>
{
    private readonly IConfiguration _configuration;

    public SmtpEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task SendConfirmationLinkAsync(
        ApplicationUser user,
        string email,
        string confirmationLink)
    {
        return SendEmailAsync(
            email,
            "Confirm your Budget Manager account",
            $"""
            <h2>Welcome to Budget Manager!</h2>
            <p>Thank you for registering.</p>
            <p>Please confirm your email address by clicking the link below:</p>
            <p><a href="{confirmationLink}">Confirm email address</a></p>
            <p>If you did not create this account, you can ignore this message.</p>
            """);
    }

    public Task SendPasswordResetLinkAsync(
        ApplicationUser user,
        string email,
        string resetLink)
    {
        return SendEmailAsync(
            email,
            "Reset your Budget Manager password",
            $"""
            <h2>Password reset</h2>
            <p>Click the link below to reset your password:</p>
            <p><a href="{resetLink}">Reset password</a></p>
            <p>If you did not request this, you can ignore this message.</p>
            """);
    }

    public Task SendPasswordResetCodeAsync(
        ApplicationUser user,
        string email,
        string resetCode)
    {
        return SendEmailAsync(
            email,
            "Reset your Budget Manager password",
            $"""
            <h2>Password reset</h2>
            <p>Use the following code to reset your password:</p>
            <p><strong>{resetCode}</strong></p>
            """);
    }

    private async Task SendEmailAsync(
        string recipient,
        string subject,
        string htmlBody)
    {
        var smtp = _configuration.GetSection("Smtp");

        var host = smtp["Host"]
            ?? throw new InvalidOperationException("SMTP host is missing.");

        var username = smtp["Username"]
            ?? throw new InvalidOperationException("SMTP username is missing.");

        var password = smtp["Password"]
            ?? throw new InvalidOperationException("SMTP password is missing.");

        var fromEmail = smtp["FromEmail"]
            ?? throw new InvalidOperationException("SMTP sender email is missing.");

        var port = smtp.GetValue<int>("Port", 587);

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress("Budget Manager", fromEmail));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;

        message.Body = new BodyBuilder
        {
            HtmlBody = htmlBody
        }.ToMessageBody();

        using var client = new SmtpClient();

        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
