# Pass-After — Regression + Positive Cases (Issue #254)

Timestamp: 2026-07-07T13-18

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:Theme_MailLabelTheming`

EXIT_CODE: 0

## Output Summary

Test Run Successful. Total tests: 3, Passed: 3.

- `Theme_MailLabelTheming_WhenReadProbeThrows_LabelsStillReThemeToUnread` [250 ms] — PASS. After the fix, the COMException from the read-state probe is caught inside `SetQfcTheme()`; the render no longer throws and both `_lblSender`/`_lblSubject` BackColor values are re-themed to the unread color (not the previous-theme sentinel).
- `Theme_MailLabelTheming_WhenProbeReturnsFalse_AppliesUnreadColors` [< 1 ms] — PASS (probe returns false -> unread colors on both labels).
- `Theme_MailLabelTheming_WhenProbeReturnsTrue_AppliesReadColors` [< 1 ms] — PASS (probe returns true -> read colors on both labels).

The three cases exercise all branches of the changed read-state block: try-success-read, try-success-unread, and catch-default. The same regression test that failed before the fix (see `fail-before.2026-07-07T13-16.md`) now passes, confirming the fix.
