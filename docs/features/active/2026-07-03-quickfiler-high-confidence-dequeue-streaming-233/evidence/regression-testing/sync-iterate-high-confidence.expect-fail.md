Timestamp: 2026-07-03T18-54
Command: & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /Tests:Iterate_HighConfidenceEnabled_DoesNotLoadDirectSynchronousBatch
EXIT_CODE: 1
Output Summary: Expected failing regression captured. VSTest ran 1 test and `Iterate_HighConfidenceEnabled_DoesNotLoadDirectSynchronousBatch` failed because `QfcHomeController.Iterate()` invoked `IQfcDatamodel.DequeueNextItemGroup(8)`, proving synchronous high-confidence iteration can bypass the dequeue-time confidence gate.

# Synchronous Iterate High-Confidence Expect-Fail Evidence

Build prerequisite:
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'` succeeded before VSTest and rebuilt `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`.
- The solution build reported 1 existing MSTEST0032 warning and 0 errors.

VSTest output:
```text
VSTest version 18.7.0 (x64)

Starting test discovery, please wait...
A total of 1 test files matched the specified pattern.
  Failed Iterate_HighConfidenceEnabled_DoesNotLoadDirectSynchronousBatch [329 ms]
  Error Message:
   Test method QuickFiler.Controllers.Tests.QfcHomeControllerIterationTests.Iterate_HighConfidenceEnabled_DoesNotLoadDirectSynchronousBatch threw exception:
Moq.MockException: high-confidence synchronous iteration must not use the direct dequeue bypass
Expected invocation on the mock should never have been performed, but was 1 times: m => m.DequeueNextItemGroup(8)

Performed invocations:

   Mock<IQfcDatamodel:1> (m):

      IQfcDatamodel.DequeueNextItemGroup(8)

Total tests: 1
     Failed: 1
 Total time: 0.6233 Seconds
Test Run Failed.
```
