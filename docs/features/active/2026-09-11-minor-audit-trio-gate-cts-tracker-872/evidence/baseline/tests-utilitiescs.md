# Phase 0 — Baseline Executed-Test Count, UtilitiesCS Test Assembly

Timestamp: 2026-09-13T05-08
Task: [P0-T8]

Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests" "/Logger:trx;LogFileName=p0-t8-baseline-utilitiescs.trx" /ResultsDirectory:TestResults\vstest\p0-t8
EXIT_CODE: 0
TotalTests: 4903
Passed: 4903
Failed: 0
Skipped: 0

vstest.console.exe was resolved through vswhere at the explicit installer path, per D1. The test-case
filter is the string pinned in the plan's Command Reference and is byte-identical to the one Phase 2
will use, which is what makes the AC11 delta comparable. Per D14 no runsettings file was passed.

This `TotalTests:` value, 4903, is the AC11 baseline for the UtilitiesCS test assembly. AC11 requires
the post-change count to be exactly minus six against it, being the nine test methods removed with the
dormant tracker's test class less the three added by AC4, so the AC11 expectation for Phase 2 is 4897.

Output Summary: the run was fully green. The printed success header reads `Test Run Successful.`, the
total line reads `Total tests: 4903`, the passed line reads `Passed: 4903`, and the run completed in
`Total time: 32.3997 Seconds`. The passed count equals the total count.

## Failure And Skip Counters, Per D8

The run printed no `Failed:` line and no `Skipped:` line at all. On this toolchain a fully green run
emits neither, so neither counter could be transcribed from a printed line and no acceptance condition
in this plan demands a zero-valued `Failed:` line. The two counters are transcribed as 0 above on the
strength of two corroborating observations that the run did print: the success header
`Test Run Successful.`, and the passed count equalling the total count, which leaves no test in any
other outcome state.

The zero-skipped value is consistent with D8's compilation argument. Two files in this test project
carry a method-level Ignore attribute and neither is compiled: the project declares explicit Compile
items with no wildcard glob, and its items name only the copies of those two test classes under its
dialogs sub-folder, which carry no Ignore attribute. The compiled test population therefore contains no
ignored test.

## Excluded Population, Per D5

The filter excludes the LiveOutlook category, which starts an external Outlook process, and the four
shell-icon test classes that stall vstest on this workstation through a Windows shell icon call. None of
the excluded classes is touched by this delivery, so the AC11 delta arithmetic is unaffected by the
exclusion.

## Transient Output, Per D10

The TRX was written to the git-ignored results directory under an explicit results directory and an
explicit log file name, so the default account-and-host TRX name vstest would otherwise compose was
never produced. The TRX is not committed and no absolute host path from it is transcribed here.

## Build Lock

The cross-item build lock was held across the vstest invocation only. Acquired 2026-09-13T05:07:19,
released 2026-09-13T05:08:11.
