---
name: objectlistview-treelistview-headless-selection
description: BrightIdeasSoftware TreeListView SelectedObject/SelectedIndex need a native window handle; headless MSTest can't select — cache the selected node via SelectionChanged for testability
metadata:
  type: project
---

When replacing a WinForms `ListBox` with a `BrightIdeasSoftware.TreeListView` (ObjectListView 2.9.1, referenced by QuickFiler and QuickFiler.Test): a controller property that reads `FolderListBox.SelectedObject`/`SelectedIndex` cannot be exercised by the existing headless (non-STA, FormatterServices.GetUninitializedObject) QuickFiler.Test controller tests, because ObjectListView selection requires a created native window handle. `SetObjects` stores models but `Items`/`SelectedObject` stay empty until a handle exists.

**Why:** ObjectListView selection goes through the underlying ListView `SelectedIndices`, which is handle-bound. QuickFiler.Test controller tests build the viewer via `FormatterServices.GetUninitializedObject(typeof(EfcViewer))` (no InitializeComponent, no handle) and are not STA, so no handle is ever created.

**How to apply:** Have the controller cache the highlighted node in a field via the TreeListView `SelectionChanged` event (`_selectedNode = tlv.SelectedObject as <Node>`), and derive `SelectedFolder`/validity from `_selectedNode` rather than reading the control live. Production updates the cache on real/programmatic selection (SetObjects + `SelectedIndex=1` fires it). Tests inject `_selectedNode` via reflection. Also: changing the Designer field type from `ListBox` to `TreeListView` breaks any test that assigns `new ListBox()` to it (CS0029) — update those assignments to `new BrightIdeasSoftware.TreeListView()`. TreeListView needs an ISupportInitialize BeginInit/EndInit wrap around its Designer property block, and the tree column must be column 0. Feed it via CanExpandGetter/ChildrenGetter/AspectGetter + SetObjects (not DataSource).
