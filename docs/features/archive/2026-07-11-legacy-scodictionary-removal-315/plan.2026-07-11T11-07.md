# legacy-scodictionary-removal — Atomic Implementation Plan

- **Issue:** #315
- **Parent:** epic/swordfish-removal-integration
- **Owner:** drmoisan
- **Last Updated:** 2026-07-11T11-07
- **Status:** Ready for preflight
- **Version:** 1.0
- **Work Mode:** full-feature
- **AC Sources:** spec.md AND user-story.md

## Absolute Path Anchors

- FEATURE_WORKTREE: `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315`
- FEATURE_FOLDER: `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/docs/features/active/2026-07-11-legacy-scodictionary-removal-315`
- EVIDENCE_ROOT: `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/docs/features/active/2026-07-11-legacy-scodictionary-removal-315/evidence`
- SOLUTION: `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/TaskMaster.sln`
- TEST_ASSEMBLY (built output, Debug/Any CPU): `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`

## Strategy and Invariants

This is a bounded C# dead-code-removal refactor retiring the legacy `ScoDictionary<TKey,TValue>`
(`UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/SCODictionary.cs`, derived from
vendored `Swordfish.NET.Collections.ConcurrentObservableDictionary<,>`). The class has zero
production consumers; it is exercised only by test code. Removal eliminates the last first-party
`Swordfish.NET.Collections` binding attributable to `ScoDictionary`, unblocking epic child F5 (#308).

Execution order preserves compile-safety per the research file: retarget the SmartSerializable
infrastructure tests to `ScoDictionaryNew<>` first (both types still exist, so the tree compiles),
then delete the obsolete tests and their csproj entries, then delete the production class and its
csproj entry, then update stale comments.

Legacy non-SDK VSTO projects have no implicit/global usings; `<Compile Include>` entries must be
edited by hand. Because compile items are removed from `UtilitiesCS.csproj` and
`UtilitiesCS.Test.csproj`, a fresh msbuild is required so the build no longer references the deleted
files; the final QC build must start from a clean invocation.

`ScoDictionaryNew<>` implements `ISmartSerializable<>` via `IScoDictionaryNew`, so `IsSmartSerializable(...)`
returns TRUE for it — unlike the retired `ScoDictionary<>`, whose `IScoDictionary<>` interface does NOT
extend `ISmartSerializable<>` and therefore returned FALSE. Consequently `ScoDictionaryNew<>` is a drop-in
ONLY for the POSITIVE serialize/deserialize round-trip stand-ins. The NEGATIVE
`IsSmartSerializable == false` assertions must NOT retarget to `ScoDictionaryNew` (they would flip to
failing); instead they use a first-party non-smart-serializable type (`ConcurrentObservableCollection<int>`),
which does NOT implement `ISmartSerializable<>`, preserving the `.Should().BeFalse()` negative-path coverage.
The redundant Static negative (`IsSmartSerializable_ScoDictionary_ReturnsFalse`) is deleted because the
existing `IsSmartSerializable_ConcurrentObservableCollection_ReturnsFalse` already covers that case. No
retargeted test asserts on the `Config` property that `ScoDictionaryNew` adds.

On-disk JSON compatibility NFR: the retargeted tests use bare/default serializer settings and never
touch the globals-converter (`ScoDictionaryConverter`/`WrapperScoDictionary`) path, so no JSON shape
changes. The authoritative persisted-dictionary compatibility coverage is the existing, out-of-scope
`UtilitiesCS.Test/ReusableTypeClasses/SerializableNew/ScoDictionaryNew_OnDiskCompatibility_Tests.cs`,
which must remain green post-change (explicit assertion task in Phase 5).

## Out of Scope (do NOT touch)

- `IScoDictionary<TKey,TValue>` (`ISCODictionary.cs`) and `IPeopleScoDictionary`.
- `ScoDictionaryConverter`, `WrapperScoDictionary`, and their tests (`ScoDictionaryConverterTests.cs`,
  `WrapperScoDictionaryTest.cs`).
- `IntelligenceConfig_Tests.cs` (references only `ScoDictionaryNew`/`PeopleScoDictionaryNew`).
- `ObservableDictionary_Tests.cs` (unrelated Swordfish type).
- All F5/#308 interface/project/solution teardown (removing `UtilitiesSwordfish` project reference or
  deleting vendored folders).

## Evidence Rules (non-overridable)

All evidence artifacts MUST be written under `EVIDENCE_ROOT/<kind>/` using the canonical scheme
`<FEATURE>/evidence/<kind>/`. Writing to `artifacts/baselines/`, `artifacts/baseline/`,
`artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/coverage/`, or any other non-canonical path is a
policy violation. Each command-step artifact MUST include `Timestamp:`, `Command:`, `EXIT_CODE:`, and
`Output Summary:`. Baseline and final-QC test artifacts MUST record a numeric coverage headline.
Fail-closed: if any required baseline, QA, or coverage-comparison artifact is missing or incomplete,
the verdict is BLOCKED/INCOMPLETE, never PASS.

---

### Phase 0 — Baseline Capture and Policy Reads

- [x] [P0-T1] Read `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/CLAUDE.md` in full. Acceptance: file read in this session; recorded in the P0-T5 evidence artifact file list.
- [x] [P0-T2] Read `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/.claude/rules/general-code-change.md` in full. Acceptance: file read in this session; recorded in the P0-T5 evidence artifact file list.
- [x] [P0-T3] Read `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/.claude/rules/general-unit-test.md` in full. Acceptance: file read in this session; recorded in the P0-T5 evidence artifact file list.
- [x] [P0-T4] Read `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/.claude/rules/csharp.md` in full. Acceptance: file read in this session; recorded in the P0-T5 evidence artifact file list.
- [x] [P0-T5] Write policy-read evidence to `EVIDENCE_ROOT/baseline/phase0-instructions-read.md`. Acceptance: file exists and contains `Timestamp:` (ISO-8601 `yyyy-MM-ddTHH-mm`), `Policy Order:` (the ordered list CLAUDE.md → general-code-change.md → general-unit-test.md → csharp.md), and an explicit list of the four files read in P0-T1..P0-T4.
- [x] [P0-T6] Capture baseline CSharpier format state. Run `dotnet tool run csharpier --check .` (or `dotnet tool run csharpier .` followed by `git status` to detect drift) from FEATURE_WORKTREE. Write `EVIDENCE_ROOT/baseline/baseline-csharpier.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (formatted/needs-format count). Acceptance: artifact exists with all four fields populated.
- [x] [P0-T7] Capture baseline analyzer build. Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` against SOLUTION from FEATURE_WORKTREE. Write `EVIDENCE_ROOT/baseline/baseline-analyzer-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build succeeded/failed, warning/error counts). Acceptance: artifact exists with all four fields populated.
- [x] [P0-T8] Capture baseline nullable + warnings-as-errors build. Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` against SOLUTION from FEATURE_WORKTREE. Write `EVIDENCE_ROOT/baseline/baseline-nullable-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build succeeded/failed, warning-as-error count). Acceptance: artifact exists with all four fields populated.
- [x] [P0-T9] Capture baseline test + coverage for the UtilitiesCS.Test suite BEFORE any change. Run `vstest.console.exe "C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll" /EnableCodeCoverage` (build the solution first if the assembly is stale). Write `EVIDENCE_ROOT/baseline/baseline-tests-coverage.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including passed/failed test counts AND the numeric coverage headline (line coverage percent, and branch coverage percent if emitted). Acceptance: artifact exists with all four fields and a numeric (non-placeholder) coverage percent recorded.

### Phase 1 — Retarget SmartSerializable Infrastructure Tests

Behavioral constraint (preflight correction): `ScoDictionaryNew<>` implements `ISmartSerializable<>` (via
`IScoDictionaryNew`), so `IsSmartSerializable(...)` returns TRUE for it, whereas the retired
`ScoDictionary<>` returned FALSE. Retarget ONLY positive round-trip stand-ins to `ScoDictionaryNew<>`.
Negative `IsSmartSerializable == false` stand-ins must retarget to a first-party non-smart-serializable
type (`ConcurrentObservableCollection<int>`), or be deleted where an equivalent negative test already
exists. No negative assertion may be retargeted to `ScoDictionaryNew<>`.

- [x] [P1-T1] In `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableBase_Tests.cs`, replace the three POSITIVE round-trip stand-in usages `ScoDictionary<string, int>` with `ScoDictionaryNew<string, int>` at the current locations of research-noted lines 52, 58, and 73 (executor verifies exact lines on disk; they may have drifted). Acceptance: `rg "\bScoDictionary<"` against this file returns zero matches for the bare `ScoDictionary<` type (only `ScoDictionaryNew<` remains), and the file still compiles.
- [x] [P1-T2] In `SmartSerializableBase_Tests.cs`, verify/adjust `using` directives: remove any now-unused `using` that pulled in the old class, and ensure `ScoDictionaryNew` (namespace `UtilitiesCS.ReusableTypeClasses`) is in scope via `using UtilitiesCS.ReusableTypeClasses;`. Acceptance: no unused-using analyzer warning for this file and no missing-type compile error.
- [x] [P1-T3] In `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableNonTyped_Tests.cs`, replace the three POSITIVE round-trip stand-in usages `ScoDictionary<string, int>` with `ScoDictionaryNew<string, int>` at the current locations of research-noted lines 76, 82, and 96 (executor verifies exact lines on disk). Do NOT touch the negative usages at lines 24 and 50 in this task. Acceptance: the three positive usages now read `ScoDictionaryNew<string, int>` and the file still compiles.
- [x] [P1-T4] In `SmartSerializableNonTyped_Tests.cs`, retarget the two NEGATIVE stand-in usages `ScoDictionary<string, int>` at the current locations of research-noted lines 24 and 50 — inside methods `IsSmartSerializable_ScoDictionaryInstance_ReturnsFalse` and `IsSmartSerializable_TypeOverload_ScoDictionary_ReturnsFalse` — to `ConcurrentObservableCollection<int>` (a first-party type that does NOT implement `ISmartSerializable<>`). Do NOT use `ScoDictionaryNew` for these two. Acceptance: `rg "\bScoDictionary<"` against this file returns zero bare `ScoDictionary<` matches (only `ScoDictionaryNew<` and `ConcurrentObservableCollection<` remain); both `IsSmartSerializable_..._ReturnsFalse` tests still assert `.Should().BeFalse()` and pass.
- [x] [P1-T5] In `SmartSerializableNonTyped_Tests.cs`, update the stale comment text at research-noted lines 23 and 49 (currently "ScoDictionary does not implement ISmartSerializable<>") to name `ConcurrentObservableCollection` as the non-smart-serializable stand-in for those two negative tests, keeping the statement accurate. Acceptance: those two comment lines reference `ConcurrentObservableCollection`; no comment in this file names the retired `ScoDictionary` stand-in usage and none of these two comment lines names `ScoDictionaryNew`.
- [x] [P1-T6] In `SmartSerializableNonTyped_Tests.cs`, verify/adjust `using` directives: keep `using UtilitiesCS.ReusableTypeClasses;` (needed for `ScoDictionaryNew`); add `using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection;` if `ConcurrentObservableCollection<>` is not already in scope; remove any now-unused `using` that pulled in the old class. Acceptance: no unused-using analyzer warning for this file and no missing-type compile error (both `ScoDictionaryNew` and `ConcurrentObservableCollection` resolve).
- [x] [P1-T7] In `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableStatic_Tests.cs`, DELETE the entire `IsSmartSerializable_ScoDictionary_ReturnsFalse` test method (research-noted approx lines 25-36; the `typeof(ScoDictionary<string, int>)` stand-in is at research-noted line 29). Do NOT retarget it — its negative coverage is already provided by the existing `IsSmartSerializable_ConcurrentObservableCollection_ReturnsFalse` method (research-noted line ~39). Acceptance: `rg "\bScoDictionary<"` against this file returns zero bare `ScoDictionary<` matches; the method `IsSmartSerializable_ScoDictionary_ReturnsFalse` no longer exists; the existing `IsSmartSerializable_ConcurrentObservableCollection_ReturnsFalse` method remains unchanged.
- [x] [P1-T8] In `SmartSerializableStatic_Tests.cs`, verify `using` directives after the method deletion: the existing `using ...Concurrent.Observable.Collection;` at research-noted line 5 MUST remain (still required by `IsSmartSerializable_ConcurrentObservableCollection_ReturnsFalse`); remove only usings that became unused as a result of the deletion. Acceptance: no unused-using analyzer warning for this file; no missing-type compile error; the `Concurrent.Observable.Collection` using is still present.

### Phase 2 — Delete Obsolete ScoDictionary Tests and csproj Entries

- [x] [P2-T1] Delete `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/UtilitiesCS.Test/ReusableTypeClasses/SCODictionary_Tests.cs`. Acceptance: the file no longer exists on disk.
- [x] [P2-T2] Delete `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/UtilitiesCS.Test/ReusableTypeClasses/SCODictionary_Additional_Tests.cs`. Acceptance: the file no longer exists on disk.
- [x] [P2-T3] In `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/UtilitiesCS.Test/UtilitiesCS.Test.csproj`, remove the `<Compile Include="ReusableTypeClasses\SCODictionary_Tests.cs" />` entry (research-noted line ~380; executor verifies exact line on disk). Acceptance: `rg "SCODictionary_Tests.cs"` against this csproj returns zero matches.
- [x] [P2-T4] In `UtilitiesCS.Test.csproj`, remove the `<Compile Include="ReusableTypeClasses\SCODictionary_Additional_Tests.cs" />` entry (research-noted line ~381; executor verifies exact line on disk). Acceptance: `rg "SCODictionary_Additional_Tests.cs"` against this csproj returns zero matches.

### Phase 3 — Delete Legacy Production Class and csproj Entry

- [x] [P3-T1] Delete `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/SCODictionary.cs`. Acceptance: the file no longer exists on disk.
- [x] [P3-T2] In `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/UtilitiesCS/UtilitiesCS.csproj`, remove the single `<Compile Include="ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs" />` entry (research-noted line ~1048; executor verifies exact line on disk). Acceptance: `rg "SCODictionary.cs"` against this csproj returns zero matches.

### Phase 4 — Update Stale Comments

- [x] [P4-T1] In `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs`, update the stale comment at research-noted lines ~239-240 that names the old Swordfish-based `ScoDictionary` so it no longer implies a live legacy binding (name `ScoDictionaryNew` or remove the obsolete reference). Acceptance: this file contains no comment implying the retired `ScoDictionary` class still exists; no code change is made.
- [x] [P4-T2] In `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/UtilitiesCS.Test/EmailIntelligence/FolderRemapController_Tests.cs`, update the stale doc comment at research-noted line ~162 that names the old `ScoDictionary` to reflect `ScoDictionaryNew` (the code at ~line 169 already constructs `new ScoDictionaryNew<string, string>()`). Acceptance: the comment references `ScoDictionaryNew`; no code change is made.
- [x] [P4-T3] In `C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/UtilitiesCS.Test/EmailIntelligence/SubjectMapEncoder_Tests.cs`, update the stale doc comments at research-noted lines ~91 and ~102 that name the old `ScoDictionary` behavior to reference `ScoDictionaryNew` or mark them as historical without implying a live class. Acceptance: those comments no longer imply the retired `ScoDictionary` class is live; no code change is made.

### Phase 5 — Final QC Toolchain Loop and Coverage Verification

Run the full C# toolchain in order. If any step fails or changes files, fix and restart the loop from P5-T1. A fresh msbuild invocation is required so removed `<Compile Include>` items are picked up. All command tasks below are unconditional; `EXIT_CODE: SKIPPED` is not a valid passing outcome.

- [x] [P5-T1] Format: run `dotnet tool run csharpier .` from FEATURE_WORKTREE. Write `EVIDENCE_ROOT/qa-gates/final-csharpier.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (files formatted / no drift). Acceptance: exit code 0 and artifact complete; if any file was reformatted, restart the loop at P5-T1.
- [x] [P5-T2] Lint/analyzers: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` against SOLUTION from FEATURE_WORKTREE. Write `EVIDENCE_ROOT/qa-gates/final-analyzer-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build result, 0 analyzer errors). Acceptance: exit code 0, build succeeded, zero analyzer errors; artifact complete.
- [x] [P5-T3] Type-check: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` against SOLUTION from FEATURE_WORKTREE. Write `EVIDENCE_ROOT/qa-gates/final-nullable-build.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build result, zero warnings-as-errors). Acceptance: exit code 0, build succeeded, zero warnings-as-errors; artifact complete.
- [x] [P5-T4] Test + coverage: run `vstest.console.exe "C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll" /EnableCodeCoverage`. Write `EVIDENCE_ROOT/qa-gates/final-tests-coverage.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including passed/failed counts and the numeric post-change coverage headline (line percent, branch percent if emitted). Acceptance: exit code 0, zero test failures, and a numeric (non-placeholder) coverage percent recorded.
- [x] [P5-T5] Assert on-disk JSON compatibility coverage remains green: confirm the P5-T4 run reports `UtilitiesCS.Test/ReusableTypeClasses/SerializableNew/ScoDictionaryNew_OnDiskCompatibility_Tests.cs` test cases all passed. Write `EVIDENCE_ROOT/regression-testing/ondisk-compat-green.md` with `Timestamp:`, the test class name, and pass counts for its methods. Acceptance: all `ScoDictionaryNew_OnDiskCompatibility_Tests` methods are recorded as passed.
- [x] [P5-T6] Residual-binding verification: run `rg "\bScoDictionary<" C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/UtilitiesCS C:/Users/DanMoisan/repos/TaskMaster-wt/legacy-scodictionary-removal-315/UtilitiesCS.Test` and confirm no live (non-commented) reference to the bare legacy `ScoDictionary<` type remains, and that `SCODictionary.cs` no longer appears in either csproj. Write `EVIDENCE_ROOT/qa-gates/residual-binding-check.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (match list or `none`, plus csproj grep results). Acceptance: no live `ScoDictionary<` reference in production or test code attributable to the retired class; `SCODictionary.cs` absent from both csproj files.
- [x] [P5-T7] Coverage delta / no-regression verification. Compare baseline coverage (P0-T9) against post-change coverage (P5-T4) and evaluate changed-line coverage. Write `EVIDENCE_ROOT/qa-gates/coverage-delta.md` with `Timestamp:` and three explicit numeric values: `Baseline coverage:`, `Post-change coverage:`, and `Changed/new-code coverage:`, plus a `Verdict:` line. Note that deleting the obsolete class and its tests removes both from the denominator; retargeted files retain their existing coverage of the generic infrastructure. Acceptance: no coverage regression on changed lines; the three numeric values are recorded and the verdict is PASS. If required coverage values are unavailable, verdict is remediation-required (not PASS).

## Acceptance Criteria Traceability (spec.md + user-story.md)

- AC1 (`SCODictionary.cs` removed and its `<Compile Include>` gone from `UtilitiesCS.csproj`): P3-T1, P3-T2.
- AC2 (no production/test code references legacy `ScoDictionary<>` or its Swordfish binding): P1-T1..P1-T8, P2-T1..P2-T4, P3-T1..P3-T2, P4-T1..P4-T3, P5-T6.
- AC3 (generic serialization/wrapper stand-in coverage preserved by retargeting to a first-party type for positives and to `ConcurrentObservableCollection<int>` for negatives): P1-T1..P1-T8, P5-T4.
- AC4 (on-disk JSON compatibility preserved for retargeted payloads): P5-T5 (existing `ScoDictionaryNew_OnDiskCompatibility_Tests.cs` stays green).
- AC5 (full C# toolchain passes with zero test regressions and no changed-line coverage regression): P5-T1..P5-T4, P5-T7.

## Preflight

Return this plan for validation-only preflight through atomic-executor using
`DIRECTIVE: PREFLIGHT VALIDATION ONLY`. Reuse this exact plan file path across all revision
iterations; do not create timestamped siblings. Expected signal: `PREFLIGHT: ALL CLEAR` or
`PREFLIGHT: REVISIONS REQUIRED`.
