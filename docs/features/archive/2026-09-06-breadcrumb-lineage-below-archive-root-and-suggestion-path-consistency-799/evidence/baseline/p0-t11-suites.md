# [P0-T11] UtilitiesCS.Test and QuickFiler.Test baseline runs

Timestamp: 2026-09-07T07-05

Command: & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\799-p0-t11-ut' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
(then) & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\799-p0-t11-qft' '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook'

EXIT_CODE: 0

EXIT-CODE-UT: 0
EXIT-CODE-QFT: 0

The single `EXIT_CODE:` field is the larger of the two invocation exit codes, both of which are 0.

## Derived counters (read from TRX `ResultSummary/Counters`)

BASELINE-UT-TOTAL: 4786
BASELINE-UT-PASSED: 4786
BASELINE-UT-FAILED: 0
BASELINE-QFT-TOTAL: 1363
BASELINE-QFT-PASSED: 1363
BASELINE-QFT-FAILED: 0

Each `BASELINE-*-FAILED` value was read from its run's TRX `ResultSummary/Counters` `failed` attribute, not from
the console, because vstest prints no `Failed:` line at all on a fully passing run.

## TRX documents read

- UtilitiesCS.Test: `TestResults\799-p0-t11-ut\<user>_<host>_2026-09-07_06_44_06_net481.trx` — the only TRX in
  that results directory, so no most-recently-modified selection was needed.
- QuickFiler.Test: `TestResults\799-p0-t11-qft\<user>_<host>_2026-09-07_06_46_42_net481.trx` — the only TRX in
  that results directory (count verified as 1), so no most-recently-modified selection was needed.

No TRX content is pasted into this artifact (R3); only parsed counter values are recorded. TRX filenames are
reduced per R3: the `runUser` and `computerName` segments in the generated file names are replaced with `<user>`
and `<host>`.

## Excluded classes (R13)

The UtilitiesCS.Test run excludes the four shell-icon classes that stall vstest on this machine, through
`FullyQualifiedName!~` clauses:

1. HelperClasses.ShellUtilities_Tests
2. HelperClasses.ShellUtilitiesStatic_Tests
3. HelperClasses.SysImageListHelperTests
4. EmailIntelligence.OSBrowser_Tests

Both runs additionally exclude `TestCategory=LiveOutlook`. The QuickFiler.Test run carries no shell-icon clause,
because those classes live in UtilitiesCS.Test. The reduced denominator recorded here is the same denominator the
Phase 2 and Phase 3 comparisons use.

Output Summary: Both baseline suites are fully green at the base commit. UtilitiesCS.Test ran 4786 tests with 4786
passed and 0 failed in 32.5 seconds; QuickFiler.Test ran 1363 tests with 1363 passed and 0 failed in 13.4 seconds.
Both invocations exited 0 and both printed `Test Run Successful.`. No pre-existing failure has to be carried into
the Phase 2 no-newly-failing comparison.
