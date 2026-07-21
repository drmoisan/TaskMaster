# RED-before-fix Evidence (issue #292)

- Timestamp: 2026-07-09T15-02
- Task: [P1-T7] `[expect-fail]`
- Command: `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~StoresWrapperEnumerationScopeTests|FullyQualifiedName~StoresEnumerationPhaseIdentity_WarnsWithoutDisabling"`
  - Note: targeted filter over the five new regression tests across both assemblies. `|` is the vstest OR operator (literal `OR` is rejected by this vstest build). `/InIsolation` loads the Moq assemblies cleanly.
- EXIT_CODE: 1 (non-zero — expected RED)

## Output Summary — RED baseline on HEAD (production fix NOT yet applied)

Total tests: 5. Result: three expected failures, two expected passes.

- **FAILED — T1** `Init_MaterializingStores_ObservesEnumerationPhaseIdentityInsideMoveNext`
  - Assertion: `Expected observed to contain only items matching (value == "<Stores-enumeration>") ... but {<null>, <null>} do(es) not match.`
  - Cause: on HEAD the `Init()` materialization (`StoresWrapper.cs:44`) runs with no ambient scope, so each `MoveNext()` observes `CurrentStoreContext.Current == null` (blank attribution).
- **FAILED — T2** `RewireOlObjectsAsync_MaterializingStores_ObservesEnumerationPhaseIdentityInsideMoveNext`
  - Assertion: `Expected observed to contain only items matching (value == "<Stores-enumeration>") ... but {<null>, <null>} do(es) not match.`
  - Cause: on HEAD the rewire materialization (`StoresWrapper.cs:89`) also runs with no ambient scope; observations are null.
- **FAILED — T3** `OnLockupDetected_StoresEnumerationPhaseIdentity_WarnsWithoutDisabling`
  - Exception: `Moq.MockException: IStoreDisableService.IsDisabled(StoreIdentity) invocation failed with mock behavior Strict. All invocations on the mock must have a corresponding setup.`
  - Cause: on HEAD `OnLockupDetected` passes the non-blank/non-unresolved phase identity through the existing guards and calls `IsDisabled`/`DisableSessionOnly` on the Strict mock — the verified crash path this fix must close.
- **PASSED — T4** `Init_HealthyMultiStore_PreservesIncludedSetAndOrder_AndClearsContextAfterReturn` (behavior-preserving invariant, GREEN before and after).
- **PASSED — T5** `Init_EnumerationThrowsMidStream_LeavesCurrentStoreContextNull` (scope-restore invariant, GREEN before and after).

## Fail-before condition for AC4

This artifact establishes the deterministic RED-before-GREEN condition: T1/T2/T3 fail on HEAD via the existing proxy/Moq seams (no live Outlook, no temp files, no waits). The GREEN transition is captured in [P2-T4] (`green-after-fix`).

## Pre-existing suite state

The full suite passed at baseline ([P0-T6]: 4514 passed, 0 failed). The three new failures above are the only failing tests introduced, and they are the intended RED regressions.
