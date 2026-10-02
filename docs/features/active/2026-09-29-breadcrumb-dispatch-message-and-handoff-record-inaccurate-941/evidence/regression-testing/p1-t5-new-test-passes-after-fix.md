# P1-T5 New test and line 86 test pass after the production literal change

Timestamp: 2026-10-01T07-05
Command: msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /nodeReuse:false /p:Configuration=Debug /p:Platform=AnyCPU ; then vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName=QuickFiler.Test.Viewers.BreadcrumbPopupBoundaryCoverageTests.DispatchValue_OwnerOnlyOnOwnerThread_FaultsOutsideExecutingCallback|FullyQualifiedName=QuickFiler.Test.Viewers.BreadcrumbPopupBoundaryCoverageTests.Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction" "/ResultsDirectory:coverage\test-results\941\p1-t5" "/Logger:trx;LogFileName=p1-t5.trx" ; then Format-TrxRunSummary (Get-TrxRunSummary -TrxContent <trx text>)
BUILD-EXIT: 0
SUMMARY-EXIT: 0
EXIT_CODE: 0
Output Summary:
- Build: `Build succeeded.` present once; `0 Warning(s)`, `0 Error(s)`; BUILD-EXIT 0.
- vstest exit code 0.
- Test run outcome: Completed
- Total 2, executed 2, passed 2, failed 0.
- Failed tests: none
- Fail-before evidence: FEATURE/evidence/regression-testing/p1-t2-expect-fail-new-test.md (Total 1, failed 1). Pass-after evidence: this run.
- The second test (Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction) is the line 86 assertion and exercises the unchanged Dispatch site.
- AC5 checked off in FEATURE/issue.md.
