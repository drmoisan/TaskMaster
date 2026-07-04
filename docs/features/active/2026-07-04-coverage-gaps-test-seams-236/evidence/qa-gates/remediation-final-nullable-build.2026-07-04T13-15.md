# Remediation Final Nullable Build

TASK: P10-T3
COMMAND: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
EXIT_CODE: 0
ERROR_LINES: 0
WARNING_LINES: 0

OUTPUT:
```text
MSBuild version 18.7.8+1ac568fee for .NET Framework
Build started 7/4/2026 2:54:37 PM.

Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.sln" on node 1 (Build target(s)).
ValidateSolutionConfiguration:
  Building solution configuration "Debug|Any CPU".
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\Tags\Tags.csproj" (2) on node 1 (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\Tags\Tags.csproj" (2) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\UtilitiesCS\UtilitiesCS.csproj" (3:2) on node 1 (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\UtilitiesCS\UtilitiesCS.csproj" (3:2) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\SVGControl\SVGControl.csproj" (5:2) on node 1 (default targets).
CoreResGen:
  No resources are out of date with respect to their source files. Skipping resource generation.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  SVGControl -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\SVGControl\bin\Debug\SVGControl.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\SVGControl\SVGControl.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\UtilitiesCS\UtilitiesCS.csproj" (3:2) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\UtilitiesSwordfish\UtilitiesSwordfish.NET.General.csproj" (4:3) on node 1 (default targets).
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  UtilitiesSwordfish.NET.General -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\UtilitiesSwordfish\bin\Debug\Swordfish.NET.General.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\UtilitiesSwordfish\UtilitiesSwordfish.NET.General.csproj" (default targets).
CoreResGen:
  No resources are out of date with respect to their source files. Skipping resource generation.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  UtilitiesCS -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\UtilitiesCS\bin\Debug\UtilitiesCS.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\UtilitiesCS\UtilitiesCS.csproj" (default targets).
CoreResGen:
  No resources are out of date with respect to their source files. Skipping resource generation.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  Tags -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\Tags\bin\Debug\Tags.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\Tags\Tags.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\ToDoModel\ToDoModel.csproj" (6) on node 1 (default targets).
CoreResGen:
  No resources are out of date with respect to their source files. Skipping resource generation.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  ToDoModel -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\ToDoModel\bin\Debug\ToDoModel.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\ToDoModel\ToDoModel.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\ToDoModel.Test\ToDoModel.Test.csproj" (7) on node 1 (default targets).
CoreResGen:
  No resources are out of date with respect to their source files. Skipping resource generation.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  ToDoModel.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\ToDoModel.Test\ToDoModel.Test.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskVisualization\TaskVisualization.csproj" (8) on node 1 (default targets).
CoreResGen:
  No resources are out of date with respect to their source files. Skipping resource generation.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  TaskVisualization -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskVisualization\bin\Debug\TaskVisualization.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskVisualization\TaskVisualization.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (9) on node 1 (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (9) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster\TaskMaster.csproj" (10:2) on node 1 (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster\TaskMaster.csproj" (10:2) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\QuickFiler\QuickFiler.csproj" (11:2) on node 1 (default targets).
MainResourcesGeneration:
Skipping target "MainResourcesGeneration" because all output files are up-to-date with respect to the input files.
CoreResGen:
  No resources are out of date with respect to their source files. Skipping resource generation.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  QuickFiler -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\QuickFiler\bin\Debug\QuickFiler.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\QuickFiler\QuickFiler.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster\TaskMaster.csproj" (10:2) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskTree\TaskTree.csproj" (12:2) on node 1 (default targets).
CoreResGen:
  No resources are out of date with respect to their source files. Skipping resource generation.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  TaskTree -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskTree\bin\Debug\TaskTree.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskTree\TaskTree.csproj" (default targets).
CoreResGen:
  No resources are out of date with respect to their source files. Skipping resource generation.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectoryAlways:
  Copying file from "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster\log4net.config" to "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster\bin\Debug\log4net.config".
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  TaskMaster -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster\bin\Debug\TaskMaster.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster\TaskMaster.csproj" (default targets).
CoreResGen:
  No resources are out of date with respect to their source files. Skipping resource generation.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectoryAlways:
  Copying file from "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster\log4net.config" to "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\UtilitiesCS.Test\bin\Debug\log4net.config".
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  UtilitiesCS.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\QuickFiler.Test\QuickFiler.Test.csproj" (13) on node 1 (default targets).
CoreResGen:
  No resources are out of date with respect to their source files. Skipping resource generation.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  QuickFiler.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\QuickFiler.Test\QuickFiler.Test.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskVisualization.Test\TaskVisualization.Test.csproj" (14) on node 1 (default targets).
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  TaskVisualization.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskVisualization.Test\TaskVisualization.Test.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\UtilitiesSwordfish.Test\UtilitiesSwordfish.NET.Test.csproj" (15) on node 1 (default targets).
MainResourcesGeneration:
Skipping target "MainResourcesGeneration" because all output files are up-to-date with respect to the input files.
CoreResGen:
  No resources are out of date with respect to their source files. Skipping resource generation.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  UtilitiesSwordfish.NET.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\UtilitiesSwordfish.Test\bin\Debug\Swordfish.NET.Test.exe
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\UtilitiesSwordfish.Test\UtilitiesSwordfish.NET.Test.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\Tags.Test\Tags.Test.csproj.metaproj" (16) on node 1 (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\Tags.Test\Tags.Test.csproj.metaproj" (16) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\Tags.Test\Tags.Test.csproj" (17) on node 1 (default targets).
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  Tags.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\Tags.Test\bin\Debug\Tags.Test.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\Tags.Test\Tags.Test.csproj" (default targets).
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\Tags.Test\Tags.Test.csproj.metaproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\VBFunctions\VBFunctions.csproj" (18) on node 1 (default targets).
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  VBFunctions -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\VBFunctions\bin\Debug\VBFunctions.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\VBFunctions\VBFunctions.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\VBFunctions.Test\VBFunctions.Test.csproj" (19) on node 1 (default targets).
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  VBFunctions.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\VBFunctions.Test\VBFunctions.Test.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.Test\TaskMaster.Test.csproj" (20) on node 1 (default targets).
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectoryAlways:
  Copying file from "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster\log4net.config" to "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.Test\bin\Debug\log4net.config".
  Copying file from "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.Test\log4net.config" to "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.Test\bin\Debug\log4net.config".
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  TaskMaster.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.Test\TaskMaster.Test.csproj" (default targets).
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-07-04-12-57\TaskMaster.sln" (Build target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.45
```
