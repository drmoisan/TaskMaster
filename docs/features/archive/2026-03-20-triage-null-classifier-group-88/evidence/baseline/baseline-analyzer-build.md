# Baseline — Analyzer Build

- **Timestamp:** 2026-03-20T09-48
- **Command:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- **EXIT_CODE:** 0
- **Output Summary:** Build succeeded. 0 errors, 18 warnings. Pre-existing warnings breakdown:
  - 3x MSB3277 (assembly version conflicts in UtilitiesCS.Test)
  - 1x CS8632 (nullable annotation outside nullable context in MeetingItemHelperTests.Part2.cs)
  - 3x CS0649 (fields never assigned in ReflectionHelper_Tests.cs)
  - 2x CS0169 (fields never used in ReflectionHelper_Tests.cs)
  - 9x MSTEST0044 (obsolete DataTestMethod usage across test files)
