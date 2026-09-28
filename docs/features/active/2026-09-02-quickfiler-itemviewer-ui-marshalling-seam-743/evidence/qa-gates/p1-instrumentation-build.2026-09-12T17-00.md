# Phase 1 — Instrumentation format and build (P1-T8)

Task: [P1-T8]
Every command below was run from the item worktree root via Set-Location inside one pwsh invocation, each while holding the shared machine build lock for item 743 (acquired immediately before and released immediately after each command). Inner quoting of the plan spans was inverted to single quotes where wrapped; semantics identical.

## Command 1 — csharpier format (write-mode)

Timestamp: 2026-09-13T02-46
Command: `pwsh -Command 'dotnet tool run csharpier format QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs'`
EXIT_CODE: 0
Output Summary:
- `Formatted 2 files in 1644ms.` (a processed count, not a changed count)
- Observation beyond the exit code: `git diff --stat -- QuickFiler.Test` afterwards reported `2 files changed, 69 insertions(+)` and zero deletions (26 insertions in the fixture file, 43 in the fixture tests file), i.e. the formatter left every pre-existing line untouched and reflowed nothing outside the P1-T6/P1-T7 insertions.

## Command 2 — csharpier check (read-only)

Timestamp: 2026-09-13T02-46
Command: `pwsh -Command 'dotnet tool run csharpier check QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs'`
EXIT_CODE: 0
Output Summary:
- `Checked 2 files in 509ms.`; no file reported as unformatted.

## Command 3 — assembly build (not a gate)

Timestamp: 2026-09-13T02-46
Command: `pwsh -Command '& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"'` (Command Reference tool resolution prepended; console output redirected to the ignored path `coverage\p1-t8-build.log`)
EXIT_CODE: 0
Output Summary:
- `Build succeeded.` / `0 Warning(s)` / `0 Error(s)` / `Time Elapsed 00:00:04.70`
- This is an incremental `/t:Build` that produces the instrumented `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`; no analyzer or nullable claim is made from it.

## Post-format line counts

- `QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs` = 304 (was 278; at most 480 required)
- `QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs` = 396 (was 353; at most 480 required)
