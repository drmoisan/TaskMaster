# Code Review - Issue #941

- Review scope: `9b3eea58447c264eae6f95a4bfee3bfcec7fb17f..HEAD` (HEAD `c7152db65`)
- Files reviewed: `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`, `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs`, `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs`, the #900 handoff record, and the feature folder.
- Method: Read and Grep at HEAD against the caller-supplied diff; no shell was used.

## Executive Summary

Verdict: PASS. Blocking findings: 0. Non-blocking findings: 3.

The production change is a single string literal at `BreadcrumbUiDispatcher.cs:183`. Surrounding logic (lines 180-188) is unchanged: the same guard, exception construction, conditional `Report`, and `Task.FromException<T>`. The line 101 `Dispatch` literal is unchanged. The new wording, "The owner-thread-only test dispatcher cannot run value-producing UI work outside an executing Dispatch callback.", is accurate: `DispatchValue` reaches this branch only when `_executingDispatcher` is not this instance and `_context` is null, and it does not read `_ownerThreadId`. The new regression test is deterministic and follows MSTest, FluentAssertions, and Arrange-Act-Assert. The handoff record corrections match the code.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Low (non-blocking) | `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` | line 106 | `result.Exception.InnerException` is read without a null guard. | No change required; optionally assert on `result.Exception` first. | The preceding `IsFaulted` assertion guarantees `Exception` is non-null; `Task.FromException<T>` wraps the supplied exception as the sole inner exception. No nullable directive applies here. | `BreadcrumbUiDispatcher.cs:187`; test lines 105-107 |
| Low (non-blocking) | `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/p5-t14-follow-up-handoff.2026-09-17T02-39.md` | line 38 | The first citation still reads `BreadcrumbPopupBoundaryCoverageTests.cs:58-61`, while `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` is now declared at line 67 with its body at lines 66-87. | Optionally refresh the line range; the citation names the test, so it stays findable. | The drift predates this branch (the #931 work reshaped the test). Issue #941 scoped the record corrections to attribution, and AC6 does not cover this range. | Test file lines 53-87; handoff record line 38 |
| Low (non-blocking) | `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` | lines 93-111 | The new test does not also assert that the sink is invoked exactly once when `reportFailure` is false. | No change required for this item. | `reportFailure = false` is a separate behavior of lines 185-186 and outside the scope of a message-wording fix. | `BreadcrumbUiDispatcher.cs:185-186` |

## Review Detail

### Behavior preservation (caller question 1)

- `BreadcrumbUiDispatcher.cs:180-188` at HEAD: `if (_context == null)` constructs `InvalidOperationException` with the new literal, calls `Report(failure)` when `reportFailure` is true, and returns `Task.FromException<T>(failure)`. The structure, branch condition, and ordering match the pre-change shape described in the diff (1 line added, 1 deleted).
- `BreadcrumbUiDispatcher.cs:99-104` (the `Dispatch` site, literal on line 101) still reads `cannot marshal cross-thread UI work.`
- The only consumers of the message text are three assertions: `BreadcrumbPopupBoundaryCoverageTests.cs:86` (unchanged `Dispatch` literal), `BreadcrumbUiThreadDispatchTests.cs:305` (updated), and the new test. A Grep of `*.cs` for `cannot marshal` returns exactly the line 86 assertion and the line 101 literal, so no other test depends on the removed wording.

### New test quality (caller question 2)

- `DispatchValue_OwnerOnlyOnOwnerThread_FaultsOutsideExecutingCallback` (lines 93-111): builds an owner-only dispatcher through the existing `CreateOwnerOnlyDispatcher` helper, calls `DispatchValue(() => executions++)` on the owner thread, and asserts synchronously. It has no sleep, delay, timeout, wait, thread hand-off, or file access.
- It exercises the exact case the research found misdescribed: the owner-thread caller, for which `Dispatch` would execute inline but `DispatchValue` faults. This pins the discriminating behavior, not only the wording.
- Assertions: faulted (with reason), exception type, message substring, action not run, sink received the same instance. Failure messages are specific.
- The regression-first record shows the test failed before the literal change with the expected message text (`evidence/regression-testing/p1-t2-expect-fail-new-test.md`) and passed after.
- The modified pattern at `BreadcrumbUiThreadDispatchTests.cs:305` is stricter than the old one: `*outside an executing Dispatch callback*` matches only the `DispatchValue` message, whereas the old pattern matched the broad wording.

### Handoff record accuracy (caller question 3)

- `_ownerThreadId` is declared at line 23, stored at line 40, supplied at lines 54 and 64, and compared at lines 276-277. `IsCurrentBoundary` has one call site, line 78 in `Dispatch`. `DispatchValue` never calls it.
- The corrected record states that only the `Dispatch` site exercises the owner-thread-id check, that the `DispatchValue` site faults for every caller outside an executing callback on any thread, and cites `:276-277`. All three statements are accurate.
- The record keeps its CRLF line endings (186 CRLF, 186 LF per `evidence/other/p1-t9-handoff-corrected.md`), so the diff is limited to the intended hunks.

### Policy conformance

- File size: 285, 480, and 410 lines; all within 500. The 410-line figure was reproduced with a Grep line count at HEAD.
- Names, visibility (`internal`), and style match the surrounding code.
- No suppression, banned API, or dependency added.
- Prose in the evidence files uses neutral, factual wording.
- No absolute host path or account name appears in the feature folder or the edited record; two public URLs are present.

## Acceptance Criteria Inventory

Source: `issue.md`, section `## Acceptance Criteria` (minor-audit). Nine items, AC1 to AC9.

## Acceptance Criteria Evaluation

| AC | Verdict | Basis |
|---|---|---|
| AC1 | PASS | Line 183 reads the required text exactly; Grep shows `cannot marshal` only at line 101. |
| AC2 | PASS | Line 101 literal unchanged. |
| AC3 | PASS | Line 305 pattern is `*outside an executing Dispatch callback*`; passes (`p1-t7`). |
| AC4 | PASS | Line 86 `Contain("cannot marshal")` unmodified; passes (`p1-t7`). |
| AC5 | PASS | New test present; failed before and passed after. |
| AC6 | PASS | Record content verified against the dispatcher source. |
| AC7 | PASS | Grep census returns two hits. |
| AC8 | PASS | Toolchain exit codes all 0 in one iteration (`p2-t9`). |
| AC9 | PASS | Changed lines hit 1 before and after; package rates unchanged; all files within 500 lines. |

A full evaluation with evidence is in `feature-audit.2026-10-01T08-00.md`.
