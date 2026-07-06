# qfc-high-confidence-empty-batch-crash (Issue #244)

- Date captured: 2026-07-06
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-high-confidence-empty-batch-crash/ (Issue #244)
- Type: Bug

- Issue: #244
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/244
- Last Updated: 2026-07-06
- Work Mode: minor-audit

## Problem / Why

Launching QuickFiler High Confidence crashes with a Deedle exception:

```
System.Exception: The interface member 'EntryId' does not exist in the column index.
  Source=FSharp.Core (Deedle)
  at Deedle.Frame`2.GetRowsAs[TRow]()
  at QuickFiler.Controllers.QfcDatamodel.InitEmailQueue(Int32 batchSize, BackgroundWorker worker) in QfcDatamodel.cs:line 225
```

### Confirmed root cause

In High Confidence mode, `QfcHomeController.RunAsync` sets the initialization batch size to `0`
(`QuickFiler/Controllers/QfcHomeController.cs:281` — `int initializationBatchSize = highConfidenceModeEnabled ? 0 : itemsPerIteration;`)
and passes it to `QfcDatamodel.InitEmailQueueAsync` -> `InitEmailQueue`.

`InitEmailQueue` (`QuickFiler/Controllers/QfcDatamodel.cs:211`) has no `batchSize == 0` guard:
- Line 216 clamps `batchSize` to `0`.
- Line 217 evaluates `_frame.GetRowsAt(new int[0])`, which Deedle reconstructs into a frame with an **empty column index** (zero selected rows).
- Line 225 `firstIteration.GetRowsAs<IEmailSortInfo>()` validates the interface members against that empty index and throws on the first member, `EntryId`.

The underlying `_frame` is well-formed and retains a correctly-cased `EntryId` column; standard (non-High-Confidence) mode passes `batchSize > 0` through the same `GetRowsAs<IEmailSortInfo>()` call and works. The sibling method `LoadRemainingEmailsToQueueAsync` already guards `_frame.RowCount == 0` before its own `GetRowsAs` call; `InitEmailQueue` lacks the equivalent zero-batch guard. This is a regression introduced by the High-Confidence streaming path.

Diagnosis artifact: `docs/research/2026-07-06-quickfiler-entryid-column-index-diagnosis.md`.

## Proposed Behavior

In High Confidence mode (or any `batchSize <= 0` initialization call), `InitEmailQueue` must not attempt to project an empty batch through `GetRowsAs<IEmailSortInfo>()`. It must return an empty initial email list while still setting up and starting the background worker that streams the remaining emails into the master queue (which the High-Confidence dequeue gate then consumes).

## Acceptance Criteria

- [x] AC1: `InitEmailQueue(0, worker)` returns an empty (non-null) `IList<MailItem>` and does not throw, regardless of `_frame` row count. Evidence: `evidence/regression-testing/fail-before-InitEmailQueue-zero-batch.2026-07-06T15-45.md` (red), `evidence/regression-testing/post-fix-test-run.2026-07-06T15-45.md` (green, narrow filter and full suite).
- [x] AC2: `InitEmailQueue(0, worker)` still sets up and starts the background worker so remaining emails are loaded to the master queue. Evidence: `evidence/regression-testing/fail-before-InitEmailQueue-worker-start.2026-07-06T15-45.md` (red), `evidence/regression-testing/post-fix-test-run.2026-07-06T15-45.md` (green, `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` passing deterministically in both the narrow-filter run and the full-suite run). The v1.1 revision replaces the raced `worker.IsBusy` assertion with `worker.WorkerSupportsCancellation` plus a bounded `TaskCompletionSource` wait on the injected `RemainingEmailLoader` seam, removing the narrow-filter/full-suite context dependency recorded against this criterion in the v1.0 evidence; no caveat remains.
- [x] AC3: `InitEmailQueue(batchSize, worker)` with `batchSize > 0` retains existing behavior (first-batch projection via `GetRowsAs<IEmailSortInfo>()` and source-frame drop). Evidence: `evidence/regression-testing/pre-fix-test-run.2026-07-06T15-45.md`, `evidence/regression-testing/post-fix-test-run.2026-07-06T15-45.md`.
- [x] AC4: A deterministic, Outlook-free regression test reproduces the failure on pre-fix code (red) and passes after the fix (green), with no live UX/COM in either state. Evidence: `evidence/regression-testing/fail-before-InitEmailQueue-zero-batch.2026-07-06T15-45.md`, `evidence/regression-testing/fail-before-InitEmailQueue-worker-start.2026-07-06T15-45.md` (red), `evidence/regression-testing/post-fix-test-run.2026-07-06T15-45.md` (green, deterministic in both the narrow-filter and full-suite runs). The v1.1 `RemainingEmailLoader` injectable-delegate seam (`QuickFiler/Controllers/QfcDatamodel.cs`) ensures every test that starts a real `BackgroundWorker` assigns an inert delegate before calling `InitEmailQueue`, so no test triggers `MessageBox.Show` or live Outlook COM in either the red or green state; no caveat remains.
- [x] AC5: Full C# toolchain (csharpier -> analyzers -> nullable -> MSTest) passes; changed-line coverage does not regress. Evidence: `evidence/qa-gates/qc-format.md`, `evidence/qa-gates/qc-lint.md`, `evidence/qa-gates/qc-nullable.md`, `evidence/qa-gates/qc-coverage.md` (QuickFiler package coverage unchanged at 72.46%, 0.00pp delta; 472/472 tests passing).

## Constraints & Risks

- Fix must be minimal and targeted (bugfix workflow): guard the zero-batch case; no broad refactor of the frame pipeline.
- `QfcDatamodel` is currently `[ExcludeFromCodeCoverage]`; the testable slice-and-project logic should be reachable by a deterministic MSTest without a live Outlook Table (internals are exposed via `InternalsVisibleTo("QuickFiler.Test")`).
- Do not change `QfcHomeController` batch-size intent (empty initial batch in High-Confidence mode is by design).

## Test Conditions to Consider

- [ ] `batchSize == 0` with a non-empty frame (High-Confidence path).
- [ ] `batchSize > 0` positive path unchanged.
- [ ] Worker setup/start still invoked in the zero-batch case.

## Next Step

- [x] Promote to GitHub issue (bug template) — Issue #244
- [x] Create active feature folder from the template
- [ ] Minimal atomic plan
- [ ] Implement fix + regression test
- [ ] Feature review (minor-audit) and PR
