# P5-T5: Final Scoped Test-and-Coverage Run

Timestamp: 2026-09-03T12-05

Command (plan-literal, attempted first): pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot UtilitiesCS.Test -Configuration Debug -CoverageOutput docs/features/active/2026-09-02-folderconverter-folderpredictor-dead-code-and-bugs-732/evidence/qa-gates/coverage-final.cobertura.xml
EXIT_CODE: 1 (known environment defect, same as P0-T10)

Output Summary (plan-literal attempt): the script threw "No test assemblies found
under '...\.claude\worktrees\agent-aa274c17b2c682ab3\UtilitiesCS.Test' for
configuration 'Debug'. Build first.", reproducing the same pre-existing
`.claude`-path-exclusion defect in scripts/vscode/Invoke-MSTestWithCoverage.ps1 line
301 documented in P0-T10. Execution falls back to the same substituted
vstest.console.exe / dotnet-coverage invocation used for the baseline capture.

Command (substituted fallback): dotnet-coverage collect --output docs/features/active/2026-09-02-folderconverter-folderpredictor-dead-code-and-bugs-732/evidence/qa-gates/coverage-final.cobertura.xml --output-format cobertura --settings coverage.config -- "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook
EXIT_CODE: 0

Output Summary (substituted fallback, primary result for this task):
"Test Run Successful. Total tests: 4786 Passed: 4786." (console log persisted verbatim
at evidence/qa-gates/p5-t5-console.log, 347,922 bytes). Failed count: 0. Passed count
(4786) equals the P0-T10-recorded baseline Passed count (4785) plus 1 (the single new
regression test added in Phase 1). The emitted Cobertura XML root `<coverage>` element
(evidence/qa-gates/coverage-final.cobertura.xml, 29,936,463 bytes) reports
`line-rate="0.7073783191750814"` -- **final line-rate = 70.74%** (lines-covered=105920,
lines-valid=149736). 70.74% >= the P0-T10-recorded baseline line-rate of 70.73%: no
coverage regression.
