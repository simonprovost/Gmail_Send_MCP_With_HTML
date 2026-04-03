using System.Net.Sockets;
using GmailMcp;
using MailKit.Net.Smtp;
using MailKit.Security;
using Moq;
using Xunit;

namespace GmailMcp.Tests;

public class SendEmailToolTests
{
    // Test 1: Missing required params return error string and never call SendAsync
    [Theory]
    [InlineData("", "subject", "body")]
    [InlineData("   ", "subject", "body")]
    [InlineData("to@x.com", "", "body")]
    [InlineData("to@x.com", "   ", "body")]
    [InlineData("to@x.com", "subject", "")]
    [InlineData("to@x.com", "subject", "   ")]
    public async Task SendEmail_MissingRequiredParams_ReturnsErrorWithoutSending(
        string to, string subject, string body)
    {
        var mock = new Mock<IEmailService>(MockBehavior.Strict);

        var result = await SendEmailTool.SendEmail(mock.Object, to, subject, body);

        Assert.StartsWith("Error:", result);
        mock.VerifyNoOtherCalls();
    }

    // Test 2: Valid params call IEmailService.SendAsync with correct args
    [Fact]
    public async Task SendEmail_ValidParams_CallsSendAsyncAndReturnsSuccess()
    {
        var mock = new Mock<IEmailService>();
        mock.Setup(s => s.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await SendEmailTool.SendEmail(
            mock.Object, "to@example.com", "Subject", "Body", "cc@x.com", "bcc@x.com", false);

        Assert.Contains("successfully", result);
        mock.Verify(s => s.SendAsync(
            "to@example.com", "Subject", "Body", "cc@x.com", "bcc@x.com", false,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // Test 3: SendAsync throwing returns error string
    [Fact]
    public async Task SendEmail_SendAsyncThrows_ReturnsErrorString()
    {
        var mock = new Mock<IEmailService>();
        mock.Setup(s => s.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Auth failed"));

        var result = await SendEmailTool.SendEmail(
            mock.Object, "to@example.com", "Subject", "Body");

        Assert.StartsWith("Error sending email:", result);
        Assert.Contains("Auth failed", result);
    }

    // Test 4: OperationCanceledException returns cancellation message
    [Fact]
    public async Task SendEmail_OperationCanceledException_ReturnsCancelledMessage()
    {
        var mock = new Mock<IEmailService>();
        mock.Setup(s => s.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var result = await SendEmailTool.SendEmail(
            mock.Object, "to@example.com", "Subject", "Body");

        Assert.Equal("Email sending was cancelled.", result);
    }

    // Test 5: AuthenticationException returns auth error with credential hint
    [Fact]
    public async Task SendEmail_AuthenticationException_ReturnsAuthError()
    {
        var mock = new Mock<IEmailService>();
        mock.Setup(s => s.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AuthenticationException("535 Bad credentials"));

        var result = await SendEmailTool.SendEmail(
            mock.Object, "to@example.com", "Subject", "Body");

        Assert.StartsWith("Authentication error:", result);
        Assert.Contains("GMAIL_APP_PASSWORD", result);
    }

    // Test 6: SmtpCommandException returns SMTP error with status code
    [Fact]
    public async Task SendEmail_SmtpCommandException_ReturnsSmtpError()
    {
        var mock = new Mock<IEmailService>();
        mock.Setup(s => s.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SmtpCommandException(SmtpErrorCode.MessageNotAccepted, SmtpStatusCode.MailboxBusy, "Mailbox busy"));

        var result = await SendEmailTool.SendEmail(
            mock.Object, "to@example.com", "Subject", "Body");

        Assert.StartsWith("SMTP error", result);
    }

    // Test 7: SocketException returns connection error with network hint
    [Fact]
    public async Task SendEmail_SocketException_ReturnsConnectionError()
    {
        var mock = new Mock<IEmailService>();
        mock.Setup(s => s.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SocketException(11001));

        var result = await SendEmailTool.SendEmail(
            mock.Object, "to@example.com", "Subject", "Body");

        Assert.StartsWith("Connection error:", result);
        Assert.Contains("network connectivity", result);
    }
}
