# Final QA — Tests + Coverage (Issue #211 PostLoad/LoadInboxes attribution probe)

Timestamp: 2026-06-24T18-30

Command:
`vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" /ResultsDirectory:TestResults\final211_1830`
then: `Microsoft.CodeCoverage.Console.exe merge <.coverage> -f cobertura -o final-coverage-2026-06-24T18-30.cobertura.xml`

EXIT_CODE: 0

Output Summary:
- Test result: Test Run Successful. Total tests: 4123; Passed: 4123; Failed: 0. (Baseline 4109 + 14 new StartupInboxAttributionProbeTests.)
- Coverage (Cobertura merged XML; instruments ALL loaded modules incl. vendored/third-party, so whole-process is low and is NOT the policy gate):
  - Whole-process line-rate: 0.619367 = 61.94% (baseline 61.89%; no regression, +0.05 pp).
  - First-party `TaskMaster` package line-rate: 0.535478 = 53.55% (baseline 53.09%; +0.46 pp, the new coverable probe lifts the package).
  - First-party `UtilitiesCS` package line-rate: 0.874568 = 87.46% (baseline 87.46%; unchanged, not touched).
- New-code coverage:
  - `TaskMaster.StartupInboxAttributionProbe` class line-rate = 1.0 = 100% (>= 90% target met).
  - `AppOlObjects.EmitPerStoreInboxAttribution` method line-rate = 1.0 = 100% (>= 90% target met).
- Raw merged XML preserved at `evidence/qa-gates/final-coverage-2026-06-24T18-30.cobertura.xml`.
