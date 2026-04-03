namespace GmailMcp;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body,
                   string? cc = null, string? bcc = null, bool isHtml = false,
                   CancellationToken cancellationToken = default);
}
