Timestamp: 2026-09-29T09-15 Command: pwsh -NoProfile -File "coverage\plan882-helper.ps1" -EvidenceDirectory docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates -StepArtifactName qa-coverage-test-run -NewTestName "BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing" ; inner collection: dotnet-coverage collect --output coverage/coverage.cobertura.xml --output-format cobertura --settings coverage/coverage.cobertura.xml.effective-coverage.config -- vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook /ResultsDirectory:coverage/test-results /Logger:trx;LogFileName=mstest-coverage-run.trx /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /logger:console;verbosity=normal EXIT_CODE: 0 Summary derived from the trx document by Get-TrxRunSummary and Format-TrxRunSummary (scripts/vscode/Invoke-MSTest.TrxSummary.ps1); the per-test outcome lines are derived by the plan helper from the UnitTestResult elements rather than reported by the summary tool.  Test run outcome: Completed
Total 1469, executed 1469, passed 1469, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none 
TEST-OUTCOME EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt = Passed
TEST-OUTCOME EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose = Passed
TEST-OUTCOME EnsureDispatcher_ScopeDisposedTwice_IsIdempotent = Passed
TEST-OUTCOME Transaction_SecondCallerCannotInstallUntilTheFirstRestores = Passed
TEST-OUTCOME Transaction_DisposedTwice_DoesNotOverReleaseTheGate = Passed
TEST-OUTCOME Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException = Passed
TEST-OUTCOME TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition = Passed
TEST-OUTCOME BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing = Passed