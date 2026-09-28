# [P0-T13] Regression-sweep baseline (TaskMaster.Test, no coverage)

- Issue: #792
- Timestamp: 2026-09-17T18-47
- Command: CMD-VSTEST (`vstest.console.exe` resolved through `vswhere -latest -products * -find 'Common7/IDE/Extensions/TestPlatform/vstest.console.exe'`), then `& $vstest TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" "/ResultsDirectory:coverage/test-results/p0-t13" "/Logger:trx;LogFileName=p0-t13.trx"` (CMD-SWEEP with `<task>` = `p0-t13`; run from `coverage/plan792-helper.ps1` with the item worktree as the working directory; console captured to the gitignored `coverage/p0-t13-sweep.log`; the TRX stays under the gitignored `coverage/test-results/p0-t13/`)
- EXIT_CODE: 0
- Output Summary: `Test Run Successful.`; `Total tests: 452`; `Passed: 452`; `Failed: 0 (omitted category)`; `Skipped: 0 (omitted category)`; `Total time: 2.6932 Seconds`; 0 lines beginning `Failed `; no Blame hang output (the run completed with a summary well inside the 4-minute per-test timeout).

## Console transcription

- Total tests: 452
- Passed: 452
- Failed: 0 (omitted category)
- Skipped: 0 (omitted category)

BASELINE_FAILURE_SET: none

## Notes

- The assembly under test was produced by the [P0-T11] nullable Rebuild (`TaskMaster.Test/bin/Debug/TaskMaster.Test.dll`, written 18:42:49 local time); no build ran in this task.
- `scripts/vscode/TaskMaster.cli.runsettings` was passed unchanged (Workers=0, ClassLevel).
