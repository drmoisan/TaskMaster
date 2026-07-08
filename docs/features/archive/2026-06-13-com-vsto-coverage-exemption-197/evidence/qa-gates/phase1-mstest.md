# Phase 1 — MSTest with Coverage

Timestamp: 2026-06-13T12-23

Command: pwsh -NoProfile scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage/coverage.phase1.cobertura.xml

EXIT_CODE: 0 (PIPELINE_EXIT=0)

## Test results
- Total tests: 4068
- Passed: 4068
- Failed: 0
- Note: the 2 pre-existing flaky timing tests (AddEntry_UseUiThreadTrue..., RequestTask_WithProvidedTask...) passed on this run, confirming they are non-deterministic/flaky rather than a regression. Behavior parity is preserved (no new persistent failures).

## Coverage headline (first-party deduped, all non-.Test packages incl vendored held constant)
- covered: 38,807
- lines-valid: 62,267
- line rate: 62.32%

## TaskVisualization exclude verification
- TaskVisualization package present in deduped first-party Cobertura: FALSE (confirmed absent).
- Denominator dropped from baseline 65,768 to 62,267 = reduction of 3,501 lines, exactly matching design memo §2.1 (~3,501 lines for the TaskVisualization assembly exclude).
- Remaining first-party packages: QuickFiler, Tags, TaskMaster, ToDoModel, UtilitiesCS, VBFunctions, plus vendored SVGControl/Swordfish.NET.General.
