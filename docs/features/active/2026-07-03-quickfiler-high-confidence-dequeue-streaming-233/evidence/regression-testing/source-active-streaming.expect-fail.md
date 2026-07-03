Timestamp: 2026-07-03T18-56
Command: & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /Tests:DequeueAsync_SourceActiveAfterRepeatedEmptyReads_ContinuesPollingUntilCandidateArrives
EXIT_CODE: 1
Output Summary: Expected failing regression captured. VSTest ran 1 test and `DequeueAsync_SourceActiveAfterRepeatedEmptyReads_ContinuesPollingUntilCandidateArrives` failed because the gate completed after one empty retry while the source was still active.

# Source-Active Streaming Expect-Fail Evidence

Build prerequisite:
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'` succeeded before VSTest.
- The solution build reported 1 existing MSTEST0032 warning and 0 errors.

VSTest output:
```text
VSTest version 18.7.0 (x64)

Starting test discovery, please wait...
A total of 1 test files matched the specified pattern.
  Failed DequeueAsync_SourceActiveAfterRepeatedEmptyReads_ContinuesPollingUntilCandidateArrives [349 ms]
  Error Message:
   Expected pending.IsCompleted to be False because the source is still active, so an empty poll must not be treated as exhaustion, but found True.

Total tests: 1
     Failed: 1
 Total time: 0.7322 Seconds
Test Run Failed.
```
