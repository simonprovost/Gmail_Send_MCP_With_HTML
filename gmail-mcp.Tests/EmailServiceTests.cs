using GmailMcp;
using MimeKit;
using Xunit;

namespace GmailMcp.Tests;

public class EmailServiceTests
{
    // Test 1: Missing credentials throw ArgumentException
    [Theory]
    [InlineData(null, "password")]
    [InlineData("", "password")]
    [InlineData("   ", "password")]
    [InlineData("user@gmail.com", null)]
    [InlineData("user@gmail.com", "")]
    [InlineData("user@gmail.com", "   ")]
    public void Constructor_InvalidCredentials_ThrowsArgumentException(string? address, string? password)
    {
        Assert.Throws<ArgumentException>(() => new EmailService(address!, password!));
    }

    // Test 2: BuildMessage constructs a valid MimeMessage
    [Fact]
    public void BuildMessage_ValidArgs_ReturnsCorrectMessage()
    {
        var svc = new EmailService("sender@gmail.com", "secret");
        var msg = svc.BuildMessage("recipient@example.com", "Hello", "Body text", null, null, false);

        Assert.Equal("sender@gmail.com", ((MailboxAddress)msg.From[0]).Address);
        Assert.Single(msg.To);
        Assert.Equal("recipient@example.com", ((MailboxAddress)msg.To[0]).Address);
        Assert.Equal("Hello", msg.Subject);
        var body = Assert.IsType<TextPart>(msg.Body);
        Assert.Equal("plain", body.ContentType.MediaSubtype);
        Assert.Equal("Body text", body.Text);
    }

    // Test 3: BuildMessage handles comma-separated recipients
    [Fact]
    public void BuildMessage_CommaSeparatedRecipients_AddedCorrectly()
    {
        var svc = new EmailService("sender@gmail.com", "secret");
        var msg = svc.BuildMessage("a@x.com, b@x.com", "Sub", "Body", "c@x.com", "d@x.com", false);

        Assert.Equal(2, msg.To.Mailboxes.Count());
        Assert.Single(msg.Cc.Mailboxes);
        Assert.Single(msg.Bcc.Mailboxes);
    }

    // Test 4: BuildMessage with isHtml=true sets HTML body
    [Fact]
    public void BuildMessage_IsHtmlTrue_SetsHtmlBody()
    {
        var svc = new EmailService("sender@gmail.com", "secret");
        var msg = svc.BuildMessage("a@x.com", "Sub", "<b>Bold</b>", null, null, true);

        Assert.NotNull(msg.HtmlBody);
        Assert.Contains("<b>Bold</b>", msg.HtmlBody);
    }

    // Test 5: BuildMessage auto-detects HTML body even when isHtml=false
    [Fact]
    public void BuildMessage_HtmlBodyDetected_SendsAsHtml()
    {
        var svc = new EmailService("sender@gmail.com", "secret");
        var msg = svc.BuildMessage("a@x.com", "Sub",
            "<!DOCTYPE html><html><body><p>Hi</p></body></html>", null, null, false);

        Assert.NotNull(msg.HtmlBody);
        Assert.Contains("<p>Hi</p>", msg.HtmlBody);
    }
}
