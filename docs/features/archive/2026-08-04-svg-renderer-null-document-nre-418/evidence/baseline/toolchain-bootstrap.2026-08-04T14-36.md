# Toolchain Bootstrap — Baseline (Issue #418)

Task: `[P0-T1]`
Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`

Timestamp: 2026-08-04T14-52

## Preconditions Observed (before bootstrap)

- `.dotnet-sdk/` — absent (`ls: cannot access '.dotnet-sdk': No such file or directory`)
- `dotnet-coverage` — not resolvable (`which: no dotnet-coverage in (...)`)
- `pwsh` — `7.6.4` (PowerShell 7 required; the installer uses
  `System.Net.Http.HttpCompletionOption`, which is unavailable in Windows PowerShell 5.1)

All three bootstrap commands therefore ran; none was skipped.

---

## Command 1 — Install the repo-local .NET SDK

Timestamp: 2026-08-04T14-45

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Install-RepoDotNetSdk.ps1`

Working directory: repository root (`c:\Users\DanMoisan\source\repos\drmoisan\TaskMaster`)

EXIT_CODE: 0

Output Summary: Downloaded SDK `8.0.205` from
`https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip`
and extracted it. Script reported
`Installed repo-local .NET SDK 8.0.205 to C:\Users\DanMoisan\source\repos\drmoisan\TaskMaster\.dotnet-sdk.`
The version marker directory `.dotnet-sdk/sdk/8.0.205` exists on disk, satisfying the
`global.json` pin (`sdk.version = 8.0.205`, `paths = [".dotnet-sdk", "$host$"]`).

---

## Command 2 — Restore the local tool manifest

Timestamp: 2026-08-04T14-49

Command: `dotnet tool restore`

Working directory: repository root, with `DOTNET_ROOT` and `PATH` pointed at `.dotnet-sdk`

EXIT_CODE: 0

Output Summary: First-run SDK banner emitted, then
`Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier`
followed by `Restore was successful.` The manifest at the repo-root
`dotnet-tools.json` pins csharpier `1.2.6`; `Install-RepoDotNetSdk.ps1` does not
perform this restore, so it was required as a separate step.

---

## Command 3 — Install `dotnet-coverage` as a global tool

Timestamp: 2026-08-04T14-51

Command: `dotnet tool install --global dotnet-coverage`

EXIT_CODE: 0

Output Summary: `Tool 'dotnet-coverage' (version '18.9.0') was successfully installed.`
Installed to `~/.dotnet/tools`, which is already on `PATH`. The task text authorizes
skipping this command only when `dotnet-coverage` already resolves; it did not resolve in
this checkout, so the command ran and its exit code is a real `0`.

---

## Post-Bootstrap Verification

| Check | Command | Result |
| --- | --- | --- |
| Repo-local SDK present | `ls -d .dotnet-sdk/sdk/8.0.205` | `.dotnet-sdk/sdk/8.0.205` (exists) |
| csharpier version | `dotnet tool run csharpier --version` | `1.2.6` |
| dotnet-coverage resolves | `dotnet-coverage --version` | `18.9.0+5e1b5d91e7399de7c03f20609606da8996ac3539` |

Output Summary (task-level): All three bootstrap commands exited `0`. `.dotnet-sdk/` now
exists with the pinned `8.0.205` SDK, `dotnet tool run csharpier --version` prints `1.2.6`,
and `dotnet-coverage --version` resolves to `18.9.0`. The environment precondition
documented in the plan is removed; tasks `[P0-T6]`, `[P0-T9]`, `[P2-T1]`, `[P2-T2]`, and
`[P2-T6]` can now run.

## Invocation Note for Downstream Tasks

`dotnet` is routed through the repo-local SDK. Downstream command steps export:

```text
DOTNET_ROOT=<repo-root>/.dotnet-sdk
PATH=<repo-root>/.dotnet-sdk:$PATH
```

before invoking `dotnet tool run csharpier ...`.
