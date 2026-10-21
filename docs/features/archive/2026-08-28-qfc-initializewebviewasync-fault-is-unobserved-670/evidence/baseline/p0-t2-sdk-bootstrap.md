# P0-T2 — Repository-pinned .NET SDK bootstrap

Timestamp: 2026-09-01T19-39
Command: `pwsh -NoProfile -File .\scripts\vscode\Install-RepoDotNetSdk.ps1`, then `dotnet --version`, then `dotnet --list-sdks`
EXIT_CODE: 0

Output Summary:

The worktree carried no `.dotnet-sdk` directory before this task ran. The install script downloaded SDK 8.0.205 and reported it installed to the repo-local root `<repo-root>\.claude\worktrees\agent-<id>\.dotnet-sdk`.

`dotnet --version` prints:

    8.0.205

`dotnet --list-sdks` prints two entries. Every bracketed SDK root is recorded here in the placeholder form the plan's section 0 prescribes, rewritten as the listing was captured rather than in a later sweep:

    8.0.205 [<repo-root>\.claude\worktrees\agent-<id>\.dotnet-sdk\sdk]
    10.0.400 [<program-files>\dotnet\sdk]

The first entry is the repo-local root `global.json` pins through its `paths` entry `.dotnet-sdk`. The second is the machine-wide install reachable through the `$host$` resolution root that `global.json` also admits; it is recorded in placeholder form for the same obligation, because a repo-local-only rewrite would not have covered it.

Capture-time sanitisation gate: a case-insensitive fixed-string sweep of this artifact for the drive-qualified user-profile root and for the drive-qualified Program Files root, in each of the two separator spellings, returns zero. Sanitisation was performed at capture time rather than deferred, because P3-T15 commits this artifact in Phase 3 and the only later sweep that reaches it is P4-T28 in Phase 4; an unrewritten literal would otherwise land in an intermediate commit.
