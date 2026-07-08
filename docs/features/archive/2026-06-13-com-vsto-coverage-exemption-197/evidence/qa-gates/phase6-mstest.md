# Phase 6 — MSTest with Coverage

Timestamp: 2026-06-13T14-08

Command: pwsh -NoProfile scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage/coverage.phase6.cobertura.xml
(Koverage dedup -> coverage/coverage.phase6.firstparty.cobertura.xml.)

EXIT_CODE: vstest reported 2 failures -> pipeline exit 1 (dedup re-applied manually)

## Test results
- Total tests: 4068
- Passed: 4066
- Failed: 2:
  - RequestTask_WithConfiguredTask_InvokesTaskAfterInterval (20s) — known flaky timing test.
  - ToggleHighConfidenceMode_FlipsStoredValue (125ms) — same AppQuickFilerSettings shared-mutable-static (Settings.Default.HighConfidenceMode) parallel race as the Phase 5 flake; varying failure set across runs confirms non-determinism. Exercises AppQuickFilerSettings (correctly NOT annotated); not a regression.

## Coverage headline (first-party deduped, all non-.Test incl vendored constant)
- covered: 36,997
- lines-valid: 51,594
- line rate: 71.71%

## Tags annotation verification
- Tags package denominator: baseline 1,008 -> 760 lines (TagLauncher + CheckBoxController removed; ~248).
- Tags package rate: 31.15% -> 38.16%.
- Annotated once each: TagLauncher, Tags/Helper Classes/CheckBoxController.cs (the compiled WinForms CheckBox event-handler in Tags.csproj; the non-compiled root Tags/CheckBoxController.cs left untouched).
- Testable seams confirmed present in Tags package: TagController (pure-logic methods), PrefixItem.
