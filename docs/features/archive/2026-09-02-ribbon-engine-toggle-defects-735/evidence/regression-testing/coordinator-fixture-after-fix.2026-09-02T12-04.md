# Finding 3 — Whole Coordinator Fixture, Both Partials (P3-T12)

Timestamp: 2026-09-03T02-44
Task: [P3-T12]
Command:

```
& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll `
  /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
  "/TestCaseFilter:FullyQualifiedName~EngineToggleStateCoordinatorTests" `
  "/Logger:trx;LogFileName=p3-t12.trx" `
  /ResultsDirectory:docs\features\active\2026-09-02-ribbon-engine-toggle-defects-735\evidence\regression-testing\p3-t12
```

EXIT_CODE: 0

## Results directory contents

Exactly one TRX file and no other entry:

```
p3-t12.trx
```

## Counts read from the TRX `ResultSummary/Counters` element

| Counter | Value |
|---|---|
| total | 24 |
| executed | 24 |
| passed | 24 |
| **failed** | **0** |
| notExecuted | 0 |

The filter matches the fully qualified name of the class, so it selects both partial files: the 18
pre-existing tests in `EngineToggleStateCoordinatorTests.cs` and the 6 new tests in
`EngineToggleStateCoordinatorTests.Race.cs`. 18 + 6 = 24, which confirms the partial split is
correctly wired and that no pre-existing test was lost.

## The two pre-existing tests the plan requires to still pass, unmodified

| Test | Outcome | Why it is the load-bearing check |
|---|---|---|
| `ExecuteToggleAsync_PerformsToggleThenRefreshThenCacheThenInvalidate_InOrder` | **Passed** | The update-before-invalidate ordering test. It probes the cache from inside the invalidation sink, which is where Office would re-query `getPressed`. The fix inserts a ticket capture and a conditional around the invalidation, so a regression that invalidated before applying the write, or that applied the write after invalidating, would fail here. |
| `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` | **Passed** | The faulted-prime test. It asserts the logged exception `BeSameAs` the original failure, so it pins base-exception unwrapping by reference. The CR-2 restructure changed the completion handler's control flow from testing the exception to testing the status; this test proves the faulted path's unwrapping behavior was preserved exactly. |

Neither test file line was modified. The only edit to
`TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` in this entire change is the single
`partial` keyword, verified independently by P3-T13.

## Full pre-existing set, all passing

`Constructor_WithNullEnginesAccessor_ThrowsArgumentNullException`,
`Constructor_WithNullInvalidateDelegate_ThrowsArgumentNullException`,
`Constructor_WithNullNotifyDelegate_ThrowsArgumentNullException`,
`Constructor_WithNullLogErrorDelegate_ThrowsArgumentNullException`,
`GetPressed_WithNullOrWhitespaceKey_ReturnsFalseWithoutPrimeOrInvalidate` (three data rows),
`GetPressed_WithUnmappedKey_ReturnsFalseWithoutPrime`,
`GetPressed_WhenEnginesAccessorReturnsNull_ReturnsFalseAndStartsNothing`,
`GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime`,
`GetPressed_AfterPrimeCompletes_ReturnsPrimedValueAndInvalidatesMappedControl`,
`GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse`,
`ExecuteToggleAsync_PerformsToggleThenRefreshThenCacheThenInvalidate_InOrder`,
`ExecuteToggleAsync_WhenToggleFaults_PropagatesUnchanged`,
`ExecuteToggleAsync_WithUnmappedKey_ThrowsArgumentException`,
`HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate`,
`HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing`,
`HandleToggleClickAsync_WhenEnginesAvailable_TogglesAndInvalidates`.

`GetPressed_AfterPrimeCompletes_ReturnsPrimedValueAndInvalidatesMappedControl` passing is a further
useful signal: it exercises the successful prime path through the new compare-and-apply write and
the new conditional invalidation, so the retyped cache and its unwrapping reader are correct on the
ordinary path as well as the contended one.

Output Summary: The whole coordinator fixture passes across both partial files. EXIT_CODE 0, TRX
counters total 24, passed 24, failed 0 — the 18 pre-existing tests plus the 6 new race tests. The
pre-existing update-before-invalidate ordering test and the pre-existing faulted-prime test are both
recorded as Passed and both are unmodified.
