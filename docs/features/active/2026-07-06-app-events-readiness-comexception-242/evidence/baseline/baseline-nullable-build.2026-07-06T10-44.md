Timestamp: 2026-07-06T11-12
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
EXIT_CODE: 0
Output Summary:
- Command completed with exit code 0.
- Compiler warning/error signal lines found: warnings=1; errors=1.
- Nullable-related signal lines found: 0.
- MSBuild summary: 0 Warning(s)
- MSBuild summary: 0 Error(s)
- Command output tail:
    VBFunctions.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
  Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\VBFunctions.Test\VBFunctions.Test.csproj" (default targets).
  Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj" (20) on node 1 (default targets).
  GenerateTargetFrameworkMonikerAttribute:
  Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
  CoreCompile:
  Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
  _CopyOutOfDateSourceItemsToOutputDirectory:
  Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
  _CopyOutOfDateSourceItemsToOutputDirectoryAlways:
    Copying file from "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster\log4net.config" to "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\bin\Debug\log4net.config".
    Copying file from "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\log4net.config" to "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\bin\Debug\log4net.config".
  _CopyAppConfigFile:
  Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
  CopyFilesToOutputDirectory:
    TaskMaster.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
  Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.Test\TaskMaster.Test.csproj" (default targets).
  Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-06-10-36\TaskMaster.sln" (Build target(s)).
  
  Build succeeded.
      0 Warning(s)
      0 Error(s)
  
  Time Elapsed 00:00:01.32
