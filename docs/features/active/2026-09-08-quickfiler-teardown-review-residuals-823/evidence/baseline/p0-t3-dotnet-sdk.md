# Phase 0 — Repository-local .NET SDK resolution

Timestamp: 2026-09-09T13-46

Task: [P0-T3]

Command: `dotnet --version` (from the worktree root)
Command: `pwsh -NoProfile -File ./scripts/vscode/Install-RepoDotNetSdk.ps1` (from the worktree root)
Command: `dotnet --version` (re-probe)

EXIT_CODE: 0

SDK-VERSION: 8.0.205
BRANCH-TAKEN: installed

First probe failed with exit code -2147450725. `global.json` pins the SDK and names `.dotnet-sdk`
in its `paths` value, and this freshly created worktree carried no `.dotnet-sdk` directory, so the
host printed the `global.json` `errorMessage` instead of a version. The repository's own installer
script provisioned .NET SDK 8.0.205 into the gitignored `.dotnet-sdk` directory of this worktree,
and the re-probe printed `8.0.205` with exit code 0.

Output Summary: Probe-install-reprobe. Initial probe exit -2147450725 (repo-local SDK absent);
`Install-RepoDotNetSdk.ps1` downloaded and installed .NET SDK 8.0.205 into the gitignored
`.dotnet-sdk`; re-probe printed `8.0.205` at exit 0.
