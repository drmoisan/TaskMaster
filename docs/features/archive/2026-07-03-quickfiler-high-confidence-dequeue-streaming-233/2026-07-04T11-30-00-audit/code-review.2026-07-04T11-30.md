# Code Review: QuickFiler High-Confidence Dequeue Streaming (#233)

**Review Date:** 2026-07-04T11-30
**Reviewer:** Codex
**Feature Folder:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233`
**Feature Folder Selection Rule:** Supplied active feature folder, confirmed by PR context.
**Base Branch:** `main`
**Head Branch:** `feature/quickfiler-high-confidence-dequeue-streaming-233`
**Merge Base:** `ec4af1f0924b175a725fe50a5d2a61f7d27a3318`
**Head SHA:** `bb4b401c04a150e3ac1f128dd4648296971fd24d`
**Review Type:** Full feature-branch review relative to base.

## Executive Summary

The implementation moves live high-confidence filtering into a dequeue-layer streaming gate and keeps the first high-confidence page out of the prior fixed-batch-then-trim path. `QfcRemainingQueueAdmission` now admits candidates without applying the confidence threshold, while `QfcDatamodel.QueueProcessing` delegates high-confidence dequeue decisions to `QfcStreamingDequeueConfidenceGate`. The UI post-display removal helper remains present but is no longer invoked by the live `LoadItemsAsync(IList<MailItem>, ProgressTracker)` path.

No production correctness blocker was identified in the reviewed C# diff. The branch is still not ready because acceptance criterion AC10 remains failed by repository-wide C# coverage: `remediation-22-18-coverage-comparison.md` records 22.87% repository-path coverage against the required 80% floor. Current check-only commands passed for CSharpier, analyzer build, nullable build, and base-to-head whitespace.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Major | `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md` | AC10 | AC10 remains unchecked because repository-path C# coverage is 22.87%, below the required 80% floor. | Keep AC10 unchecked until repository-wide coverage satisfies policy or an approved exception is recorded through repository-accepted evidence. | Passing test execution and focused new-code coverage do not satisfy AC10 when the repository-wide coverage floor fails. | `evidence/qa-gates/remediation-22-18-coverage-comparison.md`; `evidence/other/remediation-22-18-ac10-no-approved-exception.md`. |
| Major | `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md` | AC10 | The same AC10 failure remains in the user-story acceptance source. | Preserve the unchecked AC10 state in both authoritative AC files. | Full-feature acceptance tracking uses both `spec.md` and `user-story.md`; their AC10 state must match verified evidence. | `evidence/other/remediation-22-18-ac10-status.md`. |
| Info | `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | class scope | The new dequeue gate is focused and covered above the new-code threshold at 95.00%. | Retain the focused seam and tests during remediation; do not move threshold enforcement back into UI or admission paths. | The implementation aligns with AC1-AC4 and protects the tested boundary while AC10 is handled separately. | `evidence/qa-gates/remediation-22-18-coverage-comparison.md`; `evidence/other/ac1-confidence-gate-search.md`. |

## Implementation Audit

### C# Implementation Audit

The C# production diff is concentrated in the QuickFiler queue/dequeue flow. `QfcStreamingDequeueConfidenceGate` owns the threshold cutoff and streaming/backfill loop. `QfcDatamodel.QueueProcessing` selects the gate only when high-confidence mode is enabled and preserves the direct dequeue path otherwise. `QfcHomeController.Run` and `RunAsync` initialize the queue with a zero-sized first batch in high-confidence mode, then load the first page from the dequeue layer. `QfcFormController.Actions` no longer invokes the post-display high-confidence removal after secondary loading.

Type-safety and analyzer checks passed in the current environment. The public interface surface is stable for `IQfcDatamodel.DequeueNextItemGroupAsync`; `IQfcCollectionController.RemoveBelowThresholdAsync` remains documented as not being the live issue #233 enforcement gate.

No changed C# source or test file exceeds 500 lines. The project file `QuickFiler/QuickFiler.csproj` is 557 lines after CSharpier formatting, but the repository's explicit 500-line rule applies to production code, test code, and reusable scripts; no changed `.cs` file violates that rule.

## Test Quality Audit

Reviewed evidence indicates targeted and full QuickFiler tests cover the required high-confidence behaviors:

- `evidence/regression-testing/streaming-gate.pass.md` verifies the streaming gate tests after expected-fail evidence.
- `evidence/regression-testing/source-active-streaming.pass.md` verifies polling while the source can still produce candidates.
- `evidence/regression-testing/first-page-and-no-post-display-removal.pass.md` verifies first-page routing and no post-display removal.
- `evidence/regression-testing/non-high-confidence-regression.pass.md` verifies ordinary non-high-confidence behavior remains covered.
- `evidence/qa-gates/remediation-22-18-vstest.md` records 387/387 tests passing with coverage enabled.

The test design is acceptable for the implemented seam. The tests use Moq, FluentAssertions, and `FakeTimeProvider` to avoid live Outlook and timing dependency. Coverage policy remains the only test-quality blocker.

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | PASS | Diff inspection found no new secrets or credential surfaces. |
| No unsafe subprocess or command construction | N/A | The C# change does not add subprocess execution. |
| Input validation at boundaries | PASS | The gate returns an empty list for non-positive quantity and null-guards constructor delegates. |
| Error handling remains explicit | PASS | Cancellation is checked before and during dequeue; existing unhook error propagation is preserved. |
| Configuration / path handling is safe | PASS | The change uses existing QuickFiler settings for mode and threshold. |
| Coverage threshold | FAIL | Repository-path coverage is 22.87%, below the 80% floor. |

## Research Log

No external research was required. This review used repository policy, canonical PR context artifacts, issue #233 requirement sources, current check-only command output, diff inspection, and feature-folder QA evidence.

## Verdict

No production code blocker was found beyond the policy/acceptance coverage blocker. The PR is not ready for approval because AC10 remains failed. Remediation should focus on resolving the repository-wide C# coverage disposition without weakening policy or marking coverage as not applicable.
