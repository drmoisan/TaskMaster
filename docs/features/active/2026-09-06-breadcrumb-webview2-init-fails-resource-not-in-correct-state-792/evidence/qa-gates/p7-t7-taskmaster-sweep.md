# [P7-T7] Regression sweep (TaskMaster.Test, no coverage)

- Issue: #792
- Timestamp: 2026-09-17T21-12
- PASS-NUMBER: 1
- Command: CMD-VSTEST (`vstest.console.exe` resolved through `vswhere -latest -products * -find 'Common7/IDE/Extensions/TestPlatform/vstest.console.exe'`, `VSTEST-RESOLVED: True`), then `& $vstest TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" "/ResultsDirectory:coverage/test-results/p7-t7" "/Logger:trx;LogFileName=p7-t7.trx"` (CMD-SWEEP with `<task>` = `p7-t7`; run from `coverage/plan792-helper.ps1 -Step sweep -PassNumber 1` with the item worktree as the working directory; the helper's opening branch assertion `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` is the worktree proof and passed; HEAD `c9b457bda44ef856306a1bc96c94683dc528993c`; console captured to the gitignored `coverage/p7-t7-pass1-sweep.log`; the TRX stays under the gitignored `coverage/test-results/p7-t7/`, `TRX-EXISTS: True`)
- EXIT_CODE: 0
- Output Summary: `Test Run Successful.`; `Total tests: 452`; `Passed: 452`; `Failed: 0 (omitted category)`; `Skipped: 0 (omitted category)`; `Total time: 2.6925 Seconds`; 0 lines beginning `Failed ` (a test name); no Blame hang output; the run completed with a summary well inside the 4-minute per-test timeout.

## Console transcription

- Total tests: 452
- Passed: 452
- Failed: 0 (omitted category)
- Skipped: 0 (omitted category)

FAILED-SET: (empty)

NEW-FAILURES: none (the empty failed set is a subset of `BASELINE_FAILURE_SET: none` from [P0-T13]; the baseline also ran 452 tests with 452 passed)

## Hang check

A first, deliberately broad pattern (`hang|Hang dump|The active test run was aborted`) matched 11 log lines; every one is a `Passed <name>` line whose test name contains the word `Unchanged` (for example `SetHighConfidenceThresholdText_WithNonNumericInput_LeavesValueUnchanged`), that is the substring `hang` inside `Unchanged`. The strict pattern `Hang dump|The active test run was aborted|test host process crashed` matched 0 lines (`STRICT-HANG: 0`). No hang dump was produced.

## Notes

- The assembly under test was produced by the [P7-T5] nullable Rebuild of this pass (`TaskMaster.Test/bin/Debug/TaskMaster.Test.dll`, written 21:08:11 local time; unchanged between the before and after checks of this task); no build ran in this task.
- `scripts/vscode/TaskMaster.cli.runsettings` was passed unchanged (Workers=0, ClassLevel).
