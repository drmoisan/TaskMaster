# Phase 4 — Branch COST format and build (P4-T2)

Task: [P4-T2]
Branch COST was selected in P4-T1, so neither fixture file was modified in Phase 4; this task ran unconditionally to confirm the retained P1-T6/P1-T7 instrumentation is clean. Every command below was run from the item worktree root via Set-Location inside one pwsh invocation, each while holding the shared machine build lock for item 743 (acquired immediately before and released immediately after each command). Inner quoting of the plan spans was inverted to single quotes where wrapped; semantics identical. Outlook was closed.

## Command 1 — csharpier format (write-mode)

Timestamp: 2026-09-13T03-30
Command: `pwsh -Command 'dotnet tool run csharpier format QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs'`
EXIT_CODE: 0
Output Summary:
- `Formatted 2 files in 1457ms.` (a processed count, not a changed count)
- Observation beyond the exit code: `git status --porcelain -- QuickFiler.Test` immediately afterwards printed nothing, so the formatter rewrote neither file (both were already committed in their P1-T8-formatted state).

## Command 2 — csharpier check (read-only)

Timestamp: 2026-09-13T03-31
Command: `pwsh -Command 'dotnet tool run csharpier check QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs'`
EXIT_CODE: 0
Output Summary:
- `Checked 2 files in 808ms.`; neither file reported as unformatted.

## Command 3 — assembly build (not a gate)

Timestamp: 2026-09-13T03-31
Command: `pwsh -Command '& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"'` (Command Reference tool resolution prepended; console output redirected to the ignored path `coverage\p4-t2-build.log`)
EXIT_CODE: 0
Output Summary:
- `Build succeeded.` / `0 Warning(s)` / `0 Error(s)` / `Time Elapsed 00:00:01.07`
- Incremental `/t:Build`; no source changed since the P3-T6 build, so every project was up to date. The assembly under test for P4-T3 is the one produced by the P3-T5 Rebuild plus the P3-T6 incremental build.

## Post-format line counts

- `QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs` = 304 (at most 480 required)
- `QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs` = 396 (at most 480 required)

## Observation beyond the exit code — `git status --porcelain -- QuickFiler.Test` (verbatim)

```
(empty)
```
