# Pass-after coordinator surfaces

Timestamp: 2026-07-21T16-30Z

Build Command: `msbuild QuickFiler.Test/QuickFiler.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU`

Build EXIT_CODE: 0

Filtered Test Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests|FullyQualifiedName~BreadcrumbSelectorCoordinatorTests|FullyQualifiedName~BreadcrumbMessengerHubTests"`

Filtered Test EXIT_CODE: 0

- Total: 13
- Passed: 13
- Failed: 0
- Skipped: 0

Issue #398 Test Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"Name=SelectRow_WhileSuggestionsUpgradeInFlight_DoesNotThrowAndAppliesSelection"`

Issue #398 Test EXIT_CODE: 0

- Total: 1
- Passed: 1
- Failed: 0
- Skipped: 0

Compatibility Correction: `Platform=AnyCPU` is the root-authorized mechanical equivalent for the invalid direct-project `Platform='Any CPU'` token. No behavioral scope changed.

Output Summary: Immediate fallback probability, stable selection through hierarchy upgrade, closed and open keyboard behavior, Enter/Escape and activation behavior, Left/Right compatibility, per-surface render and theme broadcast, view-mode specialization, one inbound route, and idempotent attachment all passed. The named issue #398 in-flight selection regression was discovered and passed separately. Call-count assertions verified one selection or open/close transition per inbound action and one outbound render per attached surface.
