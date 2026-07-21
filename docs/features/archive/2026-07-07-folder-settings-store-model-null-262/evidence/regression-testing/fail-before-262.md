# Fail-Before (Red) — Issue #262 (P2-T4)

Timestamp: 2026-07-07T23-49

Command:
`vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~LoadStoresAsync_WhenConfigMissing_BuildsFreshStoresWrapper|FullyQualifiedName~LoadStoresAsync_WhenConfigDeserializesToNull_BuildsFreshStoresWrapper|FullyQualifiedName~LoadStoresAsync_WhenDeserializeThrows_AbsorbsExceptionAndLeavesStoresWrapperNull"`

(Run against the Phase-1 production code: `LoadStoresAsync` is still the byte-for-byte original;
the behavioral fix is NOT yet applied. `/InIsolation` is required for this Moq assembly.)

EXIT_CODE: 1 (failing run, as required for [expect-fail]; not SKIPPED)

Output Summary:
- Total tests: 3. Passed: 0. Failed: 3. All three RED as designed (AC5).

Per-test failure reason:
1. `LoadStoresAsync_WhenConfigMissing_BuildsFreshStoresWrapper` (AC1) — FAILED.
   "Expected sut.StoresWrapper to refer to UtilitiesCS.OutlookObjects.Store.StoresWrapper" (the
   sentinel), but the Phase-1 code logs `logger.Error("StoresWrapper config not found.")` and leaves
   `StoresWrapper` null; `BuildFreshStoresWrapper()` is never invoked.
2. `LoadStoresAsync_WhenConfigDeserializesToNull_BuildsFreshStoresWrapper` (AC2) — FAILED.
   Same "Expected sut.StoresWrapper to refer to ... StoresWrapper" (the sentinel); the Phase-1 code
   assigns the null deserialize result and never falls back to `BuildFreshStoresWrapper()`.
3. `LoadStoresAsync_WhenDeserializeThrows_AbsorbsExceptionAndLeavesStoresWrapperNull` (AC3) — FAILED.
   "Did not expect any exception, but found System.InvalidOperationException: Operation is not valid
   due to the current state of the object." The Phase-1 code has no try/catch, so the deserialize
   exception propagates out of `LoadStoresAsync`.

These are the exact failure modes the behavioral fix (Phase 3) will resolve.
