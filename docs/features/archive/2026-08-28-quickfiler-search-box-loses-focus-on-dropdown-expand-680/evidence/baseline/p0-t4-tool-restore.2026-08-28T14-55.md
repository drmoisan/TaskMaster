# P0-T4 — Manifest Tool Restore (Issue #680)

Timestamp: 2026-08-28T14-55

Command: `dotnet tool restore` (from the worktree root), followed by `dotnet tool run csharpier --version`

EXIT_CODE: 0

Output Summary:

- `dotnet tool restore` printed `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier`
  and `Restore was successful.` Exit code 0.
- `dotnet tool run csharpier --version` printed `1.2.6`, matching the version pinned by
  `.config/dotnet-tools.json`. Exit code 0.

Acceptance: satisfied — restore exited 0 and the manifest-pinned CSharpier version is 1.2.6.
