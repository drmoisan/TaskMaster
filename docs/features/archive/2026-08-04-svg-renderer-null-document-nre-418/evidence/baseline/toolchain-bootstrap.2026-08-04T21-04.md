# [P0-T1] Toolchain Bootstrap — Baseline (re-capture on VSTO-enabled host)

Timestamp: 2026-08-04T21-04

Issue: #418
Plan: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md`
Task: `[P0-T1]`
Branch: `bug/svg-renderer-null-document-nre-418`
HEAD: `a5695656e711f98a8ae6ad334115c0f8666c509f`
Base: `ce0c91e6` (PR #419, repository-wide NuGet package update) — merge-base with HEAD confirmed as `ce0c91e6`
Repository root: `c:\Users\DanMoisan\repos\TaskMaster`

## Why this artifact exists alongside `toolchain-bootstrap.2026-08-04T14-36.md`

The `2026-08-04T14-36` artifact set was captured on a different host that lacked
`Microsoft.Office.Tools.Outlook.v4.0.Utilities` and `Microsoft.Office.Tools.Common.v4.0.Utilities`,
and was captured before this branch was rebased onto `ce0c91e6`. That set is preserved on disk
for audit and is not modified. This `2026-08-04T21-04` set is the valid comparison basis for
Phase 1 and Phase 2 on this host.

Host VSTO precondition verified before running any command:

```
C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.0\
  Microsoft.Office.Tools.Common.v4.0.Utilities.dll   PRESENT
  Microsoft.Office.Tools.Outlook.v4.0.Utilities.dll  PRESENT
  Microsoft.Office.Tools.Common.dll                  PRESENT
  Microsoft.Office.Tools.Outlook.dll                 PRESENT
  Microsoft.Office.Tools.v4.0.Framework.dll          PRESENT
```

## Command

Command (1 of 3):
```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Install-RepoDotNetSdk.ps1
```
EXIT_CODE: 0

Command (2 of 3):
```
dotnet tool restore
```
EXIT_CODE: 0

Command (3 of 3):
```
dotnet tool install --global dotnet-coverage
```
EXIT_CODE: 0 (SKIPPED under the task's explicit authorized skip condition — see below)

Verification commands:
```
dotnet tool run csharpier --version
dotnet-coverage --version
pwsh -NoProfile -Command "if (Get-Command dotnet-coverage -ErrorAction SilentlyContinue) { 'dotnet-coverage RESOLVES' } else { 'dotnet-coverage MISSING' }"
```
EXIT_CODE: 0 / 0 / 0

## Output Summary

- `.dotnet-sdk/` exists at `c:\Users\DanMoisan\repos\TaskMaster\.dotnet-sdk`. `Install-RepoDotNetSdk.ps1`
  reported `Repo-local .NET SDK 8.0.205 is already installed at C:\Users\DanMoisan\repos\TaskMaster\.dotnet-sdk`
  and exited 0. The script is idempotent; no reinstall occurred.
- `dotnet tool restore` reported `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier`
  followed by `Restore was successful.`, and exited 0.
- `dotnet tool run csharpier --version` prints `1.2.6` — matches the `dotnet-tools.json` manifest pin.
- `dotnet-coverage --version` resolves and prints `18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3`
  from `C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage`.
- `Get-Command dotnet-coverage` resolves, so the third command (`dotnet tool install --global dotnet-coverage`)
  was skipped exactly as the task text authorizes: "skip the install only if `Get-Command dotnet-coverage`
  already resolves". This is a task-text-authorized skip branch, not a policy waiver.

Bootstrap result: all three toolchain preconditions satisfied on this host. Tasks P0-T6, P0-T9,
P2-T1, P2-T2, and P2-T6 can execute.
