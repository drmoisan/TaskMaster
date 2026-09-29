# Fixture Structure Gates (P3-T4)

Timestamp: 2026-09-29T09-09
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $fx = "QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs"; "P2-T1-INTERMEDIATE-COUNTS:"; Get-Content -LiteralPath coverage/logs/p2-t1-counts.log; foreach ($l in @("using System.Globalization;", "internal const int TransactionGateAcquireTimeoutMs = 120000;", "120000", "bounded by")) { "FINAL-P2-T1-COUNT " + $l + " = " + @(Select-String -LiteralPath $fx -SimpleMatch -CaseSensitive -Pattern $l).Count }; foreach ($l in @("TransactionGate.WaitAsync()", "TransactionGate.WaitAsync(bound)", "bool acquired = await", "if (!acquired)", "throw new TimeoutException(", "TRANSACTIONGATE_ACQUIRE_TIMEOUT", "bound.TotalMilliseconds", "CultureInfo.InvariantCulture", "TimeSpan bound", "Task<UiThreadDispatcherTransaction> BeginTransactionAsync(", "BeginTransactionAsync()", "TimeoutException", "Interlocked.Increment(ref _transactionAcquisitions);", "Interlocked.Increment(ref _contendedAcquisitions);", "new UiThreadDispatcherTransaction()", "TransactionGate.Release()", "UiThreadDispatcherFixture.BeginTransactionAsync()")) { "FINAL-P2-T2-COUNT " + $l + " = " + @(Select-String -LiteralPath $fx -SimpleMatch -CaseSensitive -Pattern $l).Count }; "LINES-FX=" + @(Get-Content -LiteralPath $fx).Count; foreach ($l in @("Interlocked.Increment(ref _contendedAcquisitions);", "TransactionGate.WaitAsync(bound)", "throw new TimeoutException(", "Interlocked.Increment(ref _transactionAcquisitions);", "return new UiThreadDispatcherTransaction();")) { "LINE-NUMBER " + $l + " = " + (Select-String -LiteralPath $fx -SimpleMatch -Pattern $l | Select-Object -First 1).LineNumber }'
EXIT_CODE: 0

P2-T1-COMMAND: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $fx = "QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs"; & { "Timestamp: " + (Get-Date).ToString("yyyy-MM-ddTHH-mm"); foreach ($l in @("using System.Globalization;", "internal const int TransactionGateAcquireTimeoutMs = 120000;", "120000", "bounded by", "TransactionGate.WaitAsync()", "BeginTransactionAsync(TimeSpan bound)")) { "P2-T1-COUNT " + $l + " = " + @(Select-String -LiteralPath $fx -SimpleMatch -CaseSensitive -Pattern $l).Count } } | Tee-Object -FilePath coverage/logs/p2-t1-counts.log'

Output Summary:
- P2-T1-INTERMEDIATE-COUNTS: (transcribed from coverage/logs/p2-t1-counts.log, Timestamp 2026-09-29T09-07; every value met at P2-T1 time)
  - using System.Globalization; = 1 (required 1)
  - internal const int TransactionGateAcquireTimeoutMs = 120000; = 1 (required 1)
  - 120000 = 1 (required 1)
  - bounded by = 1 (required at least 1)
  - TransactionGate.WaitAsync() = 1 (required 1; superseded by the P2-T2 final value 0)
  - BeginTransactionAsync(TimeSpan bound) = 0 (required 0 at P2-T1 time; after formatting the literal is split across two lines and is replaced by the P2-T2 gates `TimeSpan bound` = 1 and signature heads = 2)
- Final-state P2-T1 counts (after P3-T1 formatting): using System.Globalization; = 1; internal const int TransactionGateAcquireTimeoutMs = 120000; = 1; 120000 = 1; bounded by = 1. All hold.
- Final-state P2-T2 counts (after P3-T1 formatting), all holding:
  - TransactionGate.WaitAsync() = 0
  - TransactionGate.WaitAsync(bound) = 1
  - bool acquired = await = 1
  - if (!acquired) = 1
  - throw new TimeoutException( = 1
  - TRANSACTIONGATE_ACQUIRE_TIMEOUT = 1
  - bound.TotalMilliseconds = 1
  - CultureInfo.InvariantCulture = 1
  - TimeSpan bound = 1
  - Task<UiThreadDispatcherTransaction> BeginTransactionAsync( = 2
  - BeginTransactionAsync() = 3 (required at least 3)
  - TimeoutException = 3 (required at least 3)
  - Interlocked.Increment(ref _transactionAcquisitions); = 1
  - Interlocked.Increment(ref _contendedAcquisitions); = 1
  - new UiThreadDispatcherTransaction() = 1
  - TransactionGate.Release() = 1
  - UiThreadDispatcherFixture.BeginTransactionAsync() = 1
- LINES-FX: 342 (greater than 304, at most 500)
- LINE-NUMBERS (strictly increasing, as required): Interlocked.Increment(ref _contendedAcquisitions); = 175; TransactionGate.WaitAsync(bound) = 178; throw new TimeoutException( = 181; Interlocked.Increment(ref _transactionAcquisitions); = 188; return new UiThreadDispatcherTransaction(); = 189. The contended pre-check precedes the wait, the throw precedes the acquisitions increment, and the increment precedes construction.
