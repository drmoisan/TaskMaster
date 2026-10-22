# P1-T7 Three affected tests pass

Timestamp: 2026-10-01T07-12
Command: msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /nodeReuse:false /p:Configuration=Debug /p:Platform=AnyCPU ; then vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName=QuickFiler.Test.Viewers.BreadcrumbUiThreadDispatchTests.ProductionCaptureWithoutUiContext_FailsFast|FullyQualifiedName=QuickFiler.Test.Viewers.BreadcrumbPopupBoundaryCoverageTests.DispatchValue_OwnerOnlyOnOwnerThread_FaultsOutsideExecutingCallback|FullyQualifiedName=QuickFiler.Test.Viewers.BreadcrumbPopupBoundaryCoverageTests.Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction" "/ResultsDirectory:coverage\test-results\941\p1-t7" "/Logger:trx;LogFileName=p1-t7.trx" ; then Format-TrxRunSummary (Get-TrxRunSummary -TrxContent <trx text>)
BUILD-EXIT: 0
SUMMARY-EXIT: 0
EXIT_CODE: 0
Output Summary:
- Build: `Build succeeded.` present once; `0 Warning(s)`, `0 Error(s)`; BUILD-EXIT 0.
- vstest exit code 0.
- Test run outcome: Completed
- Total 3, executed 3, passed 3, failed 0.
- Failed tests: none
- Tests covered: BreadcrumbUiThreadDispatchTests.ProductionCaptureWithoutUiContext_FailsFast (line 305 pattern), BreadcrumbPopupBoundaryCoverageTests.DispatchValue_OwnerOnlyOnOwnerThread_FaultsOutsideExecutingCallback (new), BreadcrumbPopupBoundaryCoverageTests.Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction (line 86 assertion).
- AC3 checked off in FEATURE/issue.md.
