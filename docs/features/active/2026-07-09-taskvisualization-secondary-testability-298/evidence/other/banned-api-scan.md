# Phase 0 — Banned-API Scan (P0-T8)

Timestamp: 2026-07-10T01-17
Command: `grep -rnE "Thread\.Sleep|Task\.Delay|DateTime\.(Now|UtcNow)|DateTimeOffset\.Now|Random\.Shared" <in-scope files>`
Policy: `.claude/rules/csharp.md` (banned APIs), `.claude/rules/general-unit-test.md` (determinism infrastructure)

## Production files (in-scope)

| File | Banned-API finding |
|---|---|
| `TaskVisualization/EditFilterController.cs` | NONE |
| `TaskVisualization/EditFilterViewer.cs` | NONE |
| `TaskVisualization/ManageFilters.cs` | NONE |
| `TaskVisualization/ManageFiltersController.cs` | NONE |
| `TaskVisualization/FlagCalculations.cs` (to be created) | must remain NONE |
| `TaskVisualization/FlagTasks.cs` | NONE |
| `TaskVisualization/AutoCreateProject.cs` | NONE (uses `Task.Run`, allowed) |
| `TaskVisualization/AutoAssignContext.cs` | NONE (uses `Task.Run`, allowed) |
| `TaskVisualization/AutoAssignPeople.cs` | NONE (uses `Task.Run`, allowed) |
| `TaskVisualization/FlagChangeGroup.cs` | NONE |
| `TaskVisualization/FlagChangeItem.cs` | NONE |
| `TaskVisualization/FlagChangeTrainingQueue.cs` | NONE for banned APIs. Uses `TimedAsyncTask(500ms, ...)` (a `DispatcherTimer`-style one-shot); tests MUST NOT await the timed task and MUST NOT block on wall-clock (P6-T4 drives the `Immediate` path and asserts queue state without waiting). |

Aggregate grep over all in-scope production files returned no matches (exit code 1 = clean).

## Test files (to be created)

All new test files (Phase 6–8) MUST contain no `Thread.Sleep`, `Task.Delay`, real timer waits, or `DateTime.Now`/`UtcNow`. Enforced per-file at authoring time:
- `FlagCalculationsTests.cs`, `FlagChangeItemTests.cs`, `FlagChangeGroupTests.cs`, `FlagChangeTrainingQueueTests.cs`, `AutoCreateProjectTests.cs`, `AutoAssignContextTests.cs`, `AutoAssignPeopleTests.cs`, `EditFilterControllerTests.cs`, `ManageFiltersControllerTests.cs`.

## Result

NONE across all in-scope production files. No in-plan remediation required. New test files carry a determinism obligation folded into their respective phase tasks.
