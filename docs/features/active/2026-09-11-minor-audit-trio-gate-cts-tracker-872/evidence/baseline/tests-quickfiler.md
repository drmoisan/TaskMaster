# Phase 0 — QuickFiler Test Assembly Baseline

Timestamp: 2026-09-13T15-01
Task: [P0-T9]

Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/Logger:trx;LogFileName=p0-t9-baseline-quickfiler.trx" /ResultsDirectory:TestResults\vstest\p0-t9
EXIT_CODE: 0

TotalTests: 1395
Passed: 1395
Failed: 0
Skipped: 0

Output Summary: the run printed the success header `Test Run Successful.`, a `Total tests: 1395` line
and a `Passed: 1395` line, and exited 0 after 12.6616 seconds. The passed count equals the total count.
This `TotalTests:` value is the AC11 baseline for the QuickFiler test assembly.

## Transcription Of The Two Zero Counters, Per D8

A fully green run on this toolchain prints the success header, a total line and a passed line, and
prints no `Failed:` line and no `Skipped:` line. The two zero values above are transcribed on that
basis and corroborated by the success header and by the passed count equalling the total count.

## Re-Run Note And Comparison With The Superseded Figure, Per D15

This artifact overwrites a superseded capture taken before the main branch was merged into this
branch. The superseded capture recorded `TotalTests: 1394`. The re-measured figure is 1395, one
higher, and the cause is identified rather than assumed: the merge brought in one added test method in
`QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`, named
Init_CreatesTokenSourceBeforeAnyLoaderObservesIt, which is a regression test for issue #839 pinning
that the home controller creates its cancellation token source before any loader observes it. An
anchored name-status diff between the superseded base commit and the current base commit lists that
file as modified with 71 added lines and 0 removed lines, and the added block is exactly that one test
method. No other test file in this assembly was added or removed by the merge.

The operative AC11 baseline for this assembly is therefore 1395 and not 1394. The AC11 delta
arithmetic is unaffected by the shift, because it asserts the difference between a Phase 2 figure and
a Phase 0 figure measured on the same tree rather than either absolute value.

No test failed in this run. No failure was observed here that the previous Phase 0 run did not see,
because no failure was observed at all. In particular the three failures in the zero-batch email-queue
test class that D14 records under the MSTest runsettings parallelism did not appear: this span passes
no settings file.

## Population, Per D5

The pinned test-case filter is byte-identical to the P0-T8 filter and to the filter P2-T6 will use.
Per D14 no MSTest runsettings file is passed to this span.

## Artifact Retention

Per D10 the TRX is written to `TestResults/vstest/p0-t9/p0-t9-baseline-quickfiler.trx`, which resolves
to the git-ignore pattern for the results directory class, and is never committed. The span sets both
an explicit results directory and an explicit log file name. The run printed an overwrite warning for
that file, which is the expected consequence of re-running a task whose log file name is fixed.
