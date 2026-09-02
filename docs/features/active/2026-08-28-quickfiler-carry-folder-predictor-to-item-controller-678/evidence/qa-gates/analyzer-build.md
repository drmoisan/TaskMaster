# P2-T3 — Analyzer build

Timestamp: 2026-09-01T23-48

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0

## Output Summary

MSBuild summary lines, reproduced verbatim:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

## Acceptance conditions

### 1. `EXIT_CODE: 0` with a zero error count in the MSBuild summary

`EXIT_CODE: 0` and `0 Error(s)`, both above.

### 2. The warning count is at or below the `BASELINE_ANALYZER_SUMMARY` warning count, with any new warning named individually

| Measurement | Warnings | Errors |
|---|---:|---:|
| `BASELINE_ANALYZER_SUMMARY` (P0-T6) | 5 | 0 |
| Post-change (this run) | **5** | **0** |
| Delta | **0** | **0** |

The post-change count equals the baseline count, so it is at or below it. **No new warning was
introduced**, and there is therefore none to name individually.

The five are the same five uncoded System.Reactive `packages.config` warnings the baseline recorded,
one per project that carries a `packages.config` and references System.Reactive 7.0.0:
`UtilitiesCS`, `ToDoModel`, `QuickFiler`, `TaskMaster` and `UtilitiesCS.Test`. They come from a NuGet
package's targets file, not from a Roslyn analyzer.

A scan of the full build log for the pattern `warning <CODE>:` returned **no match**, so no `CA`,
`CS`, `IDE`, `MA`, `RCS`, `S`, `AsyncFixer` or `RS` diagnostic was emitted at any severity above
message level, matching the baseline exactly.

## Non-vacuity control

`/t:Rebuild` was used rather than `/t:Build`, verified directly rather than assumed: the build log
contains **63** `CoreCompile:` target executions, so compilation, and therefore analyzer execution,
actually ran. A warm `/t:Build` would have exited 0 with `CoreCompile` skipped on every project and
the gate could not have failed.
