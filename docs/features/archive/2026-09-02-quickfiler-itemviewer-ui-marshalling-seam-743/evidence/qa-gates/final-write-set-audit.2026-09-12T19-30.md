# Phase 6 — Write Set boundary audit (P6-T8)

Task: [P6-T8]
Anchor: `refs/plan/issue-743-base` = c358b2d809ca58db0197eb10229f872f2e9a924e (the re-anchored value recorded in the P0-T3 artifact addendum). Both commands were run as `git -C <worktree> <subcommand>` invocations of the same subcommands and arguments the plan states.

## Command 1 — anchored name-only diff

Timestamp: 2026-09-13T03-51
Command: `pwsh -Command 'git diff --name-only refs/plan/issue-743-base -- QuickFiler QuickFiler.Test UtilitiesCS TaskMaster ToDoModel Tags TaskVisualization scripts .github'`
EXIT_CODE: 0
Output Summary: seven names printed, verbatim:
```
QuickFiler.Test/Controllers/QfcItemController.SeamMarshallingTests.cs
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs
QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
QuickFiler/Viewers/IItemViewer.cs
QuickFiler/Viewers/ItemViewer.cs
```
These are exactly the seven Write Set paths and nothing else. No path under UtilitiesCS, TaskMaster, ToDoModel, Tags, TaskVisualization, scripts or .github appears; neither the Initialization controller partial nor the ViewerSetup test file appears.

## Command 2 — porcelain companion (untracked-file blindness of the diff)

Timestamp: 2026-09-13T03-51
Command: `pwsh -Command 'git status --porcelain --untracked-files=all -- QuickFiler QuickFiler.Test UtilitiesCS scripts .github'`
EXIT_CODE: 0
Output Summary: the command printed nothing. Verbatim porcelain output:
```
```
(empty)

No untracked or modified file exists under the five audited directories. The two commands together are complete: the anchored diff enumerates committed changes since the base and the porcelain status enumerates uncommitted and untracked paths; both agree that the item touched only the seven Write Set paths.
