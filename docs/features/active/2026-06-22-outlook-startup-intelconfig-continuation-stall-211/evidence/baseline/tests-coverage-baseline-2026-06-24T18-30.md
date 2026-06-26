# Test + Coverage Baseline (Issue #211 PostLoad/LoadInboxes attribution probe)

Timestamp: 2026-06-24T18-30

Command:
`vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" /ResultsDirectory:TestResults\baseline211_1830`
then: `Microsoft.CodeCoverage.Console.exe merge <.coverage> -f cobertura -o baseline-coverage-2026-06-24T18-30.cobertura.xml`

EXIT_CODE: 0

Output Summary:
- Test result: Test Run Successful. Total tests: 4109; Passed: 4109; Failed: 0.
- Coverage (Cobertura merged XML; the tool instruments ALL loaded modules incl. vendored/third-party, so the whole-process rate is low and is NOT the policy gate — the policy gate applies to the first-party testable denominator):
  - Whole-process line-rate: 0.618940 = 61.89% (includes vendored Swordfish/SVGControl and third-party modules).
  - First-party `TaskMaster` package line-rate: 0.530949 = 53.09% (includes the [ExcludeFromCodeCoverage]-exempt VSTO/WinForms/Interop classes per CLAUDE.md; the new StartupInboxAttributionProbe does not exist at baseline).
  - First-party `UtilitiesCS` package line-rate: 0.874568 = 87.46% (not touched by this plan).
- Authoritative baseline references for the post-change delta (computed identically post-change): whole-process = 61.89%; TaskMaster = 53.09%; UtilitiesCS = 87.46%.
- New code added by this plan (`StartupInboxAttributionProbe` in TaskMaster.dll; the extracted per-store attribution method in AppOlObjects) does not exist at baseline (new-code coverage = N/A pre-change).
- Raw merged XML preserved at `evidence/baseline/baseline-coverage-2026-06-24T18-30.cobertura.xml`.
