Timestamp: 2026-07-06T11-30
Command: & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Tests:Tick_WhenHookupThrowsHResult90740111_ReturnsContinuePollingAndLeavesIncomplete,IsTransientError_WhenHResult90740111_ReturnsTrueAndEFailReturnsFalse
EXIT_CODE: 0
Output Summary:
- PASS: Targeted issue #242 pass-after VSTest command completed.
- Test result summary: Total tests: 2; Passed: 2
- Coordinator readiness-hookup retry test passed: True.
- Classifier test passed, confirming 0x90740111 is transient and 0x80004005 remains false: True.

Output Tail:
VSTest version 18.7.0 (x64)

Starting test discovery, please wait...
A total of 1 test files matched the specified pattern.
  Passed Tick_WhenHookupThrowsHResult90740111_ReturnsContinuePollingAndLeavesIncomplete [95 ms]
  Passed IsTransientError_WhenHResult90740111_ReturnsTrueAndEFailReturnsFalse [18 ms]

Test Run Successful.
Total tests: 2
     Passed: 2
 Total time: 0.2214 Seconds
