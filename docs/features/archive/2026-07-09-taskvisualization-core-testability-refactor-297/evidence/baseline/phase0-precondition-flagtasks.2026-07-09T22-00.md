# Phase 0 — Precondition: FlagTasks.cs constructor call (P0-T4)

Timestamp: 2026-07-09T22-00

## FlagTasks.cs
- `[assembly: InternalsVisibleTo("TaskVisualization.Test")]` at source line 16.
- Constructs `TaskController` (source lines 62-74) via named parameters, calling the
  11-parameter constructor with these named arguments:
  `formInstance`, `olCategories`, `toDoSelection`, `defaults`, `autoAssign`,
  `projectAssign`, `contextAssign`, `projectsToPrograms`, `flagOptions`,
  `userEmailAddress`, `globals`.
  (All arguments are named, so declaration order is irrelevant to the call.)

## TaskController.cs — two public constructors
- 7-parameter ctor at source line 35:
  `(TaskViewer formInstance, Categories olCategories, List<ToDoItem> toDoSelection,
   ToDoDefaults defaults, IAutoAssign autoAssign, string userEmailAddress,
   Enums.FlagsToSet flagOptions = Enums.FlagsToSet.All)`
  - Takes `TaskViewer formInstance`; calls `formInstance.SetController(this)` (line 55);
    accept/cancel wiring at lines 56-57
    (`formInstance.AcceptButton = formInstance.OKButton;` /
     `formInstance.CancelButton = formInstance.Cancel_Button;`).
- 11-parameter ctor at source line 83:
  `(TaskViewer formInstance, Categories olCategories, List<ToDoItem> toDoSelection,
   ToDoDefaults defaults, IAutoAssign autoAssign, IAutoAssign projectAssign,
   IAutoAssign contextAssign, Func<string,string> projectsToPrograms,
   string userEmailAddress, IApplicationGlobals globals,
   Enums.FlagsToSet flagOptions = Enums.FlagsToSet.All)`
  - Takes `TaskViewer formInstance`; calls `formInstance.SetController(this)` (line 106);
    accept/cancel wiring at lines 107-108 (identical to 7-param ctor).

- `FlagTasks.cs` calls ONLY the 11-param ctor. The 7-param ctor is unused but public and
  is retained (removing it would be an out-of-scope public-API change).

AC: 11 named arguments, InternalsVisibleTo line, and both constructor signatures with
their shared accept/cancel wiring lines CONFIRMED.
