# Final QC — Step 3 (Nullable TreatWarningsAsErrors Build, No-Regression Gate) (Issue #181, Cycle 2)

Timestamp: 2026-06-08T18-06

Command: msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
EXIT_CODE: 1

Output Summary:
- 84 Error(s) (MSBuild summary) — EQUAL to the documented vendored-only baseline
  (cycle-1 `final-nullable-build.2026-06-08T12-12.md` = 84). NO REGRESSION.
- 0 instances of CS8032 (no SecurityCodeScan reintroduction; the protected gate is clean).
- 0 first-party errors: filtering all `: error ` lines to exclude the two vendored
  projects (SVGControl, UtilitiesSwordfish.NET.General) returns ZERO first-party errors.
- Error distribution (raw log lines are doubled by MSBuild — compile pass + per-project
  summary — so 168 raw lines = 84 distinct errors): SVGControl.csproj 68 lines (34 errors),
  UtilitiesSwordfish.NET.General.csproj 100 lines (50 errors). 34 + 50 = 84.
- 0 errors reference `UtilitiesCS/Extensions/IEnumerableExtensions.cs`; the formatting-only
  change introduced no nullable diagnostics.
- EXIT_CODE 1 matches the documented baseline EXIT_CODE (the baseline gate also fails at
  84 vendored errors). This is the expected protected-gate state; the no-regression
  condition is satisfied because the error count and project distribution are identical
  to the cycle-1 baseline.

Note on build invocation: an initial incremental `/t:Build` invocation reported 0 errors
because outputs were up-to-date from the P2-T3 analyzer build (the nullable property change
alone did not invalidate the up-to-date check). `/t:Rebuild` was used to faithfully reproduce
the documented baseline gate state, matching the cycle-1 command.

Verdict (AC5): The protected nullable gate holds at the 84-error vendored-only baseline
with 0 CS8032 and 0 first-party errors. The formatting-only change does NOT regress the
nullable gate.
