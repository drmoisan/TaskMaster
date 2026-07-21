# Baseline — Pragma-Driven Nullable Gate (Fail-Before)

Timestamp: 2026-07-19T05-25
Command: `MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (no `/p:Nullable=enable`)
EXIT_CODE: 1

Output Summary: Build FAILED with 2 Error(s) (`Time Elapsed 00:00:00.77`), both CS0649 in
`SVGControl/SvgImageSelector.cs`:
- `_absoluteImagePath` (line 57): "never assigned to, and will always have its default value null"
- `_relativeImagePath` (line 56): same diagnostic

This build-blocking defect (BUILD DEBT 1) stops the `/t:Rebuild` dependency chain before any
downstream project (including `UtilitiesCS.csproj`) is reached, so this is the raw first-run
observation of the fail-before baseline.

A second, diagnostic-scoping-only run was executed immediately after with
`/p:WarningsNotAsErrors=CS0649` added (purely to measure BUILD DEBT 2 past BUILD DEBT 1; this
flag is NOT part of the finalized gate and is not retained past this measurement task):

Command: `MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649`
EXIT_CODE: 1 (Build FAILED, 163 Error(s) per the MSBuild summary; each error is emitted twice
under `/m` parallel build — once inline under a project-number prefix, once in the final error
summary — so the diagnostic-code counts below are the doubled raw grep counts, matching the
convention already used in this plan's Revision note.)

Diagnostic-code breakdown (doubled raw counts; halve for true occurrence count):
- CS8604: 110 (raw) -> 55 true
- CS8602: 100 (raw) -> 50 true
- CS8601: 32 (raw) -> 16 true
- CS0618: 28 (raw) -> 14 true
- CS8625: 18 (raw) -> 9 true
- CS8603: 18 (raw) -> 9 true
- CS8619: 8 (raw) -> 4 true
- CS8620: 6 (raw) -> 3 true
- CS8600: 4 (raw) -> 2 true
- CS0168: 2 (raw) -> 1 true

Sum of doubled counts = 326, exactly matching this plan's Revision note figure
("296+28+2 = 326 total measured this session"). Distinct affected first-party `.cs` files
under `UtilitiesCS/EmailIntelligence/**` and `UtilitiesCS/OutlookObjects/Folder/**`: 62 files
(confirmed by de-duplicating the file-path portion of every error line, excluding
`SvgImageSelector.cs` which is BUILD DEBT 1's file and is demoted to a warning under this
`WarningsNotAsErrors=CS0649` scoping run). `SvgImageSelector.cs`'s own CS0649 is correctly
demoted to a warning (not an error) under this scoping run, confirming BUILD DEBT 1 and
BUILD DEBT 2 are independently measurable and additive.

This baseline was revised from an earlier plan draft that assumed EXIT_CODE 0 based on research
written against an earlier pre-fan-in worktree tip (dd17719a); the actual tip (bfcdb394) has all
12 children's file-scope remediation merged, producing real cross-child fan-in debt (BUILD DEBT
1: SVGControl/SvgImageSelector.cs CS0649; BUILD DEBT 2: ~296+28+2 diagnostics across
UtilitiesCS/EmailIntelligence/** and UtilitiesCS/OutlookObjects/Folder/**) that was never
previously measured, because ci.yml only triggers on PRs to main/development, never the
integration branch. This artifact is the fail-before evidence baseline referenced by Phase 1,
Phase 2, and Phase 7's coverage-delta comparison.
