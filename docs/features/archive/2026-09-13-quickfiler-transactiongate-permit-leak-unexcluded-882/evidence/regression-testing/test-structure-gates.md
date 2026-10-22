# Test File Structure Gates (P3-T5)

Timestamp: 2026-09-29T09-10
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $ft = "QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs"; foreach ($l in @("TimeSpan.Zero", "ThrowAsync<TimeoutException>", "TRANSACTIONGATE_ACQUIRE_TIMEOUT", "NotThrow<SemaphoreFullException>", "BeGreaterThanOrEqualTo(", "[TestMethod]", "[Timeout(GateTimeoutMs)]", "DoNotParallelize", "Thread.Sleep", "Task.Delay", "Stopwatch", "[Retry", "BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing", "UiThreadDispatcherFixture.TransactionReleases", "roundTrip.Dispose();", "EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt", "EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose", "EnsureDispatcher_ScopeDisposedTwice_IsIdempotent", "Transaction_SecondCallerCannotInstallUntilTheFirstRestores", "Transaction_DisposedTwice_DoesNotOverReleaseTheGate", "Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException", "TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition")) { "FT-COUNT " + $l + " = " + @(Select-String -LiteralPath $ft -SimpleMatch -CaseSensitive -Pattern $l).Count }; "LINES-FT=" + @(Get-Content -LiteralPath $ft).Count; git diff --numstat 177b6d78e1b2408e5aedbd794cef3aad6b7fb372 -- $ft'
EXIT_CODE: 0
Output Summary:
- P1-T1 counts re-measured after P3-T1 formatting (all hold):
  - TimeSpan.Zero = 1
  - ThrowAsync<TimeoutException> = 1
  - TRANSACTIONGATE_ACQUIRE_TIMEOUT = 2
  - NotThrow<SemaphoreFullException> = 1
  - BeGreaterThanOrEqualTo( = 1
  - [TestMethod] = 8
  - [Timeout(GateTimeoutMs)] = 8
  - DoNotParallelize = 0
  - Thread.Sleep = 0
  - Task.Delay = 0
  - Stopwatch = 0
  - [Retry = 0
  - BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing = 1
- UiThreadDispatcherFixture.TransactionReleases = 2 (required 2)
- roundTrip.Dispose(); = 2 (required 2)
- Pre-existing test method names, each exactly 1: EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt = 1; EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose = 1; EnsureDispatcher_ScopeDisposedTwice_IsIdempotent = 1; Transaction_SecondCallerCannotInstallUntilTheFirstRestores = 1; Transaction_DisposedTwice_DoesNotOverReleaseTheGate = 1; Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException = 1; TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition = 1
- NUMSTAT (git diff --numstat 177b6d78e1b2408e5aedbd794cef3aad6b7fb372 -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs): `62	0` (62 added, 0 deleted)
- LINES-FT: 458 (greater than 396, at most 500; FILE SIZE LIMIT EXCEEDED did not fire)
