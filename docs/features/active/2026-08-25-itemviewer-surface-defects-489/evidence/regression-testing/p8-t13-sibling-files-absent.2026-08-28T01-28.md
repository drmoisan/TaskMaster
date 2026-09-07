# P8-T13 — The forbidden sibling-owned viewer files are absent from the diff

Timestamp: 2026-08-28T01-28
Command: git diff --name-only <BASELINE_SHA> -- QuickFiler/ QuickFiler.Test/ UtilitiesCS/
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance

None of the three forbidden paths appears in the changed-path list:

```
QuickFiler/Viewers/ItemViewer.Breadcrumb.cs              = 0   (488-owned)
QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs        = 0   (501-owned)
QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs = 0   (501-owned)
```

`BreadcrumbBridgeCoordinator.Search.cs` carries a comment referring to the old `SetFolderItems` name.
That comment is **deliberately left stale**: the file is owned by live sibling 501 and editing it to
follow this rename would create a conflict in a file this feature does not own. The staleness is a
documentation matter for 501, not for this feature.

## The full changed-path list, 25 entries

```
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs
QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs
QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs
QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs
QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs
QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs
QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs
QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs
QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs
QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs
QuickFiler/Controllers/QfcItemController.EventHandlers.cs
QuickFiler/Controllers/QfcItemController.EventWiring.cs
QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs
QuickFiler/Controllers/QfcItemController.FolderHandling.cs
QuickFiler/Controllers/QfcItemController.MailActions.cs
QuickFiler/Viewers/IItemViewer.cs
QuickFiler/Viewers/ItemViewer.Designer.cs
QuickFiler/Viewers/ItemViewer.DisplayState.cs
QuickFiler/Viewers/ItemViewer.FolderSearch.cs
QuickFiler/Viewers/ItemViewer.cs
QuickFiler/Viewers/ItemViewerExpanded.Designer.cs
QuickFiler/Viewers/ItemViewerExpanded.cs
```

Three of the 25 match `*Breadcrumb*`, and all three are under `QuickFiler.Test/Viewers/`:
`BreadcrumbDropDownIntegrationTests.cs` and `BreadcrumbSelectorOpenRetryTests.cs` carry P8-T7's
line-neutral invocation renames, and `ItemViewerBreadcrumbDropDownContractTests.cs` is this feature's
own contract-test landing zone. **No** path under `QuickFiler/Viewers/` matches `*Breadcrumb*`, which
is the stronger form of the same assertion: no production breadcrumb file was touched at all, not
merely none of the three named ones. `UtilitiesCS/` contributes no changed path whatsoever.

## Exit-code determination

`git diff --name-only` is not subject to the zero-match exit-code ambiguity that affects
`git grep`. `git grep` exits `1` natively when it matches nothing, and wrapping it in
`(… | Measure-Object).Count` does not reset `$LASTEXITCODE`, so a zero-match `git grep` assertion
cannot report its own success from the exit code. This gate uses `git diff`, which exits `0`
whenever it completes regardless of how many paths it prints, and the observed values were
unambiguous and mutually consistent:

```
GIT_LASTEXITCODE = 0
$?               = True
$Error.Count     = 0   (under $ErrorActionPreference = 'Stop')
```

`EXIT_CODE: 0` above is therefore the genuine process exit code, not a reconstruction. The
absence assertion is evaluated in-process against the returned path array, so it cannot be
confounded by a filter's own exit code.

Output Summary: All three forbidden sibling-owned viewer files are **absent** from
`git diff --name-only <BASELINE_SHA> -- QuickFiler/ QuickFiler.Test/ UtilitiesCS/`, which lists 25
changed paths. No production file under `QuickFiler/Viewers/` matching `*Breadcrumb*` was touched at
all, and `UtilitiesCS/` contributes no changed path. `EXIT_CODE: 0` is the genuine `git diff` exit
code, corroborated by `$? = True` and `$Error.Count = 0`.
