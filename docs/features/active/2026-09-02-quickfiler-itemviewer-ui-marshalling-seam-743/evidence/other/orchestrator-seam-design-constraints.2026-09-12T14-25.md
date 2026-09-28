# Orchestrator Note: Seam Design Constraints (Issue #743)

Timestamp: 2026-09-12T14-25
Collected by: orchestrator (preparation mode)
Method: Read and Grep against the worktree; static reading only
EXIT_CODE: 0

Constraints the seam design must satisfy, each established against the current tree. These bound the
solution space before planning begins.

## Constraint 1 — the seam must be additive to `IItemViewer`, never a replacement

`QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` contains reflection-based contract
tests that assert the interface members still exist:

- `IItemViewer_StillDeclaresUiDispatcher` at line 248, which reflects `typeof(IItemViewer).GetMember("UiDispatcher", Flags)`
  at line 255 and asserts the result is not empty at line 260, with the stated reason that "UiDispatcher
  still has production consumers and must survive".
- `IItemViewer_StillDeclaresUiSyncContext` at line 264.

Removing, renaming, or narrowing `UiSyncContext` or `UiDispatcher` on `QuickFiler/Viewers/IItemViewer.cs`
therefore fails existing tests. Any new marshalling member must be added alongside them. This also means
the #511 proposal, replacing the real pump with an injectable context, cannot be executed literally.

## Constraint 2 — the inherited prohibition on a fake `SynchronizationContext` remains in force

The delegation prompt and the #592 constraint note both keep this constraint. Its origin is recorded in
the #511 and #571 closing comments: a context seam executed literally deletes the very tests #571 exists
to stabilize.

Note a nuance the plan must not trip over. `QuickFiler.Test` already contains a
`DrainableSynchronizationContext` test double, used by the breadcrumb tests, for example at
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` lines 103, 135, 165 and 241.
Its existence is not a precedent for using one in the pump-hosted tests. The constraint bars replacing
the REAL PUMP in the pump-hosted fixture; it does not bar a context double in tests that never used the
pump. The plan must state which of the two situations each touched test is in.

## Constraint 3 — `ItemViewer` is a multi-part partial type with an explicit project item list

`QuickFiler/Viewers/` contains `ItemViewer.cs`, `ItemViewer.Breadcrumb.cs`, `ItemViewer.Commands.cs`,
`ItemViewer.Designer.cs`, `ItemViewer.DisplayState.cs`, `ItemViewer.FolderSearch.cs` and
`ItemViewer.WebViewThread.cs`.

`QuickFiler/QuickFiler.csproj` lists each secondary part with a `DependentUpon` child, for example
`<Compile Include="Viewers\ItemViewer.DisplayState.cs">` with `<DependentUpon>ItemViewer.cs</DependentUpon>`.
A new partial must be added to the project file in that shape. These projects are not SDK-style, so an
omitted `Compile` entry silently excludes the file from the build.

## Constraint 4 — existing injectable-dispatcher precedent in this codebase

Two patterns already exist and should be preferred over inventing a third:

- `BreadcrumbUiDispatcher`, an injectable dispatcher abstraction already consumed by the breadcrumb code
  paths and constructed directly in tests.
- `Func<TViewer>` factory injection, used by `QuickFiler/Helper Classes/ViewerQueueCore.cs` at lines 11,
  26, 59, 136 and 146.

## Constraint 5 — the two marshalling paths are distinct and must be treated separately

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` uses two different mechanisms:

- The synchronization-context path, `await _itemViewer.UiSyncContext`, at lines 64, 282, 287, 298 and 303.
- The dispatcher path, `await _itemViewer.UiDispatcher.InvokeAsync(...)`, at line 371.

A seam that covers only one of the two leaves the other unaddressed. The plan must say which sites it
converts and which it deliberately leaves.

## Constraint 6 — coverage visibility

Recorded in full in `orchestrator-citation-verification.2026-09-12T13-50.md`. In summary:
`QuickFiler/Viewers/ItemViewer.cs` carries a type-level `[ExcludeFromCodeCoverage]` at line 20, and
`InitializeWebViewAsync` in `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` carries a
method-level one at line 47 covering the line 64 marshalling site. Acceptance criterion 4 cannot be
phrased as a bare per-file percentage without becoming unfalsifiable.

## Constraint 7 — known concurrent edits by sibling items

Declared by the run scheduler, not to be coordinated with:

- A sibling item adds `CultureInfo.InvariantCulture` and also edits
  `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`.
- Another sibling edits the WebView2 breadcrumb host.

Both overlap this item's likely write set, so the write set must be declared precisely and narrowly to
let the scheduler serialize correctly.

## Output Summary

Seven constraints established. The two binding ones are that the seam must be additive to `IItemViewer`
because reflection contract tests assert both existing members survive, and that the primary cited edit
site is already excluded from coverage measurement. Existing `BreadcrumbUiDispatcher` and `Func<TViewer>`
patterns should be reused rather than a new abstraction invented.
