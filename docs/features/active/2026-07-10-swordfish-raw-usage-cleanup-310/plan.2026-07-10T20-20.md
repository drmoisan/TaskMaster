# swordfish-raw-usage-cleanup - Atomic Plan

- **Issue:** #310
- **Parent:** epic `swordfish-removal`, child F4, wave 0
- **Owner:** drmoisan
- **Work Mode:** full-feature
- **Last Updated:** 2026-07-10T20-20
- **Status:** Ready for preflight
- **Version:** 0.2
- **Integration branch:** epic/swordfish-removal-integration
- **Working branch:** epic-child/swordfish-raw-usage-cleanup

## Required References (authoritative; do not duplicate their content here)

- `CLAUDE.md` (standing instructions + C# toolchain order)
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md` (C# code + unit-test policy)
- Feature docs: `spec.md`, `user-story.md`, `issue.md`,
  `research/swap-target-decision-record.md`

All work must comply with these policies. This plan encodes three disjoint,
behavior-neutral C# edits only. No new production types, no new tests, no
dependency/solution/project-reference changes.

## Evidence Location Contract (canonical, non-overridable)

All evidence artifacts MUST be written under the canonical feature scheme:

- Baseline: `docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/evidence/baseline/`
- QA gates: `docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/evidence/qa-gates/`
- Regression: `docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/evidence/regression-testing/`
- Other: `docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/evidence/other/`

No `artifacts/` evidence path may be used. Each command-step artifact MUST record
`Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Coverage artifacts MUST
record numeric percentages, not placeholders.

## Scope Boundary (MUST respect; verified in Phase 4)

- Do NOT modify Sco* lineage classes (`ScoDictionary`, `ScoCollection`, `ScoStack`,
  `ScoSortedDictionary`) or their consumers beyond the `KbdActions` `_list` swap.
- Do NOT delete the `UtilitiesSwordfish` project, remove any `ProjectReference`, or edit
  `TaskMaster.sln`.
- Do NOT migrate interfaces (`IScoCollection`, `IScoCollection2`).
- Do NOT add any production type, test, package, or project reference.

## Acceptance Criteria Map (checked off by executor against spec.md)

- AC1 -> Phase 1 (KbdActions swap) + Phase 5 (regression net) + Phase 6 (toolchain).
- AC2 -> Phase 2 (three unused-using removals) + Phase 2 rebuild + Phase 6 build gates.
- AC3 -> Phase 3 (TraceUtility literal deletion) + Phase 6 build gates.
- AC4 -> Phase 4 (scope-boundary verification).
- AC5 -> Phase 6 (full C# toolchain + numeric coverage + no-regression-on-changed-lines).

---

### Phase 0 — Baseline Capture and Policy Compliance

- [x] [P0-T1] Read the policy files in the mandated policy-compliance order (`CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and the four feature documents (`spec.md`, `user-story.md`, `issue.md`, `research/swap-target-decision-record.md`), then write the read-evidence artifact.
  - Acceptance: `evidence/baseline/phase0-instructions-read.md` exists and contains `Timestamp:`, `Policy Order:` (the exact ordered list), and an explicit list of every file read.

- [x] [P0-T2] Capture the baseline CSharpier format state by running `dotnet tool run csharpier --check .` (or `csharpier --check .`) from the worktree root and recording the result.
  - Acceptance: `evidence/baseline/baseline-csharpier.2026-07-10T20-20.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (clean/needs-formatting and file count).

- [x] [P0-T3] Capture the baseline analyzer build by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and recording the result.
  - Acceptance: `evidence/baseline/baseline-analyzer-build.2026-07-10T20-20.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (build succeeded/failed, warning + error counts).

- [x] [P0-T4] Capture the baseline nullable / warnings-as-errors build by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and recording the result.
  - Acceptance: `evidence/baseline/baseline-nullable-build.2026-07-10T20-20.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (build succeeded/failed, warning + error counts).

- [x] [P0-T5] Capture the baseline MSTest run in coverage mode by running `vstest.console.exe <repo-standard test assembly set> /EnableCodeCoverage`, where the assembly set MUST include `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` and `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` and MUST be recorded verbatim for reuse in Phase 6.
  - Acceptance: `evidence/baseline/baseline-mstest-coverage.2026-07-10T20-20.md` records `Timestamp:`, `Command:` (exact assembly set), `EXIT_CODE:`, and `Output Summary:` including numeric passed/failed test counts AND the numeric coverage headline: repo-wide line coverage %, plus the `KbdActions` (QuickFiler) and `TraceUtility`/`FlagDetails`/`FolderRemapController` (UtilitiesCS) affected-module coverage numbers where reported.

- [x] [P0-T6] Capture the pre-change Swordfish reference inventory for the five target files by running `rg -n "Swordfish\.NET" QuickFiler/Controllers/KbdActions.cs QuickFiler/Controllers/KeyboardHandler.cs UtilitiesCS/EmailIntelligence/Flags/FlagDetails.cs UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/FolderRemapController.cs UtilitiesCS/HelperClasses/Logging/TraceUtility.cs`.
  - Acceptance: `evidence/baseline/baseline-swordfish-reference-inventory.2026-07-10T20-20.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` listing each current `Swordfish.NET.Collections` using at `KbdActions.cs:10`, `KeyboardHandler.cs:17`, `FlagDetails.cs:13`, `FolderRemapController.cs:10`, plus the two `UtilitiesSwordfish.NET.*` literals at `TraceUtility.cs:392-393`.

---

### Phase 1 — Work Item 1: KbdActions Collection Type Swap (AC1)

File: `QuickFiler/Controllers/KbdActions.cs`. Behavior-neutral swap of the private `_list`
field from the raw Swordfish `ConcurrentObservableCollection<UClass>` to
`System.Collections.Generic.List<UClass>`. `List<T>` natively provides `Add`,
`RemoveAt(int)`, `GetEnumerator`, LINQ, and the load-bearing `FindIndex(Predicate<T>)`
(call sites at lines 81 and 126). `using System.Collections.Generic;` (line 4) already
supplies `List<T>`.

- [x] [P1-T1] Re-type the private field declaration at `QuickFiler/Controllers/KbdActions.cs:32` from `private ConcurrentObservableCollection<UClass> _list = new();` to `private List<UClass> _list = new();`. The target-typed `new()` initializer is retained and infers `List<UClass>`.
  - Acceptance: line 32 declares `_list` as `List<UClass>`; no other line changes in this edit.

- [x] [P1-T2] Update the parameterless constructor body at `QuickFiler/Controllers/KbdActions.cs:24` from `_list = new ConcurrentObservableCollection<UClass>();` to `_list = new List<UClass>();`.
  - Acceptance: line 24 assigns `_list = new List<UClass>();`; constructor signature `public KbdActions()` is unchanged.

- [x] [P1-T3] Update the `IEnumerable<UClass>` constructor body at `QuickFiler/Controllers/KbdActions.cs:29` from `_list = new ConcurrentObservableCollection<UClass>(list);` to `_list = new List<UClass>(list);`.
  - Acceptance: line 29 assigns `_list = new List<UClass>(list);`; constructor signature `public KbdActions(IEnumerable<UClass> list)` is unchanged.

- [x] [P1-T4] Remove the now-unreferenced `using Swordfish.NET.Collections;` directive at `QuickFiler/Controllers/KbdActions.cs:10`.
  - Acceptance: the file contains no `Swordfish.NET` reference; `rg -n "Swordfish\.NET" QuickFiler/Controllers/KbdActions.cs` returns no matches (`EXIT_CODE: 1`).

- [x] [P1-T5] Verify `KbdActions.cs` compiles after the swap by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and recording the result.
  - Acceptance: `evidence/qa-gates/phase1-kbdactions-build.2026-07-10T20-20.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` confirming no unresolved-type or analyzer regression for `KbdActions.cs`.

---

### Phase 2 — Work Item 2: Unused using Directive Removal (AC2)

Three files reference `Swordfish.NET.Collections` only through an otherwise-unused `using`.
The namespace exposes no extension methods, so removal cannot cause silent behavior loss; a
clean rebuild is the proof each directive was genuinely unused.

- [x] [P2-T1] Remove the `using Swordfish.NET.Collections;` directive at `QuickFiler/Controllers/KeyboardHandler.cs:17`. No other line changes.
  - Acceptance: `rg -n "Swordfish\.NET" QuickFiler/Controllers/KeyboardHandler.cs` returns no matches (`EXIT_CODE: 1`).

- [x] [P2-T2] Remove the `using Swordfish.NET.Collections;` directive at `UtilitiesCS/EmailIntelligence/Flags/FlagDetails.cs:13`. No other line changes.
  - Acceptance: `rg -n "Swordfish\.NET" UtilitiesCS/EmailIntelligence/Flags/FlagDetails.cs` returns no matches (`EXIT_CODE: 1`).

- [x] [P2-T3] Remove the `using Swordfish.NET.Collections;` directive at `UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/FolderRemapController.cs:10`. No other line changes.
  - Acceptance: `rg -n "Swordfish\.NET" UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/FolderRemapController.cs` returns no matches (`EXIT_CODE: 1`).

- [x] [P2-T4] Verify all three files still compile (proving each using was unused) by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and recording the result.
  - Acceptance: `evidence/qa-gates/phase2-unused-using-build.2026-07-10T20-20.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` confirming the solution rebuilds clean with no unresolved-reference regression in the three files.

---

### Phase 3 — Work Item 3: TraceUtility Stale Literal Deletion (AC3)

File: `UtilitiesCS/HelperClasses/Logging/TraceUtility.cs`. Delete the two dead
`_projectNames` trace-filter literals (assembly simple-names of Swordfish projects the epic
removes in F5). `List<string>.Contains`-style membership is per-element independent, so
deletion is behavior-neutral for every surviving filter name.

- [x] [P3-T1] Delete the literal line `"UtilitiesSwordfish.NET.General",` at `UtilitiesCS/HelperClasses/Logging/TraceUtility.cs:392`. Surrounding entries (`"TaskMaster",` above, `"Tags.Test",` below) are unchanged.
  - Acceptance: the `"UtilitiesSwordfish.NET.General"` literal no longer appears in the file.

- [x] [P3-T2] Delete the literal line `"UtilitiesSwordfish.NET.Test",` (originally line 393) from the same `_projectNames` initializer. No other entry changes.
  - Acceptance: `rg -n "UtilitiesSwordfish\.NET" UtilitiesCS/HelperClasses/Logging/TraceUtility.cs` returns no matches (`EXIT_CODE: 1`); both target literals are gone and the collection initializer remains syntactically valid.

- [x] [P3-T3] Verify `TraceUtility.cs` compiles after the deletions by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and recording the result.
  - Acceptance: `evidence/qa-gates/phase3-traceutility-build.2026-07-10T20-20.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` confirming no build regression.

---

### Phase 4 — Scope-Boundary and Reference Verification (AC4)

- [x] [P4-T1] Verify the change set touches only the five permitted files by running `git diff --name-only` against the base and confirming the modified-file list is exactly `QuickFiler/Controllers/KbdActions.cs`, `QuickFiler/Controllers/KeyboardHandler.cs`, `UtilitiesCS/EmailIntelligence/Flags/FlagDetails.cs`, `UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/FolderRemapController.cs`, and `UtilitiesCS/HelperClasses/Logging/TraceUtility.cs` (plus feature-doc/evidence files).
  - Acceptance: `evidence/other/scope-boundary-diff.2026-07-10T20-20.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` confirming no Sco* lineage class, no `UtilitiesSwordfish` project file, no `ProjectReference`, and no `TaskMaster.sln` entry appears in the diff.

- [x] [P4-T2] Verify the four using-removal targets and the KbdActions swap file are Swordfish-free by running `rg -n "Swordfish\.NET" QuickFiler/Controllers/KbdActions.cs QuickFiler/Controllers/KeyboardHandler.cs UtilitiesCS/EmailIntelligence/Flags/FlagDetails.cs UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/FolderRemapController.cs UtilitiesCS/HelperClasses/Logging/TraceUtility.cs`.
  - Acceptance: `evidence/other/post-change-swordfish-inventory.2026-07-10T20-20.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 1` (no matches), and `Output Summary:` confirming all five files reference no `Swordfish.NET` type or literal.

---

### Phase 5 — Regression Net (AC1)

- [x] [P5-T1] Run the existing, unchanged `KbdActions` regression tests (`QuickFiler.Test/Controllers/KbdActionsTests.cs` and `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs`) by running `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~KbdActions"` and confirming all pin the `FindIndex`/`Add`/`RemoveAt` branches green with no test-source edits.
  - Acceptance: `evidence/regression-testing/kbdactions-regression.2026-07-10T20-20.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with numeric passed count and zero failures; both test files remain unmodified (`git diff --name-only` shows neither).

---

### Phase 6 — Final C# QC Toolchain and Acceptance-Criteria Mapping (AC5)

Run the full C# toolchain in order. If any step changes files or fails, restart from step 1.

- [x] [P6-T1] Run formatting: `dotnet tool run csharpier .` (or `csharpier .`). If it rewrites any file, re-run and restart the loop.
  - Acceptance: `evidence/qa-gates/final-csharpier.2026-07-10T20-20.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` confirming zero files reformatted on the passing pass.

- [x] [P6-T2] Run analyzers: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  - Acceptance: `evidence/qa-gates/final-analyzer-build.2026-07-10T20-20.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` (build succeeded, zero analyzer errors).

- [x] [P6-T3] Run type-check/nullable: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  - Acceptance: `evidence/qa-gates/final-nullable-build.2026-07-10T20-20.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` (build succeeded, zero nullable/warnings-as-errors failures).

- [x] [P6-T4] Run tests with coverage: `vstest.console.exe <same repo-standard test assembly set used in P0-T5> /EnableCodeCoverage`.
  - Acceptance: `evidence/qa-gates/final-mstest-coverage.2026-07-10T20-20.md` records `Timestamp:`, `Command:` (identical assembly set to P0-T5), `EXIT_CODE: 0`, and `Output Summary:` with numeric passed/failed counts and post-change coverage headline: repo-wide line coverage % plus the affected `KbdActions`/`UtilitiesCS` module numbers.

- [x] [P6-T5] Compute the coverage delta and no-regression-on-changed-lines check by comparing the P0-T5 baseline numbers to the P6-T4 post-change numbers for the changed files (`KbdActions.cs`, `KeyboardHandler.cs`, `FlagDetails.cs`, `FolderRemapController.cs`, `TraceUtility.cs`).
  - Acceptance: `evidence/qa-gates/coverage-delta.2026-07-10T20-20.md` records `Timestamp:`, baseline coverage %, post-change coverage %, changed/affected-module coverage %, and an explicit `No regression on changed lines: PASS/FAIL` verdict. Outcome must be remediation-required (not PASS) if any changed-line coverage regresses.

- [x] [P6-T6] Map completed phases to `spec.md` Acceptance Criteria AC1-AC5 and record the mapping for the executor to check off.
  - Acceptance: `evidence/other/ac-mapping.2026-07-10T20-20.md` records `Timestamp:` and, for each of AC1-AC5, the phase/task IDs and evidence-artifact paths that satisfy it, with no AC left unmapped.

## Notes

- No new unit tests are authored: all three work items are behavior-neutral. The existing
  `KbdActions` tests (Phase 5) are the regression net and MUST remain green unchanged.
- The manifest "Shared Design" note naming a clean repo `ConcurrentObservableCollection` is
  inaccurate on the integration base; the resolved swap target is `List<UClass>` per the
  decision record. Any maintainer override redirecting `KbdActions` onto an F2-introduced
  clean concurrent-observable type would require re-planning (F4 -> wave 1, `depends_on: [F2]`).
