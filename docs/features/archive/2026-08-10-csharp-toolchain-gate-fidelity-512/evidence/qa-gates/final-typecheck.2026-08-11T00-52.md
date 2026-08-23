# Final QC step 3 (C#) — TYPECHECK ([P6-T8])

Timestamp: 2026-08-11T00-52
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /nologo /v:m /fl "/flp:logfile=coverage/final-typecheck.log;verbosity=normal"`
EXIT_CODE: 0

`MSBUILD` = `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe`,
invoked via `pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-typecheck-rebuild.ps1 -LogName final-typecheck`.

## Measurements

| Metric | Value | Acceptance |
|---|---|---|
| `EXIT_CODE` | **0** | required 0 — PASS |
| MSBuild summary | **`0 Error(s)`** | required `0 Error(s)` — PASS |
| `Skipping target "CoreCompile"` count | **0** | required 0 — PASS |
| Node-prefixed `error CS` count | 0 | corroborates `0 Error(s)` |
| Elapsed | 14.2 s | recorded |
| MSBuild summary | `6 Warning(s)` | not gated |
| `CoreCompile:` header-line count | 66 | informational only, not the assertion |

## AC2 counting-mechanism deviation (restated)

The non-vacuity assertion is a **zero** count of `Skipping target "CoreCompile"` in the `/fl` log,
substituted for AC2's `csc.exe` parenthetical, which measures zero at `verbosity=normal` even for
genuine compiles. `CoreCompile:` header lines are not counted; they print even when the target is
skipped, and their count varies between otherwise equivalent full rebuilds (73 / 47 / 61 / 66 across
this feature's rebuilds), which is precisely why they are unfit as an assertion. Recorded in
`spec.md` § "The non-vacuity assertion mechanism".

## Cross-run consistency

| Run | EXIT | Elapsed | Skip count | Errors |
|---|---|---|---|---|
| [P0-T14] pre-change positive control | 0 | 18.4 s | 0 | 0 |
| [P5-T4] positive control | 0 | 15.0 s | 0 | 0 |
| [P5-T5] negative control (perturbed) | **1** | 3.4 s | 0 | **1 (CS8603)** |
| [P5-T7] restoration | 0 | 15.7 s | 0 | 0 |
| [P6-T8] final | **0** | 14.2 s | **0** | **0** |

## Output Summary

The corrected type-check command passes the final QC pass with `EXIT_CODE: 0`, `0 Error(s)` and a
**zero** `Skipping target "CoreCompile"` count in 14.2 s. The gate compiles genuinely, is passable on
the delivered tree, and was proven to fail on a real nullable violation at [P5-T5]. Build outputs for
all projects are current for any subsequent test step.
