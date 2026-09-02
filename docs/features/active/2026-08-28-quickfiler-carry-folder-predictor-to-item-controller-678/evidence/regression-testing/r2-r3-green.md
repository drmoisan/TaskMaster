# R2 and R3 — Green run, with the five pins the two fixes must not break

- Timestamp: 2026-09-02T01-48
- Issue: #678
- Task: [P1-T10]

Command (Derivation D7):

```
vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&(FullyQualifiedName~ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection|FullyQualifiedName~AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder|FullyQualifiedName~LoadFolderHandlerAsync_WhenCarriedHandlerAndCancelledToken_ObservesCancellation|FullyQualifiedName~LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory|FullyQualifiedName~LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory|FullyQualifiedName~AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder|FullyQualifiedName~AssignFolderComboBox_WhenPredeterminedFolderPresent_PreselectsThatFolder|FullyQualifiedName~AssignFolderComboBox_PredeterminedFolder_PreselectsByNameAndStillPopulates)" /Logger:trx "/ResultsDirectory:TestResults\p1-t10"
```

EXIT_CODE: 0

## Clause 1 — the pre-run build exits 0

`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"` → exit **0**.

## Clause 2 — exactly 8 tests discovered and executed, all eight named individually

```
A total of 1 test files matched the specified pattern.
Total tests: 8
```

```
  Passed AssignFolderComboBox_PredeterminedFolder_PreselectsByNameAndStillPopulates [204 ms]
  Passed AssignFolderComboBox_WhenPredeterminedFolderPresent_PreselectsThatFolder [224 ms]
  Passed LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory [32 ms]
  Passed LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory [11 ms]
  Passed AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder [6 ms]
  Passed ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection [< 1 ms]
  Passed AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder [< 1 ms]
  Passed LoadFolderHandlerAsync_WhenCarriedHandlerAndCancelledToken_ObservesCancellation [1 ms]
```

None of the eight filter substrings is a substring of another, so each `~` clause selected
exactly the test it names and the count of 8 is not inflated by a prefix collision. The two
`AssignFolderComboBox_When...PredeterminedFolder...` names differ at their fourth token
(`ArchiveRooted` versus `PredeterminedFolderPresent`), and
`AssignFolderComboBox_PredeterminedFolder_PreselectsByNameAndStillPopulates` differs from
both immediately after the shared `AssignFolderComboBox_` prefix.

## Clause 3 — all 8 pass

```
Test Run Successful.
Total tests: 8
```

No `Failed:` line appears and the header is `Test Run Successful.`, so the failed count is 0.

The three tests that failed at P1-T7 now pass with their bodies unmodified. Only production
code changed between the two runs.

## Clause 4 — the two pinned test files are untouched by this cycle

Command:

```
git status --porcelain -- QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs
```

Output: **empty** (no output at all).

Conclusive at this point because P1-T14 is the first commit this cycle makes and has not yet
run, so any modification would still be uncommitted and would appear in porcelain status. A
base-ref-anchored diff cannot serve here: the previous cycle modified
`QfcItemController.FolderHandlingTests.cs` relative to
`807fb0bb6e5e49f43efa6b256b05960bf078ca19`.

## What each pin establishes

- `LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory` (AC7's
  single-initialisation test) passes with a **non**-cancelled token, so the R3 guard does not
  fire on the normal adoption path and the adoption still happens.
- `LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory`
  (AC9's negative guard) passes, so the R3 guard's placement inside the `varList is null`
  branch did not change the `FromArrayOrString` route.
- `AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder` is AC12's
  existing archive-rooted test, which R2 acceptance clause 3 requires to continue passing
  unmodified. It supplies `\\Archive` as the root and is unaffected by the guard change.
- `AssignFolderComboBox_WhenPredeterminedFolderPresent_PreselectsThatFolder` sets no
  `_globals`, so the call site still yields null and the projection is still the identity.
- `AssignFolderComboBox_PredeterminedFolder_PreselectsByNameAndStillPopulates` uses the
  predetermined folder `"Archive\\Finance"`, which has no leading separator, so no strip
  occurs under either guard.

## Output Summary

Pre-run build exit 0. Scoped run discovered and executed exactly 8 tests, named all eight
individually, and all 8 passed; run exit code 0. `git status --porcelain` over the two pinned
test files produced no output, so neither was modified by this cycle. R2 and R3 are both
closed, and AC7's, AC9's and AC12's existing tests all still pass unmodified.
