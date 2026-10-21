# Phase 6 — Post-format file-size audit (P6-T7)

Task: [P6-T7]
Timestamp: 2026-09-13T03-51
Command: `pwsh -Command '@("QuickFiler\Viewers\IItemViewer.cs","QuickFiler\Viewers\ItemViewer.cs","QuickFiler\Controllers\QfcItemController.ViewerSetup.cs","QuickFiler\Controllers\QfcItemController.Initialization.cs","QuickFiler.Test\Controllers\QfcItemController.ViewerSetupTests.cs","QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs","QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs","QuickFiler.Test\Controllers\QfcItemController.SeamMarshallingTests.cs") | ForEach-Object { "$_ = " + (Get-Content $_).Count }'` (the P0-T8 command extended with the new test file as the eighth entry). Run from the item worktree root via Set-Location inside one pwsh invocation (inner quoting inverted to single quotes; semantics identical). Run after the P6-T1 formatter pass of toolchain pass 1, which rewrote nothing.
EXIT_CODE: 0
Output Summary:
```
QuickFiler\Viewers\IItemViewer.cs = 212
QuickFiler\Viewers\ItemViewer.cs = 406
QuickFiler\Controllers\QfcItemController.ViewerSetup.cs = 478
QuickFiler\Controllers\QfcItemController.Initialization.cs = 497
QuickFiler.Test\Controllers\QfcItemController.ViewerSetupTests.cs = 498
QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs = 304
QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs = 396
QuickFiler.Test\Controllers\QfcItemController.SeamMarshallingTests.cs = 312
```
- All eight counts are at most 500 (largest: 498).
- The two files this item does not edit are recorded at exactly 497 (the Initialization controller partial) and 498 (the ViewerSetup test file), identical to the P0-T8 baseline.
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is 478, at most the 480 ceiling (P0-T8 baseline 467; +11 from the P3-T2 null-tolerant conversion and the P3-T3 comment, as recorded in P3-T4).
- Deltas against P0-T8 for the edited files: IItemViewer.cs 200 to 212; ItemViewer.cs 400 to 406; UiThreadDispatcherFixture.cs 278 to 304; UiThreadDispatcherFixtureTests.cs 353 to 396; the new SeamMarshallingTests.cs is 312.
