# Code Review: QuickFiler High-Confidence Dequeue Streaming (#233)

**Review Date:** 2026-07-03T19-16
**Reviewer:** Codex
**Feature Folder:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233`
**Base Branch:** `main`
**Merge Base:** `ec4af1f0924b175a725fe50a5d2a61f7d27a3318`
**Head SHA:** `46bc5c719546ad3cf7ae26a101bac9d8b314e8af`
**Review Type:** R4 re-audit after remediation

## Executive Summary

The production C# changes continue to route high-confidence QuickFiler startup, iteration, and queue draining through a dequeue-layer confidence gate. The reviewed implementation has targeted behavior evidence for dequeue-time scoring, source-active polling, streaming backfill, disabled-mode parity, and synchronous high-confidence startup/iteration routing. No new production-code blocker was identified in this R4 review.

The branch is not ready for merge because policy failures remain. `git diff --check` fails against the supplied merge base, `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` now exceeds the 500-line limit, and AC10 remains failed because repository-path C# coverage is 22.86% against the 80% policy floor.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Major | `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` | File length | The file is 552 lines at reviewed head. The supplied base version was 395 lines, and this branch added 177 lines, crossing the repository 500-line limit for test code. | Split the issue #233 high-confidence startup tests into a focused test class or helper file so each test file is below 500 lines. | The repository policy states that production code, test code, and reusable scripts must not exceed 500 lines per file. | R4 line-count comparison: base 395, head 552. |
| Major | `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/remediation-baseline/remediation-start-state.md` | Line 34 | `git diff --check ec4af1f0924b175a725fe50a5d2a61f7d27a3318...HEAD` reports trailing whitespace. | Remove the trailing whitespace in the evidence artifact and rerun the base-to-head whitespace check. | The feature-review workflow treats failed toolchain or policy checks as remediation triggers. | R4 command exited 1 with `remediation-start-state.md:34: trailing whitespace`. |
| Major | `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md` | AC10 | AC10 remains unchecked because repository-path C# coverage is 22.86%, below the 80% repository-wide floor. | Increase repository-path coverage to the required threshold or record an approved exception before claiming AC10. | The feature cannot satisfy all acceptance criteria while the documented coverage gate remains failed. | `evidence/qa-gates/vstest-remediation-rerun.md`; `evidence/qa-gates/coverage-comparison-remediation-final.md`. |

No Blocker production-code finding was identified in the reviewed C# implementation. The findings above still require remediation before PR readiness.

## Implementation Audit

### C# Implementation Audit

The reviewed production flow preserves the intended queue/dequeue placement:

- `QfcHomeController.Run()` and `RunAsync()` use `InitEmailQueue(0, ...)` when high-confidence mode is enabled and then load the first displayed page from `DequeueNextItemGroupAsync`.
- `QfcHomeController.Iterate()` routes high-confidence synchronous iteration through the async dequeue path.
- `QfcDatamodel.DequeueNextItemGroupAsync()` delegates high-confidence mode to `QfcStreamingDequeueConfidenceGate`.
- `QfcRemainingQueueAdmission.TryQueueAsync()` no longer rejects items based on high-confidence score at admission time.
- `QfcFormController.Actions.LoadItemsAsync(IList<MailItem>, ...)` no longer invokes post-display high-confidence removal after secondary loading.

The search evidence still shows dormant or non-live threshold helpers in `QfcCollectionController.RemoveBelowThresholdAsync`, `QfcFormController.Actions.ApplyHighConfidenceFilterAsync`, and `QfcHighConfidencePreFilter`. Those are documented for AC8 and were not identified as live enforcement calls in this review.

### Type Safety and API Notes

- R4 nullable build passed with 0 warnings and 0 errors.
- R4 analyzer build passed with 0 warnings and 0 errors.
- No new external package or CLI/API surface was introduced by the reviewed production changes.

### Error Handling and Logging

- The streaming gate observes cancellation before taking and after scoring items.
- Dequeue-time probability debug logging uses log4net and includes subject, EntryID, and score.
- No new broad catch boundary or ad hoc console output was identified in the production diff reviewed for issue #233.

## Test Quality Audit

The test suite includes targeted coverage for:

- `Run_HighConfidenceEnabled_DoesNotLoadUnfilteredInitialBatch`
- `RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue`
- `Iterate_HighConfidenceEnabled_DoesNotLoadDirectSynchronousBatch`
- `DequeueAsync_SourceActiveAfterRepeatedEmptyReads_ContinuesPollingUntilCandidateArrives`
- dequeue-time scoring, scan-many-to-yield-few backfill, source exhaustion, threshold inclusivity, cancellation, and below-threshold discard.

The tests use MSTest, Moq, and FluentAssertions. The quality issue is structural: `QfcHomeControllerRunAsyncTests.cs` now exceeds the repository file-size policy.

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets introduced | PASS | Reviewed production/test diff does not add credentials or secret material. |
| No unsafe subprocess execution | PASS | Production changes do not add process execution. |
| Nullability | PASS | R4 nullable build exited 0 with 0 warnings and 0 errors. |
| Analyzer diagnostics | PASS | R4 analyzer build exited 0 with 0 warnings and 0 errors. |
| Whitespace | FAIL | Base-to-head `git diff --check` reports trailing whitespace in issue #233 evidence. |
| File-size policy | FAIL | Modified test file `QfcHomeControllerRunAsyncTests.cs` is 552 lines. |
| Coverage policy | FAIL | Repository-path coverage remains 22.86%. |

## Research Log

No external research was required. The R4 review used repository policy, feature-review skills, PR context artifacts, branch diff, current source files, and local verification commands.

## Verdict

Functional issue #233 behavior appears supported by the reviewed production implementation and targeted tests. PR readiness remains negative because three policy findings require remediation: base-to-head whitespace failure, introduced test-file size violation, and AC10 coverage failure.
