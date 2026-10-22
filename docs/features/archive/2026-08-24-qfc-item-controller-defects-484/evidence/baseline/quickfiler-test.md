# Phase 0 — Baseline QuickFiler.Test Result

Timestamp: 2026-08-26T08-35
Task: [P0-T12]

Command (run under `pwsh -NoProfile` with the `[P0-T4]`-resolved `vstest.console.exe`):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/Logger:trx;LogFileName=baseline-quickfiler-test.trx" /ResultsDirectory:docs\features\active\qfc-item-controller-defects-484\evidence\baseline\trx-baseline
```

EXIT_CODE: 0

## Counts

| Metric | Value |
|---|---|
| Total | **938** |
| Passed | **938** |
| Failed | **0** |
| Skipped | **0** |

**BASELINE_PASSED = 938.** This is the figure `[P7-T5]` compares against.

## Log summary

```
Results File: <repo-root>\docs\features\active\qfc-item-controller-defects-484\evidence\baseline\trx-baseline\baseline-quickfiler-test.trx

Test Run Successful.
Total tests: 938
     Passed: 938
 Total time: 24.9536 Seconds
```

## TRX artifact

`docs/features/active/qfc-item-controller-defects-484/evidence/baseline/trx-baseline/baseline-quickfiler-test.trx`
exists. An explicit `LogFileName=` was supplied so the TRX name carries no host account name or machine
name.

Output Summary: The `QuickFiler.Test` assembly is fully green at the baseline: 938 total, 938 passed, 0
failed, 0 skipped, exit code 0.
