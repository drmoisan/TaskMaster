# Baseline — ANALYZE (the corrected form) before any edit ([P0-T12])

Timestamp: 2026-08-10T22-54
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:m /fl "/flp:logfile=coverage/baseline-analyze-rebuild.log;verbosity=normal"`
EXIT_CODE: 0

This is **ANALYZE**, the corrected analyzer command this feature adopts at every documented site.
Invoked via `pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-analyze-rebuild.ps1`.

## Measurements

| Metric | Value |
|---|---|
| `EXIT_CODE` | **0** |
| Elapsed | **17.5 s** |
| `Skipping target "CoreCompile"` count | **0** |
| `CoreCompile:` header-line count (informational only) | 73 |
| MSBuild summary | `0 Error(s)` |
| MSBuild summary | `6 Warning(s)` |

## Comparison against the defective form

| Run | Command | EXIT | Elapsed | Skip count |
|---|---|---|---|---|
| [P0-T9] | DOC-ANALYZE cold | 0 | 27.8 s | 0 |
| [P0-T10] | DOC-ANALYZE warm | 0 | **2.8 s** | **18** |
| [P0-T12] (this run) | **ANALYZE** (`/t:Rebuild /m`) | 0 | 17.5 s | **0** |

ANALYZE runs against the same warm tree that made DOC-ANALYZE vacuous, and still compiles every
project. `/t:Rebuild` is what removes the dependence on MSBuild's timestamp-based up-to-date check,
which does not invalidate on a command-line `/p:` change.

## Non-vacuity assertion and its recorded deviation

The pass condition is a **zero** count of `Skipping target "CoreCompile"` in the `/fl` log; the
measured count is **0**. This is a recorded deviation from AC2's `csc.exe` parenthetical: the
`csc.exe` count is zero at `verbosity=normal` even for genuine compiles, so the parenthetical as
literally written is not satisfiable by the described log. The `CoreCompile:` header count (73) is
recorded here only to demonstrate why it is **not** used as the assertion: those headers print even
when the target is skipped, which is the counting trap `spec.md` documents. The zero-skip assertion
is strictly more discriminating.

## Output Summary

The corrected analyzer command is **green before any edit**: `EXIT_CODE: 0`, `0 Error(s)`, zero
`CoreCompile` skips, 17.5 s. The measured cost of adopting `/t:Rebuild` for the analyzer step is
17.5 s against the 2.8 s vacuous warm build it replaces. This establishes that the correction
introduces no new analyzer finding and that the documented command this feature adopts is passable
against the unmodified tree.
