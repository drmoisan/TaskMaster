# Baseline — Nullable / Type Build (TreatWarningsAsErrors)

Timestamp: 2026-07-16T03-32

Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
Actual invocation (this host): "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" ... (git-bash, MSYS_NO_PATHCONV=1)

EXIT_CODE: 0 (incremental Build) ; 1 (forced Rebuild — pre-existing vendored debt)

Output Summary:
- Incremental full-solution nullable Build (/t:Build): Build succeeded, 0 Warning(s), 0 Error(s), EXIT 0. (Reuses analyzer-build outputs; MSBuild treats projects as up to date.)
- Supplementary forced Rebuild (/t:Rebuild UtilitiesCS.Test with dependency chain, Platform=AnyCPU) under /p:Nullable=enable /p:TreatWarningsAsErrors=true: 34 Error(s), 0 Warning(s), EXIT 1.
  - ALL 34 errors are confined to the vendored third-party project SVGControl.csproj (CS8618/CS8625/CS8603/CS8600/CS8602/CS0649 nullable-flow diagnostics).
  - 0 errors originate in first-party UtilitiesCS.csproj or UtilitiesCS.Test.csproj.
- This vendored SVGControl nullable debt is pre-existing baseline state, independent of and out of scope for this feature (folder-probability-plumbing #324). Consistent with prior sessions (nullable Rebuild surfaces vendored-only debt).
- Feature acceptance for the nullable gate: this baseline establishes that the two first-party projects this feature touches (UtilitiesCS, UtilitiesCS.Test) are nullable-clean at baseline; the post-change gate (P4-T3) must not introduce any NEW nullable/type error in those two projects beyond this SVGControl-only baseline.
