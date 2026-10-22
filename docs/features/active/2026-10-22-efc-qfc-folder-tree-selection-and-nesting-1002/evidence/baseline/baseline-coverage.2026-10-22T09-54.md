# Baseline: TEST-COVERAGE (#1002)

Timestamp: 2026-10-22T09-54
Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .
EXIT_CODE: 0
Output Summary:
- Test run outcome Completed: total 7504, executed 7504, passed 7504, failed 0 (summary derived from the trx by the coverage script; skipped 0 derived as total minus executed).
- Shell-icon carve-out: not needed; the run did not stall.
- First-party coverage summary line: `First-party coverage: lines 57272/66515 (86.10%), branches 13923/17337 (80.31%)`.
- Repository line coverage 86.10 percent (floor 80); branch coverage 80.31 percent (floor 75). Post-processed Cobertura root: line-rate 0.861039, branch-rate 0.80308.
- Post-processed Cobertura copied to SCRATCH as `issue-1002-baseline-cobertura.xml` (not committed); trx copied to SCRATCH as `issue-1002-baseline.trx`.

## Per-file line coverage (existing Write Set production files)

Computed from the post-processed Cobertura by unioning `<line>` elements of every `<class>` whose filename is the file (a line counts as covered when any class entry reports hits above zero). Helper: SCRATCH/issue-1002-filecov.ps1.

| File | Covered/Valid | Line coverage |
|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | 153/155 | 98.71% |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs` | 30/30 | 100.00% |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs` | 107/107 | 100.00% |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` | 129/133 | 96.99% |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` | 0/0 | not instrumented (string constants only; no executable lines) |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 239/241 | 99.17% |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` | 128/133 | 96.24% |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | 150/153 | 98.04% |
| `QuickFiler/Controllers/EfcFormController.Actions.cs` | 23/108 | 21.30% |
| `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` | 78/115 | 67.83% |
