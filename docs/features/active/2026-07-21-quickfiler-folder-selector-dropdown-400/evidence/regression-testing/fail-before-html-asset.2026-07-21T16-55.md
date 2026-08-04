# Failure-Before Evidence: Folder Breadcrumb HTML Asset

Timestamp: 2026-07-21T16:55:00Z

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

EXIT_CODE: 1

Passed: 3

Failed: 8

Skipped: 0

Output Summary: The test assembly and `FolderBreadcrumbAssetContractTests` were discovered successfully. The intended assertions failed for collapsed one-selected-row rendering, collapsed overflow suppression, the accessible drop-down button, selector-view accessibility state, expanded active-row semantics and visibility, selector-key handling, and row/toggle activation messages. The existing self-contained theme support, visible host-supplied `percentText`, and unchanged Left/Right breadcrumb messages passed. There were no compilation, discovery, tool-resolution, UI/display, or environmental failures.
