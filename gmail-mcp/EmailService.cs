using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace GmailMcp;

public sealed class EmailService : IEmailService
{
    private const string SmtpHost = "smtp.gmail.com";
    private const int SmtpPort = 587;

    private readonly string _gmailAddress;
    private readonly string _appPassword;
    private readonly ILogger<EmailService>? _logger;

    public EmailService(string gmailAddress, string appPassword, ILogger<EmailService>? logger = null)
    {
        if (string.IsNullOrWhiteSpace(gmailAddress))
            throw new ArgumentException("Gmail address must not be null or empty.", nameof(gmailAddress));
        if (string.IsNullOrWhiteSpace(appPassword))
            throw new ArgumentException("App password must not be null or empty.", nameof(appPassword));

        _gmailAddress = gmailAddress;
        _appPassword = appPassword;
        _logger = logger;
    }

    public MimeMessage BuildMessage(string to, string subject, string body,
                                    string? cc, string? bcc, bool isHtml)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_gmailAddress));

        AddAddresses(message.To, to);
        if (!string.IsNullOrWhiteSpace(cc))
            AddAddresses(message.Cc, cc);
        if (!string.IsNullOrWhiteSpace(bcc))
            AddAddresses(message.Bcc, bcc);

        message.Subject = subject;

        var treatAsHtml = isHtml || LooksLikeHtml(body);
        if (treatAsHtml)
        {
            var builder = new BodyBuilder
            {
                HtmlBody = body,
                TextBody = HtmlToPlainText(body),
            };
            message.Body = builder.ToMessageBody();
        }
        else
        {
            message.Body = new TextPart("plain") { Text = body };
        }

        return message;
    }

    private static bool LooksLikeHtml(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return false;
        var trimmed = body.AsSpan().TrimStart();
        if (trimmed.Length == 0) return false;
        if (trimmed[0] != '<') return false;
        return trimmed.IndexOf('>') > 0 &&
               (body.Contains("</", StringComparison.OrdinalIgnoreCase)
                || body.Contains("<!doctype", StringComparison.OrdinalIgnoreCase)
                || body.Contains("<html", StringComparison.OrdinalIgnoreCase)
                || body.Contains("<body", StringComparison.OrdinalIgnoreCase)
                || body.Contains("<table", StringComparison.OrdinalIgnoreCase)
                || body.Contains("<div", StringComparison.OrdinalIgnoreCase)
                || body.Contains("<p", StringComparison.OrdinalIgnoreCase)
                || body.Contains("<br", StringComparison.OrdinalIgnoreCase));
    }

    private static string HtmlToPlainText(string html)
    {
        var noTags = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", " ");
        var decoded = System.Net.WebUtility.HtmlDecode(noTags);
        return System.Text.RegularExpressions.Regex.Replace(decoded, @"\s+", " ").Trim();
    }

    public async Task SendAsync(string to, string subject, string body,
                                string? cc = null, string? bcc = null, bool isHtml = false,
                                CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Sending email to {To}", to);

        var message = BuildMessage(to, subject, body, cc, bcc, isHtml);

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(SmtpHost, SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(_gmailAddress, _appPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            _logger?.LogInformation("Email sent successfully to {To}", to);
        }
        finally
        {
            await client.DisconnectAsync(quit: true, cancellationToken);
        }
    }

    private static void AddAddresses(InternetAddressList list, string addresses)
    {
        if (InternetAddressList.TryParse(addresses, out var parsed))
            list.AddRange(parsed);
        else
            list.Add(MailboxAddress.Parse(addresses.Trim()));
    }
}
