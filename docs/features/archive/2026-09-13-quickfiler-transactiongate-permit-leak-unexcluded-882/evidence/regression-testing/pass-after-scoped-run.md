# Pass-After Scoped Run of the Fixture Test Class (P3-T3)

Timestamp: 2026-09-29T09-09
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $v = & (Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio/Installer/vswhere.exe") -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; Get-ChildItem coverage/test-results -Recurse -File -Filter "Sequence_*.xml" -ErrorAction SilentlyContinue | Remove-Item -Force; & $v QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~QfcItemController_UiThreadDispatcherFixtureTests" /ResultsDirectory:coverage/test-results "/Logger:trx;LogFileName=scoped-fixture-class.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" 2>&1 | Tee-Object -FilePath coverage/logs/pass-after-scoped-run.log | Out-Null; "EXIT=$LASTEXITCODE"; [xml]$x = Get-Content -LiteralPath coverage/test-results/scoped-fixture-class.trx -Raw; $c = $x.TestRun.ResultSummary.Counters; "TOTAL=" + $c.total + " EXECUTED=" + $c.executed + " PASSED=" + $c.passed + " FAILED=" + $c.failed; foreach ($r in @($x.TestRun.Results.UnitTestResult)) { "TEST-OUTCOME " + $r.testName + " = " + $r.outcome }; "HANG-SEQUENCE-FILES=" + @(Get-ChildItem coverage/test-results -Recurse -File -Filter "Sequence_*.xml" -ErrorAction SilentlyContinue).Count'
EXIT_CODE: 0
Output Summary:
- Run settings: scripts/vscode/TaskMaster.cli.runsettings (parallel regime, Workers 0, Scope ClassLevel); not a serial-only run.
- TOTAL=8 EXECUTED=8 PASSED=8 FAILED=0
- TEST-OUTCOME EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose = Passed
- TEST-OUTCOME Transaction_DisposedTwice_DoesNotOverReleaseTheGate = Passed
- TEST-OUTCOME BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing = Passed
- TEST-OUTCOME Transaction_SecondCallerCannotInstallUntilTheFirstRestores = Passed
- TEST-OUTCOME EnsureDispatcher_ScopeDisposedTwice_IsIdempotent = Passed
- TEST-OUTCOME EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt = Passed
- TEST-OUTCOME Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException = Passed
- TEST-OUTCOME TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition = Passed
- HANG-SEQUENCE-FILES=0
- The issue #823 re-run branch was not needed (no Failed line). The trx is retained under the gitignored coverage directory only (coverage/test-results/scoped-fixture-class.trx) and is not committed.
