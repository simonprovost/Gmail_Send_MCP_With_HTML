using GmailMcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var gmailAddress = Environment.GetEnvironmentVariable("GMAIL_ADDRESS")
    ?? throw new ArgumentException("GMAIL_ADDRESS environment variable is required.");
if (!MimeKit.MailboxAddress.TryParse(gmailAddress, out _))
    throw new ArgumentException($"GMAIL_ADDRESS '{gmailAddress}' is not a valid email address.");
var appPassword = Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD")
    ?? throw new ArgumentException("GMAIL_APP_PASSWORD environment variable is required.");

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services.AddLogging();
builder.Services.AddSingleton<IEmailService>(sp =>
    new EmailService(gmailAddress, appPassword, sp.GetService<ILogger<EmailService>>()));

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
