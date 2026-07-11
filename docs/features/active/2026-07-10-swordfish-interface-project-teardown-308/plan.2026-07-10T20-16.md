# swordfish-interface-project-teardown — Plan

- **Issue:** #308
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/308
- **Epic:** swordfish-removal (child feature F5, wave 1; depends on F1-F4)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-10T20-16
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-feature
- **Mode:** PREPARATION (plan produced now; executed later by epic-orchestrator AFTER F1-F4 merge into the integration branch). This plan targets the POST-F1-F4 end-state.
- **Authoritative inputs:** `spec.md` (AC-1..AC-16), `user-story.md`, `issue.md`, `research/2026-07-10T20-45-swordfish-teardown-research.md` (WI-0..WI-4 with exact file paths).

## Scope Summary

F5 removes the structural Swordfish surface remaining after F1-F4 eliminate all first-party `Swordfish.NET.*` type usage:

- **WI-0** — Preflight precondition (execution-time gate): assert F1-F4 have landed.
- **WI-1** — Remove three interfaces (`IScoCollection.cs`, `IScoCollection2.cs`, `ISubjectMapSco.cs`) and the dead `QfcExplorerController.UpdateForMove` method.
- **WI-2** — Remove the NINE `UtilitiesSwordfish.NET.General.csproj` `ProjectReference` blocks (eight named plus `TaskVisualization.Test.csproj`).
- **WI-3** — Remove `TaskMaster.sln` `Project(...)` declarations AND `GlobalSection(ProjectConfigurationPlatforms)` rows for both GUIDs; `git rm -r` both project folders.
- **WI-4** — Remove the three direct-Swordfish test files.

Sequencing keeps the build green after each phase: interfaces and tests (the last consumers of Swordfish types) are removed first (Phases 1-2), then the `ProjectReference` entries (Phase 3), then the solution entries and folders (Phase 4). WI-2/WI-3 depend on F1-F4 having removed production Swordfish type usage (asserted by the WI-0 gate in Phase 0).

## Coverage Note (F5 owes no backfill)

Removing the three direct-Swordfish test files removes numerator and denominator together: their subject types (`Swordfish.NET.*`) are deleted wholesale with the `UtilitiesSwordfish` assembly. No surviving first-party line loses coverage from these removals, so F5 owes no coverage backfill. Regression coverage of the clean collection base is F2-owned (verified, not authored, by F5 — AC-12).

## Evidence Location Invariant

All evidence artifacts resolve to `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/<kind>/` (`baseline`, `qa-gates`, `regression-testing`, `other`). No `artifacts/` evidence path is used. Each command-step artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; baseline and final-QC test-step artifacts record numeric coverage headline values.

### Phase 0 — Baseline Capture and Policy Compliance

- [ ] [P0-T1] Read the four policy documents in the required policy-compliance order (`CLAUDE.md`; `.claude/rules/general-code-change.md`; `.claude/rules/general-unit-test.md`; `.claude/rules/csharp.md`) and record a policy-read evidence artifact at `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/baseline/phase0-instructions-read.md`.
  - Acceptance: artifact exists and contains `Timestamp:`, `Policy Order:`, and an explicit list of the four files read in order.
- [ ] [P0-T2] Verify the WI-0 preflight precondition at REAL EXECUTION TIME (post-F1-F4 merge against the integration branch end-state). Assert (a) `rg "Swordfish" -g "*.cs"` over production source (research category A) returns zero matches outside F5's own interface/test targets, and (b) `rg "UtilitiesSwordfish\.NET\.(General|Test)" UtilitiesCS\HelperClasses\Logging\TraceUtility.cs` returns zero matches. If either assertion fails, HALT: the upstream feature (F1-F4) has not landed and F5 must not absorb upstream work. Record the search output to `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/regression-testing/wi0-preflight-precondition.md`. Delivers AC-1, AC-2.
  - Precondition: this gate is evaluated ONLY at real execution against the post-F1-F4 end-state. In the current pre-merge worktree it is expected to fail (F1-F4 not yet merged); the executor must NOT evaluate it at plan preflight and must treat a failure at execution as a HALT condition, not a plan defect.
  - Acceptance: artifact records both `rg` commands, `EXIT_CODE:`, and `Output Summary:` showing zero category-A production matches and zero `TraceUtility.cs` literal matches; OR a HALT record if the assertions fail at execution.
- [ ] [P0-T3] Capture the CSharpier baseline: run `dotnet tool run csharpier .` and record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/baseline/baseline-csharpier.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (formatted/unchanged file count).
- [ ] [P0-T4] Capture the analyzer baseline: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/baseline/baseline-analyzers.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build result, warning/error counts).
- [ ] [P0-T5] Capture the nullable/type-check baseline: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/baseline/baseline-nullable.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build result, nullable warning count).
- [ ] [P0-T6] Capture the test + coverage baseline: run `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` and record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/baseline/baseline-vstest-coverage.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including numeric baseline repo-wide line-coverage percent (testable denominator) and pass/fail totals.

### Phase 1 — WI-1 Interface and Dead-Method Removal

- [ ] [P1-T1] Delete `UtilitiesCS\Interfaces\IReusableTypeClasses\IScoCollection.cs` (`IScoCollection<T>`, `using Swordfish.NET.Collections;`) via `git rm`. Delivers AC-3.
  - Acceptance: file no longer exists; `git status` shows the deletion staged.
- [ ] [P1-T2] Delete `UtilitiesCS\Interfaces\IReusableTypeClasses\IScoCollection2.cs` (`IScoCollection2<T>`, zero implementers/consumers) via `git rm`. Delivers AC-4.
  - Acceptance: file no longer exists; `git status` shows the deletion staged.
- [ ] [P1-T3] Delete `UtilitiesCS\Interfaces\IToDo\ISubjectMapSco.cs` (dead `ISubjectMapSco : IScoCollection<SubjectMapEntry>`, no implementer) via `git rm`. Delivers AC-5.
  - Acceptance: file no longer exists; `git status` shows the deletion staged.
- [ ] [P1-T4] Remove the dead `private static void UpdateForMove(...)` method (the sole `ISubjectMapSco` reference, no call site) from `QuickFiler\Controllers\QfcExplorerController.cs` (research: lines ~271-280), including its `ISubjectMapSco` parameter dependency. Delivers AC-6.
  - Acceptance: `rg "UpdateForMove" QuickFiler\Controllers\QfcExplorerController.cs` returns zero matches; no dangling `ISubjectMapSco` symbol remains.
- [ ] [P1-T5] Verify no dangling interface symbols remain repo-wide: `rg "IScoCollection2?\b|ISubjectMapSco" -g "*.cs"` returns zero matches. Record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/regression-testing/wi1-interface-symbol-zero.md`.
  - Acceptance: artifact records the `rg` command, `EXIT_CODE:`, and `Output Summary:` showing zero matches.

### Phase 2 — WI-4 Direct-Swordfish Test Removal

- [ ] [P2-T1] Delete `UtilitiesCS.Test\ReusableTypeClasses\ObservableDictionary_Tests.cs` (exercises `Swordfish.NET.Collections.ObservableDictionary<TKey,TValue>`) via `git rm`. Delivers part of AC-11.
  - Acceptance: file no longer exists; `git status` shows the deletion staged.
- [ ] [P2-T2] Delete `UtilitiesCS.Test\ReusableTypeClasses\Concurrent\Observable\Collection\ConcurrentObservableCollectionSenderTests.cs` (sender-identity regression against `Swordfish.NET.Collections.ConcurrentObservableCollection<int>`) via `git rm`. Delivers part of AC-11.
  - Acceptance: file no longer exists; `git status` shows the deletion staged.
- [ ] [P2-T3] Delete `UtilitiesCS.Test\ReusableTypeClasses\Concurrent\Observable\Collection\ConcurrentObservableCollectionLockRecursionTests.cs` (LockRecursion regression against `Swordfish.NET.Collections.ConcurrentObservableCollection<>`) via `git rm`. Delivers part of AC-11.
  - Acceptance: file no longer exists; `git status` shows the deletion staged.
- [ ] [P2-T4] Verify no residual direct-Swordfish test usage: `rg "using Swordfish" -g "*.cs"` under `UtilitiesCS.Test\` returns zero matches. Record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/regression-testing/wi4-test-swordfish-zero.md`.
  - Acceptance: artifact records the `rg` command, `EXIT_CODE:`, and `Output Summary:` showing zero matches; completes AC-11.
- [ ] [P2-T5] Inspect F2 deliverables for equivalent sender-identity and lock-recursion regression coverage against the clean collection base; if absent, open a new GitHub issue (F5 does NOT author the coverage). Record the finding (and any issue URL) to `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/other/f2-regression-coverage-confirmation.md`. Delivers AC-12.
  - Acceptance: artifact records `Timestamp:` and a definite finding (coverage present, with file references; OR coverage absent, with the raised issue URL).

### Phase 3 — WI-2 ProjectReference Removal (nine csprojs)

- [ ] [P3-T1] Confirm the `Tags` / `TaskVisualization` / `TaskVisualization.Test` references are stale before removal: `rg "Sco|IScoCollection|ISubjectMapSco|Swordfish|ConcurrentObservable" -g "*.cs"` under `Tags\`, `TaskVisualization\`, and `TaskVisualization.Test\` returns zero matches. Record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/regression-testing/wi2-stale-reference-search.md`. Provides the AC-7 stale-reference evidence.
  - Acceptance: artifact records the `rg` command(s), `EXIT_CODE:`, and `Output Summary:` showing zero matches under all three paths.
- [ ] [P3-T2] Remove the `..\UtilitiesSwordfish\UtilitiesSwordfish.NET.General.csproj` `ProjectReference` block from `UtilitiesCS\UtilitiesCS.csproj` (research: ~1083-1086).
  - Acceptance: `rg "UtilitiesSwordfish\.NET\.General\.csproj" UtilitiesCS\UtilitiesCS.csproj` returns zero matches.
- [ ] [P3-T3] Remove the `UtilitiesSwordfish.NET.General.csproj` `ProjectReference` block from `UtilitiesCS.Test\UtilitiesCS.Test.csproj` (research: ~854-857).
  - Acceptance: `rg "UtilitiesSwordfish\.NET\.General\.csproj" UtilitiesCS.Test\UtilitiesCS.Test.csproj` returns zero matches.
- [ ] [P3-T4] Remove the `UtilitiesSwordfish.NET.General.csproj` `ProjectReference` block from `TaskMaster\TaskMaster.csproj` (research: ~510-513).
  - Acceptance: `rg "UtilitiesSwordfish\.NET\.General\.csproj" TaskMaster\TaskMaster.csproj` returns zero matches.
- [ ] [P3-T5] Remove the `UtilitiesSwordfish.NET.General.csproj` `ProjectReference` block from `TaskMaster.Test\TaskMaster.Test.csproj` (research: ~323-326).
  - Acceptance: `rg "UtilitiesSwordfish\.NET\.General\.csproj" TaskMaster.Test\TaskMaster.Test.csproj` returns zero matches.
- [ ] [P3-T6] Remove the `UtilitiesSwordfish.NET.General.csproj` `ProjectReference` block from `QuickFiler\QuickFiler.csproj` (research: ~451-454).
  - Acceptance: `rg "UtilitiesSwordfish\.NET\.General\.csproj" QuickFiler\QuickFiler.csproj` returns zero matches.
- [ ] [P3-T7] Remove the `UtilitiesSwordfish.NET.General.csproj` `ProjectReference` block from `ToDoModel\ToDoModel.csproj` (research: ~162-165).
  - Acceptance: `rg "UtilitiesSwordfish\.NET\.General\.csproj" ToDoModel\ToDoModel.csproj` returns zero matches.
- [ ] [P3-T8] Remove the `UtilitiesSwordfish.NET.General.csproj` `ProjectReference` block from `Tags\Tags.csproj` (research: ~89-92; confirmed stale by P3-T1).
  - Acceptance: `rg "UtilitiesSwordfish\.NET\.General\.csproj" Tags\Tags.csproj` returns zero matches.
- [ ] [P3-T9] Remove the `UtilitiesSwordfish.NET.General.csproj` `ProjectReference` block from `TaskVisualization\TaskVisualization.csproj` (research: ~142-145; confirmed stale by P3-T1).
  - Acceptance: `rg "UtilitiesSwordfish\.NET\.General\.csproj" TaskVisualization\TaskVisualization.csproj` returns zero matches.
- [ ] [P3-T10] Remove the ninth `UtilitiesSwordfish.NET.General.csproj` `ProjectReference` block from `TaskVisualization.Test\TaskVisualization.Test.csproj` (research: ~297-300; absent from the stale "eight" scope list, required to avoid a dangling reference after folder deletion; confirmed stale by P3-T1).
  - Acceptance: `rg "UtilitiesSwordfish\.NET\.General\.csproj" TaskVisualization.Test\TaskVisualization.Test.csproj` returns zero matches.
- [ ] [P3-T11] Verify all nine references are removed repo-wide: `rg "UtilitiesSwordfish\.NET\.General\.csproj" -g "*.csproj"` returns zero matches. Record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/regression-testing/wi2-projectreference-zero.md`. Completes AC-7.
  - Acceptance: artifact records the `rg` command, `EXIT_CODE:`, and `Output Summary:` showing zero matches.

### Phase 4 — WI-3 Solution Entries and Project-Folder Deletion

- [ ] [P4-T1] Remove from the repo-root solution file `.\TaskMaster.sln` the `Project(...)`/`EndProject` declarations for `{F2E1680E-1B15-4CF2-BAB0-54B8C8F6ABDF}` (`UtilitiesSwordfish.NET.General`) and `{9A04D222-2B52-4E93-9B92-CC6EF54D5848}` (`UtilitiesSwordfish.NET.Test`) (research: lines 33-36). Record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/regression-testing/wi3-sln-declarations-removed.md`. Delivers AC-8.
  - Acceptance: `rg "F2E1680E-1B15-4CF2-BAB0-54B8C8F6ABDF|9A04D222-2B52-4E93-9B92-CC6EF54D5848" .\TaskMaster.sln` returns no `Project(...)` declaration lines for either GUID; artifact records the command, `EXIT_CODE:`, and `Output Summary:`.
- [ ] [P4-T2] Remove from the repo-root solution file `.\TaskMaster.sln` the `GlobalSection(ProjectConfigurationPlatforms)` configuration rows for both GUIDs (research: lines 194-205 General, 206-217 Test), leaving no orphaned configuration entries. Record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/regression-testing/wi3-sln-config-rows-removed.md`. Delivers AC-9.
  - Acceptance: `rg "F2E1680E-1B15-4CF2-BAB0-54B8C8F6ABDF|9A04D222-2B52-4E93-9B92-CC6EF54D5848" .\TaskMaster.sln` returns zero matches (no declaration and no configuration row for either GUID); artifact records the command, `EXIT_CODE:`, and `Output Summary:`.
- [ ] [P4-T3] Delete the `UtilitiesSwordfish\` project folder via `git rm -r UtilitiesSwordfish\` (removes `UtilitiesSwordfish.NET.General.csproj`, all vendored `*.cs`, and the nested `UtilitiesSwordfish\Swordfish.NET.sln`). Delivers part of AC-10.
  - Acceptance: the `UtilitiesSwordfish\` directory no longer exists on disk; `git status` shows the recursive deletion staged.
- [ ] [P4-T4] Delete the `UtilitiesSwordfish.Test\` project folder via `git rm -r UtilitiesSwordfish.Test\` (removes `UtilitiesSwordfish.NET.Test.csproj`, all vendored `*.cs`, and the XAML). Delivers part of AC-10.
  - Acceptance: the `UtilitiesSwordfish.Test\` directory no longer exists on disk; `git status` shows the recursive deletion staged.
- [ ] [P4-T5] Verify solution and folder teardown: `rg "F2E1680E-1B15-4CF2-BAB0-54B8C8F6ABDF|9A04D222-2B52-4E93-9B92-CC6EF54D5848" TaskMaster.sln` returns zero matches and both project folders are absent. Record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/regression-testing/wi3-solution-folder-teardown.md`. Confirms AC-8, AC-9, AC-10.
  - Acceptance: artifact records the `rg` command output (zero matches) and directory-absence checks for both folders, with `EXIT_CODE:` and `Output Summary:`.

### Phase 5 — Final QC, Verification, and Acceptance-Criteria Check-off

Run the full C# toolchain in this exact order; if any step fails or auto-fixes files, restart from the CSharpier step (P5-T1) and repeat until a single clean pass completes.

- [ ] [P5-T1] Run CSharpier: `dotnet tool run csharpier .`. Record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/qa-gates/finalqc-csharpier.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; if any file was reformatted, restart the loop from this task.
- [ ] [P5-T2] Run analyzers: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/qa-gates/finalqc-analyzers.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with zero analyzer errors; on failure, fix and restart from P5-T1.
- [ ] [P5-T3] Run nullable/type-check: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/qa-gates/finalqc-nullable.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with zero nullable warnings-as-errors; on failure, fix and restart from P5-T1.
- [ ] [P5-T4] Run tests with coverage: `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`. Record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/qa-gates/finalqc-vstest-coverage.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including numeric post-change repo-wide line-coverage percent (testable denominator) and pass/fail totals; on failure, fix and restart from P5-T1.
- [ ] [P5-T5] Verify coverage thresholds and delta: compare the P0-T6 baseline coverage, the P5-T4 post-change coverage, and changed/new-code coverage; confirm repo-wide line coverage `>= 80%` on the testable denominator and changed/new code `>= 90%`, with no regression on surviving first-party lines attributable to the removed tests. Record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/qa-gates/coverage-delta-verification.md`. Delivers AC-16.
  - Acceptance: artifact records baseline percent, post-change percent, and changed/new-code percent as numeric values, plus a pass/fail verdict against the 80%/90% thresholds; notes that the removed Swordfish tests dropped numerator and denominator together (no backfill owed).
- [ ] [P5-T6] Verify repo-wide Swordfish zero: `rg "Swordfish" -g "*.cs" -g "*.csproj" -g "*.sln"` returns zero matches (only archived docs/memory in Markdown and `.claude/agent-memory/**` may remain, which are excluded from the code globs). Record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/regression-testing/repo-wide-swordfish-zero.md`. Delivers AC-13.
  - Acceptance: artifact records the `rg` command, `EXIT_CODE:`, and `Output Summary:` showing zero matches over `*.cs`/`*.csproj`/`*.sln`.
- [ ] [P5-T7] Verify the solution builds green with both `UtilitiesSwordfish` and `UtilitiesSwordfish.Test` removed and no unresolved type reference (confirmed by the successful P5-T2/P5-T3 builds). Record confirmation in `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/qa-gates/finalqc-build-green.md`. Delivers AC-14.
  - Acceptance: artifact references the green analyzer and nullable build results and confirms no unresolved reference to either removed project.
- [ ] [P5-T8] Confirm the full C# toolchain (P5-T1 -> P5-T2 -> P5-T3 -> P5-T4) completed green in a single final pass with no restart. Record `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/qa-gates/finalqc-single-pass.md`. Delivers AC-15.
  - Acceptance: artifact records the four `EXIT_CODE:` values (all success) from one uninterrupted pass and a statement that no step auto-fixed files or failed in that pass.
- [ ] [P5-T9] Check off AC-1..AC-16 in `spec.md` and `user-story.md` as each is verified (per `acceptance-criteria-tracking`), and record a status summary at `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/other/ac-checkoff.md`.
  - Acceptance: every AC-N line in both `spec.md` and `user-story.md` is marked `[x]` only when its supporting evidence artifact exists; the status summary lists each AC with its evidence artifact path.

## Acceptance-Criteria Coverage Map

- AC-1, AC-2 -> P0-T2
- AC-3 -> P1-T1; AC-4 -> P1-T2; AC-5 -> P1-T3; AC-6 -> P1-T4 (P1-T5 dangling-symbol verification)
- AC-7 -> P3-T1 (stale evidence) + P3-T2..P3-T10 (removals) + P3-T11 (repo-wide verification)
- AC-8 -> P4-T1; AC-9 -> P4-T2; AC-10 -> P4-T3, P4-T4 (P4-T5 verification)
- AC-11 -> P2-T1, P2-T2, P2-T3, P2-T4; AC-12 -> P2-T5
- AC-13 -> P5-T6; AC-14 -> P5-T7; AC-15 -> P5-T8; AC-16 -> P5-T5
- Cross-document check-off -> P5-T9

## Open Questions / Notes

- WI-0 (P0-T2) is an execution-time HALT gate against the post-F1-F4 integration end-state; it is expected to fail in the current pre-merge worktree and must not be evaluated at plan preflight.
- If P2-T5 finds F2 lacks equivalent sender-identity/lock-recursion coverage, F5 raises a new issue rather than authoring tests against a clean type it does not own.
