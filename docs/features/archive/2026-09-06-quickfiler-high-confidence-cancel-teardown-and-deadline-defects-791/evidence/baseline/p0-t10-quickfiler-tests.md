# [P0-T10] QuickFiler.Test baseline run

Timestamp: 2026-09-06T14-27

Command:

```
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\791-p0-t10' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook'
```

`$vstest` was re-bound inside this command block by the two R10 resolution lines. The resolved value
reduced per R3 is `<program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.

EXIT_CODE: 0

BASELINE-QFT-TOTAL: 1339
BASELINE-QFT-PASSED: 1339
BASELINE-QFT-FAILED: 0

Output Summary: `Test Run Successful. Total tests: 1339, Passed: 1339, Total time: 13.2586 Seconds.`
The three derived lines above are read from the TRX `ResultSummary/Counters` element
(`total`, `passed`, `failed`), and the run's `ResultSummary/outcome` attribute is `Completed`. No
raw TRX content is reproduced here: a TRX carries `runUser` and `computerName` attributes and its
default filename embeds both, which R3 forbids in an artifact. The results directory
`TestResults/` is git-ignored at `.gitignore` line 39, so the TRX is a local run output and is not
committed.

`BASELINE-QFT-FAILED: 0` is the value [P2-T15] compares against: `POST-QFT-FAILED` must be less than
or equal to it, and `NEWLY-FAILING` must be `NONE`. Because the baseline failure set is empty, any
failure in [P2-T15] is newly failing by construction.

`/InIsolation` is present because a shared test host in this worktree loads assemblies from sibling
worktrees; the run is scoped to a single explicitly named assembly, so no `.claude` worktree path is
enumerated (D15). No shell-icon exclusion clause is required here, because the four hanging classes
live in `UtilitiesCS.Test`, which this run does not load.
