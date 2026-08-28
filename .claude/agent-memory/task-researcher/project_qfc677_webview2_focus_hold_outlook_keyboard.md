---
name: qfc677-webview2-focus-hold-outlook-keyboard
description: 'Issue #677: Outlook-wide keyboard death while QuickFiler open is NOT a repo hook — WebView2 holds thread focus (WebView2Feedback #951) and FinishClose unconditionally re-focuses the anchor WebView2 on click-out; ToolStripDropDown menu mode is the secondary thread-wide filter'
metadata:
  type: project
---

Issue #677 research (2026-08-28, artifact at `docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/research/2026-08-28T09-15-...-research.md`).

**Why:** the obvious suspects (KeyboardHandler scoping, hooks, message filters, disabled windows) are all provably innocent; the real mechanism spans repo focus calls + an external runtime bug, and future QFC focus work must not re-litigate this.
**How to apply:** any fix/plan for #677 (or future WebView2 focus work in QuickFiler/EFC) starts from the focus-permission-predicate + deactivate-parking shape; verify the `[[qfc438-search-focus-steal]]` close-side steal is guarded too — same `FinishClose` code path.

Key verified findings:
- `KeyboardHandler` wiring is strictly form-internal (`QfcFormController.SetupDisposal.cs:149-175`, `QfcItemController.EventWiring.cs:40-91`), per-launch lifetime (`QfcHomeController.cs:93,181-186,350-359`). No first-party hook/filter/EnableWindow/Owner anywhere. Legacy tree (`QuickFiler\Legacy\*`, incl. SendKeys) is dead — referenced only from within Legacy; live path is `RibbonController.cs:104-139` -> `QfcHomeController.LaunchAsync`.
- Primary mechanism: 2 WebView2s per ItemViewer row (`ItemViewer.Designer.cs:46,49`) + breadcrumb pipeline parks Win32 focus on WebView2 (`ItemViewer.Breadcrumb.cs:252-262`, `QfcItemController.Navigation.cs:33-46`); `BreadcrumbDropDownHost.FinishClose` (Host.cs:410-420) ALWAYS invokes `_focusAnchor` = FocusBreadcrumbCore, including `OnDropDownClosed` from a user click-out (scheduled async, so it lands AFTER the click into Outlook). WebView2Feedback #951 (open since 2021, verified 2026-08-28): WebView2 in VSTO WinForms host holds keyboard focus; clicking back into host changes selection but keys keep routing to WebView2. Same thread = one input queue => Outlook keyboard dead, mouse fine, recovers on QFC close (WebView2 disposal).
- Secondary: `ToolStripDropDown AutoClose=true` (Host.cs:165-170) enters WinForms menu mode; in VSTO (`Application.MessageLoop==false`) the framework installs a thread WH_GETMESSAGE hook (HostedWindowsFormsMessageHook) redirecting ALL thread keyboard to the dropdown — invisible to repo greps for SetWindowsHookEx. Transient (click-out dismisses) unless the popup outlives deactivation. NOTE: reference-source background knowledge, not session-verified.
- Repo has ZERO `CoreWebView2Controller`/`MoveFocus`/`IsBrowserAcceleratorKeysEnabled` usage — no focus mitigation exists anywhere.
- Recommended fix shape: injectable `Func<bool>` focus-permission predicate on BreadcrumbDropDownHost (guards `_focusAnchor` + late `_focusPending`, evaluated at EXECUTION time not scheduling time), plus Form.Deactivate handler that parks focus off WebView2 and cancels the open selector. Do NOT delete `_focusAnchor` (breaks #438/#400 ACs).
- End-to-end confirmation is live-Outlook-only; predicate/parking logic is fully unit-testable via existing host delegate seams.
