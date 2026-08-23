# P0-T1 — C# Toolchain Bootstrap

Issue: #230
Task: [P0-T1]

## Step 1 — Repo-local .NET SDK install

- Timestamp: 2026-08-07T21-30
- Command: `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1`
- EXIT_CODE: 0
- Output Summary: Downloaded .NET SDK 8.0.205 from
  `https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip`
  and installed it to
  `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-07T20-30\.dotnet-sdk`.
  Before this step `.dotnet-sdk` did not exist in this fresh worktree, so
  `global.json` (`sdk.version 8.0.205`, `paths: [".dotnet-sdk", "$host$"]`)
  could not be satisfied by the globally installed SDK.

## Step 2 — dotnet tool restore

- Timestamp: 2026-08-07T21-30
- Command: `./.dotnet-sdk/dotnet.exe tool restore`
- EXIT_CODE: 0
- Output Summary: `Tool 'csharpier' (version '1.2.6') was restored. Available
  commands: csharpier` / `Restore was successful.` The tool manifest is
  `dotnet-tools.json` at repository root (legacy location, not `.config/`);
  restore resolved it correctly.

## Step 3 — csharpier version (D2 verification)

- Timestamp: 2026-08-07T21-30
- Command: `./.dotnet-sdk/dotnet.exe tool run csharpier --version`
- EXIT_CODE: 0
- Output Summary: `1.2.6` — matches the D2 pin. Subcommand CLI
  (`format | check | pipe-files | server`) applies; the v0 `csharpier .` form is
  not used anywhere in this plan.

### D2 fallback status

The explicit `./.dotnet-sdk/dotnet.exe` form was used for Step 2 and Step 3
because the repo-local SDK had just been installed in the same session. After
installation, the plain form was also verified to work:

- Command: `dotnet tool run csharpier --version`
- EXIT_CODE: 0
- Output Summary: `1.2.6`

Both the plain `dotnet` form and the `./.dotnet-sdk/dotnet.exe` fallback form
resolve identically in this worktree. Remaining plan tasks use the plain
`dotnet tool run csharpier ...` form.

## Step 4 — dotnet-coverage global tool

- Timestamp: 2026-08-07T21-30
- Command: `dotnet-coverage --version`
- EXIT_CODE: 0
- Output Summary: `18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3`. Already
  present on PATH (`C:\Users\DanMoisan\.dotnet\tools`); the conditional
  `dotnet tool install --global dotnet-coverage` was therefore not required and
  was not run.

## Step 5 — NuGet restore

- Timestamp: 2026-08-07T21-30
- Command: `pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1`
- EXIT_CODE: 0
- Output Summary: `Installed: 171 package(s) to packages.config projects`;
  `Build succeeded. 0 Warning(s) 0 Error(s)`; elapsed 00:00:03.72.

## Result

All five bootstrap commands exited 0. csharpier 1.2.6 and dotnet-coverage
18.5.2 both resolve. Toolchain is ready for the Phase 0 baseline captures.
