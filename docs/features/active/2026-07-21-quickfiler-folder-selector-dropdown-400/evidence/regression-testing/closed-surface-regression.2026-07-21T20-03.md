# Closed-Surface Regression

Timestamp: 2026-07-21T20-03Z
Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Tests:ClosedSurfaceReadyBoundary_DefersPopupReplayAndReopenDoesNotDuplicateSubscriptions,OpenAsync_IsLazyUsesSuppliedEnvironmentAndReusesOneSurfaceAcrossOpens,ExplicitCommitAndUncommittedClose_HaveDistinctCallbacks,OpenAsync_WhenAlreadyOpen_FocusesPendingWithoutRecreatingOrShowing,ResetAndPooledReuse_DetachPopupAndDoNotDuplicateCallbacks`
EXIT_CODE: 0
Output Summary: The new closed-surface readiness boundary test and four existing popup close/reopen lifecycle tests passed. The closed surface stayed attached while popup attachment and cached replay remained deferred until readiness; reopen reused one ready surface without duplicate subscription or replay. No additional production defect or scope expansion was exposed.

## Results

- Resolved vstest: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
- Total tests: 5
- Passed: 5
- Failed: 0
- Skipped: 0
- Test time: 1.9456 seconds

Per-test outcomes:

1. PASS — `ClosedSurfaceReadyBoundary_DefersPopupReplayAndReopenDoesNotDuplicateSubscriptions`
2. PASS — `OpenAsync_IsLazyUsesSuppliedEnvironmentAndReusesOneSurfaceAcrossOpens`
3. PASS — `ExplicitCommitAndUncommittedClose_HaveDistinctCallbacks`
4. PASS — `OpenAsync_WhenAlreadyOpen_FocusesPendingWithoutRecreatingOrShowing`
5. PASS — `ResetAndPooledReuse_DetachPopupAndDoNotDuplicateCallbacks`

The test-only batch modified only `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs`, which is 436 lines. No production file was changed for P2-T6.
