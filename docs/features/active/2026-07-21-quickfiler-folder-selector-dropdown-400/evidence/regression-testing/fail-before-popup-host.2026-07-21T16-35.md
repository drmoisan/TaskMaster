# Fail-before popup host

Timestamp: 2026-07-21T16-35Z

Build Command: `msbuild QuickFiler.Test/QuickFiler.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU`

Build EXIT_CODE: 0

Filtered Test Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbPopupPlacementTests|FullyQualifiedName~BreadcrumbDropDownHostTests|FullyQualifiedName~BreadcrumbDropDownLifecycleTests"`

Filtered Test EXIT_CODE: 1

- Total: 18
- Passed: 0
- Failed: 18
- Skipped: 0

Intended fail-before signatures:

- Eight placement cases failed because the pure `BreadcrumbPopupPlacement` calculator did not exist.
- Five native host cases failed because `BreadcrumbDropDownHost` did not exist, leaving ToolStrip ownership, non-topmost scope, smart placement, explicit/uncommitted close, focus, and theme contracts absent.
- Five lifecycle cases failed because the popup host did not exist, leaving supplied-environment reuse, lazy initialization, reset/disposal, partial-failure cleanup, and post-disposal callback suppression absent.

Compatibility Correction: `Platform=AnyCPU` is the root-authorized mechanical equivalent for the invalid direct-project `Platform='Any CPU'` token. No behavioral scope changed.

Output Summary: The test project compiled, 18 tests were discovered, and all 18 failed only on the intended missing placement, host, and lifecycle types. There was no compile, discovery, tool-resolution, live UI/display, or other environmental failure.
