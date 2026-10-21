# AC7 — Fail-Before Record for the Retry-Literal Regression Test

Timestamp: 2026-09-09T16-48

Command: & $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000"

EXIT_CODE: 1
ExpectedExitCode: 1

TestsPassed: 0
TestsFailed: 1
TestsSkipped: 0 (omitted category, transcribed per D10)

FailureReason: The second value the factory received was 2000 rather than 750. vstest reported
`Failed GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000` with the error message
"Expected recordedTimeouts[1] to be 750 because the retry must arm on the caller's timeoutMs rather
than on a literal 2000, but found 2000 (difference of 1250)."

Output Summary: The run was scoped to the single new test by test-case filter, against the test
assembly built at P2-T3 from the pre-change production file. vstest printed "Total tests: 1",
"Failed: 1" and "Test Run Failed." in 1.6037 seconds. Per D10, `Passed:` and `Skipped:` are omitted
when their counters are zero, so both are transcribed as 0; TestsPassed is `Total tests:` minus the
printed `Failed:` figure. The run also emitted the diagnostic line "Task timed out on try 0" from
UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs, which is the Console.WriteLine
inside the TimeoutException catch and independently confirms that the injected throw reached that
catch.

## Why this is the expected outcome for this task

The figure 2000 is the literal at UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs
line 106, which the TimeoutException catch opened at line 95 passes in place of the caller's
timeoutMs. The factory is invoked at UtilitiesCS/Threading/TimeOutTask.cs line 52, outside the try
opened at line 61, so its throw is captured into the task the await at TableAccess.cs line 57
observes and enters that catch. The recursion at line 103 then invokes the factory a second time
with the literal.

The count half of the assertion passed: the factory recorded exactly two invocations, which proves
the retry branch was entered rather than the call failing earlier. Only the value assertion failed,
and it failed on precisely the defect item 2 exists to close. P3-T5 replaces that literal with
timeoutMs, after which the second recorded value is 750 and this test passes.
