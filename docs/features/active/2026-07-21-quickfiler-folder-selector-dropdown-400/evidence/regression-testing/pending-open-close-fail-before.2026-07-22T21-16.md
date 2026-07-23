# Pending-open close failure-first regression

Timestamp: `2026-07-22T21:16:00-04:00`

Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbPendingOpenCloseTests" /Logger:"console;Verbosity=normal"`

Resolved VSTest: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`

Result: expected failure, exit code `1`. Exactly five tests were discovered and all five failed against the pre-fix implementation:

- Factory-pending close allowed the stale popup to show (`ShowCount` 1 instead of 0).
- Readiness-pending close allowed the stale popup to show (`ShowCount` 1 instead of 0).
- A canceled factory attempt completed `true` instead of `false`, preventing correct fresh-open isolation.
- Toggle/Escape during pending open made zero host close calls instead of one.
- Automatic selector close during pending open made zero host close calls instead of one.

This confirms both root symptoms: `BreadcrumbDropDownHost.Close` rejects pending work when `IsOpen` is false, and `BreadcrumbDropDownOpenCoordinator` suppresses pending close requests behind the same `IsOpen` prerequisite.
