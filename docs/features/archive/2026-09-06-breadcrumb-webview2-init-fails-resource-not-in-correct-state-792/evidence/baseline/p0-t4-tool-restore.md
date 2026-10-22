# [P0-T4] Manifest tool restore

- Issue: #792
- Timestamp: 2026-09-17T18-35
- Command: `dotnet tool restore` then `dotnet tool list` (manifest: repository-root `dotnet-tools.json`; run with the item worktree as the working directory)
- EXIT_CODE: 0
- Output Summary:
  - `dotnet tool restore` printed `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier` and `Restore was successful.`; exit 0.
  - `dotnet tool list` printed one row whose first column is `csharpier` and second column is `1.2.6` (third column `csharpier`); exit 0. The Manifest column is not transcribed because it carries the account name.
