# Issue #269 — Corrected root cause: Light-theme mail-label foreground/background swap

This document supersedes the earlier #269 root-cause hypotheses (the `NullReferenceException`
probe-abort theory and the dark-mode framing). Those were incorrect. The corrected diagnosis below
is confirmed by git history and by a red-before-green regression test executed against a real build.

## Corrected symptom (user-reported)

- Affected controls: `_lblSender` and `_lblSubject` on `ItemViewer`.
- Mode: **Light mode only.** Dark mode renders correctly.
- Appearance: some items show a blue background with light text, others a black background with
  light text. The colors are applied by the theme, not by the Designer.

## Root cause (CONFIRMED)

Commit `44bfdf204` ("wip: coverage seams for issue 236", 2026-07-04) converted the QuickFiler theme
definitions in `QuickFiler/Helper Classes/QfcThemeHelper.cs` from named-argument `new Theme(...)`
calls to positional `CreateTheme(...)` calls. The `CreateTheme` signature orders the mail-label
parameters as `(mailReadForeColor, mailReadBackColor, mailUnreadForeColor, mailUnreadBackColor)`.

The pre-refactor **Light** blocks listed these values in the opposite order (background before
foreground). When they were carried over positionally, foreground and background were transposed —
but only for `LightNormal` and `LightActive`. The **Dark** blocks had already listed foreground
before background, so they matched the new signature and were unaffected. This is exactly why dark
mode is correct and light mode is wrong.

Pre-refactor (correct) vs current HEAD (bug), `LightNormal`:

| Parameter | Pre-refactor (44bfdf204~1) | Current HEAD (bug) |
|---|---|---|
| mailReadForeColor | `SystemColors.ControlText` (dark text) | `SystemColors.Control` (light) |
| mailReadBackColor | `SystemColors.Control` (light bg) | `SystemColors.ControlText` (black) |
| mailUnreadForeColor | `Color.MediumBlue` (blue text) | `SystemColors.Control` (light) |
| mailUnreadBackColor | `SystemColors.Control` (light bg) | `Color.MediumBlue` (blue) |

`LightActive` has the identical transposition (`Color.LightCyan` background instead of blue/black).
`SetMailRead()`/`SetMailUnread()` (`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs:356-411`) assign
these values to `_lblSender`/`_lblSubject`, producing the reported appearance.

## Fix (applied)

Swap the four transposed values in the `LightNormal` and `LightActive` `CreateTheme(...)` calls back
to the pre-refactor (correct) order, restoring dark text on a light background with blue text as the
unread accent. A documenting comment records the positional order to prevent recurrence. One
production file changed: `QuickFiler/Helper Classes/QfcThemeHelper.cs`.

## Verification (executed against a real build)

- Regression test: `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs`,
  `SetupThemes_LightThemes_MailLabelColorsAreDarkTextOnLightBackground` — asserts the corrected
  Light colors and confirms Dark remains correct.
- Red (buggy HEAD code, full solution build): test FAILED, EXIT 1.
- Green (fixed code, full solution build): test PASSED, EXIT 0.
- Toolchain, in order, all EXIT 0:
  - CSharpier `format` — clean (`Formatted 2 files`, idempotent).
  - MSBuild analyzer build (`EnableNETAnalyzers`, `EnforceCodeStyleInBuild`) — Build succeeded, 0 errors.
  - MSBuild nullable build (`Nullable=enable`, `TreatWarningsAsErrors=true`) — Build succeeded, 0 errors.
  - vstest `UtilitiesCS.Test.dll` + `QuickFiler.Test.dll` `/EnableCodeCoverage` — Test Run Successful, 4663/4663 passed.

## Note on the prior (reverted) fix

The earlier #269 change (a `catch (NullReferenceException)` in `Theme.Rendering.cs` plus a
probe null-guard) addressed a different, hypothesized fault and did not affect these colors. It was
reverted so the #269 diff reflects only the real fix. That defensive guard remains available as
optional, separate hardening if desired; it is not required to resolve this issue.
