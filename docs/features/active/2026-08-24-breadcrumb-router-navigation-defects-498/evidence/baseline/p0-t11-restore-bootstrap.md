# P0-T11 — Execution Worktree Toolchain Bootstrap

Timestamp: 2026-08-26T08-35

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Install-RepoDotNetSdk.ps1"; dotnet tool restore; if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }; & "scripts\vscode\Invoke-Restore.ps1" -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

Pre-state confirmed before the command ran: this worktree had NEITHER `.dotnet-sdk/` NOR `packages/`.
Both were reported absent by `ls -d .dotnet-sdk packages` ("No such file or directory" for each).
`global.json` pins `sdk.version` `8.0.205` with `rollForward: latestFeature` and
`paths: [".dotnet-sdk", "$host$"]`, so every `dotnet` invocation would have failed on the `global.json`
`errorMessage` until the repo-local SDK was installed. The installer ran under pwsh **7.6.5**, not Windows
PowerShell 5.1.

All four required outcomes, each measured after the command completed:

| Outcome | Verification command | Observed |
|---|---|---|
| 1. Repo-local .NET SDK installed and resolving | `dotnet --version` from the workspace root | `8.0.205` — matches the `global.json` pin exactly. `.dotnet-sdk/sdk` contains one directory, `8.0.205`. |
| 2. CSharpier restored by `dotnet tool restore` | `dotnet tool run csharpier --version` | `1.2.6` — matches the `dotnet-tools.json` manifest pin (`"isRoot": true`, manifest at the repository root, not under `.config/`). |
| 3. `dotnet-coverage` resolves on PATH | `Get-Command dotnet-coverage` | Resolves to the per-user global tool path `<user-profile>\.dotnet\tools\dotnet-coverage.exe`. It was already present, so the conditional `dotnet tool install --global dotnet-coverage` branch did not execute. `scripts/vscode/Invoke-MSTestWithCoverage.ps1:292-293` will therefore not throw `dotnet-coverage not found` in `P0-T15`. |
| 4. NuGet restore completed | `Invoke-Restore.ps1` output; `packages/` directory count | `Installed: 172 package(s) to packages.config projects`; `Build succeeded. 0 Warning(s) 0 Error(s)`; `Time Elapsed 00:00:02.39`. The `packages/` directory now holds 172 package directories. |

No `error CS0006` and no analyzer package version skew was reported by the restore. Restore feeds used were
the local NuGet cache, `https://api.nuget.org/v3/index.json`, and the Visual Studio fallback package folder.

Absolute host paths in the raw tool output are redacted here to the `<user-profile>` placeholder.
