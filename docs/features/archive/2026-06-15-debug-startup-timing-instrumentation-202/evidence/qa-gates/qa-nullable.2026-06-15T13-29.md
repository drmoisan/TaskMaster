# QA Gate — Type-check / Nullable (TreatWarningsAsErrors) (Issue #202, P2-T3)

Timestamp: 2026-06-15T13-29

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`

EXIT_CODE: 0

Output Summary:

- Build succeeded (MSBUILD exit 0) with `/p:Nullable=enable /p:TreatWarningsAsErrors=true`.
- Zero errors across the solution; zero warnings-as-errors.
- No nullable or warning-as-error diagnostics in either `ApplicationGlobalsTests.cs` or
  `ApplicationGlobalsStartupTimingTests.cs`. With the nullable context enabled, the CS8632
  annotations observed under the analyze gate are valid and do not fire here, confirming the
  split introduces no new type-safety regression.
