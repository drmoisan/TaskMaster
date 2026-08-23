# P5-T46 focused coverage green result

Timestamp: 2026-08-06T18-20

Command:

`pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-04-folder-tree-dispatcher-thread-affinity-420/evidence/regression-testing/remediation-cycle4-focused-coverage.cobertura.xml`

Result: PASS. The wrapper discovered eight test assemblies, used `/InIsolation` and `TestCategory!=LiveOutlook`, completed in 45.3110 seconds, and passed 6,166/6,166 tests.

Cobertura inventory: 93,666/110,478 lines (84.7825%) and 21,453/27,698 branches. Changed production, reconciled against `git diff origin/main -- '*.cs'` including authorized untracked production, is 890/892 lines (99.7758%).

Target coverage: `FilterOlFoldersController.cs` 101/102; `FilterOlFoldersController.Lifecycle.cs` 334/335; `WpfUiDispatcher.cs` 12/12; `OutlookFolderTreeService.cs` 359/359; `AppOlObjects.FolderTreeService.cs` 291/292. Every emitted changed target method is at least 95%. The remaining uncovered target lines are unchanged controller line 81, unchanged lifecycle line 81, and unchanged AppOl line 289 (`Task.Status` snapshot); they are not changed-production denominator misses.

The P5 capacity checks remain satisfied: controller coverage partial 499 lines with one adjacent `Compile` entry, lifecycle-races partial 296 lines, and lifecycle production file 498 lines. No extra partial or compile entry exists.
