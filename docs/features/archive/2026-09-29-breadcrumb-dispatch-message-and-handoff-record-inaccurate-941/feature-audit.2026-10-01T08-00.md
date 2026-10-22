# Feature Audit - Issue #941

## Scope and Baseline

- Work Mode: minor-audit. The acceptance-criteria source is the `## Acceptance Criteria` section of `issue.md` (AC1 to AC9). `spec.md` and `user-story.md` are intentionally absent.
- Baseline: merge base `9b3eea58447c264eae6f95a4bfee3bfcec7fb17f`. Reviewed range: `9b3eea58447c264eae6f95a4bfee3bfcec7fb17f..HEAD` (HEAD `c7152db65`).
- Footprint: `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` (line 183), `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs` (line 305), `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` (one new test), the #900 handoff record, and the feature folder.
- Method: each criterion was checked against the files at HEAD with Read and Grep, and against the committed evidence under `evidence/`. The executor's check-off marks and summaries were not relied on. No shell was used.

## Acceptance Criteria Inventory

- AC1: `DispatchValue` no-context message at line 183 reads the exact new text and no longer contains `cannot marshal cross-thread UI work`.
- AC2: `Dispatch` no-context message at line 101 unchanged.
- AC3: `BreadcrumbUiThreadDispatchTests.cs` line 305 pattern changed to `*outside an executing Dispatch callback*`, and the test passes.
- AC4: `BreadcrumbPopupBoundaryCoverageTests.cs` line 86 assertion passes without modification.
- AC5: New regression test for the owner-thread, outside-callback `DispatchValue` case; fails before AC1, passes after.
- AC6: Handoff record attributes only the `Dispatch` site test to the owner-thread-id check, attributes the `DispatchValue` site to the executing-callback and null-context checks, and cites lines 276-277.
- AC7: No other test asserts either message text without a matching update.
- AC8: C# toolchain passes in order.
- AC9: Coverage does not regress on changed lines; no changed file exceeds 500 lines.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| AC1 | PASS | `BreadcrumbUiDispatcher.cs:183` reads `"The owner-thread-only test dispatcher cannot run value-producing UI work outside an executing Dispatch callback."` (Read at HEAD). The phrase `cannot marshal cross-thread UI work` occurs only at line 101 (Grep over `*.cs`). |
| AC2 | PASS | `BreadcrumbUiDispatcher.cs:101` still reads `"The owner-thread-only test dispatcher cannot marshal cross-thread UI work."`; the supplied diff shows no change to that line; `evidence/other/p1-t4-dispatch-site-unchanged.md`. |
| AC3 | PASS | `BreadcrumbUiThreadDispatchTests.cs:305` reads `.WithMessage("*outside an executing Dispatch callback*")`. `ProductionCaptureWithoutUiContext_FailsFast` passed in the three-test run (3 of 3 passed): `evidence/regression-testing/p1-t7-three-tests-pass.md`. |
| AC4 | PASS | `BreadcrumbPopupBoundaryCoverageTests.cs:86` still reads `Contain("cannot marshal")`; the supplied diff shows the only change in that file is the 24-line insertion after line 87. `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` passed in `p1-t7`. |
| AC5 | PASS | New test at lines 93-111. Before the literal change it failed with `Expected fault.Message "...cannot marshal cross-thread UI work." to contain "outside an executing Dispatch callback"` (`evidence/regression-testing/p1-t2-expect-fail-new-test.md`, total 1, failed 1). After the change it passed (`p1-t5-new-test-passes-after-fix.md`, `p1-t7`). The test calls `DispatchValue` on the owner thread of a `CreateOwnerOnlyDispatcher` instance outside any executing callback. |
| AC6 | PASS | Record content verified against source: `_ownerThreadId` stored at line 40, supplied at 54 and 64, compared at 276-277 in `IsCurrentBoundary`, which is called only from `Dispatch` (line 78). `DispatchValue` (180-188) tests only `_context == null`. The record now says the `DispatchValue` site faults for every caller outside an executing callback on any thread and cites `:276-277`. Hunk, token, and line-ending checks in `evidence/other/p1-t9-handoff-corrected.md`. |
| AC7 | PASS | Grep of `*.cs` for `cannot marshal` returns exactly `BreadcrumbPopupBoundaryCoverageTests.cs:86` and `BreadcrumbUiDispatcher.cs:101`; also `evidence/other/p1-t10-cannot-marshal-census.md` and `evidence/qa-gates/p2-t8-scope-and-size.md`. |
| AC8 | PASS | `evidence/qa-gates/p2-t9-toolchain-pass.md`: csharpier format (REWRITTEN 0), csharpier check, analyzers rebuild, nullable rebuild, and scoped MSTest coverage all exit 0 in loop iteration 1. The test run reports total 1470, passed 1470, failed 0 (`p2-t5-mstest-coverage.md`). |
| AC9 | PASS | Changed lines 180-187 hit 1 before and after (`p0-t11`, `p2-t6`, `p2-t7`: clause 1 held 8 of 8). QuickFiler package 82.01% lines and 78.27% branches, identical before and after. Sizes 285, 480, and 410 lines (410 reproduced by Grep). See the coverage note below. |

### Coverage note on AC9

The first recorded final measurement read one covered line and one covered branch below baseline (10459 and 2517). An identical second measurement, with no source change between, matched baseline exactly (10460 and 2518). Both are quoted in `evidence/qa-gates/p2-t5-mstest-coverage.md` and `p2-t7-coverage-delta.md`. The disposition is adequate: the diff changes a string literal and adds a test that sits outside the coverage denominator, so it cannot remove a covered production line. The changed lines are hit in both measurements. The cause of the one-unit variation is not established, which is recorded as a non-blocking observation in the policy audit.

### Additional behavior check

Production behavior is unchanged apart from the message text at line 183. The guard at line 180, the `Report` call under `reportFailure`, and the `Task.FromException<T>` return are unchanged. Line 101 is untouched.

## Acceptance Criteria Check-off

- Source: `issue.md`, `## Acceptance Criteria`.
- All nine items AC1 to AC9 were already marked `[x]` by the executor. Each was independently re-verified above as PASS. No additional items were checked off by this review, and none were unchecked.
- Not part of the acceptance criteria: the `## Proposed Fix / Validation Ideas` and `## Next Step` checkboxes in `issue.md` remain unchecked; they are not the minor-audit criteria source.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-09-29-breadcrumb-dispatch-message-and-handoff-record-inaccurate-941/issue.md`
- Total AC items: 9
- Checked off (delivered): 9
- Remaining (unchecked): 0
- Items remaining: none

## Summary

Verdict: PASS. All nine acceptance criteria are met with independently verified evidence. Blocking findings: 0. Non-blocking findings: 3 (listed in `code-review.2026-10-01T08-00.md` and `policy-audit.2026-10-01T08-00.md`).

Follow-up candidates for the coordinator (out of scope for this item):

- Refresh the stale line range in the #900 handoff record's first citation (`BreadcrumbPopupBoundaryCoverageTests.cs:58-61`), which predates this branch.
- Add per-file line and branch rates for touched files to the standard coverage projection evidence, so per-file floors can be restated without the raw Cobertura document.
- Establish why two identical scoped QuickFiler coverage runs differ by one covered line and branch.
- Reconcile the 80 percent floor in `CLAUDE.md` with the 85 percent floor in `.claude/rules/general-unit-test.md` (policy-owner decision; both are push-down or maintainer owned).

No remediation inputs are required.
