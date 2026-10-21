# P0-T12 — Pre-Edit Literal-Presence Baseline

Timestamp: 2026-09-03T11-25
Command:
Select-String -Path 'QuickFiler/Controllers/QfcHomeController.Metrics.cs' -SimpleMatch -CaseSensitive 'hh:mm'
Select-String -Path 'QuickFiler/Controllers/EfcHomeController.Metrics.cs' -SimpleMatch -CaseSensitive 'hh:mm'
Select-String -Path 'QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs' -SimpleMatch -CaseSensitive 'hh:mm'
Select-String -Path 'QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs' -SimpleMatch -CaseSensitive 'hh:mm'
(all four paths passed as absolute paths into the item worktree)
EXIT_CODE: 0

Output Summary:
- QuickFiler/Controllers/QfcHomeController.Metrics.cs: 3 matches — lines 46, 48, 127
- QuickFiler/Controllers/EfcHomeController.Metrics.cs: 1 match — line 96
- QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs: 4 matches — lines 227, 243, 265, 278
- QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs: 0 matches

Total baseline occurrences across all four files: 8. All counts match the plan's expected
baseline (P0-T12 acceptance text) exactly, confirming no drift from the plan's cited tree state.
