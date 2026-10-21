# breadcrumb-router-navigation-defects

- Issue: #498
- Also closes: #439, #440, #499
- Type: bug
- Work Mode: full-bug
- Epic: quickfiler-bug-family
- Integration branch: epic/quickfiler-bug-family-integration
- Last Updated: 2026-08-24

> Provenance note: the four issues below and their potential entries were created and promoted before
> this feature folder existed. No new potential entry and no new GitHub issue were created for this
> feature. `new_active_feature_folder` was the only promotion MCP call made, so this `issue.md` was
> authored by the orchestrator rather than copied from a promoted source. The authoritative
> requirement text for each defect is the promoted potential document cited under each defect.

## Summary

Four defects in the breadcrumb bridge router and folder navigation surface are fixed together as one
feature because they share the same code, the same seams, and the same test fixtures. Two of them
(#439 and #440) are coupled: parent-node navigation is only meaningful once rows carry a resolved
multi-segment ancestor chain.

| Issue | Title | Severity | Surface |
|---|---|---|---|
| #498 | Breadcrumb router segment index unvalidated, host crash | High | Efc |
| #499 | Breadcrumb router stale `SelectedFolderPath` after rebind | High | Efc |
| #439 | EfcViewer missing lineage and segment navigation | High | Efc |
| #440 | Breadcrumb Left/Right arrow parent-child navigation | Medium | Qfc + Efc |

## Defects

### #498 — Out-of-range `segmentIndex` escapes the `async void` host boundary

Authoritative source: `docs/features/potential/promoted/2026-08-08-breadcrumb-router-segment-index-unvalidated-host-crash.md`

An out-of-range `segmentIndex` in a `segmentDoubleClick` message passes codec validation (the codec
validates presence and JSON-integer type only, never range), throws `ArgumentOutOfRangeException` in
`BreadcrumbRow.CollapseAfter`, and escapes `BreadcrumbBridgeRouter.OnHostMessageReceived`, which
catches only `BreadcrumbMessageException`. On .NET Framework 4.8 an exception rethrown on the
captured `SynchronizationContext` from an `async void` method is unhandled, so a malformed message
from the WebView2 document can terminate the Outlook host process.

The router's own XML doc comment states the contract that is violated: a malformed payload should
fail fast with the codec's `BreadcrumbMessageException` and leave state unchanged.

### #499 — `SelectedFolderPath` is not cleared on re-bind

Authoritative source: `docs/features/potential/promoted/2026-08-08-breadcrumb-router-stale-selectedfolderpath-after-rebind.md`

`BreadcrumbBridgeRouter.BindRowsAsync` clears `_selectedRowId` but not `SelectedFolderPath`. After a
re-bind the UI shows no row highlighted while the controller still reports the previously selected
folder. Because the re-bind runs on every search keystroke, a confirm action taken at that moment can
file mail to a folder the user can no longer see selected. The failure is silent — there is no
exception and no error text.

### #439 — Ancestor lineage never resolves; non-leaf segment click does not navigate

Authoritative source: `docs/features/potential/promoted/2026-08-07-efcviewer-missing-lineage-and-segment-navigation.md`

Two distinct sub-defects:

- **A. Path-form mismatch.** Presented row text is an archive-root-relative stem, while
  `OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` matches with exact `OrdinalIgnoreCase`
  equality against a full Outlook folder path (which embeds the store name). Resolution therefore
  always returns null, the ancestor chain is never fetched, and the row builder takes its documented
  single-segment fallback. The row stays visible and selectable, which is why the failure presents as
  cosmetically missing lineage rather than as missing rows.
- **B. No ancestor-navigation gesture.** A segment cell wires only `dblclick`, which posts
  `segmentDoubleClick` and collapses trailing segments. There is no gesture that selects a non-leaf
  ancestor node or expands that ancestor into its children.

### #440 — Left/Right do not perform parent/child tree navigation

Authoritative source: `docs/features/potential/promoted/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation.md`

Both surfaces implement a breadcrumb display-collapse semantic rather than a tree-selection
semantic. Left collapses displayed breadcrumb text; Right re-expands or expands the leaf. The
selected node never moves up the tree, so keyboard-only filing cannot reach a folder that is not
already in the presented row set.

This is a genuine behavior change on both surfaces, not a one-line repair. It must be reconciled
against issue #400, whose landed acceptance criteria state that Left and Right retain the existing
breadcrumb expansion, collapse, and fall-through behavior. The reconciliation is a spec decision.

## Scope

### Files this feature owns

- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`
- `QuickFiler/Controllers/KeyboardHandler.cs`
- `QuickFiler/Resources/FolderBreadcrumb.html`
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`
- `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs`
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs`
- `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs`

### Files this feature must not write

Owned by sibling epic children executing concurrently against the same integration branch:

- `QuickFiler/Controllers/KbdActions.cs` — feature 444. This is a different file from
  `KeyboardHandler.cs`, which this feature does own.
- `QuickFiler/Controllers/EfcFormController.cs` — feature 464.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs` — read-only for this feature unless the
  #439 fix is shown to genuinely require it. The #439 potential explicitly directs that no
  prefix-matching heuristic be added inside the builder.

Any fix that appears to require one of these files is recorded in `spec.md` as a cross-feature note
and kept out of the plan.

## Verification

Per the Bugfix Workflow in `CLAUDE.md`, each of the four defects requires a failing regression test
first, then the minimal targeted fix, then a full toolchain pass.

C# toolchain, in order:

1. `dotnet tool run csharpier format .` (verify `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Test framework: MSTest. Mocking: Moq. Assertions: FluentAssertions.

## Acceptance Criteria

For work mode `full-bug` the authoritative acceptance-criteria source is `spec.md`. This section
records the issue-level closure conditions only.

- [ ] #498 closed: an out-of-range `segmentIndex` leaves row state unchanged, is logged, and does not
      escape the host-message boundary.
- [ ] #499 closed: after a re-bind the controller-visible selected folder agrees with the rendered
      selection.
- [ ] #439 closed: suggestion and search rows render their resolved multi-segment ancestor chain, and
      a non-leaf segment gesture navigates to that ancestor.
- [ ] #440 closed: Left selects the parent node and Right expands the selected node into its
      children, with the same contract on both the Qfc and Efc surfaces, and the #400 reconciliation
      recorded explicitly.
- [ ] Regression test added for each of the four defects, each demonstrated failing before its fix.
- [ ] Full C# toolchain passes in one clean pass.
- [ ] No file outside the owned set is modified.
