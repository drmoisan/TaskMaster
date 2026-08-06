# Toolchain Bootstrap — Remediation Cycle 1

- Task: `[P0-T1]`
- Issue: #418
- Branch / HEAD: `bug/svg-renderer-null-document-nre-418` @ `ea106111`
- Evidence series: `2026-08-05T01-50`

Timestamp: 2026-08-05T01-22 (UTC)

## Step 1 — repo-local .NET SDK

Command: `ls -d .dotnet-sdk`
EXIT_CODE: 0
Output Summary: `.dotnet-sdk/` is present in this checkout, so
`scripts/vscode/Install-RepoDotNetSdk.ps1` was **not** required and was not run
(the task text conditions it on `.dotnet-sdk/` being absent).

## Step 2 — dotnet tool restore

Command: `dotnet tool restore`
EXIT_CODE: 0
Output Summary: `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier` /
`Restore was successful.`

## Step 3 — csharpier availability

Command: `dotnet tool run csharpier --version`
EXIT_CODE: 0
Output Summary: resolved version `1.2.6`

## Step 4 — dotnet-coverage availability

Command: `dotnet-coverage --version`
EXIT_CODE: 0
Output Summary: resolved version `18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3`.
Already present on `PATH`, so `dotnet tool install --global dotnet-coverage` was
**not** required and was not run (the task text conditions it on
`dotnet-coverage --version` failing).

## Acceptance

Both required probes returned exit 0:

| Probe | Resolved version | EXIT_CODE |
|---|---|---|
| `dotnet tool run csharpier --version` | `1.2.6` | 0 |
| `dotnet-coverage --version` | `18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3` | 0 |

The csharpier tasks (`[P0-T6]`, `[P2-T1]`, `[P2-T2]`) and the coverage tasks
(`[P0-T9]`, `[P1-T19]`, `[P2-T6]`) are unblocked.
