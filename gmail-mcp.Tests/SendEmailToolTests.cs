using GmailMcp;
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
}
