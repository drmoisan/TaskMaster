# [P0-T1] Toolchain Bootstrap — Baseline Evidence

- **Issue:** #424
- **Task:** [P0-T1]
- **Repo root:** `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-04T18-38`
- **Branch:** `bug/quickfiler-high-confidence-queue-init-stall-424`

Timestamp: 2026-08-06T22-13

## Step 1 — Repo-local .NET SDK

Command: `pwsh -NoProfile -Command "& ./scripts/vscode/Install-RepoDotNetSdk.ps1"`
EXIT_CODE: 0

Output Summary: `Repo-local .NET SDK 8.0.205 is already installed at C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-04T18-38\.dotnet-sdk.` No download or install action was required; the script returned without error.

## Step 2 — Local dotnet tool manifest restore

Command: `./.dotnet-sdk/dotnet.exe tool restore`
EXIT_CODE: 0

Output Summary: `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier` / `Restore was successful.` Manifest resolved from repo-root `dotnet-tools.json` (this repo keeps the manifest at the root rather than under `.config/`).

## Step 3 — NuGet packages.config restore

Command: `pwsh -NoProfile -Command "& ./scripts/vscode/Invoke-Restore.ps1"`
EXIT_CODE: 0

Output Summary: `Done Building Project "TaskMaster.sln" (Restore target(s)).` / `Build succeeded. 0 Warning(s) 0 Error(s)` / `Time Elapsed 00:00:01.12`.

## Step 4 — csharpier version check

Command: `./.dotnet-sdk/dotnet.exe tool run csharpier --version`
EXIT_CODE: 0

Output Summary: `1.2.6`

## Step 5 — dotnet-coverage version check

Command: `dotnet-coverage --version`
EXIT_CODE: 0

Output Summary: `18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3` (resolved from the global tools path `C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe`).

## Aggregate

Command: (5 commands above)
EXIT_CODE: 0

Output Summary: All five bootstrap commands returned EXIT_CODE 0. Both required tool versions resolve:
- **csharpier 1.2.6** — v1 CLI, so the runnable forms are `csharpier format .` / `csharpier check .` per Decisions Record item 11.
- **dotnet-coverage 18.5.2** — available on PATH for the `[P0-T7]` / `[P6-T4]` coverage runs.
