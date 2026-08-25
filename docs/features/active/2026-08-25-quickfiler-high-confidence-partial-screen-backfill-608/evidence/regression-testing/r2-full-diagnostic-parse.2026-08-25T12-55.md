Timestamp: 2026-08-25T12-55
Command: Parse `r2-full-diagnostic.trx` `UnitTestResult` nodes with outcome other than `Passed`, joining `testId` to `TestDefinitions/UnitTest/TestMethod`.
EXIT_CODE: 0
Output Summary: `FailedTestCount: 1`. The only non-passing result is a QuickFiler gate test; its assertion expected one item but received two.

FailedTestCount: 1

1. Fully Qualified Test Name: `QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_DeadlineExpiresDuringInFlightScore_IncludesFinalAcceptedItem`
   - Source Assembly: `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-25T11-36\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`
   - Outcome: `Failed`
   - Error Message: `Expected result to contain a single item because the final in-flight acceptance is included, but found {Mock<MailItem:200>.Object, Mock<MailItem:201>.Object}.`
   - Stack Trace:
     ```text
     at FluentAssertions.Execution.LateBoundTestFramework.Throw(String message) in /_/Src/FluentAssertions/Execution/LateBoundTestFramework.cs:line 22
     at FluentAssertions.Execution.AssertionChain.FailWith(Func`1 getFailureReason) in /_/Src/FluentAssertions/Execution/AssertionChain.cs:line 277
     at FluentAssertions.Collections.GenericCollectionAssertions`3.ContainSingle(String because, Object[] becauseArgs) in /_/Src/FluentAssertions/Collections/GenericCollectionAssertions.cs:line 1214
     at QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.<DequeueAsync_DeadlineExpiresDuringInFlightScore_IncludesFinalAcceptedItem>d__21.MoveNext() in C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-25T11-36\QuickFiler.Test\Controllers\QfcStreamingDequeueConfidenceGateTests.Part2.cs:line 184
     --- End of stack trace from previous location where exception was thrown ---
     at System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
     at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
     at Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.Execution.TestMethodInfo.<ExecuteInternalAsync>d__58.MoveNext()
     ```
