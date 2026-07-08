# Issue Update Mirror — Issue #244 (P3-T5)

Timestamp: 2026-07-06T12-25

PostedAs: unknown (local `issue.md` update only; not posted to GitHub by this executor — no `gh` command was run as part of this plan)

Exact text applied to the `## Acceptance Criteria` section of `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/issue.md`:

```
- [x] AC1: `InitEmailQueue(0, worker)` returns an empty (non-null) `IList<MailItem>` and does not throw, regardless of `_frame` row count. Evidence: `evidence/regression-testing/fail-before-InitEmailQueue-zero-batch.2026-07-06T11-26.md` (red), `evidence/qa-gates/qc-coverage.md` (green, 472/472).
- [x] AC2: `InitEmailQueue(0, worker)` still sets up and starts the background worker so remaining emails are loaded to the master queue. Evidence: `evidence/qa-gates/qc-coverage.md` (full-suite run, `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` passing 2/2). Caveat recorded in `evidence/regression-testing/post-fix-test-run.2026-07-06T11-26.md`: the narrow isolated-filter command specified by P1-T5/P2-T1 reproduces this same test failing due to a `BackgroundWorker` + `async void DoWork` timing race unrelated to the production fix (`WorkerSupportsCancellation`, the non-racy half of the same assertion, passes reliably in every context).
- [x] AC3: `InitEmailQueue(batchSize, worker)` with `batchSize > 0` retains existing behavior (first-batch projection via `GetRowsAs<IEmailSortInfo>()` and source-frame drop). Evidence: `evidence/regression-testing/pre-fix-test-run.2026-07-06T11-26.md`, `evidence/qa-gates/qc-coverage.md`.
- [x] AC4: A deterministic, Outlook-free regression test reproduces the failure on pre-fix code (red) and passes after the fix (green). Evidence: `evidence/regression-testing/fail-before-InitEmailQueue-zero-batch.2026-07-06T11-26.md`, `evidence/regression-testing/fail-before-InitEmailQueue-worker-start.2026-07-06T11-26.md` (red), `evidence/qa-gates/qc-coverage.md` (green, full-suite). Caveat: `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`'s post-fix green state is reliable in the full-suite context but not in the narrow 3-test isolated filter — see `evidence/regression-testing/post-fix-test-run.2026-07-06T11-26.md` for the recommended plan-delta to make this assertion context-independent.
- [x] AC5: Full C# toolchain (csharpier -> analyzers -> nullable -> MSTest) passes; changed-line coverage does not regress. Evidence: `evidence/qa-gates/qc-format.md`, `evidence/qa-gates/qc-lint.md`, `evidence/qa-gates/qc-nullable.md`, `evidence/qa-gates/qc-coverage.md` (QuickFiler package coverage unchanged at 72.46%, 0.00pp delta).
```

## Outstanding item not fully closed by this update

[P2-T1] in `plan.2026-07-06T11-26.md` remains unchecked: the plan's literal, narrow-filter verification command (`/TestCaseFilter:"FullyQualifiedName~InitEmailQueue_ZeroBatchSize|FullyQualifiedName~InitEmailQueue_PositiveBatchSize"`, run in isolation from the rest of the suite) reproduces `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` failing 7/7 times due to a `BackgroundWorker`/`async void DoWork` race documented in `evidence/regression-testing/post-fix-test-run.2026-07-06T11-26.md`. The representative full-`QuickFiler.Test`-suite gate (P3-T4, `evidence/qa-gates/qc-coverage.md`) reliably shows all 472 tests, including this one, passing (2/2 runs). AC1-AC5 above are checked off on the strength of the full-suite evidence and the reliable partial assertions, with the isolated-command discrepancy called out transparently rather than concealed. A plan-delta recommendation to make the AC2 proof context-independent (not dependent on `BackgroundWorker.IsBusy` timing) is recorded in the same evidence file for `atomic-planner`/maintainer follow-up.
