# QA Gate 04 — Test + Coverage (P8-T4)

Timestamp: 2026-07-07T23-35

Command: pwsh ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage\postchange.cobertura.xml
(Canonical coverage path: dotnet-coverage collect wrapping
`vstest.console.exe <all 7 *.Test.dll> /Settings:TaskMaster.cli.runsettings /InIsolation`, output
Cobertura. Run over all test assemblies as CI does. Confirmed reproducible by a second run
`coverage\verify.cobertura.xml` yielding 81.07%.)

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 5032, Passed: 5032, Failed: 0 (baseline was 4995; +37 new tests).
- Repository line coverage (post-change, de-duplicated denominator): 81.08% (79667 / 98254);
  reproduced at 81.07% (79656 / 98254) on a second run.
- New-code coverage (per-class, from Cobertura):
  - StoreIdentity.cs: 100.00% (50/50 lines)
  - StoreDisableService.cs: 97.92% (188/192 lines)
  - DisabledStoreEntry (IStoreDisableService.cs): 100.00% (8/8 lines)
- Touched-code coverage (filter/attribution deltas):
  - StoreFilterAttribution.cs: 100.00% (96/96 lines)
  - StoresWrapper.cs: 98.60% (424/430 lines)
- New-code coverage >= 90%: PASS. Repository line coverage >= 80% (testable denominator): PASS (81.08%).
