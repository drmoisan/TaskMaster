# Final QC — Analyzer / Code-Style Build (P9-T2)

Timestamp: 2026-07-19T22-03

## Command

`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
(VS18 full-framework msbuild.exe; `MSYS_NO_PATHCONV=1`).

EXIT_CODE: 0

## Output Summary

- Build succeeded (EXIT_CODE 0).
- Errors: 0.
- No new analyzer errors introduced by #366.
- CS8632 warnings (annotation-outside-nullable-context): 33 total, ALL in sibling-owned
  `TaskMaster.Test/**` test files (e.g. `StoresWrapperTests.cs`,
  `ApplicationGlobalsStartupTimingTests.cs`, `TestableApplicationGlobals.cs`,
  `StoreRehookCoordinatorTests.cs`, `AppToDoObjectsTests.cs`, `EngineInitTimingProbeTests.cs`).
  ZERO CS8632 warnings originate in the #366 `ReusableTypeClasses/**` cluster. These are
  pre-existing / sibling-owned and out of #366 scope.
- Remaining messages are benign MSBuild assembly-binding remap suggestions (Azure.Core,
  System.Text.Encoding.CodePages), pre-existing and unrelated to #366.

The analyzer/code-style build is clean for #366: zero errors, and no new analyzer diagnostic
originates in any remediated file.
