# Baseline — DOC-TYPECHECK run warm: Defect A's false-pass mode ([P0-T11])

Timestamp: 2026-08-10T22-52
Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true /nologo /v:m /fl "/flp:logfile=coverage/baseline-doc-typecheck-warm.log;verbosity=normal"`
EXIT_CODE: 0

This is the **defective documented type-check form** (DOC-TYPECHECK) — the mandatory step 3 of the
documented C# toolchain at `CLAUDE.md` § C#1 item 3, § CUT3 step 3, § "C# Toolchain (run in this
exact order)" step 3, `.claude/rules/csharp.md` § Toolchain item 3, and
`.claude/skills/csharp-qa-gate/SKILL.md` step 3. Measured only; it is not adopted.

Invoked via `pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/run-doc-typecheck-warm.ps1`.
Run immediately after [P0-T10], against a warm tree, exactly as the documented toolchain loop
prescribes (step 2 always precedes step 3).

## Measurements

| Metric | Value |
|---|---|
| `EXIT_CODE` | **0** |
| Elapsed | **1.8 s** |
| `Skipping target "CoreCompile"` count | **18** |
| MSBuild summary | `0 Error(s)` |
| MSBuild summary | `5 Warning(s)` |

## Defect A — the vacuous pass this feature removes

The documented type-check gate **passed** in 1.8 s having skipped `CoreCompile` on **18 of 18
projects**. `/p:Nullable=enable` and `/p:TreatWarningsAsErrors=true` never reached the compiler.
The gate reported success without performing any type checking whatsoever.

The skip count is greater than zero (18) as the acceptance condition requires. This figure reproduces
the pre-existing measurement M2 in
`baseline-nullable-gate-vacuity.2026-08-10T14-25.md` (exit 0, 1.8 s, 18 of 18) exactly.

This measurement is also the mechanism behind the "strengthening, not a relaxation" argument: the
property whose removal a reviewer might read as a relaxation was never reaching the compiler in the
first place, because the step that would have carried it never compiled.

## Non-vacuity assertion and its recorded deviation

The count of `Skipping target "CoreCompile"` in the `/fl` log is the assertion mechanism, deviating
from AC2's `csc.exe` parenthetical, which measurement shows is zero at `verbosity=normal` even for
genuine compiles. `CoreCompile:` header lines are not counted; they print even when the target is
skipped. Recorded in `spec.md` § "The non-vacuity assertion mechanism".

## Output Summary

Defect A's false-pass mode is reproduced at this branch head: `EXIT_CODE: 0` in 1.8 s with 18 of 18
`CoreCompile` targets skipped. A zero skip count here would have contradicted the recorded defect and
halted the plan; it did not occur. [P0-T14] runs the corrected TYPECHECK form as the pre-change
positive control.
