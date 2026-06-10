# Analyzer DLL Relative Paths for <Analyzer Include> (Issue #181)

Timestamp: 2026-06-08T12-27
Command: directory listing of each restored package's analyzers/ tree (ls / find under packages/<id>.<version>/analyzers)

IMPORTANT FINDING — roslyn-version subfolders:
The build compiler is Roslyn 5.6.0 (csc 5.6.0-2.26230.15). Several packages ship analyzer DLLs under roslyn-version subfolders (analyzers/dotnet/roslynX.Y/cs/) rather than a single analyzers/dotnet/cs/. For non-SDK projects using hard-coded <Analyzer Include> paths, the correct subfolder must be chosen explicitly (NuGet does not auto-select it for packages.config projects). The highest-compatible subfolder at or below the compiler version is selected. Roslyn analyzers are forward-compatible (an analyzer built for an older Roslyn loads in a newer compiler).

Relative paths are written from a project directory one level under the repo root (e.g. QuickFiler\QuickFiler.csproj), i.e. `..\packages\...`. All 15 first-party projects are one directory deep, so the same relative form applies to each.

## Meziantou.Analyzer 3.0.101 (selected: roslyn5.0)
- ..\packages\Meziantou.Analyzer.3.0.101\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll

## SonarAnalyzer.CSharp 10.27.0.140913
- ..\packages\SonarAnalyzer.CSharp.10.27.0.140913\analyzers\SonarAnalyzer.CSharp.dll

## Roslynator.Analyzers 4.15.0 (selected: roslyn4.7; multi-DLL set — all required for the analyzer to load)
- ..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll
- ..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll
- ..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll
- ..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll

## AsyncFixer 2.1.0
- ..\packages\AsyncFixer.2.1.0\analyzers\dotnet\cs\AsyncFixer.dll

## SecurityCodeScan.VS2019 5.6.7
- ..\packages\SecurityCodeScan.VS2019.5.6.7\analyzers\dotnet\SecurityCodeScan.VS2019.dll
- ..\packages\SecurityCodeScan.VS2019.5.6.7\analyzers\dotnet\YamlDotNet.dll  (REQUIRED dependency — see correction note below)

CORRECTION (P4-T16): SecurityCodeScan.VS2019.dll has a runtime dependency on YamlDotNet 11.0.0.0 (co-located in the same analyzers/dotnet/ folder). When only SecurityCodeScan.VS2019.dll is wired, every SecurityCodeScan analyzer type fails to initialize with CS8032 (TypeInitializationException -> FileNotFoundException for YamlDotNet). CS8032 is a compiler warning that is promoted to an error under /p:TreatWarningsAsErrors=true, which regressed the protected nullable gate (first observed as +16 errors in VBFunctions during the P4-T16 rebuild). The fix is to also wire YamlDotNet.dll as an <Analyzer Include> so it joins the analyzer load context. Both DLLs are now included in every first-party .csproj.

## Microsoft.CodeAnalysis.BannedApiAnalyzers 3.3.4
- ..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4\analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll
- ..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll

## Canonical per-project <ItemGroup> template (the "6 analyzer packages" wired set)
```xml
<ItemGroup>
  <!-- Meziantou.Analyzer 3.0.101 -->
  <Analyzer Include="..\packages\Meziantou.Analyzer.3.0.101\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll" />
  <!-- SonarAnalyzer.CSharp 10.27.0.140913 -->
  <Analyzer Include="..\packages\SonarAnalyzer.CSharp.10.27.0.140913\analyzers\SonarAnalyzer.CSharp.dll" />
  <!-- Roslynator.Analyzers 4.15.0 (roslyn4.7) -->
  <Analyzer Include="..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll" />
  <Analyzer Include="..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll" />
  <Analyzer Include="..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll" />
  <Analyzer Include="..\packages\Roslynator.Analyzers.4.15.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll" />
  <!-- AsyncFixer 2.1.0 -->
  <Analyzer Include="..\packages\AsyncFixer.2.1.0\analyzers\dotnet\cs\AsyncFixer.dll" />
  <!-- SecurityCodeScan.VS2019 5.6.7 -->
  <Analyzer Include="..\packages\SecurityCodeScan.VS2019.5.6.7\analyzers\dotnet\SecurityCodeScan.VS2019.dll" />
  <!-- Microsoft.CodeAnalysis.BannedApiAnalyzers 3.3.4 -->
  <Analyzer Include="..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4\analyzers\dotnet\cs\Microsoft.CodeAnalysis.BannedApiAnalyzers.dll" />
  <Analyzer Include="..\packages\Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll" />
  <!-- BannedSymbols.txt shared file at repo root -->
  <AdditionalFiles Include="$(MSBuildThisFileDirectory)..\BannedSymbols.txt" />
</ItemGroup>
```
