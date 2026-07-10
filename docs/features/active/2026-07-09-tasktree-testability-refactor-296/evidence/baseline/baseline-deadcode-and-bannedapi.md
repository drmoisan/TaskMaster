# Baseline — Dead-Code Disposition and Banned-API Scan (P0-T8)

Timestamp: 2026-07-09T16-42

## Debug-helper external-caller scan (instance-scoped)

Searched the repo (excluding `packages/`) for invocations that resolve to a `TaskTreeController` instance:
- `.WriteTreeToDisk(` : ZERO matches.
- `.LoopTreeToWrite(` : ZERO matches.
- `.AppendLineToCSV(` : ZERO matches.
- `.ToDoTree` member-access : ZERO matches.

Declaration/self-call sites (bare-name) reside only in:
- `TaskTree/TaskTreeController.cs` — the controller's own declarations + internal self-calls (the deletion targets).
- `ToDoModel/Data Model/Tree/TreeOfToDoItems.cs` — an INDEPENDENT, same-named set (`WriteTreeToCSVDebug` at line 442, `LoopTreeToWrite` at line 452, `AppendLineToCSV` at line 471), invoked only from within `TreeOfToDoItems` itself (line 449, recursion at line 466). These members belong to `TreeOfToDoItems`, NOT `TaskTreeController`. They are UNRELATED to the controller and are EXPECTED non-callers that MUST NOT trip the zero-callers gate.

## Banned-API scan on touched production files

Searched `TaskTree/TaskTreeController.cs` and `TaskTree/TaskTreeForm.cs` for
`DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`:
- ZERO matches. (Controller uses `Task.Run`, which is NOT banned.)

## Disposition

DELETE disposition CONFIRMED for `WriteTreeToDisk`, `LoopTreeToWrite`, `AppendLineToCSV`, and the
`ToDoTree` field (planner-selected preferred option). No unexpected external caller found; the
retention-with-line-sink fallback is NOT triggered.

Binary outcome: zero external TaskTreeController debug-helper callers AND zero banned symbols. PASS.
