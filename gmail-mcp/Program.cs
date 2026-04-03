using GmailMcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var gmailAddress = Environment.GetEnvironmentVariable("GMAIL_ADDRESS")
    ?? throw new ArgumentException("GMAIL_ADDRESS environment variable is required.");
var appPassword = Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD")
    ?? throw new ArgumentException("GMAIL_APP_PASSWORD environment variable is required.");

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services.AddSingleton<IEmailService>(new EmailService(gmailAddress, appPassword));

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
