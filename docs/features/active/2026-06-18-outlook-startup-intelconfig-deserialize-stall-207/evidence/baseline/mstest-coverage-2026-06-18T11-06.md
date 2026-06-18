# Phase 0 — MSTest + Coverage Baseline (Issue #207)

Timestamp: 2026-06-18T11-06

Command:
- `vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /InIsolation /EnableCodeCoverage /ResultsDirectory:<evidence/baseline/trx> /Logger:"trx;LogFileName=baseline.trx"`
- Coverage converted to Cobertura: `dotnet-coverage merge <run>.coverage -f cobertura -o baseline.cobertura.xml`

EXIT_CODE: 0

Output Summary:
- Test result: Test Run Successful. Total tests: 3912, Passed: 3912, Failed: 0. Total time ~43.6 s.
- `/InIsolation` required for this Moq-bearing assembly (per prior STTE FileNotFound finding).
- Repository-wide raw Cobertura line-rate (whole-solution denominator including COM/VSTO/WinForms-exempt and vendored Swordfish/SVGControl packages): 0.5932 (59.32%); lines-covered 91516 / lines-valid 154264. This raw figure is the whole-tree denominator, NOT the policy "testable denominator" used for the 80% floor; it is recorded only as the comparison baseline for no-regression on changed lines.
- Targeted module `UtilitiesCS\EmailIntelligence\IntelligenceConfig.cs`:
  - class `UtilitiesCS.EmailIntelligence.IntelligenceConfig` line-rate = 0.8947 (89.47%)
  - state machine `IntelligenceConfig.<ReadConfigurationAsync>d__11` line-rate = 1.0 (100%)
  - sibling class `IntelligenceConfigResourceWriter` line-rate = 0 (resx-writer wrapper; not instrumentation target).
- Baseline Cobertura artifact: evidence/baseline/trx/baseline.cobertura.xml.
