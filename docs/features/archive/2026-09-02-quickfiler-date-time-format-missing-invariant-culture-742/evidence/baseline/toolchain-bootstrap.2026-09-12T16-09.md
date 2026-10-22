# Toolchain Bootstrap (issue #742, [P0-T3])

Timestamp: 2026-09-14T01-57

Command:

1. `pwsh -NoProfile -Command 'nuget restore TaskMaster.sln'`
2. `pwsh -NoProfile -Command 'dotnet tool restore'`
3. `pwsh -NoProfile -Command 'if (Get-Command dotnet-coverage -ErrorAction SilentlyContinue) { Write-Output "DOTNET_COVERAGE_PRESENT" } else { dotnet tool install --global dotnet-coverage; Write-Output "DOTNET_COVERAGE_INSTALLED" }'`

EXIT_CODE:

1. `nuget restore TaskMaster.sln` — 0
2. `dotnet tool restore` — 0 (second attempt; see the prerequisite note below)
3. `dotnet-coverage` availability probe — 0

Output Summary:

1. `nuget restore` reported `Installed: 172 package(s) to packages.config projects` using
   MSBuild 18.10.1.42706 from the VS 18 Community install. Packages were written to
   `<worktree>\packages`.
2. `dotnet tool restore` reported `Tool 'csharpier' (version '1.2.6') was restored.` followed by
   `Restore was successful.`
3. The probe printed exactly `DOTNET_COVERAGE_PRESENT`, so `dotnet-coverage` was already on PATH
   and no global tool install was performed.

## Prerequisite note (recorded for audit fidelity)

The first `dotnet tool restore` attempt in this fresh worktree failed with
`The repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the
repository root`, exit code `-2147450725`. The remedy named by the tool's own error message was
executed as a mechanically necessary micro-action:

`pwsh -NoProfile -Command './scripts/vscode/Install-RepoDotNetSdk.ps1'`

which reported `Installed repo-local .NET SDK 8.0.205 to <worktree>\.dotnet-sdk.` and exited 0.
`dotnet tool restore` was then re-run and exited 0, as recorded above. `.dotnet-sdk` and `packages`
are build inputs restored per worktree and are not tracked files; neither appears in this change's
Write Set.

## Build-lock discipline

`nuget restore`, `dotnet tool restore` and the `dotnet-coverage` probe were run with the shared
parallel build lock held for the build-affecting command and released immediately after. The SDK
install ran with the lock released, because it writes only to this worktree's private
`.dotnet-sdk` directory and does not contend on shared build outputs.
