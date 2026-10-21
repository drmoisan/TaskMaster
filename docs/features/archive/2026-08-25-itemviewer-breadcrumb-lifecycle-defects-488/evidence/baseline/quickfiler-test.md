# Phase 0 — Baseline `QuickFiler.Test` Result ([P0-T12])

Timestamp: 2026-08-28T05-16

Command (run under `pwsh -NoProfile` from the worktree root, with the vstest path resolved in
`[P0-T4]`):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/Logger:trx;LogFileName=baseline-quickfiler-test.trx" /ResultsDirectory:docs\features\active\itemviewer-breadcrumb-lifecycle-defects-488\evidence\baseline\trx-p0-t12
```

EXIT_CODE: 0

## Counts

| Measure | Value |
| --- | --- |
| Total | **1192** |
| **BASELINE_PASSED** | **1192** |
| Failed | **0** |
| Skipped / not executed | **0** |
| Total time | 9.3518 seconds |
| Run result | `Test Run Successful.` |

The counts above are corroborated by the TRX `ResultSummary/Counters` element, which records
`total="1192" executed="1192" passed="1192" failed="0" notExecuted="0"`. The console summary and the
TRX agree.

## BASELINE_FAILURE_SET

```
(empty)
```

**No test in `QuickFiler.Test` fails at the baseline.** The verbatim failing-test-name set is empty.

## Consequences of the empty failure set

- `[P4-T8]`'s subset condition ("the observed failing set is a subset of `BASELINE_FAILURE_SET`")
  reduces to: the observed failing set must be empty. Any test failing after D4's affinity guard lands
  is by definition outside the baseline set and triggers the constraint C6 escalation.
- `[P8-T5]`'s subset condition likewise reduces to failed count 0 with `EXIT_CODE: 0`, and its passed
  count must be at least `BASELINE_PASSED + 9` = **1201**, the nine being ten added test methods minus
  one deleted test method.
- `[P9-T9]`'s criterion asserts an absolute zero failures. Because the baseline set is empty, that
  criterion is reachable and its "leave unchecked and report" branch is not triggered.

## TRX artifact

`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/baseline/trx-p0-t12/baseline-quickfiler-test.trx`
is present, 1,705,281 bytes. `LogFileName=` was supplied explicitly so that vstest did not name the
TRX after the host account and machine, which its default naming would have done.

## Note on the absence of a category filter

This command carries no `/TestCaseFilter:`. That is correct for this assembly: a repository search of
`QuickFiler.Test` for the fixed string `LiveOutlook` returns **zero** files, so the assembly contains
no test that would launch a real Outlook process. The `TestCategory!=LiveOutlook` filter that the
repository-wide runner applies is required for the aggregate run in `[P0-T14]` and `[P8-T6]`, which
covers assemblies that do carry that category, and is applied there by
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`.

Output Summary: EXIT_CODE 0. 1192 total, **1192 passed**, **0 failed**, 0 skipped, in 9.35 seconds.
`BASELINE_FAILURE_SET` is empty; `BASELINE_PASSED` is 1192. The named TRX is present in the task's own
results directory.
