# Baseline Analyzer / Code-Style Build

Timestamp: 2026-07-19T00-20

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
(invoked via `MSBuild.exe` from `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`)

EXIT_CODE: 1

Output Summary:

Solution-wide build FAILED with 8 Error(s) / 2 Warning(s) total (aggregated across all
failing projects). All 8 errors are `CS0006: Metadata file '...' could not be found` for
three analyzer DLLs (`Meziantou.Analyzer.3.0.101`, `SonarAnalyzer.CSharp.10.27.0.140913`,
`Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4`), and they occur ONLY in
`UtilitiesCS\UtilitiesCS.csproj` and `VBFunctions\VBFunctions.csproj`. These two `.csproj`
files hard-code `<Analyzer Include>` paths to older analyzer package versions
(`Meziantou.Analyzer.3.0.101`, `SonarAnalyzer.CSharp.10.27.0.140913`,
`Microsoft.CodeAnalysis.BannedApiAnalyzers.3.3.4`) that do not match the versions actually
present in `packages/` after `scripts/vscode/Invoke-Restore.ps1` (`Meziantou.Analyzer.3.0.123`,
`SonarAnalyzer.CSharp.10.29.0.143774`, `Microsoft.CodeAnalysis.BannedApiAnalyzers.5.6.0`).
This is a pre-existing package-version-pin defect on the epic integration branch tip, confirmed
present before any change by this feature. It is out of scope for this feature: this feature
touches only `SVGControl/` hand-authored `.cs` files and does not modify `UtilitiesCS.csproj`
or `VBFunctions.csproj`.

`SVGControl\SVGControl.csproj` itself built successfully with 2 warnings and 0 errors:
- `CS0649` on `SvgImageSelector.cs(55,24)` — field `_relativeImagePath` never assigned (pre-existing).
- `CS0649` on `SvgImageSelector.cs(56,24)` — field `_absoluteImagePath` never assigned (pre-existing).

`SVGControl.csproj` has no `<Analyzer Include>` items at all (confirmed by grep), so it is
unaffected by the analyzer-package-version mismatch that fails `UtilitiesCS`/`VBFunctions`.

This baseline confirms the pre-existing solution-wide analyzer-build failure is unrelated to
`SVGControl/` and will be re-confirmed unchanged (same 8 errors, same 2 files) at Phase 6
final QC, since this feature does not modify `UtilitiesCS.csproj` or `VBFunctions.csproj`.
