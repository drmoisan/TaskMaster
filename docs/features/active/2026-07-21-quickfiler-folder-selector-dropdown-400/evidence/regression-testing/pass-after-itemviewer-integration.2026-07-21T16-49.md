# Phase 5 ItemViewer integration pass-after evidence

Timestamp: 2026-07-21T16:49:12.8277987Z

## Build command

```powershell
msbuild QuickFiler.Test/QuickFiler.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU
```

EXIT_CODE: 0

Compatibility correction: the plan records `/p:Platform='Any CPU'`, but the direct legacy project configuration is named `AnyCPU`. The space-free project-platform spelling is the mechanically equivalent direct-project command and avoids a pre-compilation configuration error.

Build summary: 3 pre-existing System.Reactive `packages.config` compatibility warnings, 0 errors.

## Filtered test command

The test runner was resolved in the same PowerShell process as the build:

```powershell
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests|FullyQualifiedName~BreadcrumbDropDownIntegrationTests|FullyQualifiedName~QfcItemControllerBreadcrumbDropDownTests|FullyQualifiedName~QfcItemController_NavigationTests|FullyQualifiedName~QfcItemController_EventHandlersTests|FullyQualifiedName~QfcItemController_SeamDispatcherTests|FullyQualifiedName~QfcItemController_FolderSuggestionsTests"
```

EXIT_CODE: 0

Passed: 61
Failed: 0
Skipped: 0

## Discovery and behavior summary

- `ItemViewerBreadcrumbDropDownContractTests`: 4 passed.
- `BreadcrumbDropDownIntegrationTests`: 7 passed.
- `QfcItemControllerBreadcrumbDropDownTests`: 3 passed.
- `QfcItemController_NavigationTests`: 13 passed.
- `QfcItemController_EventHandlersTests`: 16 passed.
- `QfcItemController_SeamDispatcherTests`: 14 passed.
- `QfcItemController_FolderSuggestionsTests`: 4 passed.
- Moq call-count assertions verified one host open for each open intent, one uncommitted close for the close intent, one live inbound subscription per surface, and no duplicate selection routing across reset and pooled reuse.
- The passing cases cover anchor screen bounds, active working-area forwarding, existing environment reuse, lazy popup creation, dark/light propagation, initialization-failure rollback, host ownership on disposal, and compatibility with the existing folder controller paths.
