# Feature Audit: QuickFiler High-Confidence Dequeue Streaming (#233)

**Audit Date:** 2026-07-03
**Feature Folder:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233`
**Base Branch:** `main`
**Head Branch:** `feature/quickfiler-high-confidence-dequeue-streaming-233`
**Work Mode:** `full-feature`
**Audit Type:** Initial acceptance review

## Scope and Baseline

- **Base branch:** `main` at merge base `00507b595297c3e6970634a1855f1144c987dbdf`
- **Head branch/commit:** `feature/quickfiler-high-confidence-dequeue-streaming-233` at `b1351b7e4e3977f1c2f806a3bd67f66ad14ff6b0`
- **Merge base:** `00507b595297c3e6970634a1855f1144c987dbdf`
- **Evidence sources:**
  - Primary: `artifacts/pr_context.summary.txt`
  - Secondary baseline diff: `artifacts/pr_context.appendix.txt`
  - Feature evidence: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/**`
  - Local review commands: `git diff --check`, `dotnet tool run csharpier -- check .`, analyzer msbuild, nullable msbuild
- **Feature folder used:** `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233`
- **Requirements source:** `spec.md` and `user-story.md`
- **Work mode resolution note:** `issue.md` contains `- Work Mode: full-feature`, so `spec.md` and `user-story.md` are authoritative.
- **Scope note:** Review scope is feature branch versus `main`; no caller narrowing was accepted.

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md` - primary full-feature requirements
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md` - primary full-feature user-facing requirements

### Acceptance criteria

1. AC1 - High-confidence filtering exists in exactly one location (the queue/dequeue layer). The post-hoc removal path (`ApplyHighConfidenceFilterAsync` -> `RemoveBelowThresholdAsync`) is no longer invoked to enforce the confidence threshold in the live flow, and no first-screen path loads an unfiltered fixed batch that is later trimmed by confidence. A repo-wide search shows no confidence-threshold comparison outside the single dequeue-layer location (excluding the dormant #171 pre-filter, whose disposition is recorded under AC8).
2. AC2 - The confidence threshold is evaluated at dequeue time. A unit test demonstrates that an item whose dequeue-time score is >= threshold is returned even if a different (earlier) score would have rejected it, and an item whose dequeue-time score is < threshold is discarded, with the decision driven by the dequeue-time measurement.
3. AC3 - Streaming backfill: when N items are requested in high-confidence mode and the candidate source contains at least N qualifying items interleaved with below-threshold items, the dequeue returns exactly N qualifying items (all >= threshold), having discarded the below-threshold candidates it encountered. A unit test covers the "must scan many to yield few" case.
4. AC4 - Source-exhaustion boundary: when fewer than N qualifying items remain, the dequeue returns all remaining qualifying items (0..N-1) without blocking indefinitely and without throwing. A unit test covers the zero-qualifying-remaining case and the partial case.
5. AC5 - No post-display removal: after an item is returned by the dequeue and placed on a page, a subsequent recomputation of its score below the threshold does not remove it from that page. A unit test demonstrates a surfaced item remains present after a simulated below-threshold rescore.
6. AC6 - Empty-page regression: a scenario reproducing the reported symptom yields full pages of qualifying items up to the per-iteration size and no empty page while qualifying items remain.
7. AC7 - Disabled-mode parity: when `HighConfidenceModeEnabled == false`, dequeue behavior is unchanged from today.
8. AC8 - Disposition of the two pipelines is explicit: the live path is the redesigned dequeue-layer filter; the dormant Issue #171 pre-filter is either wired to the new single location or explicitly retired/left dormant with a recorded decision. No third filtering pipeline is introduced.
9. AC9 - Threshold semantics preserved: the boundary remains inclusive (score == threshold qualifies), matching the existing `>=` keep / `<` reject convention.
10. AC10 - Full C# toolchain passes on the final pass (CSharpier -> .NET analyzers -> nullable/warnings-as-errors -> MSTest with coverage). New/changed non-COM-bound code meets the >= 90% coverage target; repository-wide coverage does not regress below 80% on the testable denominator.
11. AC11 - The probability debug logging introduced by issue #232 remains intact; any new dequeue-time scoring introduced by this work emits an equivalent debug log line.
12. AC12 - No unhandled behavioral regression in the ordinary non-high-confidence bulk-processing flow.

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| AC1 | Single live queue/dequeue confidence gate | FAIL | Async path uses the gate, but synchronous `Run()` and `Iterate()` still bypass it via `InitEmailQueue`/`LoadItems` and `DequeueNextItemGroup`. | Source inspection; `git diff --check`; PR context appendix | The checkbox is already marked in source, but review evidence does not support PASS. |
| AC2 | Dequeue-time threshold evaluation | PASS | `QfcStreamingDequeueConfidenceGate.DequeueAsync` scores before threshold comparison; tests cover dequeue-time selection. | Existing VSTest evidence; source inspection | PASS for the async gate scope. |
| AC3 | Streaming backfill until requested count or exhaustion | PARTIAL | Gate scans low/high items when the queue has candidates available, but lacks worker/source-completion awareness when the queue is temporarily empty. | Existing VSTest evidence; source inspection | Needs source-active test and implementation fix. |
| AC4 | Source-exhaustion boundary | PARTIAL | Zero and partial cases are tested for a static source, but runtime source exhaustion is inferred from one empty retry rather than a completion signal. | Existing VSTest evidence; source inspection | Does not prove the background source is exhausted. |
| AC5 | No post-display removal | PASS | Async `LoadItemsAsync(IList<MailItem>, ProgressTracker)` no longer calls `ApplyHighConfidenceFilterAsync` after `LoadSecondaryAsync`. | Existing targeted evidence; source inspection | PASS for async mail-item load path. |
| AC6 | Empty-page regression | FAIL | The gate can return empty after one delayed empty read while the worker may still be active, and synchronous paths bypass the gate. | Source inspection of `QfcStreamingDequeueConfidenceGate.cs` and `QfcHomeController.cs` | The reported symptom can still occur in the reviewed implementation. |
| AC7 | Disabled-mode parity | PASS | Disabled async dequeue path returns `DequeueDirectAsync`; tests and evidence cover disabled-mode parity. | Existing VSTest evidence; source inspection | No regression identified for disabled mode. |
| AC8 | Pipeline disposition explicit | PASS | `ac8-dormant-171-disposition.md` records dormant #171 disposition; no third filtering pipeline was identified beyond the blocker synchronous bypass. | `ac8-dormant-171-disposition.md`; source inspection | PASS for documentation, but AC1 still fails due live path coverage. |
| AC9 | Inclusive threshold semantics | PASS | Gate uses `score >= _cutoff`; boundary test covers score 900 at threshold 0.90. | Existing VSTest evidence; source inspection | PASS. |
| AC10 | Final C# toolchain and coverage | FAIL | CSharpier/analyzer/nullable pass; VSTest evidence passes; coverage comparison exits 1 and `git diff --check` fails. | Review commands and `coverage-comparison-remediation-final.md` | AC10 remains unchecked in both sources and should remain unchecked. |
| AC11 | Probability debug logging preserved and dequeue log added | PASS | Existing issue #232 logging tests pass; new gate logs subject, entry ID, and score. | Existing regression evidence; source inspection | PASS. |
| AC12 | Ordinary non-high-confidence regression coverage | PASS | Existing non-high-confidence regression evidence passes and disabled mode direct dequeue remains. | `non-high-confidence-regression.pass.md`; source inspection | PASS based on available evidence. |

## Summary

**Overall Feature Readiness:** BLOCKED

**Criteria summary:**
- **PASS:** 6 criteria
- **PARTIAL:** 2 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 4 criteria

**Top gaps preventing PASS:**

1. Synchronous `Run()`/`Iterate()` paths do not use the high-confidence dequeue gate.
2. The streaming gate can return partial or empty results without knowing whether the background source is exhausted.
3. AC10 fails because coverage comparison and whitespace checks fail.

**Recommended follow-up verification steps:**

1. Add behavior tests for high-confidence enabled synchronous `Run()` and `Iterate()` paths, or remove/prove those paths are not live.
2. Add source-active/source-exhausted behavior to the gate and test repeated empty queue intervals while candidates may still arrive.
3. Repair whitespace and produce a passing numeric C# coverage comparison.

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules, no source-file checkbox changes were made by this review. Criteria already checked in `spec.md` and `user-story.md` were not rechecked. AC10 remains unchecked in both files and must remain unchecked because it is FAIL. AC1, AC3, AC4, and AC6 are marked as not passing in this audit even though the source files currently show checked boxes; remediation should reconcile those source checkboxes after fixes.

### AC Status Summary

- Source: `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md`; `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`
- Total AC items: 24
- Checked off (delivered): 22 source checkboxes currently checked
- Remaining (unchecked): 2 source checkboxes currently unchecked
- Items remaining: AC10 in both authoritative source files

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md` | 12 | 11 source checkboxes checked | 1 | Review verdict disagrees with source checkboxes for AC1, AC3, AC4, and AC6. |
| `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md` | 12 | 11 source checkboxes checked | 1 | Review verdict disagrees with source checkboxes for AC1, AC3, AC4, and AC6. |
