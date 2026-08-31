---
name: selectrow-two-families-637
description: "#637: TWO unrelated SelectRow families (bare grep over-counts ~10x); blanket TryMakeArchiveRelative in SelectRow would reject every relative/Trash row; ButtonOK_Click does NOT rethrow (delegates to a catch-all ButtonOkClickAsync)"
metadata:
  type: project
---

Issue #637 research (2026-08-29), branch `bug/breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637`.

**Three findings that a single-pass reading gets wrong:**

1. **`SelectRow` names two unrelated surfaces.** Family A (in scope) = private `BreadcrumbBridgeRouter.SelectRow(BreadcrumbRow)` / `SelectHierarchyPath(BreadcrumbRow, string)` in `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs`. Family B = `SelectRow(int index)` on `BreadcrumbStateModel` / `BreadcrumbSelectionSession` / `FolderBreadcrumbBridgeRouter` / `BreadcrumbBridgeCoordinator` (the ItemViewer drop-down selector). A bare grep for `SelectRow` returns ~106 lines / 34 files; only 6 are Family A. Family A has exactly 2 declarations + 7 call sites, ALL in production, ZERO in tests (both members are private; tests drive them via `ProcessInboundAsync` / `SelectFirstRow`).

2. **A blanket "commit only when `TryMakeArchiveRelative` succeeds" rewrite of `SelectRow` is a REGRESSION.** `TryMakeArchiveRelative` returns FALSE for an already-relative value (`Clients\North`) and for the `Trash to Delete` sentinel, because it is a rooted-prefix test. The change must stay nested inside the existing `ArchiveStemContract.IsFullOutlookPath(selection)` arm. The actual defect is only the negated third conjunct + the missing `stem.Length == 0` clause.

3. **`EfcFormController.ButtonOK_Click` is `async void` but does NOT rethrow.** It delegates to `ButtonOkClickAsync`, which wraps everything in `catch (System.Exception) { BoundaryErrorSink(...) }` (log-only, injectable seam). The real button-path defect is that `ActionOkAsync` calls `_formViewer.Hide()` BEFORE `await ExecuteMovesAsync()` and `Dispose()/Cleanup()` AFTER, so a throw leaves the form hidden-and-undisposed with no user message. The genuinely unhandled paths are the keyboard ones: the always-on `Keys.Return` action registering `ActionOkAsync` directly, and `KbdExecuteAsync(ActionOkAsync)` for `'K'` — `KbdExecuteAsync` has no try/catch.

**How to apply:** when an issue body asserts an async-void handler "rethrows", verify the delegated `*Async` body before accepting it; several EFC handlers in this repo follow the delegate-to-a-catch-all pattern. When counting a method family, always disambiguate same-named members on unrelated types first — see [[feedback-exemption-audit-check-proven-techniques]] for the sibling-consistency habit.

Reusable seam facts: `EfcHomeController.MoveFailureMessageAction` (`internal Action<string>` defaulting to `MessageBox.Show`) is the repo's established injectable abort-notification pattern; `ExecuteMovesAsync`'s existing try/finally is the narrowest seam for a benign degrade. `BreadcrumbBridgeRouterIssue439Tests.cs` is already 694 lines (over the 500 limit) — put new tests elsewhere.

Full research: docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637/research/research.2026-08-29T12-30.md
