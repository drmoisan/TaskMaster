Timestamp: 2026-07-03T18-54
Command: & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /Tests:Run_HighConfidenceEnabled_DoesNotLoadUnfilteredInitialBatch
EXIT_CODE: 1
Output Summary: Expected failing regression captured. VSTest ran 1 test and `Run_HighConfidenceEnabled_DoesNotLoadUnfilteredInitialBatch` failed because `QfcHomeController.Run()` invoked `IQfcDatamodel.InitEmailQueue(7, null)`, proving the synchronous high-confidence startup path still requests a fixed unfiltered first batch.

# Synchronous Run High-Confidence Expect-Fail Evidence

Build prerequisite:
- `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /p:Configuration=Debug /p:Platform='Any CPU'` failed because the project does not define that platform outside the solution context.
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'` succeeded and rebuilt `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`.

VSTest output:
```text
VSTest version 18.7.0 (x64)

Starting test discovery, please wait...
A total of 1 test files matched the specified pattern.
  Failed Run_HighConfidenceEnabled_DoesNotLoadUnfilteredInitialBatch [420 ms]
  Error Message:
   Test method QuickFiler.Controllers.Tests.QfcHomeControllerRunAsyncTests.Run_HighConfidenceEnabled_DoesNotLoadUnfilteredInitialBatch threw exception:
Moq.MockException: high-confidence synchronous startup must not request a fixed unfiltered first batch
Expected invocation on the mock should never have been performed, but was 1 times: m => m.InitEmailQueue(7, It.IsAny<BackgroundWorker>())

Performed invocations:

   Mock<IQfcDatamodel:1> (m):

      IQfcDatamodel.InitEmailQueue(7, null)

Total tests: 1
     Failed: 1
 Total time: 0.7058 Seconds
Test Run Failed.
```
