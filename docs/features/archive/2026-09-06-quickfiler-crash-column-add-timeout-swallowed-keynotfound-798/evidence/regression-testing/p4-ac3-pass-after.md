# Phase 4 — AC3 required-column validation green transition

Timestamp: 2026-09-07T02-55
Task: [P4-T4]
Issue: #798

Host-specific absolute paths, user account names and machine names are redacted to `<repo-root>`,
`<msbuild>`, `<vstest>`, `<user>` and `<machine>` tokens.

ExpectedExitCode: 1

The AC4 and AC5 members of `BASELINE_FAILURE_SET` are still expected to fail at this point in the
plan, so the test run's non-zero exit is the expected outcome for this task. AC4 lands in Phase 5 and
AC5 in Phase 6.

## Step 1 — formatting

Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0

A `git status --porcelain --untracked-files=all -- . ":(exclude).claude"` observation was taken
immediately before and immediately after the format pass. The two observations are identical
element-for-element: the same 10 modified or added tracked paths and the same 37 untracked paths, in
the same order.

**That porcelain comparison does not establish that no file was rewritten, and this pass did rewrite
one.** A file already carrying an `M` status keeps that same status when it is rewritten again, so
the observation can only detect a path whose tracked status changed or a path that appeared or
disappeared. It is recorded here for scope, not as the rewrite gate.

The rewrite question was settled separately, by file modification time. A second
`dotnet tool run csharpier format .` pass was run over the same tree and rewrote neither
`UtilitiesCS/Extensions/DfDeedle.cs` nor `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`, which
establishes that csharpier writes only the files it actually changes. Against that fact the
modification times read:

- `UtilitiesCS/Extensions/DfDeedle.cs` — 02:52:08, inside the first format pass. **Rewritten by the
  first format pass.**
- `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` — 02:51:30, its P4-T1 edit time. Not rewritten.

Because the first format pass rewrote a file, the toolchain loop was restarted from step 1. The
restart's step 1 rewrote nothing, and its step 2 check passed:

```
Checked 1593 files in 5706ms.
```

Steps 2, 3 and 4 below all executed after the first format pass and therefore already ran against the
post-format content, and no file changed between them and the restart, so their recorded results
remain the results for the final clean pass.

The format command's own summary line is recorded for completeness but is not used as the gate,
because it reports files scanned rather than files rewritten and reads identically on a clean run and
on a repairing one:

```
Formatted 1593 files in 3281ms.
```

Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0

Success-case summary line, quoted verbatim, beginning with the literal `Checked ` and ending with the
literal `ms.`:

```
Checked 1593 files in 5673ms.
```

## Step 2 — analyzer build

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0

Summary lines, quoted verbatim:

```
    0 Warning(s)
    0 Error(s)
```

`/t:Rebuild` was used, not `/t:Build`: MSBuild's up-to-date check does not invalidate on a
command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped and runs no
analyzers. The warning count matches the Phase 0 baseline of 0.

## Step 3 — nullable build

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
EXIT_CODE: 0

Summary lines, quoted verbatim:

```
    0 Warning(s)
    0 Error(s)
```

`/p:Nullable=enable` was not supplied, matching CI and the plan's toolchain conventions: no project
carries a `<Nullable>` element and there is no repository-root build property file, so the property
would conscript every file that has never adopted the pragma. This satisfies the P4-T2 acceptance
clause requiring the nullable build to report `0 Error(s)`, which is the gate covering the
non-nullable `string` capture that task introduces.

## Step 4 — test run over the three affected assemblies

Command: `<vstest> <repo-root>\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll <repo-root>\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll <repo-root>\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p4-ac3 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"`
EXIT_CODE: 1

The filter carries the shell-icon exclusion extension because P0-T8 recorded
`SHELL_ICON_EXCLUSION: REQUIRED`.

TRX: `coverage\trx\p4-ac3\<user>_<machine>_2026-09-07_02_54_12_net481.trx`

TRX `ResultSummary/Counters`:

- total: 6570
- executed: 6570
- passed: 6563
- failed: 7
- notExecuted: 0
- aborted: 0
- timeout: 0
- total time: 48.2023 s

No test hung; the four-minute blame hang timeout did not fire. The issue #780 sporadic failure
`DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` was not observed in this run.

### AC3 green transition

The eight AC3 members of `BASELINE_FAILURE_SET` are now passing. The failed count moved from 15 to 7,
and the difference is exactly those eight tests.

| Class | Total | Passed | Failed |
|---|---|---|---|
| `UtilitiesCS.Test.Extensions.DfDeedleRequiredColumnValidationTests` | 9 | **9** | 0 |
| `UtilitiesCS.Test.Extensions.DfDeedle_Tests` | 15 | **15** | 0 |
| `UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests` | 26 | **26** | 0 |
| `UtilitiesCS.Test.Extensions.DfDeedleQfcColumnTimeoutTests` | 8 | **8** | 0 |

All nine `DfDeedleRequiredColumnValidationTests` tests pass, which is the P4-T1 acceptance condition.
The nine are the five negative cases added by P2-T6 and the four message-content, multi-missing,
case-variant and positive cases added by P2-T7:

- `ValidateRequiredEmailColumns_MissingEntryID_Throws`
- `ValidateRequiredEmailColumns_MissingMessageClass_Throws`
- `ValidateRequiredEmailColumns_MissingSentOn_Throws`
- `ValidateRequiredEmailColumns_MissingConversationId_Throws`
- `ValidateRequiredEmailColumns_MissingTriage_Throws`
- `ValidateRequiredEmailColumns_MissingTwoKeys_MessageNamesBothAndFolder`
- `ValidateRequiredEmailColumns_CaseVariantEntryid_ReportsEntryIDMissing`
- `ValidateRequiredEmailColumns_AllKeysPresent_DoesNotThrow`
- `ValidateRequiredEmailColumns_MissingSentOn_MessageNamesFolder`

`DfDeedle_Tests` and `DfDeedle_COM_Tests` are the pre-existing fixed-arity pins required by AC9.
Both classes pass in full and neither was modified by Phase 4, so the parameter lists of
`Email2dToRecords`, `Email2dArrayToDf` and `GetEmailDataFromTable` are unchanged and still pinned.

`DfDeedleQfcColumnTimeoutTests` passing in full confirms that the AC1 and AC2 work delivered in
Phase 3 did not regress under the Phase 4 edits.

### Remaining failures — exactly the AC4 and AC5 members of `BASELINE_FAILURE_SET`

Seven tests failed. All seven are expected failures for this phase; no other test failed.

AC4 (1 test), delivered in Phase 5:

- `QuickFiler.Controllers.Tests.QfcDatamodelRethrowTests.GetEmailsInViewDfAsync_InnerFailure_PreservesOriginatingFrameInStack`

AC5 (6 tests), delivered in Phase 6, all under `TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests`:

- `RunAsync_ActionThrows_InvokesLogSinkOnce`
- `RunAsync_ActionThrows_InvokesPresentationSinkOnce`
- `RunAsync_ActionThrows_DoesNotPropagateToCaller`
- `RunAsync_PresentationSinkThrows_IsContainedAndDoesNotPropagate`
- `RunAsync_AggregateException_PresentedMessageIncludesInnerExceptionDetail`
- `NamedQuickFilerHandlers_AreAwaitedAsyncVoidAndRouteThroughTheBoundary`

The set is exactly 1 + 6 = 7 and contains no other member.

## Change-scope observations recorded by this task

`UtilitiesCS/Extensions/DfDeedle.cs` counted occurrences:

- `ValidateRequiredEmailColumns(tableSnapshot.Item2` : **1** (P4-T2 acceptance)
- `ValidateRequiredEmailColumns(` : **2** (P4-T3 acceptance)

Anchored diff check for the AC9 arity pin. `git diff c431dc32 -- UtilitiesCS/Extensions/DfDeedle.cs`
produced 115 removed lines, all of them the Phase 1 relocation of the four column methods. The count
of removed lines carrying any of the three identifiers `Email2dToRecords`, `Email2dArrayToDf` or
`GetEmailDataFromTable` is **0**, which pins all three arities without depending on a declaration
fitting on one line.

Post-change line counts of the files this phase modified, against the repository's 500-line cap:

- `UtilitiesCS/Extensions/DfDeedle.cs` = 314 — under cap
- `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` = 297 — under cap

Neither file this phase modified breaches the cap. `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs`
remains at 869 lines, which is the pre-existing over-cap file recorded by P0-T12; it strictly
decreased from its base-commit 882 and was not modified by Phase 4.

Output Summary: All four toolchain steps ran in order. The first format pass rewrote
`UtilitiesCS/Extensions/DfDeedle.cs`, established by modification time against the fact that a second
format pass rewrote nothing, so the loop was restarted from step 1; the restart rewrote no file and
its check passed at `Checked 1593 files in 5706ms.` The first pass's check was clean at
`Checked 1593 files in 5673ms.` Analyzer build and nullable build both exit 0 with
`0 Warning(s)` and `0 Error(s)`, matching the baseline of 0. Test run: 6570 total, 6563 passed,
7 failed, 0 skipped, EXIT_CODE 1 matching ExpectedExitCode 1. All nine
`DfDeedleRequiredColumnValidationTests` pass, and the fixed-arity pins `DfDeedle_Tests` (15/15) and
`DfDeedle_COM_Tests` (26/26) pass unmodified. The 7 remaining failures are exactly the 1 AC4 and
6 AC5 members of `BASELINE_FAILURE_SET` and nothing else.
