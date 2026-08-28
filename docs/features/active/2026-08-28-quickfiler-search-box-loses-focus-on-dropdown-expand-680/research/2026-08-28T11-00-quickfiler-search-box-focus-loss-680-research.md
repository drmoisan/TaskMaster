# QuickFiler search box loses focus on drop-down auto-open — Research (#680)

- **Issue:** #680
- **Date:** 2026-08-28T11-00
- **Author:** task-researcher agent
- **Status:** Root cause identified (static code analysis + framework source verification)
- **Worktree analyzed:** `<repo-root>` at branch `TaskMaster-wt-2026-08-28T08-42`, HEAD `361a49b8` (merge of PR #676)

## 1. Summary and root cause

Typing the first character into the QuickFiler folder-search textbox correctly opens the
breadcrumb results drop-down, after which no further keystrokes reach the textbox until the user
manually dismisses the drop-down and clicks back in — allowing exactly one more character before
the cycle repeats.

**Root cause: WinForms modal-menu-mode keyboard capture, engaged as a side effect of showing the
`ToolStripDropDown` popup.** This is mechanism (c) of the delegation's hypothesis list — not a
managed focus call, not (by itself) a WebView2 focus grab, and not a regression of #438's fix.
The chain:

1. The keystroke path is fully non-focusing at the managed level (the #438 fix is intact and
   verified below). No managed code moves focus off the textbox on a search-driven open.
2. The popup is a `ToolStripDropDown` constructed with `AutoClose = true`
   (`QuickFiler/Viewers/BreadcrumbDropDownHost.cs:165-171`) and shown via
   `dropDown.Show(anchor, point)`
   (`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:101-105`, `ShowOwnedPopup`).
3. In the WinForms framework, `ToolStripDropDown.SetVisibleCore(true)` for a `TopLevel` dropdown
   calls `ReparentToActiveToolStripWindow()`, whose first statement is
   `ToolStripManager.ModalMenuFilter.SetActiveToolStrip(this)`. This call is unconditional for
   top-level dropdowns on the open path. `SetActiveToolStripCore` then enters **menu mode**
   (`EnterMenuModeCore`) — *unless* the dropdown's `AutoClose` is `false`, in which case the
   framework explicitly returns early with the comment "Don't actually enter menu mode."
   (Verified verbatim against dotnet/winforms source; see §7 for the verification basis.)
4. Menu mode installs an application-level message filter. Its `PreFilterMessage` handling for
   keyboard messages (`WM_KEYDOWN` … `WM_SYSDEADCHAR`) contains, verbatim:

   ```csharp
   if (!activeToolStrip.ContainsFocus)
   {
       // Route all keyboard messages to the active dropdown.
       m.HWnd = activeToolStrip.Handle;
   }
   ```

   The popup window itself is shown with `SW_SHOWNOACTIVATE` (verified in the framework's
   `ShowParams`), so Win32 focus genuinely stays in the search textbox — which means
   `activeToolStrip.ContainsFocus` is **false**, which means **every subsequent keyboard message
   is retargeted to the drop-down's window handle and never reaches the textbox**. The #438 fix's
   very success (focus stays in the textbox) is what makes the retargeting branch active.
5. When the user clicks back into the textbox, the modal menu filter's mouse handling dismisses
   the `AutoClose = true` popup (close reason `AppClicked`), exiting menu mode. The next
   character then reaches the textbox, re-runs the search handler, reopens the popup, and
   re-enters menu mode — producing the observed one-character-per-cycle loop.

The user-visible report of "loses keyboard focus" is the correct phenomenology of keystroke
retargeting: the caret owner never receives another key message, which is indistinguishable to
the user from focus loss. Win32 focus itself likely does not move on open (the show is
`SW_SHOWNOACTIVATE` and no managed focus call runs); what is captured is the *keyboard message
stream*, not the focus.

### Secondary contributor (close side, worktree-state dependent)

In this worktree, `BreadcrumbDropDownHost.FinishClose`
(`QuickFiler/Viewers/BreadcrumbDropDownHost.cs:410-420`) still unconditionally invokes
`_focusAnchor` (→ `FocusBreadcrumbCore` → collapsed breadcrumb `WebView2.Focus()`,
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:252-262`) on every close, including the
`OnDropDownClosed` auto-close path (`BreadcrumbDropDownHost.cs:397-408`). This is exactly the
#677 mechanism and it adds friction to the recovery click (the click that dismisses the popup is
followed by a managed refocus of the anchor WebView2). Per the delegation, PR #684 gates this
with a `MayTakeFocus` predicate; that fix is **not present in this worktree** (see §6), so its
post-merge interaction could not be read directly. Regardless of #684's state, the menu-mode
capture in §1 is sufficient on its own to produce the reported one-character loop.

## 2. Verified keystroke-to-open trace (all managed steps are non-focusing)

| Step | Location | Behavior |
|---|---|---|
| Keystroke → `TextChanged` | `QuickFiler/Viewers/ItemViewer.FolderSearch.cs:67-71` (event forwards `TxtboxSearch.TextChanged`) | Plain WinForms `TextBox` (`ItemViewer.Designer.cs:224-234`) |
| Handler | `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:173-182` | `FindFolder` + single `PresentFolderSearchResults(folders)` call; no focus intent, no drop-down state call (the #438 shape) |
| Viewer forwarding | `ItemViewer.FolderSearch.cs:38-39` → `ItemViewer.Breadcrumb.cs:289-297` | Bare-viewer branch deliberately performs no focus call |
| Lifecycle | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs:34-43` | Latches `LatchNextOpenTakesNoFocus()` **before** the composite runs |
| Composite | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs:47-100` | Replace-preserving-session → open-if-closed → pending-only highlight; one render; raises `SelectorOpenStateChanged` only when actually opened |
| Open request | `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:171-184` → `RequestOpen` (:104-118) → `BeginOpenCore` (:232-264) | Consumes the latch exactly once (:245-246) and calls `_host.OpenAsync(..., takeFocus: false)` (:257-259) |
| Host open | `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs:52-72` | Already-open branch schedules `_focusPending` only when `takeFocus` is true (:65) |
| Open lifetime | `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:215-256` | `EnsureSurfaceAsync` → placement → `ShowCurrentSurface` (`_host.ShowPopup`, :258-278) → `FocusCurrentSurface(lease, takeFocus)` |
| Focus step | `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.Focus.cs:32-51` | `_host.FocusPending()` skipped when `takeFocus` is false (:39-40) |
| Native show | `BreadcrumbDropDownHost.cs:269` → `BreadcrumbPopupUiOperations.cs:101-105` | `DropDown.Show(anchor, anchor.PointToClient(screenLocation))` — this is where the framework enters menu mode |

Conclusion of the trace: hypothesis (b) — coordinator-level explicit focus moves — is **excluded**
by direct reading of every step above. The defect enters at the final native `Show` call, inside
the framework.

### Why the first-open WebView2 focus grab — hypothesis (a) — is not the primary mechanism

The popup's WebView2 surface is created once and reused: `EnsureSurfaceAsync` early-returns when
`_host.HasInstalledSurface` (`BreadcrumbDropDownOpenLifetime.cs:289-292`). A first-creation
CoreWebView2 focus grab (the #438 spec's second residual risk, and the #677 family's upstream
WebView2Feedback #951 behavior) could aggravate the *first* cycle only; it cannot explain the
recurring per-reopen one-character loop, because subsequent opens create no new surface. Menu-mode
capture is unconditional on every open of the `AutoClose = true` popup and alone explains every
reported observation. Whether a first-open grab additionally occurs is only observable live
(same HV class as #438's HV-1) and does not change the recommended fix.

## 3. Relationship to #438 — documented residual risk materialized, not a regression

Evidence from `docs/features/archive/2026-08-07-quickfiler-search-keystroke-focus-steal-438/spec.md`:

- #438's symptom was the same user-level report, but its verified root causes were **managed**
  focus transfers: open-side `_focusPending` / `Schedule(_focusPending)` and close-side
  `_focusAnchor` via per-keystroke session close/reopen, plus a committed-selection mutation
  (spec §"Root Cause Analysis", items 1-3).
- #438's fix design (spec §"Proposed Fix") is fully present and intact in this worktree — every
  guard verified with citations in §2 above. The #438 regression suite also exists and pins the
  managed seam: `QuickFiler.Test/Controllers/QfcItemController.SearchFocusRegressionTests.cs`
  (AC-1 negative/positive assertions), plus `BreadcrumbDropDownSearchIntegrationTests(.Part2)`,
  `BreadcrumbDropDownOpenCoordinatorTests.Part3`, and
  `BreadcrumbItemViewerLifecycleCoordinatorTests`. These tests were not run in this session
  (research-only), but reading them against the current production code shows their assertions
  still match the shipped shapes; nothing in the #438 fix was undone.
- Critically, #438's spec **predicted this exact gap and scoped it out of automation**:
  - Risks: "`ToolStripDropDown.AutoClose = true` is expected to keep the non-activated popup
    open while the user types in the same form; this is inferred WinForms behavior, not provable
    in a unit test." (spec, Risks bullet 1)
  - HV-1: "type an eight-character folder name at normal speed and confirm the caret never
    leaves the textbox" — live-Outlook-only, explicitly **not a merge gate**, with "a negative
    outcome is promoted as its own issue rather than blocking #438 delivery."

**Determination: #680 is HV-1's negative outcome — the residual native-behavior risk #438
documented and deferred, now confirmed as a real defect with a source-level mechanism.** The
inferred WinForms behavior in #438's risk register was wrong in one respect: the popup does stay
open while typing, but menu mode diverts the typing itself. #438's regression tests should stay
green before and after the #680 fix, because the defect lives below the managed seam they pin.

## 4. Relationship to #677 — same problem family, distinct mechanism; `MayTakeFocus` cannot fix it

- #677 (per the delegation summary of PR #684; artifacts not on disk here, see §6): close-side
  managed refocus (`FinishClose` → `_focusAnchor`) into a WebView2 surface that then retains
  Win32 focus (upstream WebView2 runtime behavior). Fixed by gating the managed focus calls with
  a `MayTakeFocus` predicate plus a `Form.Deactivate` focus-parking handler.
- #680: open-side **framework** keyboard capture. No managed focus call participates, and no
  WebView2 focus retention is required for the symptom. A `MayTakeFocus`-style guard gates
  *focus-taking calls*; menu mode is not a focus-taking call and is engaged inside
  `ToolStripDropDown.SetVisibleCore` before any host code can intervene. **Widening #677's guard
  cannot address #680.** #680 needs its own suppression at the only public-API lever the
  framework provides: the `AutoClose` gate on menu-mode entry (§5).
- #677's spec reportedly carried a Rollout & Follow-up item flagging the WinForms modal-menu-mode
  contributor as "asserted, not verified." **#680 is that contributor, now verified at framework
  source level** (§7). The #680 fix should be recorded as discharging that follow-up item.
- Interaction note for the planner: #684's `FinishClose` gating (once merged) and the #680 fix
  touch the same host. The #680 change must be built on a base that includes #684 and must not
  reintroduce an unconditional `_focusAnchor` path.

## 5. Recommended fix approach (minimal scope; description only — no code changed in this task)

### Minimal correct behavior

While the user is typing in `TxtboxSearch`: every keystroke is delivered to the textbox; the
drop-down auto-opens/refreshes without capturing the keyboard; caret and Win32 focus remain in
the textbox continuously. Explicit gestures (Down arrow, mouse toggle, row click) keep their
current focus-on-open, auto-close-on-outside-click semantics (#400 AC-13 as qualified by #438).

### Recommended: extend the existing `takeFocus: false` intent to a "non-capturing open" (suppress menu mode via `AutoClose`)

The framework's own escape hatch is exact: `ModalMenuFilter.SetActiveToolStripCore` skips menu
mode entirely for a dropdown whose `AutoClose` is `false` (verified verbatim, §7). Therefore:

1. In `BreadcrumbDropDownHost`, key off the already-threaded `takeFocus` intent: for a
   `takeFocus: false` open, set `DropDown.AutoClose = false` **before** `ShowPopup` runs (menu
   mode is entered inside `SetVisibleCore(true)`, so the property must be set pre-`Show`).
   The FIFO `BreadcrumbPopupUiOperations` queue makes the ordering deterministic, mirroring the
   #438 latch design.
2. Restore `AutoClose = true` at the two transitions where standard popup semantics must resume:
   (a) a focusing (gesture) open or focus handoff into the popup — e.g., the Down-arrow path
   (`TextBoxSearch_KeyDown`, `QfcItemController.EventHandlers.cs:184-193`) and the already-open
   `takeFocus: true` branch (`BreadcrumbDropDownHost.Open.cs:62-70`); and (b) close completion
   (`CompleteClose`/`FinishClose`), so the next lifecycle starts from the default.
3. Own the dismissal paths that `AutoClose = true` previously provided while the search popup is
   in the non-capturing state:
   - Programmatic closes are unaffected: `CloseNative` uses
     `ToolStripDropDownCloseReason.CloseCalled` (`BreadcrumbDropDownHost.cs:384-395`), and the
     framework's closing branch cancels non-`CloseCalled` closes only when `AutoClose` is false —
     `CloseCalled` always passes (verified in `SetVisibleCore`'s false branch, §7).
   - Outside-click / focus-departure dismissal needs an explicit managed trigger: close the
     search popup when `TxtboxSearch` loses focus to anything other than the popup surface
     (viewer `Leave`/`LostFocus` seam routed through the controller), and/or the form-level
     `Deactivate` handler that #684 introduced. Escape-while-typing likewise needs a
     textbox-side route to the existing cancel path (`CancelSelector`), since Escape no longer
     reaches the popup through menu mode.

This is the smallest change that removes the capture: it reuses the exact intent boundary #438
built (`takeFocus`), touches primarily `BreadcrumbDropDownHost`(+`Open.cs`) and the
open-coordinator handoff, is additive at the seams, and leaves every gesture path untouched.

Deterministic fail-before regression (unit-testable, no live Outlook): the host tests already
inject the `_showPopup` delegate (`BreadcrumbDropDownHost.cs:143-163` internal constructor).
Assert `DropDown.AutoClose == false` at the moment the injected show delegate executes for a
`takeFocus: false` open (fails today: it is `true`), `== true` for a gesture open, and `== true`
again after close/gesture-handoff. Menu-mode engagement itself is not unit-observable (§8).

### Rejected alternatives (brief)

- **Refocus the textbox after show.** Ineffective: the filter retargets keys precisely when the
  active dropdown does *not* contain focus; focus location is not the problem.
- **Exit menu mode after show** (`ToolStripManager.ModalMenuFilter.ExitMenuMode`). Internal
  framework API; would require reflection against framework internals. Brittle, policy-hostile.
- **Replace `ToolStripDropDown` with an owned borderless `Form` shown `SW_SHOWNOACTIVATE`.**
  Robust (no menu filter at all) and directionally aligned with the VSTO-exit preference, but it
  rewires the #400 host architecture (placement, lifecycle, close reasons, ~all host tests).
  Not minimal for a defect fix; viable long-term follow-up if `AutoClose` toggling proves
  fragile in live verification.

## 6. Evidence-state caveats (worktree and artifact availability)

- This worktree (`TaskMaster-wt-2026-08-28T08-42`, HEAD `361a49b8` = PR #676 merge) **does not
  contain PR #684**: `MayTakeFocus` has zero grep hits repo-wide, `FinishClose` is unconditional
  at `BreadcrumbDropDownHost.cs:410-420`, and no `Deactivate` handler exists in `QuickFiler`.
- The #677 feature folder
  (`docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/`) named in
  the delegation does not exist in this worktree, in the primary checkout, or in the one other
  worktree on disk; the #677 research and spec could not be read. All #677/#684 characterization
  in this artifact derives from the delegation prompt's summary and is marked accordingly.
- Consequence for planning: the #680 fix must be planned against a base that includes #684; the
  file/line citations for `BreadcrumbDropDownHost.cs` in this artifact will shift once #684's
  `MayTakeFocus` changes land in that file.

## 7. Framework-source verification basis

Verified verbatim by fetching `dotnet/winforms` (branch `main`) sources over the network:

- `ToolStripDropDown.cs` → `SetVisibleCore(true)`: `if (TopLevel) { ReparentToActiveToolStripWindow(); }`;
  `ReparentToActiveToolStripWindow()` body begins `ToolStripManager.ModalMenuFilter.SetActiveToolStrip(this);`.
  `ShowParams` returns `SW_SHOWNOACTIVATE`. `SetVisibleCore(false)`: `e.Cancel = e.CloseReason != ToolStripDropDownCloseReason.CloseCalled && !AutoClose;`.
- `ToolStripManager.ModalMenuFilter.cs` → `SetActiveToolStripCore`: for a dropdown with
  `!dropDown.AutoClose`, stores the active HWND and returns before entering menu mode
  ("Don't actually enter menu mode."). `PreFilterMessage` keyboard branch:
  `if (!activeToolStrip.ContainsFocus) { m.HWnd = activeToolStrip.Handle; }`.

Caveat: the application targets .NET Framework 4.8.1. The .NET Framework reference source
(referencesource.microsoft.com) is offline (301 to GitHub) and the GitHub mirror directory was
not reachable in this session, so Framework-vs-.NET byte-level parity of these methods is
**asserted, not verified**. The `ModalMenuFilter` design (menu-mode entry on dropdown show,
`AutoClose == false` opt-out, keyboard retargeting) predates .NET Core WinForms and is the
long-standing desktop behavior; the risk that net48 diverges on the load-bearing points is low
but must be discharged by the live HV step below.

## 8. Automation Feasibility

- **Code-level investigation: fully automated/static.** Every managed-pipeline claim in §1-§2 is
  from direct reading of this worktree's sources with file/line citations. No production file,
  configuration, or test was modified or executed in this session.
- **Framework mechanism: verified against dotnet/winforms source over the network** (§7), with
  the stated net48 parity caveat.
- **Automated regression testing of the fix: feasible and deterministic.** The `AutoClose`
  state machine (false-before-show for `takeFocus: false`, true for gestures, restored on
  close/handoff) is assertable through the existing injected `_showPopup`/`_closePopup` host
  seams and the real-`ToolStripDropDown`-no-show harness pattern already used by
  `BreadcrumbDropDownHostTests`. The latch/consume semantics and the dismissal-ownership paths
  (textbox-leave close, Escape cancel) are assertable at the existing coordinator and controller
  mock seams. A genuine fail-before test exists at the host seam (§5).
- **End-to-end confirmation requires manual interactive verification in a live Outlook
  session — exactly as #677 and #438's HV-1 did. This is expected and does not block the
  automated work above.** Menu-mode engagement, keyboard-message retargeting, `SW_SHOWNOACTIVATE`
  behavior under the VSTO-hosted message loop, any first-open CoreWebView2 focus grab, and the
  dismissal UX with `AutoClose = false` are only observable with a real message pump, a real
  popup window, and a live WebView2 — none of which are permitted or reliable in unit tests
  (no-temp-file, no-external-process, determinism policies). Recommend an HV runbook item
  mirroring #438's HV-1: type an eight-plus-character folder name at normal speed; confirm every
  character lands and the drop-down tracks each keystroke; then verify gesture paths (Down arrow
  handoff, outside-click dismissal, Escape) still behave per #400/#438.

## 9. Testing implications (for the planner; no test code written here)

1. Host seam (fail-before): `AutoClose` is false when the injected show delegate runs for a
   `takeFocus: false` open; true for the 3-parameter (gesture) open; restored to true after
   `Close(...)` completes and after a `takeFocus: true` open on an already-open popup.
2. Coordinator seam: the non-capturing intent rides the existing latch; two consecutive search
   refreshes still produce one `OpenAsync` / zero `Close` (preserve #438 AC-3 unmodified).
3. Controller seam: new dismissal ownership — search-box leave/Escape routes exactly one close
   or cancel intent; existing `SearchFocusRegressionTests` assertions remain green unmodified.
4. Contract tests: any new host member is additive; `ItemViewerBreadcrumbDropDownContractTests`
   passes unmodified (mirror #438 AC-10 discipline).
5. All #438 and #400 suites named in the #438 spec's Test Strategy must pass unmodified; this
   fix operates strictly below the seams they pin.
