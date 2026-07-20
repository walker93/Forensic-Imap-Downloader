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
2. Enter the IMAP server, port, username and password, then **Connect**.
3. Optionally narrow down what gets downloaded with **Filter** (build an
   `AND(...)`/`OR(...)`/`NOT(...)` query against fields like `Subject`, `From`,
   `Sent After`, `Seen`, etc.).
4. Pick which folders to include in the grid, then **Download**.
5. **Abort** during a download lets you discard, keep (for a later resume), or
   archive-and-hash what has been downloaded so far.

## Project layout

- `ForensicIMAPDownloader.Core` - platform-independent library: IMAP connection/search/
  download handling (`ImapDownloadService`), the search query parser
  (`SearchQueryParser`), the zip/hash archiving and verification logic
  (`MailArchiveService`), and file hashing (`HashCalculator`). No WinForms dependency,
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

- The IMAP/MailKit integration is not covered by automated tests (would require a real
  or fake IMAP server); the `Core` test suite covers the pure logic: query parsing,
  hashing, and `.eml`/timestamp handling.
- Windows-only, like the original WinForms UI.
