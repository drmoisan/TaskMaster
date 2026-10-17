# Issue 995 issue-995-targeted-build

Timestamp: 2026-10-17T23-05
Command: pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build -EnableNETAnalyzers -EnforceCodeStyleInBuild -TreatWarningsAsErrors
EXIT_CODE: 0
Output Summary:          Copying file from ".\TaskMaster\bin\Debug\TaskTree.pdb" to ".\UtilitiesCS.Test\bin\Debug\TaskTree.pdb".
         Copying file from ".\TaskMaster\bin\Debug\TaskVisualization.pdb" to ".\UtilitiesCS.Test\bin\Debug\TaskVisualization.pdb".
         Copying file from ".\TaskMaster\bin\Debug\Tags.pdb" to ".\UtilitiesCS.Test\bin\Debug\Tags.pdb".
       _CopyOutOfDateSourceItemsToOutputDirectory:
       Skipping target "_CopyOutOfDateSourceItemsToOutputDirectory" because all output files are up-to-date with respect to the input files.
       _CopyOutOfDateSourceItemsToOutputDirectoryAlways:
         Copying file from ".\TaskMaster\log4net.config" to ".\UtilitiesCS.Test\bin\Debug\log4net.config".
       _CopyAppConfigFile:
       Skipping target "_CopyAppConfigFile" because all output files are up-to-date with respect to the input files.
       CopyFilesToOutputDirectory:
         Copying file from ".\UtilitiesCS.Test\obj\Debug\UtilitiesCS.Test.dll" to ".\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll".
         UtilitiesCS.Test -> .\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
         Touching ".\UtilitiesCS.Test\obj\Debug\Utilitie.C83D05E3.Up2Date".
         Copying file from ".\UtilitiesCS.Test\obj\Debug\UtilitiesCS.Test.pdb" to ".\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.pdb".
    16>Done Building Project ".\UtilitiesCS.Test\UtilitiesCS.Test.csproj" (default targets).
     1>Done Building Project ".\TaskMaster.sln" (Build target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:11.61
