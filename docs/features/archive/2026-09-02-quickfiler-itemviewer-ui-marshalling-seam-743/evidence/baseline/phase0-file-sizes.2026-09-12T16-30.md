# Phase 0 — File-size and headroom baseline (P0-T8)

Task: [P0-T8]
Timestamp: 2026-09-13T02-25
Command: `pwsh -Command '@("QuickFiler\Viewers\IItemViewer.cs","QuickFiler\Viewers\ItemViewer.cs","QuickFiler\Controllers\QfcItemController.ViewerSetup.cs","QuickFiler\Controllers\QfcItemController.Initialization.cs","QuickFiler.Test\Controllers\QfcItemController.ViewerSetupTests.cs","QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs","QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs") | ForEach-Object { "$_ = " + (Get-Content $_).Count }'` Run from the item worktree root via Set-Location inside one pwsh invocation (inner quoting inverted to single quotes; semantics identical).
EXIT_CODE: 0
Output Summary:
```
QuickFiler\Viewers\IItemViewer.cs = 200
QuickFiler\Viewers\ItemViewer.cs = 400
QuickFiler\Controllers\QfcItemController.ViewerSetup.cs = 467
QuickFiler\Controllers\QfcItemController.Initialization.cs = 497
QuickFiler.Test\Controllers\QfcItemController.ViewerSetupTests.cs = 498
QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs = 278
QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs = 353
```
- The seven values are exactly the expected 200, 400, 467, 497, 498, 278, 353. The origin/main merge (`c358b2d809ca58db0197eb10229f872f2e9a924e`) changed none of these files, so every line citation in the plan remains valid.
- Headroom against the 500-line limit: 300, 100, 33, 3, 2, 222, 147 respectively.
