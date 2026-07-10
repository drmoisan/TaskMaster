# Baseline — Single Caller (P0-T7)

Timestamp: 2026-07-09T16-40

Verbatim call site `TaskMaster/Ribbon/RibbonController.cs::LoadTaskTree` (lines 88-95):

```csharp
        internal void LoadTaskTree()
        {
            var taskTreeViewer = new TaskTreeForm();
            var dataModel = new TreeOfToDoItems([]);
            dataModel.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadInView, Globals);
            var taskTreeController = new TaskTreeController(Globals, taskTreeViewer, dataModel);
            taskTreeViewer.Show();
        }
```

The 3-argument construction `new TaskTreeController(Globals, taskTreeViewer, dataModel)` and
`taskTreeViewer.Show()` must remain valid without modification after the refactor. The
post-refactor no-edit invariant will be verified by an empty `git diff` on this file (P4-T1, P7-T7).

Binary outcome: verbatim call site recorded. PASS.
