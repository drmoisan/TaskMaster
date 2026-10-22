# Issue #439 Typed Navigation Regression Evidence

- Timestamp: `2026-08-24T19:32:18.0136131-04:00`
- Scope: headless router, row, codec, renderer, and generated-document asset boundaries only. No WinForms, WebView2 control/handle, Outlook COM, message pump, or runtime UI was created.

## Build prerequisite

```powershell
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'
```

- Exit code: `0`
- Result: succeeded with `0` errors and the established five System.Reactive `packages.config` warnings.

## Focused regression command

```powershell
$env:Path='C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow;'+$env:Path; vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~Issue439|FullyQualifiedName~BreadcrumbMessageCodecTests|FullyQualifiedName~BreadcrumbHtmlRendererTests|FullyQualifiedName~BreadcrumbRowStateTests" /InIsolation
```

- Exit code: `0`
- Result: `60` passed, `0` failed.
- Verified: typed codec round trips and missing-index rejection; invalid row-state transitions preserve active state; literal Unicode arrows; active-ancestor child metadata; embedded `segmentActivate` and `renderedChildActivate` messages with stopped propagation; ancestor archive-relative selection; ancestor-key immediate-child query; archive-relative child/sibling selection; existing collapse and banner/pseudo-row coverage.
