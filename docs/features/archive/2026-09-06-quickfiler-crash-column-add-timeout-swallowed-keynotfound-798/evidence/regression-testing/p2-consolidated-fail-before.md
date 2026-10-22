# Phase 2 — Consolidated fail-before set

Timestamp: 2026-09-07T02-11
Task: [P2-T11]
Issue: #798

Host-specific absolute paths, user account names and machine names are redacted to `<repo-root>`,
`<vstest>`, `<user>` and `<machine>` tokens.

## Formatting gate

Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0
ExpectedExitCode: 0

Summary line, quoted verbatim: `Checked 1593 files in 5956ms.`

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

`/t:Rebuild` was used, not `/t:Build`: MSBuild's up-to-date check does not invalidate on a
command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every
project and runs no analyzers.

## Test run

Command: `<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p2-failbefore /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"`

EXIT_CODE: 1
ExpectedExitCode: 1

The filter carries the shell-icon exclusion extension because P0-T8 recorded
`SHELL_ICON_EXCLUSION: REQUIRED`.

TRX: `coverage\trx\p2-failbefore\<user>_<machine>_2026-09-07_02_11_37_net481.trx`

- total: 6570
- passed: 6549
- failed: 21
- skipped: 0
- total run time: 51.8055 s

The console produced no `Skipped:` line, and the TRX outcome tally contains only `Passed` and
`Failed`, so the skipped count is 0.

The total rose from the 6545 recorded at P1-T13 by exactly the 25 tests Phase 2 added: 8 in
`DfDeedleQfcColumnTimeoutTests`, 9 in `DfDeedleRequiredColumnValidationTests`, 7 in
`RibbonCommandBoundaryTests` and 1 in `QfcDatamodelRethrowTests`.

## BASELINE_FAILURE_SET

The following 21 tests are designated `BASELINE_FAILURE_SET`. They are the only tests failing in the
three affected assemblies at the end of Phase 2, and they are the only set later phases must turn
green. P3-T6, P4-T4, P5-T4 and P6-T5 consume this designation.

```
QuickFiler.Controllers.Tests.QfcDatamodelRethrowTests.GetEmailsInViewDfAsync_InnerFailure_PreservesOriginatingFrameInStack
TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.NamedQuickFilerHandlers_AreAwaitedAsyncVoidAndRouteThroughTheBoundary
TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_ActionThrows_DoesNotPropagateToCaller
TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_ActionThrows_InvokesLogSinkOnce
TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_ActionThrows_InvokesPresentationSinkOnce
TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_AggregateException_PresentedMessageIncludesInnerExceptionDetail
TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_PresentationSinkThrows_IsContainedAndDoesNotPropagate
UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumns_EmitsDfTimingLineForEachColumnOperation
UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_AdderCompletesBeforeSecondDeadline_ReturnsWithoutThrowingAndStartsOneTask
UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_AdderCompletesBeforeThirdDeadline_ReturnsWithoutThrowingAndStartsOneTask
UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_ThirdDeadlineExpires_ThrowsNamingFolderAndStep
UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_ThreeDeadlines_InvokesColumnAdderExactlyOnce
UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.HasUserDefinedProperty_EmitsDfTimingLineForPropertyEnumeration
UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_CaseVariantEntryid_ReportsEntryIDMissing
UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingConversationId_Throws
UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingEntryID_Throws
UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingMessageClass_Throws
UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingSentOn_MessageNamesFolder
UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingSentOn_Throws
UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingTriage_Throws
UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingTwoKeys_MessageNamesBothAndFolder
```

Membership by acceptance criterion:

| Criterion | Members | Turns green in |
|---|---|---|
| AC1 | 4 (`AddQfcColumnsAsync_*`) | P3-T6 |
| AC2 | 2 (`*_EmitsDfTimingLine*`) | P3-T6 |
| AC3 | 8 (`ValidateRequiredEmailColumns_*`) | P4-T4 |
| AC4 | 1 (`GetEmailsInViewDfAsync_InnerFailure_*`) | P5-T4 |
| AC5 | 6 (`RunAsync_*` and `NamedQuickFilerHandlers_*`) | P6-T5 |

## No other failure

No test in the three assemblies is failing at this point apart from the 21 members of
`BASELINE_FAILURE_SET`.

The issue #780 sporadic failure
`UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` did
**not** appear in this run; it passed. It is therefore recorded here as not observed, and it is not a
member of `BASELINE_FAILURE_SET`.

## Tests added in Phase 2 that pass and are outside BASELINE_FAILURE_SET

Four of the 25 new tests pass at Phase 2 and are correctly outside the set. They are guards and
positive paths that must remain passing through the later phases:

- `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_AdderCompletesBeforeFirstDeadline_ReturnsWithoutThrowingAndStartsOneTask`
- `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_CancellationRequestedMidLoop_DoesNotThrowTimeoutExhausted`
- `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_AllKeysPresent_DoesNotThrow`
- `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_ActionSucceeds_InvokesNeitherSink`

## Hang check

No test hung. The `/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None` collector reported
`All tests finished running, Sequence file will not be generated.` in every Phase 2 run, the
four-minute hang timeout never fired, and the whole consolidated suite completed in 51.8 seconds.

Output Summary: csharpier check EXIT_CODE 0 with `Checked 1593 files in 5956ms.`; analyzer build
EXIT_CODE 0 with `0 Error(s)` and `0 Warning(s)`; test run EXIT_CODE 1 with 6570 total, 6549 passed,
21 failed, 0 skipped. The 21 failures are exactly `BASELINE_FAILURE_SET`. EXIT_CODE 1 matches
ExpectedExitCode 1.
