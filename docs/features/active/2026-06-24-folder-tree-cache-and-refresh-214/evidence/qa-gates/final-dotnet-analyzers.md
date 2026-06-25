# Final .NET Analyzer Build

Timestamp: 2026-06-24T19:09:38-04:00
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
Output Summary: PASS. Analyzer build completed.     20 Warning(s)     0 Error(s)

Raw Output Tail:
```
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\Tags.Test\Tags.Test.csproj.metaproj" (16) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\Tags.Test\Tags.Test.csproj" (17) on node 1 (default targets).
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  Tags.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\Tags.Test\bin\Debug\Tags.Test.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\Tags.Test\Tags.Test.csproj" (default targets).
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\Tags.Test\Tags.Test.csproj.metaproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj" (18) on node 1 (default targets).
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  VBFunctions -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\bin\Debug\VBFunctions.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions.Test\VBFunctions.Test.csproj" (19) on node 1 (default targets).
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  VBFunctions.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions.Test\VBFunctions.Test.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.Test\TaskMaster.Test.csproj" (20) on node 1 (default targets).
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectory:
Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
_CopyOutOfDateSourceItemsToOutputDirectoryAlways:
  Copying file from "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster\log4net.config" to "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.Test\bin\Debug\log4net.config".
  Copying file from "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.Test\log4net.config" to "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.Test\bin\Debug\log4net.config".
_CopyAppConfigFile:
Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  TaskMaster.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.Test\TaskMaster.Test.csproj" (default targets).
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (Build target(s)).

Build succeeded.

"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (default target) (9) ->
(CoreCompile target) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\TestHelpers\ManualFireTimerWrapper.cs(24,56): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensions_Tests.cs(1707,37): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensions_Tests.cs(1708,37): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensions_Tests.cs(1738,22): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensions_Tests.cs(1799,41): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensions_Tests.cs(1693,26): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\Threading\ProgressTracker_Tests.cs(374,27): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\Threading\ProgressTracker_Tests.cs(410,27): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\Threading\ProgressTracker_Tests.cs(475,27): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\OutlookObjects\Conversation\ConversationHelper_ExtendedTests.cs(305,65): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\OutlookObjects\Conversation\ConversationHelper_ExtendedTests.cs(405,19): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\OutlookObjects\Conversation\ConversationHelper_ExtendedTests.cs(555,19): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\OutlookObjects\Conversation\ConversationHelper_ExtendedTests.cs(575,19): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\OutlookObjects\Conversation\ConversationHelper_ExtendedTests.cs(587,33): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\OutlookObjects\Conversation\ConversationHelper_ExtendedTests.cs(599,33): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\OutlookObjects\Conversation\ConversationHelper_ExtendedTests.cs(607,33): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensions_Tests.cs(669,35): warning CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\ReusableTypeClasses\SmartSerializable_Tests.cs(824,54): warning CS0067: The event 'SmartSerializable_Tests.TestSmartItem.PropertyChanged' is never used [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs(170,54): warning CS0067: The event 'StoreWrapperControllerTests.OlObjectsStubBase.PropertyChanged' is never used [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\ReusableTypeClasses\SmartSerializableBase_Tests.cs(652,76): warning CS0067: The event 'SmartSerializableBase_Tests.BaseLoaderItem.PropertyChanged' is never used [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj]

    20 Warning(s)
    0 Error(s)

Time Elapsed 00:00:04.13
```
