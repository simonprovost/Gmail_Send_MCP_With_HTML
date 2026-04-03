# GmailSend MCP

A minimal MCP (Model Context Protocol) server for sending email via Gmail SMTP. Exposes a single `send_email` tool to Claude over stdio transport.

## Prerequisites

### 1. .NET 10 SDK

Install from [dot.net](https://dotnet.microsoft.com/download).

### 2. Gmail App Password

Requires a Google account with 2-Step Verification enabled.

1. Go to [myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords)
2. Create a new app password — label it something like `claude-mcp`
3. Copy the 16-character password (format: `xxxx xxxx xxxx xxxx`)

## Setup

### Environment Variables

The server reads credentials from environment variables at startup and will refuse to start if either is missing.

| Variable | Example value |
|---|---|
| `GMAIL_ADDRESS` | `you@gmail.com` |
| `GMAIL_APP_PASSWORD` | `abcd efgh ijkl mnop` |

### Project MCP Config (Claude Code)

A `.mcp.json` file is used when running Claude Code from this project directory. It is git-ignored because it contains credentials.

Copy the template and fill in your credentials:

```bash
cp .mcp.json.example .mcp.json   # if you create one, or edit .mcp.json directly
```

Edit `.mcp.json`:

```json
{
  "mcpServers": {
    "gmail": {
      "command": "dotnet",
      "args": ["run", "--project", "gmail-mcp"],
      "env": {
        "GMAIL_ADDRESS": "you@gmail.com",
        "GMAIL_APP_PASSWORD": "abcd efgh ijkl mnop"
      }
    }
  }
}
```

### Claude Desktop Config

Add to `%APPDATA%\Claude\claude_desktop_config.json` (Windows) or `~/Library/Application Support/Claude/claude_desktop_config.json` (macOS):

```json
{
  "mcpServers": {
    "gmail": {
      "command": "dotnet",
      "args": ["run", "--project", "C:/path/to/GmailSend-mcp/gmail-mcp"],
      "env": {
        "GMAIL_ADDRESS": "you@gmail.com",
        "GMAIL_APP_PASSWORD": "abcd efgh ijkl mnop"
      }
    }
  }
}
```

> For faster startup, replace `dotnet run --project gmail-mcp` with the path to the compiled binary after running `dotnet publish`.

## Usage

Once registered, Claude will have access to the `send_email` tool.

### Tool: `send_email`

| Parameter | Type | Required | Description |
|---|---|---|---|
| `to` | string | yes | Recipient address(es), comma-separated |
| `subject` | string | yes | Email subject line |
| `body` | string | yes | Email body content |
| `cc` | string | no | CC recipient(s), comma-separated |
| `bcc` | string | no | BCC recipient(s), comma-separated |
| `is_html` | bool | no | Send body as HTML (default: false) |

### Example prompts

```
Send an email to alice@example.com with subject "Meeting notes" and a summary of our conversation.
```

```
Draft and send a HTML-formatted status update to the team at team@example.com.
```

```
Email bob@example.com and cc carol@example.com to let them know the deploy succeeded.
```

## Capabilities

- Send plain-text or HTML email via Gmail SMTP
- Multiple recipients on To, CC, and BCC (comma-separated)
- Credentials sourced from environment variables only — nothing stored in code

## Limitations

- **Gmail only** — hardcoded to `smtp.gmail.com:587` with STARTTLS
- **App Password required** — OAuth2 is not supported
- **No attachments**
- **No sent-mail tracking** — the tool returns a success/failure string but does not verify delivery
- **Gmail sending limits** — Gmail enforces a limit of 500 emails per day for regular accounts
- **`dotnet run` startup latency** — each Claude session cold-starts the server via `dotnet run`. Use a published binary to reduce this

## Development

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~EmailService"

# Run the server manually (requires env vars)
$env:GMAIL_ADDRESS="you@gmail.com"
$env:GMAIL_APP_PASSWORD="xxxx xxxx xxxx xxxx"
dotnet run --project gmail-mcp
```

## Project Structure

```
gmail-mcp/
├── Program.cs        — DI wiring, env var validation, MCP stdio server
├── EmailService.cs   — IEmailService interface + MailKit SMTP implementation
└── SendEmailTool.cs  — MCP tool definition ([McpServerTool] attribute)
gmail-mcp.Tests/
├── EmailServiceTests.cs   — Unit tests for credential validation and message construction
└── SendEmailToolTests.cs  — Unit tests for tool parameter validation and send behaviour
```
