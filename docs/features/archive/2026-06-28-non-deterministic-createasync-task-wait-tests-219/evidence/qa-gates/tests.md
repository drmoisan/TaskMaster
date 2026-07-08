# Phase 2 — QA Gate: Tests + Coverage (Issue #219)

Timestamp: 2026-06-28T20-10

Command:
vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /EnableCodeCoverage

(Full UtilitiesCS.Test assembly. `/InIsolation` is required for this Moq-bearing assembly to
avoid the documented STTE Setup FileNotFound failure; it does not change outcomes. A second
targeted run was executed to confirm the two named tests pass in isolation.)

EXIT_CODE: 0

Output Summary:
- Full assembly: Total tests: 4089; Passed: 4089; Failed: 0; Total time 23.07 s.
- Targeted confirmation run (both in-scope methods):
  - Passed CreateAsync_HiddenLabel_WithMatchingSyncContext_ReturnsInitializedDetails [95 ms]
  - Passed CreateAsync_VisibleLabel_WithMatchingSyncContext_ReturnsOnState [1 ms]
  Both named tests pass.
- Post-change coverage (Cobertura, merged from the run's .coverage attachment):
  - Production class `UtilitiesCS.QfcTipsDetails` line-rate = 0.91045 (91.05%) in the full-assembly
    run (higher than the P0-T3 targeted-run figure of 22.39% because the full assembly exercises
    many additional QfcTipsDetails methods; the two figures are not directly comparable).
  - Changed-line / in-scope state machines exercised by the two target tests:
    - `UtilitiesCS.QfcTipsDetails.<CreateAsync>d__3` line-rate = 1.0 (100%).
    - `UtilitiesCS.QfcTipsDetails.<InitializeAsync>d__5` line-rate = 1.0 (100%).
  - These match the P0-T3 baseline (both 1.0). No regression on the lines covered by the
    changed tests.
