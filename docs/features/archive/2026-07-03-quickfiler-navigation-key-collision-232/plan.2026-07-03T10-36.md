# quickfiler-navigation-key-collision (Plan)

- **Issue:** #232
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-07-03T10-45
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-bug

> Requirements source: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/spec.md` (full spec-driven expectations; `user-story.md` intentionally absent for `full-bug` mode). Supporting evidence: `artifacts/research/2026-07-03T00-00-quickfiler-kbdactions-duplicate-key-research.md` (Investigation 1: navigation-key defect; Investigation 2: probability debug logging).

**Fail-closed evidence rule:** Include explicit baseline artifact tasks, final-QA artifact tasks, and coverage-comparison tasks for each in-scope language when policy requires coverage. If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Record the expected artifact path or location in each evidence-producing task. Do not mark evidence-backed work complete without the artifact.

**Evidence location invariant:** All evidence artifacts in this plan resolve to `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/<kind>/` per `evidence-and-timestamp-conventions`. No task in this plan writes to `artifacts/baselines/`, `artifacts/qa/`, or `artifacts/coverage/`.

**Scope note (two bundled, non-overlapping changes, per user instruction and spec.md):**
- **Part A** (defect fix, 1 file): `QuickFiler/Controllers/QfcCollectionController.cs` — missing `UnregisterNavigation()`/`RegisterNavigation()` pairing in the page-swap path, plus a double-registration guard.
- **Part B** (additive logging, 3 files, no overlap with Part A): `QuickFiler/Controllers/QfcDatamodel.cs`, `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`, `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`.
- Both parts are planned as separate, independently-sequenced phase groups (Part A: Phases 1-3; Part B: Phase 4) with no file overlap and no execution-order dependency between them; a single Final QA Loop (Phase 5) covers both.

---

### Phase 0 — Baseline Capture & Policy Reads

- [x] [P0-T1] Read `CLAUDE.md` in full (repo root).
  - AC: file read in this session; no content changed.
- [x] [P0-T2] Read `.claude/rules/general-code-change.md` in full.
  - AC: file read in this session; no content changed.
- [x] [P0-T3] Read `.claude/rules/general-unit-test.md` in full.
  - AC: file read in this session; no content changed.
- [x] [P0-T4] Read `.claude/rules/csharp.md` in full.
  - AC: file read in this session; no content changed.
- [x] [P0-T5] Write the Phase 0 policy-read evidence artifact recording the exact reading order and the four files read in P0-T1..P0-T4, each with a `Timestamp:` field.
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/baseline/phase0-instructions-read.md`
  - AC: file exists and contains `Timestamp:`, `Policy Order:`, and an explicit list of the four files read.
- [x] [P0-T6] Record git baseline (branch name `TaskMaster-wt-2026-07-03-10-11`, HEAD commit SHA) via `git rev-parse HEAD` and `git status --porcelain` (expect clean).
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/baseline/git-baseline.md`
  - AC: file contains `Timestamp:`, the resolved HEAD SHA, and the porcelain status output.
- [x] [P0-T7] Run `dotnet tool run csharpier . --check` (or `csharpier . --check`) and record the baseline formatting state.
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/baseline/csharpier-baseline.md`
  - AC: file contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (pass/fail state as-is; baseline capture does not require a passing exit code).
- [x] [P0-T8] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the baseline analyzer build result.
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/baseline/msbuild-analyzers-baseline.md`
  - AC: file contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P0-T9] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and record the baseline nullable/warnings-as-errors build result.
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/baseline/msbuild-nullable-baseline.md`
  - AC: file contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P0-T10] Run `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage` and record baseline pass/fail counts and the numeric repository-wide coverage headline plus the `QfcHighConfidencePreFilter.cs` module coverage percentage (the one non-exempt file touched by this change).
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/baseline/vstest-baseline.md`
  - AC: file contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with numeric pass/fail counts, repo-wide coverage %, and `QfcHighConfidencePreFilter.cs` coverage %.
- [x] [P0-T11] Confirm the pre-fix source state matches the research citations by reading `QuickFiler/Controllers/QfcCollectionController.cs:252-262` (defective `LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)`), `:870-878` (`SwapItemGroups`, currently dead code), and `:1139-1221` (`RemoveSpecificControlGroupAsync`, unconditional trailing `RegisterNavigation()` at line ~1219), and confirm `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` has no existing `log4net.ILog logger` field.
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/baseline/pre-fix-source-confirmation.md`
  - AC: file records line-range confirmation for all four citations above, each marked "confirmed present as described" or with a noted discrepancy.

---

### Phase 1 — Part A: Failing Regression Test First (Bugfix Workflow Step 1)

- [x] [P1-T1] [expect-fail] Add test method `LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix` to `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`. Build an uninitialized `QfcCollectionController` via `FormatterServices.GetUninitializedObject` (matching the file's existing pattern) and inject via reflection: `_itemGroups` = a 1-item outgoing page; `_kbdHandler` = a `Mock<IQfcKeyboardHandler>` (Loose) whose `StringActionsAsync` getter returns a real `KbdActions<string, KaStringAsync, Func<string,Task>>` pre-populated with `"Collection"`-sourced entries for keys `"1"` (the outgoing page) and `"2"` (an orphaned key left behind by an earlier, separately-abandoned page, simulating prior unfixed swap history); `_moveMonitor` = `Mock<IEmailMoveMonitor>` (Loose); `_formViewer` = `Mock<IQfcFormViewer>` (Loose) with `L1v0L2L3v_TableLayout` returning `null`. Act: call `LoadControlsAndHandlers_01(null, cachedTwoItemPage)` where `cachedTwoItemPage` is a new 2-item `List<QfcItemGroup>`. Assert: the call throws `System.ArgumentException` with a message containing `"Key 2 SourceId Collection"` (matching the reported stack trace in `issue.md`).
  - AC: test method exists in the file, compiles, and — run against the current (pre-fix) `QfcCollectionController.cs` — fails the assertion because no exception is thrown pre-fix (pre-fix, the trailing register never runs inside this call, so the collision is not yet reproduced at this call boundary); OR, if written as an `act.Should().Throw<ArgumentException>()` assertion, it fails because the pre-fix method does not throw at all. Either framing is acceptable as long as the test fails pre-fix for a reason traceable to the missing register/unregister pairing.
- [x] [P1-T2] [expect-fail] Run the new test in isolation: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix` and confirm it fails.
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/regression-testing/reported-repro.expect-fail.md`
  - AC: file contains `Timestamp:`, `Command:`, `EXIT_CODE:` (non-zero / test-failed), and `Output Summary:` describing the failure reason.

---

### Phase 2 — Part A: Minimal Targeted Fix

- [x] [P2-T1] In `QuickFiler/Controllers/QfcCollectionController.cs`, modify `LoadControlsAndHandlers_01(TableLayoutPanel tlp, List<QfcItemGroup> itemGroups)` (currently lines 252-262) so the item-groups swap is routed through the existing `SwapItemGroups(List<QfcItemGroup>)` method (lines 870-878) instead of calling `ActivateQueuedItemGroups(itemGroups)` directly, preserving the existing `_moveMonitor.HookItem` loop, `_formViewer.SuspendLayout()`/`ResumeLayout()`, `ActivateQueuedTlp(tlp)`, and the trailing `ActiveIndex = -1;` statement.
  - AC: `ActivateQueuedItemGroups(itemGroups)` is no longer called directly from this overload; `SwapItemGroups(itemGroups)` is called in its place; the method still compiles with an unchanged public signature.
- [x] [P2-T2] In the same file, add a double-registration guard to `RemoveSpecificControlGroupAsync` (lines 1139-1221): declare a local boolean (default `false`) inside the method, set it to `true` immediately after the zero-item branch's `await ((QfcFormController)_parent).SkipGroupAsync();` call (currently line ~1209) completes, and wrap the trailing unconditional `RegisterNavigation();` call (currently line ~1219) in `if (!<guardVariable>) { RegisterNavigation(); }`.
  - AC: the guard variable is declared and scoped to the method; it is set only within the zero-item branch after `SkipGroupAsync()` returns; the trailing `RegisterNavigation()` call site is conditioned on `!<guardVariable>`; the method compiles with an unchanged public signature.
- [x] [P2-T3] Run the Phase 1 regression test in isolation: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix` and confirm it now passes (no `ArgumentException`).
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/regression-testing/reported-repro.pass-after-fix.md`
  - AC: file contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` confirming the test now passes.

---

### Phase 3 — Part A: Additional Regression Coverage, Guard Verification, and Scope Confirmation

- [x] [P3-T1] Add test method `LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys` to `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` (AC1): using the same reflection-injection pattern as P1-T1, set up a controller with `_itemGroups` = a 2-item outgoing page whose keys `"1"` and `"2"` are pre-registered in a real `KbdActions<string, KaStringAsync, Func<string,Task>>` on the injected `_kbdHandler` mock; call `LoadControlsAndHandlers_01(null, oneItemIncomingPage)`; assert (FluentAssertions) that `StringActionsAsync` no longer contains any `"Collection"`-sourced key from the outgoing page and now contains exactly one `"Collection"`-sourced key `"1"` for the incoming page.
  - AC: test exists, compiles, and passes against the post-fix `QfcCollectionController.cs`.
- [x] [P3-T2] Run the new test in isolation: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys` and confirm it passes.
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/regression-testing/swap-register-unregister-order.pass.md`
  - AC: file contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`.
- [x] [P3-T3] Add test method `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException` to `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` demonstrating the double-registration hazard the Phase 2 guard prevents: inject `_itemGroups` and `_kbdHandler` as above for a 2-item page with no keys pre-registered; call `RegisterNavigation()` once (succeeds), then call `RegisterNavigation()` again immediately; assert the second call throws `System.ArgumentException` with a message containing `"SourceId Collection"`.
  - AC: test exists, compiles, and passes (proving the underlying `KbdActions.Add` contract that the guard exists to avoid triggering).
- [x] [P3-T4] Add test method `SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey` (AC3) to the same file: using the sequence `UnregisterNavigation()` (1-item outgoing page) -> remove the item from `_itemGroups` -> `LoadControlsAndHandlers_01(null, twoItemCachedPage)` (exercising the fixed swap-and-register path) -> assert `StringActionsAsync` contains exactly one `"Collection"`-sourced entry per key `"1"` and `"2"` (no duplicates) and no exception was thrown, confirming the production guard's effect (skipping the redundant trailing register) is correct.
  - AC: test exists, compiles, and passes against the post-fix `QfcCollectionController.cs`.
- [x] [P3-T5] Run both P3-T3 and P3-T4 tests in isolation: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException,SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey` and confirm both pass.
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/regression-testing/double-registration-guard.pass.md`
  - AC: file contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` confirming both tests pass.
- [x] [P3-T6] Confirm AC8 (no unintended scope creep) via `git diff --stat` against the Phase 0 baseline commit: verify the diff touches only `QuickFiler/Controllers/QfcCollectionController.cs` for Part A, and confirm no changes appear in `QuickFiler/Controllers/QfcDatamodel.cs` (`InitEmailQueue`/`InitEmailQueueAsync`/`DequeueNextItemGroupAsync`/`WaitForQueue`), `QuickFiler/Controllers/QfcHighConfidencePreFilterLoader.cs` (if present), or the `removespecificcontrolgroupcounter` field/guard logic in `QfcCollectionController.cs` beyond the P2-T2 addition.
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/other/ac8-scope-confirmation.md`
  - AC: file records the `git diff --stat` output and an explicit confirmation statement that the fixed-batch-without-backfill pattern, the dormant #171 pre-filter loader, and the `removespecificcontrolgroupcounter` reentrancy hygiene issue remain untouched.

---

### Phase 4 — Part B: Additive Probability Debug Logging (No Control-Flow Change)

- [x] [P4-T1] In `QuickFiler/Controllers/QfcDatamodel.cs`, add one `logger.Debug(...)` call inside `ScoreRemainingQueueMailItemAsync` (lines 316-326), placed immediately after `var score = await scoringService.ScoreAsync(mailItem, _globals, cancel).ConfigureAwait(false);` and before `return score.Score;`, logging `mailItem.Subject`, `mailItem.EntryID`, `score.Score`, and the literal caller-context string `"QfcDatamodel.LoadRemainingEmailsToQueueAsync (master-queue admission)"`.
  - AC: the new line compiles; the method's `return score.Score;` statement and its return type are unchanged; no other line in the method is modified.
- [x] [P4-T2] In `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`, add one `logger.Debug(...)` call immediately after each of the two `_folderHandler = ...` assignments in `LoadFolderHandler(object varList = null)` (currently lines 31-36 and 39-44), each logging `ItemHelper?.Subject`, `ItemHelper?.EntryId`, `_folderHandler?.Suggestions?.TopScore() ?? 0`, and a caller-context string that distinguishes the two branches (e.g. `"QfcItemController.LoadFolderHandler (FromField)"` vs. `"QfcItemController.LoadFolderHandler (FromArrayOrString)"`).
  - AC: both new lines compile; the `if (varList is null) { ... } else { ... }` branch structure and both assignment statements are otherwise unchanged.
- [x] [P4-T3] In the same file, add one `logger.Debug(...)` call immediately after each of the two `_folderHandler = await Task.Run(...)` assignments completes in `LoadFolderHandlerAsync(CancellationToken cancel, object varList = null)` (currently lines 54-70 and 94-109), each logging `ItemHelper?.Subject`, `ItemHelper?.EntryId`, `_folderHandler?.Suggestions?.TopScore() ?? 0`, and a caller-context string distinguishing the two branches (e.g. `"QfcItemController.LoadFolderHandlerAsync (FromField)"` vs. `"QfcItemController.LoadFolderHandlerAsync (FromArrayOrString)"`).
  - AC: both new lines compile; the existing `catch (ArgumentNullException e)` fallback block (currently lines 72-85) and the `catch (System.Exception e)` block (currently lines 86-90) are unchanged; no new call site is added inside either catch block.
- [x] [P4-T4] In `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`, add a new field to the `QfcHighConfidencePreFilter` static class: `private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);`, matching the convention already present in `QfcDatamodel.cs:27-29` and `QfcCollectionController.cs:23-25`.
  - AC: the field compiles; no existing member of `QfcHighConfidencePreFilter`, `QfcPreScoredItem`, `IFolderScoringService`, or `FolderScoringService` is modified or removed.
- [x] [P4-T5] In the same file, add one `logger.Debug(...)` call inside `FilterAsync`'s per-item scoring lambda (currently lines 62-70), placed immediately after `var (score, topFolder) = await service.ScoreAsync(item, globals, token);`, logging `item.Subject`, `item.EntryID`, `score`, `topFolder`, and the literal caller-context string `"QfcHighConfidencePreFilter.FilterAsync"`.
  - AC: the new line compiles; the lambda's `return (index, item, score, topFolder);` statement is unchanged; `FilterAsync`'s public signature and its cutoff/`Where`/`OrderBy`/`Select` pipeline (lines 74-78) are unchanged.
- [x] [P4-T6] Run the existing, unmodified test files that exercise the four Part B call sites — `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`, `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs`, `QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs` — in isolation via `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:<comma-separated test names from these three files>` and confirm all pass unmodified (AC7).
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/regression-testing/part-b-logging-no-regression.md`
  - AC: file contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` with pass counts for all three test files, and an explicit statement that no test assertions were modified to accommodate the new logging.

---

### Phase 5 — Final QA Loop (Full C# Toolchain, Both Parts)

- [x] [P5-T1] Run `dotnet tool run csharpier .` and confirm no files are modified by the formatter (or, if files are modified, restart the loop from this step after re-running).
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/qa-gates/csharpier-final.md`
  - AC: file contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero files changed on the reported pass.
- [x] [P5-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and confirm a clean build with no new analyzer diagnostics versus the Phase 0 baseline.
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/qa-gates/msbuild-analyzers-final.md`
  - AC: file contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`.
- [x] [P5-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and confirm a clean build with no new nullable warnings versus the Phase 0 baseline.
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/qa-gates/msbuild-nullable-final.md`
  - AC: file contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`.
- [x] [P5-T4] Run `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage` and record the full post-change pass/fail counts and numeric coverage headline (repository-wide % and `QfcHighConfidencePreFilter.cs` module %).
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/qa-gates/vstest-final.md`
  - AC: file contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` with numeric pass/fail counts, repo-wide coverage %, and `QfcHighConfidencePreFilter.cs` coverage %; total test failures do not exceed the Phase 0 baseline count.
- [x] [P5-T5] Compare the Phase 0 baseline coverage (`evidence/baseline/vstest-baseline.md`) against the Phase 5 final coverage (`evidence/qa-gates/vstest-final.md`) for `QfcHighConfidencePreFilter.cs` and confirm: (a) repository-wide coverage has not regressed below its baseline value, and (b) `QfcHighConfidencePreFilter.cs`'s changed lines (the new `logger` field and the new `logger.Debug(...)` call) are exercised at `>= 90%` per the new/changed-code target (AC10). `QfcCollectionController.cs`, `QfcDatamodel.cs`, and `QfcItemController.FolderHandling.cs` remain covered by the ratified COM/WinForms `[ExcludeFromCodeCoverage]` exemption and carry no new numeric coverage obligation from this change.
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/qa-gates/coverage-delta.md`
  - AC: file records baseline coverage %, final coverage %, the delta, and the `QfcHighConfidencePreFilter.cs` changed-line coverage %, with an explicit PASS/FAIL determination against the `>= 90%` new/changed-code target and the no-regression requirement.

---

### Phase 6 — Documentation, Acceptance Criteria Closeout, and Evidence Consolidation

- [x] [P6-T1] Update `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/spec.md` Acceptance Criteria section, checking off AC1-AC10 with a one-line evidence pointer for each (referencing the specific Phase/task and evidence artifact that satisfies it).
  - AC: all 10 AC checkboxes in `spec.md` are updated to `[x]` with an inline evidence reference, or left unchecked with an explicit blocking reason recorded.
- [x] [P6-T2] Update `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/issue.md` to record the fix summary, the two bundled changes, and links to the Phase 5 final QA evidence.
  - AC: `issue.md` contains an updated status note referencing the resolved defect and the completed logging addition, with a timestamp.
- [x] [P6-T3] Confirm the working tree is clean and all evidence artifacts listed in Phases 0, 1, 2, 3, 4, and 5 are present on disk under `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/`.
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/other/evidence-inventory.md`
  - AC: file lists every evidence artifact path referenced in this plan and confirms each exists; `git status --porcelain` output included and shows no unintended untracked files outside the feature folder and the four modified/added production and test files.
- [x] [P6-T4] Record follow-up candidates (not part of this change, per spec.md Rollout & Follow-up) as a short note: (1) fixed-batch-without-backfill pattern in `QfcDatamodel.InitEmailQueue`/`DequeueNextItemGroupAsync`; (2) dormant Issue #171 pre-filter pipeline wiring; (3) `removespecificcontrolgroupcounter` reentrancy-counter hygiene.
  - Evidence: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/other/follow-up-candidates.md`
  - AC: file lists all three follow-up candidates with a one-line rationale each, matching spec.md's Rollout & Follow-up section.
