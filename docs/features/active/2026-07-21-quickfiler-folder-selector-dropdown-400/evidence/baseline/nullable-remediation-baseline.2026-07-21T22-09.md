Timestamp: 2026-07-21T22-09Z
Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0
Output Summary: Nullable warnings-as-errors solution build succeeded with 5 existing System.Reactive packages.config compatibility warnings, 0 compiler/nullable diagnostics, and 0 errors.

Complete command result:

```text
MSBuild version 18.8.2+ce25c0108 for .NET Framework
Build started 7/21/2026 6:08:13 PM.

Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" on node 1 (Build target(s)).
ValidateSolutionConfiguration:
  Building solution configuration "Debug|Any CPU".
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\Tags\Tags.csproj" (2) on node 1 (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\Tags\Tags.csproj" (2) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS\UtilitiesCS.csproj" (3:2) on node 1 (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS\UtilitiesCS.csproj" (3:2) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\SVGControl\SVGControl.csproj" (4:2) on node 1 (default targets).
CoreResGen:
  No resources are out of date with respect to their source files. Skipping resource generation.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  SVGControl -> C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\SVGControl\bin\Debug\SVGControl.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\SVGControl\SVGControl.csproj" (default targets).
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
  UtilitiesCS -> C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS\bin\Debug\UtilitiesCS.dll
C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (You can suppress this message by setting the RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.) [C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS\UtilitiesCS.csproj]
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS\UtilitiesCS.csproj" (default targets).
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
  Tags -> C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\Tags\bin\Debug\Tags.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\Tags\Tags.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\ToDoModel\ToDoModel.csproj" (5) on node 1 (default targets).
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
  ToDoModel -> C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\ToDoModel\bin\Debug\ToDoModel.dll
C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (You can suppress this message by setting the RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.) [C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\ToDoModel\ToDoModel.csproj]
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\ToDoModel\ToDoModel.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\ToDoModel.Test\ToDoModel.Test.csproj" (6) on node 1 (default targets).
CopyMSTestV2Resources:
Skipping target "CopyMSTestV2Resources" because it has no outputs.
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
  ToDoModel.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\ToDoModel.Test\ToDoModel.Test.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskVisualization\TaskVisualization.csproj" (7) on node 1 (default targets).
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
  TaskVisualization -> C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskVisualization\bin\Debug\TaskVisualization.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskVisualization\TaskVisualization.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (8) on node 1 (default targets).
CopyMSTestV2Resources:
Skipping target "CopyMSTestV2Resources" because it has no outputs.
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (8) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster\TaskMaster.csproj" (9:2) on node 1 (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster\TaskMaster.csproj" (9:2) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler\QuickFiler.csproj" (10:2) on node 1 (default targets).
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
  QuickFiler -> C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler\bin\Debug\QuickFiler.dll
C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (You can suppress this message by setting the RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.) [C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler\QuickFiler.csproj]
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler\QuickFiler.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster\TaskMaster.csproj" (9:2) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskTree\TaskTree.csproj" (11:2) on node 1 (default targets).
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
  TaskTree -> C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskTree\bin\Debug\TaskTree.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskTree\TaskTree.csproj" (default targets).
CoreResGen:
  No resources are out of date with respect to their source files. Skipping resource generation.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectoryAlways:
  Copying file from "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster\log4net.config" to "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster\bin\Debug\log4net.config".
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  TaskMaster -> C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster\bin\Debug\TaskMaster.dll
C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (You can suppress this message by setting the RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.) [C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster\TaskMaster.csproj]
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster\TaskMaster.csproj" (default targets).
CoreResGen:
  No resources are out of date with respect to their source files. Skipping resource generation.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectoryAlways:
  Copying file from "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster\log4net.config" to "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS.Test\bin\Debug\log4net.config".
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  UtilitiesCS.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (You can suppress this message by setting the RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.) [C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\QuickFiler.Test.csproj" (12) on node 1 (default targets).
CopyMSTestV2Resources:
Skipping target "CopyMSTestV2Resources" because it has no outputs.
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
  QuickFiler.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\QuickFiler.Test.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskVisualization.Test\TaskVisualization.Test.csproj" (13) on node 1 (default targets).
CopyMSTestV2Resources:
Skipping target "CopyMSTestV2Resources" because it has no outputs.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  TaskVisualization.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskVisualization.Test\TaskVisualization.Test.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\Tags.Test\Tags.Test.csproj.metaproj" (14) on node 1 (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\Tags.Test\Tags.Test.csproj.metaproj" (14) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\Tags.Test\Tags.Test.csproj" (15) on node 1 (default targets).
CopyMSTestV2Resources:
Skipping target "CopyMSTestV2Resources" because it has no outputs.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  Tags.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\Tags.Test\bin\Debug\Tags.Test.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\Tags.Test\Tags.Test.csproj" (default targets).
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\Tags.Test\Tags.Test.csproj.metaproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskTree.Test\TaskTree.Test.csproj" (16) on node 1 (default targets).
CopyMSTestV2Resources:
Skipping target "CopyMSTestV2Resources" because it has no outputs.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  TaskTree.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskTree.Test\bin\Debug\TaskTree.Test.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskTree.Test\TaskTree.Test.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\VBFunctions\VBFunctions.csproj" (17) on node 1 (default targets).
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  VBFunctions -> C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\VBFunctions\bin\Debug\VBFunctions.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\VBFunctions\VBFunctions.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\VBFunctions.Test\VBFunctions.Test.csproj" (18) on node 1 (default targets).
CopyMSTestV2Resources:
Skipping target "CopyMSTestV2Resources" because it has no outputs.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  VBFunctions.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\VBFunctions.Test\VBFunctions.Test.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.Test\TaskMaster.Test.csproj" (19) on node 1 (default targets).
CopyMSTestV2Resources:
Skipping target "CopyMSTestV2Resources" because it has no outputs.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectoryAlways:
  Copying file from "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster\log4net.config" to "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.Test\bin\Debug\log4net.config".
  Copying file from "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.Test\log4net.config" to "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.Test\bin\Debug\log4net.config".
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  TaskMaster.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.Test\TaskMaster.Test.csproj" (default targets).
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (Build target(s)).

Build succeeded.

"C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\Tags\Tags.csproj" (default target) (2) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS\UtilitiesCS.csproj" (default target) (3:2) ->
(_RxCheckPackagesConfig target) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (You can suppress this message by setting the RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.) [C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS\UtilitiesCS.csproj]


"C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\ToDoModel\ToDoModel.csproj" (default target) (5) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (You can suppress this message by setting the RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.) [C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\ToDoModel\ToDoModel.csproj]


"C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (default target) (8) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster\TaskMaster.csproj" (default target) (9:2) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler\QuickFiler.csproj" (default target) (10:2) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (You can suppress this message by setting the RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.) [C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler\QuickFiler.csproj]


"C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (default target) (8) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster\TaskMaster.csproj" (default target) (9:2) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (You can suppress this message by setting the RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.) [C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster\TaskMaster.csproj]


"C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (default target) (8) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning : The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later. Please migrate to PackageReference. (You can suppress this message by setting the RxUseUnsupportedPackagesConfig property to true, but be aware this is an unsupported scenario.) [C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\UtilitiesCS.Test\UtilitiesCS.Test.csproj]

    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.47
```
