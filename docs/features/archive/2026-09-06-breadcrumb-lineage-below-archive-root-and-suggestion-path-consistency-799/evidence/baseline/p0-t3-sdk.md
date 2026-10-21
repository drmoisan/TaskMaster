# [P0-T3] Repository-local .NET SDK bootstrap

Timestamp: 2026-09-07T06-45

Command: pwsh -NoProfile -File scripts\vscode\Install-RepoDotNetSdk.ps1 ; then
$env:DOTNET_ROOT = (Resolve-Path '.dotnet-sdk').Path ; $env:PATH = "$env:DOTNET_ROOT;$env:PATH" ;
dotnet --version ; Test-Path '.dotnet-sdk\sdk\8.0.205'

EXIT_CODE: 0

EXEC-ENVIRONMENT: pwsh-permitted

## Before / after existence

- BEFORE: `.dotnet-sdk` exists = False
- BEFORE: `.dotnet-sdk\sdk\8.0.205` exists = False
- AFTER: `.dotnet-sdk` exists = True
- AFTER: `.dotnet-sdk\sdk\8.0.205` exists = True

## Printed version

`dotnet --version` printed `8.0.205` with exit code 0.

Output Summary: The worktree had no repository-local SDK tree before this task, consistent with the Phase 0
preamble. The install script downloaded and extracted SDK 8.0.205 into `<repo-root>\.dotnet-sdk` and reported
`Installed repo-local .NET SDK 8.0.205`. After the install, the `global.json`-pinned marker directory
`.dotnet-sdk\sdk\8.0.205` exists and `dotnet --version` prints `8.0.205`, a version beginning `8.0.`, so the pin is
satisfied. The derived line `EXEC-ENVIRONMENT: pwsh-permitted` is recorded per plan rule R11b: this executor
session is not worktree-isolated and PowerShell command blocks ran normally, including this one, so no task in
this plan is blocked by the R11b constraint. Host paths in this artifact are reduced per R3.
