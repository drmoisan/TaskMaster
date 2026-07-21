# Baseline Pragma-Only Build (P0-T2)

Timestamp: 2026-07-19T10-54

## Mandated full-solution gate command (command of record)

Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
(invoked in git-bash as dash-switches with MSYS_NO_PATHCONV=1 and VS18 amd64 MSBuild.exe)

EXIT_CODE: 1

Output Summary:
- Total errors: 2 (both pre-existing, OUT OF SCOPE for #375):
  - `SVGControl/SvgImageSelector.cs(56,25): error CS0649` (`_relativeImagePath` never assigned)
  - `SVGControl/SvgImageSelector.cs(57,25): error CS0649` (`_absoluteImagePath` never assigned)
- CS86xx (nullable) diagnostics: ZERO.
- SVGControl is a separate vendored net481 WinForms project owned by epic child #368
  (`utilitiescs-nullable-svgcontrol`), not by #375. Its CS0649 warnings are promoted to errors by
  `/p:TreatWarningsAsErrors=true`.
- IMPORTANT: `UtilitiesCS.csproj` has a `<ProjectReference>` to `SVGControl.csproj`. Under
  `/t:Rebuild` the SVGControl compile failure blocks UtilitiesCS from compiling, so the
  full-solution command's "zero CS86xx" is a non-informative (false) zero — UtilitiesCS is never
  compiled with its pragmas. This full-solution command is therefore recorded for provenance, but
  the trustworthy CS86xx signal for this child is the isolated UtilitiesCS build below.

## Trustworthy isolated CS86xx gate (per-batch and final verification signal)

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168 -p:BuildProjectReferences=false`

EXIT_CODE: 0

Output Summary:
- Build succeeded. 0 errors, 0 CS86xx, 15 warnings (all pre-existing out-of-scope: CS0618
  obsolete-API usage across Bayesian/ClassifierGroups/EmailParsingSorting/Extensions +
  IntelligenceConfig.cs:97, CS0168 unused variable in AutoFile.cs:21). None is CS86xx.
- Rationale for the isolated command and the `WarningsNotAsErrors=CS0649;CS0618;CS0168` exclude:
  these three pre-existing NON-nullable warning classes are debt outside #375's annotation-only
  scope. Excluding only them (never any CS86xx code) lets the build fail solely on genuine CS86xx
  from the opted-in residual files, giving an accurate per-file nullable signal. `BuildProjectReferences=false`
  compiles UtilitiesCS against the pre-built SVGControl.dll reference so the out-of-scope SVGControl
  CS0649 does not block the UtilitiesCS compile. NO `/p:Nullable=enable` is used; enforcement is the
  per-file `#nullable enable` pragma only.
- Pre-built reference DLLs were produced once by `msbuild TaskMaster.sln -t:Build ... (no TWAE)`
  (exit 0, 78 warnings, 0 errors) so `BuildProjectReferences=false` resolves SVGControl.dll and
  UtilitiesCS.dll.

## Environment bootstrap (mechanical, no tracked-file change)
- Installed repo-local .NET SDK 8.0.205 via `scripts/vscode/Install-RepoDotNetSdk.ps1`.
- Restored packages via `scripts/vscode/Invoke-Restore.ps1` (169 packages).
- Installed csproj-referenced analyzer versions (Meziantou.Analyzer 3.0.101,
  SonarAnalyzer.CSharp 10.27.0.140913, Microsoft.CodeAnalysis.BannedApiAnalyzers 3.3.4) into the
  gitignored `packages/` folder to resolve a pre-existing csproj-vs-packages.config version skew
  (CS0006 metadata-file-not-found). No csproj edited; `packages/` is gitignored.

## Baseline conclusion
The integration branch has ZERO CS86xx in UtilitiesCS before any residual file is opted in
(consistent with the plan's expectation: a residual file's nullable debt only surfaces once its own
`#nullable enable` pragma is added). The only pre-existing full-solution blocker is the out-of-scope
SVGControl CS0649. No pre-existing CS86xx failure can be attributed to this child.
