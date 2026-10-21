# Code Review — issue #796 (QuickFiler folder drop-down closes on open; row click does not select)

- Timestamp: 2026-09-07T17-05
- Issue: #796
- Work Mode: full-bug
- Base: `a6b259160f9ac1fbe251708d897fd4721486259e`; head reported by the caller: `8e427fe1`
- Scope reviewed: the full code footprint, 15 paths under `QuickFiler/` and `QuickFiler.Test/`
- Method: read-only inspection of the supplied full-code diff plus direct reads of current file state. The Bash tool was not used, per the caller's binding constraint.

## Verdict

Approve. **Blocking findings: 0.** Seven non-blocking findings are recorded below, three of which
(CR-1, CR-2, CR-3) are recommended for promotion to follow-up issues rather than being fixed on this
branch.

## 1. Does the ordering fix actually establish commit-before-cancel?

This was the caller's specific question. The answer is a qualified yes, and the qualification matters.

### What the mechanism actually is

The change does not reorder anything. It makes the cancel step CONDITIONAL. `BreadcrumbDropDownHost.cs`
line 447:

```csharp
if (reason == BreadcrumbDropDownCloseReason.Uncommitted && !IsCommitPending)
    _cancelSelection();
```

with `IsCommitPending` set at line 255-256 when a close arrives carrying `ExplicitCommit`, and cleared
only in `ShowPopup` (`BreadcrumbDropDownHost.Open.cs` line 123).

### Why that nevertheless satisfies the stated invariant

I traced the commit path rather than accepting the summary. `BreadcrumbDropDownOpenCoordinator.cs`
lines 186-189: when the selector reports it is no longer open, the coordinator calls
`CloseCore(BreadcrumbDropDownCloseReason.ExplicitCommit)`. The selector reports closed BECAUSE the
session already committed the pending identity upstream — `ExplicitCommit` is documented on the enum
(`IBreadcrumbDropDownHost.cs` line 11) as "Enter or row activation explicitly committed the pending
identity".

So by the time `Close(ExplicitCommit)` reaches the host, the commit has already happened in the
session. The commit is therefore structurally before the host-level cancel on that path, and what the
latch adds is protection against a LATER uncommitted-reason close undoing it. The spec's own
Invariant section states the property in exactly this form — "cancels the pending selection if and
only if the close was not caused by this add-in's own activation or focus movement and no selection
commit is in flight" — and the implementation matches that wording, not the looser "commits before
cancels" phrasing in AC3.

### The limit of the protection, stated because it is not stated in the code

The latch protects only the ordering in which the commit's `Close` reaches the host FIRST. If a
native uncommitted-reason close reached `FinishClose` before the commit's `Close(ExplicitCommit)` were
issued, `IsCommitPending` would still be false and the cancel would run.

This limit is not currently a live defect: on all three observed gestures the close carried
`CloseReason=CloseCalled` with `ProgrammaticClose=True`, i.e. it was downstream of the add-in's own
close call, not an independent framework close arriving first. But the added tests exercise only the
commit-first ordering (`NativeCloseWhileCommitPending_DoesNotCancelSelection` sets the latch, then
raises the close), so the reverse ordering is neither protected nor tested. Recording it so a later
reader does not assume more protection than exists. No action required now.

### Scoping is correctly proved

The suppression is proved to be conditional rather than global by three retained assertions that were
deliberately not modified: `CancelCount.Should().Be(1)` at `BreadcrumbPendingOpenCloseTests.cs` lines
48 and 79, plus the new `NativeCloseWithNoCommitPending_StillCancelsSelection`. That is the right
shape — a suppression that drove either of the retained assertions to zero would be a design signal,
and the spec says so in advance rather than after the fact.

## 2. Correctness

### AC2 guard ordering — verified, and it holds

The AC2 guard reads `host.IsOpen`, whereas the AC6 per-item diagnostic reads
`_itemViewer.IsFolderDropDownOpen`, which is `BreadcrumbCoordinator?.IsSelectorOpen == true`
(`ItemViewer.FolderSearch.cs` line 93). These are two different states, so the observation's
`SelectorWasOpen=True` does not by itself establish that the guard would have fired.

I checked the one thing that decides it. `BreadcrumbDropDownOpenLifetime.ShowCurrentSurface` sets
`_host.OpenState = true` (line 268) BEFORE calling `_host.ShowPopup(...)` (line 276), and
`BreadcrumbDropDownHost.IsOpen => OpenState` (line 191). The popup's native show is what takes
activation from the form, so at any deactivation the show provokes, `host.IsOpen` is already true and
the guard fires. The ordering is guaranteed by statement order within one UI-thread operation, not by
timing. Good.

The residual window is narrow and I could not close it from source alone: a deactivation occurring
after the session opened but before `OpenState` is set — i.e. during asynchronous surface creation —
would not be caught. No popup is shown during that window, so a Win32 activation change there is
implausible, but it is not proven absent.

### AC4 latch — correct, with one behaviour worth naming

The latch has two producers (`TextBoxSearch_TextChanged` line 183, `TextBoxSearch_KeyDown` line 220)
and three release sites (Escape at 234, "nothing is open" at 257, and consumption at 265). The
consumer at 263 dismisses only what the search box owns. The mouse path needs no edit because it
never sets the flag, which is the cleanest possible expression of the requirement.

The reasoning for not reusing `_searchLeaveHandoffPending` is correct and I verified it against that
field's own semantics: it is read-and-cleared at lines 247-251, so it is false again immediately after
the handoff it guards while the popup is still open. The two latches genuinely have different
lifetimes and overloading one would have broken #680.

Behaviour worth naming, not a defect: `TextBoxSearch_TextChanged` sets the latch unconditionally. If a
mouse gesture opens the popup and the user then types into the search box, the search box TAKES
ownership of a popup it did not open, and a subsequent leave will dismiss it. That is defensible —
after typing, the search box is the thing driving the list — but it is a transfer of ownership that
neither the comment nor a test states. Consider documenting it.

### AC5 — the guard is meaningful

`RowSetRefreshWhileOpen_NeverClosesHost` asserts `Close` is never invoked across two row-set
replacements while the selector is open. The guard has real content precisely because the
session-preserving replacement path in `UtilitiesCS` is deliberately outside this diff, so what the
test observes is the untouched path. Using the existing headless-viewer plus mocked-host pattern
instead of adding a third harness to the file was the right call.

## 3. Findings

### CR-1 — Stale comment now contradicts the code directly above it (Minor, non-blocking)

`QuickFiler/Viewers/BreadcrumbDropDownHost.cs` line 450, inside `FinishClose`:

```csharp
// Issue #677: only the focus step is gated; the cancel step above always runs.
FocusAnchorIfPermitted
```

The cancel step above no longer always runs. As of line 447 it is gated on two conditions. This
comment was true before this change and is false after it, and it sits three lines below the new
comment that explains the gating — so the file now asserts both that the cancel is conditional and
that it always runs.

`CLAUDE.md` § C#6.3 requires comments to stay synchronized with behaviour. This is the one place in an
otherwise carefully commented diff where that slipped.

Suggested replacement: "Issue #677 / #796: the focus step is gated on the may-take-focus predicate and
the cancel step above is gated on the pending-commit latch; the two gates are independent."

Not blocking: a comment cannot change behaviour, every gate is green, and the misleading text is
adjacent to correct text that explains the actual rule.

### CR-2 — `IsCommitPending` is never cleared on consumption, so a stale latch can survive into the next popup lifetime (Minor/latent, non-blocking)

The XML documentation at `BreadcrumbDropDownHost.Open.cs` lines 102-107 states: "The lifetime is one
popup opening: `ShowPopup` clears it as each fresh native show begins, and nothing else clears it."

That is accurate about the intent but it holds only when a native show actually begins. I traced the
paths that reach `FinishClose(Uncommitted)` and found one that does not:

- `Reset()` → `ResetCoreAsync` reaches `CompleteClose` only `if (OpenState)` (line 316) — guarded.
- `Dispose()` → `DisposeCoreAsync` reaches it only `if (OpenState && !_resetPending)` (line 338) — guarded.
- `RestoreAfterOpenFailure()` (line 455) calls `FinishClose(BreadcrumbDropDownCloseReason.Uncommitted)`
  UNCONDITIONALLY, and it is invoked from `BreadcrumbDropDownOpenLifetime.HandleOpenFailureAsync`
  (line 376), which is reached from the `catch` around the whole open sequence (line 251-255).

So the reachable sequence is: popup opens (`ShowPopup` clears the latch) → user commits
(`Close(ExplicitCommit)` sets it) → popup closes with the latch left true → a later open throws before
`ShowPopup` runs, for example during surface creation → `RestoreAfterOpenFailure` → `FinishClose(Uncommitted)`
→ the cancel is suppressed by a latch belonging to the PREVIOUS popup lifetime, leaving a selector
session open with no popup.

Likelihood is low: it requires an open failure following a committed close in the same host instance.
But WebView2 initialisation failures are not hypothetical in this codebase — the spec's own Log
evidence section cites recurring `WebView2BreadcrumbHost` initialisation errors in a neighbouring
component.

Suggested fix, for a follow-up issue rather than this branch: clear the latch when it is consumed or
when a close completes, for example set `IsCommitPending = false` at the end of `FinishClose`, or
clear it in `RestoreAfterOpenFailure` alongside `OpenState = false`. Either restores the documented
one-popup-lifetime semantics on every path.

### CR-3 — The AC2 guard also gates the #791 Cancel teardown, where its predicate has no meaning (Major/latent, non-blocking)

`ParkFocusAndCancelSelectors` has two callers, and the new guard at
`QfcFormController.Deactivate.cs` line 118 applies to both:

1. `FormViewer_Deactivated` (line 26-27) — the Form.Deactivate event. The guard's semantics fit here.
2. `ActionCancelAsync` stage `"park-focus"` (`QfcFormController.EventHandlers.cs` line 144) — the
   ordered #791 Cancel teardown. Here there is no deactivation at all, so "is this deactivation
   self-inflicted by our own popup?" is not a question the caller is asking. The predicate reduces to
   "is any breadcrumb popup open?", and if one is, the teardown's selector-cancel stage is skipped
   entirely.

The reason this matters is stated in this file's own class-level documentation (lines 12-14): "no
breadcrumb `ToolStripDropDown` may stay open, or WinForms modal menu mode keeps redirecting thread
keyboard messages to the popup after the user has left."

I then walked the mitigation chain rather than stopping at the finding, and all three links exist:

- `ActionCancelAsync` → `"groups-cleanup"` → `QfcCollectionController.Cleanup()` (line 2128) →
  `RemoveControls()` (line 737) → `_itemGroups.ForEach(grp => grp.ItemController.Cleanup())` (line 749);
- `QfcItemController.Cleanup()` (`QfcItemController.ViewerSetup.cs` line 418) →
  `(_itemViewer as ItemViewer)?.ResetBreadcrumb()`;
- `ResetBreadcrumb()` (`ItemViewer.Breadcrumb.cs` line 327) → lifecycle `Reset()` (line 207) →
  `BreadcrumbDropDownOpenCoordinator.Reset()` (line 193), which posts
  `if ((!_host.IsOpen || !_host.Close(Uncommitted)) && _isSelectorOpen()) _cancelSelector();` and then
  `_host.Reset()`.

So the popup IS closed and the selector IS cancelled during Cancel teardown — but at a later stage,
through a different path, and via a fire-and-forget `PostAsync` rather than the synchronous stage the
teardown was ordered to use. The net effect of this change on the Cancel path is a weakened ordering
guarantee, not a lost responsibility. That is why this is non-blocking.

Suggested fix, for a follow-up issue: give `ParkFocusAndCancelSelectors` a parameter such as
`bool honourSelfInflictedGuard`, passed `true` from `FormViewer_Deactivated` and `false` from the
teardown stage, so the guard applies only where its predicate is meaningful. A regression test on the
teardown path ("Cancel with a popup open still cancels every selector at the park-focus stage") would
pin it.

### CR-4 — Dead internal accessor (Minor, non-blocking)

`QfcItemController.EventHandlers.cs` line 209, `SearchOwnsDropDownDismissal`, has no reader anywhere in
the tree. Fully adjudicated in `policy-audit.2026-09-07T17-05.md` § 8 F1; not repeated here. Not
blocking. Recommended: delete it, or give it the reader described in CR-6.

### CR-5 — `_breadcrumbPopupOwners` entries are never removed (Informational)

`QfcFormViewer.cs` lines 1067-1068 of the diff: a `Dictionary<Control, Func<bool>>` that gains entries
in `SetBreadcrumbPopupOwner` and never loses one. Each entry holds a strong reference to an item-viewer
`Control` key and a closure capturing a `BreadcrumbDropDownHost`.

Growth is bounded, so this is informational rather than a leak finding: item viewers are pooled — the
comment at `QfcItemController.ViewerSetup.cs` line 416 says "before releasing the pooled viewer" — and
the dictionary is keyed by viewer, so re-registration replaces rather than appends. The choice of a
keyed dictionary over a list is the right one and is documented as such.

Two smaller notes on the same member. First, the predicate's value after a host is disposed is
`OpenState`, which a disposed host leaves false, so a stale entry cannot wrongly report `true`.
Second, `IsDeactivationSelfInflictedByOwnPopup` invokes every registered predicate on every
deactivation via `.Any(...)`; with nine item groups that is at most nine field reads, so the cost is
immaterial.

### CR-6 — The re-pin sets private state by string field name (Informational)

`QfcItemController.SearchDismissalTests.cs` line 85:

```csharp
QfcItemControllerTestSupport.SetField(controller, "_searchOwnedDismissal", true);
```

This couples the test to a private field's spelling, so a rename breaks it at run time rather than at
compile time. It is also the direct cause of CR-4: because the state is reached by reflection, the
`internal` accessor added for exactly this state never acquired a reader.

The sibling suite shows the alternative works:
`SearchLeaveAfterSearchDrivenOpen_ClosesDropDown` establishes the same state by driving the real
`TextBoxSearch_TextChanged` open path, with no reflection. Driving the real path here too would remove
the string literal and let CR-4's accessor be deleted outright. Minimal-diff was a legitimate reason to
choose reflection for a deliberate re-pin; recording the trade-off.

### CR-7 — The AC2 producer has no automated test (Informational)

`QfcFormViewer.IsDeactivationSelfInflictedByOwnPopup` and the `ItemViewer.Breadcrumb.cs` registration
call are both in types carrying a class-level `[ExcludeFromCodeCoverage]`, and no test exercises
either. The AC2 tests mock `IQfcFormViewer`, so the CONSUMER's gating is proven and the PRODUCER's
derivation is not. Permitted by the ratified WinForms exemption; recorded because the spec's
automation table claims AC2 is automatable "Yes, fully", which is true at the seam and not end to end.

## 4. Design, naming, and error handling — positive observations

These are recorded because they are load-bearing for the verdict, not as praise.

- **Pure formatters.** Making the three diagnostic renderers `static` and argument-driven converts what
  would otherwise have been a source-text scan into a deterministic assertion, and lets the AC6 field
  set be pinned without a popup, a window, or a WebView2. This is the correct way to make logging
  testable and it should be reused.
- **Load-bearing polarity documented at the declaration.** `IQfcFormViewer.cs` states that `false`
  means GENUINE and must not be inverted, and gives the reason: Moq's default `bool` return is `false`,
  so the existing deactivate suite continues to describe a genuine deactivation. That is a contract a
  future editor could otherwise invert without any test failing loudly.
- **A refuted hypothesis is documented at the site that would tempt someone to re-adopt it.** The
  `activeFormIsNull` parameter documentation records that the `Form.ActiveForm` discriminator ran
  opposite to prediction on all four observations and is retained as data only. The
  `IsDeactivationSelfInflictedByOwnPopup` implementation repeats the warning. The decision record adds
  that the refuted discriminator is deliberately NOT inverted and re-used, because one observation of a
  single genuine deactivation is too thin a basis. That reasoning is correct and unusually disciplined.
- **The guard is scoped to the cancel loop and not to focus parking**, and the reason given is
  measured rather than assumed: parking did not run on two of the three defective gestures
  (`WebView2Focused=False`), so suppressing it cannot be what fixes the defect. The decision is
  recorded as an explicit branch (`AC2-PARK-FOCUS-SUPPRESSED: NO`) rather than allowed to happen by
  default, which is what the spec demanded.
- **The 500-line ceiling was managed by relocation rather than by exception.** Moving
  `OnDropDownClosed` into the new part bought headroom, and the main file ended two lines SHORTER than
  it started (498 → 496) despite receiving new code.
- **Error handling is untouched where it should be.** The per-item boundary catch and its rationale
  comment survive verbatim, and the new guard sits outside the `try`, so a self-inflicted deactivation
  returns before the loop rather than short-circuiting inside it.

## 5. Summary of findings by severity

| ID | Severity | Blocking | Summary |
|---|---|---|---|
| CR-1 | Minor | No | Stale `// only the focus step is gated` comment at `BreadcrumbDropDownHost.cs:450` now contradicts line 447 |
| CR-2 | Minor / latent | No | `IsCommitPending` not cleared on consumption; `RestoreAfterOpenFailure` can consume a previous lifetime's latch |
| CR-3 | Major / latent | No | AC2 guard also gates the #791 Cancel teardown caller; mitigated later in teardown by the reset chain |
| CR-4 | Minor | No | `SearchOwnsDropDownDismissal` has no reader anywhere in the tree |
| CR-5 | Informational | No | `_breadcrumbPopupOwners` entries are never removed; bounded by the item-viewer pool |
| CR-6 | Informational | No | Re-pin sets private state by string field name; causes CR-4 |
| CR-7 | Informational | No | AC2 producer side is in coverage-exempt types with no automated test |

**Blocking findings: 0.**

Recommended follow-up: promote CR-1, CR-2 and CR-3 through the potential-to-issue lifecycle so they
survive the merge of this feature folder. CR-3 is the one worth prioritising, because it changes the
ordering guarantees of a teardown path that a previous issue (#791) was opened specifically to make
deterministic.
