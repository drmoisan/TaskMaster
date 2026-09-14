# Phase 2 — Production format (P2-T4)

Task: [P2-T4]
Every command below was run from the item worktree root via Set-Location inside one pwsh invocation; the two csharpier commands were each run while holding the shared machine build lock for item 743 (acquired immediately before and released immediately after each command). Inner quoting of the plan spans was inverted to single quotes where wrapped; semantics identical.

## Command 1 — csharpier format (write-mode)

Timestamp: 2026-09-13T02-57
Command: `pwsh -Command 'dotnet tool run csharpier format QuickFiler\Viewers\IItemViewer.cs QuickFiler\Viewers\ItemViewer.cs QuickFiler\Controllers\QfcItemController.ViewerSetup.cs'`
EXIT_CODE: 0
Output Summary:
- `Formatted 3 files in 2299ms.` (a processed count, not a changed count)
- Observation beyond the exit code: `git diff --stat -- QuickFiler` afterwards reported `3 files changed, 21 insertions(+), 3 deletions(-)` (ViewerSetup.cs 3 insertions / 3 deletions, IItemViewer.cs 12 insertions, ItemViewer.cs 6 insertions), i.e. the formatter reflowed nothing outside the P2-T1/P2-T2/P2-T3 edits.

## Command 2 — csharpier check (read-only)

Timestamp: 2026-09-13T02-57
Command: `pwsh -Command 'dotnet tool run csharpier check QuickFiler\Viewers\IItemViewer.cs QuickFiler\Viewers\ItemViewer.cs QuickFiler\Controllers\QfcItemController.ViewerSetup.cs'`
EXIT_CODE: 0
Output Summary:
- `Checked 3 files in 662ms.`; no file reported as unformatted.

## Post-format line counts

- `QuickFiler\Viewers\IItemViewer.cs` = 212 (was 200; at most 215 required)
- `QuickFiler\Viewers\ItemViewer.cs` = 406 (was 400; at most 415 required)
- `QuickFiler\Controllers\QfcItemController.ViewerSetup.cs` = 467 (was 467; at most 480 required)

## Observation beyond the exit code — `git status --porcelain -- QuickFiler` (verbatim)

```
 M QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
 M QuickFiler/Viewers/IItemViewer.cs
 M QuickFiler/Viewers/ItemViewer.cs
```

Exactly the three edited files are listed.
