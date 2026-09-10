# Phase 3 — Green Run After Threading the TimeProvider and Fixing the Retry Literal

Timestamp: 2026-09-09T16-57

Command: & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
EXIT_CODE: 0

Command: & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~GetTableInViewAsyncClockTests|FullyQualifiedName~OlTableExtensions_Tests|FullyQualifiedName~OlTableExtensionsEtlClockTests"
EXIT_CODE: 0

Command: & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~DfDeedleEtlTimeoutTests|FullyQualifiedName~DfDeedle_COM_Tests"
EXIT_CODE: 0

## Run 1 counters

TotalTests: 90
TestsPassed: 90
TestsFailed: 0 (omitted category, transcribed per D10)
TestsSkipped: 0 (omitted category, transcribed per D10)

## Run 2 counters

TotalTests: 29
TestsPassed: 29
TestsFailed: 0 (omitted category, transcribed per D10)
TestsSkipped: 0 (omitted category, transcribed per D10)

## Named per-test results

Passed GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000 [2 ms]
Passed GetTableInViewAsync_InjectedClock_ArmsAcquisitionDeadlineOnInjectedProvider [< 1 ms]
Passed GetTableInViewAsync_ExplicitFactorySupplied_TakesPrecedenceOverTimeProvider [< 1 ms]
Passed GetTableInViewAsync_NoTimeProviderSupplied_UsesSystemClockAndCompletes [< 1 ms]
Passed GetEmailDataInViewAsync_EtlDeadlineExpires_ThrowsInvalidOperationNamingFolder [46 ms]
Passed GetEmailDataInViewAsync_ClockNeverAdvances_ReturnsOneRowFrame [4 ms]
Passed GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform [481 ms]

The first four are the new tests in
UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs and appear in run 1. The last
three are the DfDeedle-path tests and appear in run 2:
GetEmailDataInViewAsync_EtlDeadlineExpires_ThrowsInvalidOperationNamingFolder and
GetEmailDataInViewAsync_ClockNeverAdvances_ReturnsOneRowFrame are declared in
UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs, and
GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform is declared in
UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs, which the run 2 filter covers.

Output Summary: The solution rebuild reported "Build succeeded", 0 Warning(s) and 0 Error(s) in
00:00:12.58. Both scoped runs printed "Test Run Successful." with `Passed:` equalling
`Total tests:`. Per D10 a fully green run on this toolchain emits no `Failed:` and no `Skipped:`
line, so both counters are transcribed as 0 for each run.

Three results are worth naming explicitly.
GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000 failed against the pre-change
file at P2-T4 with the second recorded factory argument at 2000 and now passes, which is the
fail-before/pass-after pair AC7 requires.
GetTableInViewAsync_InjectedClock_ArmsAcquisitionDeadlineOnInjectedProvider completed its
`await barrier.Armed`, which is only reachable if a timer was created on the injected provider, so
it is the empirical proof AC5 requires.
GetEmailDataInViewAsync_EtlDeadlineExpires_ThrowsInvalidOperationNamingFolder passes with the
bounded timer-ordering update of P3-T7 and P3-T8, confirming the Branch A adjudication: the
table-acquisition timer is now the first arming signal on the barrier and each of the three timers
arms inside its own await window.
