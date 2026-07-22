# P5 structural headroom focused regression

Timestamp: 2026-07-22T07:46:13.4127481Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownCoverageThresholdTests' '/Logger:console;Verbosity=detailed'`

EXIT_CODE: 0

Output Summary: VSTest 18.8.0 discovered and passed all 32 selected cases in 2.4485 seconds with 32 passed, 0 failed, and 0 skipped. Discovery remained complete by class: 4 `BreadcrumbSelectorToggleUiBoundaryTests`, 13 `BreadcrumbPopupControlDispatchTests` cases including all three invalid-navigation data rows, 8 `BreadcrumbSelectorOpenRetryTests` cases including all four stale-placement data rows, and 7 `BreadcrumbDropDownCoverageThresholdTests`. The selector-toggle creator-thread, popup dispatch/cleanup, mouse/keyboard retry, placement generation, Dispose race, primary-preserving rollback, reset/readiness cleanup, and fresh-retry assertions all remained passing. No workspace-owned VSTest or testhost process remained after the run.
