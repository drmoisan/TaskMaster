---
name: qfc254-darkmode-stale-labels
description: Issue #254 root cause — QuickFiler dark/light toggle leaves sender/subject labels stale because label recoloring lives only in the MailRead()-guarded branch of Theme.SetQfcTheme()
metadata:
  type: project
---

Issue #254 (distinct from #251/#252 unsubscribe fix, which is intact): in QuickFiler High-Confidence
view, toggling dark/light leaves several rows' `LblSender`/`LblSubject` at the prior theme's colors
while the rest of the row re-themes.

Root cause (confirmed structural): `_lblSender`/`_lblSubject` colors are assigned in ONLY two places
— `Theme.SetMailRead()` and `Theme.SetMailUnread()` (`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs`).
The private `Theme.SetQfcTheme()` (`Theme.Rendering.cs`) reaches them only via a trailing branch
`if (!MailRead()) SetMailUnread(); else SetMailRead();`, evaluated AFTER nav/panel/tips coloring.
`MailRead` is the injected `Func<bool>` `() => !controller.Mail.UnRead` (built in
`QfcThemeHelper.BuildProductionControlSet`). A COM fault reading `.UnRead` on a stale/moved MailItem
throws, aborting `SetQfcTheme()` after panels are re-themed but before the labels are. Because the
toggle dispatches each item's render with `async: true` (`_uiDispatcher.InvokeAsync`, fire-and-forget,
Task discarded by `SetThemeDark`/`SetThemeLight`), the throw is isolated per item — so SOME rows stay
stale while others succeed. Exact COM trigger is LIKELY, not runtime-verified in No-COM env.

**Why this matters**: recommended minimal fix is a defensive `try/catch (COMException)` around the
`MailRead()` probe in `Theme.Rendering.cs` so labels always re-theme; deterministic regression test
uses the `Theme` big-constructor handle-less-doubles seam + a throwing `Func<bool> mailRead` (no live
Outlook), pattern proven in `Theme.DispatcherTests.cs` and `QfcItemController.FocusAndThemeTests.cs`.

**How to apply**: for future QuickFiler theming bugs, remember label coloring is NOT in the main
theme loop — it is only in the MailRead-guarded branch, and the toggle path is fire-and-forget async
so per-item faults do not surface. Full research:
`docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/research/root-cause-darkmode-toggle-254.md`.
Related: [[project_qfc_high_confidence_dual_pipeline]].
