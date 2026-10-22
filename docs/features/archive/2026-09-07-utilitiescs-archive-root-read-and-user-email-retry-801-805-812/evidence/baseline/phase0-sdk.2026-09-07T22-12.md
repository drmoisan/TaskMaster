# Phase 0 — Repository-Pinned .NET SDK Provisioning (P0-T2)

Timestamp: 2026-09-08T06-31

Command: `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1` (run with the current directory set to the worktree root)

EXIT_CODE: 0

Output Summary: The script downloaded .NET SDK 8.0.205 and installed it into the repository-local `.dotnet-sdk` directory of this worktree. Verification commands run afterwards from the worktree root:

- `dotnet --version` prints `8.0.205`.
- `dotnet --list-sdks` reports one entry, version `8.0.205`, whose path ends with `.dotnet-sdk\sdk`. The absolute prefix of that path is deliberately not transcribed here because it carries the account name.

`global.json` pins `sdk.version` to `8.0.205` with `paths: [".dotnet-sdk", "$host$"]`, so a plain `dotnet` invocation made with the current directory set to the worktree root now resolves to the repository-local SDK without any PATH modification; this was confirmed by running `dotnet --version` without prepending `.dotnet-sdk` to PATH and observing `8.0.205`. `.gitignore` ignores `.dotnet*/`, so the provisioned tree does not dirty the worktree.
