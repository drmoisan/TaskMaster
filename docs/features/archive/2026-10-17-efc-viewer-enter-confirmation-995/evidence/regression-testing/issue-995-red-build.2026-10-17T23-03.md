# Issue 995 issue-995-red-build

Timestamp: 2026-10-17T23-03
Command: pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath UtilitiesCS.Test/UtilitiesCS.Test.csproj -Platform AnyCPU -Target Build
EXIT_CODE: 0
Output Summary:          Begin writing request for UtilitiesCS.Test
         End writing request for UtilitiesCS.Test
         Begin reading response for UtilitiesCS.Test
         End reading response for UtilitiesCS.Test
         CompilerServer: server - server processed compilation - UtilitiesCS.Test
       _CopyOutOfDateSourceItemsToOutputDirectory:
       Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
       _CopyOutOfDateSourceItemsToOutputDirectoryAlways:
         Copying file from ".\TaskMaster\log4net.config" to ".\UtilitiesCS.Test\bin\Debug\log4net.config".
       _CopyAppConfigFile:
       Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
       CopyFilesToOutputDirectory:
         Copying file from ".\UtilitiesCS.Test\obj\Debug\UtilitiesCS.Test.dll" to ".\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll".
         UtilitiesCS.Test -> .\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
         Copying file from ".\UtilitiesCS.Test\obj\Debug\UtilitiesCS.Test.pdb" to ".\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.pdb".
     1>Done Building Project ".\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (Build target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:05.42
