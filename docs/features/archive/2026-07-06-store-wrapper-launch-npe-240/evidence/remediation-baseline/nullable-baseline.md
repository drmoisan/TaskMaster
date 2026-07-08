# Nullable/Type-Check Baseline (Remediation Cycle, Issue #240)

- Timestamp: 2026-07-06T12-15
- Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- EXIT_CODE: 1
- Output Summary: 84 pre-existing nullable errors, confined entirely to `SVGControl.csproj` and `UtilitiesSwordfish\UtilitiesSwordfish.NET.General.csproj` (verified via `grep -oE "\[.*\.csproj\]"` deduplicated to exactly these two vendored/legacy project paths). No errors in `UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj`, or any other first-party project. These pre-existing failures are out of scope for this remediation cycle (Finding 1 only touches `UtilitiesCS.Test`) and are unaffected by the split.
