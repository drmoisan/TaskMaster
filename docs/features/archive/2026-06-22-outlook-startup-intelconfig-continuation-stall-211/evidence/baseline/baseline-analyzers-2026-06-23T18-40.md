# Baseline — .NET Analyzers (#211 Phase 3.1)

Timestamp: 2026-06-23T18-40
Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
(msbuild: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`; run with `-m -v:m`)
EXIT_CODE: 0

Output Summary:
- Build succeeded. All 19 projects compiled (VBFunctions, UtilitiesSwordfish.NET.General/Test, SVGControl, UtilitiesCS, Tags, ToDoModel, TaskTree, TaskVisualization, QuickFiler, TaskMaster, and the *.Test assemblies).
- No analyzer errors. Analyzer-stack diagnostics (Meziantou, SonarAnalyzer.CSharp, Roslynator, AsyncFixer, BannedApiAnalyzers) are configured at `suggestion` severity per `.editorconfig` and do not break the analyzer build. This is the analyzer baseline; the Phase 5 final-QC analyzer run is compared against it for "no new diagnostics".
