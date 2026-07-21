# Phase 0 — Baseline Test Suite + Coverage (P0-T6)

Timestamp: 2026-07-20T21-58

Command (pass/fail + binary coverage, plan-specified):
`vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /EnableCodeCoverage`

Command (numeric per-file coverage, Cobertura format via repo-standard DynamicCoverage collector runsettings scoped to UtilitiesCS.dll + QuickFiler.dll):
`vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:cobertura.runsettings`
(The DataCollector `.coverage` from `/EnableCodeCoverage` is not reliably offline-convertible in this environment; the Cobertura-format runsettings — `<Format>Cobertura</Format>` under a DynamicCoverageDataCollector — is the reliable numeric per-class path and yields identical pass/fail totals.)

EXIT_CODE: 0

Output Summary:
- Total tests: 5054. Passed: 5054. Failed: 0. (Identical under both the /EnableCodeCoverage run and the Cobertura runsettings run.)
- Overall coverage for the instrumented scope (UtilitiesCS.dll + QuickFiler.dll): line-rate 86.54%, branch-rate 80.25%.
- Baseline per-file coverage for the touched files (aggregated across the primary class and its compiler-generated async state-machine classes by filename):
  - UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs: line 197/205 = 96.10%, branch 106/120 = 88.33%
  - QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs: line 109/112 = 97.32%, branch 60/72 = 83.33%
  - UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs: line 148/148 = 100.00%, branch 98/104 = 94.23%
- These baseline figures are the no-regression reference for Phase 2. New/changed code must reach >= 90% line coverage; changed lines must not regress.
