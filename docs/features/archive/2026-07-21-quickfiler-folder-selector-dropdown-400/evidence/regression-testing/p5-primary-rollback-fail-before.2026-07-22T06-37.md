# P5 Primary Rollback Failure-First Test

Timestamp: 2026-07-22T06:37:19.2976437Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbDropDownCoverageThresholdTests.OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery|FullyQualifiedName~BreadcrumbDropDownCoverageThresholdTests.OpenAsync_FocusCallback' '/Logger:console;Verbosity=normal'`

EXIT_CODE: 1

Output Summary: VSTest 18.8.0 discovered exactly 2 requested tests from the fully expanded test assembly path. `OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery` failed only at the intended current-production defects: cancellation occurred twice instead of once, `LastInitializationException` was replaced by the rollback failure instead of retaining the initiating initialization failure, and the rollback secondary was not observed by the error sink. `OpenAsync_FocusCallbackFailsAfterShow_ClosesThenPermitsRetry` passed, proving one native close, one anchor-focus return, authoritative closed state, false first completion, and successful retry eligibility. Totals were 1 passed, 1 failed, and 0 skipped; the nonzero exit is the required failure-first result rather than a build, discovery, crash, timeout, or unrelated-test failure. This artifact supersedes the relative-assembly P5-T48 artifact.
