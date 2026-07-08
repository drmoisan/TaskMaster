# [expect-fail] Fail-Before Evidence — BuildProductionControlSet_WithNullMail_MailReadReturnsFalseWithoutThrowing (Issue #269)

- Timestamp: 2026-07-08T10-00
- Task: [P1-T3] [expect-fail]

## Test Added

`QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs`, method `BuildProductionControlSet_WithNullMail_MailReadReturnsFalseWithoutThrowing`, inside the existing `[TestClass] QfcThemeHelperTests` fixture. MSTest `[TestMethod]` + FluentAssertions + Moq (`IUiDispatcher` mock), reusing the existing `CreateController` and `CreateItemViewer` helpers.

## Command

```
MSYS_NO_PATHCONV=1 vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~BuildProductionControlSet_WithNullMail_MailReadReturnsFalseWithoutThrowing"
```

(test assembly rebuilt against pre-fix production code prior to this run.)

## EXIT_CODE

1 (test run failed — 1 of 1 test failed)

## Output Summary

`Total tests: 1. Failed: 1.`

```
Failed BuildProductionControlSet_WithNullMail_MailReadReturnsFalseWithoutThrowing [371 ms]
Error Message:
 Did not expect any exception, but found System.NullReferenceException: Object reference not set to an instance of an object.
 at QuickFiler.QfcThemeHelper.<>c__DisplayClass5_0.<BuildProductionControlSet>b__0() in QfcThemeHelper.cs:line 89
 at QfcThemeHelperTests.<>c__DisplayClass6_0.<...>b__1() in QfcThemeHelperTests.cs:line 148
```

Confirms the pre-fix `NullReferenceException` is thrown directly from the probe lambda `() => !controller.Mail.UnRead` at `QfcThemeHelper.cs:89` when `controller.Mail` is `null`. This satisfies AC4's fail-before half for the probe-construction-site component of the fix.
