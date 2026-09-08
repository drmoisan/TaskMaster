---
name: qfc810-teardown-dropdown-residuals
description: "Issue #810 research (2026-09-07): method-group/optional-param hazard at the park-focus stage, two QuickFiler files pinned at 496/500 lines, dead accessor is in QfcItemController not the host, and a 4th unguarded Cancel() sharer"
metadata:
  type: project
---

Issue #810 consolidates #793 (from the #791 Cancel-teardown review) and #808 (from the #796
drop-down review). Research artifact:
`docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/research/research.2026-09-07T22-10.md`.

Six findings that are not derivable by re-reading the obvious file:

1. **`ParkFocusAndCancelSelectors` is passed as a METHOD GROUP** at
   `QfcFormController.EventHandlers.cs:144` (`RunTeardownStage("park-focus", ParkFocusAndCancelSelectors)`).
   C# method-group conversion does not apply optional-argument defaults, so adding
   `bool honourSelfInflictedGuard = true` breaks that line with CS0123. Use a REQUIRED parameter and
   wrap the teardown call site in a lambda. This is the most likely first-attempt failure.
2. **The dead `SearchOwnsDropDownDismissal` accessor is at
   `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:209`, not in `BreadcrumbDropDownHost.cs`.**
   The #810 delegation prompt says "same file/area" as the host and is wrong. AC6 spans two unrelated
   files.
3. **Two files are pinned at 496 of the 500-line ceiling**: `QuickFiler/Controllers/QfcHomeController.cs`
   and `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`. The host change (latch clear + comment fix)
   cannot fit; `FinishClose` and `RestoreAfterOpenFailure` must be relocated to
   `BreadcrumbDropDownHost.Open.cs` (131 lines), which already owns `IsCommitPending` and `ShowPopup`.
4. **`QfcHomeController.Cleanup()` does not null `_datamodel`** (it nulls five siblings at :390-394 but
   not that one), which is the actual second-pass route to `Cancel()` on the disposed token source.
   The N1 fix should null both.
5. **`UtilitiesCS/Threading/ProgressViewer.cs:75` is a FOURTH sharer** of the same
   `CancellationTokenSource`, calling `_cancelSource!.Cancel()` with no null guard from a user-clicked
   button. The #791 review enumerated only three sharers.
6. **`FinishClose`'s latch clear must go INSIDE the `CompleteAll(params Action[])` operation list**,
   not after the call: `CompleteAll` rethrows the first failure, so a trailing statement would be
   skipped on any throw.

Reusable test assets found: `CloseOrderingHostHarness` in
`QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs:159-266` drives a REAL
`BreadcrumbDropDownHost` headlessly (panel surface, stub messenger, counting delegates, inline
SyncContext) and is exactly the fixture the commit-pending-latch regression needs — no new harness.

**Why:** these six are all "read the neighbouring file, not the named one" facts; each was found only
by reading callers/siblings rather than the file the finding names.

**How to apply:** on any follow-up to #810 or to the #791/#796 family, check the method-group hazard
before adding a parameter to a teardown-stage body, and check the 496-line files before adding a
comment. See [[qfc791-deadline-and-cancel-teardown]] and [[qfc677-webview2-focus-hold-outlook-keyboard]].

Session note: the Bash tool was fully DISABLED in this agent worktree
(`Error: No such tool available: Bash`), so no `git`, `msbuild`, `vstest` or `csharpier` evidence
could be produced; all findings are Read/Grep/Glob only.
