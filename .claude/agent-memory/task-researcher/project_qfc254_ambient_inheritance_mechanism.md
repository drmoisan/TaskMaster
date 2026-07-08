---
name: qfc254-ambient-inheritance-mechanism
description: Issue #254 exact mechanism — blue unread labels on dark rows explained by a single aborted SetQfcTheme() call plus WinForms ambient BackColor inheritance, not a duplicate Theme/controller
metadata:
  type: project
---

New runtime evidence (2026-07-07): after a dark toggle, the FIRST TWO (unread) QuickFiler rows show
Sender/Subject labels with the LIGHT theme's unread blue (`Color.MediumBlue`) while the rest of those
same rows (panel, buttons, nav) is correctly dark. This looked contradictory: a full abort of
`Theme.SetQfcTheme()` at the `MailRead()` branch (`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs:42-50`)
would also leave buttons (painted after the mail branch, `:61-72`) un-re-themed — but buttons were
dark.

**Resolved mechanism (LIKELY, structurally CONFIRMED except the live exception trigger):** a single
per-item Dark `SetQfcTheme()` call sets the row's own `TableLayoutPanel` (`_l0vh_Tlp`) dark at
`Theme.Rendering.cs:15-18` (before the mail branch), then throws an uncaught non-`COMException` type
(most likely `NullReferenceException` from a null `Mail`, per the already-documented catch-clause gap)
inside the `MailRead()` probe at `:42-50`, aborting BEFORE the label branch (`:52-58`) and BEFORE the
button loop (`:61-72`) ever run. Labels keep their last successfully-applied color (Light-unread-blue,
from `QfcItemController.InitializeAsync()`'s own independent `_globals.Ol.DarkMode` check at
`QfcItemController.Initialization.cs:215-222`). Buttons (except `_btnDelItem`, which has an explicit
Designer `BackColor = SystemColors.Control` pin at `ItemViewer.Designer.cs:3958`) have NO explicit
Designer `BackColor` and are children of `_l1h1L2v1h3Panel` -> `_l0vh_Tlp`
(`ItemViewer.Designer.cs:132`), so they render dark via ordinary WinForms **ambient BackColor
inheritance** from the already-dark panel, not via `Theme`'s own (never-executed) button loop. This
resolves the "buttons dark but labels stuck" contradiction with NO second Theme instance, no duplicate
controller, and no `ItemViewerQueue` recycling needed.

**Falsifiable disambiguator:** on the affected rows, `_btnDelItem` should still show light gray
(`SystemColors.Control`) and other buttons should show pure `Color.Black` (ambient, == `TlpBackColor`)
rather than `Color.DimGray` (== `DarkNormal.ButtonBackColor`, the color the button loop would have
explicitly applied). If a live repro shows `_btnDelItem` also dark, this mechanism is refuted.

**Hypotheses refuted with citations:** H1 (`ItemViewerQueue` recycling — the queue only ever mints
`new ItemViewer()`, confirmed via `ItemViewerQueue.cs`/`ViewerQueueCore.cs`, no reuse path exists); H3
(wrong theme key for Active items — a wrong key would miscolor the WHOLE row via one `SetQfcTheme()`
call, not labels alone); H2/H4 literal forms (no production caller of `Theme.SetMailUnread(bool)`
exists anywhere in the repo — grepped exhaustively; the one direct-label-only caller,
`ApplyReadEmailFormat` at `QfcItemController.FocusAndTheme.cs:318-324`, only applies READ colors and
keys off the synchronously-updated `_activeTheme` field, so it cannot be the blue-unread trigger).

**How to apply:** for WinForms theming bugs in this codebase, always check whether an unthemed child
control has an explicit Designer-time color pin or is ambient — ambient children silently "look
correct" even when the code that's supposed to color them never ran, which can mask a partial-render
abort as a fully-successful one. Full detail:
`docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/research/mechanism-unread-labels-blue-254.md`.
Related: [[project_qfc254_darkmode_stale_labels]], [[project_qfc254_residual_after_comexception_fix]].
