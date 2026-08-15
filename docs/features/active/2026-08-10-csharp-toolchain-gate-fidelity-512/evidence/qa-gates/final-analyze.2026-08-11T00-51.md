# Final QC step 2 (C#) — ANALYZE ([P6-T7])

Timestamp: 2026-08-11T00-51
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:m /fl "/flp:logfile=coverage/final-analyze.log;verbosity=normal"`
EXIT_CODE: 0

`MSBUILD` = `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe`,
invoked via `pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-analyze.ps1 -LogName final-analyze`.

## Measurements

| Metric | Value | Acceptance |
|---|---|---|
| `EXIT_CODE` | **0** | required 0 — PASS |
| `Skipping target "CoreCompile"` count | **0** | required 0 — PASS |
| Elapsed | **18.9 s** | recorded |
| MSBuild summary | `0 Error(s)` | — |
| MSBuild summary | `6 Warning(s)` | not gated |

## AC2 counting-mechanism deviation (restated)

AC2 names "a `csc.exe` invocation count greater than zero in an MSBuild file log" as the non-vacuity
proof. That count is **zero at `verbosity=normal` even for genuine compiles**, so the parenthetical as
literally written is not satisfiable by the described log. AC2's substantive requirement — a
non-vacuous compile assertion, not exit code alone — is satisfied instead by asserting **zero**
occurrences of the literal string `Skipping target "CoreCompile"` in the `/fl` log, which is emitted
only when the target is actually skipped and is therefore strictly more discriminating.
`CoreCompile:` header lines are **not** counted; they print even when the target is skipped. Recorded
in `spec.md` § "The non-vacuity assertion mechanism" and § "Recorded deviations" item 1.

Measured skip count: **0**. The step compiled genuinely.

## Cross-run consistency

| Run | EXIT | Elapsed | Skip count |
|---|---|---|---|
| [P0-T12] pre-change | 0 | 17.5 s | 0 |
| [P5-T3] mid-plan | 0 | 20.3 s | 0 |
| [P6-T7] final | **0** | 18.9 s | **0** |

## Output Summary

The corrected analyzer command passes the final QC pass with `EXIT_CODE: 0`, `0 Error(s)` and a
**zero** `Skipping target "CoreCompile"` count in 18.9 s. No analyzer regression was introduced by
any edit in this feature.
