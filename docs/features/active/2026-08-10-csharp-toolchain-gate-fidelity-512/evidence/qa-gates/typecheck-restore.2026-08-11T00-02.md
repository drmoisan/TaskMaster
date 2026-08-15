# TYPECHECK restoration build after the negative control ([P5-T7])

Timestamp: 2026-08-11T00-02
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /nologo /v:m /fl "/flp:logfile=coverage/qa-typecheck-restore.log;verbosity=normal"`
EXIT_CODE: 0

Invoked via `pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-typecheck-rebuild.ps1 -LogName qa-typecheck-restore`.

## Why this task is mandatory

The failing `/t:Rebuild` in [P5-T5] issued `Clean` to every project before the first `CoreCompile`,
so the negative control left every project's `bin`/`obj` deleted or incomplete. `spec.md` requires
this restoration to be an **ordered plan task**, not an implicit consequence: without it,
`vstest.console.exe` would find no assemblies. It also re-runs the positive control to confirm the
tree is green again after the revert.

## Measurements

| Metric | Value | Acceptance |
|---|---|---|
| `EXIT_CODE` | **0** | required 0 — PASS |
| MSBuild summary | **`0 Error(s)`** | required `0 Error(s)` — PASS |
| `Skipping target "CoreCompile"` count | **0** | required 0 — PASS |
| Node-prefixed `error CS` count | 0 | corroborates `0 Error(s)` |
| Elapsed | 15.7 s | recorded |
| MSBuild summary | `6 Warning(s)` | not gated |

The single `CS8603` produced by the perturbation is gone, confirming the [P5-T6] revert took effect
in the compiler's view of the tree and not merely in `git status`.

## Rebuilt-assembly confirmation

```
$ ls -la UtilitiesCS/bin/Debug/UtilitiesCS.dll
-rwxr-xr-x 1 DanMoisan 197121 17612288 Aug 10 23:13 UtilitiesCS/bin/Debug/UtilitiesCS.dll*

$ ls -d */bin/Debug | wc -l
18
```

`UtilitiesCS/bin/Debug` contains a rebuilt `UtilitiesCS.dll` with a current timestamp, and all 18
projects have a `bin/Debug` output directory.

## AC2 counting-mechanism deviation (restated)

The non-vacuity assertion is a **zero** count of `Skipping target "CoreCompile"`, substituted for
AC2's `csc.exe` parenthetical (zero at `verbosity=normal` even for genuine compiles). `CoreCompile:`
header lines are not counted. Recorded in `spec.md` § "The non-vacuity assertion mechanism".

## Output Summary

The restoration build returns `EXIT_CODE: 0` with `0 Error(s)` and a **zero** `CoreCompile` skip
count in 15.7 s, and `UtilitiesCS/bin/Debug` contains a rebuilt assembly. The three-run negative-path
proof (positive control -> negative control -> revert and restore) is complete and the tree is green
again.
