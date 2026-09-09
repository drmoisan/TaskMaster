# Phase 0 — NuGet restore and analyzer-reference resolution

Timestamp: 2026-09-09T13-48

Task: [P0-T4]

Command: `msbuild TaskMaster.sln /t:Restore /m /p:RestorePackagesConfig=true /p:Configuration=Debug "/p:Platform=Any CPU"`

EXIT_CODE: 0

Restore result: `Installed: 172 package(s) to packages.config projects`, followed by
`Build succeeded.` with `0 Warning(s)` and `0 Error(s)`.

Analyzer item enumeration, resolved against each declaring project's own directory:

RESOLVED: UtilitiesCS/UtilitiesCS.csproj | ..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll | True
RESOLVED: UtilitiesCS/UtilitiesCS.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll | True
RESOLVED: UtilitiesCS/UtilitiesCS.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll | True
RESOLVED: UtilitiesCS/UtilitiesCS.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll | True
RESOLVED: UtilitiesCS/UtilitiesCS.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll | True
RESOLVED: UtilitiesCS/UtilitiesCS.csproj | ..\packages\AsyncFixer.2.1.0\analyzers\dotnet\cs\AsyncFixer.dll | True
RESOLVED: UtilitiesCS/UtilitiesCS.csproj | ..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll | True
RESOLVED: UtilitiesCS/UtilitiesCS.csproj | ..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll | True
RESOLVED: UtilitiesCS/UtilitiesCS.csproj | ..\packages\SonarAnalyzer.CSharp.10.33.0.1635\analyzers\SonarAnalyzer.CSharp.dll | True
RESOLVED: UtilitiesCS.Test/UtilitiesCS.Test.csproj | ..\packages\MSTest.Analyzers.4.4.0\analyzers\dotnet\cs\MSTest.Analyzers.CodeFixes.dll | True
RESOLVED: UtilitiesCS.Test/UtilitiesCS.Test.csproj | ..\packages\MSTest.Analyzers.4.4.0\analyzers\dotnet\cs\MSTest.Analyzers.dll | True
RESOLVED: UtilitiesCS.Test/UtilitiesCS.Test.csproj | ..\packages\SonarAnalyzer.CSharp.10.33.0.1635\analyzers\SonarAnalyzer.CSharp.dll | True
RESOLVED: UtilitiesCS.Test/UtilitiesCS.Test.csproj | ..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll | True
RESOLVED: UtilitiesCS.Test/UtilitiesCS.Test.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll | True
RESOLVED: UtilitiesCS.Test/UtilitiesCS.Test.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll | True
RESOLVED: UtilitiesCS.Test/UtilitiesCS.Test.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll | True
RESOLVED: UtilitiesCS.Test/UtilitiesCS.Test.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll | True
RESOLVED: UtilitiesCS.Test/UtilitiesCS.Test.csproj | ..\packages\AsyncFixer.2.1.0\analyzers\dotnet\cs\AsyncFixer.dll | True
RESOLVED: UtilitiesCS.Test/UtilitiesCS.Test.csproj | ..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll | True
RESOLVED: UtilitiesCS.Test/UtilitiesCS.Test.csproj | ..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll | True
RESOLVED: QuickFiler/QuickFiler.csproj | ..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll | True
RESOLVED: QuickFiler/QuickFiler.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll | True
RESOLVED: QuickFiler/QuickFiler.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll | True
RESOLVED: QuickFiler/QuickFiler.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll | True
RESOLVED: QuickFiler/QuickFiler.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll | True
RESOLVED: QuickFiler/QuickFiler.csproj | ..\packages\AsyncFixer.2.1.0\analyzers\dotnet\cs\AsyncFixer.dll | True
RESOLVED: QuickFiler/QuickFiler.csproj | ..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll | True
RESOLVED: QuickFiler/QuickFiler.csproj | ..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll | True
RESOLVED: QuickFiler/QuickFiler.csproj | ..\packages\SonarAnalyzer.CSharp.10.33.0.1635\analyzers\SonarAnalyzer.CSharp.dll | True
RESOLVED: QuickFiler.Test/QuickFiler.Test.csproj | ..\packages\MSTest.Analyzers.4.4.0\analyzers\dotnet\cs\MSTest.Analyzers.CodeFixes.dll | True
RESOLVED: QuickFiler.Test/QuickFiler.Test.csproj | ..\packages\MSTest.Analyzers.4.4.0\analyzers\dotnet\cs\MSTest.Analyzers.dll | True
RESOLVED: QuickFiler.Test/QuickFiler.Test.csproj | ..\packages\SonarAnalyzer.CSharp.10.33.0.1635\analyzers\SonarAnalyzer.CSharp.dll | True
RESOLVED: QuickFiler.Test/QuickFiler.Test.csproj | ..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll | True
RESOLVED: QuickFiler.Test/QuickFiler.Test.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll | True
RESOLVED: QuickFiler.Test/QuickFiler.Test.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll | True
RESOLVED: QuickFiler.Test/QuickFiler.Test.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll | True
RESOLVED: QuickFiler.Test/QuickFiler.Test.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll | True
RESOLVED: QuickFiler.Test/QuickFiler.Test.csproj | ..\packages\AsyncFixer.2.1.0\analyzers\dotnet\cs\AsyncFixer.dll | True
RESOLVED: QuickFiler.Test/QuickFiler.Test.csproj | ..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll | True
RESOLVED: QuickFiler.Test/QuickFiler.Test.csproj | ..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll | True

UNRESOLVED-COUNT: 0

Output Summary: `packages.config`-aware restore succeeded at exit 0, installing 172 packages with
0 warnings and 0 errors. All 40 `<Analyzer Include>` items declared by the four in-scope projects
resolve to an existing file; no version skew between a declared analyzer path and the restored
`packages/` tree.
