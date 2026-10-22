# Phase 7 — Whole-Assembly Green Run After Removing DoNotParallelize

Timestamp: 2026-09-09T17-15

Command: & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
EXIT_CODE: 0

Command: & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"
EXIT_CODE: 0

TotalTests: 4919
TestsPassed: 4919
TestsFailed: 0 (omitted category, transcribed per D10)
TestsSkipped: 0 (omitted category, transcribed per D10)

Output Summary: The solution rebuild reported "Build succeeded", 0 Warning(s) and 0 Error(s) in
00:00:11.48. The whole-assembly run printed "Test Run Successful.", "Total tests: 4919" and
"Passed: 4919" in 31.8719 seconds. Per D10 a fully green run on this toolchain emits no `Failed:`
and no `Skipped:` line, so both counters are transcribed as 0; that transcription is corroborated by
the printed success header and by `Passed:` equalling `Total tests:`.

This is the first run in which OlTableExtensions_Tests executes without [DoNotParallelize], so its
tests ran under the assembly's class-level parallelism alongside the rest of UtilitiesCS.Test. The
run is recorded as evidence that the change compiles and the suite is green, not as the
justification for the attribute removal; that justification is the item 2 code change and is
recorded at evidence/other/ac21-justification.md with RunsObserved deliberately at 0.

The LiveOutlook exclusion is mandatory rather than convenient: the repository's only test in that
category constructs a real Outlook Application and polls a live store, which is an external-process
dependency the unit-test policy forbids in a unit-test run.

D6's non-termination hazard did not occur. The four shell-icon classes that have previously stalled
full local vstest runs through SHGetFileInfo completed within this run, which finished in under
32 seconds, and the #780 TryAddValuesAsync flake did not reproduce. No search root was narrowed and
no additional filter clause was introduced.
