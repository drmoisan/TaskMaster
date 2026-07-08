Timestamp: 2026-07-03T22-04-04:00
Command: $env:PATH = 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform;' + $env:PATH; vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:HighConfidencePreFilterLoader_CanBeOverridden_ForTesting,RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue,RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload,RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly
EXIT_CODE: 0
Output Summary: Targeted moved tests passed. VSTest ran 4 tests, 4 passed, 0 failed.

Build Prerequisite:
```text
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'
EXIT_CODE: 0
Build succeeded with 1 existing MSTEST0032 warning and 0 errors.
```

Output:
```text
VSTest version 18.7.0 (x64)

Starting test discovery, please wait...
A total of 1 test files matched the specified pattern.
  Passed HighConfidencePreFilterLoader_CanBeOverridden_ForTesting [253 ms]
  Passed RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue [173 ms]
  Passed RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload [58 ms]
  Passed RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly [1 ms]

Test Run Successful.
Total tests: 4
     Passed: 4
 Total time: 0.8261 Seconds
```
