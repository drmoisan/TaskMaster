# P1-T7 — AC16 single-initialisation regression test, GREEN

Timestamp: 2026-09-01T23-02

## Preceding build (Derivation D7)

Command: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

## Scoped run (Derivation D7, new results directory)

Command:

```
vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll
  /Settings:scripts/vscode/TaskMaster.cli.runsettings
  /InIsolation
  /TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName~LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory
  /Logger:trx
  /ResultsDirectory:TestResults\p1-t7
```

EXIT_CODE: 0

Output Summary:

```
  Passed LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory [185 ms]
Test Run Successful.
Total tests: 1
     Passed: 1
 Total time: 1.2954 Seconds
```

The identical test that P1-T3 recorded as failing with the sentinel exception now passes. The
results directory is `p1-t7`, distinct from P1-T3's `p1-t3`, so the two runs are told apart.

## What made it pass

`QuickFiler/Controllers/QfcItemController.FolderHandling.cs`, inside `LoadFolderHandlerAsync`'s
`varList is null` branch and before the existing `try`: when `_carriedFolderHandler` is non-null it
is assigned to `_folderHandler`, a debug line is logged in the established
`Probability debug [...]` shape, and the method returns. Neither `_folderPredictorFactory` nor
`FolderPredictor.InitAsync` is reached (AC7).

## AC8 — the un-carried path is unchanged

The adoption is guarded on `_carriedFolderHandler is not null`. With no carried handler the branch
falls through to the pre-existing `try`, which builds a predictor through `_folderPredictorFactory`
and initialises it with `FolderPredictor.InitOptions.FromField`, including both existing catch arms.
No statement of that path was edited.

## AC9 — the `FromArrayOrString` paths are unchanged

The `else` arm of `LoadFolderHandlerAsync` and both branches of the synchronous `LoadFolderHandler`
are byte-identical to the base ref. The adoption sits inside the `varList is null` branch only, so a
carried handler is never adopted on a `FromArrayOrString` call. The negative test is P1-T8.

## AC10 — release in cleanup

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`: `_carriedFolderHandler = null;` was added
immediately after the **first** of the two `_folderHandler = null;` statements, which was at `:465`
at the base ref. The duplicate `_folderHandler = null;` two lines below is pre-existing and was left
in place; removing it is not required by any acceptance criterion and would be an opportunistic
edit. The file is now exactly **500** lines, which is at the cap and not over it, as the plan's
file-size section predicted.

## AC14 — unchanged behaviour

`QfcDequeueStop` handling in `IterateQueueAsync` is unchanged: the `else if (batch.Stop ==
QfcDequeueStop.SourceExhausted)` arm and its `CompleteAddingAsync` call are untouched, and the
empty-batch early return is unchanged. The only edit in that method is the third argument added to
the `EnqueueAsync` call inside the existing `listObjects.Count > 0` guard. The carrier overload of
`LoadItemsAsync` at `QuickFiler/Controllers/QfcFormController.Actions.cs:125-135` was not edited at
all, so it still returns early on `preScored is null` and not on empty, matching the
`IList<MailItem>` overload's condition.

## Acceptance conditions

### 1. The AC16 test passes on a re-run of Derivation D7 with a new `p1-t7` results directory

Recorded above.

### 2-4. The three existing `LoadFolderHandlerAsync` tests pass with their bodies unmodified

### 5. The four existing `AssignFolderComboBox` tests pass with their bodies unmodified

A single scoped Derivation D7 run over the whole class
(`FullyQualifiedName~QfcItemController_FolderHandlingTests`,
`/ResultsDirectory:TestResults\p1-t7-folder`) reported EXIT_CODE 0,
`Total tests: 18`, `Passed: 18`, `Test Run Successful.` Every named test is present in the executed
list by name:

| Test named by the plan | Declared at (base ref) | Result |
|---|---:|---|
| `LoadFolderHandlerAsync_WhenVarListNull_InvokesFactoryWithExpectedArgs` | :230 | Passed |
| `LoadFolderHandlerAsync_WhenVarListProvided_InvokesFactoryWithArrayOrStringArgs` | :264 | Passed |
| `LoadFolderHandlerAsync_WhenPrimaryFactoryThrowsArgumentNull_InvokesEmptyFactoryFallback` | :298 | Passed |
| `AssignFolderComboBox_WhenNoPredeterminedFolder_SelectsTopSuggestionViaViewer` | :416 | Passed |
| `AssignFolderComboBox_WhenPredeterminedFolderPresent_PreselectsThatFolder` | :440 | Passed |
| `AssignFolderComboBox_WhenFolderHandlerNull_DoesNotTouchViewer` | :465 | Passed |
| `AssignFolderComboBox_WhenSingleSuggestionNoPredeterminedMatch_SelectsIndexZero` | :481 | Passed |

**Bodies unmodified, proved by diff rather than asserted.**
`git diff 807fb0bb6e5e49f43efa6b256b05960bf078ca19 -- QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs`
produces exactly one hunk, a single-line change of
`public class QfcItemController_FolderHandlingTests` to
`public partial class QfcItemController_FolderHandlingTests` at `:19`. No other line in that file
differs from the base ref, so every one of the seven test bodies is byte-identical.

The four `AssignFolderComboBox` tests together cover the two cases AC11 names: the
predetermined-folder case (`:440`, which asserts `SetFolderSelectedItem(@"\\A\chosen")` once and
`SetFolderSelectedIndex` never) and the index fallback cases (`:416` selecting index 1, `:481`
selecting index 0 for a single suggestion, and `:465` the null-handler short circuit that touches the
viewer not at all).

Two of these pass **because** of a deliberate property of the AC12 normalisation added by P1-T9 in
this same task's file: `ProjectPredeterminedFolder` returns its input unchanged when the archive
root is null or empty. `_globals` is null in these tests, so `_globals?.Ol?.ArchiveRootPath` is null,
the projection is the identity, and the pre-change selection behaviour is preserved exactly. Had the
projection been unconditional the test at `:440` would have failed.

## The source-text test

`LoadFolderHandler_ProbabilityDebugLog_IncludesCallerSubjectEntryIdAndTopScore`, declared at
`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs:133`, reads
`QuickFiler/Controllers/QfcItemController.FolderHandling.cs` from disk through the
`ReadControllerSource` helper (declared at `:120-130`, ending one line later than the plan's
`:120-129` citation) and asserts five string literals against its source text.

**It passed** in the run above, after this task's edit and after `dotnet tool run csharpier format .`
was applied. All five literals it asserts are intact:
`Probability debug [QfcItemController.LoadFolderHandler (FromField)]`,
`Probability debug [QfcItemController.LoadFolderHandlerAsync (FromArrayOrString)]`,
`Subject='{ItemHelper?.Subject}'`, `EntryID='{ItemHelper?.EntryId}'` and
`TopScore={_folderHandler?.Suggestions?.TopScore() ?? 0}`.

The new `Probability debug [QfcItemController.LoadFolderHandlerAsync (carried)]` line this task adds
uses the same three interpolation fragments, so it reinforces rather than disturbs the assertions.
It will be re-run by P2-T5 after the final format pass. Had it failed, the failure would have been
attributed to a literal moved or reflowed rather than treated as a behavioural regression; it did
not fail.

## TRX handling

Both TRX files were written under `TestResults\p1-t7\` and `TestResults\p1-t7-folder\`, which are
git-ignored (`.gitignore:39`), and are referenced here by results directory only. No absolute host
path, account name or machine name is recorded in this artifact.
