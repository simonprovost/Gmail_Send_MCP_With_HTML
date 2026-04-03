# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A minimal MCP (Model Context Protocol) server written in C# (.NET 10) that exposes a `send_email` tool via stdio transport. Sends email through Gmail SMTP using an App Password — no OAuth2, no attachments, no multi-provider support.

## Commands

```bash
# Build
dotnet build

# Run (requires env vars set)
dotnet run --project gmail-mcp

# Run tests
dotnet test

# Run a single test
dotnet test --filter "FullyQualifiedName~EmailService"
```

## Required Environment Variables

```
GMAIL_ADDRESS=you@gmail.com
GMAIL_APP_PASSWORD=xxxx-xxxx-xxxx-xxxx
```

The server must fail fast at startup if either is missing — not at send time.

## Architecture

```
gmail-mcp/
├── Program.cs        — DI wiring, env var validation, MCP stdio server setup
├── EmailService.cs   — MailKit SMTP send logic (implements IEmailService)
├── SendEmailTool.cs  — MCP tool definition ([McpServerTool] attribute)
gmail-mcp.Tests/
└── ...               — xUnit tests using mocked IEmailService
```

**Key design decisions:**
- `IEmailService` interface is required to enable mocking in tests without hitting SMTP
- `EmailService` connects to `smtp.gmail.com:587` with `SecureSocketOptions.StartTls`, disconnects in `finally`
- `SendEmailTool` returns a plain success/failure string — no structured output
- `EmailService` should expose a `BuildMessage()` method (tested separately from `SendAsync`)

## NuGet Packages

```xml
<PackageReference Include="ModelContextProtocol" Version="*" />
<PackageReference Include="MailKit" Version="*" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="*" />
```

xUnit test project also uses `Moq` for mocking `IEmailService`.

## Claude Desktop Registration

```json
{
  "mcpServers": {
    "gmail": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/gmail-mcp"],
      "env": {
        "GMAIL_ADDRESS": "you@gmail.com",
        "GMAIL_APP_PASSWORD": "xxxx-xxxx-xxxx-xxxx"
      }
    }
  }
}
```

## Tool Parameters

| Name      | Type   | Required | Notes                          |
| --------- | ------ | -------- | ------------------------------ |
| `to`      | string | yes      | Comma-separated recipients     |
| `subject` | string | yes      |                                |
| `body`    | string | yes      |                                |
| `cc`      | string | no       |                                |
| `bcc`     | string | no       |                                |
| `is_html` | bool   | no       | Default: false                 |

## Out of Scope

OAuth2, attachments, multiple SMTP providers, sent mail tracking, rate limiting.
