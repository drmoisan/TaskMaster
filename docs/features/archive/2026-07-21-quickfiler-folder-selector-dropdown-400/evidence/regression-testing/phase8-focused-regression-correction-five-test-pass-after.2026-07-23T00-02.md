# Phase 8 focused-regression correction five-test pass-after

Timestamp: 2026-07-23T00:02:37.4514523-04:00

Command: `$vswhere=Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest=& $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if(-not $vstest){throw 'VSTest was not resolved.'}; & $vstest 'QuickFiler.Test\bin\Debug\QuickFiler.Test.dll' /InIsolation /Tests:ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily,ConfigureBreadcrumbDropDown_LightThemeUsesSameControllerSetupSeam,ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost,InitializationFailure_CancelsSessionWithoutDuplicateClose,OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery /Logger:'console;Verbosity=normal'; exit $LASTEXITCODE`

EXIT_CODE: 0

Output Summary: VSTest 18.8.0 resolved through `vswhere`. Exactly 5 tests were discovered and all 5 passed, with 0 failed and 0 skipped.

## Passing tests

- `ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily`
- `ConfigureBreadcrumbDropDown_LightThemeUsesSameControllerSetupSeam`
- `ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost`
- `InitializationFailure_CancelsSessionWithoutDuplicateClose`
- `OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery`

The integration witness asserts exactly one total `Close(It.IsAny<BreadcrumbDropDownCloseReason>())` invocation. The placement witness passed against the restored exact message `The active working area has no space for the folder selector popup.`
