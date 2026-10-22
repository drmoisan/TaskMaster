Timestamp: 2026-09-09T11-43
Command: Select-String -Path UtilitiesCS.Test\UtilitiesCS.Test.csproj -Pattern 'Analyzer Include="([^"]+)"' + Test-Path per resolved path (relative to UtilitiesCS.Test\); version-token comparison against UtilitiesCS.Test\packages.config
EXIT_CODE: 0
Output Summary: All 11 <Analyzer Include> paths in UtilitiesCS.Test.csproj resolve True relative to UtilitiesCS.Test\. Meziantou.Analyzer path token "3.0.203" equals packages.config's Meziantou.Analyzer version="3.0.203". Roslynator.Analyzers path token "5.0.0" equals packages.config's Roslynator.Analyzers version="5.0.0". No skew detected.

Resolved paths (all True):
..\packages\MSTest.Analyzers.4.4.0\analyzers\dotnet\cs\MSTest.Analyzers.CodeFixes.dll
..\packages\MSTest.Analyzers.4.4.0\analyzers\dotnet\cs\MSTest.Analyzers.dll
..\packages\SonarAnalyzer.CSharp.10.33.0.1635\analyzers\SonarAnalyzer.CSharp.dll
..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll
..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll
..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll
..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll
..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll
..\packages\AsyncFixer.2.1.0\analyzers\dotnet\cs\AsyncFixer.dll
..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll
..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll
