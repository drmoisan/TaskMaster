# quickfiler-darkmode-unread-labels-stay-blue (Issue #269)

- Date captured: 2026-07-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-darkmode-unread-labels-stay-blue/ (Issue #269)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #269
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/269
- Last Updated: 2026-07-08
- Work Mode: minor-audit

## Summary

After switching the QuickFiler (Quick File) window to dark mode, the Sender and Subject fields of unread items retain the light theme's unread background (blue) instead of adopting the dark theme. This is the residual of issue #254, which was closed with a fix that addressed only one exception path.

## Environment

- OS/version: Windows (Outlook desktop add-in, VSTO)
- Python version: n/a (C# / .NET Framework)
- Command/flags used: QuickFiler window; toggle light -> dark theme
- Data source or fixture: live mailbox with at least one unread message in the QuickFiler list

## Steps to Reproduce

1. Open the Quick File window with a mix of read and unread messages.
2. Switch the add-in to dark mode.
3. Observe the Sender and Subject fields of the unread items.

## Expected Behavior

In Light mode, the `_lblSender` and `_lblSubject` fields on `ItemViewer` render as dark text on a light background (read items), with blue text as the unread accent. Dark mode already renders correctly.

## Actual Behavior

In Light mode, the Sender/Subject fields render with foreground and background transposed: read items show light text on a black background and unread items show light text on a blue background. Dark mode is unaffected.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: User-reported: "It is only happening in Light mode. Dark mode appears correctly. In Light mode, some items have a background of blue with white text. Others have a background of black with white text." (Sender/Subject fields.)

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

Root cause CONFIRMED by git history and a red-before-green test (see `research/root-cause-corrected-light-mode-fore-back-swap.md`). The earlier hypotheses in this folder (a `NullReferenceException` probe-abort in `Theme.Rendering.cs`, and a dark-mode framing) were incorrect and have been reverted.

- Commit `44bfdf204` ("coverage seams for issue 236", 2026-07-04) converted the theme definitions in `QuickFiler/Helper Classes/QfcThemeHelper.cs` from named-argument `new Theme(...)` to positional `CreateTheme(...)` calls.
- `CreateTheme` orders the mail parameters `(mailReadForeColor, mailReadBackColor, mailUnreadForeColor, mailUnreadBackColor)`. The pre-refactor Light blocks listed background before foreground, so the positional conversion transposed foreground and background — but only for `LightNormal` and `LightActive`. The Dark blocks already listed foreground first, so they were unaffected.
- Result in Light mode: read = light text on black bg; unread = light text on blue bg. Dark mode is correct.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: `QfcThemeHelper.SetupThemes` Light-theme mail-label colors are dark text on a light background (blue unread accent); Dark themes remain correct.
- [x] Integration scenario to retest: switch to Light mode; confirm Sender/Subject fields are dark-on-light.
- [x] Manual verification notes: after the fix, Light-mode Sender/Subject fields render dark text on a light background.

Fix applied: swap the four transposed mail-label values in the `LightNormal` and `LightActive` `CreateTheme(...)` calls back to the pre-refactor (correct) order, in `QuickFiler/Helper Classes/QfcThemeHelper.cs`.

## Acceptance Criteria

- [x] AC1: In Light mode, `_lblSender`/`_lblSubject` on `ItemViewer` render dark text on a light background for read items and blue text on a light background for unread items. No Light-mode item shows light text on a black or blue background.
- [x] AC2: Dark mode remains correct: `_lblSender`/`_lblSubject` render light text on a black background (goldenrod unread accent). No regression to the Dark themes.
- [x] AC3: Root cause is corrected with a minimal, targeted change (no opportunistic refactor). The change is confined to the `LightNormal`/`LightActive` `CreateTheme(...)` mail-label arguments in `QuickFiler/Helper Classes/QfcThemeHelper.cs`, restoring the pre-refactor values.
- [x] AC4: A deterministic regression test reproduces the defect (fails before the fix, passes after) using seams only — `QfcThemeHelper.SetupThemes` with a handle-less control set; no live Outlook, no COM, no temporary files. The test asserts the corrected Light colors and that Dark remains correct.
- [x] AC5: The full C# toolchain (CSharpier -> analyzers -> nullable -> MSTest with coverage) passes; full impacted suite 4663/4663 with no regression.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
