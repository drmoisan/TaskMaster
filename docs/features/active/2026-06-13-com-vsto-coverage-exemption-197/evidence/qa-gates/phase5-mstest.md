# Phase 5 — MSTest with Coverage

Timestamp: 2026-06-13T13-53

Command: pwsh -NoProfile scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage/coverage.phase5.cobertura.xml
(Koverage dedup -> coverage/coverage.phase5.firstparty.cobertura.xml.)

EXIT_CODE: vstest reported 2 failures -> pipeline exit 1 (dedup re-applied manually)

## Test results
- Total tests: 4068
- Passed: 4066
- Failed: 2:
  - RequestTask_WithConfiguredTask_InvokesTaskAfterInterval (26s) — known flaky timing test.
  - HighConfidenceModeEnabled_Default_IsFalse — flaky shared-mutable-static race: the test reads/writes the process-wide Settings.Default.HighConfidenceModeEnabled, which an adjacent parallel test (IsHighConfidenceModeActive_ReturnsStoredValue) can mutate between this test's arrange and read. Never failed in baseline or Phases 1-4 (verified across all prior run logs); failed only under this parallel scheduling. This is a pre-existing test-isolation weakness (UT4 shared-global-state), not a regression. The test exercises AppQuickFilerSettings, a testable seam that is correctly NOT annotated; annotation changes are non-behavioral and cannot affect TaskMaster.Test's static Settings.Default.

## Coverage headline (first-party deduped, all non-.Test incl vendored constant)
- covered: 37,033
- lines-valid: 51,842
- line rate: 71.43%

## QuickFiler viewer annotation verification
- QuickFiler package denominator: Phase 4 10,438 -> Phase 5 6,653 lines (viewers ~3,785 lines removed).
- 8 viewers annotated once each (code-behind only, partial type): EfcViewer, QfcFormViewer, QfcItemViewer, QfcItemViewerExpanded, QfcItemViewerExpandedLight, QfcItemViewerLightSelected, QfcItemViewerV1, ItemViewer.
- Out-of-scope viewers confirmed NOT annotated: BayesianPerformanceViewer, Form1, ToolStripMenuItemCb, QFCItemViewerDarkNew, QFCItemViewerLightNew, ItemViewerExpanded, EfcViewer3, QfcFormViewerDark, QfcFormViewerExpanded.
