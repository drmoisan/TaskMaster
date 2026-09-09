# Toolchain bootstrap (issue #826, [P0-T3])

Timestamp: 2026-09-09T19-01

All five commands were run from the repository root of the `-exec` worktree with the plan's C2 preamble
branch guard active. Absolute host paths in the tool output are replaced by `<repo-root>`.

## Command 1

Command: `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1`
EXIT_CODE: 0
Output Summary: downloaded .NET SDK 8.0.205 and installed it to `<repo-root>\.dotnet-sdk`. The worktree
had no `.dotnet-sdk` directory before this step, so `global.json` could not have been satisfied.

## Command 2

Command: `& (Join-Path $Root ".dotnet-sdk\dotnet.exe") tool restore`
EXIT_CODE: 0
Output Summary: `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier` /
`Restore was successful.`

## Command 3

Command: `& (Join-Path $Root ".dotnet-sdk\dotnet.exe") tool run csharpier --version`
EXIT_CODE: 0
Output Summary: `1.2.6` — the manifest-pinned version required by CLAUDE.md §C#1.1.

## Command 4

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1`
EXIT_CODE: 0
Output Summary: MSBuild 18.9.1 restored the solution; `Installed: 172 package(s) to packages.config
projects`; `Build succeeded. 0 Warning(s) 0 Error(s)`.

## Command 5

Command: `dotnet-coverage --version`
EXIT_CODE: 0
Output Summary: `18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342`.

Output Summary: all five bootstrap commands exited 0 and the reported CSharpier version is 1.2.6, so the
acceptance condition for [P0-T3] holds.

---

Timestamp correction, recorded rather than absorbed: the `Timestamp:` value in this artifact was initially written as an extrapolated value rather than read from the clock. At 2026-09-09T19-10 the executor read the real clock, observed the discrepancy, and replaced the value with this artifact's own observed filesystem write time. The corrected value is an observation; the superseded value was not.
