# Toolchain final pass — issue #839

Timestamp: 2026-09-13T06-14
Command: CMD-FORMAT (write-mode) and CMD-FORMAT-CHECK; CMD-ANALYZE with STAGE=final-analyzers; CMD-NULLABLE with STAGE=final-nullable; CMD-COVERAGE with STAGE=final
EXIT_CODE: 0
PASS-NUMBER: 1

## Output Summary

The four toolchain steps ran unconditionally in CLAUDE.md order and all four passed in a single pass. No step was skipped and no restart was triggered.

| Step | Task | Per-step artifact | Exit code |
|---|---|---|---|
| 1 — format | [P3-T1] | evidence/qa-gates/final-format.md | 0 |
| 2 — lint / analyzers | [P3-T2] | evidence/qa-gates/final-analyzers.md | 0 |
| 3 — type-check / nullable | [P3-T3] | evidence/qa-gates/final-nullable.md | 0 |
| 4 — test with coverage | [P3-T4] | evidence/qa-gates/final-tests.md | 0 |

This is pass number 1 and it is the clean pass. The [P3-T1] artifact of this pass records `FORMAT_CHANGED_OWNED_PATCH=False`, so the write-mode formatter altered nothing in the anchored patch over the two owned source trees and there was no earlier pass to report a restart cause for.

Supporting result signals from the four per-step artifacts, each recorded in full there:

- Step 1: `FORMAT_EXIT=0`, `FORMAT_CHANGED_OWNED_PATCH=False`, `PORCELAIN_NON_DOCS_LINES=2` naming only the two owned Write Set source files; the read-only verification pass checked 1624 files with zero `Was not formatted` lines.
- Step 2: `CSC_TASK_LINES=18`, `ZERO_ERROR_SUMMARY_LINES=1`, final summary line `0 Error(s)`.
- Step 3: `CSC_TASK_LINES=18`, `ZERO_ERROR_SUMMARY_LINES=1`, final summary line `0 Error(s)`, `CS86_ERROR_LINES=0`.
- Step 4: `COBERTURA_EXISTS=True`, `RUN_SUCCESSFUL_LINES=1`, Total tests 1394, Passed 1394, Failed 0, and all three named tests passed.

Both msbuild steps used the Rebuild target, and the non-zero `CSC_TASK_LINES` value on each proves the compiler actually ran rather than being skipped by the up-to-date check, so neither diagnostic gate is vacuous. Neither passed a solution-wide Nullable property. Outlook was verified closed before each rebuild and was never killed.

This artifact is an index in addition to, never instead of, the four per-step artifacts listed above; all four exist under evidence/qa-gates/ in this feature folder.
