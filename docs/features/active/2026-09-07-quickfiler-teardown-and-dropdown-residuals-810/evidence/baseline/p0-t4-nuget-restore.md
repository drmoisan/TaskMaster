# [P0-T4] packages.config-Aware NuGet Restore and Analyzer Resolution

Timestamp: 2026-09-08T09-14
Command: `msbuild TaskMaster.sln /t:Restore /m /p:RestorePackagesConfig=true /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0
Output Summary: The restore installed 172 packages to the `packages.config` projects and reported `Build succeeded.` with 0 warnings and 0 errors. All 20 `<Analyzer Include>` items declared by the two QuickFiler projects resolve to files that exist on disk.

## Restore tail

```
    Installed:
        172 package(s) to packages.config projects
1>Done Building Project "<worktree>\TaskMaster.sln" (Restore target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:03.14
```

The absolute solution path printed by MSBuild is reduced to `<worktree>` per D14.

## Analyzer resolution

Each `Include` value is resolved against the declaring project's own directory and tested with `System.IO.File.Exists`.

RESOLVED: QuickFiler\QuickFiler.csproj | ..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll | True
RESOLVED: QuickFiler\QuickFiler.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll | True
RESOLVED: QuickFiler\QuickFiler.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll | True
RESOLVED: QuickFiler\QuickFiler.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll | True
RESOLVED: QuickFiler\QuickFiler.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll | True
RESOLVED: QuickFiler\QuickFiler.csproj | ..\packages\AsyncFixer.2.1.0\analyzers\dotnet\cs\AsyncFixer.dll | True
RESOLVED: QuickFiler\QuickFiler.csproj | ..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll | True
RESOLVED: QuickFiler\QuickFiler.csproj | ..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll | True
RESOLVED: QuickFiler\QuickFiler.csproj | ..\packages\SonarAnalyzer.CSharp.10.33.0.1635\analyzers\SonarAnalyzer.CSharp.dll | True
RESOLVED: QuickFiler.Test\QuickFiler.Test.csproj | ..\packages\MSTest.Analyzers.4.4.0\analyzers\dotnet\cs\MSTest.Analyzers.CodeFixes.dll | True
RESOLVED: QuickFiler.Test\QuickFiler.Test.csproj | ..\packages\MSTest.Analyzers.4.4.0\analyzers\dotnet\cs\MSTest.Analyzers.dll | True
RESOLVED: QuickFiler.Test\QuickFiler.Test.csproj | ..\packages\SonarAnalyzer.CSharp.10.33.0.1635\analyzers\SonarAnalyzer.CSharp.dll | True
RESOLVED: QuickFiler.Test\QuickFiler.Test.csproj | ..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll | True
RESOLVED: QuickFiler.Test\QuickFiler.Test.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll | True
RESOLVED: QuickFiler.Test\QuickFiler.Test.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll | True
RESOLVED: QuickFiler.Test\QuickFiler.Test.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll | True
RESOLVED: QuickFiler.Test\QuickFiler.Test.csproj | ..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll | True
RESOLVED: QuickFiler.Test\QuickFiler.Test.csproj | ..\packages\AsyncFixer.2.1.0\analyzers\dotnet\cs\AsyncFixer.dll | True
RESOLVED: QuickFiler.Test\QuickFiler.Test.csproj | ..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll | True
RESOLVED: QuickFiler.Test\QuickFiler.Test.csproj | ..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll | True

UNRESOLVED-COUNT: 0

Nine items are declared by `QuickFiler/QuickFiler.csproj` and eleven by `QuickFiler.Test/QuickFiler.Test.csproj`, for twenty in total. Every one exists.
