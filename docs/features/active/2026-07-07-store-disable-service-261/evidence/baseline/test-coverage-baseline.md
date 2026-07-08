# Phase 0 — Test + Coverage Baseline (P0-T11)

Timestamp: 2026-07-07T23-05

Command: pwsh ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage\baseline.cobertura.xml
This is the repository's canonical coverage path: it wraps
`vstest.console.exe <all *.Test.dll> /Settings:TaskMaster.cli.runsettings /InIsolation` inside
`dotnet-coverage collect --settings coverage.config --output-format cobertura`, producing a
numeric Cobertura report. It runs ALL seven test assemblies (as CI does), not just the two named
in the plan's literal command, because a repo-wide coverage percentage is only meaningful when
every test assembly contributes; a 2-assembly run would spuriously report un-exercised first-party
code as 0%. The orchestrator explicitly authorized running all *.Test.dll for the coverage figure.
A plain `vstest /EnableCodeCoverage` run emits a binary `.coverage` file that is not offline-
convertible to a numeric percentage in this environment, so the Cobertura path is used.

Test assemblies run: QuickFiler.Test, Tags.Test, TaskMaster.Test, TaskVisualization.Test,
ToDoModel.Test, UtilitiesCS.Test, VBFunctions.Test.

EXIT_CODE: 0

Output Summary:
- Test Run Successful. Total tests: 4995, Passed: 4995, Failed: 0. Total time ~27.9s.
- Repository raw overall line coverage (Cobertura root): 47.16% (lines-covered=85011 / lines-valid=180246); branch-rate 41.92%.
- Per-package line coverage: QuickFiler 72.53% (9309/12348), UtilitiesCS 46.81% (66634/141188),
  TaskMaster 48.61% (3664/7784), Swordfish.NET.General 33.19% (1820/5580, vendored),
  SVGControl 16.28% (544/3264, vendored), TaskVisualization 18.31% (52/238), Tags 33.64% (998/2986),
  ToDoModel 28.34% (1982/6850), VBFunctions 100.00% (8/8).
- Caveat: this raw repo-wide figure is a PRE-EXISTING state that includes large volumes of
  un-annotated COM/Outlook-interop and vendored code inside UtilitiesCS (141k valid lines) and the
  vendored Swordfish/SVGControl packages. It is well below the 80% testable-denominator target
  independent of this feature. F1's binding coverage obligations are therefore new-code >= 90%
  (StoreIdentity.cs, StoreDisableService.cs) and no regression on previously-covered lines, which
  P8-T4/P8-T5 verify against this baseline.
