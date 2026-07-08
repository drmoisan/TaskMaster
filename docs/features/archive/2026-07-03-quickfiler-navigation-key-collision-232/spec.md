# quickfiler-navigation-key-collision (Spec)

- **Issue:** #232
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-07-03T10-45
- **Status:** Approved for planning
- **Version:** 0.2

## Context
QuickFiler throws `System.ArgumentException: Cannot add key because it already exists. Key 2 SourceId Collection` when a page transition (OK/Skip, including the automatic skip triggered by popping out the last item on a page) swaps in a new page without unregistering the outgoing page's keyboard navigation keys or registering the incoming page's keys, leaving stale `"Collection"`-sourced keys in the shared `KbdActions` registry that later collide. Bundled with this fix, at the user's request, is an additive debug-logging change so that every folder-confidence probability calculation is logged (item summary, score, caller) to make a separately-observed "only a subset of items appears in high-confidence mode" symptom empirically diagnosable in future sessions.

Environment:
- OS/version: Windows (VSTO Outlook add-in host)
- Repo/branch: TaskMaster, branch `TaskMaster-wt-2026-07-03-10-11`, HEAD `00507b59`
- Command/flags used: QuickFiler run in "high confidence mode"
- Data source or fixture: Live Outlook inbox (production run, not a test fixture)

Impact / Severity:
- [x] High
- [ ] Blocker
- [ ] Medium
- [ ] Low

Rationale: crashes the active QuickFiler session (unhandled exception surfaced to the user) during ordinary bulk-processing use; reachable via everyday OK/Skip page transitions, not an edge case.


## Repro & Evidence
Steps to Reproduce:
1. Run QuickFiler in high-confidence mode with a page that has exactly one item.
2. Click the "pop out" button for that item.
3. Internally: `RemoveSpecificControlGroupAsync` unregisters the current page's key(s), removes the item (count reaches 0), and calls `QfcFormController.SkipGroupAsync()` to bring forward the next cached page via `LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)`, which swaps `_itemGroups` without calling `UnregisterNavigation()`/`RegisterNavigation()`.
4. `RemoveSpecificControlGroupAsync` then unconditionally calls `RegisterNavigation()` again for the newly swapped-in page.
5. If any key in the newly-active page's range ("1".."N") is still occupied by an orphaned entry left behind by an earlier page that was abandoned mid-page via the same defective swap path (OK or Skip while items remained), `KbdActions.Add` throws `ArgumentException`.

Expected:
Keyboard digit-navigation keys ("1".."N", `SourceId = "Collection"`) always match exactly the currently-displayed page's items, regardless of whether the page changed via individual item removal, "OK" (move + load next), or "Skip". No stale keys should ever remain registered for a page that is no longer displayed.

Actual:
`QfcCollectionController.LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)` (`QuickFiler/Controllers/QfcCollectionController.cs:252-262`) — the swap-in path used by both the "OK" flow (`QfcFormController.EventHandlers.cs:136-143`) and the "Skip" flow (`QfcFormController.EventHandlers.cs:361-395`, also invoked internally from `RemoveSpecificControlGroupAsync`'s zero-item branch) — never calls `UnregisterNavigation()` for the outgoing page or `RegisterNavigation()`/`WireUpAsyncKeyboardHandler()` for the incoming page. Its unused sibling `SwapItemGroups` (line 870-878) shows the correct pattern. `_kbdHandler.StringActionsAsync` is a single, session-lifetime collection shared across all pages, so keys orphaned by this gap collide the next time `RegisterNavigation()` walks from position 0 — which happens unconditionally at the end of `RemoveSpecificControlGroupAsync` (line 1219).

Logs / Screenshots:
- [x] Attached minimal logs or screenshot
- Snippet:
```
System.ArgumentException
  HResult=0x80070057
  Message=Cannot add key because it already exists. Key 2 SourceId Collection
Parameter name: instance
  Source=QuickFiler
  StackTrace:
   at QuickFiler.Controllers.KbdActions`3.Add(UClass instance) in KbdActions.cs:line 121
   at QuickFiler.Controllers.QfcCollectionController.RegisterNavigationAsyncAction(Int32 itemIndex, Int32 digits) in QfcCollectionController.cs:line 1346
   at QuickFiler.Controllers.QfcCollectionController.RegisterNavigation() in QfcCollectionController.cs:line 1325
   at QuickFiler.Controllers.QfcCollectionController.<RemoveSpecificControlGroupAsync>d__96.MoveNext() in QfcCollectionController.cs:line 1219
   at QuickFiler.Controllers.QfcCollectionController.<PopOutControlGroupAsync>d__84.MoveNext() in QfcCollectionController.cs:line 966
   at QuickFiler.Controllers.QfcItemController.<BtnPopOut_Click>d__163.MoveNext() in QfcItemController.EventHandlers.cs:line 67
```


## Scope & Non-Goals
- In scope:
  - Fix the missing `UnregisterNavigation()`/`RegisterNavigation()` pairing in `QfcCollectionController.LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)` (`QuickFiler/Controllers/QfcCollectionController.cs`), most naturally by routing the swap through the existing (currently dead) `SwapItemGroups` method.
  - Decide and implement whether the trailing unconditional `RegisterNavigation()` in `RemoveSpecificControlGroupAsync` (same file, line ~1219) needs to become conditional to avoid double-registration once the swap path registers correctly on its own.
  - Add regression test coverage reproducing the reported scenario (1-item page popped out; cached ≥2-item page swapped in via Skip; no `ArgumentException` on the subsequent registration).
  - Add `logger.Debug(...)` calls at the three identified folder-confidence scoring call sites (`QfcDatamodel.ScoreRemainingQueueMailItemAsync`, `QfcItemController.LoadFolderHandler`/`LoadFolderHandlerAsync`, `QfcHighConfidencePreFilter.FilterAsync`), each capturing item summary, computed score, and caller context, per existing log4net convention. Add a new `logger` field to `QfcHighConfidencePreFilter.cs` since none exists today.
- Out of scope / non-goals:
  - Fixing the fixed-batch-without-backfill pattern responsible for the separately-reported "subset of items shown" symptom (screen 1: `QfcDatamodel.InitEmailQueue`/`InitEmailQueueAsync`; subsequent screens: `QfcDatamodel.DequeueNextItemGroupAsync`/`WaitForQueue`). This is a larger batch-sizing/backfill design decision, flagged as a follow-up issue.
  - Wiring up the currently-dead Issue #171 pre-filter pipeline (`QfcHighConfidencePreFilterLoader`) into the live QuickFiler startup path.
  - Any change to the `removespecificcontrolgroupcounter` reentrancy-counter hygiene issue (the counter can leak upward on any exception mid-method, including this one) beyond what is strictly necessary to fix the reported defect. Flagged as a follow-up if a separate finding surfaces during execution.
  - Broadening `KbdActions<TKey,UClass,VDelegate>.Add`'s duplicate-key contract (e.g., making it idempotent/overwrite-on-duplicate) — Approach B in the research artifact was evaluated and rejected as a masking fix.
- Explicitly excluded systems, integrations, or datasets: live Outlook/COM integration testing (no live-Outlook test dependency is introduced); the `KbdActions.cs` file itself (no change needed — its throw-on-duplicate contract is correct and unchanged).

## Root Cause Analysis
Full diagnosis at `artifacts/research/2026-07-03T00-00-quickfiler-kbdactions-duplicate-key-research.md`. Confirmed root cause: missing `UnregisterNavigation()`/`RegisterNavigation()` pairing in `LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)`, `QuickFiler/Controllers/QfcCollectionController.cs:252-262`. Copilot's initial "overlapping `RemoveSpecificControlGroupAsync` race" hypothesis was investigated and ruled out as the primary explanation — this is a deterministic sequencing/bookkeeping gap, not a race. Recommended fix: route the swap through the existing (currently dead) `SwapItemGroups` method, which already does `UnregisterNavigation(); ...; RegisterNavigation();` correctly, and decide whether the trailing unconditional `RegisterNavigation()` in `RemoveSpecificControlGroupAsync` (line 1219) needs to become conditional to avoid double-registration once the swap path registers correctly on its own.

**Bundled scope (user-requested, same change):** Add `logger.Debug(...)` calls at every point a folder-confidence probability is computed, each capturing item summary (Subject/EntryID), the computed score, and a literal caller-context string, per the existing log4net convention already used throughout this file family. Three call sites identified in Investigation 2 of the same research artifact:
- `QuickFiler/Controllers/QfcDatamodel.cs:316-326` (`ScoreRemainingQueueMailItemAsync`)
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:27-111` (`LoadFolderHandler`/`LoadFolderHandlerAsync`, 4 assignment points)
- `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:62-70` (`FilterAsync` lambda; needs a new `logger` field added)

This logging is intended to make a separately-reported symptom ("only a subset of items appears in high-confidence mode, on the first screen and subsequent screens") empirically diagnosable. Research concluded the subset symptom is most likely explained by an existing fixed-batch-without-backfill pattern (not a threshold bug, and not proven to be caused by score mutation across time) — see Investigation 2 in the research artifact. Fixing that batch/backfill behavior, or wiring up the currently-dead Issue #171 pre-filter pipeline, are explicitly **not** part of this change; they are flagged as separate candidate follow-up issues.


## Proposed Fix

### Design summary (what changes where):
Two independent, non-overlapping changes bundled into one PR:
1. **Navigation-key fix** (`QfcCollectionController.cs`): make `LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)` unregister the outgoing page's keys and register the incoming page's keys during the swap, by delegating the item-groups swap portion to the existing `SwapItemGroups(List<QfcItemGroup>)` method (which already implements `UnregisterNavigation(); ...; RegisterNavigation();` correctly) instead of calling `ActivateQueuedItemGroups(itemGroups)` directly. Then re-examine `RemoveSpecificControlGroupAsync`'s trailing, unconditional `RegisterNavigation()` (line ~1219): once the zero-item branch's `SkipGroupAsync()` call performs a correct register via the fixed swap path, the trailing call would attempt to re-register the same page's keys a second time in the same call stack and must be skipped in that specific branch (guard on whether a swap-and-register already occurred during this invocation).
2. **Probability debug logging** (`QfcDatamodel.cs`, `QfcItemController.FolderHandling.cs`, `QfcHighConfidencePreFilter.cs`): add `logger.Debug(...)` immediately after each of the three identified score-computation points, with no control-flow change.

### Boundaries and invariants to preserve:
- `_kbdHandler.StringActionsAsync` must, after any page transition, contain exactly one `"Collection"`-sourced entry per key `"1".."N"` (or zero-padded per current `Digits`) matching the live `_itemGroups`, and zero entries for any page no longer displayed.
- `KbdActions<TKey,UClass,VDelegate>.Add`'s existing throw-on-duplicate contract is preserved unchanged (Approach B, making it idempotent, is rejected).
- `SwapItemGroups`'s existing signature and behavior are preserved; it is invoked from a new call site, not modified.
- The public surface of `LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)` (signature, callers) is unchanged.
- Probability logging is purely additive: no return values, thresholds, or control flow may change as a result of adding the log calls.

### Dependencies or blocked work:
None. Both changes are self-contained within `QuickFiler/Controllers/`.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
1. `QuickFiler/Controllers/QfcCollectionController.cs` — navigation-key fix (route swap through `SwapItemGroups`; guard the trailing `RegisterNavigation()` in `RemoveSpecificControlGroupAsync` against double-registration after a swap-triggered registration).
2. `QuickFiler/Controllers/QfcDatamodel.cs` — add debug log after score computation in `ScoreRemainingQueueMailItemAsync` (lines ~316-326).
3. `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` — add debug log after each of the 4 `_folderHandler = ...` assignment points in `LoadFolderHandler`/`LoadFolderHandlerAsync` (lines ~27-111).
4. `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` — add a new `logger` field and a debug log inside the `FilterAsync` per-item lambda (lines ~62-70).

Corresponding test files (existing `QuickFiler.Test` project, MSTest/Moq/FluentAssertions per repo policy):
- `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` (or a new sibling test file if the existing one is at/near the 500-line cap) — add/extend mock-based assertions for register/unregister ordering on page swap.
- A new or extended `QfcCollectionController`-focused test (seam-permitting) reproducing the exact reported scenario.

#### Functions/classes/CLI commands impacted:
- `QfcCollectionController.LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)`
- `QfcCollectionController.RemoveSpecificControlGroupAsync(int selection)`
- `QfcCollectionController.SwapItemGroups(List<QfcItemGroup>)` (new caller, no signature change)
- `QfcDatamodel.ScoreRemainingQueueMailItemAsync(MailItem, CancellationToken)`
- `QfcItemController.LoadFolderHandler(object)` / `LoadFolderHandlerAsync(CancellationToken, object)`
- `QfcHighConfidencePreFilter.FilterAsync(...)`

No CLI commands are impacted (WinForms/VSTO add-in, no CLI surface).

#### Data flow and validation changes:
No data-flow or validation-rule changes. The navigation-key fix corrects internal bookkeeping (a collection membership invariant); the logging change reads already-computed values without altering them.

#### Error handling and logging updates:
- Navigation-key fix removes the reachability of the reported `ArgumentException` for the documented scenario; no new exception types are introduced.
- New `logger.Debug(...)` calls at 3 call sites (4 assignment points in one of them) per the Suspected Cause / Notes section above, following the existing `private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(...)` convention already present in `QfcDatamodel.cs`/`QfcItemController.cs` and newly added to `QfcHighConfidencePreFilter.cs`.

#### Rollback/feature-flag considerations (if applicable):
None required — both changes are behavior-preserving except for the specific defect being corrected (no feature flag; a straight revert of the commit(s) fully rolls back).

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
No public interface/contract changes. Log message format: include item summary (Subject and/or EntryID), the numeric score, and a literal caller-context string identifying the invoking method/branch (per convention documented in the research artifact, Investigation 2 §2.2).

#### Required configuration keys and defaults:
None new. Debug-level visibility is controlled entirely by the existing `log4net.config` root logger level (`<root><level value="ALL" /></root>`); no new config key is introduced.

#### Backward-compatibility expectations:
Fully backward compatible — no public API signatures change, no persisted data formats change, no configuration schema changes.

#### Performance constraints (latency/throughput/memory):
Negligible: one additional string-formatted debug log call per item per scoring call site (bounded by page size / inbox batch size, consistent with existing `logger.Debug` calls already present in this file family); no additional COM round-trips introduced (reuses already-loaded `Subject`/`EntryID` values per the research artifact's convention citation).

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access): the shared `_kbdHandler.StringActionsAsync` collection remains a single, session-lifetime instance per the confirmed research finding (`KeyboardHandler.cs:84-89`); no other in-flight change re-architects keyboard-handler ownership.
- Constraints (budget, performance, compatibility): repo file-size cap (500 lines) applies to any modified/new test file; no live-Outlook dependency may be introduced in tests; MSTest + Moq + FluentAssertions only.
- External dependencies (services, libraries, releases): none. No new package dependencies.

## Data / API / Config Impact
- User-facing or API changes: none (internal bookkeeping fix; logging is developer/diagnostic-facing only, gated by existing log4net config, not user-visible UI).
- Data or migration considerations: none.
- Logging/telemetry updates (if any): 3 new `logger.Debug(...)` call sites (4 assignment points in one of them) per Proposed Fix above; no telemetry system changes.
- Compatibility notes (CLI flags, config schemas, versioning): none (no CLI surface; no config schema changes).

## Test Strategy
Seeded from issue:

- [x] Unit coverage areas: `QfcFormControllerTests.cs`-style mock-based assertions that `LoadItems(TableLayoutPanel, List<QfcItemGroup>)` invokes `RegisterNavigation()`/`UnregisterNavigation()` in the correct order; a logical-level regression test reproducing the exact reported scenario (1-item page popped out, cached ≥2-item page swapped in, no `ArgumentException` on the subsequent `RegisterNavigation()`); no dedicated new tests required for the additive `logger.Debug` calls beyond confirming they do not throw.
- [ ] Integration scenario to retest: manual QuickFiler high-confidence-mode run exercising OK, Skip, and single-item pop-out transitions across multiple pages.
- [ ] Manual verification notes: after the fix, confirm `_kbdHandler.StringActionsAsync` never accumulates stale entries across a full multi-page processing session.

- Regression tests to add or update: new MSTest test(s) in `QuickFiler.Test/Controllers/` reproducing (a) register/unregister ordering on `LoadItems`/`LoadControlsAndHandlers_01` swap via the existing `IQfcCollectionController` mock pattern, and (b) the exact reported crash scenario at whatever seam is feasible per the coverage-exemption carve-out for testable seams within COM-bound classes.
- Unit tests (MSTest/Moq/FluentAssertions) for the fixed behavior and boundaries: positive (swap registers/unregisters correctly), negative (pre-fix reproduction would throw; post-fix must not), edge (page size 1 → 0 transition, page size crossing the 9/10-item digit-format boundary if reachable within the fix's scope).
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values): zero-item outgoing page (no keys to unregister — must remain a no-op, not an error); incoming cached page with 0 items (no keys to register); digit-format boundary (1 → 2 digits) during a swap.
- Error handling and logging verification: confirm no `ArgumentException` is thrown for the documented scenario; confirm new debug log calls do not throw when their referenced fields (`ItemHelper`, `_folderHandler`, `mailItem`) are in normal post-assignment state.
- Coverage impact and targets for changed lines/modules: `QfcCollectionController.cs` is `[ExcludeFromCodeCoverage]` (ratified COM/WinForms exemption) — no numeric coverage obligation on that file's changed lines, but behavioral verification via the mock-based test pattern is still required per Test Strategy above. `QfcDatamodel.cs` and `QfcItemController.FolderHandling.cs` are within the same ratified exemption. `QfcHighConfidencePreFilter.cs` is NOT exempt — its existing `QfcHighConfidencePreFilterTests.cs` scoring-seam mock must continue to exercise the new log line without a coverage regression.
- Toolchain commands to run (format → lint → type-check → test): `dotnet tool run csharpier .` → `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` → `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` → `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`.
- Manual validation steps (if required): manual QuickFiler high-confidence-mode session exercising OK, Skip, and single-item pop-out transitions across multiple pages, confirming no `ArgumentException` and confirming new debug log lines appear per item scored.

## Acceptance Criteria
- [x] AC1: `LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)` unregisters the outgoing page's `"Collection"` navigation keys and registers the incoming page's navigation keys on every swap (verified via mock-based assertion on `RegisterNavigation()`/`UnregisterNavigation()` call order). — Evidence: P3-T1/P3-T2, `evidence/regression-testing/swap-register-unregister-order.pass.md`.
- [x] AC2: The reported reproduction (1-item page popped out; `RemoveSpecificControlGroupAsync` reaches zero items; `SkipGroupAsync` swaps in a cached page whose key range overlaps a previously-abandoned page's stale keys) no longer throws `ArgumentException`; a regression test encodes this exact scenario and passes. — Evidence: P1-T2 (expect-fail) `evidence/regression-testing/reported-repro.expect-fail.md`, P2-T3 `evidence/regression-testing/reported-repro.pass-after-fix.md`.
- [x] AC3: `RemoveSpecificControlGroupAsync`'s trailing `RegisterNavigation()` does not double-register keys already registered by a swap that occurred earlier in the same call (no `ArgumentException` from re-adding the same key twice within one invocation). — Evidence: P2-T2 guard fix; P3-T3/P3-T4/P3-T5 `evidence/regression-testing/double-registration-guard.pass.md`.
- [x] AC4: `QfcDatamodel.ScoreRemainingQueueMailItemAsync` logs at debug level, for every item scored, the item summary (Subject/EntryID), the computed score, and a caller-context string. — Evidence: P4-T1 (`QfcDatamodel.cs` `logger.Debug` in `ScoreRemainingQueueMailItemAsync`); verified by P4-T6 `evidence/regression-testing/part-b-logging-no-regression.md`.
- [x] AC5: `QfcItemController.LoadFolderHandler`/`LoadFolderHandlerAsync` log at debug level, at all 4 assignment points, the item summary, computed score, and a caller-context string distinguishing the branch. — Evidence: P4-T2/P4-T3 (four `logger.Debug` calls with FromField/FromArrayOrString caller-context strings in `QfcItemController.FolderHandling.cs`); verified by P4-T6 `evidence/regression-testing/part-b-logging-no-regression.md`.
- [x] AC6: `QfcHighConfidencePreFilter.FilterAsync` logs at debug level, for every item scored, the item summary, computed score, and topFolder, and a caller-context string; a new `logger` field is added to this file following repo log4net convention. — Evidence: P4-T4 (new `logger` field) and P4-T5 (`logger.Debug` in the `FilterAsync` scoring lambda logging Subject/EntryID/Score/TopFolder); verified by P4-T6 `evidence/regression-testing/part-b-logging-no-regression.md`.
- [x] AC7: The logging additions introduce no behavior change — all pre-existing tests covering `ScoreRemainingQueueMailItemAsync`, `LoadFolderHandler(Async)`, and `QfcHighConfidencePreFilterTests.cs` continue to pass unmodified. — Evidence: P4-T6 `evidence/regression-testing/part-b-logging-no-regression.md` (29/29 tests pass across the three unmodified test files; no assertions altered).
- [x] AC8: No unintended behavior changes outside the defined scope (fixed-batch-without-backfill pattern, dormant #171 pre-filter, and `removespecificcontrolgroupcounter` reentrancy hygiene remain untouched and are documented as follow-up). — Evidence: P3-T6 `evidence/other/ac8-scope-confirmation.md`; follow-ups recorded in P6-T4 `evidence/other/follow-up-candidates.md`.
- [x] AC9: Full C# toolchain (csharpier → .NET analyzers → nullable/TreatWarningsAsErrors → MSTest via vstest.console.exe) passes with no regressions, in this exact order, in a single clean pass. — Evidence: P5-T1 `evidence/qa-gates/csharpier-final.md` (0 files changed), P5-T2 `evidence/qa-gates/msbuild-analyzers-final.md` (0 errors, no new diagnostics), P5-T3 `evidence/qa-gates/msbuild-nullable-final.md` (zero new nullable diagnostics; identical 540-error pre/post population), P5-T4 `evidence/qa-gates/vstest-final.md` (4641/4641 pass, 0 fail).
- [x] AC10: Repository-wide and changed-line coverage obligations are met per the ratified COM/WinForms exemption boundary (no new obligation on `QfcCollectionController.cs`/`QfcDatamodel.cs`/`QfcItemController.FolderHandling.cs`; `QfcHighConfidencePreFilter.cs` changed lines meet the `>=90%` new/changed-code target). — Evidence: P5-T5 `evidence/qa-gates/coverage-delta.md` (`QfcHighConfidencePreFilter.cs` changed lines 100% >= 90%; repo-wide 76.5758% -> 76.5712%, within measurement variance, no changed production line uncovered). Remediation cycle 1 (2026-07-03T16-58): coverage now verified from the persisted machine-readable Cobertura artifact `artifacts/csharp/coverage.xml` and `evidence/coverage/2026-07-03T16-58/coverage.xml` (repo-wide line-rate 76.5750%, 40355/52700; all 6 `QfcHighConfidencePreFilter.cs` classes line-rate=1) per `evidence/qa-gates/coverage-verification.2026-07-03T16-58.md` — AC10 PASS.

## Risks & Mitigations
- Technical or operational risks: (1) guarding the trailing `RegisterNavigation()` in `RemoveSpecificControlGroupAsync` incorrectly could reintroduce a related defect (incoming page silently unregistered on some path) — mitigate via the negative/edge regression tests in Test Strategy. (2) `QfcCollectionController` is COM/WinForms-bound and coverage-exempt, so behavioral verification depends on the `IQfcCollectionController` mock seam rather than direct coverage — mitigate by also adding a direct-seam test if one is feasible per the "testable seams within otherwise-COM-bound assemblies are not exempt" policy carve-out (research artifact §8). (3) Adding a `logger` field to `QfcHighConfidencePreFilter.cs` for a currently-dead-in-production code path has low risk but should not be mistaken for wiring up Issue #171 — explicitly out of scope.
- Mitigations and rollbacks: both changes are behavior-preserving except for the targeted defect; a straight `git revert` of the change fully rolls back with no data/schema/config cleanup required.

## Rollout & Follow-up
- Release/rollout steps: standard PR merge to `main`; no phased rollout, feature flag, or data migration required.
- Post-fix monitoring or clean-up tasks: candidate follow-up issues (not part of this change) — (1) fixed-batch-without-backfill pattern in `QfcDatamodel.InitEmailQueue`/`DequeueNextItemGroupAsync` causing "subset of items shown" in high-confidence mode; (2) dormant Issue #171 pre-filter pipeline never wired into the live startup path; (3) `removespecificcontrolgroupcounter` reentrancy-counter hygiene (leaks upward on any exception mid-method, unsynchronized outside the `Interlocked` calls themselves).
- Links: Issue #232 (`https://github.com/drmoisan/TaskMaster/issues/232`); research artifact `artifacts/research/2026-07-03T00-00-quickfiler-kbdactions-duplicate-key-research.md`.
