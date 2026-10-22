# Phase 3 — AC1 and AC2 green transition

Timestamp: 2026-09-07T02-37
Task: [P3-T6]
Issue: #798

Host-specific absolute paths, user account names and machine names are redacted to `<repo-root>`,
`<vstest>`, `<user>` and `<machine>` tokens.

## Formatting gate

Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0
ExpectedExitCode: 0

Summary line, quoted verbatim: `Formatted 1593 files in 3013ms.`

That line reports the number of files **scanned**, not the number rewritten, so it cannot
distinguish a clean run from a repairing one. The before-and-after observation is recorded below.

Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0
ExpectedExitCode: 0

Summary line, quoted verbatim: `Checked 1593 files in 5516ms.`

### Before-and-after format observation

Command: git status --porcelain --untracked-files=all -- . ":(exclude).claude" ":(exclude)docs"

The tracked and untracked path set was identical before and after the format pass:

```
 M QuickFiler.Test/QuickFiler.Test.csproj
 M TaskMaster.Test/TaskMaster.Test.csproj
 M TaskMaster/TaskMaster.csproj
 M UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs
 M UtilitiesCS.Test/UtilitiesCS.Test.csproj
 M UtilitiesCS/Extensions/DfDeedle.cs
 M UtilitiesCS/UtilitiesCS.csproj
?? QuickFiler.Test/Controllers/QfcDatamodelRethrowTests.cs
?? TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs
?? TaskMaster/Ribbon/RibbonCommandBoundary.cs
?? UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs
?? UtilitiesCS.Test/Extensions/DfDeedleRequiredColumnValidationTests.cs
?? UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs
```

The format pass rewrote only files already inside this change's write set. It introduced no file
outside it.

## Analyzer build

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
ExpectedExitCode: 0

Summary lines, quoted verbatim from the normal-verbosity file log:

```
Build succeeded.
0 Warning(s)
0 Error(s)
```

## Nullable build

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
EXIT_CODE: 0
ExpectedExitCode: 0

Summary lines, quoted verbatim from the normal-verbosity file log:

```
Build succeeded.
0 Warning(s)
0 Error(s)
```

`/p:Nullable=enable` was not supplied. No project carries a `<Nullable>` element and there is no
repository-root build property file, so the property would conscript every file that never adopted
the pragma. CI omits it deliberately.

## Test run

Command: `<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p3-ac1ac2 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"`

EXIT_CODE: 1
ExpectedExitCode: 1

The filter carries the shell-icon exclusion extension because P0-T8 recorded
`SHELL_ICON_EXCLUSION: REQUIRED`.

TRX: `coverage\trx\p3-ac1ac2\<user>_<machine>_2026-09-07_02_35_30_net481.trx`

- total: 6570
- passed: 6555
- failed: 15
- skipped: 0 (TRX `notExecuted=0`; the console printed no `Skipped:` line)
- total run time: 48.2785 s

No test hung; the run completed well inside the four-minute hang timeout.

The exit code is 1 because the AC3, AC4 and AC5 members of `BASELINE_FAILURE_SET` are still expected
to fail at the end of Phase 3. They land in P4-T4, P5-T4 and P6-T5.

## Per-test status for `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests`

Every test in the class passed. The list includes the two members that were already passing at P2-T3
and P2-T4 and are therefore outside `BASELINE_FAILURE_SET`, so P9-T7's evidence condition is
satisfiable from this artifact without consulting another run.

| Test | Status | Was in BASELINE_FAILURE_SET |
|---|---|---|
| `AddQfcColumnsAsync_ThreeDeadlines_InvokesColumnAdderExactlyOnce` | Passed | yes (AC1) |
| `AddQfcColumnsAsync_ThirdDeadlineExpires_ThrowsNamingFolderAndStep` | Passed | yes (AC1) |
| `AddQfcColumnsAsync_AdderCompletesBeforeSecondDeadline_ReturnsWithoutThrowingAndStartsOneTask` | Passed | yes (AC1) |
| `AddQfcColumnsAsync_AdderCompletesBeforeThirdDeadline_ReturnsWithoutThrowingAndStartsOneTask` | Passed | yes (AC1) |
| `AddQfcColumns_EmitsDfTimingLineForEachColumnOperation` | Passed | yes (AC2) |
| `HasUserDefinedProperty_EmitsDfTimingLineForPropertyEnumeration` | Passed | yes (AC2) |
| `AddQfcColumnsAsync_AdderCompletesBeforeFirstDeadline_ReturnsWithoutThrowingAndStartsOneTask` | Passed | no, already passing |
| `AddQfcColumnsAsync_CancellationRequestedMidLoop_DoesNotThrowTimeoutExhausted` | Passed | no, already passing (P2-T4 guard) |

The two pre-existing direct-call pins in the sibling COM test class also passed:
`UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests.AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing`
and
`UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests.AddQfcColumnsAsync_PreCancelledToken_CompletesGracefully`.

## Remaining failures

Exactly 15 tests failed, and they are exactly the AC3, AC4 and AC5 members of
`BASELINE_FAILURE_SET`. No test outside that set failed.

```
QuickFiler.Controllers.Tests.QfcDatamodelRethrowTests.GetEmailsInViewDfAsync_InnerFailure_PreservesOriginatingFrameInStack
TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.NamedQuickFilerHandlers_AreAwaitedAsyncVoidAndRouteThroughTheBoundary
TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_ActionThrows_DoesNotPropagateToCaller
TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_ActionThrows_InvokesLogSinkOnce
TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_ActionThrows_InvokesPresentationSinkOnce
TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_AggregateException_PresentedMessageIncludesInnerExceptionDetail
TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_PresentationSinkThrows_IsContainedAndDoesNotPropagate
UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_CaseVariantEntryid_ReportsEntryIDMissing
UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingConversationId_Throws
UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingEntryID_Throws
UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingMessageClass_Throws
UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingSentOn_MessageNamesFolder
UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingSentOn_Throws
UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingTriage_Throws
UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingTwoKeys_MessageNamesBothAndFolder
```

The issue #780 sporadic failure
`UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`
passed in this run and was not observed.

## Counted search of `LogDfTiming(`

Command: counted search of `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` for the literal `LogDfTiming(`

Observed total: 7.

Breakdown by call site, matching what P3-T3 and P3-T4 require:

| Line | Call site | Owner |
|---|---|---|
| 41 | `Columns.Add("SentOn")` | P3-T4 |
| 49 | `Columns.Add(MAPIFields.Schemas.ConversationId)` | P3-T4 |
| 57 | `Columns.Add(MAPIFields.Schemas.Triage)` | P3-T4 |
| 65 | `Columns.Remove("Subject")` | P3-T4 |
| 73 | `Columns.Remove("CreationTime")` | P3-T4 |
| 81 | `Columns.Remove("LastModificationTime")` | P3-T4 |
| 236 | user-defined property enumeration in `HasUserDefinedProperty` | P3-T3 |

Six for the column operations, one for the property enumeration, and no pre-existing call site: the
partial was created by P1-T1 with no timing instrumentation, so all seven were added by this phase.
The emissions go through the existing `LogDfTiming(string phase, string? details = null)` helper in
`UtilitiesCS/Extensions/DfDeedle.cs`; its prefix, format and level are unchanged.

## Deviation record — test-harness repairs required to satisfy this task

Three defects in the Phase 2 test file were only observable once the AC1 fix landed. Each is recorded
here because the plan's Phase 3 task text describes production edits only, while P3-T6's acceptance
requires a pass status for every test in the class. All three repairs preserve each test's stated
assertion; none weakens a criterion.

1. **`FireOneDeadlineAsync` deadlocked from the second deadline onward.** The helper re-armed the
   adder-entry signal after every deadline and awaited it before the next one. Under the recursive
   loop the adder was re-invoked per retry so the signal was set again; under the fixed loop the work
   is entered exactly once, so the re-armed signal could never be set. Three tests would have hung
   rather than failed. The repair removes the entry-signal re-arm from the helper and awaits the
   single completed entry signal on every deadline; the arming signal is still re-armed per deadline.
   A reintroduced recursion still fails the invocation-count assertion, so the test remains
   discriminating.

2. **`AddQfcColumnsAsync_ThreeDeadlines_InvokesColumnAdderExactlyOnce` contradicted P3-T2.** It drove
   three deadlines and then awaited the call bare, which asserts no exception, while its sibling
   `AddQfcColumnsAsync_ThirdDeadlineExpires_ThrowsNamingFolderAndStep` drives the identical sequence
   and asserts an exception. Both could not pass. The repair replaces the bare await with
   `FluentActions.Awaiting(() => call).Should().ThrowAsync<TimeoutException>()`. The test's assertion
   of record, an adder invocation count of 1, is unchanged, and the exhaustion throw it now observes
   is the behaviour P3-T2 mandates.

3. **The log4net capture harness could not observe production emissions.** Both AC2 tests failed with
   an empty capture after the instrumentation landed, in the full run and in an isolated
   single-class run, so it was not cross-class state pollution. Neither `UtilitiesCS` nor
   `UtilitiesCS.Test` carries a log4net configurator attribute; those attributes live on the
   `TaskMaster` and `QuickFiler` assemblies. An unconfigured `Hierarchy` reports every level disabled,
   so the production `logger.Debug` call was a no-op however the appender was attached. The repair
   sets `Configured` on the repository for the duration of the capture, calls `ActivateOptions` on the
   appender, and restores the previous value in the same `finally` that detaches the appender. This
   contradicts the `LOG4NET_CROSS_ASSEMBLY_CAPTURE: CONFIRMED` verdict recorded by P0-T13; the probe
   that produced that verdict has been removed and cannot be re-examined, so the verdict is recorded
   here as not reproducible rather than corrected in place.

`UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` measures 500 lines after these
repairs, equal to its Phase 2 value and within the repository cap. Five documentation and helper
lines were compacted to pay for the added code, so no new test file was created and the sixteen-path
write set fixed by AC13 is unchanged.

## File sizes after this phase

- `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` = 259 lines
- `UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs` = 500 lines

Output Summary: csharpier check EXIT_CODE 0 with `Checked 1593 files in 5516ms.`; analyzer build
EXIT_CODE 0 with `0 Error(s)` and `0 Warning(s)`; nullable build EXIT_CODE 0 with `0 Error(s)` and
`0 Warning(s)`; test run EXIT_CODE 1 with 6570 total, 6555 passed, 15 failed, 0 skipped. All eight
tests in `DfDeedleQfcColumnTimeoutTests` passed, turning the four AC1 and two AC2 members of
`BASELINE_FAILURE_SET` green while the two already-passing members stayed green. The 15 remaining
failures are exactly the AC3, AC4 and AC5 members of that set. `LogDfTiming(` appears 7 times in the
column partial: six column operations and one property enumeration. EXIT_CODE 1 matches
ExpectedExitCode 1.
