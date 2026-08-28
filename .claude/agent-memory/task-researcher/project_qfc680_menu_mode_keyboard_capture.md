---
name: qfc680-menu-mode-keyboard-capture
description: 'Issue #680: search-box keystroke loss is WinForms ModalMenuFilter menu-mode keyboard retargeting on ToolStripDropDown show (AutoClose=true), NOT a #438 regression and NOT fixable by widening #677 MayTakeFocus; AutoClose=false is the framework opt-out'
metadata:
  type: project
---

Issue #680 research (2026-08-28, artifact in `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/research/`).

**Why:** three plausible mechanisms (WebView2 grab, managed focus call, menu mode) look identical at the user level; the verified mechanism dictates a completely different fix than #677's guard.
**How to apply:** any breadcrumb-popup keyboard/focus work must account for menu mode: showing a TopLevel `ToolStripDropDown` with `AutoClose=true` unconditionally calls `ModalMenuFilter.SetActiveToolStrip` inside `SetVisibleCore(true)` and enters menu mode; the filter then retargets ALL keyboard messages (`m.HWnd = activeToolStrip.Handle`) whenever `!activeToolStrip.ContainsFocus`. So a NON-focusing open (the #438 fix) is exactly what activates the retargeting. `AutoClose=false` set BEFORE Show is the only public-API opt-out ("Don't actually enter menu mode" early return in `SetActiveToolStripCore`); programmatic `Close(CloseCalled)` still works with AutoClose=false, but outside-click/Escape dismissal must be reimplemented in managed code.

Other verified non-obvious findings:
- #438's fix is fully intact (latch -> takeFocus:false -> FocusPending skipped); its regression suite stays green because the defect is below the managed seam. #680 = #438's documented HV-1 residual risk materializing (spec Risks bullet 1 predicted "inferred WinForms behavior, not provable in a unit test").
- Popup WebView2 first-creation focus grab cannot be the recurring mechanism: `EnsureSurfaceAsync` early-returns on `HasInstalledSurface`, so only the first cycle could involve it.
- The popup is shown `SW_SHOWNOACTIVATE`; Win32 focus genuinely stays in the textbox — the keyboard STREAM is captured, not focus.
- Deterministic fail-before test exists WITHOUT live Outlook: assert `DropDown.AutoClose == false` at the moment the injected `_showPopup` delegate runs for a takeFocus:false open.
- Framework verification basis: dotnet/winforms `main` (fetched verbatim); referencesource.microsoft.com is offline (301) and the GitHub mirror dir unreachable, so net48 parity is asserted, not byte-verified.
- Worktree gotcha: the 2026-08-28T08-42 worktree (HEAD = PR #676 merge) did NOT contain PR #684 (#677 fix) or its feature folder; `FinishClose` was still unconditional there. Plan #680 against a base including #684; Host.cs line numbers will shift.

Related: [[qfc438-search-focus-steal]], [[qfc438-residual-materialized]].
