# P2-T9 — `.editorconfig` Severities Toolchain Verification (Issue #181)

Timestamp: 2026-06-08T13-16

Purpose (revision 2.0): Verify that the revision-2 `.editorconfig` change (P2-T8: SecurityCodeScan `SCS*` severities removed; the five in-scope analyzers' severities MA*/S*/RCS*/AsyncFixer*/RS0030 retained) does not regress the build, run against the current on-disk state. At this point the 15 first-party `.csproj` files STILL contain the SecurityCodeScan + co-located `YamlDotNet.dll` `<Analyzer Include>` items (Phase 4 cleanup not yet performed) and the 15 first-party `packages.config` files still contain the SecurityCodeScan.VS2019 `<package>` entries (Phase 3 cleanup not yet performed).

> Note: this artifact supersedes the v1.0 P2-T9 record. The v1.0 record verified the `.editorconfig` severity ADDITIONS before any csproj wiring; this revision-2 record verifies the SCS-severity REMOVAL against the v1.0-wired tree.

## Toolchain Step Results

### Step 1 — `dotnet tool restore`
Command: `dotnet tool restore`
EXIT_CODE: 0
Output Summary: csharpier 1.2.6 restored successfully.

### Step 2 — CSharpier format check
Command: `dotnet tool run csharpier check .`
EXIT_CODE: 1
Output Summary:
- Checked 1057 files. 31 files reported "Was not formatted".
- 1 `.cs` file: `UtilitiesCS\Extensions\IEnumerableExtensions.cs`. This is the PRE-EXISTING Phase 0 baseline finding (see `evidence/baseline/baseline-format.2026-06-08T12-12.md`), a `System.Threading.Timer` lambda formatting difference in a file this plan does not touch. Recorded as a baseline condition, not a regression; not reformatted to preserve plan scope.
- 30 XML files: the 15 first-party `packages.config` and 15 first-party `.csproj` files modified by plan v1.0. CSharpier 1.2.6 formats XML project files; the v1.0-inserted analyzer entries are not in CSharpier's canonical XML layout. These same 30 files are edited by Phase 3 (packages.config) and Phase 4 (.csproj) of this revised plan; they will be normalized and re-checked via the CSharpier format step at final QA (P6-T1) after cleanup.
- No first-party `.cs` source file other than the pre-existing baseline file is flagged.

### Step 3 — `nuget restore TaskMaster.sln`
Command: `nuget.exe restore TaskMaster.sln`
EXIT_CODE: 0
Output Summary: All packages listed in packages.config already installed. Restore succeeded.

### Step 4 — Analyzer / code-style build
Command: `msbuild TaskMaster.sln -t:Rebuild -p:Configuration=Debug "-p:Platform=Any CPU" -p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0
Output Summary:
- Build succeeded. 0 Error(s).
- 0 instances of CS8032 in this build mode.
- The five in-scope analyzers are wired and active; their rule IDs (MA*/S*/RCS*/AsyncFixer*/RS0030) are emitted at `suggestion` severity, which MSBuild surfaces as messages rather than warnings, consistent with the severity-first ordering invariant. No in-scope analyzer diagnostic is promoted to error.

### Step 5 — Nullable TreatWarningsAsErrors build (PROTECTED no-regression gate)
Command: `msbuild TaskMaster.sln -t:Rebuild -p:Configuration=Debug "-p:Platform=Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true`
EXIT_CODE: 1
Output Summary:
- 84 Error(s), 0 Warning(s). MSBuild final summary: `84 Error(s)`.
- ALL 84 errors are confined to the two vendored projects (SVGControl, UtilitiesSwordfish.NET.General). Distinct error lines by project: SVGControl 68, UtilitiesSwordfish.NET.General 100 (each error emitted twice under parallel build, halving to the 84-error summary). Zero errors in any first-party project.
- Error categories are pre-existing nullable diagnostics (CS8625, CS8618, CS8603, CS8600, CS8602, CS8601, CS0649, CS8619, CS8604).
- 0 instances of CS8032.

OBSERVED-STATE NOTE (recorded faithfully): The plan's P2-T9 note anticipated that, because the `.csproj` files still reference the SecurityCodeScan + YamlDotNet analyzers, the nullable step would still show the +16 CS8032 regression (84 -> 100) recorded in the v1.0 P4-T16 blocking finding. In this run the nullable step instead returned 84 errors with 0 CS8032 — already at the Phase 0 baseline — even though the SecurityCodeScan/YamlDotNet `<Analyzer Include>` items and the restored `SecurityCodeScan.VS2019.5.6.7` package (with the co-located `YamlDotNet.dll`) remain on disk. The CS8032 load failure did not reproduce in this environment/run. This discrepancy from the recorded v1.0 finding does NOT alter the plan: the SecurityCodeScan.VS2019 wiring is still removed entirely in Phase 3/Phase 4 per the revision-2 decision to drop the analyzer, and the protected-gate success condition (84 errors, no CS8032) remains the target. The no-regression assertion is formally confirmed at P4-T16 after the cleanup edits.

### Step 6 — MSTest with coverage
Command: `vstest.console.exe <7 first-party *.Test.dll> /EnableCodeCoverage /InIsolation /Logger:trx`
EXIT_CODE: 1
Output Summary:
- Total tests: 4064; Passed: 4057; Failed: 4 (distinct); Skipped: 2.
- The 4 failing tests are timing/timer/threading-sensitive (known-flaky category matching the Phase 0 baseline of 4 timer-sensitive failures): `ConcurrentEnqueue_BatchesAllItems`, `EmptyQueue_AfterSeveralIntervals_StopsTimer`, `Serialize_WithConfiguredPath_TriggersDeferredThreadSafeWrite`, `WireNotifications_OnMappedToChange_RaisesPropertyChanged`. The specific failing subset varies per run (wall-clock timer dependence). They are unrelated to the `.editorconfig` change (which cannot affect test execution) and are recorded as a baseline flakiness condition, not a regression.
- Repo-wide line coverage (raw merged Cobertura): line-rate 0.5887 = 58.87% (lines-covered 101533 of lines-valid 172456). Within rounding of the 58.89% Phase 0 baseline; no coverage regression. Authoritative 80%/90% gate is the CI run.

## CS8032 controllability note
CS8032 is a C# compiler warning (analyzer instance cannot be created), not an analyzer rule ID, and therefore cannot be set to `suggestion` via `.editorconfig`. The only way to eliminate any CS8032 attributable to SecurityCodeScan is to remove the SecurityCodeScan/YamlDotNet `<Analyzer Include>` items from the first-party `.csproj` files (Phase 4) and the SecurityCodeScan.VS2019 `<package>` entries from the first-party `packages.config` files (Phase 3). The protected-gate no-regression assertion is deferred to the post-cleanup verification in P4-T16.

## Verdict
The P2-T8 `.editorconfig` change retains the five in-scope analyzers' severities at `suggestion`, removes all SCS* severities, and does not introduce any new error in the analyzer/code-style build (step 4: 0 errors). The protected nullable gate is at the 84-error vendored-only baseline with no CS8032. CSharpier XML findings on the 30 v1.0-edited project files and the 1 pre-existing baseline `.cs` file are documented and will be resolved/re-checked through the planned Phase 3/4 cleanup and final QA. P2-T9 verification complete.
