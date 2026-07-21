# Analyzer / Codestyle Build Baseline (P0-T3)

- Timestamp: 2026-07-19T10-50
- Task: [P0-T3]
- Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Resolved binary: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe` (VS18 full-framework MSBuild; not on PATH).
- EXIT_CODE: 0

## Output Summary

- Result: Build succeeded. 0 errors.
- Total warnings: 78 (analyzer + code-style; not promoted to errors because `/p:TreatWarningsAsErrors` is NOT set for this stage).
- Pre-remediation warning-code breakdown:
  - CS8632 (nullable annotation outside `#nullable` context): 33
  - CS0618 (obsolete usage): 28
  - CS0108 (hides inherited member): 4
  - CS0169 (field never used): 3
  - CS0067 (event never used): 3
  - CS0649 (field never assigned): 2  (vendored SVGControl pre-existing)
  - MSTEST0032: 1
  - CS8625: 1
  - CS4014: 1
  - CS2002: 1
  - CS0168: 1
- These are the pre-existing baseline warnings against which the final P10-T2 analyzer gate is compared. The nullable/type-check gate is run separately (P0-T4) under `/p:TreatWarningsAsErrors=true /t:Rebuild` without `/p:Nullable=enable`.
