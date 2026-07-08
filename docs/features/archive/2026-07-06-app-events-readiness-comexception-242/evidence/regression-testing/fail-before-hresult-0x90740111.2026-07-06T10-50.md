Timestamp: 2026-07-06T11-29
Command: & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Tests:Tick_WhenHookupThrowsHResult90740111_ReturnsContinuePollingAndLeavesIncomplete,IsTransientError_WhenHResult90740111_ReturnsTrueAndEFailReturnsFalse
EXIT_CODE: 1
Output Summary:
- EXPECTED_FAIL: Targeted issue #242 fail-before VSTest command completed.
- Test result summary: Total tests: 2; Passed: 1; Failed: 1
- Classifier-level regression failed because 0x90740111 is not transient before the fix: True.
- Non-transient 0x80004005 false assertion remained preserved: True.
- Coordinator issue #242 readiness-hookup test passed: True.

Output Tail:
VSTest version 18.7.0 (x64)

Starting test discovery, please wait...
A total of 1 test files matched the specified pattern.
  Passed Tick_WhenHookupThrowsHResult90740111_ReturnsContinuePollingAndLeavesIncomplete [93 ms]
  Failed IsTransientError_WhenHResult90740111_ReturnsTrueAndEFailReturnsFalse [47 ms]
  Error Message:
   Expected gate.IsTransientError(MakeComException(0x90740111)) to be True, but found False.
  Stack Trace:
     at FluentAssertions.Execution.LateBoundTestFramework.Throw(String message) in /_/Src/FluentAssertions/Execution/LateBoundTestFramework.cs:line 22
   at FluentAssertions.Execution.AssertionChain.FailWith(Func`1 getFailureReason) in /_/Src/FluentAssertions/Execution/AssertionChain.cs:line 267
   at FluentAssertions.Primitives.BooleanAssertions`1.BeTrue(String because, Object[] becauseArgs) in /_/Src/FluentAssertions/Primitives/BooleanAssertions.cs:line 82
   at TaskMaster.Test.AppGlobals.HookReadinessCoordinatorTests.IsTransientError_WhenHResult90740111_ReturnsTrueAndEFailReturnsFalse() in C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\HookReadinessCoordinatorTests.cs:line 124

Total tests: 2
     Passed: 1
     Failed: 1
Test Run Failed.
 Total time: 0.2470 Seconds
