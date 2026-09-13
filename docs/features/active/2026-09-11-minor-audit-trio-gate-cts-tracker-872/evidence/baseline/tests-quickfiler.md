# Phase 0 — Baseline Executed-Test Count, QuickFiler Test Assembly

Timestamp: 2026-09-13T05-09
Task: [P0-T9]

Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/Logger:trx;LogFileName=p0-t9-baseline-quickfiler.trx" /ResultsDirectory:TestResults\vstest\p0-t9
EXIT_CODE: 0
TotalTests: 1394
Passed: 1394
Failed: 0
Skipped: 0

vstest.console.exe was resolved through vswhere at the explicit installer path, per D1. The test-case
filter is byte-identical to the one used in P0-T8 and to the one Phase 2 will use. Per D14 no
runsettings file was passed.

This `TotalTests:` value, 1394, is the AC11 baseline for the QuickFiler test assembly. AC11 requires the
post-change count to be exactly plus two against it, being the tests added by AC1 and AC2, so the AC11
expectation for Phase 2 is 1396.

Output Summary: the run was fully green. The printed success header reads `Test Run Successful.`, the
total line reads `Total tests: 1394`, the passed line reads `Passed: 1394`, and the run completed in
`Total time: 13.6733 Seconds`. The passed count equals the total count.

## Failure And Skip Counters, Per D8

The run printed no `Failed:` line and no `Skipped:` line, so both are transcribed as 0 on the strength of
the success header and of the passed count equalling the total count. The reasoning is identical to
P0-T8 and is stated there in full.

## D14 Confirmed: No Deedle Type-Initialization Failure

D14 records that under the MSTest runsettings file's class-level parallelism three tests in the
QuickFiler zero-batch email-queue test class fail with a type-initialization exception for the Deedle
reflection type, and that the same command with only that switch removed measured 1393 of 1393 passed at
exit 0. This run passed no runsettings file and reproduced no such failure, which confirms D14's
diagnosis from the passing side.

The observed total, 1394, is one higher than the 1393 D14 records. That is not a divergence against any
acceptance value: D14's figure is a measurement of the tree at the time the decision was recorded, and
the mandated reconciliation merge of the current main branch into this item's branch has since added one
test to this assembly. The same merge raised each project's Compile-item count by one, which P0-T12
records. No acceptance condition in this plan cites 1393; AC11 is a delta against the baseline captured
here, and the operative baseline is the 1394 measured above.

## Transient Output, Per D10

The TRX was written to the git-ignored results directory under an explicit results directory and an
explicit log file name. It is not committed and no absolute host path from it is transcribed here.

## Build Lock

The cross-item build lock was held across the vstest invocation only. Acquired 2026-09-13T05:08:37,
released 2026-09-13T05:09:04.
