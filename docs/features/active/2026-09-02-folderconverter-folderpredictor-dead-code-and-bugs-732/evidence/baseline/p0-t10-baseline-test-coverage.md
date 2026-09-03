# P0-T10: Baseline Scoped Test-and-Coverage Run

Timestamp: 2026-09-03T11-28

Command (plan-literal, attempted first): pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot UtilitiesCS.Test -Configuration Debug -CoverageOutput docs/features/active/2026-09-02-folderconverter-folderpredictor-dead-code-and-bugs-732/evidence/baseline/coverage-baseline.cobertura.xml
EXIT_CODE: 1 (known environment defect)

Output Summary (plan-literal attempt): the script threw "No test assemblies found under
'...\.claude\worktrees\agent-aa274c17b2c682ab3\UtilitiesCS.Test' for configuration
'Debug'. Build first." This is the known pre-existing defect in
scripts/vscode/Invoke-MSTestWithCoverage.ps1 line 301
(`$_.FullName -notmatch '\\\.claude\\'`), which excludes every assembly whose absolute
path contains a `.claude` path segment. This item worktree lives under
`.claude\worktrees\agent-aa274c17b2c682ab3\`, so the filter always excludes
UtilitiesCS.Test.dll here even though the build succeeded (confirmed present at
UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll from the P0-T8/P0-T9 solution
rebuilds). This defect is out of this plan's scope and is not edited by this plan; per
the delegation instructions, execution falls back to a direct vstest.console.exe /
dotnet-coverage invocation below.

Command (substituted fallback): dotnet-coverage collect --output docs/features/active/2026-09-02-folderconverter-folderpredictor-dead-code-and-bugs-732/evidence/baseline/coverage-baseline.cobertura.xml --output-format cobertura --settings coverage.config -- "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook
EXIT_CODE: 0

Output Summary (substituted fallback, primary result for this task):
"Test Run Successful. Total tests: 4785 Passed: 4785." (console log persisted verbatim
at evidence/baseline/p0-t10-console.log, 348,033 bytes). Failed count: 0. The emitted
Cobertura XML root `<coverage>` element
(evidence/baseline/coverage-baseline.cobertura.xml, 29,933,133 bytes) reports
`line-rate="0.7072916597091885"` -- **baseline line-rate = 70.73%**
(lines-covered=105895, lines-valid=149719; whole-instrumented-process figure including
vendored/third-party assemblies loaded at runtime, consistent with this repo's known
raw-vs-first-party coverage gap). This 70.73% baseline line-rate is the reference value
P5-T5's no-regression acceptance criterion compares against. The Passed count of 4785
is the reference value P5-T5's "+1 new regression test" acceptance criterion compares
against.
