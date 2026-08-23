# Popup Lifecycle Fail-Before

Timestamp: 2026-07-21T19-13Z
Command: $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Tests:ConcurrentOpenAsync_PendingInitializationIsSharedAndOpensOnePopup,Reset_DuringPendingInitializationDisposesLateSuccessAndAllowsFreshOpen,Dispose_DuringPendingInitializationDisposesLateSuccessWithoutMutation,Dispose_DuringPendingInitializationIgnoresLateFailureWithoutMutation,ResetLifecycle_LateFailureCannotOverwriteLaterSuccessfulLifecycle,CurrentLifecycle_FactoryFailureRemainsObservableAndRestoresOnce
EXIT_CODE: 1
Output Summary: Six exact lifecycle regressions were discovered. One current-failure control passed and five tests failed for the intended duplicate-creation or stale-lifecycle mutation defects. No compile, discovery, tool-resolution, UI, or environmental failure occurred.

## Results

- Total tests: 6
- Passed: 1
- Failed: 5
- Skipped: 0
- Test time: 1.1848 seconds

Intended pre-fix diagnostics:

1. `ConcurrentOpenAsync_PendingInitializationIsSharedAndOpensOnePopup`: expected one factory invocation, found two.
2. `Reset_DuringPendingInitializationDisposesLateSuccessAndAllowsFreshOpen`: reset-invalidated late success opened the popup (`true` instead of `false`).
3. `Dispose_DuringPendingInitializationDisposesLateSuccessWithoutMutation`: disposed late success opened the popup (`true` instead of `false`).
4. `Dispose_DuringPendingInitializationIgnoresLateFailureWithoutMutation`: disposed late failure invoked focus return once instead of zero callbacks.
5. `ResetLifecycle_LateFailureCannotOverwriteLaterSuccessfulLifecycle`: stale reset failure overwrote `LastInitializationException` after later success.

Control result:

- `CurrentLifecycle_FactoryFailureRemainsObservableAndRestoresOnce`: passed, confirming that a current-lifecycle failure is already observable and distinguishing it from the stale-failure defect.
