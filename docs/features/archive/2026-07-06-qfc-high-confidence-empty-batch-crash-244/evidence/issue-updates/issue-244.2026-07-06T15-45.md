# Issue #244 Update Mirror — v1.1 Revision AC Check-Off

Timestamp: 2026-07-06T15-45

PostedAs: unknown (local `issue.md` update only; not posted to GitHub in this execution cycle)

## Exact text applied to `issue.md` `## Acceptance Criteria` section

- [x] AC1: `InitEmailQueue(0, worker)` returns an empty (non-null) `IList<MailItem>` and does not throw, regardless of `_frame` row count. Evidence: `evidence/regression-testing/fail-before-InitEmailQueue-zero-batch.2026-07-06T15-45.md` (red), `evidence/regression-testing/post-fix-test-run.2026-07-06T15-45.md` (green, narrow filter and full suite).
- [x] AC2: `InitEmailQueue(0, worker)` still sets up and starts the background worker so remaining emails are loaded to the master queue. Evidence: `evidence/regression-testing/fail-before-InitEmailQueue-worker-start.2026-07-06T15-45.md` (red), `evidence/regression-testing/post-fix-test-run.2026-07-06T15-45.md` (green, `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` passing deterministically in both the narrow-filter run and the full-suite run). The v1.1 revision replaces the raced `worker.IsBusy` assertion with `worker.WorkerSupportsCancellation` plus a bounded `TaskCompletionSource` wait on the injected `RemainingEmailLoader` seam, removing the narrow-filter/full-suite context dependency recorded against this criterion in the v1.0 evidence; no caveat remains.
- [x] AC3: `InitEmailQueue(batchSize, worker)` with `batchSize > 0` retains existing behavior (first-batch projection via `GetRowsAs<IEmailSortInfo>()` and source-frame drop). Evidence: `evidence/regression-testing/pre-fix-test-run.2026-07-06T15-45.md`, `evidence/regression-testing/post-fix-test-run.2026-07-06T15-45.md`.
- [x] AC4: A deterministic, Outlook-free regression test reproduces the failure on pre-fix code (red) and passes after the fix (green), with no live UX/COM in either state. Evidence: `evidence/regression-testing/fail-before-InitEmailQueue-zero-batch.2026-07-06T15-45.md`, `evidence/regression-testing/fail-before-InitEmailQueue-worker-start.2026-07-06T15-45.md` (red), `evidence/regression-testing/post-fix-test-run.2026-07-06T15-45.md` (green, deterministic in both the narrow-filter and full-suite runs). The v1.1 `RemainingEmailLoader` injectable-delegate seam (`QuickFiler/Controllers/QfcDatamodel.cs`) ensures every test that starts a real `BackgroundWorker` assigns an inert delegate before calling `InitEmailQueue`, so no test triggers `MessageBox.Show` or live Outlook COM in either the red or green state; no caveat remains.
- [x] AC5: Full C# toolchain (csharpier -> analyzers -> nullable -> MSTest) passes; changed-line coverage does not regress. Evidence: `evidence/qa-gates/qc-format.md`, `evidence/qa-gates/qc-lint.md`, `evidence/qa-gates/qc-nullable.md`, `evidence/qa-gates/qc-coverage.md` (QuickFiler package coverage unchanged at 72.46%, 0.00pp delta; 472/472 tests passing).

## Removed caveat text (v1.0 -> v1.1)

The v1.0 `issue.md` recorded two context-dependent caveats against AC2 and AC4, both referencing a
narrow-filter `worker.IsBusy` race in `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`. Both
caveats are removed in this v1.1 update: the seam-based fix (`RemainingEmailLoader`, P1-T2) plus the
rewritten assertion (`WorkerSupportsCancellation` + bounded `TaskCompletionSource` wait, P1-T4) make
all three regression tests deterministically green in every run context verified in this cycle
(narrow filter and full suite, each run at least twice).
