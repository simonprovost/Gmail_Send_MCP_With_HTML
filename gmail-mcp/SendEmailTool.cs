using System.ComponentModel;
using ModelContextProtocol.Server;

namespace GmailMcp;

[McpServerToolType]
public static class SendEmailTool
{
    [McpServerTool(Name = "send_email")]
    [Description("Send an email via Gmail SMTP. Requires GMAIL_ADDRESS and GMAIL_APP_PASSWORD environment variables.")]
    public static async Task<string> SendEmail(
        IEmailService emailService,
        [Description("Recipient email address(es), comma-separated")] string to,
        [Description("Email subject line")] string subject,
        [Description("Email body content")] string body,
        [Description("CC recipient(s), comma-separated")] string? cc = null,
        [Description("BCC recipient(s), comma-separated")] string? bcc = null,
        [Description("Set to true to send body as HTML")] bool is_html = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(to))
            return "Error: 'to' parameter is required.";
        if (string.IsNullOrWhiteSpace(subject))
            return "Error: 'subject' parameter is required.";
        if (string.IsNullOrWhiteSpace(body))
            return "Error: 'body' parameter is required.";

        try
        {
            await emailService.SendAsync(to, subject, body, cc, bcc, is_html, cancellationToken);
            return $"Email sent successfully to {to}.";
        }
        catch (Exception ex)
        {
            return $"Error sending email: {ex.Message}";
        }
    }
}
