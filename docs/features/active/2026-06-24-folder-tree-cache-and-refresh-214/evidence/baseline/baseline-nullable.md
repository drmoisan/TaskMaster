Timestamp: 2026-06-24T15-59
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
EXIT_CODE: 1
Output Summary: Nullable baseline build failed. See command output below for diagnostics.

Command Output:
MSBuild version 18.7.8+1ac568fee for .NET Framework
Build started 6/24/2026 3:59:32 PM.

Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" on node 1 (Build target(s)).
ValidateSolutionConfiguration:
  Building solution configuration "Debug|Any CPU".
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\Tags\Tags.csproj" (2) on node 1 (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\Tags\Tags.csproj" (2) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS\UtilitiesCS.csproj" (3:2) on node 1 (default targets).
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS\UtilitiesCS.csproj(1189,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\NETStandard.Library.2.0.3\build\netstandard2.0\NETStandard.Library.targets.
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS\UtilitiesCS.csproj" (default targets) -- FAILED.
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\Tags\Tags.csproj" (2) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesSwordfish\UtilitiesSwordfish.NET.General.csproj" (4:2) on node 1 (default targets).
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files.
CopyFilesToOutputDirectory:
  UtilitiesSwordfish.NET.General -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesSwordfish\bin\Debug\Swordfish.NET.General.dll
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesSwordfish\UtilitiesSwordfish.NET.General.csproj" (default targets).
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\Tags\Tags.csproj" (default targets) -- FAILED.
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\ToDoModel\ToDoModel.csproj" (5) on node 1 (default targets).
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\ToDoModel\ToDoModel.csproj(190,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\System.ValueTuple.4.6.2\build\net471\System.ValueTuple.targets.
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\ToDoModel\ToDoModel.csproj" (default targets) -- FAILED.
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\ToDoModel.Test\ToDoModel.Test.csproj" (6) on node 1 (default targets).
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\ToDoModel.Test\ToDoModel.Test.csproj(317,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\System.ValueTuple.4.6.2\build\net471\System.ValueTuple.targets.
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\ToDoModel.Test\ToDoModel.Test.csproj" (default targets) -- FAILED.
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (8) on node 1 (default targets).
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj(818,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\System.ValueTuple.4.6.2\build\net471\System.ValueTuple.targets.
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (default targets) -- FAILED.
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\QuickFiler\QuickFiler.csproj" (9) on node 1 (default targets).
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\QuickFiler\QuickFiler.csproj(495,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\NETStandard.Library.2.0.3\build\netstandard2.0\NETStandard.Library.targets.
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\QuickFiler\QuickFiler.csproj" (default targets) -- FAILED.
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\QuickFiler.Test\QuickFiler.Test.csproj" (10) on node 1 (default targets).
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\QuickFiler.Test\QuickFiler.Test.csproj(325,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\System.ValueTuple.4.6.2\build\net471\System.ValueTuple.targets.
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\QuickFiler.Test\QuickFiler.Test.csproj" (default targets) -- FAILED.
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskVisualization.Test\TaskVisualization.Test.csproj" (11) on node 1 (default targets).
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskVisualization.Test\TaskVisualization.Test.csproj(287,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\System.ValueTuple.4.6.2\build\net471\System.ValueTuple.targets.
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskVisualization.Test\TaskVisualization.Test.csproj" (default targets) -- FAILED.
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster\TaskMaster.csproj" (13) on node 1 (default targets).
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster\TaskMaster.csproj(540,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\NETStandard.Library.2.0.3\build\netstandard2.0\NETStandard.Library.targets.
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster\TaskMaster.csproj" (default targets) -- FAILED.
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesSwordfish.Test\UtilitiesSwordfish.NET.Test.csproj" (14) on node 1 (default targets).
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
  UtilitiesSwordfish.NET.Test -> C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesSwordfish.Test\bin\Debug\Swordfish.NET.Test.exe
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesSwordfish.Test\UtilitiesSwordfish.NET.Test.csproj" (default targets).
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj" (16) on node 1 (default targets).
ResolveAssemblyReferences:
  Primary reference "ExCSS, Version=4.3.1.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a, processorArchitecture=MSIL".
C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Microsoft.Common.CurrentVersion.targets(2453,5): warning MSB3245: Could not resolve this reference. Could not locate the assembly "ExCSS, Version=4.3.1.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a, processorArchitecture=MSIL". Check to make sure the assembly exists on disk. If this reference is required by your code, you may get compilation errors. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
          For SearchPath "{HintPathFromItem}".
          Considered "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\packages\ExCSS.4.3.1\lib\net48\ExCSS.dll", but it didn't exist.
          For SearchPath "{TargetFrameworkDirectory}".
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\ExCSS.winmd", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\ExCSS.dll", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\ExCSS.exe", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Facades\ExCSS.winmd", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Facades\ExCSS.dll", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Facades\ExCSS.exe", but it didn't exist.
          For SearchPath "{AssemblyFoldersFromConfig:C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\AssemblyFolders.config,v4.8.1}".
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\SqlDb\ExCSS.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\SqlDb\ExCSS.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\SqlDb\ExCSS.exe", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\ExCSS.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\ExCSS.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\ExCSS.exe", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.5\ExCSS.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.5\ExCSS.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.5\ExCSS.exe", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.0\ExCSS.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.0\ExCSS.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.0\ExCSS.exe", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v2.0\ExCSS.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v2.0\ExCSS.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v2.0\ExCSS.exe", but it didn't exist.
          For SearchPath "{Registry:Software\Microsoft\.NETFramework,v4.8.1,AssemblyFoldersEx}".
          Considered AssemblyFoldersEx locations.
          For SearchPath "{AssemblyFolders}".
          Considered "C:\Program Files\IIS\Microsoft Web Deploy V3\ExCSS.winmd", but it didn't exist.
          Considered "C:\Program Files\IIS\Microsoft Web Deploy V3\ExCSS.dll", but it didn't exist.
          Considered "C:\Program Files\IIS\Microsoft Web Deploy V3\ExCSS.exe", but it didn't exist.
          For SearchPath "{GAC}".
          Considered "ExCSS, Version=4.3.1.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a, processorArchitecture=MSIL", which was not found in the GAC.
          For SearchPath "{RawFileName}".
          Considered treating "ExCSS, Version=4.3.1.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a, processorArchitecture=MSIL" as a file name, but it didn't exist.
          For SearchPath "bin\Debug\".
          Considered "bin\Debug\ExCSS.winmd", but it didn't exist.
          Considered "bin\Debug\ExCSS.dll", but it didn't exist.
          Considered "bin\Debug\ExCSS.exe", but it didn't exist.
  Primary reference "Fizzler, Version=1.3.1.0, Culture=neutral, PublicKeyToken=4ebff4844e382110, processorArchitecture=MSIL".
C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Microsoft.Common.CurrentVersion.targets(2453,5): warning MSB3245: Could not resolve this reference. Could not locate the assembly "Fizzler, Version=1.3.1.0, Culture=neutral, PublicKeyToken=4ebff4844e382110, processorArchitecture=MSIL". Check to make sure the assembly exists on disk. If this reference is required by your code, you may get compilation errors. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
          For SearchPath "{HintPathFromItem}".
          Considered "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\packages\Fizzler.1.3.1\lib\netstandard2.0\Fizzler.dll", but it didn't exist.
          For SearchPath "{TargetFrameworkDirectory}".
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Fizzler.winmd", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Fizzler.dll", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Fizzler.exe", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Facades\Fizzler.winmd", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Facades\Fizzler.dll", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Facades\Fizzler.exe", but it didn't exist.
          For SearchPath "{AssemblyFoldersFromConfig:C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\AssemblyFolders.config,v4.8.1}".
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\SqlDb\Fizzler.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\SqlDb\Fizzler.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\SqlDb\Fizzler.exe", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\Fizzler.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\Fizzler.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\Fizzler.exe", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.5\Fizzler.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.5\Fizzler.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.5\Fizzler.exe", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.0\Fizzler.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.0\Fizzler.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.0\Fizzler.exe", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v2.0\Fizzler.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v2.0\Fizzler.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v2.0\Fizzler.exe", but it didn't exist.
          For SearchPath "{Registry:Software\Microsoft\.NETFramework,v4.8.1,AssemblyFoldersEx}".
          Considered AssemblyFoldersEx locations.
          For SearchPath "{AssemblyFolders}".
          Considered "C:\Program Files\IIS\Microsoft Web Deploy V3\Fizzler.winmd", but it didn't exist.
          Considered "C:\Program Files\IIS\Microsoft Web Deploy V3\Fizzler.dll", but it didn't exist.
          Considered "C:\Program Files\IIS\Microsoft Web Deploy V3\Fizzler.exe", but it didn't exist.
          For SearchPath "{GAC}".
          Considered "Fizzler, Version=1.3.1.0, Culture=neutral, PublicKeyToken=4ebff4844e382110, processorArchitecture=MSIL", which was not found in the GAC.
          For SearchPath "{RawFileName}".
          Considered treating "Fizzler, Version=1.3.1.0, Culture=neutral, PublicKeyToken=4ebff4844e382110, processorArchitecture=MSIL" as a file name, but it didn't exist.
          For SearchPath "bin\Debug\".
          Considered "bin\Debug\Fizzler.winmd", but it didn't exist.
          Considered "bin\Debug\Fizzler.dll", but it didn't exist.
          Considered "bin\Debug\Fizzler.exe", but it didn't exist.
  Primary reference "log4net, Version=3.3.1.0, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a, processorArchitecture=MSIL".
C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Microsoft.Common.CurrentVersion.targets(2453,5): warning MSB3245: Could not resolve this reference. Could not locate the assembly "log4net, Version=3.3.1.0, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a, processorArchitecture=MSIL". Check to make sure the assembly exists on disk. If this reference is required by your code, you may get compilation errors. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
          For SearchPath "{HintPathFromItem}".
          Considered "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\packages\log4net.3.3.1\lib\net462\log4net.dll", but it didn't exist.
          For SearchPath "{TargetFrameworkDirectory}".
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\log4net.winmd", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\log4net.dll", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\log4net.exe", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Facades\log4net.winmd", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Facades\log4net.dll", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Facades\log4net.exe", but it didn't exist.
          For SearchPath "{AssemblyFoldersFromConfig:C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\AssemblyFolders.config,v4.8.1}".
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\SqlDb\log4net.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\SqlDb\log4net.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\SqlDb\log4net.exe", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\log4net.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\log4net.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\log4net.exe", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.5\log4net.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.5\log4net.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.5\log4net.exe", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.0\log4net.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.0\log4net.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.0\log4net.exe", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v2.0\log4net.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v2.0\log4net.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v2.0\log4net.exe", but it didn't exist.
          For SearchPath "{Registry:Software\Microsoft\.NETFramework,v4.8.1,AssemblyFoldersEx}".
          Considered AssemblyFoldersEx locations.
          For SearchPath "{AssemblyFolders}".
          Considered "C:\Program Files\IIS\Microsoft Web Deploy V3\log4net.winmd", but it didn't exist.
          Considered "C:\Program Files\IIS\Microsoft Web Deploy V3\log4net.dll", but it didn't exist.
          Considered "C:\Program Files\IIS\Microsoft Web Deploy V3\log4net.exe", but it didn't exist.
          For SearchPath "{GAC}".
          Considered "log4net, Version=3.3.1.0, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a, processorArchitecture=MSIL", which was not found in the GAC.
          For SearchPath "{RawFileName}".
          Considered treating "log4net, Version=3.3.1.0, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a, processorArchitecture=MSIL" as a file name, but it didn't exist.
          For SearchPath "bin\Debug\".
          Considered "bin\Debug\log4net.winmd", but it didn't exist.
          Considered "bin\Debug\log4net.dll", but it didn't exist.
          Considered "bin\Debug\log4net.exe", but it didn't exist.
  Primary reference "Svg, Version=3.4.0.0, Culture=neutral, PublicKeyToken=12a0bac221edeae2, processorArchitecture=MSIL".
C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Microsoft.Common.CurrentVersion.targets(2453,5): warning MSB3245: Could not resolve this reference. Could not locate the assembly "Svg, Version=3.4.0.0, Culture=neutral, PublicKeyToken=12a0bac221edeae2, processorArchitecture=MSIL". Check to make sure the assembly exists on disk. If this reference is required by your code, you may get compilation errors. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
          For SearchPath "{HintPathFromItem}".
          Considered "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\packages\Svg.3.4.7\lib\net481\Svg.dll", but it didn't exist.
          For SearchPath "{TargetFrameworkDirectory}".
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Svg.winmd", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Svg.dll", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Svg.exe", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Facades\Svg.winmd", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Facades\Svg.dll", but it didn't exist.
          Considered "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Facades\Svg.exe", but it didn't exist.
          For SearchPath "{AssemblyFoldersFromConfig:C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\AssemblyFolders.config,v4.8.1}".
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\SqlDb\Svg.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\SqlDb\Svg.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\SqlDb\Svg.exe", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\Svg.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\Svg.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\Svg.exe", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.5\Svg.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.5\Svg.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.5\Svg.exe", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.0\Svg.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.0\Svg.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v4.0\Svg.exe", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v2.0\Svg.winmd", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v2.0\Svg.dll", but it didn't exist.
          Considered "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\ReferenceAssemblies\v2.0\Svg.exe", but it didn't exist.
          For SearchPath "{Registry:Software\Microsoft\.NETFramework,v4.8.1,AssemblyFoldersEx}".
          Considered AssemblyFoldersEx locations.
          For SearchPath "{AssemblyFolders}".
          Considered "C:\Program Files\IIS\Microsoft Web Deploy V3\Svg.winmd", but it didn't exist.
          Considered "C:\Program Files\IIS\Microsoft Web Deploy V3\Svg.dll", but it didn't exist.
          Considered "C:\Program Files\IIS\Microsoft Web Deploy V3\Svg.exe", but it didn't exist.
          For SearchPath "{GAC}".
          Considered "Svg, Version=3.4.0.0, Culture=neutral, PublicKeyToken=12a0bac221edeae2, processorArchitecture=MSIL", which was not found in the GAC.
          For SearchPath "{RawFileName}".
          Considered treating "Svg, Version=3.4.0.0, Culture=neutral, PublicKeyToken=12a0bac221edeae2, processorArchitecture=MSIL" as a file name, but it didn't exist.
          For SearchPath "bin\Debug\".
          Considered "bin\Debug\Svg.winmd", but it didn't exist.
          Considered "bin\Debug\Svg.dll", but it didn't exist.
          Considered "bin\Debug\Svg.exe", but it didn't exist.
CoreResGen:
  No resources are out of date with respect to their source files. Skipping resource generation.
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
  Setting DOTNET_TieredCompilation to '0'
  C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Roslyn\csc.exe /noconfig /unsafe+ /nowarn:1701,1702 /fullpaths /nostdlib+ /errorreport:prompt /warn:4 /define:DEBUG;TRACE /highentropyva+ /nullable:enable /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Microsoft.CSharp.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\mscorlib.dll" /reference:"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\System.Buffers.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Configuration.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Core.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Data.DataSetExtensions.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Data.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Design.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Drawing.dll" /reference:"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\System.Memory.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Net.Http.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Numerics.dll" /reference:"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\System.Numerics.Vectors.dll" /reference:"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\System.Runtime.CompilerServices.Unsafe.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Web.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Windows.Forms.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Xml.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Xml.Linq.dll" /debug+ /debug:full /filealign:512 /optimize- /out:obj\Debug\SVGControl.dll /subsystemversion:6.00 /resource:obj\Debug\SVGControl.ButtonSVG.resources /resource:obj\Debug\SVGControl.PictureBoxSVG.resources /resource:obj\Debug\SVGControl.Properties.Resources.resources /target:library /warnaserror+ /utf8output /deterministic+ /langversion:latest /analyzerconfig:C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\.editorconfig ButtonSVG.cs ButtonSVG.Designer.cs DropDownEditor.cs ISvgResource.cs PictureBoxSVG.cs PictureBoxSVG.Designer.cs SvgOptionsConverter.cs SvgOptionsConverter2.cs SvgRenderer.cs SvgResourceConverter.cs ToggleSwitch.cs ToggleSwitch.Designer.cs PathInternal.cs RelativePath.cs SvgImageSelector.cs Properties\Resources.Designer.cs Properties\AssemblyInfo.cs SvgFileNameEditor.cs SVGParser.cs ValueStringBuilder.cs "obj\Debug\.NETFramework,Version=v4.8.1.AssemblyAttributes.cs"
  Compilation request SVGControl, PathToTool=C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Roslyn\csc.exe
  CommandLine = ' /noconfig'
  BuildResponseFile = '/unsafe+ /nowarn:1701,1702 /fullpaths /nostdlib+ /errorreport:prompt /warn:4 /define:DEBUG;TRACE /highentropyva+ /nullable:enable /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Microsoft.CSharp.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\mscorlib.dll" /reference:"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\System.Buffers.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Configuration.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Core.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Data.DataSetExtensions.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Data.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Design.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Drawing.dll" /reference:"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\System.Memory.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Net.Http.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Numerics.dll" /reference:"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\System.Numerics.Vectors.dll" /reference:"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\PublicAssemblies\System.Runtime.CompilerServices.Unsafe.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Web.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Windows.Forms.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Xml.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Xml.Linq.dll" /debug+ /debug:full /filealign:512 /optimize- /out:obj\Debug\SVGControl.dll /subsystemversion:6.00 /resource:obj\Debug\SVGControl.ButtonSVG.resources /resource:obj\Debug\SVGControl.PictureBoxSVG.resources /resource:obj\Debug\SVGControl.Properties.Resources.resources /target:library /warnaserror+ /utf8output /deterministic+ /langversion:latest /analyzerconfig:C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\.editorconfig ButtonSVG.cs ButtonSVG.Designer.cs DropDownEditor.cs ISvgResource.cs PictureBoxSVG.cs PictureBoxSVG.Designer.cs SvgOptionsConverter.cs SvgOptionsConverter2.cs SvgRenderer.cs SvgResourceConverter.cs ToggleSwitch.cs ToggleSwitch.Designer.cs PathInternal.cs RelativePath.cs SvgImageSelector.cs Properties\Resources.Designer.cs Properties\AssemblyInfo.cs SvgFileNameEditor.cs SVGParser.cs ValueStringBuilder.cs "obj\Debug\.NETFramework,Version=v4.8.1.AssemblyAttributes.cs"'
  Attempt to open named pipe 'hyXX0rP9EK4Emx2hQnwRjEA1zoT6k+IS4SoSWPf3Jdw'
  Attempt to connect named pipe 'hyXX0rP9EK4Emx2hQnwRjEA1zoT6k+IS4SoSWPf3Jdw'
  Named pipe 'hyXX0rP9EK4Emx2hQnwRjEA1zoT6k+IS4SoSWPf3Jdw' connected
  Begin writing request for SVGControl
  End writing request for SVGControl
  Begin reading response for SVGControl
  End reading response for SVGControl
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\PictureBoxSVG.cs(14,7): error CS0246: The type or namespace name 'Fizzler' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\PictureBoxSVG.cs(15,7): error CS0246: The type or namespace name 'Svg' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgRenderer.cs(14,7): error CS0246: The type or namespace name 'Svg' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgImageSelector.cs(12,7): error CS0246: The type or namespace name 'Svg' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGParser.cs(9,7): error CS0246: The type or namespace name 'Svg' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGParser.cs(66,16): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGParser.cs(72,16): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgRenderer.cs(218,16): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGParser.cs(84,40): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGParser.cs(84,17): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgImageSelector.cs(26,33): error CS0246: The type or namespace name 'log4net' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgRenderer.cs(320,23): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgRenderer.cs(144,28): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgRenderer.cs(153,28): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgRenderer.cs(20,33): error CS0246: The type or namespace name 'log4net' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgRenderer.cs(174,17): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  CompilerServer: server - server processed compilation - SVGControl
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj" (default targets) -- FAILED.
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj" (17) on node 1 (default targets).
GenerateTargetFrameworkMonikerAttribute:
Skipping target "GenerateTargetFrameworkMonikerAttribute" because all output files are up-to-date with respect to the input files.
CoreCompile:
  Setting DOTNET_TieredCompilation to '0'
  C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Roslyn\csc.exe /noconfig /nowarn:1701,1702 /fullpaths /nostdlib+ /errorreport:prompt /warn:4 /define:DEBUG;TRACE /highentropyva+ /nullable:enable /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Microsoft.CSharp.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Microsoft.VisualBasic.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\mscorlib.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Core.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Data.DataSetExtensions.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Data.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Net.Http.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Xml.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Xml.Linq.dll" /debug+ /debug:full /filealign:512 /optimize- /out:obj\Debug\VBFunctions.dll /subsystemversion:6.00 /target:library /warnaserror+ /utf8output /deterministic+ /langversion:latest /analyzerconfig:C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\.editorconfig /analyzer:..\packages\Meziantou.Analyzer.3.0.101\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll /analyzer:..\packages\SonarAnalyzer.CSharp.10.27.0.140913\analyzers\SonarAnalyzer.CSharp.dll /analyzer:..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll /analyzer:..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll /analyzer:..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll /analyzer:..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll /analyzer:..\packages\AsyncFixer.2.1.0\analyzers\dotnet\cs\AsyncFixer.dll /analyzer:..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4\analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll /analyzer:..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll /additionalfile:C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\..\BannedSymbols.txt ComputerInfo.cs Properties\AssemblyInfo.cs "obj\Debug\.NETFramework,Version=v4.8.1.AssemblyAttributes.cs"
  Compilation request VBFunctions, PathToTool=C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Roslyn\csc.exe
  CommandLine = ' /noconfig'
  BuildResponseFile = '/nowarn:1701,1702 /fullpaths /nostdlib+ /errorreport:prompt /warn:4 /define:DEBUG;TRACE /highentropyva+ /nullable:enable /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Microsoft.CSharp.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\Microsoft.VisualBasic.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\mscorlib.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Core.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Data.DataSetExtensions.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Data.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Net.Http.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Xml.dll" /reference:"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8.1\System.Xml.Linq.dll" /debug+ /debug:full /filealign:512 /optimize- /out:obj\Debug\VBFunctions.dll /subsystemversion:6.00 /target:library /warnaserror+ /utf8output /deterministic+ /langversion:latest /analyzerconfig:C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\.editorconfig /analyzer:..\packages\Meziantou.Analyzer.3.0.101\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll /analyzer:..\packages\SonarAnalyzer.CSharp.10.27.0.140913\analyzers\SonarAnalyzer.CSharp.dll /analyzer:..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll /analyzer:..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll /analyzer:..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll /analyzer:..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll /analyzer:..\packages\AsyncFixer.2.1.0\analyzers\dotnet\cs\AsyncFixer.dll /analyzer:..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4\analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll /analyzer:..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll /additionalfile:C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\..\BannedSymbols.txt ComputerInfo.cs Properties\AssemblyInfo.cs "obj\Debug\.NETFramework,Version=v4.8.1.AssemblyAttributes.cs"'
  Attempt to open named pipe 'hyXX0rP9EK4Emx2hQnwRjEA1zoT6k+IS4SoSWPf3Jdw'
  Attempt to connect named pipe 'hyXX0rP9EK4Emx2hQnwRjEA1zoT6k+IS4SoSWPf3Jdw'
  Named pipe 'hyXX0rP9EK4Emx2hQnwRjEA1zoT6k+IS4SoSWPf3Jdw' connected
  Begin writing request for VBFunctions
  End writing request for VBFunctions
  Begin reading response for VBFunctions
  End reading response for VBFunctions
CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.101\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]
CSC : error CS0006: Metadata file '..\packages\SonarAnalyzer.CSharp.10.27.0.140913\analyzers\SonarAnalyzer.CSharp.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]
CSC : error CS0006: Metadata file '..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]
CSC : error CS0006: Metadata file '..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]
CSC : error CS0006: Metadata file '..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]
CSC : error CS0006: Metadata file '..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]
CSC : error CS0006: Metadata file '..\packages\AsyncFixer.2.1.0\analyzers\dotnet\cs\AsyncFixer.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]
CSC : error CS0006: Metadata file '..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4\analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]
CSC : error CS0006: Metadata file '..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]
  CompilerServer: server - server processed compilation - VBFunctions
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj" (default targets) -- FAILED.
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions.Test\VBFunctions.Test.csproj" (18) on node 1 (default targets).
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions.Test\VBFunctions.Test.csproj(263,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\System.ValueTuple.4.6.2\build\net471\System.ValueTuple.targets.
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions.Test\VBFunctions.Test.csproj" (default targets) -- FAILED.
Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (1) is building "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.Test\TaskMaster.Test.csproj" (19) on node 1 (default targets).
C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.Test\TaskMaster.Test.csproj(315,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\System.ValueTuple.4.6.2\build\net471\System.ValueTuple.targets.
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.Test\TaskMaster.Test.csproj" (default targets) -- FAILED.
Done Building Project "C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (Build target(s)) -- FAILED.

Build FAILED.

"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj" (default target) (16) ->
(ResolveAssemblyReferences target) ->
  C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Microsoft.Common.CurrentVersion.targets(2453,5): warning MSB3245: Could not resolve this reference. Could not locate the assembly "ExCSS, Version=4.3.1.0, Culture=neutral, PublicKeyToken=bdbe16be9b936b9a, processorArchitecture=MSIL". Check to make sure the assembly exists on disk. If this reference is required by your code, you may get compilation errors. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Microsoft.Common.CurrentVersion.targets(2453,5): warning MSB3245: Could not resolve this reference. Could not locate the assembly "Fizzler, Version=1.3.1.0, Culture=neutral, PublicKeyToken=4ebff4844e382110, processorArchitecture=MSIL". Check to make sure the assembly exists on disk. If this reference is required by your code, you may get compilation errors. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Microsoft.Common.CurrentVersion.targets(2453,5): warning MSB3245: Could not resolve this reference. Could not locate the assembly "log4net, Version=3.3.1.0, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a, processorArchitecture=MSIL". Check to make sure the assembly exists on disk. If this reference is required by your code, you may get compilation errors. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\Microsoft.Common.CurrentVersion.targets(2453,5): warning MSB3245: Could not resolve this reference. Could not locate the assembly "Svg, Version=3.4.0.0, Culture=neutral, PublicKeyToken=12a0bac221edeae2, processorArchitecture=MSIL". Check to make sure the assembly exists on disk. If this reference is required by your code, you may get compilation errors. [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]


"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\Tags\Tags.csproj" (default target) (2) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS\UtilitiesCS.csproj" (default target) (3:2) ->
(EnsureNuGetPackageBuildImports target) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS\UtilitiesCS.csproj(1189,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\NETStandard.Library.2.0.3\build\netstandard2.0\NETStandard.Library.targets.


"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\ToDoModel\ToDoModel.csproj" (default target) (5) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\ToDoModel\ToDoModel.csproj(190,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\System.ValueTuple.4.6.2\build\net471\System.ValueTuple.targets.


"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\ToDoModel.Test\ToDoModel.Test.csproj" (default target) (6) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\ToDoModel.Test\ToDoModel.Test.csproj(317,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\System.ValueTuple.4.6.2\build\net471\System.ValueTuple.targets.


"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (default target) (8) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\UtilitiesCS.Test\UtilitiesCS.Test.csproj(818,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\System.ValueTuple.4.6.2\build\net471\System.ValueTuple.targets.


"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\QuickFiler\QuickFiler.csproj" (default target) (9) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\QuickFiler\QuickFiler.csproj(495,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\NETStandard.Library.2.0.3\build\netstandard2.0\NETStandard.Library.targets.


"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\QuickFiler.Test\QuickFiler.Test.csproj" (default target) (10) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\QuickFiler.Test\QuickFiler.Test.csproj(325,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\System.ValueTuple.4.6.2\build\net471\System.ValueTuple.targets.


"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskVisualization.Test\TaskVisualization.Test.csproj" (default target) (11) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskVisualization.Test\TaskVisualization.Test.csproj(287,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\System.ValueTuple.4.6.2\build\net471\System.ValueTuple.targets.


"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster\TaskMaster.csproj" (default target) (13) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster\TaskMaster.csproj(540,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\NETStandard.Library.2.0.3\build\netstandard2.0\NETStandard.Library.targets.


"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj" (default target) (16) ->
(CoreCompile target) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\PictureBoxSVG.cs(14,7): error CS0246: The type or namespace name 'Fizzler' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\PictureBoxSVG.cs(15,7): error CS0246: The type or namespace name 'Svg' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgRenderer.cs(14,7): error CS0246: The type or namespace name 'Svg' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgImageSelector.cs(12,7): error CS0246: The type or namespace name 'Svg' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGParser.cs(9,7): error CS0246: The type or namespace name 'Svg' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGParser.cs(66,16): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGParser.cs(72,16): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgRenderer.cs(218,16): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGParser.cs(84,40): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGParser.cs(84,17): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgImageSelector.cs(26,33): error CS0246: The type or namespace name 'log4net' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgRenderer.cs(320,23): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgRenderer.cs(144,28): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgRenderer.cs(153,28): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgRenderer.cs(20,33): error CS0246: The type or namespace name 'log4net' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SvgRenderer.cs(174,17): error CS0246: The type or namespace name 'SvgDocument' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\SVGControl\SVGControl.csproj]


"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj" (default target) (17) ->
  CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.101\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]
  CSC : error CS0006: Metadata file '..\packages\SonarAnalyzer.CSharp.10.27.0.140913\analyzers\SonarAnalyzer.CSharp.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]
  CSC : error CS0006: Metadata file '..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]
  CSC : error CS0006: Metadata file '..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]
  CSC : error CS0006: Metadata file '..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]
  CSC : error CS0006: Metadata file '..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]
  CSC : error CS0006: Metadata file '..\packages\AsyncFixer.2.1.0\analyzers\dotnet\cs\AsyncFixer.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]
  CSC : error CS0006: Metadata file '..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4\analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]
  CSC : error CS0006: Metadata file '..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll' could not be found [C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions\VBFunctions.csproj]


"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions.Test\VBFunctions.Test.csproj" (default target) (18) ->
(EnsureNuGetPackageBuildImports target) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\VBFunctions.Test\VBFunctions.Test.csproj(263,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\System.ValueTuple.4.6.2\build\net471\System.ValueTuple.targets.


"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.sln" (Build target) (1) ->
"C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.Test\TaskMaster.Test.csproj" (default target) (19) ->
  C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-24-14-52\TaskMaster.Test\TaskMaster.Test.csproj(315,5): error : This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is ..\packages\System.ValueTuple.4.6.2\build\net471\System.ValueTuple.targets.

    4 Warning(s)
    35 Error(s)

Time Elapsed 00:00:00.44
