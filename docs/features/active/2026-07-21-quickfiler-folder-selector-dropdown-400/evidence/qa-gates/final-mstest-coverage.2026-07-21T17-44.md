# Final MSTest coverage gate

Timestamp: 2026-07-21T17-44Z

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput 'docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\qa-gates\coverage-final.2026-07-21T17-44.cobertura.xml'`

EXIT_CODE: 0

Discovered test assemblies: 8

Total: 5830

Passed: 5830

Failed: 0

Skipped: 0

Elapsed: 53.4409 seconds

Coverage artifact: `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-final.2026-07-21T17-44.cobertura.xml`

Output Summary: The wrapper resolved VSTest through `vswhere`, discovered and ran all eight first-party test assemblies, including the complete `UtilitiesCS.Test.dll` and `QuickFiler.Test.dll` assemblies, under `/TestCaseFilter:TestCategory!=LiveOutlook`. All 5830 tests passed. The 15 issue #400 class families and the five named issue #398 regressions have explicit discovery proof in `evidence/regression-testing/issue-400-integrated.2026-07-21T17-08.md` and `evidence/regression-testing/issue-398-regression.2026-07-21T17-08.md`; the complete assemblies containing those tests passed again in this run. The added router cancellation-path test was included. `coverage.config` remained unchanged with Git object hash `83a8ce3bb198244c9b248bf1fe08a523ed9161d3`.
