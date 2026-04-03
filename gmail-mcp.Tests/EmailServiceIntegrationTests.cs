using GmailMcp;

namespace GmailMcp.Tests;

public class EmailServiceIntegrationTests
{
    private static string? GmailAddress => Environment.GetEnvironmentVariable("GMAIL_ADDRESS");
    private static string? AppPassword => Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD");
    private static string? TestRecipient => Environment.GetEnvironmentVariable("TEST_RECIPIENT");

    private static bool HasCredentials =>
        !string.IsNullOrWhiteSpace(GmailAddress) &&
        !string.IsNullOrWhiteSpace(AppPassword) &&
        !string.IsNullOrWhiteSpace(TestRecipient);

    [Fact]
    [Trait("Category", "Integration")]
    public async Task SendAsync_WithRealCredentials_SendsEmail()
    {
        // Skipped when GMAIL_ADDRESS, GMAIL_APP_PASSWORD, or TEST_RECIPIENT are not set.
        // To run: set all three env vars and execute dotnet test --filter "Category=Integration"
        if (!HasCredentials)
            return;

        var service = new EmailService(GmailAddress!, AppPassword!);

        await service.SendAsync(
            to: TestRecipient!,
            subject: $"Integration test — {DateTime.UtcNow:O}",
            body: "This is an automated integration test email from GmailSend-mcp.");
    }
}
