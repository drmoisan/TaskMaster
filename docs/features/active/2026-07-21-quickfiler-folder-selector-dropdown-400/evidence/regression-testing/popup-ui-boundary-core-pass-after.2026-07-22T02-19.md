# Popup UI-boundary core focused regression gate, coverage-correction restart

Timestamp: 2026-07-22T02:19:04.2876817Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests' '/Logger:console;Verbosity=normal'`

EXIT_CODE: 0

Output Summary: The corrected focused core batch passed 20 of 20 tests in 1.5358 seconds. Ambient-null and worker-completion cases scheduled every factory, navigation, and cleanup operation through the owning dispatcher; completed coordinator-post behavior remained guarded; injected scheduling, action, initialization, navigation, and readiness failures were observed with the required once-only reporting semantics. This artifact supersedes the pre-coverage-correction 02-13 focused test artifact.
