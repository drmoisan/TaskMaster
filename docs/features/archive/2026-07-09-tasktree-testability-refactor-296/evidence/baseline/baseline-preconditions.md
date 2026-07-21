# Baseline — Structural Preconditions (P0-T6)

Timestamp: 2026-07-09T16-39

Confirmed facts:
- `TaskTree/TaskTreeController.cs` = 546 lines. CONFIRMED (exceeds 500-line limit).
- `TaskTree/TaskTreeForm.cs` = 108 lines. CONFIRMED.
- `TaskTree/TaskTreeForm.Designer.cs` = 311 lines. CONFIRMED.
- `UtilitiesCS/Interfaces/IWinForm/IForm.cs` declares `public interface IForm : IContainerControl, IScrollableControl` (line 8). CONFIRMED.
- `TaskTree/TaskTree.csproj` is a legacy non-SDK packages.config project with explicit `<Compile Include>` items (no glob): AssemblyInfo.cs, TaskTreeController.cs, TaskTreeForm.cs, TaskTreeForm.Designer.cs. CONFIRMED.
- `Tags.Test/Tags.Test.csproj`, `Tags.Test/packages.config`, `Tags.Test/app.config`, `Tags.Test/Properties/AssemblyInfo.cs` all exist (mirror source). CONFIRMED.
- NO `TaskTree.Test/` folder exists. CONFIRMED (ABSENT).
- `TaskMaster.sln` contains the `Tags.Test` project entry at line 37 (`Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Tags.Test", "Tags.Test\Tags.Test.csproj", "{486C1CAE-5C32-406E-963F-79F654EC9B07}"`) and its ProjectConfigurationPlatforms config block at lines 216-227. CONFIRMED.

Binary outcome: all listed facts confirmed. PASS.
