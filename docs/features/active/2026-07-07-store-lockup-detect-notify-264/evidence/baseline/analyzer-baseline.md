# Analyzer-Build Baseline (P0-T7)

Timestamp: 2026-07-08T07-58

Command:
`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
(MSBuild from VS18 Community, located via vswhere; run from repo root with MSYS_NO_PATHCONV=1.)

EXIT_CODE: 0

Output Summary:
- Build succeeded.
- 0 Error(s).
- 75 Warning(s) — all pre-existing in test code, not F4-related. Predominant kinds:
  - CS8632 (nullable annotation used outside a `#nullable` context) in UtilitiesCS.Test files.
  - CS0067 (event never used) in UtilitiesCS.Test stub classes.
- This 75-warning, 0-error state is the analyzer baseline against which P9-T2 verifies no increase.
