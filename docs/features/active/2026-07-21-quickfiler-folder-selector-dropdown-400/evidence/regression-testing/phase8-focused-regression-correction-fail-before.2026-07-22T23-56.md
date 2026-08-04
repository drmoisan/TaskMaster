# Phase 8 focused-regression correction failure reconciliation

Timestamp: 2026-07-22T23:56:58.5552518-04:00

Command: `$failPath='docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/regression-testing/issue-400-focused-regression-fail.2026-07-22T23-41.md'; $diagnosisPath='docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/regression-testing/issue-400-focused-regression-diagnosis.2026-07-22T23-45.md'; $fail=Get-Content -Raw $failPath; $diagnosis=Get-Content -Raw $diagnosisPath; $checks=@(($fail -match '358 cases') -and ($fail -match '353 passed, 5 failed, and zero skipped') -and ($fail -match 'EXIT_CODE: 1'), ($diagnosis -match '358 discovered, 353 passed, 5 failed, 0 skipped'), (($diagnosis | Select-String -Pattern 'The three .* failures are stale test setup' -AllMatches).Matches.Count -eq 1), (($diagnosis | Select-String -Pattern 'contains a stale pre-P6 expectation' -AllMatches).Matches.Count -eq 1), (($diagnosis | Select-String -SimpleMatch 'detected a production contract regression' -AllMatches).Matches.Count -eq 1)); if ($checks -contains $false) { throw 'Phase 8 regression reconciliation failed.' }; 'RECONCILIATION_OK discovered=358 passed=353 intended_failures=5 skipped=0 failing_command_exit=1 mappings=3'`

EXIT_CODE: 0

Output Summary: The cited exact 35-class VSTest command completed naturally with exit code 1 after discovering 358 tests: 353 passed, exactly five named intended failures occurred, and zero were skipped. The deterministic inspection returned `RECONCILIATION_OK discovered=358 passed=353 intended_failures=5 skipped=0 failing_command_exit=1 mappings=3`.

## Reconciled failures

1. `ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily` maps only to missing production-order pipeline initialization in stale test setup.
2. `ConfigureBreadcrumbDropDown_LightThemeUsesSameControllerSetupSeam` maps only to missing production-order pipeline initialization in stale test setup.
3. `ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost` maps only to missing production-order pipeline initialization in stale test setup.
4. `InitializationFailure_CancelsSessionWithoutDuplicateClose` maps only to the stale zero-close expectation superseded by P6-T10 and P6-T16.
5. `OpenAsync_RollbackCallbackFailsOnce_OuterPipelineCompletesRecovery` maps only to the unauthorized placement-message change.

No additional or differently diagnosed failure was present in the recorded run.
