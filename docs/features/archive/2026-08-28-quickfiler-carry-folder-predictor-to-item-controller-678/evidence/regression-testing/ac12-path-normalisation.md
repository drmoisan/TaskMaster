# P1-T9 [expect-fail] — AC12 raw-versus-projected path normalisation

Timestamp: 2026-09-01T23-14

The `[expect-fail]` tag governs the **first** of the two runs recorded here. The second is a normal
pass gate.

## The defect

`FolderScoringService.ScoreAsync` returns the RAW top-suggestion path:

```csharp
string topFolder = predictor.Suggestions.ToArray(1).FirstOrDefault() ?? string.Empty;
```

`FolderPredictor.FolderArray` stores the **projected** form. `FolderPredictor.AddSuggestions` builds
it as `Suggestions.ToArray(5).Select(ProjectSuggestionPath)`, and
`FolderPredictor.ProjectSuggestionPath` strips `_globals.Ol.ArchiveRootPath + "\\"` from the front of
an archive-rooted path, case-insensitively, when the remainder is non-empty.

For an archive-rooted suggestion the two forms therefore differ. `_itemViewer.FolderContains` is
probed with the raw form against a combo box populated from the projected form, the probe misses,
and the selection silently falls back to the index-1 entry. The carried predetermined folder has no
effect at all for exactly the suggestions the archive root is most likely to produce.

## The resolution, and which side was normalised

**The consumer side was normalised.** `AssignFolderComboBox` in
`QuickFiler/Controllers/QfcItemController.FolderHandling.cs` now projects `_predeterminedFolder`
through a new `internal static string ProjectPredeterminedFolder(string folderPath, string
archiveRootPath)` before the containment probe and before `SetFolderSelectedItem`, so the carried
`PredeterminedFolder` and the `FolderArray` entries are compared in the same form.

Two properties of the choice, both deliberate:

- **The projection is duplicated rather than reused.** `FolderPredictor.ProjectSuggestionPath` is
  `private` and lives in `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`, which AC23 forbids
  this change from modifying. Making it accessible would be a change under `UtilitiesCS/`. The
  duplicate mirrors the original statement for statement, and the code comment records why it is a
  duplicate so a later reader does not treat it as an oversight.
- **The projection is the identity when the archive root is null or empty.** That preserves the
  pre-change selection behaviour exactly for the standard path and for every existing test that
  supplies no globals. It is not a convenience: `AssignFolderComboBox_WhenPredeterminedFolderPresent_PreselectsThatFolder`
  passes `\\A\chosen` with `_globals` null, and an unconditional projection would have changed what
  that test observes.

The producer side was considered and rejected. Normalising `FolderScoringService.ScoreAsync` would
also work, but that class is `[ExcludeFromCodeCoverage]` and COM-bound, so the resulting behaviour
could not be pinned by any headless test, and the mismatch would remain latent for any future
producer that publishes a raw path.

This decision is also stated in the change description written by P1-T11, as the plan requires.

## Run 1 — RED, against the unnormalised form

To produce honest fail-before evidence the projection call was temporarily replaced by
`string predetermined = _predeterminedFolder;`, which is the pre-change expression, and the test was
run against that build. The projection was then restored before run 2.

Command:

```
vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll
  /Settings:scripts/vscode/TaskMaster.cli.runsettings
  /InIsolation
  /TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName~AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder
  /Logger:trx
  /ResultsDirectory:TestResults\p1-t9-red
```

EXIT_CODE: 1
ExpectedExitCode: 1

Output Summary:

```
Moq.MockException: the archive-rooted suggestion must be preselected by name once both sides use the same normalisation
Expected invocation on the mock once, but was 0 times: v => v.SetFolderSelectedItem("Projects\Active")

Performed invocations:
   Mock<IItemViewer:1> (v):
      IItemViewer.InvokeRequired
      IItemViewer.AddFolderItems(["\\A\header", "\\A\top", "Projects\Active"])
      IItemViewer.FolderContains("\\Archive\Projects\Active")
      IItemViewer.SetFolderSelectedIndex(1)
      IItemViewer.GetSelectedFolder()

Total tests: 1
     Failed: 1
Test Run Failed.
```

The recorded invocation list is the defect itself, observed rather than described: the combo box was
populated with the projected `Projects\Active`, the containment probe was made with the raw
`\\Archive\Projects\Active`, and the code fell through to `SetFolderSelectedIndex(1)`. Exactly 1
test was discovered and executed, so the failure is a real assertion failure and not a filter that
matched nothing.

## Run 2 — GREEN, after normalisation

Same command with `/ResultsDirectory:TestResults\p1-t9-green`.

EXIT_CODE: 0

```
  Passed AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder [227 ms]
Test Run Successful.
Total tests: 1
     Passed: 1
```

## Acceptance conditions

1. **One side is normalised so the carried `PredeterminedFolder` and the `FolderArray` entries use
   the same form.** The consumer side, as described above.
2. **The new test exists with the mandated name and assertion shape.**
   `AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder` in
   `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` asserts
   `SetFolderSelectedItem` is invoked `Times.Once()` and `SetFolderSelectedIndex(It.IsAny<int>())` is
   invoked `Times.Never()`, mirroring the assertion shape at
   `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs:456-460`.
3. **The two runs used `TestResults\p1-t9-red` and `TestResults\p1-t9-green`.** Recorded above.
4. **The test is recorded as failing against the unnormalised form and passing after.** Both runs
   above, with the failing run's full invocation log.
5. **The chosen normalisation and the reason for choosing that side are stated in the change
   description written by P1-T11.** See `evidence/other/change-description.md`.

### On the assertion argument

The plan states the test "asserts `SetFolderSelectedItem` is invoked once with the archive-rooted
path". The value actually passed is `Projects\Active`, the projected form of the archive-rooted
suggestion `\\Archive\Projects\Active`. That is not a weakening: it is the only form present in the
combo box, because `FolderArray` stores the projection, so it is the form that any correct
implementation must pass. The scenario is the archive-rooted suggestion the criterion names; the
argument is that suggestion as it exists in the control. This reading is recorded explicitly rather
than left implicit.

## Supporting boundary test

`ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection`, in the same file, pins six
boundary cases of the helper directly: null archive root, empty archive root, null path, a path
outside the archive root, a path equal to the root plus a separator with nothing after it, and a
case-differing root. It exists so the helper cannot later be simplified into something that mangles
a non-archive path, and so the identity-projection property the existing tests depend on is asserted
rather than incidental.

## Whole-class re-run after restoration

A scoped run over the entire class
(`FullyQualifiedName~QfcItemController_FolderHandlingTests`,
`/ResultsDirectory:TestResults\p1-t9-class`) reported EXIT_CODE 0, `Total tests: 21`,
`Passed: 21`, `Test Run Successful.` The 21 comprise the 17 pre-existing tests, all unmodified, and
the four added by P1-T3, P1-T8 and P1-T9.

## Test policy

MSTest, Moq and FluentAssertions only. No temporary file. No live Outlook COM: the run carries
`/TestCaseFilter:TestCategory!=LiveOutlook` and the tests construct only Moq objects.

## TRX handling

All TRX files were written under `TestResults\`, which is git-ignored (`.gitignore:39`), and are
referenced here by results directory only. No absolute host path, account name or machine name is
recorded in this artifact.
