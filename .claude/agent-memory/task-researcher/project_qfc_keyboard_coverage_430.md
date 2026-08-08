---
name: qfc-keyboard-coverage-430
description: Issue #430 (epic #136 child F3) KeyboardHandler.cs research — MyBox seam unreachable from QuickFiler.Test, headless ItemViewer precedent, 3 unassigned viewer files in the epic
metadata:
  type: project
---

Research completed 2026-08-07 for `quickfiler-keyboard-actions-coverage` (issue #430, epic
`quickfiler-per-file-coverage` #136 child F3). Artifacts in
`docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/research/`
(`01-KeyboardHandler.md`, `02-IQfcKeyboardHandler.md`, `03-QfcFormKeyHandler.md`).

**Why:** epic.md Shared Design §1 ratified that `[ExcludeFromCodeCoverage]` on a *testable* seam is
Blocking — the CLAUDE.md "without an injectable seam" qualifier is a live obligation, not standing
permission. `KeyboardHandler.cs` (414 lines) carried the attribute with zero tests.

**How to apply:** these are non-obvious facts that cost real time to establish; re-verify before
acting, but start from them.

- **`UtilitiesCS` does NOT grant `InternalsVisibleTo("QuickFiler.Test")`** (only
  `DynamicProxyGenAssembly2`, `UtilitiesCS.Test`, `ToDoModel.Test`). So the existing
  `MyBox.DialogInvoker` AsyncLocal dialog-suppression seam is **unreachable from QuickFiler tests**.
  Any QuickFiler production code calling `MyBox.ShowDialog` needs its own local seam; do not reach
  for the UtilitiesCS one. `QuickFiler` itself DOES grant `InternalsVisibleTo("QuickFiler.Test")`.
- **Headless `new QuickFiler.ItemViewer()` works in ordinary `[TestClass]` files** — no
  `*.StaTests.cs` exists in QuickFiler.Test at all. Precedent in three files
  (`BreadcrumbPendingOpenCloseTests.cs:363`, `BreadcrumbCoordinatorLifecycleTests.cs:477`,
  `QfcItemControllerBreadcrumbDropDownTests.cs:373`), each wrapping construction in a
  SynchronizationContext save/restore scope. The STA last-resort clause is usually NOT needed.
- **`ItemViewer.SetFolderDroppedDown` no longer touches a ComboBox** — it forwards to
  `SetBreadcrumbDropDownState`, which returns early when `_breadcrumbLifecycleCoordinator == null`.
  Inert and handle-free on a bare viewer.
- **`ComboBox.DroppedDown` cannot report `true` without a window handle** and its setter
  force-creates one. Any `if (combo.DroppedDown)` branch needs an injectable predicate seam; any
  `combo.DroppedDown = x` assignment must be kept inside a mocked `IUiDispatcher` action that the
  test records but never executes.
- **Epic #136's "every one of the 121 compiled files is assigned to exactly one child" claim has a
  gap.** `QuickFiler/Viewers/QfcFormViewerExpanded.cs`, `QfcFormViewerDark.cs`, and `EfcViewer3.cs`
  consume `IQfcKeyboardHandler`/`QfcFormKeyHandler` but appear in no child's assignment. Flagged in
  all three artifacts; belongs to the epic orchestrator / F16 capstone, not to F3.
- Dead members found in `KeyboardHandler.cs` with zero repo-wide callers: `ClearFilter()` (81),
  `KeyboardHandler_PreviewKeyDown` non-async (96), `GetItemViewer` (247). Line 189
  (`actions.Length == 0`) is provably unreachable. Three unused usings including
  `Microsoft.Office.Interop.Outlook` — the file is not actually Outlook-bound, which is likely how
  it acquired the exemption.

Related: [[qfc227-headless-itemviewer-and-tlpcellsnapshot]] (earlier headless-ItemViewer
confirmation), [[feedback-exemption-audit-check-proven-techniques]].
