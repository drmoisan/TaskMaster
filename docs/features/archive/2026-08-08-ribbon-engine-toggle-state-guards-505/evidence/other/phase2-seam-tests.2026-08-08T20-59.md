# P2-T6 — New Seam Tests

Timestamp: 2026-08-08T20-59

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; & '<VSTEST>' 'TaskMaster.Test\bin\Debug\TaskMaster.Test.dll' /InIsolation '/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests|FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleCatalogTests'"
```

EXIT_CODE: 0

## Output Summary

`Test Run Successful.` Total tests: **25** — Passed: **25**, Failed: **0**, Skipped: **0**.

The count reconciles exactly to the plan's prediction: **22 test members** across the two files
(6 in `EngineToggleCatalogTests`, 16 in `EngineToggleStateCoordinatorTests`), of which two are
`[DataTestMethod]`s contributing 2 and 3 data rows respectively, giving
`(6 - 1) + 2 + (16 - 1) + 3 = 25` executed cases. **25 is the correct result, not a mismatch.**

### `EngineToggleCatalogTests` — 7 executed cases, all PASSED

```
TryGetControlId_ForEachToggleEngineKey_ReturnsExpectedControlId ("Spam","SpamBayesEnabledToggle")
TryGetControlId_ForEachToggleEngineKey_ReturnsExpectedControlId ("Triage","TriageEnabledToggle")
TryGetControlId_ForUnknownEngineName_ReturnsFalse
TryGetControlId_WithNullEngineName_ReturnsFalse
TryGetControlId_WithEmptyEngineName_ReturnsFalse
EngineNames_ContainsExactlyTheTwoToggleEngineKeys
EngineNames_ContainsNoDuplicates
```

### `EngineToggleStateCoordinatorTests` — 18 executed cases, all PASSED

```
Constructor_WithNullEnginesAccessor_ThrowsArgumentNullException
Constructor_WithNullInvalidateDelegate_ThrowsArgumentNullException
Constructor_WithNullNotifyDelegate_ThrowsArgumentNullException
Constructor_WithNullLogErrorDelegate_ThrowsArgumentNullException
GetPressed_WithNullOrWhitespaceKey_ReturnsFalseWithoutPrimeOrInvalidate (null)
GetPressed_WithNullOrWhitespaceKey_ReturnsFalseWithoutPrimeOrInvalidate ("")
GetPressed_WithNullOrWhitespaceKey_ReturnsFalseWithoutPrimeOrInvalidate ("   ")
GetPressed_WithUnmappedKey_ReturnsFalseWithoutPrime
GetPressed_WhenEnginesAccessorReturnsNull_ReturnsFalseAndStartsNothing
GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime
GetPressed_AfterPrimeCompletes_ReturnsPrimedValueAndInvalidatesMappedControl
GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse
ExecuteToggleAsync_PerformsToggleThenRefreshThenCacheThenInvalidate_InOrder
ExecuteToggleAsync_WhenToggleFaults_PropagatesUnchanged
ExecuteToggleAsync_WithUnmappedKey_ThrowsArgumentException
HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate
HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing
HandleToggleClickAsync_WhenEnginesAvailable_TogglesAndInvalidates
```

Wall time: 1.6394 s. No test slept, polled, read the wall clock, or started a message pump; every
asynchronous outcome was driven by a `TaskCompletionSource` and awaited through the coordinator's
`GetPrimeTask` handle.

Binary outcome: PASS — zero failed, zero skipped.
