# Popup UI-boundary core focused regression gate after recovery

Timestamp: 2026-07-22T03:35:26.3285861Z

Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe' 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests' '/Logger:console;Verbosity=normal'`

EXIT_CODE: 0

Output Summary: The exact recovered P5 core filter passed 31 of 31 tests with zero failures and zero skips in 1.5900 seconds. Ambient-null and worker completions dispatch every factory, initialization, core-read, navigation, handler-detachment, and cleanup operation. The coordinator-post guard remains green. Initialization, navigation, readiness, null-navigation, and scheduling failures are each observed once; primary errors are preserved while all owned resources are attempted exactly once. Invalid factory arguments fail before dispatcher capture. This artifact supersedes every earlier P5 core focused-test artifact.
