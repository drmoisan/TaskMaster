# QC — Nullable / Type Build (TreatWarningsAsErrors)

Timestamp: 2026-07-16T03-32

Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
Actual invocation (this host): "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe" ... plus a supplementary forced Rebuild of the two touched projects (Platform=AnyCPU) to genuinely exercise nullable flow on the new/changed files.

EXIT_CODE: 0 (incremental full-solution Build) ; feature verdict: PASS

Output Summary:
- Incremental full-solution nullable Build (/t:Build): Build succeeded, 0 Warning(s), 0 Error(s), EXIT 0.
- Supplementary forced Rebuild (/t:Rebuild UtilitiesCS.Test with dependency chain) under /p:Nullable=enable /p:TreatWarningsAsErrors=true, serial (-m:1):
  - Distinct nullable errors in vendored SVGControl.csproj: 34 (IDENTICAL to the P0-T4 baseline set; the feature did not change the vendored debt).
  - Nullable errors in UtilitiesCS.csproj: 0.
  - Nullable errors in UtilitiesCS.Test.csproj: 0.
  - No non-SVGControl project reports any nullable error.
- Verdict: the two first-party projects this feature touches are nullable-clean under TreatWarningsAsErrors. The only failures are the pre-existing vendored SVGControl nullable debt (out of feature scope, unchanged from baseline). The feature introduces zero new nullable/type diagnostics.
