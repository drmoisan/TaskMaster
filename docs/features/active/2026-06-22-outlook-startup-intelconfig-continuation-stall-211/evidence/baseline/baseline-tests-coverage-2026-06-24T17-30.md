# Baseline — MSTest with Coverage (AC10, issue #211)

Timestamp: 2026-06-24T19-12
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" /ResultsDirectory:TestResults/baseline211
  then: Microsoft.CodeCoverage.Console.exe merge <.coverage> -f cobertura -o baseline-coverage-2026-06-24T17-30.cobertura.xml
EXIT_CODE: 0

Output Summary:
- Test result: Test Run Successful. Total tests: 4099; Passed: 4099; Failed: 0.
- Coverage (Cobertura merged XML; tool groups by ROOT NAMESPACE, instruments ALL loaded modules incl. vendored, so whole-process is low and is NOT the policy gate):
  - Whole-process line-rate: 0.61840 = 61.84% (lines-covered 98261 / lines-valid 158895). Includes vendored and third-party modules.
  - First-party `TaskMaster` package line-rate: 0.51904 = 51.90% (receives the new JunkFolderPathNavigator helper; includes [ExcludeFromCodeCoverage]-exempt VSTO/WinForms/Interop classes per CLAUDE.md).
  - First-party `UtilitiesCS` package line-rate: 0.87446 = 87.45% (not touched by this plan).
- Authoritative baseline references for the post-change delta (computed identically post-change): whole-process = 61.84%; TaskMaster = 51.90%; UtilitiesCS = 87.45%.
- New code added by this plan (`JunkFolderPathNavigator` in TaskMaster.dll; the COM adapter is [ExcludeFromCodeCoverage]) does not exist at baseline (coverage = N/A pre-change).
- Raw merged XML preserved at evidence/baseline/baseline-coverage-2026-06-24T17-30.cobertura.xml.
