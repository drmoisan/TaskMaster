# Host Readiness and Lifecycle Pass

Timestamp: 2026-07-21T19-55Z
Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Tests:OpenAsync_ReadinessPendingDefersAttachmentReplayShowAndFocusUntilSuccess,OpenAsync_ReadinessFailureRollsBackDisposesPartialSurfaceAndReturnsFocusOnce,ConcurrentOpenAsync_PendingInitializationIsSharedAndOpensOnePopup,Reset_DuringPendingInitializationDisposesLateSuccessAndAllowsFreshOpen,Dispose_DuringPendingInitializationDisposesLateSuccessWithoutMutation,Dispose_DuringPendingInitializationIgnoresLateFailureWithoutMutation,ResetLifecycle_LateFailureCannotOverwriteLaterSuccessfulLifecycle,CurrentLifecycle_FactoryFailureRemainsObservableAndRestoresOnce`
EXIT_CODE: 0
Output Summary: All eight exact Phase 1 readiness and lifecycle regressions passed from `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`. No test failed or was skipped.

## Results

- Resolved vstest: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
- Total tests: 8
- Passed: 8
- Failed: 0
- Skipped: 0
- Test time: 1.1654 seconds

Per-test outcomes:

1. PASS — `OpenAsync_ReadinessPendingDefersAttachmentReplayShowAndFocusUntilSuccess`
2. PASS — `OpenAsync_ReadinessFailureRollsBackDisposesPartialSurfaceAndReturnsFocusOnce`
3. PASS — `ConcurrentOpenAsync_PendingInitializationIsSharedAndOpensOnePopup`
4. PASS — `Reset_DuringPendingInitializationDisposesLateSuccessAndAllowsFreshOpen`
5. PASS — `Dispose_DuringPendingInitializationDisposesLateSuccessWithoutMutation`
6. PASS — `Dispose_DuringPendingInitializationIgnoresLateFailureWithoutMutation`
7. PASS — `ResetLifecycle_LateFailureCannotOverwriteLaterSuccessfulLifecycle`
8. PASS — `CurrentLifecycle_FactoryFailureRemainsObservableAndRestoresOnce`

The pass verifies readiness-gated messenger exposure, cached replay, popup display, and focus; one shared pending initialization; reset/dispose invalidation; stale-success cleanup; stale-failure suppression; fresh lifecycle success; and current-failure observability.
