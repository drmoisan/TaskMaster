Timestamp: 2026-09-29T09-15
Command: pwsh -NoProfile -File "coverage\plan882-helper.ps1" -EvidenceDirectory docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/qa-gates -StepArtifactName qa-coverage-test-run -NewTestName "BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing" ; inner collection: dotnet-coverage collect --output coverage/coverage.cobertura.xml --output-format cobertura --settings coverage/coverage.cobertura.xml.effective-coverage.config -- vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook /ResultsDirectory:coverage/test-results /Logger:trx;LogFileName=mstest-coverage-run.trx /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /logger:console;verbosity=normal
EXIT_CODE: 0
Output Summary:
- Scope: QuickFiler.Test.dll only. QuickFiler.Test is a test project outside the first-party coverage denominator (Get-KoverageProjectAllowlist drops every assembly whose name ends in .Test), so the figures are the first-party lines the QuickFiler.Test suite alone exercises; the repository-wide 80 percent line floor and 75 percent branch floor are NOT measured by this run, and the floor outcomes below are observations of the single-assembly denominator, not gates.
- TEST-RUN-OUTCOME=Completed
- TOTAL=1469 EXECUTED=1469 PASSED=1469 FAILED=0 SKIPPED-DERIVED=0
- FAILED-TESTS=NONE
- TEST-OUTCOME EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt = Passed
- TEST-OUTCOME EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose = Passed
- TEST-OUTCOME EnsureDispatcher_ScopeDisposedTwice_IsIdempotent = Passed
- TEST-OUTCOME Transaction_SecondCallerCannotInstallUntilTheFirstRestores = Passed
- TEST-OUTCOME Transaction_DisposedTwice_DoesNotOverReleaseTheGate = Passed
- TEST-OUTCOME Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException = Passed
- TEST-OUTCOME TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition = Passed
- TEST-OUTCOME BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing = Passed
- HANG-SEQUENCE-FILES=0
- First-party coverage: lines 15182/62182 (24.42%), branches 3763/16222 (23.20%)
- LINE-FLOOR-80-OBSERVATION=FAIL: Cobertura line coverage 24.4154% is below the required 80% threshold.
- BRANCH-FLOOR-75-OBSERVATION=FAIL: Cobertura branch coverage 23.1969% is below the required 75% threshold.
- Raw collector document, post-processed document and trx retained under the gitignored coverage directory only; none is committed.
ITERATION: 1
