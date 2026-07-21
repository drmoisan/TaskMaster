# [P9-T1] File-Size Check (<= 500 lines)

Timestamp: 2026-07-10T06:06:59Z
Command: `wc -l` over every `.cs` file changed vs `origin/epic/winforms-testability-refactor-integration...HEAD`

## Result: PASS — all in-scope files <= 500 lines

### Touched production files

| Lines | File |
|------:|------|
| 289 | TaskVisualization/EditFilterController.cs |
| 261 | TaskVisualization/AutoCreateProject.cs |
| 207 | TaskVisualization/FlagTasks.cs |
| 131 | TaskVisualization/AutoAssignPeople.cs |
| 129 | TaskVisualization/EditFilterViewer.cs |
| 115 | TaskVisualization/AutoAssignContext.cs |
| 100 | TaskVisualization/ManageFiltersController.cs |
|  96 | TaskVisualization/FlagCalculations.cs |
|  89 | TaskVisualization/ManageFilters.cs |
|  62 | TaskVisualization/IEditFilterViewer.cs |
|  39 | TaskVisualization/IManageFiltersViewer.cs |

### Touched test files

| Lines | File |
|------:|------|
| 205 | TaskVisualization.Test/AutoCreateProjectTests.cs |
| 160 | TaskVisualization.Test/EditFilterControllerTests.cs |
| 134 | TaskVisualization.Test/ManageFiltersControllerTests.cs |
| 102 | TaskVisualization.Test/FlagCalculationsTests.cs |
|  97 | TaskVisualization.Test/AutoAssignPeopleTests.cs |
|  88 | TaskVisualization.Test/AutoAssignContextTests.cs |
|  72 | TaskVisualization.Test/FlagChangeGroupTests.cs |
|  69 | TaskVisualization.Test/FlagChangeTrainingQueueTests.cs |
|  40 | TaskVisualization.Test/FlagChangeItemTests.cs |

Maximum touched-file size: **289 lines** (`EditFilterController.cs`), well under the
500-line limit.

## Designer carve-out (explicitly noted)

`TaskVisualization/EditFilterViewer.designer.cs` is **503 lines**. It is
Designer-generated code, carries the form partial's class-level
`[ExcludeFromCodeCoverage]`, and was **NOT** modified or hand-split by #298 (it does
not appear in the change set). Per Scope Lock and General Code Change Policy §4, raw
Designer-generated code is not subject to hand-splitting; splitting it would risk the
Windows Forms designer round-trip. No violation.

## Note

The two modified `.csproj` files (`TaskVisualization.csproj`,
`TaskVisualization.Test.csproj`) are XML project files, not source code, and are not
subject to the 500-line source limit; both received only `<Compile Include>` /
`<ProjectReference>` additions.
