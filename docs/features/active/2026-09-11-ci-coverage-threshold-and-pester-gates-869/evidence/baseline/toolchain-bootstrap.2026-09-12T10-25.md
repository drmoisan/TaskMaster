# Phase 0 — Worktree toolchain bootstrap (P0-T5)

Timestamp: 2026-09-14T17-55

Every command in this task was issued through `pwsh` with the worktree prologue, because the Bash allowlist grants no bare `dotnet`, `nuget` or `msbuild` entry and denies each chained segment independently.

## Prerequisite — repo-local .NET SDK

Command: `pwsh -NoProfile -Command '<worktree prologue>; Test-Path -LiteralPath ".dotnet-sdk"'`
EXIT_CODE: 0
Output: `True`

The directory is present, so no install was run. Acceptance for this prerequisite tolerates the already-present case as success, and this is that case. Neither failing outcome occurred: the directory is not absent, and no install command was run that could have returned a non-zero exit code.

The four required facts:

1. `global.json` at the repository root pins the SDK version `8.0.205` with `rollForward` set to `latestFeature` and `paths` set to the two entries `.dotnet-sdk` and `$host$`, and carries an `errorMessage` naming `scripts/vscode/Install-RepoDotNetSdk.ps1`. Read directly from that file in this pass: the `version`, `rollForward`, `allowPrerelease`, `paths` and `errorMessage` members occupy lines 3 through 10. `latestFeature` does not roll forward across major versions, so a host SDK of a different major version does not satisfy the pin.
2. A missing repo-local SDK makes `dotnet tool restore` fail with that `errorMessage` rather than proceeding. This fact is orchestrator-measured rather than executor-measured: the orchestrator ran `dotnet tool restore` in this worktree before the install and observed that failure, and observed that after the install it exits 0 and restores csharpier 1.2.6. The executor records it as a relayed observation and did not reproduce it, because doing so would require removing an established prerequisite.
3. The SDK directory is ignored by `.gitignore` at its line 350, whose pattern is `.dotnet*/`. Re-derived in this pass by reading that line. It can therefore appear in no porcelain output and in no write-set assertion, and it disturbs neither P0-T3's scoped observation nor P10-T13's four-root assertion nor P10-T14's unscoped assertion. P0-T3's unscoped observation, recorded in this run, contains no `.dotnet-sdk` entry, which corroborates this.
4. The orchestrator already performed this install for the current run, so the check was expected to print `True` and the install step was expected to be a no-op. Both expectations held.

## Command — dotnet tool restore

Command: `pwsh -NoProfile -Command '<worktree prologue>; dotnet tool restore'`
EXIT_CODE: 0
Output:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

The prologue is load-bearing for this command beyond the general rule: the tool manifest this repository uses is `dotnet-tools.json` at the repository root and no `.config` manifest exists, and `dotnet tool restore` resolves the manifest by searching from the working directory upward. Without the prologue it would search upward from the coordinator session worktree and restore into that tree instead of this one. The same reasoning applies to the two csharpier invocations in P0-T17 and P10-T7.

## Command — nuget restore TaskMaster.sln

Command: `pwsh -NoProfile -Command '<worktree prologue>; nuget restore TaskMaster.sln'`
EXIT_CODE: 0
Output Summary: `All packages listed in packages.config are already installed.` NuGet auto-detected MSBuild 18.10.1.42706 and completed the vulnerability-index fetch without error.

## Command — Get-Command dotnet-coverage

Command: `pwsh -NoProfile -Command '<worktree prologue>; Get-Command dotnet-coverage'`
EXIT_CODE: 0
Resolved command name: `dotnet-coverage.exe`
Resolved command path: `<user-profile>\.dotnet\tools\dotnet-coverage.exe`

The lookup succeeded on the first attempt, so the fallback branch prescribing `dotnet tool install --global dotnet-coverage` was not taken. `Get-Command` is a cmdlet rather than a native process, so `$LASTEXITCODE` is not set by it; the zero exit code recorded here is the pwsh invocation's own exit code, and the success signal is the resolved path printed with no terminating error.

## Command — Get-Command msbuild

Command: `pwsh -NoProfile -Command '<worktree prologue>; Get-Command msbuild'`
EXIT_CODE: 0
Resolved command name: `MSBuild.exe`
Resolved command path: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`

The lookup succeeded, so the vswhere fallback branch was not taken and task P0-T6 uses the bare command name `msbuild` inside its pwsh payload rather than an absolute path. The same remark about `$LASTEXITCODE` applies.

## dotnet-coverage version strings, from two named sources

PinnableVersion: 18.10.0

Read from the Version column of the `dotnet-coverage` row printed by `pwsh -NoProfile -Command '<worktree prologue>; dotnet tool list --global'`. The full table printed:

```
Package Id           Version      Commands
-------------------------------------------------
csharpier            1.3.0        csharpier
dotnet-coverage      18.10.0      dotnet-coverage
vpk                  1.0.1        vpk
```

This is the authoritative pinnable value and is the literal P7-T1 pins in the workflow file. No version literal in this plan is executor-chosen.

InformationalVersion: 18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342

Read from `pwsh -NoProfile -Command '<worktree prologue>; dotnet-coverage --version'`. This row is explicitly informational and is never pinned. The `+` suffix is build metadata, build metadata is not part of NuGet package identity, and a workflow step installing that string with `--version` does not resolve the package, so pinning the informational string is prohibited and would make P7-T1 unsatisfiable by any correct workflow.

Relationship between the two strings: they differ only by the build-metadata suffix. All three numeric components are identical at `18.10.0`. That is the expected relationship and is recorded as expected rather than as a discrepancy. No discrepancy exists on this run.

The `--version` switch was supported, so the fallback wording for an unsupported switch does not apply.

Incidental observation, recorded because it bears on every csharpier invocation in this plan: the globally installed csharpier is 1.3.0, while the manifest restore above pinned 1.2.6. Every csharpier invocation in this plan is made through `dotnet tool run csharpier`, which resolves the manifest-pinned 1.2.6, per the C# code change policy in CLAUDE.md.

Output Summary: all four commands recorded `EXIT_CODE: 0`. The repo-local SDK prerequisite was already satisfied and no install was run. `dotnet-coverage` and `msbuild` both resolved on the first lookup, so neither fallback branch was taken. The pinnable dotnet-coverage version is `18.10.0`; the informational string carries a build-metadata suffix and is not pinned.
