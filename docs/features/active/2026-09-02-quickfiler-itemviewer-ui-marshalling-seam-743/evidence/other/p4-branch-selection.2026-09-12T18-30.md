# Phase 4 branch selection (P4-T1)

Task: [P4-T1]
Timestamp: 2026-09-13T03-30
Command: none (branch selected by reading `evidence/baseline/ac1-mechanism-verdict.2026-09-12T17-00.md`, the P1-T11 artifact, directly in this run)
EXIT_CODE: 0
Output Summary: the P1-T11 verdict is `H-LEAK REJECTED`; **Branch COST** is selected and executed. Neither fixture file is modified.

## Selecting figures, quoted verbatim from the P1-T11 artifact

- Serial-regime contended count (section (ii), SERIAL row, `contended` column): `0`
- Serial-regime balance test (section (ii), SERIAL row, `Balance test` column): `Passed (11 - 10 = 1)`
- Section (iii) applies the pre-declared decision-rule row `| 0 | passed (difference equals 1) | H-LEAK REJECTED by direct observation; H-COST is the surviving mechanism |` and states: "Measured: serial contended count = 0; serial balance test = passed with difference exactly 1. Row 1 is selected." and "**REJECTED hypothesis: H-LEAK**".

## Branch executed: COST (exactly one branch)

Per the P4-T1 text, under Branch COST no further change is made to either fixture file. The three monotonic counters (`TransactionAcquisitions`, `TransactionReleases`, `ContendedAcquisitions`) added to `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` in P1-T6 and the balance test `TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition` added to `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` in P1-T7 are RETAINED as permanent assertions, which spec section 6.4 explicitly permits. This retention decision is recorded here.

Branch LEAK was not executed: no owner-anchored release, no `[TestCleanup]`/`[AssemblyCleanup]`-scoped path and no abandoned-owner regression test were added.

## Files modified by this task

Neither `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` nor `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` was modified. P4-T2 (format and build) and P4-T3 (both-regime confirmation runs) run unconditionally after this selection.
