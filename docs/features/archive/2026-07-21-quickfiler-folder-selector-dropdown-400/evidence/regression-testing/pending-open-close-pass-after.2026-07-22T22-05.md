# Pending-open close regression pass-after

Timestamp: `2026-07-22T22:05:44-04:00`

Resolved VSTest: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`

Command:

`& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbPendingOpenCloseTests|FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownLifecycleConcurrencyTests|FullyQualifiedName~BreadcrumbDropDownLifecycleTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests' '/Logger:console;Verbosity=normal'`

Result: PASS. VSTest returned `EXIT_CODE: 0`; all 46 selected tests passed with no failures or skips.

The passing set covers shared pending-open ownership, factory/readiness cancellation, deterministic false settlement, repeated-close idempotence, fresh reopen, stale resource disposal, mouse/keyboard/automatic close equivalence, exactly-once rollback and focus, reset/dispose late-completion suppression, creator-thread dispatch, failure observation, and existing host lifecycle safeguards.

Additional compatibility diagnostic: the combined `BreadcrumbDropDownHostTests`, `BreadcrumbDropDownLifecycleCoverageTests`, and `BreadcrumbPopupBoundaryCoverageTests` filter passed 48 of 48 tests before the formal gate.
