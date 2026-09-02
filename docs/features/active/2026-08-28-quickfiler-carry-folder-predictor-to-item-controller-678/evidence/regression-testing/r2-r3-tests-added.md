# R2 and R3 — Test changes landed

- Timestamp: 2026-09-02T01-37
- Issue: #678
- Task: [P1-T6]
- File: `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`

Three edits, and no others in that file.

## Edit 1 — the single authorised assertion correction

The assertion for
`ProjectPredeterminedFolder(@"\\Archive\Projects\Active", string.Empty)` asserted an
identity projection. That parity does not hold.
`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:845-858` guards on `_globals is null`
and then forms `archivePrefix = _globals.Ol.ArchiveRootPath + "\\"` unconditionally, so for a
non-null globals with an **empty** archive root the prefix is a single separator. The path
`\\Archive\Projects\Active` starts with that separator and is longer than it, so
`ProjectSuggestionPath` strips it and returns `\Archive\Projects\Active`.

Before:

```csharp
                .Be(@"\\Archive\Projects\Active", "an empty archive root is the identity");
```

After:

```csharp
                .Be(
                    @"\Archive\Projects\Active",
                    "a non-null globals with an EMPTY archive root gives FolderPredictor an "
                        + "archivePrefix of one separator, which it strips"
                );
```

This is the one correction scope constraint 4 authorises. The surrounding five assertions,
the test name `ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection` and
the `[TestMethod]` attribute are untouched. After the R2 fix that test name becomes accurate
at the `(folderPath, archiveRootPath)` level the test actually exercises, so the test is
neither renamed nor weakened.

## Edit 2 — the R2 boundary test

`AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder`
arranges a `Mock<IApplicationGlobals>` whose `Ol.ArchiveRootPath` returns `string.Empty`,
sets `_predeterminedFolder` to the raw value `@"\Projects\Active"`, sets `_folderHandler`
through `BuildFolderHandlerWithArray` so the folder array holds the projected value
`@"Projects\Active"`, configures the viewer mock so `FolderContains(@"Projects\Active")`
returns true and `GetSelectedFolder()` returns `@"Projects\Active"`, calls
`AssignFolderComboBox()`, and asserts `SetFolderSelectedItem(@"Projects\Active")` exactly
once and `SetFolderSelectedIndex(It.IsAny<int>())` never — the assertion shape used by the
sibling archive-rooted test at `:192-203` of the pre-edit file.

The assertion is made at the `_itemViewer.FolderContains` boundary, which is what R2's
invariant names, rather than on the textual equality of two helper bodies.

## Edit 3 — the R3 cancellation test

`LoadFolderHandlerAsync_WhenCarriedHandlerAndCancelledToken_ObservesCancellation` sets
`_globals`, sets `_carriedFolderHandler` to a mock, injects the sentinel-throwing predictor
factory built by `BuildThrowingPredictorFactoryMock()`, passes the token of an
already-cancelled `CancellationTokenSource` to `LoadFolderHandlerAsync`, and asserts three
things: that an `OperationCanceledException` is thrown; that the private field
`_folderHandler` is null, so the carried handler was not adopted; and that the predictor
factory was invoked `Times.Never()`.

A `using` **statement** is used rather than a `using` declaration. `QuickFiler.Test` compiles
at C# 7.3, where a using declaration is `CS8370: Feature 'using declarations' is not
available in C# 7.3`. The first analyzer build of this task reported exactly that error and
nothing else; converting the declaration to a statement block cleared it. That intermediate
failure was a language-version error in the new test, not a defect in the production code,
and is recorded here rather than as a red run.

## Clause-by-clause acceptance

Both anchored comparisons use `HEAD` rather than
`807fb0bb6e5e49f43efa6b256b05960bf078ca19`, because this file did not exist at the base ref —
the previous cycle created it — so a base-anchored diff would report every line as an
addition and zero removals, and the removal-count clause would pass vacuously. `HEAD` is the
correct anchor at this point because P1-T14 is the first commit this cycle makes and has not
yet run.

| # | Clause | Result |
|---|---|---|
| 1 | exactly two more `[TestMethod]` declarations than at `HEAD` | PASS — **4** at `HEAD`, **6** on disk |
| 2 | exactly one removed line, the corrected expected-value line, and no other removal | PASS — see below |
| 3 | the analyzer build exits 0 | PASS — exit 0, `CoreCompile:` 62 |
| 4 | the file measures at most 500 lines by Derivation D8 | PASS — **354** |
| 5 | MSTest, Moq, FluentAssertions; no temporary file; no live Outlook COM | PASS — see below |

Clause 1 commands:
`git show HEAD:QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`
piped to a `[TestMethod]` count → **4**; the same count over the file on disk → **6**.

Clause 2 command:
`git diff HEAD -- QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`

`--numstat` reports `114  1`, that is 114 added lines and **1** removed line. The single
removed line is:

```
-                .Be(@"\\Archive\Projects\Active", "an empty archive root is the identity");
```

That line was the expected-value line of the corrected assertion, inside the
`ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection` region of the
pre-edit file (`:212-239`). There is no other removal anywhere in the file. The added-line
count is unconstrained by the clause, because the two new tests and the reflow of the
corrected assertion both add lines.

Clause 5 detail. Both new tests use `[TestMethod]` from
`Microsoft.VisualStudio.TestTools.UnitTesting`; both use Moq (`Mock<IItemViewer>`,
`Mock<IApplicationGlobals>`, `Mock<IFolderSearchHandler>`, and the existing delegate-mock
helper); both assert exclusively through FluentAssertions (`Should().Be`, `Should().BeNull`,
`ThrowAsync<T>`) or through Moq's own `Verify` with a `Times` argument, which is the shape
the file's existing tests already use. Neither creates a temporary file, touches the
filesystem, opens a network connection, or starts an external process. Neither requires live
Outlook COM: `FolderController` is the test-local subclass of `QfcItemController`, the folder
predictor is built by `BuildFolderHandlerWithArray` through reflection with a null
`Application`, and every remaining collaborator is a mock. Neither test carries a
`LiveOutlook` category. Both follow Arrange-Act-Assert with explicit section comments.

## Output Summary

Three edits landed in one file. `[TestMethod]` count rose from 4 to 6. The diff against
`HEAD` shows 114 added lines and exactly 1 removed line, that line being the corrected
assertion's expected-value line. The analyzer build exits 0 with 62 `CoreCompile:`
occurrences, so all three tests compile against the current unfixed production code and the
P1-T7 failures will be runtime failures. The file measures 354 lines, 146 short of the cap.
