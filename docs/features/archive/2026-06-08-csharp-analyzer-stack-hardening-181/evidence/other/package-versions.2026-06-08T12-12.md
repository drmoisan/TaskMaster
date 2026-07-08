# Analyzer Package Versions (Issue #181)

Timestamp: 2026-06-08T12-27
Command (per package): nuget.exe install <id> -OutputDirectory packages -DependencyVersion Ignore
EXIT_CODE: 0 (all six installs succeeded)

Build toolchain Roslyn/compiler version (governs analyzer DLL roslyn-subfolder selection):
- csc.exe -version: 5.6.0-2.26230.15
- Microsoft.CodeAnalysis.dll ProductVersion: 5.6.0-2.26230.15
- MSBuild: 18.6.3 (.NET Framework), VS18 Community.

## Meziantou.Analyzer
- Resolved version: 3.0.101
- Command: nuget.exe install Meziantou.Analyzer -OutputDirectory packages -DependencyVersion Ignore
- EXIT_CODE: 0
- Compatibility: package ships roslyn-versioned analyzer subfolders (roslyn4.2, 4.4, 4.6, 4.8, 4.14, 5.0). For compiler 5.6.0 the roslyn5.0 build is selected.

## SonarAnalyzer.CSharp
- Resolved version: 10.27.0.140913
- Command: nuget.exe install SonarAnalyzer.CSharp -OutputDirectory packages -DependencyVersion Ignore
- EXIT_CODE: 0
- Compatibility: analyzer DLL at analyzers/SonarAnalyzer.CSharp.dll (no roslyn-version subfolder).

## Roslynator.Analyzers
- Resolved version: 4.15.0
- Command: nuget.exe install Roslynator.Analyzers -OutputDirectory packages -DependencyVersion Ignore
- EXIT_CODE: 0
- Compatibility: ships roslyn3.8 and roslyn4.7 analyzer subfolders. For compiler 5.6.0 the roslyn4.7 build is selected (highest available). Multiple support DLLs ship alongside the analyzer.

## AsyncFixer
- Resolved version: 2.1.0
- Command: nuget.exe install AsyncFixer -OutputDirectory packages -DependencyVersion Ignore
- EXIT_CODE: 0
- DLL loads under VS2022/VS18 build tools: expected yes. The plan referenced 1.6.0; the current stable on the feed is 2.1.0, which targets a modern Roslyn loader (analyzers/dotnet/cs/AsyncFixer.dll). 2.1.0 is the compatible version for Roslyn 4.x/5.x; 1.6.0 targeted older Roslyn and is superseded. Resolved version 2.1.0 is used.

## SecurityCodeScan.VS2019
- Resolved package id confirmed: SecurityCodeScan.VS2019 (exists on the NuGet feed).
- Resolved version: 5.6.7
- Command: nuget.exe install SecurityCodeScan.VS2019 -OutputDirectory packages -DependencyVersion Ignore
- EXIT_CODE: 0
- Compatibility: analyzer DLL at analyzers/dotnet/SecurityCodeScan.VS2019.dll plus YamlDotNet.dll dependency in the same folder. Compatibility with VS18/Roslyn 5.6 verified at build time (P4-T16/P4-T17).

## BannedApiAnalyzers
- Resolved id: Microsoft.CodeAnalysis.BannedApiAnalyzers
- Resolved version: 3.3.4
- Command: nuget.exe install Microsoft.CodeAnalysis.BannedApiAnalyzers -OutputDirectory packages -DependencyVersion Ignore
- EXIT_CODE: 0
- Compatibility: analyzer DLLs at analyzers/dotnet/cs/Microsoft.CodeAnalysis.BannedApiAnalyzers.dll and analyzers/dotnet/cs/Microsoft.CodeAnalysis.CSharp.BannedApiAnalyzers.dll.

## Resolved version summary (for packages.config entries)
| Package | Version |
|---|---|
| Meziantou.Analyzer | 3.0.101 |
| SonarAnalyzer.CSharp | 10.27.0.140913 |
| Roslynator.Analyzers | 4.15.0 |
| AsyncFixer | 2.1.0 |
| SecurityCodeScan.VS2019 | 5.6.7 |
| Microsoft.CodeAnalysis.BannedApiAnalyzers | 3.3.4 |
