# Fail-before ItemViewer integration

Timestamp: 2026-07-21T16-43Z

Build Command: `msbuild QuickFiler.Test/QuickFiler.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU`

Build EXIT_CODE: 0

Filtered Test Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests|FullyQualifiedName~BreadcrumbDropDownIntegrationTests|FullyQualifiedName~QfcItemControllerBreadcrumbDropDownTests"`

Filtered Test EXIT_CODE: 1

- Total: 14
- Passed: 2
- Failed: 12
- Skipped: 0

Intended fail-before signatures:

- Controller dark/light setup cases failed because ViewerSetup had no method that passed its existing `CoreWebView2Environment` into popup configuration.
- ItemViewer production and injected configuration contracts failed because no drop-down host/environment or deterministic screen-geometry seam existed.
- Seven integration/lifecycle cases failed because the ItemViewer could not configure the host, so open/close, active-monitor placement inputs, two-surface attachment, theme, reset/reuse, initialization failure, and disposal ownership were absent.
- The existing anchor type and existing public folder event/drop-down intent signatures passed, confirming those compatibility boundaries were present before the fix.

Compatibility Correction: `Platform=AnyCPU` is the root-authorized mechanical equivalent for the invalid direct-project `Platform='Any CPU'` token. No behavioral scope changed.

Output Summary: The test project compiled, all 14 tests were discovered, two compatibility baselines passed, and 12 failed only on the intended missing open, placement, environment, focus/lifecycle, reset, and surface-attachment seams. There was no compile, discovery, tool-resolution, live UI/display, or other environmental failure.
