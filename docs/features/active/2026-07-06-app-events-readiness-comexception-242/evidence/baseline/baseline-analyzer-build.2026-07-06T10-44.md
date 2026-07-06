Timestamp: 2026-07-06T11-11
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
Output Summary:
- Command completed with exit code 0.
- Analyzer warning/error signal lines found: warnings=145; errors=1.
- MSBuild summary: 72 Warning(s)
- MSBuild summary: 0 Error(s)
- Command output tail:
  "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\QuickFiler.Test\QuickFiler.Test.csproj" (default target) (13) ->
    C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\QuickFiler.Test\Controllers\QfcFormControllerTests.cs(694,13): warning MSTEST0032: Review or remove the assertion as its condition is known to be always true (https://learn.microsoft.com/dotnet/core/testing/mstest-analyzers/mstest0032) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\QuickFiler.Test\QuickFiler.Test.csproj]
  
  
  "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.sln" (Build target) (1) ->
  "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj" (default target) (20) ->
    C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\OutlookObjects\Store\StoresWrapperTests.cs(390,27): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
    C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\TestableApplicationGlobals.cs(21,39): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
    C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\ApplicationGlobalsStartupTimingTests.cs(227,43): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
    C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\ApplicationGlobalsStartupTimingTests.cs(231,30): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
    C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\TestableApplicationGlobals.cs(25,26): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
    C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\AppToDoObjectsTests.cs(47,31): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
    C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\AppToDoObjectsTests.cs(48,33): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
    C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\EngineInitTimingProbeTests.cs(91,73): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
    C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\EngineInitTimingProbeTests.cs(136,80): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
  
      72 Warning(s)
      0 Error(s)
  
  Time Elapsed 00:00:14.96
