# Phase 0 — Baseline Test Result (Issue #219)

Timestamp: 2026-06-28T19-53

Command:
vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation
/Tests:CreateAsync_VisibleLabel_WithMatchingSyncContext_ReturnsOnState,CreateAsync_HiddenLabel_WithMatchingSyncContext_ReturnsInitializedDetails
/EnableCodeCoverage

(`/InIsolation` is required for this Moq-bearing assembly to avoid the documented
STTE Setup FileNotFound failure; it does not change test outcomes.)

EXIT_CODE: 0

Output Summary:
- Total tests: 2; Passed: 2; Failed: 0.
- Passed CreateAsync_HiddenLabel_WithMatchingSyncContext_ReturnsInitializedDetails [110 ms]
- Passed CreateAsync_VisibleLabel_WithMatchingSyncContext_ReturnsOnState [2 ms]
- Coverage headline (Cobertura, merged from the run's .coverage attachment):
  - Production class `UtilitiesCS.QfcTipsDetails` line-rate = 0.22388 (22.39%).
  - State machine `UtilitiesCS.QfcTipsDetails.<CreateAsync>d__3` line-rate = 1.0 (100%).
  - State machine `UtilitiesCS.QfcTipsDetails.<InitializeAsync>d__5` line-rate = 1.0 (100%).
  The two target tests exercise the CreateAsync and InitializeAsync paths to full line
  coverage at baseline; this is the no-regression reference for P2-T4.
