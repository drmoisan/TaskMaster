---
name: qfc438-search-focus-steal
description: 'Issue #438: QuickFiler search keystroke focus steal has TWO mechanisms (open-side _focusPending AND close-side _focusAnchor via per-keystroke Clear); SelectRow-while-open + CancelSelector-no-SelectionChanged leaves stale _selectedFolder'
metadata:
  type: project
---

Issue #438 research (2026-08-08, artifact at `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/research/2026-08-08T10-30-...-research.md`).

**Why:** the issue body only recorded the open-side focus steal; the close-side mechanism and the stale-selection defect were found in this session and change the required fix shape.
**How to apply:** any plan/fix for #438 (or future breadcrumb focus work) must address BOTH mechanisms; suppressing only `_focusPending` is insufficient.

Key verified non-obvious findings:
- Two focus steals per typing cycle: (1) open pipeline `BreadcrumbDropDownOpenLifetime.FocusCurrentSurface` -> `_host.FocusPending()` (line 294) and the already-open branch `BreadcrumbDropDownHost.OpenAsync:237` `Schedule(_focusPending)`; (2) close pipeline `FinishClose` (Host.cs:427-437) ALWAYS invokes `_focusAnchor` (= FocusBreadcrumbCore) — and the search handler's per-keystroke `ClearFolderItems()` closes the open session (`ClearSelector` emits OpenStateChanged), so every keystroke while open closes+reopens the popup (churn + both steals).
- `SelectRow` while a selector session is open mutates the MODEL selection (collapsed surface + GetSelectedFolder) but not session CommittedIdentity; it always raises SelectionChanged -> controller caches `_selectedFolder`. `CancelSelector` emits NO SelectionChanged (BreadcrumbSelectionSession.cs:297-304), so after Escape the controller `_selectedFolder` stays stale at the mid-search row.
- Router already has session-preserving replacement primitives: `ReplaceRowsPreservingSession` + `ReconcileRowsReplaced` (suggestions path). `FolderBreadcrumbBridgeRouter.SetItems` (:119-135) exists but is UNREACHABLE from the coordinator and does NOT reconcile the session — don't reuse as-is.
- Focus-intent transport gotcha: `openCoordinator.SetDroppedDown(true)` triggers the native open indirectly via the SelectorOpenStateChanged EVENT -> HandleSelectorOpenStateChanged -> RequestOpen, so a takeFocus parameter must be latched in the open coordinator (deterministic: same FIFO BreadcrumbPopupUiOperations queue).
- Removing focus from the open pipeline entirely breaks #400 AC-13 for mouse-toggle opens: `FocusFolderDropDown()` focuses the collapsed ANCHOR WebView2, not the popup; `_focusPending` is the only popup-focus path.
- Recommended (Option 3): composite `IItemViewer.PresentFolderSearchResults(string[])` + router `ReplaceItemsPreservingSession` + session `HighlightRow` (pending-only) + additive `IBreadcrumbDropDownHost.OpenAsync(..., bool takeFocus)` overload; ~12 production files. #400 AC-13 needs a sanctioned gesture-scoped qualification (search opens non-focusing).
- Test spec: only `QfcItemController.EventHandlersTests.cs:313-350` pins the defective search composition (must be rewritten); Down-arrow tests (:355-388), BreadcrumbDropDownIntegrationTests, BreadcrumbDropDownHostTests FocusPendingCount cases all pin default paths and stay green. `ItemViewerDropDownHarness` (BreadcrumbDropDownIntegrationTests.cs:328-473) = headless ItemViewer + Mock<IBreadcrumbDropDownHost>, ideal regression seam.
- EfcViewer search (`EfcFormController.SearchText_TextChanged:556`) is NOT the same defect: persistently visible WebView2, NavigateToString re-render, no managed focus call in the path.
- Residual non-automatable checks: CoreWebView2 native focus grab on first popup creation and ToolStripDropDown AutoClose behavior while typing — live-Outlook-only; recommend #400-style documented exception, not a merge gate.
