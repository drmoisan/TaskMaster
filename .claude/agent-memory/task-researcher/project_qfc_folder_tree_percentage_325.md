---
name: qfc-folder-tree-percentage-325
description: Issue #325 (epic child 9003) QuickFiler folder-tree+percentage research — only 1 live viewer variant despite 9 CboFolders declarations
metadata:
  type: project
---

Issue #325 (epic `folder-tree-percentage-ui`, child 9003, wave 1, C3): QuickFiler `CboFolders`
dropdown gets EfcViewer-parity tree expand/collapse + right-aligned whole-number percentage.
Research written 2026-07-15T16-43 to `docs/features/active/2026-07-15-quickfiler-folder-tree-percentage-325/research/`.

**Why:** epic estimates "up to nine variants" of `CboFolders`, but only ONE is live.
**How to apply:** when planning/executing #325, scope the functional change to `ItemViewer` only.

Key verified non-obvious findings:
- Ten types declare `CboFolders`, but only `ItemViewer` is instantiated at runtime
  (`QuickFiler/Helper Classes/ItemViewerQueue.cs:105` `new ItemViewer()`). The other nine
  (Form1, ItemViewerExpanded, QfcItemViewer, QFCItemViewerDarkNew, QfcItemViewerExpanded,
  QfcItemViewerExpandedLight, QFCItemViewerLightNew, QfcItemViewerLightSelected, QfcItemViewerV1)
  are dead Designer-field variants, each `[ExcludeFromCodeCoverage]`. Required-change count = 1.
- Live seam: `IItemViewer.SetFolderItems(string[])` -> `ItemViewer.FolderSearch.cs:13` -> `CboFolders.Items`.
  Controller build path: `QfcItemController.FolderHandling.AssignFolderComboBox` + `.EventHandlers.TextBoxSearch_TextChanged` + `.MailActions` ("Trash to Delete").
- Probability is discarded today at `FolderScorer.ToArray(int)` (only path keys survive). `Prediction<T>.Probability`
  is a double [0,1] fraction (FolderScorer multiplies *1000). #325 consumes upstream 9001 contract (folder identity + probability); does NOT recompute.
- Owner-draw precedent already exists: `QfcItemViewerLightSelected.cs:46 CboFolders_DrawItem(DrawItemEventArgs)`.
  Recommended approach = owner-draw existing ComboBox + host-neutral visible-row projection (keeps seam, low churn).
- Reusable host-neutral `TreeNode<T>` at `UtilitiesCS/ReusableTypeClasses/Other/TreeNodeOfT.cs` and
  `FolderTreeNodeKey` at `UtilitiesCS/OutlookObjects/Folder/FolderTreeNodeKey.cs` — reuse for hierarchy builder.
- Coverage discrepancy flagged: CLAUDE.md says line >=80% / new modules >=90%; `.claude/rules/general-unit-test.md`
  + quality-tiers.md say uniform line >=85% / branch >=75%. Plan to the stricter bar.
- net48 constraint applies to the 9001 DTO: no record/record struct/init (see [[reference_net48_no_init_record_struct]] in user auto-memory) — use plain class or readonly struct w/ explicit ctor.
- 9004 non-interference: forbidden files = `MailItemHelper.Html.cs`, `ItemViewer.WebViewThread.cs`,
  `WebView2CoreInitializer.cs`, `IWebViewCoreInitializer.cs`, WebView2/NavigateToString members. Disjoint from #325.
- Both `QuickFiler.Test` and `UtilitiesCS.Test` are legacy non-SDK net4.8.1 — new test files need explicit `<Compile Include>`.
