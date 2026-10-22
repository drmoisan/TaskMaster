# Phase 0 — Toolchain bootstrap (P0-T4)

Task: [P0-T4]
Worktree: the item worktree for issue #743 (branch `bug/quickfiler-itemviewer-ui-marshalling-seam-743`, HEAD `514956570` at the time of this task). Every command below was run from the worktree root via `Set-Location` inside one `pwsh -NoProfile -Command` invocation, with the Command Reference tool resolution prepended where the plan span uses `$msbuild`. Each command was run while holding the shared machine build lock for item 743 (acquired immediately before, released immediately after).

Resolved tool path (vswhere, `-requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe'`):
`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`

## Block 1 — repo-local .NET SDK install

Timestamp: 2026-09-13T02-11
Command: `pwsh -File scripts\vscode\Install-RepoDotNetSdk.ps1`
EXIT_CODE: 0
Output Summary:
- Printed `Repo-local .NET SDK 8.0.205 is already installed at <worktree>\.dotnet-sdk.` (the worktree path is elided here per the evidence-hygiene rule).
- No download was performed; the SDK directory from the previous run is intact.

## Block 2 — dotnet local tool restore

Timestamp: 2026-09-13T02-11
Command: `pwsh -Command 'dotnet tool restore'`
EXIT_CODE: 0
Output Summary:
- `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier`
- `Restore was successful.`

## Block 3 — msbuild solution restore

Timestamp: 2026-09-13T02-11
Command: `pwsh -Command '& $msbuild TaskMaster.sln /t:Restore /m /p:Configuration=Debug "/p:Platform=Any CPU"'`
EXIT_CODE: 0
Output Summary:
- `MSBuild version 18.10.1-1.26427.6+3cd27c13e for .NET Framework`
- Restore target printed `Nothing to do. None of the projects specified contain packages to restore.`
- `Build succeeded.` with `0 Warning(s)` and `0 Error(s)`; Time Elapsed 00:00:01.00.
- Observation that triggered the fallback: after this command, `Test-Path packages` printed `False` and the worktree held no `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`. Every project in the solution is a legacy `packages.config` project, which `/t:Restore` does not handle, so the packages.config restore had not taken place even though the exit code was 0. Per the task text, the `nuget restore` fallback was run as Block 4.

## Block 4 — packages.config fallback restore

Timestamp: 2026-09-13T02-12
Command: `pwsh -Command 'nuget restore TaskMaster.sln'`
EXIT_CODE: 0
Output Summary:
- Feeds used: the local NuGet global-packages cache, `https://api.nuget.org/v3/index.json`, and the Visual Studio offline package fallback folder.
- `Installed: 172 package(s) to packages.config projects`
- After this command, `(Get-ChildItem packages -Directory).Count` printed `172`.
- This is the final `EXIT_CODE: 0` restore block the task requires.
