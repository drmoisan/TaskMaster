# QA Gate — Wiring and File Size (P4-T2)

Timestamp: 2026-07-09T17-03

## `TaskTree/TaskTree.csproj` Compile includes (new files wired)
- `<Compile Include="ITaskTreeForm.cs" />` (line 66)
- `<Compile Include="TreeListViewVisual.cs" />` (line 67)
- `<Compile Include="TaskTreeController.cs" />` (line 68)
- `<Compile Include="TaskTreeController.MoveLogic.cs" />` (line 69)
- `<Compile Include="TaskTreeForm.cs">` (line 70)
- `<Compile Include="TaskTreeForm.Designer.cs">` (line 73)

All three new `.cs` files (`ITaskTreeForm.cs`, `TreeListViewVisual.cs`, `TaskTreeController.MoveLogic.cs`) are explicitly wired (legacy explicit-include project requires this).

## Production file line counts (all <= 500)
| File | Lines |
|---|---|
| TaskTree/TaskTreeController.cs | 184 |
| TaskTree/TaskTreeController.MoveLogic.cs | 287 |
| TaskTree/ITaskTreeForm.cs | 79 |
| TaskTree/TreeListViewVisual.cs | 45 |
| TaskTree/TaskTreeForm.cs | 194 |
| TaskTree/TaskTreeForm.Designer.cs | 311 |

Binary outcome: all new files wired AND every production file <= 500 lines. PASS.
