# P0-T3 — Repo-pinned .NET SDK Provisioning (Issue #680)

Timestamp: 2026-08-28T14-55

Command: `pwsh -NoProfile -File .\scripts\vscode\Install-RepoDotNetSdk.ps1` (run from the worktree
root), followed by `dotnet --version` and `dotnet --list-sdks` from the same directory.

EXIT_CODE: 0

Output Summary:

- Provisioning script output: `Repo-local .NET SDK 8.0.205 is already installed at <repo-root>\.dotnet-sdk.`
  The script is idempotent and early-returned; no download was performed.
- `dotnet --version` printed `8.0.205` — an SDK version, not the `global.json` resolution error.
- `dotnet --list-sdks` output includes the entry
  `8.0.205 [<repo-root>\.dotnet-sdk\sdk]`, so a path segment ending `.dotnet-sdk\sdk` is present.
  The machine-wide `10.0.400 [<program-files>\dotnet\sdk]` entry is also listed; `global.json`
  pins resolution to 8.0.205.

Acceptance: satisfied — an SDK version is printed and the `.dotnet-sdk\sdk` path segment is listed.
