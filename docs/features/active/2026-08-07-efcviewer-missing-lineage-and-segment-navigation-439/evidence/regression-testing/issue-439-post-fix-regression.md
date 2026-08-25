# Issue #439 Post-Fix Regression Evidence

- Timestamp: `2026-08-24T19:33:34-04:00`
- Scope: deterministic headless router, row, codec, renderer, and builder tests only. The run does not create Outlook COM objects, temporary files, external-service calls, WinForms or WebView2 windows/handles, UI message pumps, or real GUI.

## Command

```powershell
$env:Path='C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow;'+$env:Path; vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~Issue439|FullyQualifiedName~BreadcrumbHtmlRendererTests|FullyQualifiedName~BreadcrumbMessageCodecTests|FullyQualifiedName~BreadcrumbRowBuilderTests|FullyQualifiedName~BreadcrumbRowStateTests" /InIsolation
```

- Exit code: `0`
- Result: `78` passed, `0` failed.

## Restarted P4-T1 regression

Timestamp: 2026-08-24T20:20:00-04:00
Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~Issue439|FullyQualifiedName~EfcFormControllerTests|FullyQualifiedName~BreadcrumbHtmlRendererTests|FullyQualifiedName~BreadcrumbMessageCodecTests|FullyQualifiedName~BreadcrumbRowBuilderTests|FullyQualifiedName~BreadcrumbRowStateTests" /InIsolation`
EXIT_CODE: 0
Output Summary: 83 passed, 0 failed. The selection includes Issue #439 router, row, codec, renderer, generated-document, and Efc binding-boundary coverage.

All selected tests exercised headless seams only: no Outlook COM, temporary files, external service, WinForms or WebView2 window/handle creation, `Show`, `ShowDialog`, `Application.Run`, UI message pump, or real GUI.

## Restarted P4-T1 after formatting

Timestamp: 2026-08-24T20:21:10-04:00
Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~Issue439|FullyQualifiedName~EfcFormControllerTests|FullyQualifiedName~BreadcrumbHtmlRendererTests|FullyQualifiedName~BreadcrumbMessageCodecTests|FullyQualifiedName~BreadcrumbRowBuilderTests|FullyQualifiedName~BreadcrumbRowStateTests" /InIsolation`
EXIT_CODE: 0
Output Summary: 83 passed, 0 failed after CSharpier formatted the newly added Issue #439 test content.

The run remains headless: no Outlook COM, temporary files, external service, WinForms or WebView2 window/handle creation, `Show`, `ShowDialog`, `Application.Run`, UI message pump, or real GUI.

## P4-T1 restart after stale expectation correction

Timestamp: 2026-08-24T20:29:09-04:00
Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~Issue439|FullyQualifiedName~EfcFormControllerTests|FullyQualifiedName~BreadcrumbHtmlRendererTests|FullyQualifiedName~BreadcrumbMessageCodecTests|FullyQualifiedName~BreadcrumbRowBuilderTests|FullyQualifiedName~BreadcrumbRowStateTests" /InIsolation`
EXIT_CODE: 0
Output Summary: 83 passed, 0 failed. Before this task, two stale queue-test expectations failed the full coverage run because Issue #439 deliberately changed provider cancellation to fallback rendering and active-leaf expansion to use the binding-captured key. The corrected named tests independently passed 2/2 after a clean build.

All selected tests and the two corrected queue tests use only router, row, renderer, codec, and Moq boundaries. They create no Outlook COM, filesystem, network, external process, WinForms/WebView2 window or handle, `Show`, `ShowDialog`, `Application.Run`, or UI message pump.

## P4-T1 restarted regression with queue-test coverage

Timestamp: 2026-08-24T20:31:00-04:00
Command: `& 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~Issue439|FullyQualifiedName~EfcFormControllerTests|FullyQualifiedName~BreadcrumbBridgeRouterQueueTests|FullyQualifiedName~BreadcrumbHtmlRendererTests|FullyQualifiedName~BreadcrumbMessageCodecTests|FullyQualifiedName~BreadcrumbRowBuilderTests|FullyQualifiedName~BreadcrumbRowStateTests" /InIsolation`
EXIT_CODE: 0
Output Summary: 97 passed, 0 failed. The focused P4 gate now includes the two renamed queue tests and their pure router/Moq test class.

All selected tests remain headless: no Outlook COM, filesystem, network, external process, WinForms/WebView2 window or handle, `Show`, `ShowDialog`, `Application.Run`, UI message pump, or real GUI.
