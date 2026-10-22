# P6-T5 — AC5 green transition (ribbon command boundary)

Timestamp: 2026-09-07T03-15
Task: [P6-T5]
Base commit: c431dc32
ExpectedExitCode: 0

Absolute host paths are redacted to `<repo-root>`, `<user>` and `<host>` tokens throughout.

---

## Step 1 — Formatting

Command: `dotnet tool run csharpier format .`

EXIT_CODE: 0

Command: `dotnet tool run csharpier check .`

EXIT_CODE: 0

Success-case summary line, quoted verbatim:

```
Checked 1593 files in 5486ms.
```

Output Summary: the check command exits 0 and reports 1593 files checked.

### Did the format pass rewrite any file?

No. Two independent observations establish this, because a `git status --porcelain` comparison
alone cannot detect the rewrite of a file already marked `M`, and the csharpier summary line
reports files **scanned** rather than files rewritten.

1. **Pre-format check.** `dotnet tool run csharpier check .` was run *before* the format pass,
   after all four Phase 6 source edits were complete, and exited 0 reporting
   `Checked 1593 files in 5923ms.` A clean check means every file already matched formatter
   output, so the format pass had nothing to rewrite.
2. **Modification times.** `TaskMaster/Ribbon/RibbonViewer.cs` carries an mtime of `03:11:58` and
   `TaskMaster/Ribbon/RibbonCommandBoundary.cs` an mtime of `03:10:11`. Both precede the format
   pass, which ran at approximately `03:12:2x`. Had the formatter written either file, its mtime
   would follow the format pass rather than precede it.

Because no file was rewritten, the toolchain loop did not restart at step 1.

---

## Step 2 — Analyzer build

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

EXIT_CODE: 0

Summary lines, quoted verbatim:

```
0 Warning(s)
0 Error(s)
```

Output Summary: `0 Error(s)`. The warning count is 0, matching the P0-T6 baseline of 0.

---

## Step 3 — Nullable build

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true

EXIT_CODE: 0

Summary lines, quoted verbatim:

```
0 Warning(s)
0 Error(s)
```

Output Summary: `0 Error(s)`. The warning count is 0, matching the P0-T7 baseline of 0.
`/p:Nullable=enable` was not supplied, matching CI.

---

## Step 4 — Test run over the three affected assemblies

Command: `<vstest>` `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p6-ac5 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"

EXIT_CODE: 0

The filter carries the shell-icon extension because P0-T8 recorded the verdict
`SHELL_ICON_EXCLUSION: REQUIRED`.

Results file: `<repo-root>\coverage\trx\p6-ac5\<user>_<host>_2026-09-07_03_13_34_net481.trx`

### Counts

| Metric | Value |
|---|---|
| Total tests | 6570 |
| Passed | 6570 |
| Failed | 0 |
| Skipped | 0 |
| Total time | 51.0248 seconds |

The reporter printed `Test Run Successful.` and printed neither a `Failed:` nor a `Skipped:`
line, which for this reporter means both counts are 0. A counted search of the run log for lines
matching a failed-test result returned 0.

Output Summary: 6570 total, 6570 passed, 0 failed, 0 skipped. Exit code 0, matching
`ExpectedExitCode`.

### Movement across the phase sequence

| Run | Total | Passed | Failed |
|---|---|---|---|
| P2-T11 fail-before | 6570 | 6549 | 21 |
| P5-T4 (after AC4) | 6570 | 6564 | 6 |
| P6-T5 (this run) | 6570 | 6570 | 0 |

---

## `BASELINE_FAILURE_SET` — all 21 members pass

The run recorded zero failing tests, so every member of the set designated by P2-T11 passes.
Enumerated for audit:

1. `QuickFiler.Controllers.Tests.QfcDatamodelRethrowTests.GetEmailsInViewDfAsync_InnerFailure_PreservesOriginatingFrameInStack`
2. `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.NamedQuickFilerHandlers_AreAwaitedAsyncVoidAndRouteThroughTheBoundary`
3. `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_ActionThrows_DoesNotPropagateToCaller`
4. `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_ActionThrows_InvokesLogSinkOnce`
5. `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_ActionThrows_InvokesPresentationSinkOnce`
6. `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_AggregateException_PresentedMessageIncludesInnerExceptionDetail`
7. `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.RunAsync_PresentationSinkThrows_IsContainedAndDoesNotPropagate`
8. `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumns_EmitsDfTimingLineForEachColumnOperation`
9. `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_AdderCompletesBeforeSecondDeadline_ReturnsWithoutThrowingAndStartsOneTask`
10. `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_AdderCompletesBeforeThirdDeadline_ReturnsWithoutThrowingAndStartsOneTask`
11. `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_ThirdDeadlineExpires_ThrowsNamingFolderAndStep`
12. `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.AddQfcColumnsAsync_ThreeDeadlines_InvokesColumnAdderExactlyOnce`
13. `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests.HasUserDefinedProperty_EmitsDfTimingLineForPropertyEnumeration`
14. `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_CaseVariantEntryid_ReportsEntryIDMissing`
15. `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingConversationId_Throws`
16. `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingEntryID_Throws`
17. `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingMessageClass_Throws`
18. `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingSentOn_MessageNamesFolder`
19. `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingSentOn_Throws`
20. `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingTriage_Throws`
21. `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests.ValidateRequiredEmailColumns_MissingTwoKeys_MessageNamesBothAndFolder`

---

## `RibbonCommandBoundaryTests` — all seven members pass

This includes `RunAsync_ActionSucceeds_InvokesNeitherSink`, which already passed at P2-T9 and is
therefore outside `BASELINE_FAILURE_SET`. Recording it here makes P9-T5's evidence condition,
which covers all seven AC5 tests, satisfiable from this artifact alone.

```
Passed RunAsync_ActionSucceeds_InvokesNeitherSink [< 1 ms]
Passed RunAsync_ActionThrows_InvokesLogSinkOnce [1 ms]
Passed RunAsync_ActionThrows_InvokesPresentationSinkOnce [< 1 ms]
Passed RunAsync_ActionThrows_DoesNotPropagateToCaller [< 1 ms]
Passed RunAsync_PresentationSinkThrows_IsContainedAndDoesNotPropagate [< 1 ms]
Passed RunAsync_AggregateException_PresentedMessageIncludesInnerExceptionDetail [< 1 ms]
Passed NamedQuickFilerHandlers_AreAwaitedAsyncVoidAndRouteThroughTheBoundary [2 ms]
```

The sixth of these is the P6-T2 clause. It asserts that an `AggregateException` reaching the
boundary is presented with its inner exception's message rather than the wrapper's own
"One or more errors occurred.", which carries no actionable content. The wrapping happens in the
timeout helper's result marshalling, upstream of the AC4 rethrow, so `throw;` restores the
original stack but does not unwrap; inner-exception rendering is what makes the dialog
diagnosable.

---

## Other observations

The issue #780 sporadic `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`
passed in this run. No rerun was required, and the rerun count for this task is 0.

No test failed for a reason the plan does not predict.

---

## Verdict

AC5 is delivered. The toolchain completed in a single pass with no restart: no step failed and no
step rewrote a file. All four steps report exit code 0, and the test run is fully green at
6570/6570.
