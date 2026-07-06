Timestamp: 2026-07-06T11-28
Command: msbuild TaskMaster.Test\TaskMaster.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
Output Summary:
- PASS: Project build completed for issue #242 fail-before regression tests.
- MSBuild warning count: 9; error count: 0.
- Issue #242 test methods present in TaskMaster.Test\AppGlobals\HookReadinessCoordinatorTests.cs: True.
- Test DLL rebuild after test source timestamp: yes.
- TaskMaster.Test\bin\Debug\TaskMaster.Test.dll LastWriteTime=2026-07-06T11:28:47 Length=251904.

Output Tail:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  Copying file from "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\obj\Debug\TaskMaster.Test.dll" to "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll".
  TaskMaster.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
  Copying file from "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\obj\Debug\TaskMaster.Test.pdb" to "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\bin\Debug\TaskMaster.Test.pdb".
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj" (Build target(s)).

Build succeeded.

"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj" (Build target) (1) ->
(CoreCompile target) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\TestableApplicationGlobals.cs(21,39): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\ApplicationGlobalsStartupTimingTests.cs(227,43): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\ApplicationGlobalsStartupTimingTests.cs(231,30): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\OutlookObjects\Store\StoresWrapperTests.cs(390,27): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\TestableApplicationGlobals.cs(25,26): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\AppToDoObjectsTests.cs(47,31): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\AppToDoObjectsTests.cs(48,33): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\EngineInitTimingProbeTests.cs(91,73): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\AppGlobals\EngineInitTimingProbeTests.cs(136,80): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj]

    9 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.23
