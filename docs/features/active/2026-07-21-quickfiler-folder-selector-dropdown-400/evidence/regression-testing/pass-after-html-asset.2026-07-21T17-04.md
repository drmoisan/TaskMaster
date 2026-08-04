# Pass-After Evidence: Folder Breadcrumb HTML Asset

Timestamp: 2026-07-21T17:04:00Z

## Build

Command:

```powershell
msbuild QuickFiler.Test/QuickFiler.Test.csproj /t:Build /p:Configuration=Debug /p:Platform=AnyCPU
```

EXIT_CODE: 0

The direct legacy project accepts `AnyCPU`. The plan's literal `Any CPU` platform value fails project configuration resolution before compilation, so this evidence uses the mechanically equivalent direct-project platform name. The build completed with three existing `System.Reactive` package-compatibility warnings and zero errors.

## Filtered Regression Tests

Command:

```powershell
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~FolderBreadcrumbAssetContractTests"
```

EXIT_CODE: 0

Passed: 12

Failed: 0

Skipped: 0

Output Summary: `FolderBreadcrumbAssetContractTests` was discovered from the compiled resource and all 12 tests passed. The tests map collapsed committed-row and direct `percentText` rendering to AC-1; hidden collapsed overflow and the single accessible drop-down button to AC-2; prevented selector-key browser behavior to AC-5 through AC-8; preserved Left/Right routing to AC-9; selector-view mode and row-aligned stable identity/selectability state to AC-12; and theme-independent listbox, option, active-descendant, and focus behavior to AC-13. The root-authorized mechanical protocol prerequisite was also verified separately by 14 passing coordinator/hub tests: `selectorView.options` is row-index aligned, and surface mode rewriting preserves those options and unknown extension fields.
