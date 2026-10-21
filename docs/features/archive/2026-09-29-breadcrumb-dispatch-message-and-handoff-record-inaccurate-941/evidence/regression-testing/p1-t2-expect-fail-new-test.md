# P1-T2 Expect-fail run of the new regression test (before the production literal change)

Timestamp: 2026-10-01T06-55
Command: msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /nodeReuse:false /p:Configuration=Debug /p:Platform=AnyCPU ; then vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName=QuickFiler.Test.Viewers.BreadcrumbPopupBoundaryCoverageTests.DispatchValue_OwnerOnlyOnOwnerThread_FaultsOutsideExecutingCallback" "/ResultsDirectory:coverage\test-results\941\p1-t2" "/Logger:trx;LogFileName=p1-t2.trx" ; then Format-TrxRunSummary (Get-TrxRunSummary -TrxContent <trx text>)
ExpectedExitCode: 1
BUILD-EXIT: 0
SUMMARY-EXIT: 0
EXIT_CODE: 1
Output Summary:
- Build: `Build succeeded.` present once; `0 Warning(s)`, `0 Error(s)`; BUILD-EXIT 0.
- vstest exit code 1 (the expected outcome of this expect-fail task).
- Test run outcome: Failed
- Total 1, executed 1, passed 0, failed 1.
- Failed tests: DispatchValue_OwnerOnlyOnOwnerThread_FaultsOutsideExecutingCallback
- Failed test Message (trx Message element): Expected fault.Message "The owner-thread-only test dispatcher cannot marshal cross-thread UI work." to contain "outside an executing Dispatch callback".
- Message contains `to contain`: yes. Message contains `outside an executing Dispatch callback`: yes. The failure text matches the plan's prediction; no mismatch.
- No vstest.console, testhost or dotnet-coverage process was running before the build and test (checked with Get-Process).
- The trx file stays under the git-ignored coverage\test-results\941\p1-t2\ directory and is not copied here.
