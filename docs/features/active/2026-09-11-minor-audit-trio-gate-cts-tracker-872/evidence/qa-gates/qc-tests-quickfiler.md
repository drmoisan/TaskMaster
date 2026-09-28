# Phase 2 — QuickFiler Test Assembly Run

Timestamp: 2026-09-13T15-36
Task: [P2-T6]

Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/Logger:trx;LogFileName=p2-t6-final-quickfiler.trx" /ResultsDirectory:TestResults\vstest\p2-t6
EXIT_CODE: 0

TotalTests: 1397
Passed: 1397
Failed: 0
Skipped: 0

Output Summary: the run printed the success header `Test Run Successful.`, a `Total tests: 1397` line
and a `     Passed: 1397` line, and exited 0 after 13.8748 seconds. The passed count equals the total
count. Measured against the P0-T9 baseline of 1395 the difference is exactly plus two, which is the
AC11 expectation for this assembly and corresponds to the two tests added by P1-T2 and P1-T3.

## Transcription Of The Two Zero Counters, Per D8

A fully green run on this toolchain prints no `Failed:` line and no `Skipped:` line at all, so the two
zero values above are transcribed rather than quoted. The transcription is corroborated by the success
header and by the passed count equalling the total count.

## Filter Comparability, Per D5

The `/TestCaseFilter:` value is byte-identical to the value P0-T9 recorded. It was supplied as a single
argument built by concatenating the switch name with a single-quoted PowerShell literal, so no shell
quoting could alter it. Per D14 no MSTest runsettings file is passed to this span; the three zero-batch
email-queue failures that D14 describes arise only under the runsettings file's ClassLevel parallelism
and did not occur here.

## No Unexplained Failure

Every discovered test passed. In particular the QuickFiler zero-batch email-queue class, which was the
locus of the netstandard 2.1 Deedle type-initialization failure before pull request 880, produced no
failure in this run. Nothing was worked around and no test was adjusted.

## Artifact Retention

Per D10 the TRX is written to `TestResults/vstest/p2-t6/p2-t6-final-quickfiler.trx`, a path under the
git-ignored results directory class, and is never committed. Both an explicit results directory and an
explicit log file name are set, so the default account-and-host TRX name is never produced. P2-T15 and
P2-T16 read this TRX in place.

The build lock was acquired immediately before this invocation and released immediately after it
returned. Outlook was verified not running.
