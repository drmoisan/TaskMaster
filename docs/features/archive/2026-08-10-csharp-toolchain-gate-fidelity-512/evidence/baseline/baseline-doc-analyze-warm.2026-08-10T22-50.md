# Baseline — DOC-ANALYZE run warm: Defect C reproduced ([P0-T10])

Timestamp: 2026-08-10T22-50
Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /v:m /fl "/flp:logfile=coverage/baseline-doc-analyze-warm.log;verbosity=normal"`
EXIT_CODE: 0

Identical command to [P0-T9], re-run immediately against the outputs that run produced.
Invoked via `pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-doc-analyze-warm.ps1`.

## Measurements

| Metric | Cold ([P0-T9]) | Warm (this run) |
|---|---|---|
| `EXIT_CODE` | 0 | **0** |
| Elapsed | 27.8 s | **2.8 s** |
| `Skipping target "CoreCompile"` count | 0 | **18** |
| MSBuild summary | `0 Error(s)` | `0 Error(s)` |
| MSBuild summary | `6 Warning(s)` | `5 Warning(s)` |

## Defect C

The identical command, run a second time against a warm tree, returns `EXIT_CODE: 0` in **2.8 s**
having skipped `CoreCompile` on **18 of 18 projects**. Analyzer diagnostics are produced during
compilation, so a build that skips compilation on every project runs **no analyzers**. The documented
analyzer step is vacuous in the normal steady state of a working tree.

The elapsed time is **under 5 s** as the acceptance condition requires, and the skip count is
**greater than zero** (18), so the measured defect is reproduced and the plan proceeds. A zero skip
count here would have contradicted the defect and halted the plan for re-scoping; it did not occur.

Note that the warm run reports one fewer warning than the cold run (5 vs 6). This is a further
consequence of the same mechanism: a diagnostic emitted during compilation is not re-emitted when
compilation is skipped. It is recorded as corroboration, not as a separate finding.

## Non-vacuity assertion and its recorded deviation

The count of `Skipping target "CoreCompile"` in the `/fl` log is the assertion mechanism, deviating
from AC2's `csc.exe` parenthetical, which measurement shows is zero at `verbosity=normal` even for
genuine compiles. `CoreCompile:` header lines are not counted; they print even when the target is
skipped. This deviation is recorded in `spec.md` § "The non-vacuity assertion mechanism".

## Output Summary

Defect C is reproduced at this branch head: a warm DOC-ANALYZE returns exit 0 in 2.8 s with
`CoreCompile` skipped on 18 of 18 projects. This is the measurement that authorizes SD2 (correcting
the analyzer command alongside the type-check command) and is the "before" figure the corrected
ANALYZE run in [P0-T12] and the corrected `lint:` task run in [P5-T8] are contrasted against.
