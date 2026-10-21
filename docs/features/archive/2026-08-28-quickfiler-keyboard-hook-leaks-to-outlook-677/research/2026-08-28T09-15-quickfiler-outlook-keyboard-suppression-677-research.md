# Research: QuickFiler keyboard interception leaks to native Outlook windows (#677)

- Date: 2026-08-28
- Author: task-researcher agent
- Scope: root-cause analysis only; no code changes
- Canonical issue: #677

## Summary

While QuickFiler (`QfcFormViewer`, shown modelessly on Outlook's UI thread) is open, clicking
into a native Outlook window leaves Outlook unresponsive to keyboard input, while the mouse and
out-of-process applications work normally. Static analysis of every keyboard-related code path in
this repository shows that no first-party code intercepts, hooks, or filters keyboard input beyond
QuickFiler's own control tree. The mechanism is instead a focus-routing defect: QuickFiler's
breadcrumb pipeline repeatedly and, in one path unconditionally, places Win32 keyboard focus on
WebView2 browser surfaces, and the WebView2 runtime is known to hold keyboard focus in VSTO
WinForms hosts even after the user clicks into the host application (MicrosoftEdge/WebView2Feedback
issue #951, open, "WebView2 WinForms control in Excel VSTO Task pane steals and holds on to
keyboard focus": clicking back into the host changes selection visually, but keyboard input keeps
routing to the WebView2). Because all of Outlook's windows and QuickFiler share one UI thread and
one input queue, a WebView2 child window holding thread focus silently consumes every keystroke
the user types "into" Outlook.

## Current State Analysis

### Live entry point and object lifetimes

- The live launch path is `TaskMaster\Ribbon\RibbonController.cs:104-139` →
  `QfcHomeController.LaunchAsync` (`QuickFiler\Controllers\QfcHomeController.cs:35-84`). The
  `QuickFiler\Legacy\` tree (`QuickFileController`, `QfcLauncher`, `AcceleratorParser`,
  `QfcFormLegacyViewer`) is referenced only from within `QuickFiler\Legacy\` itself (verified by
  repo-wide reference search) and is dead code for this bug. Its `SendKeys.Send("{ESC}")` calls
  (`QuickFiler\Legacy\QuickFileController.cs:460,580`) are unreachable.
- `KeyboardHandler` is created per QuickFiler launch (`QfcHomeController.cs:93,135`, loader at
  `QfcHomeController.cs:181-186`) and nulled in `Cleanup()` (`QfcHomeController.cs:350-359`). It is
  neither static nor a process singleton. `_kbdActive` defaults to `false`
  (`QuickFiler\Controllers\KeyboardHandler.cs:42`) and is toggled only by
  `ToggleKeyboardDialog[Async]` (`KeyboardHandler.cs:206-245`).

### Where keyboard events are wired

Every `KeyboardHandler` subscription targets controls inside the QuickFiler form tree only:

- `QfcFormController.SetupDisposal.cs:149-175` (`RegisterFormEventHandlers`) walks
  `_formViewer.Controls` via `ForAllControls` and attaches `PreviewKeyDown`/`KeyDown`; the
  matching `UnregisterFormEventHandlers` is at `:177-203`.
- `QfcItemController.EventWiring.cs:40-46,81,91` wires per-row controls, the synthetic breadcrumb
  `FolderKeyDown`, and the search textbox.
- `QfcFormViewer.ProcessCmdKey` (`QuickFiler\Viewers\QfcFormViewer.cs:56-73`, mirrored in
  `QfcFormViewerDark.cs:41-49` and `QfcFormViewerExpanded.cs:41-49`) handles only the Alt toggle
  command. `ProcessCmdKey` is invoked by WinForms preprocessing exclusively for messages targeted
  at the form's own window tree; it cannot observe keystrokes destined for Outlook HWNDs.

Even with `KbdActive == true` and populated key/char action tables, these handlers can only fire
for WinForms events raised on QuickFiler's own controls. `AlwaysOnKeyActionsAsync`
(`KeyboardHandler.cs:155-160`) bypasses the `KbdActive` gate but not the wiring scope.

### Process-wide interception surfaces (all verified absent in first-party code)

- No `SetWindowsHookEx`, `WH_KEYBOARD*`, `GetAsyncKeyState`, `RegisterHotKey` anywhere (confirms
  the preliminary reconnaissance).
- No active `Application.AddMessageFilter` (`TaskVisualization\TaskViewer.cs:28` is commented out;
  `MouseDownFilter.PreFilterMessage` always returns `false`).
- No `EnableWindow`, no `Enabled = false` on any window representing Outlook, no `Owner =`
  assignment on the QuickFiler form. All three `Show()` call sites
  (`QfcHomeController.cs:267`, `QfcFormController.Actions.cs:101,157`,
  `QfcFormController.SetupDisposal.cs:32`) pass no owner. The "disabled Outlook window" variant of
  the symptom is therefore excluded: keystrokes are being captured, not rejected.
- No secondary message-pump thread in the live path (no `Application.Run`, no `Dispatcher.Run`;
  `UiThread.Initialize` in `UtilitiesCS\Threading\UiThread.cs:48-79` creates a hidden form and
  captures the existing context on Outlook's UI thread; `ThreadMonitor` runs a watchdog thread that
  does not pump input).
- No `CoreWebView2Controller`, `MoveFocus`, or `IsBrowserAcceleratorKeysEnabled` usage anywhere in
  the repository — i.e., no WebView2 focus management or mitigation exists at all.

### WebView2 population while QuickFiler is open

Every visible item row (`ItemViewer`) hosts two WebView2 instances on Outlook's UI thread: the
breadcrumb anchor `_l0vhBreadcrumb_WebView2` and the mail-body surface `_l0v2h2_WebView2`
(`QuickFiler\Viewers\ItemViewer.Designer.cs:46,49,116,119`). A third, popup WebView2 is created
lazily inside a `ToolStripDropDown` when the breadcrumb selector opens
(`BreadcrumbPopupUiOperations.cs:376`, hosted via `ToolStripControlHost`,
shown at `BreadcrumbPopupUiOperations.cs:101-105`). With N rows per iteration this is 2N+
browser child-window hierarchies alive inside Outlook's process for the whole session; they are
disposed when QuickFiler closes — matching the observation that closing QuickFiler restores
Outlook's keyboard.

## Root Cause

### Mechanism (primary): WebView2 surfaces hold thread keyboard focus after click-out

All Outlook windows and the QuickFiler form share a single UI thread, hence one Win32 input queue
and one focus window. Keystrokes are delivered to whatever HWND holds thread focus — regardless of
which window the user believes is active. Two facts combine:

1. **QuickFiler routinely parks Win32 focus on a WebView2.** The breadcrumb pipeline focuses the
   anchor WebView2 as part of ordinary keyboard filing:
   - `FocusBreadcrumbCore()` → `_l0vhBreadcrumb_WebView2.Focus()`
     (`ItemViewer.Breadcrumb.cs:252-262`).
   - Gesture navigation calls it directly: `QfcItemController.Navigation.cs:33-34,45-46` and
     `QfcItemController.EventHandlers.cs:188-189` (`FocusFolderDropDown()` →
     `FocusBreadcrumb()`, `ItemViewer.FolderSearch.cs:43`). The interface comment at
     `QuickFiler\Viewers\IItemViewer.cs:101-105` already documents this composition as
     "stealing keyboard focus".
   - The popup-open pipeline focuses the popup WebView2 (`_focusPending` =
     `host.ControlHost?.Control.Focus()`, `ItemViewer.Breadcrumb.cs:203`; consumed at
     `BreadcrumbDropDownOpenLifetime.Focus.cs:32-51` and, for the already-open branch, at
     `BreadcrumbDropDownHost.Open.cs:60-69`).
   - Critically, the popup-close pipeline **unconditionally** re-focuses the anchor WebView2:
     `FinishClose` always invokes `_focusAnchor` (`BreadcrumbDropDownHost.cs:410-420`), and
     `_focusAnchor` is `FocusBreadcrumbCore` (`ItemViewer.Breadcrumb.cs:204`). `FinishClose` runs
     for **every** close reason, including `OnDropDownClosed`
     (`BreadcrumbDropDownHost.cs:397-408`) — the event raised when WinForms auto-closes the
     `AutoClose = true` dropdown (`BreadcrumbDropDownHost.cs:165-170`) because the user clicked
     outside it. The re-focus is scheduled asynchronously (`_openLifetime.InvalidateAndSchedule`),
     so it executes **after** the user's click into Outlook has landed. In other words: clicking
     from QuickFiler into an Outlook window while the breadcrumb popup is open triggers a coded,
     deterministic focus steal back into a WebView2 at exactly the moment the user is leaving.

2. **Once a WebView2 holds focus in a VSTO WinForms host, clicking back into the Office host does
   not reliably restore keyboard routing.** This is MicrosoftEdge/WebView2Feedback issue #951
   (open since 2021-02-17, labeled bug/In-progress/tracked; verified by web fetch on 2026-08-28):
   the WebView2 "steals and holds on to keyboard focus"; after clicking back into the host,
   selection changes visually but keyboard input continues routing to the WebView2. That reported
   behavior matches every observed property of this bug: keystrokes into native Outlook windows
   are silently consumed (they are delivered to the browser child HWND and rendered into an
   off-screen/invisible web document), the mouse keeps working (mouse messages are posted to the
   window under the cursor, not the focus window), applications outside the Outlook process are
   unaffected (separate threads/input queues), and closing QuickFiler recovers (the WebView2s are
   disposed and thread focus is released).

The user-visible failure chain: the user files mail with the keyboard (which per the citations in
(1) leaves focus on a breadcrumb WebView2, or re-plants it there via `FinishClose` during the
click-out itself), clicks into the Outlook Explorer/Inspector, Outlook activates and repaints, but
the thread's focus window remains (or asynchronously returns to) a WebView2 inside the inactive
QuickFiler form. Every subsequent keystroke is swallowed by that browser surface until QuickFiler
is closed.

### Mechanism (secondary, popup-open window only): WinForms modal menu mode

`BreadcrumbDropDownHost` shows a `ToolStripDropDown` with `AutoClose = true`
(`BreadcrumbDropDownHost.cs:165-170`) via `ToolStripDropDown.Show`
(`BreadcrumbPopupUiOperations.cs:101-105`). While such a dropdown is open, WinForms enters modal
menu mode (`ToolStripManager.ModalMenuFilter`), and — because `Application.MessageLoop` is false
in a VSTO host, where Outlook pumps the messages — the .NET Framework installs its hosted-message
hook (`HostedWindowsFormsMessageHook`, a thread-scoped `WH_GETMESSAGE` hook inside
System.Windows.Forms) and redirects keyboard messages for the whole Outlook UI thread to the
active dropdown. This is a real "keyboard hook scoped to the whole Outlook window set" that
QuickFiler indirectly creates, and it is invisible to repo-level greps for `SetWindowsHookEx`
because the framework, not this repository, installs it. Caveat on evidence strength: the
ModalMenuFilter/hosted-hook behavior is asserted from .NET Framework reference-source knowledge,
not verified in this session. In the normal path it is transient — the click into Outlook itself
dismisses the dropdown and exits menu mode — so it explains keystroke loss only while the popup is
open, not the persistent symptom. It becomes relevant to the fix because any change must ensure
the popup is actually closed (not merely orphaned) whenever the QuickFiler form deactivates, so
menu mode cannot outlive the user's departure.

### Rejected mechanisms (with evidence)

- First-party global hook / message filter: none exists (searches above).
- `KeyboardHandler` mis-scoping: wiring is strictly form-internal
  (`QfcFormController.SetupDisposal.cs:149-175`, `QfcItemController.EventWiring.cs:40-91`); the
  handler instance is per-launch, not shared or static (`QfcHomeController.cs:93,181-186,350-359`).
  A stale `KbdActive == true` cannot affect Outlook windows because no Outlook-window surface is
  subscribed.
- Win32 window disabling / modal ownership: no `ShowDialog` on the main form, no `Owner`
  assignment, no `EnableWindow`/`Enabled = false` on any Outlook wrapper anywhere in QuickFiler.
- Legacy accelerator tree: unreachable from the live entry point.
- Timer/Idle-driven activation stealing: no `Activate()`/`BringToFront()`/`TopMost` on the live
  QuickFiler form, no `Application.Idle` focus grabs (`ApplicationIdleTimer` is unrelated to
  focus).

## User Hypothesis: Verdict

**Refuted in its literal form; its spirit is confirmed.** There is no QuickFiler keyboard hook
scoped to the Outlook process: `KeyboardHandler` is ordinary WinForms event wiring confined to the
QuickFiler control tree (citations above), and no first-party hook, filter, or Outlook-window
subscription exists. However, the hypothesis's underlying intuition — that something QuickFiler
brings into the process affects keyboard delivery to the entire Outlook window set — is correct
one layer down: (a) the WebView2 runtime instances QuickFiler hosts hold thread-wide keyboard
focus (WebView2Feedback #951), a state QuickFiler's own focus calls actively create and, in the
`FinishClose` path (`BreadcrumbDropDownHost.cs:410-420`), re-create at the moment of click-out;
and (b) while the breadcrumb popup is open, WinForms menu mode filters keyboard messages for the
whole Outlook UI thread.

## Minimal Correct Scope Boundary

Keyboard interception and focus placement must satisfy: *QuickFiler may take or restore keyboard
focus only while the QuickFiler form owns activation; once the user moves activation to any
non-QuickFiler window, QuickFiler must neither hold nor re-acquire the thread's focus window, and
no QuickFiler-created popup (and therefore no menu-mode filter) may remain open.*

## Recommended Fix Approach (research only — no code written)

Selected approach: **activation-guarded focus restoration plus deactivate-time focus parking**, in
three coordinated parts:

1. **Guard the unconditional `_focusAnchor` in `FinishClose`.** Restore focus to the breadcrumb
   anchor only when the QuickFiler form still owns focus/activation — e.g., inject a
   `Func<bool>` "may take focus" predicate into `BreadcrumbDropDownHost` (alongside the existing
   `_focusPending`/`_focusAnchor` delegates, `BreadcrumbDropDownHost.cs:27-28,158-159`) that the
   `ItemViewer` implements as "the form containing `_l0vhBreadcrumb_WebView2` is the active form /
   contains focus". Apply the same predicate to the late-arriving `_focusPending` executions
   (`BreadcrumbDropDownOpenLifetime.Focus.cs:32-51`, `BreadcrumbDropDownHost.Open.cs:60-69`) so an
   open that completes after the user has clicked out does not pull focus back. This removes the
   deterministic click-out steal.
2. **Park focus off WebView2 when the form deactivates.** Handle `Form.Deactivate` on
   `QfcFormViewer` (routed through the controller like the existing Seam B events,
   `QfcFormViewer.cs:128-147`): if the current focused control within the form is a WebView2,
   move WinForms focus to a benign non-WebView2 control of the form (focus parking is the
   established mitigation family for WebView2Feedback #951), and close any open breadcrumb
   selector via the existing `BreadcrumbCoordinator.CancelSelector()`/host `Close` path so the
   `ToolStripDropDown` — and with it WinForms menu mode — cannot outlive the deactivation.
3. **No change to `KeyboardHandler` wiring.** It is already correctly scoped. Optionally (defense
   in depth, not required for the fix): gate `KbdActive` processing on
   `_formViewer.ContainsFocus`, but this has no effect on the reported symptom because the
   handlers cannot receive events from Outlook windows.

Rejected alternatives (brief):

- *Suppress `_focusAnchor` entirely*: breaks the intended in-form behavior that Escape/commit
  returns the caret to the breadcrumb (issue #438/#400 acceptance criteria); the guard preserves
  in-form behavior and changes only the cross-window case.
- *Move to `CoreWebView2Controller.MoveFocus`/controller-level focus management*: no
  `CoreWebView2Controller` surface is used anywhere in the repo today; introducing it is a larger
  architectural change than the guard requires and does not address the popup menu-mode residue.
- *Rewrite the popup off `ToolStripDropDown`*: removes the menu-mode filter but not the primary
  WebView2 focus-retention mechanism, at much higher cost.

## Behavior Semantics

- Success: with QuickFiler open (any state: navigation on/off, popup open/closed, mid-search),
  clicking into any native Outlook window and typing must operate Outlook normally; returning to
  QuickFiler by click restores QuickFiler keyboard navigation; Escape/commit inside QuickFiler
  still returns the caret to the breadcrumb anchor.
- Failure/edge cases to cover: click-out while the popup is open (the `FinishClose` steal), click-
  out while an open is in flight (late `_focusPending`), click-out during search typing (per-
  keystroke close/reopen churn from #438), and deactivate while a WebView2 (anchor, body, or
  popup) holds focus.
- Ordering rule: the "may take focus" predicate must be evaluated at execution time of the
  scheduled focus action (not at scheduling time), because the steal arises precisely from the
  scheduling/execution gap.

## Requirements Mapping (proposed shape, no code)

- `BreadcrumbDropDownHost`: additive optional constructor delegate `Func<bool>` focus-permission
  predicate, defaulting to `() => true` to keep every existing test green; consulted in
  `FinishClose` before `_focusAnchor` and in `FocusCurrentSurface`/already-open refocus before
  `_focusPending`.
- `ItemViewer.Breadcrumb.cs`: supply the predicate from the production wiring at
  `ConfigureBreadcrumbDropDown` (`ItemViewer.Breadcrumb.cs:197-207`); implement it against
  `FindForm()` activation/containment.
- `QfcFormViewer` + `QfcFormController`: Seam-B style `FormDeactivated` event and a controller
  handler that parks focus and cancels the open selector.

## Testing Implications

- The existing seams already support deterministic unit tests: `Mock<IBreadcrumbDropDownHost>`,
  the injectable `showPopup`/`closePopup` delegates (`BreadcrumbDropDownHost.cs:86,140-141`), and
  the focus delegates are constructor-injected, so "focus not invoked when predicate false" and
  "focus invoked when predicate true" are directly assertable with Moq + FluentAssertions under
  MSTest, headless.
- The `FinishClose`-on-`OnDropDownClosed` path is coverable by raising `DropDown.Closed` on the
  real host with stub delegates (pattern already used by `BreadcrumbDropDownHostTests`
  FocusPendingCount cases per prior #438 research).
- The deactivate-parking handler can be tested at the controller level through the viewer
  interface event without showing a window.
- What cannot be unit-tested: the WebView2-runtime focus retention itself and actual Outlook
  keystroke delivery — see Automation Feasibility.

## Automation Feasibility

Manual interactive verification in a live Outlook session is required to confirm the fix
end-to-end. This is expected and acceptable, and it does not block automated code-level
investigation, static analysis, or automated unit/regression testing of the isolated logic (the
focus-permission predicate, the deactivate parking handler, and the popup-close-on-deactivate
ordering are all deterministically unit-testable through existing seams, as described above).

Reasons live verification is mandatory: the failure exists only in the composition of (a) Outlook's
native windows and message pump, (b) real WebView2 runtime child windows and their focus behavior
(WebView2Feedback #951 is a runtime defect, not reproducible with mocks), and (c) real Win32
activation/focus transitions from user clicks. None of these can be exercised in a headless MSTest
run without violating the repository's determinism and no-external-process test policies.

Provenance of findings in this document:

- Derived from static code reading of this repository (all file/line citations): the wiring scope
  of `KeyboardHandler`, the absence of hooks/filters/window-disabling, the focus call graph, the
  unconditional `_focusAnchor` in `FinishClose`, the `AutoClose = true` popup configuration, the
  per-row WebView2 population, and the live-vs-legacy entry-point determination.
- Verified by web fetch (2026-08-28): existence, open status, and symptom description of
  MicrosoftEdge/WebView2Feedback issue #951.
- Asserted from .NET Framework reference-source background knowledge, not verified this session:
  the `ToolStripManager.ModalMenuFilter`/`HostedWindowsFormsMessageHook` thread-hook behavior in
  hosted (no `Application.MessageLoop`) processes. Treat as a candidate contributor to be
  confirmed (or ruled out) during live verification, e.g., by checking whether keyboard loss ever
  occurs when no breadcrumb popup has been opened in the session.
- Not verified by any means (unknown): which exact focus path (gesture navigation residue vs.
  `FinishClose` steal vs. late `_focusPending`) dominates in the user's specific repro; the live
  session should log the focused HWND at click-out time if disambiguation is needed.
