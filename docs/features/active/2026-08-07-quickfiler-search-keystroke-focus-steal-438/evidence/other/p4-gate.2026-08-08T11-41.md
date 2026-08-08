# [P4-T5] Phase 4 Gate — Contract and Integration Suites Green Unmodified

- **Issue:** #438
- **Task:** [P4-T5]
- **Timestamp:** 2026-08-08T11-41

## Command 1 — scoped contract and integration suites

`pwsh -NoProfile -Command "& 'C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:\"FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests|FullyQualifiedName~BreadcrumbDropDownIntegrationTests|FullyQualifiedName~BreadcrumbDropDownSearchIntegrationTests\" ; exit $LASTEXITCODE"`

- **EXIT_CODE:** 0

```
Total tests: 24
     Passed: 24
     Failed: 0
```

`ItemViewerBreadcrumbDropDownContractTests` passes unmodified, confirming the additive `IItemViewer` member did not disturb the pinned `SetFolderDroppedDown(bool)` signature or the folder event types (AC-10). All 10 `BreadcrumbDropDownIntegrationTests` cases pass unmodified.

## Command 2 — integration file byte-unmodified

`pwsh -NoProfile -Command "git diff --name-only -- QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs"`

- **EXIT_CODE:** 0
- **Output:** empty

`BreadcrumbDropDownIntegrationTests.cs` (500 lines, exactly at the ceiling) is byte-unmodified, as required. The new suite reuses its `internal` `ItemViewerDropDownHarness` and `TrackingMessenger` from a separate file and registers its own 4-parameter `OpenAsync` setup on the shared loose mock.

### Harness caveat resolution

The harness's 3-parameter `OpenAsync` setup carries a `.Callback` that sets the private `_hostOpen` field backing `Host.IsOpen`. A 4-parameter setup cannot reach that private field. Rather than re-registering `SetupGet(h => h.IsOpen)`, the new file drives the harness's existing `internal void SetHostOpen(bool)` seam from the 4-parameter callback:

```csharp
.Callback<Rectangle, Rectangle, Size, bool>((anchor, work, desired, takeFocus) => harness.SetHostOpen(true))
```

This is the harness's own published seam (used by `InitializationFailure_CancelsSessionWithoutDuplicateClose` in the primary file), reaches the same `_hostOpen` field the 3-parameter callback writes, and leaves `BreadcrumbDropDownIntegrationTests.cs` byte-unmodified — the outcome the plan's executor note required. Verified by `PresentFolderSearchResults_TwoConsecutiveRefreshes_OpenOnceAndNeverClose`, which only passes if `Host.IsOpen` flipped after the first search open.

## Files delivered in Phase 4

| File | Change |
|---|---|
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | one-token `partial` |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs` | **new** — `PresentSearchResults` composite (replace -> open-if-closed -> highlight) and the single-publication helper |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | one-token `partial` |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs` | **new** — non-focusing `PresentSearchResults`: latches the open coordinator, performs no `Focus(focus)` in the no-open-coordinator branch |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | in place — `PresentBreadcrumbSearchResults`, whose bare-viewer branch performs no `FocusBreadcrumb()` |
| `QuickFiler/Viewers/IItemViewer.cs` | **additive** `void PresentFolderSearchResults(string[] items)` |
| `QuickFiler/Viewers/ItemViewer.FolderSearch.cs` | in place — thin coverage-exempt forwarding |
| `QuickFiler/QuickFiler.csproj` | two `<Compile Include>` entries |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownSearchIntegrationTests.cs` | **new** — 9 integration tests |
| `QuickFiler.Test/QuickFiler.Test.csproj` | one `<Compile Include>` entry |

`SetBreadcrumbDropDownState(true)` is unchanged, so every explicit gesture keeps its exact current semantics.

### Implementer sweep

`git grep` for `IItemViewer` implementers returned exactly one: `QuickFiler/Viewers/ItemViewer.cs:21` (`public partial class ItemViewer : UserControl, IItemViewer, IContainerControlLocal`). There is no manual (non-Moq) `IItemViewer` test fake, so no additional implementer required updating.

## Render-count design note (AC-8)

`PresentSearchResults` calls three router members synchronously (`ReplaceItemsPreservingSession`, `OpenSelector` when closed, `HighlightRow`) but publishes **once**: only the final handled transition's `RenderJson` is posted, together with one selector-state message. The open-state notification is still raised when the selector actually opened, because that event is what drives the native open through the posted FIFO queue. `SelectionChanged` is never raised. Verified by `PresentFolderSearchResults_RefreshWhileOpen_EmitsOneRenderPerSurface`, which asserts the render count on both the collapsed and popup messengers increases by exactly one per refresh.

## GUI-seam compliance

`ItemViewerDropDownHarness` constructs a `UserControl`-derived `ItemViewer` but never shows it, never forces handle creation, and never runs a message pump; the popup host is a Moq mock, so no native `ToolStripDropDown` is created. No window appears while these tests run.

## Result

- **Output Summary:** EXIT_CODE 0 with 24 of 24 tests passing across the contract, integration, and new search-integration suites. `git diff --name-only` for `BreadcrumbDropDownIntegrationTests.cs` is empty, confirming the 500-line file is byte-unmodified. Accept criteria met.
