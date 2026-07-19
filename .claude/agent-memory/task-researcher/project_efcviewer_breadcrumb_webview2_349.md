---
name: efcviewer-breadcrumb-webview2-349
description: Issue #349 (epic folder-tree-breadcrumb-redesign, child 9102) EfcViewer WebView2 breadcrumb research — EfcViewer3 dead, no JS bridge precedent, percent defect = unscaled ColumnHeader widths
metadata:
  type: project
---

Issue #349 (epic `folder-tree-breadcrumb-redesign`, child 9102, wave 1, C4, depends_on 9101):
replace EfcViewer `TreeListView` with WebView2 breadcrumb. Research written 2026-07-16T22-30 to
`docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/research/`.

**Why:** several verified findings contradict the issue's drafted ACs and the naive reading of the epic.
**How to apply:** when planning/reviewing #349 (and sibling #9103), start from these instead of re-deriving.

Key verified non-obvious findings:
- `EfcViewer3` is DEAD: only instantiation is `new EfcViewer()` at `QuickFiler/Helper Classes/EfcViewerQueue.cs:83`;
  `EfcFormController` is typed to concrete `EfcViewer`. AC says "both EfcViewer and EfcViewer3" — the
  EfcViewer3 half is a Designer-only mechanical swap (its FolderListBox has no controller wiring).
- Repo has NO JS->.NET bridge precedent: zero production hits for `WebMessageReceived|PostWebMessage|ExecuteScriptAsync|AddHostObjectToScript`.
  Existing WebView2 usage is one-way `NavigateToString` + the feature-326 `WebResourceRequested` in-memory cid: handler
  (`QfcItemController.ViewerSetup.cs:66-99`). Bridge is the novel surface.
- Percent-obscuring defect primary candidate: `olvColumnFolder.Width=3200` / `olvColumnPercent.Width=500`
  (`EfcViewer.Designer.cs:915,921`) authored at `AutoScaleDimensions (12F,25F)` (line 4250); WinForms font
  autoscaling does NOT rescale `ColumnHeader.Width`, so at normal DPI the 3200px folder column exceeds the
  control's client width and pushes the % column off-viewport. Statics fit (3700 <= 3728 design width) —
  matches "static math shows no overlap". Repro = runtime log of ClientSize vs column widths + screenshot.
- WebView2 SDK 1.0.3912.50 (net462 libs on net481) already in `QuickFiler.csproj:79-86`; supports
  PostWebMessageAsJson/WebMessageReceived/SetVirtualHostNameToFolderMapping. No package change needed.
- Newtonsoft.Json 13.0.4 is approved but NOT referenced by QuickFiler.csproj — put bridge message
  contracts in UtilitiesCS (has Newtonsoft; shared with sibling 9103).
- `EfcFormController` is wholly `[ExcludeFromCodeCoverage]` (line 26) — new logic must live in
  non-exempt router/model classes, not in the controller.
- 9101 provider did not exist on branch at research time; assumed consumer surface documented as
  `IFolderHierarchyProvider.GetAncestorChainAsync/GetImmediateSubfoldersAsync` returning net48-safe
  `readonly struct FolderSegmentInfo {FullPath, DisplayName, HasSubfolders}`. Leaf `HasSubfolders`
  must be cheap (snapshot-backed) — route to 9101 contract review. Existing seams 9101 builds on:
  `OutlookFolderHierarchyReader` internal `IOutlookFolderAdapter` (MAPIFolder.Folders at :270-275),
  `IOutlookFolderTreeService`/`FolderTreeSnapshotNode` (has ChildKeys/ParentKey/FolderPath).
- Behavior-parity items easy to miss in the swap: Up-at-index-0 focuses SearchText
  (`EfcFormController.cs:416-419`), "Trash to Delete" pseudo-row (`:762-770`), banner "====" rejection
  in `IsValidSelection` (`:1076-1088`), 'F' jump-to-list keyboard action (`:601-605`).
- Related: [[qfc-folder-tree-percentage-325]] (prior epic, FolderRow/FolderScore plumbing, net48 no-init constraint).
