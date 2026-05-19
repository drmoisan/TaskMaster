# P1-T5 Expect-Fail Evidence

Timestamp: 2026-03-27T09:15:00Z

Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug

EXIT_CODE: 1

Failure: RemoveItem_WhenTokenPreCancelled_DoesNotThrow — Failed [83 ms]

```
Did not expect System.OperationCanceledException, but found
System.OperationCanceledException: The operation was canceled.
  at System.Threading.CancellationToken.ThrowOperationCanceledException()
  at FluentAssertions.Specialized.AsyncFunctionAssertions`2.<NotThrowAsync>d__12`1.MoveNext()
    in AsyncFunctionAssertions.cs:line 302

Stack trace root:
  at QuickFiler.Controllers.Tests.QfcQueueTests.<RemoveItem_WhenTokenPreCancelled_DoesNotThrow>d__0.MoveNext()
    in C:\Users\DanMoisan\repos\TaskMaster\QuickFiler.Test\Controllers\QfcQueueTests.cs:line 56
```

Test counts: Total 2874 | Passed 2871 | Failed 1 | Skipped 2

Conclusion: Bug confirmed. The test correctly reproduces the scenario. RemoveItem propagates OperationCanceledException when _token is pre-cancelled and _jobsRunning > 0.
