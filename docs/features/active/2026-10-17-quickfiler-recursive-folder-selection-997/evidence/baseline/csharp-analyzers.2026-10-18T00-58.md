Timestamp: 2026-10-18T00-58
Command: pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -Target Rebuild -EnableNETAnalyzers -EnforceCodeStyleInBuild
EXIT_CODE: 0
ExpectedExitCode: 0
Output Summary:          Copying file from "packages\Microsoft.Web.WebView2.1.0.4258.31\runtimes\win-arm64\native\WebView2Loader.dll" to "UtilitiesCS.Test\bin\Debug\runtimes\win-arm64\native\WebView2Loader.dll".
         Copying file from "packages\Microsoft.Web.WebView2.1.0.4258.31\runtimes\win-x64\native\WebView2Loader.dll" to "UtilitiesCS.Test\bin\Debug\runtimes\win-x64\native\WebView2Loader.dll".
         Copying file from "packages\Tesseract.5.2.0\x64\leptonica-1.82.0.dll" to "UtilitiesCS.Test\bin\Debug\x64\leptonica-1.82.0.dll".
         Copying file from "packages\Microsoft.Web.WebView2.1.0.4258.31\runtimes\win-x86\native\WebView2Loader.dll" to "UtilitiesCS.Test\bin\Debug\runtimes\win-x86\native\WebView2Loader.dll".
         Copying file from "packages\Tesseract.5.2.0\x86\tesseract50.dll" to "UtilitiesCS.Test\bin\Debug\x86\tesseract50.dll".
         Copying file from "packages\Tesseract.5.2.0\x86\leptonica-1.82.0.dll" to "UtilitiesCS.Test\bin\Debug\x86\leptonica-1.82.0.dll".
         Copying file from "packages\Tesseract.5.2.0\x64\tesseract50.dll" to "UtilitiesCS.Test\bin\Debug\x64\tesseract50.dll".
       _CopyOutOfDateSourceItemsToOutputDirectoryAlways:
         Copying file from "TaskMaster\log4net.config" to "UtilitiesCS.Test\bin\Debug\log4net.config".
       _CopyAppConfigFile:
         Copying file from "UtilitiesCS.Test\app.config" to "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll.config".
       CopyFilesToOutputDirectory:
         Copying file from "UtilitiesCS.Test\obj\Debug\UtilitiesCS.Test.dll" to "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll".
         UtilitiesCS.Test -> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
         Creating "UtilitiesCS.Test\obj\Debug\Utilitie.C83D05E3.Up2Date" because "AlwaysCreate" was specified.
         Touching "UtilitiesCS.Test\obj\Debug\Utilitie.C83D05E3.Up2Date".
         Copying file from "UtilitiesCS.Test\obj\Debug\UtilitiesCS.Test.pdb" to "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.pdb".
    12>Done Building Project "UtilitiesCS.Test\UtilitiesCS.Test.csproj" (Rebuild target(s)).
     1>Done Building Project "TaskMaster.sln" (Rebuild target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:16.39


