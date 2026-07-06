Timestamp: 2026-07-06T11-18-04:00
Issue: #243
Command: pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild
EXIT_CODE: 1
Output Summary: FAIL. MSBuild launched from `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`, then failed with `35 Error(s)` and `4 Warning(s)`. Primary diagnostics were missing NuGet package build imports for `System.ValueTuple.4.6.2` and `NETStandard.Library.2.0.3`, missing analyzer assemblies such as `Meziantou.Analyzer.dll`, `SonarAnalyzer.CSharp.dll`, and Roslynator analyzers, and missing SVGControl references such as `Fizzler`, `Svg`, and `log4net`. The wrapper ended with `MSBuild failed with exit code 1`.

Primary Diagnostics:
- `ToDoModel\ToDoModel.csproj(190,5): error : This project references NuGet package(s) that are missing on this computer. The missing file is ..\packages\System.ValueTuple.4.6.2\build\net471\System.ValueTuple.targets.`
- `TaskMaster\TaskMaster.csproj(550,5): error : This project references NuGet package(s) that are missing on this computer. The missing file is ..\packages\NETStandard.Library.2.0.3\build\netstandard2.0\NETStandard.Library.targets.`
- `TaskMaster.Test\TaskMaster.Test.csproj(330,5): error : This project references NuGet package(s) that are missing on this computer. The missing file is ..\packages\System.ValueTuple.4.6.2\build\net471\System.ValueTuple.targets.`
- `VBFunctions\VBFunctions.csproj: CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.101\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found.`
- `VBFunctions\VBFunctions.csproj: CSC : error CS0006: Metadata file '..\packages\SonarAnalyzer.CSharp.10.27.0.140913\analyzers\SonarAnalyzer.CSharp.dll' could not be found.`
- `SVGControl\PictureBoxSVG.cs(14,7): error CS0246: The type or namespace name 'Fizzler' could not be found.`
- `SVGControl\PictureBoxSVG.cs(15,7): error CS0246: The type or namespace name 'Svg' could not be found.`
- `SVGControl\SvgRenderer.cs(20,33): error CS0246: The type or namespace name 'log4net' could not be found.`
- `Exception: scripts\vscode\Invoke-VSBuild.ps1:156:5`
- `MSBuild failed with exit code 1`
