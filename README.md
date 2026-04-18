<!--suppress HtmlDeprecatedAttribute -->
<div align="center">
   <h1 align="center">Gmail Send MCP</h1>
   <h4 align="center">
      A minimal Model Context Protocol server for sending email via Gmail SMTP —
      <a href="https://github.com/simonprovost/Gmail_Send_MCP_With_HTML/releases/latest">Download</a> ·
      <a href="#installation">Install</a> ·
      <a href="#tool-reference">Tool reference</a>
   </h4>
</div>

<div align="center">

<a href="https://github.com/simonprovost/Gmail_Send_MCP_With_HTML/releases/latest">
   <img alt="Release" src="https://img.shields.io/github/v/release/simonprovost/Gmail_Send_MCP_With_HTML?label=Release&color=472727&labelColor=FD706B&style=for-the-badge&logo=github&logoColor=white">
</a>

<a href="https://github.com/simonprovost/Gmail_Send_MCP_With_HTML/actions/workflows/release.yml">
   <img alt="Build" src="https://img.shields.io/github/actions/workflow/status/simonprovost/Gmail_Send_MCP_With_HTML/release.yml?label=Build&color=472727&labelColor=FD706B&style=for-the-badge&logo=githubactions&logoColor=white">
</a>

<img src="https://img.shields.io/static/v1?label=.NET&message=10&color=472727&labelColor=FD706B&style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10">

<img src="https://img.shields.io/static/v1?label=MCP&message=stdio&color=472727&labelColor=FD706B&style=for-the-badge&logo=anthropic&logoColor=white" alt="MCP stdio">

<img src="https://img.shields.io/static/v1?label=Tests&message=passing&color=472727&labelColor=FD706B&style=for-the-badge&logo=xunit&logoColor=white" alt="Tests passing">

<img src="https://img.shields.io/static/v1?label=License&message=MIT&color=472727&labelColor=FD706B&style=for-the-badge&logo=opensourceinitiative&logoColor=white" alt="MIT licensed">

</div>

> [!NOTE]
> This repository is a fork of [**Shade666/gmail-send-mcp**](https://github.com/Shade666/gmail-send-mcp). It adds:
>
> - A real fix for the `is_html` parameter (HTML bodies now render correctly via `multipart/alternative` with auto-detection and a plain-text fallback).
> - Pre-built single-file binaries published as **GitHub Release assets** so you no longer need the .NET SDK to use the server.

## <a id="about"></a><img src="docs/assets/icons/lucide/mail.svg" width="32" alt="" /> About The Project

`gmail-send-mcp` is a single-purpose Model Context Protocol server. It exposes one tool — `send_email` — over stdio, and sends the message through Gmail's SMTP relay using a Google App Password. No OAuth dance, no attachments, no multi-provider abstraction. Drop the binary in place, set two environment variables, register it with your MCP client, and your agent can send mail.

The server treats HTML bodies as first-class: pass an HTML document and it is delivered as a proper `multipart/alternative` message with a generated plain-text fallback. You don't need to remember the `is_html` flag — it's auto-detected — but you can pass it for clarity.

**Why did I do this MCP?**

Agentic Automation Sending Me NY-TIMES/WIRED designed newsletter on the morning without having to sub. to any of these news; I make my coffee, I read. As easy as that.

Cheers!
## <a id="installation"></a><img src="docs/assets/icons/lucide/rocket.svg" width="32" alt="" /> Getting Started

### <img src="docs/assets/icons/lucide/roman-i.svg" width="22" alt="" align="absmiddle" /> Pick & Install

- <details>
  <summary><strong>macOS — Apple Silicon</strong></summary>

  <br>

  ```bash
  mkdir -p ~/.local/bin && \
    curl -L https://github.com/simonprovost/Gmail_Send_MCP_With_HTML/releases/latest/download/gmail-mcp-osx-arm64.tar.gz \
    | tar -xz -C ~/.local/bin && \
    chmod +x ~/.local/bin/gmail-mcp && \
    xattr -d com.apple.quarantine ~/.local/bin/gmail-mcp 2>/dev/null || true
  ```

  </details>

- <details>
  <summary><strong>macOS — Intel</strong></summary>

  <br>

  ```bash
  mkdir -p ~/.local/bin && \
    curl -L https://github.com/simonprovost/Gmail_Send_MCP_With_HTML/releases/latest/download/gmail-mcp-osx-x64.tar.gz \
    | tar -xz -C ~/.local/bin && \
    chmod +x ~/.local/bin/gmail-mcp && \
    xattr -d com.apple.quarantine ~/.local/bin/gmail-mcp 2>/dev/null || true
  ```

  </details>

- <details>
  <summary><strong>Linux — x64</strong></summary>

  <br>

  ```bash
  mkdir -p ~/.local/bin && \
    curl -L https://github.com/simonprovost/Gmail_Send_MCP_With_HTML/releases/latest/download/gmail-mcp-linux-x64.tar.gz \
    | tar -xz -C ~/.local/bin && \
    chmod +x ~/.local/bin/gmail-mcp
  ```

  </details>

- <details>
  <summary><strong>Linux — arm64</strong></summary>

  <br>

  ```bash
  mkdir -p ~/.local/bin && \
    curl -L https://github.com/simonprovost/Gmail_Send_MCP_With_HTML/releases/latest/download/gmail-mcp-linux-arm64.tar.gz \
    | tar -xz -C ~/.local/bin && \
    chmod +x ~/.local/bin/gmail-mcp
  ```

  </details>

- <details>
  <summary><strong>Windows — x64</strong></summary>

  <br>

  ```powershell
  $dest = "$HOME\bin"; New-Item -ItemType Directory -Force -Path $dest | Out-Null
  Invoke-WebRequest https://github.com/simonprovost/Gmail_Send_MCP_With_HTML/releases/latest/download/gmail-mcp-win-x64.zip -OutFile $env:TEMP\gmail-mcp.zip
  Expand-Archive $env:TEMP\gmail-mcp.zip -DestinationPath $dest -Force
  ```

  </details>

Then [grab a Gmail App Password](#get-a-gmail-app-password) and [register the server](#register-with-an-mcp-client) with your client.

<details>
<summary><a id="manual-install"></a><strong>If you'd prefer a step-by-step walkthrough, click here.</strong></summary>

<br>

#### 1. Download

Pick whichever workflow you prefer.

##### Option A — Manual download

Grab the archive for your platform from the [latest release](https://github.com/simonprovost/Gmail_Send_MCP_With_HTML/releases/latest):

| Platform               | Asset                              |
|------------------------|------------------------------------|
| macOS — Apple Silicon  | `gmail-mcp-osx-arm64.tar.gz`       |
| macOS — Intel          | `gmail-mcp-osx-x64.tar.gz`         |
| Linux — x64            | `gmail-mcp-linux-x64.tar.gz`       |
| Linux — arm64          | `gmail-mcp-linux-arm64.tar.gz`     |
| Windows — x64          | `gmail-mcp-win-x64.zip`            |

Verify the download against `SHA256SUMS.txt` from the same release.

##### Option B — One-liner (`curl`)

```bash
# macOS — Apple Silicon (replace the asset name for your platform)
curl -L -o gmail-mcp.tar.gz \
  https://github.com/simonprovost/Gmail_Send_MCP_With_HTML/releases/latest/download/gmail-mcp-osx-arm64.tar.gz
```

Or pin a specific version (recommended for reproducible installs — replace `vX.Y.Z`, e.g. `v0.1.0`):

```bash
curl -L -o gmail-mcp.tar.gz \
  https://github.com/simonprovost/Gmail_Send_MCP_With_HTML/releases/download/vX.Y.Z/gmail-mcp-osx-arm64.tar.gz
```

#### 2. Install

```bash
# macOS / Linux
mkdir -p ~/.local/bin
tar -xzf gmail-mcp-osx-arm64.tar.gz -C ~/.local/bin
chmod +x ~/.local/bin/gmail-mcp

# macOS only — clear the gatekeeper quarantine flag
xattr -d com.apple.quarantine ~/.local/bin/gmail-mcp 2>/dev/null || true
```

```powershell
# Windows
Expand-Archive gmail-mcp-win-x64.zip -DestinationPath $HOME\bin
```

</details>

### <img src="docs/assets/icons/lucide/roman-ii.svg" width="22" alt="" align="absmiddle" /> Get a Gmail App Password

A Google App Password is a 16-character credential that lets a non-Google application sign in on your behalf, **without** sharing your real account password.

1. Enable 2-Step Verification on your Google account if you haven't already.
2. Visit [myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords).
3. Create a new app password — label it `gmail-send-mcp` (or anything you'll recognise later).
4. Copy the 16-character value. The format looks like `xxxx xxxx xxxx xxxx`.

> [!WARNING]
> Treat the App Password like the account password itself. Anyone holding it can send mail as you until you revoke it from the same page.

### <a id="register-with-an-mcp-client"></a><img src="docs/assets/icons/lucide/roman-iii.svg" width="22" alt="" align="absmiddle" /> Register With an MCP Client

<details>
<summary><strong>Codex CLI</strong></summary>

<br>

```bash
codex mcp add gmail \
  --env GMAIL_ADDRESS=you@gmail.com \
  --env "GMAIL_APP_PASSWORD=abcd efgh ijkl mnop" \
  -- ~/.local/bin/gmail-mcp
```

Verify with `codex mcp list` and `codex mcp get gmail`. Remove with `codex mcp remove gmail`.

</details>

<details>
<summary><strong>Claude Desktop</strong></summary>

<br>

Edit `~/Library/Application Support/Claude/claude_desktop_config.json` (macOS) or `%APPDATA%\Claude\claude_desktop_config.json` (Windows):

```json
{
  "mcpServers": {
    "gmail": {
      "command": "/Users/you/.local/bin/gmail-mcp",
      "env": {
        "GMAIL_ADDRESS": "you@gmail.com",
        "GMAIL_APP_PASSWORD": "abcd efgh ijkl mnop"
      }
    }
  }
}
```

</details>

<details>
<summary><strong>Claude Code (project-scoped)</strong></summary>

<br>

Create a `.mcp.json` at the project root (git-ignored — it carries credentials):

```json
{
  "mcpServers": {
    "gmail": {
      "command": "/absolute/path/to/gmail-mcp",
      "env": {
        "GMAIL_ADDRESS": "you@gmail.com",
        "GMAIL_APP_PASSWORD": "abcd efgh ijkl mnop"
      }
    }
  }
}
```

</details>

## <a id="tool-reference"></a><img src="docs/assets/icons/lucide/square-code.svg" width="32" alt="" /> Tool Reference

The server exposes a single tool: `send_email`.

| Parameter | Type    | Required | Description                                                                                          |
|-----------|---------|----------|------------------------------------------------------------------------------------------------------|
| `to`      | string  | yes      | Recipient address(es), comma-separated.                                                              |
| `subject` | string  | yes      | Subject line.                                                                                        |
| `body`    | string  | yes      | Email body. HTML is auto-detected and sent as `multipart/alternative` with a plain-text fallback.    |
| `cc`      | string  | no       | CC recipient(s), comma-separated.                                                                    |
| `bcc`     | string  | no       | BCC recipient(s), comma-separated.                                                                   |
| `is_html` | boolean | no       | Optional. Pass `true` to be explicit; HTML is auto-detected either way.                              |

### Example agent prompts

```
Send an email to alice@example.com with subject "Meeting notes" and a one-paragraph
recap of our last conversation.
```

```
Render this newsletter HTML and email it to team@example.com with subject
"Weekly digest // Wed 22 Apr 2026". Send the rendered HTML as the body.
```

```
Email bob@example.com and cc carol@example.com to confirm the deploy succeeded.
```

## <a id="capabilities-and-limits"></a><img src="docs/assets/icons/lucide/circle-check-big.svg" width="32" alt="" /> Capabilities & Limits

<details>
<summary><strong>What it does</strong></summary>

<br>

- Plain-text or HTML email through Gmail SMTP (`smtp.gmail.com:587`, STARTTLS).
- Multiple recipients on `To`, `Cc`, and `Bcc`.
- Auto-detects HTML and emits a properly structured `multipart/alternative` message — no client-side fallback rendering surprises.
- Reads credentials from environment variables only; nothing is persisted on disk.

</details>

<details>
<summary><strong>What it deliberately doesn't do</strong></summary>

<br>

- **Gmail only.** SMTP host and port are hardcoded. Other providers are out of scope.
- **App Password required.** No OAuth2.
- **No attachments.**
- **No delivery receipts.** The tool returns a success/failure string after `SEND` accepts the message; it does not poll for bounces.
- **Gmail's own limits apply.** Free accounts cap at ~500 recipients per rolling 24 hours.

</details>

## <a id="build-from-source"></a><img src="docs/assets/icons/lucide/wrench.svg" width="32" alt="" /> Build From Source

<details>
<summary>Only needed if you want to modify the server. Most users should download a release binary instead.</summary>

<br>

```bash
# Prerequisites: .NET 10 SDK
dotnet build
dotnet test

# Single-file self-contained binary for the host platform
dotnet publish gmail-mcp/gmail-mcp.csproj -c Release -r osx-arm64 -o out
ls -lh out/gmail-mcp   # ~39 MB, no DLLs
```

Supported runtime identifiers: `osx-arm64`, `osx-x64`, `linux-x64`, `linux-arm64`, `win-x64`. The same RIDs are produced automatically by `.github/workflows/release.yml` whenever a `v*` tag is pushed.

</details>

## <a id="troubleshooting"></a><img src="docs/assets/icons/lucide/triangle-alert.svg" width="32" alt="" /> Troubleshooting

<details>
<summary>Common errors and fixes.</summary>

<br>

- **`Authentication error`** — the App Password is wrong or has been revoked. Generate a new one at [myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords).
- **`GMAIL_ADDRESS environment variable is required`** — your MCP client isn't passing the env block. Re-check the `env` field in your client config (or the `--env` flag if using `codex mcp add`).
- **HTML email arrives as plain text in Gmail** — fixed in this fork. If you still see it, confirm your client is launching this binary (not the upstream one) and that the body actually contains HTML tags.
- **macOS "cannot be opened because it is from an unidentified developer"** — run `xattr -d com.apple.quarantine /path/to/gmail-mcp` once. The binary is unsigned; signing is on the roadmap if there is demand.

</details>

## <a id="license"></a><img src="docs/assets/icons/lucide/fingerprint-pattern.svg" width="32" alt="" /> License

`gmail-send-mcp` is released under the [MIT License](./LICENSE). The original work it forks from — [Shade666/gmail-send-mcp](https://github.com/Shade666/gmail-send-mcp) — is acknowledged with thanks.
