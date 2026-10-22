# Phase 0 — UtilitiesCS Test Assembly Baseline

Timestamp: 2026-09-13T14-59
Task: [P0-T8]

Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/Logger:trx;LogFileName=p0-t8-baseline-utilitiescs.trx" /ResultsDirectory:TestResults\vstest\p0-t8
EXIT_CODE: 0

TotalTests: 4903
Passed: 4903
Failed: 0
Skipped: 0

Output Summary: the run printed the success header `Test Run Successful.`, a `Total tests: 4903` line
and a `Passed: 4903` line, and exited 0 after 12.3335 seconds. The passed count equals the total count.
This `TotalTests:` value is the AC11 baseline for this assembly.

## Transcription Of The Two Zero Counters, Per D8

On this toolchain a fully green run prints the success header, a total line and a passed line, and
prints no `Failed:` line and no `Skipped:` line at all. The two zero values above are therefore
transcribed rather than quoted, and the transcription is corroborated by the success header and by the
passed count equalling the total count. No acceptance condition in this plan demands a zero-valued
`Failed:` line, because that line is never emitted on the run the condition is meant to pass.

D8 also records why the zero-skipped expectation is satisfiable: two files in this test project carry
a method-level Ignore attribute and neither is compiled, because the project declares explicit Compile
items with no wildcard glob and its items name only the copies of those two test classes that live
under its dialogs sub-folder, which carry no Ignore attribute.

## Re-Run Note And Comparison With The Superseded Figure, Per D15

This artifact overwrites a superseded capture taken before the main branch carrying the fix for issue
#877 was merged into this branch. The superseded capture recorded `TotalTests: 4903`. The re-measured
figure is also 4903. The two figures agreeing is an observation and not an assumption: the run above
was executed against the post-merge tree and its total was read from that run's own output. The
agreement is consistent with what the #877 fix did, which was to move the body of this project's
assembly initializer into a shared source file referenced by both test projects; that change relocates
code without adding or removing a test method, so the discovered population is unchanged.

No test failed in this run. No failure was observed here that the previous Phase 0 run did not see,
because no failure was observed at all.

## Population, Per D5

The pinned test-case filter excludes the LiveOutlook category, which starts an external Outlook
process, and the four shell-icon test classes that stall vstest on this workstation through a Windows
shell icon call. None of the excluded classes is touched by this delivery. The identical filter string
is used by P2-T5, which is what makes the AC11 delta comparable.

Per D14 no MSTest runsettings file is passed to this span.

## Artifact Retention

Per D10 the TRX is written to `TestResults/vstest/p0-t8/p0-t8-baseline-utilitiescs.trx`, which resolves
to the git-ignore pattern for the results directory class, and is never committed. The span sets both
an explicit results directory and an explicit log file name, so the default TRX name that vstest
composes from the account name and the host name is never produced. The run printed an overwrite
warning for that file, which is the expected consequence of re-running a task whose log file name is
fixed: the re-run overwrote the superseded TRX rather than adding a second file to the directory.
