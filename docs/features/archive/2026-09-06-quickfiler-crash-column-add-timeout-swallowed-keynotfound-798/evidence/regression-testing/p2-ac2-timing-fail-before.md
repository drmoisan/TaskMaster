# Phase 2 — AC2 timing regression tests (fail-before)

Timestamp: 2026-09-07T02-00
Task: [P2-T5] [expect-fail]
Issue: #798

> **SUPERSEDED as AC2 fail-before evidence** by `p3-ac2-failbefore-rederived.2026-09-07T02-45.md`: the log capture these two tests depend on was inert when this artifact was written, so the failures recorded below would have occurred whether or not the AC2 instrumentation existed and this gate did not discriminate.

Host-specific absolute paths, user account names and machine names are redacted to `<repo-root>`,
`<vstest>`, `<user>` and `<machine>` tokens.

## Assertion strategy selected by the P0-T13 verdict

P0-T13 recorded, verbatim:

`LOG4NET_CROSS_ASSEMBLY_CAPTURE: CONFIRMED`

P2-T5 therefore implements the **first** branch of the task: both tests attach a
`log4net.Appender.MemoryAppender` to the logger named by `typeof(DfDeedle).FullName`, reached through
`(log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository()`, set that logger's level
to `log4net.Core.Level.Debug`, and detach the appender and restore the level in a `finally`.

The `NOT AVAILABLE` fallback — attaching the appender to the repository reached from the production
assembly's own logger instance — was **not** implemented, because the verdict did not select it. The
indirect strategy floated in spec.md's assumptions section, asserting AC2 through the injected
column-adder, was also not used: injecting an adder replaces the whole `AddQfcColumns` body, so the
AC2 instrumentation would never execute and the assertion could not fail. P0-T13 records that reason.

Both assertions are **existence** assertions, not count assertions. Each test asserts that at least
one captured message is a `[Df timing]` line naming the expected operation. No test asserts how many
such lines were captured. log4net binds one logger per type for the whole process, so a concurrently
running class can add events but can never remove them, which makes an existence claim deterministic
and a count assertion order-dependent.

## Tests added

- `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumns_EmitsDfTimingLineForEachColumnOperation`
- `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.HasUserDefinedProperty_EmitsDfTimingLineForPropertyEnumeration`

Both reach the relocated methods the way the existing COM test class already reaches
`AddQfcColumnsAsync`: through `typeof(DfDeedle).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)`,
invoked against a Moq-built `Table` and a Moq-built `MAPIFolder` whose `UserDefinedProperties`
collection contains a `Triage` entry, so `EnsureTriageColumnExists` returns true without reaching
`MessageBoxInvoker` and all six `Columns` operations execute. Neither `AddQfcColumns` nor
`HasUserDefinedProperty` was widened from `private`. The mock builders are declared privately in the
new test class rather than shared with the existing COM test class.

## Build

Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
ExpectedExitCode: 0

## Test run

Command: `<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p2-t5 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~DfDeedleQfcColumnTimeoutTests&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"`

EXIT_CODE: 1
ExpectedExitCode: 1

The filter carries the shell-icon exclusion extension because P0-T8 recorded
`SHELL_ICON_EXCLUSION: REQUIRED`. The run covers the whole test class, so it also re-verifies the
P2-T1 through P2-T4 observations after the class was compacted to satisfy the 500-line cap.

TRX: `coverage\trx\p2-t5\<user>_<machine>_2026-09-07_02_00_30_net481.trx`

- total: 8
- passed: 2
- failed: 6
- total run time: 2.5461 s

## Observed status of the two AC2 tests

| Test | Observed status | Duration |
|---|---|---|
| `AddQfcColumns_EmitsDfTimingLineForEachColumnOperation` | Failed | 23 ms |
| `HasUserDefinedProperty_EmitsDfTimingLineForPropertyEnumeration` | Failed | < 1 ms |

Failure messages, quoted verbatim:

```
Expected HasTimingLineNaming(messages, columnName) to be True because no [Df timing] line names the column operation 'SentOn', but found False.
```

```
Expected HasTimingLineNaming(messages, "HasUserDefinedProperty") to be True because the property enumeration must emit its own [Df timing] line, but found False.
```

Both failures are the ones the plan predicts, and neither is incidental. Both tests reached their
assertion, so the reflective invocation of the private static method succeeded and the production
method ran to completion; the assertion failed because the unfixed methods emit no `[Df timing]` line
at all. Neither test hung; the four-minute blame hang timeout did not fire.

## Re-verification of P2-T1 through P2-T4 in the same run

| Test | Observed status | Observed adder invocation count |
|---|---|---|
| `AddQfcColumnsAsync_ThreeDeadlines_InvokesColumnAdderExactlyOnce` | Failed | 3 |
| `AddQfcColumnsAsync_ThirdDeadlineExpires_ThrowsNamingFolderAndStep` | Failed (no exception thrown) | not asserted |
| `AddQfcColumnsAsync_AdderCompletesBeforeFirstDeadline_ReturnsWithoutThrowingAndStartsOneTask` | Passed | 1 |
| `AddQfcColumnsAsync_AdderCompletesBeforeSecondDeadline_ReturnsWithoutThrowingAndStartsOneTask` | Failed | 2 |
| `AddQfcColumnsAsync_AdderCompletesBeforeThirdDeadline_ReturnsWithoutThrowingAndStartsOneTask` | Failed | 3 |
| `AddQfcColumnsAsync_CancellationRequestedMidLoop_DoesNotThrowTimeoutExhausted` | Passed | not asserted |

Every observation recorded by P2-T1 through P2-T4 is unchanged after the compaction.

## File-size note

`UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` measures 500 lines after this task,
which is at, and not over, the repository's 500-line cap. The eight tests of P2-T1 through P2-T5 were
compacted into that budget by extracting a shared call helper and shortening documentation comments;
no test was removed and no assertion was weakened, as the re-verification table above shows.

Output Summary: 8 total, 2 passed, 6 failed. Both AC2 timing tests failed because the unfixed methods
emit no `[Df timing]` line, which is the AC2 fail-before condition. The strategy implemented is the
`LOG4NET_CROSS_ASSEMBLY_CAPTURE: CONFIRMED` branch, asserting existence rather than a count.
EXIT_CODE 1 matches ExpectedExitCode 1.
