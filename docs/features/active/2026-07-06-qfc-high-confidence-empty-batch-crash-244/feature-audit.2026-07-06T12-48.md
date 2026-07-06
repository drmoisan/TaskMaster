# Feature Audit — Issue #244 (qfc-high-confidence-empty-batch-crash)

- Component/Feature: `2026-07-06-qfc-high-confidence-empty-batch-crash-244`
- Date: 2026-07-06
- Reviewer: feature-review agent
- Work Mode: minor-audit — acceptance-criteria source is `issue.md` `## Acceptance Criteria` only (no `spec.md`/`user-story.md` required or present).

## Scope and Baseline

- Base branch (resolved): `main`
- Merge-base SHA: `961a768e0b093ec468c8180c9dc53996e1e6421a`
- Head SHA: `03f89411700d1ff9964630c919b58df2ed5abcd0`
- Diff range: `961a768e0b093ec468c8180c9dc53996e1e6421a..03f89411700d1ff9964630c919b58df2ed5abcd0`
- Changed files (git diff --stat, independently derived — see policy-audit's Rejected Scope Narrowing note about the PR-context summary's file-classification gap): `QuickFiler/Controllers/QfcDatamodel.cs` (+31/-1), `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` (new, +212), `QuickFiler.Test/QuickFiler.Test.csproj` (+1), plus 28 Markdown documentation/evidence/memory files.
- Diagnosis artifact: `docs/research/2026-07-06-quickfiler-entryid-column-index-diagnosis.md` (confirms root cause: `InitEmailQueue` had no `batchSize == 0` guard; `_frame.GetRowsAt(Enumerable.Range(0,0))` yields an empty-column-index frame that `GetRowsAs<IEmailSortInfo>()` cannot resolve).
- Plan: `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/plan.2026-07-06T11-26.md` (v1.1, all Phase 0-3 tasks marked complete).

## Acceptance Criteria Inventory

Source: `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/issue.md`, `## Acceptance Criteria` section.

1. **AC1**: `InitEmailQueue(0, worker)` returns an empty (non-null) `IList<MailItem>` and does not throw, regardless of `_frame` row count.
2. **AC2**: `InitEmailQueue(0, worker)` still sets up and starts the background worker so remaining emails are loaded to the master queue.
3. **AC3**: `InitEmailQueue(batchSize, worker)` with `batchSize > 0` retains existing behavior (first-batch projection via `GetRowsAs<IEmailSortInfo>()` and source-frame drop).
4. **AC4**: A deterministic, Outlook-free regression test reproduces the failure on pre-fix code (red) and passes after the fix (green), with no live UX/COM in either state.
5. **AC5**: Full C# toolchain (csharpier → analyzers → nullable → MSTest) passes; changed-line coverage does not regress.

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence | Notes |
|---|---|---|---|
| AC1 | **PASS** | `QuickFiler/Controllers/QfcDatamodel.cs:237-242` (guard reviewed directly); `evidence/regression-testing/fail-before-InitEmailQueue-zero-batch.2026-07-06T15-45.md` (red, exact Deedle exception reproduced); `evidence/regression-testing/post-fix-test-run.2026-07-06T15-45.md` (green, `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` passing in narrow-filter and full-suite runs) | Source inspection confirms the guard returns `new List<MailItem>()` (non-null, empty) and precedes all `_frame.GetRowsAt`/`GetRowsAs` calls, so the exception path is structurally unreachable for `batchSize <= 0`. |
| AC2 | **PASS** | Same guard block calls `SetupWorker(worker); worker.RunWorkerAsync();` before returning; `evidence/regression-testing/fail-before-InitEmailQueue-worker-start.2026-07-06T15-45.md` (red); `evidence/regression-testing/post-fix-test-run.2026-07-06T15-45.md` (green, `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` passing deterministically in both narrow-filter and full-suite runs) | The v1.1 revision replaced a racy `worker.IsBusy` assertion with `WorkerSupportsCancellation` (a synchronous, non-racing side effect of `SetupWorker`) plus a bounded `TaskCompletionSource` wait on the injected `RemainingEmailLoader`, removing the context-dependent caveat recorded against the v1.0 evidence. No caveat remains for the v1.1 evidence. |
| AC3 | **PASS** | `git diff` confirms the pre-existing `batchSize > 0` body (clamp, `GetRowsAt`, frame drop, `GetRowsAs<IEmailSortInfo>()`, `GetItemFromID` projection, `SetupWorker`/`RunWorkerAsync`) is textually unchanged below the new guard; `evidence/regression-testing/pre-fix-test-run.2026-07-06T15-45.md` and `post-fix-test-run.2026-07-06T15-45.md` both show `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` passing (this test is unaffected by the fix, as expected). | Test asserts both the 2-item result and that the source `_frame` is drained to `RowCount == 0`, matching the documented pre-existing contract. |
| AC4 | **PASS** | `evidence/regression-testing/fail-before-InitEmailQueue-zero-batch.2026-07-06T15-45.md`, `fail-before-InitEmailQueue-worker-start.2026-07-06T15-45.md` (red, reproducing the exact reported Deedle exception with no live UX/COM — `grep -c "MessageBox"` == 0); `post-fix-test-run.2026-07-06T15-45.md` (green, deterministic in both narrow-filter and full-suite runs, `grep -c "MessageBox"` == 0) | The `RemainingEmailLoader` injectable-delegate seam ensures every test that starts a real `BackgroundWorker` assigns an inert delegate before calling `InitEmailQueue`, independently confirmed by reading all three test methods in `QfcInitEmailQueueZeroBatchTests.cs`. |
| AC5 | **PASS** (on its own literal terms) | `evidence/qa-gates/qc-format.md`, `qc-lint.md`, `qc-nullable.md` (all exit 0); `qc-coverage.md` (472/472 tests passing; `QuickFiler` package coverage unchanged at 72.46%, 0.00 pp delta). Independently re-verified in this audit: `dotnet tool run csharpier check` on both touched `.cs` files → exit 0. | AC5's literal text ("full C# toolchain passes; changed-line coverage does not regress") is fully supported by this evidence. **Separately**, this review's own mandatory Coverage Verification procedure requires a canonical `artifacts/csharp/coverage.xml` artifact for any language with changed files; that artifact is absent from the repository. This is recorded as a distinct **BLOCKING policy finding** in `policy-audit.2026-07-06T12-48.md` §5 and `remediation-inputs.2026-07-06T12-48.md` — it does not indicate that AC5's own stated criterion (toolchain passes; changed-line coverage does not regress) is unmet, so AC5 is not downgraded here, but the feature is not clear for merge until the blocking policy finding is resolved. |

## Summary

All five acceptance criteria are supported by direct evidence and independent source/diff inspection performed during this audit. The fix is minimal, matches the confirmed root cause, and the v1.1 test revision correctly eliminated two self-identified defects (a live-UX/COM-triggering test and a flaky `worker.IsBusy` assertion) before this review. The feature is **not yet clear for merge**: a separate, review-level policy requirement (a canonical `artifacts/csharp/coverage.xml` artifact for the changed C# files) is unmet and is tracked as the sole blocking finding in `policy-audit.2026-07-06T12-48.md` and `remediation-inputs.2026-07-06T12-48.md`.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/issue.md`
- Total AC items: 5
- Checked off (delivered): 5 (AC1-AC5, already checked `[x]` by the executor at P3-T5; independently re-verified as PASS in this audit — no change made to `issue.md`)
- Remaining (unchecked): 0
- Items remaining: none

## Acceptance Criteria Check-off

All five acceptance criteria in `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/issue.md` were already marked `[x]` by the executor (task P3-T5, evidence mirrored at `evidence/issue-updates/issue-244.2026-07-06T15-45.md`) prior to this review. This audit independently re-verified each criterion against the source diff and regression-testing evidence and confirms all five check-offs are warranted; no reversal or additional check-off was required. No edits were made to `issue.md` by this review.
