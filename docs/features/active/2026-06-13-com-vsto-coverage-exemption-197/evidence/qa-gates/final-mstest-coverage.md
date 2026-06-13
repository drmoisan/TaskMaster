# Final QC — MSTest with Coverage

Timestamp: 2026-06-13T14-25

Command: pwsh -NoProfile scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage/coverage.final.cobertura.xml
(Koverage dedup -> coverage/coverage.final.firstparty.cobertura.xml.)

EXIT_CODE: vstest reported 2 failures -> pipeline exit 1 (dedup re-applied manually)

## Test results
- Total tests: 4068
- Passed: 4066
- Failed: 2:
  - AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException
  - RequestTask_WithProvidedTask_InvokesTaskAfterInterval
- These are exactly the same 2 pre-existing flaky timing/threading tests recorded in the Phase 0 baseline (roadmap §0.1). Identical to baseline set -> behavior parity confirmed.

## Coverage headline (production-only first-party deduped, all non-.Test incl vendored constant)
- covered: 37,010
- lines-valid: 51,594
- line rate: 71.73%

## Verification
- TaskVisualization package absent from first-party denominator: confirmed.
- Remaining first-party packages: QuickFiler, Tags, TaskMaster, ToDoModel, UtilitiesCS, VBFunctions (+ vendored SVGControl, Swordfish.NET.General held constant).
