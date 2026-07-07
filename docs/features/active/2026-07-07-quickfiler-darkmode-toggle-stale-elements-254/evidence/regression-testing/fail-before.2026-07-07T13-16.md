# Fail-Before — Regression Test (Issue #254)

Timestamp: 2026-07-07T13-16

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:Theme_MailLabelTheming_WhenReadProbeThrows_LabelsStillReThemeToUnread`

EXIT_CODE: 1

## Output Summary

Test Run Failed. Total tests: 1, Failed: 1.

Failing test: `UtilitiesCS.Test.HelperClasses.ThemeHelpers.Theme_MailLabelThemingTests.Theme_MailLabelTheming_WhenReadProbeThrows_LabelsStillReThemeToUnread`

Failure reason (verbatim from trx):
> Did not expect any exception, but found System.Runtime.InteropServices.COMException (0x80004005): The item has been moved or deleted.
>    at UtilitiesCS.Test.HelperClasses.ThemeHelpers.Theme_MailLabelThemingTests...

This is the issue #254 defect reproduced deterministically: when the injected `MailRead()` read-state probe throws (simulating a stale/moved Outlook `MailItem`), the current `Theme.SetQfcTheme()` mail branch (`Theme.Rendering.cs` lines 33-41) evaluates `MailRead()` unguarded at line 34, so the exception propagates out of the private renderer before `SetMailUnread()`/`SetMailRead()` run. The sender/subject labels therefore retain their previous-theme colors. The test's `act.Should().NotThrow()` assertion fails, confirming the fault precedes the label re-theming.

Test is deterministic and seam-based: handle-less WinForms `Label` doubles via the `Theme` big constructor, injected throwing `Func<bool>`, `async: false` synchronous render. No live Outlook, no COM object, no dispatcher, no temp files.
