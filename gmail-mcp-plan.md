# Gmail MCP Server — Claude Code Handoff Plan

## Goal

A minimal, local MCP server written in C# (.NET 8) that exposes a single `send_email` tool via stdio transport. No hardcoded credentials. No unnecessary dependencies.

---

## Stack

| Concern     | Choice                                    | Rationale                                         |
| ----------- | ----------------------------------------- | ------------------------------------------------- |
| Language    | C# 14 / .NET 10                           | User preference                                   |
| MCP SDK     | `ModelContextProtocol` (Microsoft, NuGet) | Official .NET SDK, stdio support                  |
| Email       | `MailKit` (NuGet)                         | Industry standard, replaces obsolete `SmtpClient` |
| SMTP host   | Gmail (`smtp.gmail.com:587`)              | Simplest auth story (App Password)                |
| Credentials | Environment variables                     | No secrets in source; works cross-platform        |
| Transport   | stdio                                     | Local server, zero networking overhead            |

---

## Prerequisites (document for user)

1. **Gmail App Password** — requires 2FA enabled on the Google account.
   Generate at: https://myaccount.google.com/apppasswords
   Label it something like `claude-mcp`.

2. **Environment variables** — set before running the server:
   ```
   GMAIL_ADDRESS=you@gmail.com
   GMAIL_APP_PASSWORD=xxxx-xxxx-xxxx-xxxx
   ```
   On Windows: set via System Properties → Environment Variables, or in the Claude `claude_desktop_config.json` `env` block (see Registration below).

---

## Project Structure

```
gmail-mcp/
├── gmail-mcp.csproj
├── Program.cs          ← wires up MCP server + DI
├── EmailService.cs     ← MailKit send logic
└── SendEmailTool.cs    ← MCP tool definition
```

---

## NuGet Dependencies

```xml
<PackageReference Include="ModelContextProtocol" Version="*" />
<PackageReference Include="MailKit" Version="*" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="*" />
```

---

## Implementation Notes

### `Program.cs`
- Read `GMAIL_ADDRESS` and `GMAIL_APP_PASSWORD` from `Environment.GetEnvironmentVariable`.
- Fail fast with a clear error message if either is missing.
- Register `EmailService` as a singleton.
- Register the MCP server with stdio transport.
- Register `SendEmailTool`.

### `EmailService.cs`
- Constructor takes `gmailAddress` and `appPassword` strings.
- Single async method: `SendAsync(string to, string subject, string body, string? cc, string? bcc, bool isHtml)`.
- Connects to `smtp.gmail.com:587` with `SecureSocketOptions.StartTls`.
- Disconnects cleanly in a `finally` block.
- Throws descriptive exceptions on auth or send failure.

### `SendEmailTool.cs`
- Decorated with `[McpServerTool]` attribute.
- Parameters:
  | Name      | Type   | Required | Description                             |
  | --------- | ------ | -------- | --------------------------------------- |
  | `to`      | string | ✅       | Recipient address(es), comma-separated  |
  | `subject` | string | ✅       | Email subject line                      |
  | `body`    | string | ✅       | Email body                              |
  | `cc`      | string | ❌       | CC recipients                           |
  | `bcc`     | string | ❌       | BCC recipients                          |
  | `is_html` | bool   | ❌       | Set true for HTML body (default: false) |
- Returns a simple success/failure string — no structured output needed.
- Tool description should clearly state it sends via Gmail SMTP.

---

## Registration in Claude (`claude_desktop_config.json`)

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

> For production use, prefer setting env vars at the OS level rather than in the config file to avoid credentials sitting in a JSON file.

---

## TDD Approach (Red/Green)

Suggested test project: `gmail-mcp.Tests` (xUnit).

### Tests to write first:

1. **`EmailService` — missing credentials throws**
   Pass null/empty strings → expect `ArgumentException`.

2. **`EmailService` — constructs valid `MimeMessage`**
   Unit test message assembly without sending (extract a `BuildMessage()` method).

3. **`SendEmailTool` — missing required params returns error**
   Pass empty `to` or `subject` → expect error string, no send attempted.

4. **`SendEmailTool` — valid params calls `EmailService.SendAsync`**
   Mock `IEmailService`, verify `SendAsync` called with correct args.

Extract `IEmailService` interface to enable mocking without hitting SMTP.

---

## Out of Scope (keep it minimal)

- OAuth2 / refresh tokens — App Password is sufficient for personal/team use
- Attachments
- Multiple SMTP providers
- Sent mail tracking / read receipts
- Rate limiting (Gmail's 500/day limit is sufficient)

---

## Acceptance Criteria

- [ ] `dotnet build` succeeds with no warnings
- [ ] All unit tests pass
- [ ] Server starts and registers in Claude without error
- [ ] `send_email` tool visible in Claude tool list
- [ ] Test email received when tool is invoked
- [ ] Missing env vars produce a clear startup error, not a runtime crash
