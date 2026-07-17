---
name: qfc-breadcrumb-webview2-351
description: Issue #351 (epic 9103) QuickFiler WebView2 breadcrumb research — 9101 provider absent, bridge is greenfield, Theme TODO is top percentage hypothesis
metadata:
  type: project
---

Issue #351 (epic `folder-tree-breadcrumb-redesign`, child 9103, wave 1, C4): replace `CboFolders`
ComboBox with a WebView2 HTML/CSS/JS breadcrumb in the live `ItemViewer`. Research written
2026-07-16T22-30 to `docs/features/active/2026-07-16-quickfiler-breadcrumb-webview2-351/research/`.

**Why:** several load-bearing facts are not derivable from the feature docs and were verified.
**How to apply:** cite these when planning/executing #351 or sibling 9102, and re-verify 9101
presence before execution.

Key verified non-obvious findings:
- Liveness re-confirmed unchanged from [[qfc-folder-tree-percentage-325]]: only `ItemViewer` is
  constructed (`ItemViewerQueue.cs:105`, hard-bound `ViewerQueueCore<ItemViewer>`); 9 variants dead.
- **9101 provider does NOT exist on the branch** (no feature folder, no interface). Probable
  substrate: `IOutlookFolderHierarchyReader` + `FolderTreeSnapshotNode` (has ParentKey/ChildKeys)
  in `UtilitiesCS.OutlookObjects.Folder` — predates the epic. Plan codes against an assumed
  `IFolderHierarchyProvider` (GetAncestorChainAsync / GetImmediateSubfoldersAsync), flagged
  ASSUMED-PENDING-9101-MERGE.
- **JS<->.NET bridge is greenfield**: zero repo usages of WebMessageReceived /
  AddHostObjectToScript / ExecuteScriptAsync. Existing WebView2 use is one-way NavigateToString;
  init via injected `IWebViewCoreInitializer` (`QfcItemController.ViewerSetup.cs:36`), shared
  env in `%LocalAppData%\WindowsFormsWebView2`, options string `"–incognito "` (en-dash typo,
  silently ignored — reuse verbatim for env compat). SDK pinned 1.0.3912.50 in all csprojs.
- Percentage-obscuring top hypothesis: `Theme.Rendering.cs:96-98` sets CboFolders fore/back with
  literal TODO "colors do not work as expected"; owner-draw geometry reserves a clean 46px right
  column (no overlap). Secondary: dropdown scrollbar overlay, DPI clipping. Runtime repro required.
- Two population paths must both survive: FolderRow suggestions (`AssignFolderComboBox`,
  FolderHandling.cs:161-200, only prod caller of `FolderHierarchyBuilder.Build` is
  `ItemViewer.FolderSearch.cs:26`) AND plain `string[]` search results
  (`TextBoxSearch_TextChanged`, EventHandlers.cs:164-178). `"Trash to Delete"` is a literal string
  contract (MailActions.cs:90). Selection output = full folder path string (`GetSelectedFolder`).
- Keyboard Left/Right routing is ComboBox-shaped (`KeyboardHandler.cs:543-583`,
  `CboFolders_KeyDownAsync` throws on non-ComboBox sender); browser consumes arrows, so bridge
  must report unhandled arrows for legacy fall-throughs (Right -> Pop Out/Enumerate dialog).
- HTML asset precedent: `Resources\EmailHeader.html` Content include (QuickFiler.csproj:495);
  recommend embedded string + NavigateToString over virtual-host folder mapping.
- Host-neutral precedent to mirror: `FolderTreeStateModel` + `FolderNodeViewModel` (not exempt);
  glue stays in exempt ItemViewer partials. net481 everywhere (no record/init).
