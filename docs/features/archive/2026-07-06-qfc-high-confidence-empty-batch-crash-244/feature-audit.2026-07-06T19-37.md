# Feature Audit — Issue #244 (qfc-high-confidence-empty-batch-crash) — Re-audit Cycle 2

- Component/Feature: `2026-07-06-qfc-high-confidence-empty-batch-crash-244`
- Date: 2026-07-06
- Reviewer: feature-review agent
- Work Mode: minor-audit — acceptance-criteria source is `issue.md` `## Acceptance Criteria` only (no `spec.md`/`user-story.md` required or present).
- Prior cycle: `feature-audit.2026-07-06T12-48.md`. All five acceptance criteria were already evaluated PASS in cycle 1; this re-audit re-confirms each against unchanged production code and records the resolution of the cycle-1 policy-level blocking finding.

## Scope and Baseline

- Base branch (resolved via `git merge-base HEAD origin/main`): `main`
- Merge-base SHA: `b5f279624377cc82b884bb24ff81c46c899f3e6d`
- Head SHA: `9e01a4b827af1d819e8484b6de1775a703c9662b`
- Diff range: `b5f279624377cc82b884bb24ff81c46c899f3e6d..9e01a4b827af1d819e8484b6de1775a703c9662b`
- Note: the delegating prompt for this cycle supplied merge-base `961a768e0b093ec468c8180c9dc53996e1e6421a`, which is stale (one merged PR, #245, behind the actual current `main`). This audit used the correctly resolved merge-base per the Scope Invariant; see `policy-audit.2026-07-06T19-37.md` Rejected Scope Narrowing section for the verification detail.
- Changed files (git diff --stat, independently derived): `QuickFiler/Controllers/QfcDatamodel.cs` (+30/-1), `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` (new, +212), `QuickFiler.Test/QuickFiler.Test.csproj` (+1), plus 37 Markdown documentation/evidence/memory/governance files (including the new `coverage-policy-exception.md` and cycle-1's own audit artifacts).
- Diagnosis artifact: `docs/research/2026-07-06-quickfiler-entryid-column-index-diagnosis.md` (confirms root cause: `InitEmailQueue` had no `batchSize == 0` guard; `_frame.GetRowsAt(Enumerable.Range(0,0))` yields an empty-column-index frame that `GetRowsAs<IEmailSortInfo>()` cannot resolve).
- Plan: `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/plan.2026-07-06T11-26.md` (v1.1, all Phase 0-3 tasks marked complete).
- Production code is byte-identical to what cycle 1 evaluated; only documentation/governance/evidence artifacts changed between cycles.

## Acceptance Criteria Inventory

Source: `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/issue.md`, `## Acceptance Criteria` section.

1. **AC1**: `InitEmailQueue(0, worker)` returns an empty (non-null) `IList<MailItem>` and does not throw, regardless of `_frame` row count.
2. **AC2**: `InitEmailQueue(0, worker)` still sets up and starts the background worker so remaining emails are loaded to the master queue.
3. **AC3**: `InitEmailQueue(batchSize, worker)` with `batchSize > 0` retains existing behavior (first-batch projection via `GetRowsAs<IEmailSortInfo>()` and source-frame drop).
4. **AC4**: A deterministic, Outlook-free regression test reproduces the failure on pre-fix code (red) and passes after the fix (green), with no live UX/COM in either state.
5. **AC5**: Full C# toolchain (csharpier -> analyzers -> nullable -> MSTest) passes; changed-line coverage does not regress.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence | Notes |
|---|---|---|---|
| AC1 | **PASS** | `QuickFiler/Controllers/QfcDatamodel.cs:237-242` (guard reviewed directly, unchanged since cycle 1); `evidence/regression-testing/fail-before-InitEmailQueue-zero-batch.2026-07-06T15-45.md` (red, exact Deedle exception reproduced); `evidence/regression-testing/post-fix-test-run.2026-07-06T15-45.md` (green, `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` passing in narrow-filter and full-suite runs) | Source inspection confirms the guard returns `new List<MailItem>()` (non-null, empty) and precedes all `_frame.GetRowsAt`/`GetRowsAs` calls, so the exception path is structurally unreachable for `batchSize <= 0`. |
| AC2 | **PASS** | Same guard block calls `SetupWorker(worker); worker.RunWorkerAsync();` before returning; `evidence/regression-testing/fail-before-InitEmailQueue-worker-start.2026-07-06T15-45.md` (red); `evidence/regression-testing/post-fix-test-run.2026-07-06T15-45.md` (green, `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` passing deterministically in both narrow-filter and full-suite runs) | The v1.1 revision replaced a racy `worker.IsBusy` assertion with `WorkerSupportsCancellation` plus a bounded `TaskCompletionSource` wait on the injected `RemainingEmailLoader`; no caveat remains. |
| AC3 | **PASS** | `git diff` (re-confirmed this cycle) shows the pre-existing `batchSize > 0` body (clamp, `GetRowsAt`, frame drop, `GetRowsAs<IEmailSortInfo>()`, `GetItemFromID` projection, `SetupWorker`/`RunWorkerAsync`) is textually unchanged below the new guard; `evidence/regression-testing/pre-fix-test-run.2026-07-06T15-45.md` and `post-fix-test-run.2026-07-06T15-45.md` both show `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` passing. | Test asserts both the 2-item result and that the source `_frame` is drained to `RowCount == 0`, matching the documented pre-existing contract. |
| AC4 | **PASS** | `evidence/regression-testing/fail-before-InitEmailQueue-zero-batch.2026-07-06T15-45.md`, `fail-before-InitEmailQueue-worker-start.2026-07-06T15-45.md` (red, no live UX/COM — `grep -c "MessageBox"` == 0); `post-fix-test-run.2026-07-06T15-45.md` (green, deterministic in both narrow-filter and full-suite runs, `grep -c "MessageBox"` == 0) | The `RemainingEmailLoader` injectable-delegate seam ensures every test that starts a real `BackgroundWorker` assigns an inert delegate before calling `InitEmailQueue`. |
| AC5 | **PASS** | `evidence/qa-gates/qc-format.md`, `qc-lint.md`, `qc-nullable.md` (all exit 0); `qc-coverage.md` (472/472 tests passing; `QuickFiler` package coverage unchanged at 72.46%, 0.00 pp delta). Independently re-verified in this cycle: `dotnet tool run csharpier check` on both touched `.cs` files -> exit 0. | AC5's literal text (toolchain passes; changed-line coverage does not regress) is fully supported. This review's separate, review-level requirement for a canonical `artifacts/csharp/coverage.xml` artifact was a distinct BLOCKING policy finding in cycle 1; that finding is now resolved for this PR by the committed, repository-owner-authorized exception `244-COV-001` (see `policy-audit.2026-07-06T19-37.md` sections 5, 8, 10). AC5 is PASS both on its own literal terms and with respect to the review's overall merge-readiness gate. |

## Summary

All five acceptance criteria are supported by direct evidence and independent source/diff inspection performed during this re-audit; the production diff is unchanged since cycle 1. The fix is minimal, matches the confirmed root cause, and the test suite is deterministic and Outlook-free. Unlike cycle 1, the feature is now **clear for merge**: the sole blocking finding from cycle 1 (a canonical `artifacts/csharp/coverage.xml` artifact for the changed C# files) is resolved by the committed, repository-owner-authorized, PR-scoped coverage exception `244-COV-001`, fully tracked in `policy-audit.2026-07-06T19-37.md`. Two non-blocking quality observations (MEDIUM, LOW) remain open as documented follow-up recommendations and do not block merge.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/issue.md`
- Total AC items: 5
- Checked off (delivered): 5 (AC1-AC5, already checked `[x]` prior to this cycle; independently re-verified as PASS in this re-audit — no change made to `issue.md`)
- Remaining (unchecked): 0
- Items remaining: none

## Acceptance Criteria Check-off

All five acceptance criteria in `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/issue.md` were already marked `[x]` prior to this cycle (executor task P3-T5, evidence mirrored at `evidence/issue-updates/issue-244.2026-07-06T15-45.md`, and re-confirmed correct in `feature-audit.2026-07-06T12-48.md`). This re-audit independently re-verified each criterion against the unchanged source diff and the same regression-testing evidence, and additionally confirmed that the cycle-1 blocking coverage-artifact policy finding (which had gated overall merge-readiness without affecting any individual AC verdict) is now resolved by `244-COV-001`. No reversal or additional check-off was required. No edits were made to `issue.md` by this review.
