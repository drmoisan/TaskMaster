# quickfiler-darkmode-toggle-stale-elements (Issue #254)

- Date captured: 2026-07-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-darkmode-toggle-stale-elements/ (Issue #254)
- Type: Bug

- Issue: #254
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/254
- Last Updated: 2026-07-07
- Work Mode: minor-audit

## Problem / Why

In the QuickFiler High Confidence view, switching between dark mode and light mode
does not re-theme every element. After a toggle, certain email item rows retain the
colors of the previous theme while the rest of the view updates. The screenshot
`artifacts/Screenshot 2026-07-07 123101.png` shows several item rows whose
sender/subject region keeps a blue background (prior-theme mail read/unread coloring)
while sibling rows render in the current theme.

The theme application path is:

- `QfcCollectionController.DarkMode_CheckedChanged` → `SetDarkMode`/`SetLightMode`
  iterates item groups and calls `IQfcItemController.SetThemeDark`/`SetThemeLight`.
- `QfcItemController.SetThemeDark`/`SetThemeLight`
  (`QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs`) select a theme key
  from `_themes` based on `_activeTheme` and call `Theme.SetQfcTheme`.
- `Theme.SetQfcTheme` (`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs`)
  applies colors to the fixed control set, including the mail read/unread
  sender/subject labels via `SetMailRead`/`SetMailUnread`.

The elements that fail to switch appear to be the read/unread mail labels, suggesting
the toggle path either does not reach `SetMailRead`/`SetMailUnread` for those items or
selects the wrong theme key. Root cause is unconfirmed and requires investigation.

## Proposed Behavior

Toggling dark/light mode re-themes all elements of every QuickFiler item, including the
mail sender/subject read/unread labels, so no element retains prior-theme colors.

## Acceptance Criteria

- [x] **AC1** — After a dark→light or light→dark toggle, every QuickFiler item's
      sender/subject (and any other read/unread-driven) elements render in the
      newly-selected theme, with no element retaining prior-theme colors.
- [x] **AC2** — The identified root cause of the stale-element behavior is corrected
      with the minimal, targeted change (no opportunistic refactor).
- [x] **AC3** — A deterministic regression test reproduces the defect (fails before the
      fix, passes after) using seams only — no live Outlook/COM/WinForms objects.
- [x] **AC4** — No regression to the issue #251 cleanup-unsubscribe behavior, and the
      full C# toolchain (CSharpier, analyzers, nullable, MSTest) passes with no
      coverage regression on changed lines.

## Constraints & Risks

- WinForms/COM-adjacent code; tests must use seams, not live Outlook objects.
- Must not reduce coverage on changed lines; repo C# gates apply (CSharpier, analyzers,
  nullable, MSTest).

## Test Conditions to Consider

- [ ] Unit coverage: theme-key selection and mail read/unread re-theming on toggle.
- [ ] Regression: toggle after items are in read vs. unread state.

## Next Step

- [x] Promote to GitHub issue (bug template) — Issue #254
- [x] Create active feature folder from the template
- [ ] Root-cause research
- [ ] Atomic plan → execution → feature-review → PR
