# Targeted Post-Fix Regression Tests — UtilitiesCS.Test (Issue #269)

- Timestamp: 2026-07-08T10-10
- Task: [P1-T7]
- Command: `MSYS_NO_PATHCONV=1 vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /EnableCodeCoverage "/TestCaseFilter:FullyQualifiedName~Theme_MailLabelThemingTests"`
- EXIT_CODE: 0

## Output Summary

`Test Run Successful. Total tests: 4. Passed: 4.`

```
Passed Theme_MailLabelTheming_WhenReadProbeThrows_LabelsStillReThemeToUnread [217 ms]
Passed Theme_MailLabelTheming_WhenReadProbeThrowsNullReferenceException_LabelsStillReThemeToUnread [< 1 ms]
Passed Theme_MailLabelTheming_WhenProbeReturnsFalse_AppliesUnreadColors [< 1 ms]
Passed Theme_MailLabelTheming_WhenProbeReturnsTrue_AppliesReadColors [1 ms]
```

All four tests in `Theme_MailLabelThemingTests` pass post-fix, including:
- The new `Theme_MailLabelTheming_WhenReadProbeThrowsNullReferenceException_LabelsStillReThemeToUnread` (issue #269 fail-before test, now passing).
- The pre-existing `Theme_MailLabelTheming_WhenReadProbeThrows_LabelsStillReThemeToUnread` (`COMException` case, issue #254), confirming no regression to the existing `catch (COMException)` handling.

Satisfies AC1, AC4 (pass-after half), and AC5 (`COMException` non-regression).
