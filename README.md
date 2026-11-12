# Forensic IMAP Downloader

A Windows desktop tool that bulk-downloads an IMAP mailbox into a hash-verified ZIP
archive for forensic collection: every message is saved as a timestamp-preserving
`.eml`, per-file MD5 hashes and a SHA-1 hash of the final archive are recorded, and a
continuous log is kept alongside the evidence for chain-of-custody purposes.

Folders are always opened **read-only**, so collection never mutates mailbox state
(e.g. `\Seen` flags) on the source account.

## Requirements

- Windows (WinForms)
- [.NET 10 SDK](https://dotnet.microsoft.com/) or later

## Building and running

```powershell
dotnet build ForensicIMAPDownloader.slnx
dotnet run --project ForensicIMAPDownloader/ForensicIMAPDownloader.vbproj
```

Or open `ForensicIMAPDownloader.slnx` in Visual Studio 2022+ and run/debug `ForensicIMAPDownloader`.

## Running the tests

```powershell
dotnet test ForensicIMAPDownloader.Tests/ForensicIMAPDownloader.Tests.vbproj
```

## Usage

1. Pick a destination `.zip` file (**Browse**).
2. Enter the IMAP server, port, username and password, then **Connect**. For
   Outlook.com/Microsoft 365 and Gmail, pick **Microsoft OAuth** or **Google OAuth** in
   **Auth** instead of typing a password - see [Authentication](#authentication).
3. Optionally narrow down what gets downloaded with **Filter** (build an
   `AND(...)`/`OR(...)`/`NOT(...)` query against fields like `Subject`, `From`,
   `Sent After`, `Seen`, etc.).
4. Tick the folders to download, then **Download**. The list can be sorted by any column
   and its columns resized; **Select all** / **Select none** / **Invert selection** act on
   every folder, and Space toggles all the highlighted rows. The selection is logged and
   locked while the download runs.
5. **Abort** during a download lets you discard, keep (for a later resume), or
   archive-and-hash what has been downloaded so far.

## Authentication

| Auth | Use for | Needs |
|---|---|---|
| Password | Generic IMAP servers; Gmail with an app password | Username and password |
| Microsoft OAuth | Outlook.com / Hotmail; Microsoft 365 with your own client ID | Nothing for personal accounts (built-in application); your own app registration for work accounts |
| Google OAuth | Gmail | `client_secret.json` of your own Google Cloud OAuth client |

Microsoft and Google no longer accept a plain password over IMAP, so those mailboxes are
reached with OAuth 2.0 (SASL XOAUTH2): the account owner signs in in the system browser -
or, for Microsoft, on another device with a device code - and the tool never sees the
password. Registering the applications is free and is described in
[docs/oauth-setup.md](docs/oauth-setup.md).

Tokens are held in memory only. The sign-in (method, client ID, account, granted scopes)
is written to `report.log`, and when the download finishes the tool offers to end the
access it was granted.

The planned split between the free and the paid edition is in
[docs/editions.md](docs/editions.md).

## Project layout

- `ForensicIMAPDownloader.Core` - platform-independent library: IMAP connection/search/
  download handling (`ImapDownloadService`), the search query parser
  (`SearchQueryParser`), the zip/hash archiving and verification logic
  (`MailArchiveService`), file hashing (`HashCalculator`), and the OAuth token providers
  (`OAuth/`, built on MSAL and Google.Apis.Auth). No WinForms dependency,
  so it builds and its tests run on any OS.
- `ForensicIMAPDownloader` - the WinForms UI, targeting `net10.0-windows`.
- `ForensicIMAPDownloader.Tests` - xUnit tests for the `Core` project.

## Architecture notes

- **Mail library**: [MailKit](https://github.com/jstedfast/MailKit) instead of the
  unmaintained S22.Imap. Folders are opened read-only; each concurrent folder download
  uses its own IMAP connection (MailKit's `ImapClient` isn't safe to share across
  threads), and the TLS certificate validation callback is explicit and logged for the
  forensic audit trail.
- **Timeouts**: per-message download timeouts use a linked `CancellationTokenSource`
  instead of the previous `Thread.Abort()`-based helper, which is both unsafe and
  unavailable on modern .NET.
- **Concurrency**: hashes recorded during a download are kept in a
  `ConcurrentDictionary`, since multiple folder downloads write to it in parallel.
- **Search query builder**: the Scintilla-based query editor uses Scintilla's built-in
  autocomplete list instead of the unmaintained `AutoCompleteMenu-ScintillaNET`
  add-on, whose current package pulls in `Microsoft.Windows.Compatibility` and
  `System.Data.SqlClient` - both flagged with known vulnerabilities - just for a text
  popup.

## Known limitations

- Only IMAP authentication is covered by automated tests against a fake IMAP server;
  folder listing, search and download are not. The interactive OAuth sign-in itself needs
  a real account and is not tested automatically. The rest of the `Core` test suite covers
  the pure logic: query parsing, hashing, and `.eml`/timestamp handling.
- A connection dropped by the server in the middle of a folder is not re-established.
- Windows-only, like the original WinForms UI.
