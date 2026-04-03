using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace GmailMcp;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body,
                   string? cc = null, string? bcc = null, bool isHtml = false,
                   CancellationToken cancellationToken = default);
}

public sealed class EmailService : IEmailService
{
    private readonly string _gmailAddress;
    private readonly string _appPassword;

    public EmailService(string gmailAddress, string appPassword)
    {
        if (string.IsNullOrWhiteSpace(gmailAddress))
            throw new ArgumentException("Gmail address must not be null or empty.", nameof(gmailAddress));
        if (string.IsNullOrWhiteSpace(appPassword))
            throw new ArgumentException("App password must not be null or empty.", nameof(appPassword));

        _gmailAddress = gmailAddress;
        _appPassword = appPassword;
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
        message.Body = new TextPart(isHtml ? "html" : "plain") { Text = body };

        return message;
    }

    public async Task SendAsync(string to, string subject, string body,
                                string? cc = null, string? bcc = null, bool isHtml = false,
                                CancellationToken cancellationToken = default)
    {
        var message = BuildMessage(to, subject, body, cc, bcc, isHtml);

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(_gmailAddress, _appPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
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
