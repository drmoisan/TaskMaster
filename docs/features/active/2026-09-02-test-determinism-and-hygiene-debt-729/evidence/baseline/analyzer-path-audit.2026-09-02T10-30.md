# Analyzer path audit (P0-T6)

Timestamp: 2026-09-03T01-11

Command: enumerate every `Analyzer` `Include` value from every non-`packages` `*.csproj` in the
workspace, resolve each value against the declaring project's own directory, and `Test-Path` each
resolved path.

EXIT_CODE: 0

Projects scanned: 18
Analyzer Include entries: 162
Resolved: 162
Unresolved: 0

## Distinct resolved analyzer paths (repository-relative)

| Resolved | Path |
|---|---|
| True | packages\AsyncFixer.2.1.0\analyzers\dotnet\cs\AsyncFixer.dll |
| True | packages\Meziantou.Analyzer.3.0.194\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll |
| True | packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll |
| True | packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll |
| True | packages\MSTest.Analyzers.4.3.3\analyzers\dotnet\cs\MSTest.Analyzers.CodeFixes.dll |
| True | packages\MSTest.Analyzers.4.3.3\analyzers\dotnet\cs\MSTest.Analyzers.dll |
| True | packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll |
| True | packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll |
| True | packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll |
| True | packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll |
| True | packages\SonarAnalyzer.CSharp.10.33.0.1635\analyzers\SonarAnalyzer.CSharp.dll |

Package identifiers backing those paths: `AsyncFixer` 2.1.0, `Meziantou.Analyzer` 3.0.194,
`Microsoft.CodeAnalysis.BannedApiAnalyzers` 5.6.0, `MSTest.Analyzers` 4.3.3,
`Roslynator.Analyzers` 5.0.0, `SonarAnalyzer.CSharp` 10.33.0.1635.

Unresolved: 0

Output Summary: All 162 `Analyzer` `Include` values across the 18 non-`packages` project files
resolve to an existing file under the workspace-root `packages\` directory. No package install or
copy step was required and no re-run of the audit was needed. Only repository-relative paths and
package identifiers are recorded here; no absolute host path, account name, or machine name is
written into this artifact.
