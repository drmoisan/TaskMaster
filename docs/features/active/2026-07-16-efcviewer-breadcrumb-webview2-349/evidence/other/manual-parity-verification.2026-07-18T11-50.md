# Behavior-Parity Manual Verification Checklist (P8-T2)

Timestamp: 2026-07-18T11-50
Status: REMEDIATION-REQUIRED on all eight items (manual verification outstanding) — a live
Outlook runtime session is structurally unavailable to the executing agent, so no item is
recorded as a runtime PASS. Each item lists the automated evidence that exists today and the
outstanding manual step.

| # | Parity item | Verdict | Automated evidence available (not a runtime pass) |
|---|---|---|---|
| 1 | Up-at-top focuses `SearchText` | remediation-required | Router posts `focusSearch` and raises `FocusSearchRequested` (BreadcrumbBridgeRouterTests.ArrowKeyUp_AtTopSelectableRow_PostsFocusSearchAndRaisesEvent); controller wires `FocusSearchRequested -> SearchText.Select()` (EfcFormController.ConfigureBreadcrumbControl). |
| 2 | SearchText down-arrow enters the list and selects the first row | remediation-required | `SearchText_DownArrow` focuses the WebView2 control and calls `Router.SelectFirstRow()`; SelectFirstRow behavior covered by BreadcrumbBridgeRouterTests.SelectFirstRow_SelectsTopSelectableRowAndPostsRender. |
| 3 | `"Trash to Delete"` pseudo-row selectable after delete | remediation-required | `ActionDeleteAsync` prepends the pseudo-row and rebinds through `Router.BindRowsAsync`; trash classification/selectability covered by BreadcrumbRowBuilderTests + BreadcrumbHtmlRendererTests.RenderRowFragment_TrashPseudoRow_IsSelectableWithoutAffordance. |
| 4 | `"===="` banner rows non-interactive and rejected as filing targets | remediation-required | Renderer emits non-interactive banner markup (RenderRowFragment_BannerRow_IsNonInteractive); router never selects banners (RowSelected_OnBannerRow_IsIgnored); `IsValidSelection` retains its `"===="` rejection unchanged. |
| 5 | `'F'` focuses the breadcrumb control | remediation-required | The `'F'` KbdAction still targets `_formViewer.FolderListBox`, now the WebView2 control (unchanged `JumpToAsync(_formViewer.FolderListBox)` wiring). |
| 6 | Dark-mode toggle re-themes the document | remediation-required | `DarkMode_Changed` routes to `Router.ApplyTheme(DarkMode)`; dark re-render covered by BreadcrumbBridgeRouterTests.ApplyTheme_Dark_ReDeliversDarkDocument and renderer theme tests. |
| 7 | Leaf expand lists real Outlook subfolders (incl. one not among ranked suggestions) | remediation-required | Router issues `GetImmediateSubfoldersAsync` via the 9101 provider with requestId correlation (LeafExpandToggle_IssuesSubfolderQueryAndPostsCorrelatedResult); no suggestion-row prefix matching exists in the breadcrumb path. Live-subfolder observation requires Outlook. |
| 8 | Selection feeds filing via `SelectedFolder` | remediation-required | `SelectedFolder` derives from `Router.SelectedFolderPath` (RowSelected_UpdatesSelectedFolderPathAndRaisesEvent); ExecuteMoves path re-verified green (EfcHomeControllerExecuteMovesTests, 7/7). |

Required remediation: run the eight checks in a live Outlook session and update each verdict to
PASS/FAIL with observations before treating spec AC-10 (and the runtime aspects of AC-1..AC-4)
as fully verified.
