# Baseline MSTest + Coverage (issue #211, Phase 3.4)

Timestamp: 2026-06-24T14-30
Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`
(VS18 Community vstest.console.exe; /InIsolation required for Moq test assemblies per repo memory.)
EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 3916; Passed: 3916; Failed: 0. Total time ~22.5 s.
- Coverage file: TestResults/32ef23c0-b836-49b7-a86b-888ad27ef60a/...11_12_09.coverage
- Coverage merged to Cobertura via `dotnet-coverage merge -f cobertura`.
- Raw merged Cobertura overall line-rate (ALL assemblies incl. vendored/COM-bound/Designer/test): 0.59280 (59.28%).
  NOTE: This raw figure is NOT the policy "testable denominator." Per the CLAUDE.md COM/VSTO/WinForms exemption, the 80% floor applies to production-only first-party testable code after excluding VSTO lifecycle, WinForms/Designer, and Outlook-Interop event-handler classes. The raw cobertura percentage is dragged below 80% by those exempt assemblies and by test code; it is recorded here only as the comparison baseline for the no-regression check (post-change must not fall below this raw figure).
- Target module baseline: `UtilitiesCS.OutlookObjects.Store.StoresWrapper` class line-rate = 1.0 (100%), 221/221 lines covered (the new instrumented helper added in Phase 2 must not reduce this).

Acceptance: numeric baseline coverage values recorded (no UNVERIFIED placeholder). Baseline raw cobertura overall = 59.28%; StoresWrapper module = 100%.
