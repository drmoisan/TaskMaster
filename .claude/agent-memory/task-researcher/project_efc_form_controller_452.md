---
name: efc-form-controller-452
description: "#452/epic #136 F9 EfcFormController research: IQfcFormViewer:IForm is the proven de-COM pattern; ViewerQueueCore.Dequeue does NOT pool; #439 is a path-namespace mismatch; no STA needed"
metadata:
  type: project
---

Research for `QuickFiler/Controllers/EfcFormController.cs` (issue #452, epic #136 child F9), 2026-08-07.
Artifact: `docs/features/active/2026-08-07-quickfiler-efc-form-item-controller-coverage-452/research/EfcFormController.research.md`.

**Why:** F9 must remove `[ExcludeFromCodeCoverage]` (line 27) from a 1,086-line COM+WinForms controller,
split it under 500 lines, and reach 80% line / 75% branch — without editing F5/F8/F12/F13/F14 files.

**How to apply:** reuse these verified facts before re-deriving them on any EFC/QFC controller work.

## Load-bearing findings

- **The merged de-COM pattern for a QuickFiler form controller is already in-tree.** `QfcFormViewer.cs:18`
  is `public partial class QfcFormViewer : Form, IQfcFormViewer`; `IQfcFormViewer.cs:12` is
  `public interface IQfcFormViewer : IForm`; `QfcFormController.cs:168` holds `IQfcFormViewer _formViewer`;
  `QfcFormControllerTests.cs:103` mocks it. Copy this triple rather than inventing a seam.
- **`UtilitiesCS.Interfaces.IWinForm.IControl` inherits `IComponent, IDropTarget, ISynchronizeInvoke,
  IWin32Window, IDisposable, IBindableComponent`** (`IControl.cs:9-15`). So an `IForm`-derived interface
  gets `Handle`, `Dispose()`, `Invoke(Delegate)`, `BeginInvoke(Delegate)` for free — they are NOT declared
  anywhere in the `IWinForm` folder, which makes a naive grep say they are missing.
- **`ViewerQueueCore.Dequeue` is consume-once, not a pool** (`ViewerQueueCore.cs:63-85`): it dequeues then
  refills with `new EfcViewer()`. Do not plan around "pooled viewer reuse causes handler accumulation" —
  the `WebView2BreadcrumbHost` XML doc's "pooled-viewer re-initialization" language is defensive, not
  descriptive.
- **`BreadcrumbBridgeRouter` (F12, `public sealed`) is fully constructible headlessly** over
  `Mock<IFolderHierarchyProvider>` + `Mock<IBreadcrumbWebHost>` + three plain classes. Tests should build a
  REAL router rather than seeking an interface over it — no F12 edit needed. `CoreInitialized` lives on the
  concrete `WebView2BreadcrumbHost` (`:63`), NOT on `IBreadcrumbWebHost`.
- **`EfcDataModel` (F5) and `EfcHomeController` (F8) are concrete with zero virtual members** → Moq cannot
  mock them, and both are sibling-owned. Use the already-merged in-family injectable-delegate idiom
  (`EfcHomeController.ExecuteMoves.cs:86-109`, `EfcHomeController.cs:294-305`): an `internal` settable
  delegate defaulting to null with an `X is null ? concrete : X` fall-through at the call site.
- **No STA last-resort tests are needed for `EfcFormController`.** After the viewer interface seam plus a
  pure layout-math extraction, every remaining WinForms touch is either on the mocked viewer or on an
  unparented `Button`/`CheckBox` that needs no handle.
- **`QuickFiler` DOES grant `InternalsVisibleTo("QuickFiler.Test")`** (`QuickFiler/Properties/AssemblyInfo.cs:5`)
  — unlike `UtilitiesCS`, which does not. `internal` seams on QuickFiler types are directly reachable.
- **Issue #450 concerns `QfcFormControllerTests.cs` (827 lines), not the EFC tests.**

## Issue #439 mechanism (open bug running through this controller)

Path-namespace mismatch, not a rendering bug: `FolderPredictor.AddSuggestions`
(`FolderPredictor.cs:804-808`) emits relative folder **stems**, but
`OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` (`OutlookFolderHierarchyProvider.cs:64-68`) matches on
the rooted Outlook `node.FolderPath`. Every lookup returns null, so `BreadcrumbRowBuilder` falls back to
single-segment rendering. F9 must NOT fix it; characterization tests must pin current pass-through
behavior. The eventual fix point is the provider construction at `EfcFormController.cs:840-842`.

## Epic manifest defect

Epic `docs/features/epics/quickfiler-per-file-coverage/epic.md` corrected the exemption count 33 -> 21 in
its marker-accuracy note, but lines 224 and 324 still say "the 33 existing `[ExcludeFromCodeCoverage]`
attributes". F1's ledger must use 21.

Related: [[quickfiler-percoverage-epic-136]], [[quickfiler-interface-only-files-433]],
[[efc-home-controller-coverage-437]], [[efc-home-controller-deps-437]],
[[qfc-breadcrumb-webview2-351]], [[efcviewer-breadcrumb-webview2-349]],
[[folder-hierarchy-provider-350]], [[qfc-helper-classes-f4-434]]
