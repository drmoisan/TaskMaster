# QA-03 — Nullable / TreatWarningsAsErrors

Timestamp: 2026-06-13T00-38

Command (canonical, per plan/policy):
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true

EXIT_CODE: 1 (canonical whole-solution form — pre-existing, out-of-scope vendored breakage; see analysis)

Scoped verification of the changed files (authoritative for this test-only change):
- Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true  (after touching the two changed files to force recompilation of UtilitiesCS.Test)
- Diagnostics referencing the two changed files (`TimeOutTask_Tests.cs`, `TimeOutTask_AdditionalTests.cs`): NONE_IN_CHANGED_FILES (grep of `: (error|warning) CS` lines filtered to the changed files returned no matches).
- EXIT_CODE of the scoped recompile: 1, solely due to pre-existing UtilitiesCS.Test warnings in unrelated files (20 diagnostics: CS8632 nullable-context in ProgressTracker_Tests.cs / ConversationHelper_ExtendedTests.cs / OlTableExtensions_Tests.cs, and CS0067 unused-event in SmartSerializable_Tests.cs / SmartSerializableBase_Tests.cs / StoreWrapperControllerTests.cs) being promoted to errors by TreatWarningsAsErrors. None are introduced by this change.

Output Summary:
- The change introduces ZERO new nullable or compiler diagnostics. A `TreatWarningsAsErrors=true` recompile of UtilitiesCS.Test that actually rebuilds the two changed files reports no diagnostic on either changed file.
- The whole-solution `/p:Nullable=enable /p:TreatWarningsAsErrors=true` form cannot pass in this repository for environmental, out-of-scope reasons documented in repo policy (csharp.md "Analyzer Stack"): the 4 vendored projects (SVGControl, UtilitiesSwordfish.NET.General, and the two vendored test projects) are explicitly excluded from analyzer/nullable enforcement, but the global `Nullable=enable` MSBuild property force-enables nullable on them, producing 84 pre-existing vendored errors (e.g., SVGControl CS8600/CS8602/CS8618/CS0649, UtilitiesSwordfish CS8603/CS8625/CS8601). These are not caused by, and cannot be addressed by, this test-only change (test-only scope; 0 production files).
- After verification, the clean Debug build was restored: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` -> 0 Error(s), EXIT 0, ensuring a valid `UtilitiesCS.Test.dll` for the test gate (P2-T4).

Gate verdict for this change: PASS for the changed files (no new nullable/compiler diagnostics). The non-zero whole-solution exit is attributable entirely to pre-existing, explicitly out-of-scope vendored-project and unrelated-test-file conditions, not to the change under audit.
