# Phase 2 — UtilitiesCS Test Assembly Run

Timestamp: 2026-09-13T15-35
Task: [P2-T5]

Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/Logger:trx;LogFileName=p2-t5-final-utilitiescs.trx" /ResultsDirectory:TestResults\vstest\p2-t5
EXIT_CODE: 0

TotalTests: 4897
Passed: 4897
Failed: 0
Skipped: 0

Output Summary: the run printed the success header `Test Run Successful.`, a `Total tests: 4897` line
and a `     Passed: 4897` line, and exited 0 after 33.5632 seconds. The passed count equals the total
count. Measured against the P0-T8 baseline of 4903 the difference is exactly minus six, which is the
AC11 expectation for this assembly. No test failed, so D9's single-re-run allowance for the sporadic
dictionary-extensions test was not invoked and the command was run once.

## Transcription Of The Two Zero Counters, Per D8

On this toolchain a fully green run prints the success header, a total line and a passed line, and
prints no `Failed:` line and no `Skipped:` line at all. The two zero values above are therefore
transcribed rather than quoted, and the transcription is corroborated by the success header and by the
passed count equalling the total count. No acceptance condition demands a zero-valued `Failed:` line,
because that line is never emitted on a green run.

The zero-skipped expectation is satisfiable for the reason D8 records: the two files in this test
project that carry a method-level Ignore attribute are not compiled, because the project declares
explicit Compile items with no wildcard glob and its items name only the copies of those two test
classes under its dialogs sub-folder, which carry no Ignore attribute.

## Filter Comparability, Per D5

The `/TestCaseFilter:` value above is byte-identical to the value P0-T8 recorded, which is what makes
the AC11 comparison valid. It was supplied to the process as a single argument built by concatenating
the switch name with a single-quoted PowerShell literal, so no shell quoting could alter it. Per D14 no
MSTest runsettings file is passed to this span.

## No Unexplained Failure

Every discovered test passed, so there is no failure to attribute either to this delivery's edits or to
the assembly-resolution change that pull request 880 made. Nothing needed to be worked around and no
test was adjusted.

## Artifact Retention

Per D10 the TRX is written to `TestResults/vstest/p2-t5/p2-t5-final-utilitiescs.trx`, a path under the
git-ignored results directory class, and is never committed. The span sets both an explicit results
directory and an explicit log file name, so the default TRX name that vstest composes from the account
name and the host name is never produced and no account or host token can reach a committed artifact.
P2-T14 reads this TRX in place.

The build lock was acquired immediately before this invocation and released immediately after it
returned. Outlook was verified not running.
