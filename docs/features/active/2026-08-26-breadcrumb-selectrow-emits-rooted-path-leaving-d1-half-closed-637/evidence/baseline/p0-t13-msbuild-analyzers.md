Timestamp: 2026-08-31T09-36
Command: pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\\Installer\\vswhere.exe"; $msbuildExe = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\\**\\Bin\\MSBuild.exe" | Select-Object -First 1; & $msbuildExe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; "EXIT_CODE=$LASTEXITCODE"'
EXIT_CODE: 1
NULLABLE_OPT_IN_PROPERTY: absent
Output Summary: Rebuild target executed but failed with 0 warnings and 10 errors because the restored packages directory lacks the analyzer versions referenced by projects.

BASELINE_BUILD_RED:
- VBFunctions.csproj and UtilitiesCS.csproj: CS0006 for ..\\packages\\Meziantou.Analyzer.3.0.156\\analyzers\\dotnet\\roslyn5.0\\cs\\Meziantou.Analyzer.dll.
- VBFunctions.csproj and UtilitiesCS.csproj: CS0006 for ..\\packages\\Roslynator.Analyzers.4.16.0\\analyzers\\dotnet\\roslyn4.7\\cs\\Roslynator.CSharp.Analyzers.dll.
- VBFunctions.csproj and UtilitiesCS.csproj: CS0006 for ..\\packages\\Roslynator.Analyzers.4.16.0\\analyzers\\dotnet\\roslyn4.7\\cs\\Roslynator_Analyzers_Roslynator.Common.dll.
- VBFunctions.csproj and UtilitiesCS.csproj: CS0006 for ..\\packages\\Roslynator.Analyzers.4.16.0\\analyzers\\dotnet\\roslyn4.7\\cs\\Roslynator_Analyzers_Roslynator.Core.dll.
- VBFunctions.csproj and UtilitiesCS.csproj: CS0006 for ..\\packages\\Roslynator.Analyzers.4.16.0\\analyzers\\dotnet\\roslyn4.7\\cs\\Roslynator_Analyzers_Roslynator.CSharp.dll.

Observed restored package directories:
- Meziantou.Analyzer.3.0.174
- Roslynator.Analyzers.4.16.1
