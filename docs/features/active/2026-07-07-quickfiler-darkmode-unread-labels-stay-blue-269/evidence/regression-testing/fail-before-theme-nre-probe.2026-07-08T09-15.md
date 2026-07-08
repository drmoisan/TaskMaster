# [expect-fail] Fail-Before Evidence — Theme_MailLabelTheming_WhenReadProbeThrowsNullReferenceException_LabelsStillReThemeToUnread (Issue #269)

- Timestamp: 2026-07-08T09-55
- Task: [P1-T2] [expect-fail]

## Test Added

`UtilitiesCS.Test/HelperClasses/ThemeHelpers/Theme.MailLabelThemingTests.cs`, method `Theme_MailLabelTheming_WhenReadProbeThrowsNullReferenceException_LabelsStillReThemeToUnread`, inside the existing `[TestClass] Theme_MailLabelThemingTests` fixture. MSTest `[TestMethod]` + FluentAssertions, reusing the existing `BuildTheme` helper.

## Command

```
MSYS_NO_PATHCONV=1 vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~Theme_MailLabelTheming_WhenReadProbeThrowsNullReferenceException_LabelsStillReThemeToUnread"
```

(vstest.console.exe: `C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe`; test assembly rebuilt against pre-fix production code prior to this run.)

## EXIT_CODE

1 (test run failed — 1 of 1 test failed)

## Output Summary

`Total tests: 1. Failed: 1.`

```
Failed Theme_MailLabelTheming_WhenReadProbeThrowsNullReferenceException_LabelsStillReThemeToUnread [304 ms]
Error Message:
 Did not expect any exception, but found System.NullReferenceException: simulated null Mail
 at Theme_MailLabelThemingTests.<>c.<...>b__7_0() in Theme.MailLabelThemingTests.cs:line 131
 at UtilitiesCS.Theme.SetQfcTheme() in Theme.Rendering.cs:line 45
 at UtilitiesCS.Theme.SetQfcTheme(Boolean async) in Theme.cs:line 426
```

Confirms the pre-fix `NullReferenceException` propagates uncaught out of `SetQfcTheme()` at `Theme.Rendering.cs:45` (the `isRead = MailRead();` call, only guarded by `catch (COMException)`), aborting the render before the label branch runs. This satisfies AC4's fail-before half for the mail-label-guard component of the fix.
