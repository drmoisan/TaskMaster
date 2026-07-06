# P3-T4 — Final QA: Test + Coverage Gate (Issue #244, v1.1)

Timestamp: 2026-07-06T15-45

Command: `MSYS_NO_PATHCONV=1 "/c/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /EnableCodeCoverage`

EXIT_CODE: 0

## Output Summary

Total tests: 472, Passed: 472, Failed: 0. Test Run Successful. This includes all 3 v1.1 regression
tests from `QfcInitEmailQueueZeroBatchTests.cs` — `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`,
`InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`, and
`InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` — all Passed, plus all 469
previously-passing tests still passing (472 = 469 baseline + 3 new). Reproduced identically across the
P2-T1 narrow-filter/full-suite runs and this final gate run: no flake observed in this cycle. Unlike
the v1.0 revision, `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` no longer asserts
`worker.IsBusy` (the race condition documented against v1.0), so there is no context-dependent
narrow-filter-vs-full-suite discrepancy to report for this revision.

## Numeric coverage (Cobertura-format rerun, same 472/472 result)

Using the same Cobertura-format `/Settings:` runsettings as the P0-T6 baseline (excluding
`[ExcludeFromCodeCoverage]`/DebuggerHidden/DebuggerNonUserCode/CompilerGenerated/GeneratedCode-attributed
members):

- Baseline `QuickFiler` package line-rate (P0-T6): **72.46%** (`0.72456993268511594`, lines
  3875/5348, complexity 913)
- Post-change `QuickFiler` package line-rate: **72.46%** (`0.72456993268511594`, lines 3875/5348,
  complexity 913)
- Delta: **0.00 percentage points** — no regression.

This exact match is expected: `QfcDatamodel` carries a class-level `[ExcludeFromCodeCoverage]`
attribute, so neither the pre-existing `batchSize <= 0` guard's lines nor the v1.1
`RemainingEmailLoader` seam's lines (the property, its constructor assignments, and the
`Worker_DoWork` call-site change) are part of the measured denominator, before or after this change,
per the plan's Root Cause Summary and the repository's COM/VSTO/WinForms coverage exemption
(`.claude/rules/general-unit-test.md`).

## No pop-up / no live COM confirmation

The captured console log for this run (`final-coverage-run.log` in the execution scratchpad) contains
zero occurrences of the string `MessageBox` (`grep -c "MessageBox"` returned `0`). No dialog or hang
occurred; the run completed in under 7 seconds and returned `EXIT_CODE: 0`.
