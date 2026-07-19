# Final QC — Coverage Delta and Changed-Line No-Regression Check

- Timestamp: 2026-07-19T11-05
- Task: [P9-T6]
- Inputs: baseline Cobertura `evidence/baseline/coverage-baseline.2026-07-19T08-51.cobertura.xml` (P0-T5) and post-change Cobertura `evidence/qa-gates/final-coverage.2026-07-19T11-05.cobertura.xml` (P9-T4).

## Overall (root `<coverage>`)

| Metric | Baseline | Post-change | Delta |
|---|---|---|---|
| line-rate | 0.7206711694864292 (72.07%) | 0.7206925124474786 (72.07%) | +0.0000214 |
| branch-rate | 0.48442797445399355 (48.44%) | 0.48442797445399355 (48.44%) | +0.0000000 |
| lines covered/valid | 98270 / 136359 | 98283 / 136373 | +13 / +14 |

## Targeted production `UtilitiesCS/Threading/` aggregate

| Metric | Baseline | Post-change | Delta |
|---|---|---|---|
| line-rate | 3855/4710 = 0.8185 (81.85%) | 3890/4748 = 0.8193 (81.93%) | +0.0008 |

Aggregate production Threading line coverage **increased** (+0.08 pp); covered line count rose (3855 -> 3890). No overall regression.

## Per-production-file delta (25 files)

All files flat or improved EXCEPT one per-file rate movement on `ProgressTrackerPane.cs`:

- `ApplicationIdleTimer.cs` 0.881 -> 0.882 (+); `ProgressTracker.cs` 0.866 -> 0.871 (+); `ProgressTrackerAsync.cs` 0.883 -> 0.893 (+); `StoreLockupResponder.cs` 0.941 -> 0.943 (+); `UiThread.cs` 0.740 -> 0.744 (+); `SyncContextForm.cs` 1.000 -> 1.000 (=); all others unchanged.
- `ProgressTrackerPane.cs`: baseline 178/234 = 0.761 -> post-change 180/239 = 0.753 (per-file rate -0.0075).

## Changed-Line No-Regression Conclusion: PASS

The edits are annotation-only and preferred `?` / `= null!` / justified `!` over new `if (x is null)` guards; **no new runtime guard / branch was added in any file**. No previously-covered executable line became uncovered:

- The `ProgressTrackerPane.cs` per-file rate dip is a benign line-counting artifact, not a coverage regression on a changed line. Its **covered** line count INCREASED (178 -> 180). Uncovered-line analysis confirms the newly-instrumented lines fall entirely inside the `ProgressTrackerPane` **constructor**, which is uncovered at baseline in the headless test environment because it requires a live `UiThread.Dispatcher` (a pre-existing coverage gap unrelated to this feature). The `_jobName = null!` field initializer (an executable line) and CSharpier reflow of the constructor's justified-`!` dereference expressions added instrumentable lines within that already-uncovered constructor, growing the denominator by 5 while adding 2 covered lines.
- `_jobName = null!` was verified to be required: annotating `_jobName` as `string?` instead produces CS8620 at the `_parent.Progress.Report((parentProgress, _jobName))` site because the concrete `IProgress<(int Value, string JobName)>` tuple type argument is non-oblivious. `= null!` preserves the non-null `JobName` tuple contract and is behavior-identical.
- No changed line went from covered to uncovered in any file. Aggregate Threading and overall coverage did not regress (both flat-to-up).

Outcome: **PASS** (no coverage regression on changed lines).
