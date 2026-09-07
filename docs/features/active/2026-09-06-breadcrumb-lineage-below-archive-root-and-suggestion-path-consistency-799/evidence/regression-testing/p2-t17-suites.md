# [P2-T17] Whole-assembly post-change suite run

Timestamp: 2026-09-07T07-39

Command:

```
<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:TestResults\799-p2-t17-ut /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests
<vstest> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:TestResults\799-p2-t17-qft /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:TestCategory!=LiveOutlook
```

`<vstest>` is the vswhere-resolved vstest.console.exe path pinned by [P0-T7].

EXIT_CODE: 0

ExpectedExitCode: 0

EXIT-CODE-UT: 0

EXIT-CODE-QFT: 0

POST-UT-TOTAL: 4816

POST-UT-PASSED: 4816

POST-UT-FAILED: 0

POST-QFT-TOTAL: 1370

POST-QFT-PASSED: 1370

POST-QFT-FAILED: 0

NEWLY-FAILING: NONE

## Output Summary

Both invocations printed `Test Run Successful.` Every `POST-*-FAILED` value is read from its run's
TRX `ResultSummary/Counters` `failed` attribute and not from the console, because vstest prints no
`Failed:` line at all on a fully passing run.

Comparison against the [P0-T11] baseline:

| Counter | Baseline | Post-change | Verdict |
|---|---|---|---|
| UT total | 4786 | 4816 | +30, the four new UtilitiesCS.Test classes (11 + 7 + 8 + 4) |
| UT passed | 4786 | 4816 | +30 |
| UT failed | 0 | 0 | `POST-UT-FAILED` <= `BASELINE-UT-FAILED` — met |
| QFT total | 1363 | 1370 | +7, the new QuickFiler.Test score-join class |
| QFT passed | 1363 | 1370 | +7 |
| QFT failed | 0 | 0 | `POST-QFT-FAILED` <= `BASELINE-QFT-FAILED` — met |

`NEWLY-FAILING: NONE` is derivable directly from the counters: zero tests failed in either run, so
the set of tests failing here that were not failing in the baseline is empty.

## Suite selection (R13)

The UtilitiesCS.Test run carries the same four `FullyQualifiedName!~` shell-icon exclusions the
[P0-T11] baseline carried — `HelperClasses.ShellUtilities_Tests`,
`HelperClasses.ShellUtilitiesStatic_Tests`, `HelperClasses.SysImageListHelperTests` and
`EmailIntelligence.OSBrowser_Tests` — so the reduced denominator is identical on both sides of the
comparison above. Those four classes call SHGetFileInfo and stall vstest on this machine.

## TRX documents read (R3-reduced names)

- UtilitiesCS.Test: `TestResults\799-p2-t17-ut\<user>_<host>_2026-09-07_07_38_28_net481.trx`
- QuickFiler.Test: `TestResults\799-p2-t17-qft\<user>_<host>_2026-09-07_07_39_09_net481.trx`

Each results directory held exactly one TRX at read time.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact. No TRX content
was pasted: only parsed counter values were read.
