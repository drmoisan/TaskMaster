# quickfiler-navigation-key-collision (Issue #232)

- Date captured: 2026-07-03
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-navigation-key-collision/ (Issue #232)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #232
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/232
- Last Updated: 2026-07-03
- Work Mode: full-bug

## Summary

QuickFiler throws `System.ArgumentException: Cannot add key because it already exists. Key 2 SourceId Collection` when a page transition (OK/Skip, including the automatic skip triggered by popping out the last item on a page) swaps in a new page without unregistering the outgoing page's keyboard navigation keys or registering the incoming page's keys, leaving stale `"Collection"`-sourced keys in the shared `KbdActions` registry that later collide. Bundled with this fix, at the user's request, is an additive debug-logging change so that every folder-confidence probability calculation is logged (item summary, score, caller) to make a separately-observed "only a subset of items appears in high-confidence mode" symptom empirically diagnosable in future sessions.

## Environment

- OS/version: Windows (VSTO Outlook add-in host)
- Repo/branch: TaskMaster, branch `TaskMaster-wt-2026-07-03-10-11`, HEAD `00507b59`
- Command/flags used: QuickFiler run in "high confidence mode"
- Data source or fixture: Live Outlook inbox (production run, not a test fixture)

## Steps to Reproduce

1. Run QuickFiler in high-confidence mode with a page that has exactly one item.
2. Click the "pop out" button for that item.
3. Internally: `RemoveSpecificControlGroupAsync` unregisters the current page's key(s), removes the item (count reaches 0), and calls `QfcFormController.SkipGroupAsync()` to bring forward the next cached page via `LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)`, which swaps `_itemGroups` without calling `UnregisterNavigation()`/`RegisterNavigation()`.
4. `RemoveSpecificControlGroupAsync` then unconditionally calls `RegisterNavigation()` again for the newly swapped-in page.
5. If any key in the newly-active page's range ("1".."N") is still occupied by an orphaned entry left behind by an earlier page that was abandoned mid-page via the same defective swap path (OK or Skip while items remained), `KbdActions.Add` throws `ArgumentException`.

## Expected Behavior

Keyboard digit-navigation keys ("1".."N", `SourceId = "Collection"`) always match exactly the currently-displayed page's items, regardless of whether the page changed via individual item removal, "OK" (move + load next), or "Skip". No stale keys should ever remain registered for a page that is no longer displayed.

## Actual Behavior

`QfcCollectionController.LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)` (`QuickFiler/Controllers/QfcCollectionController.cs:252-262`) — the swap-in path used by both the "OK" flow (`QfcFormController.EventHandlers.cs:136-143`) and the "Skip" flow (`QfcFormController.EventHandlers.cs:361-395`, also invoked internally from `RemoveSpecificControlGroupAsync`'s zero-item branch) — never calls `UnregisterNavigation()` for the outgoing page or `RegisterNavigation()`/`WireUpAsyncKeyboardHandler()` for the incoming page. Its unused sibling `SwapItemGroups` (line 870-878) shows the correct pattern. `_kbdHandler.StringActionsAsync` is a single, session-lifetime collection shared across all pages, so keys orphaned by this gap collide the next time `RegisterNavigation()` walks from position 0 — which happens unconditionally at the end of `RemoveSpecificControlGroupAsync` (line 1219).

## Logs / Screenshots

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

## Impact / Severity

- [x] High
- [ ] Blocker
- [ ] Medium
- [ ] Low

Rationale: crashes the active QuickFiler session (unhandled exception surfaced to the user) during ordinary bulk-processing use; reachable via everyday OK/Skip page transitions, not an edge case.

## Suspected Cause / Notes

Full diagnosis at `artifacts/research/2026-07-03T00-00-quickfiler-kbdactions-duplicate-key-research.md`. Confirmed root cause: missing `UnregisterNavigation()`/`RegisterNavigation()` pairing in `LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)`, `QuickFiler/Controllers/QfcCollectionController.cs:252-262`. Copilot's initial "overlapping `RemoveSpecificControlGroupAsync` race" hypothesis was investigated and ruled out as the primary explanation — this is a deterministic sequencing/bookkeeping gap, not a race. Recommended fix: route the swap through the existing (currently dead) `SwapItemGroups` method, which already does `UnregisterNavigation(); ...; RegisterNavigation();` correctly, and decide whether the trailing unconditional `RegisterNavigation()` in `RemoveSpecificControlGroupAsync` (line 1219) needs to become conditional to avoid double-registration once the swap path registers correctly on its own.

**Bundled scope (user-requested, same change):** Add `logger.Debug(...)` calls at every point a folder-confidence probability is computed, each capturing item summary (Subject/EntryID), the computed score, and a literal caller-context string, per the existing log4net convention already used throughout this file family. Three call sites identified in Investigation 2 of the same research artifact:
- `QuickFiler/Controllers/QfcDatamodel.cs:316-326` (`ScoreRemainingQueueMailItemAsync`)
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:27-111` (`LoadFolderHandler`/`LoadFolderHandlerAsync`, 4 assignment points)
- `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:62-70` (`FilterAsync` lambda; needs a new `logger` field added)

This logging is intended to make a separately-reported symptom ("only a subset of items appears in high-confidence mode, on the first screen and subsequent screens") empirically diagnosable. Research concluded the subset symptom is most likely explained by an existing fixed-batch-without-backfill pattern (not a threshold bug, and not proven to be caused by score mutation across time) — see Investigation 2 in the research artifact. Fixing that batch/backfill behavior, or wiring up the currently-dead Issue #171 pre-filter pipeline, are explicitly **not** part of this change; they are flagged as separate candidate follow-up issues.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: `QfcFormControllerTests.cs`-style mock-based assertions that `LoadItems(TableLayoutPanel, List<QfcItemGroup>)` invokes `RegisterNavigation()`/`UnregisterNavigation()` in the correct order; a logical-level regression test reproducing the exact reported scenario (1-item page popped out, cached ≥2-item page swapped in, no `ArgumentException` on the subsequent `RegisterNavigation()`); no dedicated new tests required for the additive `logger.Debug` calls beyond confirming they do not throw.
- [ ] Integration scenario to retest: manual QuickFiler high-confidence-mode run exercising OK, Skip, and single-item pop-out transitions across multiple pages.
- [ ] Manual verification notes: after the fix, confirm `_kbdHandler.StringActionsAsync` never accumulates stale entries across a full multi-page processing session.

## Resolution

- Resolved: 2026-07-03T13-45
- Status: Fix implemented and verified; all acceptance criteria AC1-AC10 checked off in `spec.md`.

Two bundled, non-overlapping changes were delivered:

1. **Part A — navigation-key defect fix** (`QuickFiler/Controllers/QfcCollectionController.cs`):
   `LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)` now routes the item-groups swap
   through the existing `SwapItemGroups(List<QfcItemGroup>)` method, which unregisters the outgoing page's
   `"Collection"` navigation keys and registers the incoming page's keys. A method-scoped double-registration
   guard was added to `RemoveSpecificControlGroupAsync` so its trailing unconditional `RegisterNavigation()`
   is skipped when the zero-item branch already registered via `SkipGroupAsync()`. This removes the
   reachability of the reported `System.ArgumentException: ... Key 2 SourceId Collection`. Four MSTest
   regression tests were added to `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` (reported
   repro, swap register/unregister ordering, double-registration hazard, and guarded-register final-state).

2. **Part B — additive probability debug logging** (no control-flow change) at the three folder-confidence
   scoring sites: `QfcDatamodel.ScoreRemainingQueueMailItemAsync`, `QfcItemController.LoadFolderHandler` /
   `LoadFolderHandlerAsync` (4 assignment points), and `QfcHighConfidencePreFilter.FilterAsync` (with a new
   `logger` field added to `QfcHighConfidencePreFilter.cs`). Each logs item Subject/EntryID, computed score
   (and topFolder where applicable), and a caller-context string.

Out of scope (spun out as follow-ups, per Rollout & Follow-up): the fixed-batch-without-backfill "subset of
items shown" behavior (candidate Issue #233), the dormant Issue #171 pre-filter pipeline wiring, and the
`removespecificcontrolgroupcounter` reentrancy-counter hygiene issue. See
`evidence/other/follow-up-candidates.md`.

Final QA (single clean toolchain pass, csharpier -> analyzers -> nullable/TreatWarningsAsErrors -> vstest):
- `evidence/qa-gates/csharpier-final.md` (0 files changed)
- `evidence/qa-gates/msbuild-analyzers-final.md` (0 errors, no new diagnostics)
- `evidence/qa-gates/msbuild-nullable-final.md` (zero new nullable diagnostics introduced by this change)
- `evidence/qa-gates/vstest-final.md` (4641/4641 pass, 0 fail)
- `evidence/qa-gates/coverage-delta.md` (`QfcHighConfidencePreFilter.cs` changed lines 100%; repo-wide flat)

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
