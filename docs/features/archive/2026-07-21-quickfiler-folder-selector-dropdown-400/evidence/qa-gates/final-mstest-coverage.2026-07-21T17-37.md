# Final MSTest Coverage QA

Timestamp: 2026-07-21T17:37:11Z

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput 'docs\features\active\2026-07-21-quickfiler-folder-selector-dropdown-400\evidence\qa-gates\coverage-final.2026-07-21T17-35.cobertura.xml'`

EXIT_CODE: 0

Discovered test assemblies: 8

Total: 5829

Passed: 5829

Failed: 0

Skipped: 0

Elapsed: 59.8884 seconds

Coverage artifact: `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-final.2026-07-21T17-35.cobertura.xml`

Output Summary: The wrapper resolved VSTest through `vswhere`, discovered and ran all eight first-party test assemblies, including `UtilitiesCS.Test.dll` and `QuickFiler.Test.dll`, under `/TestCaseFilter:TestCategory!=LiveOutlook`. All tests passed. The 15 issue #400 class families and the five named issue #398 regressions have explicit discovery proof in `evidence/regression-testing/issue-400-integrated.2026-07-21T17-08.md` and `evidence/regression-testing/issue-398-regression.2026-07-21T17-08.md`; the complete assemblies containing those tests passed in this final run. `coverage.config` retained the baseline hash `83a8ce3bb198244c9b248bf1fe08a523ed9161d3`.
