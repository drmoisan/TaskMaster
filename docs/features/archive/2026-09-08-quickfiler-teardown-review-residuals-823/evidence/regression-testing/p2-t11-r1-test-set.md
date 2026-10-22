# Phase 2 — The six named R1 tests run together

Timestamp: 2026-09-09T14-12

Task: [P2-T11]

Command:

```
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll `
  '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' `
  '/ResultsDirectory:TestResults\823-p2-t11' `
  '/TestCaseFilter:FullyQualifiedName~PopulateWithCurrent_OnTwoFailingStoresInOneController_RetriesEachStoreOnce|FullyQualifiedName~PopulateWithCurrent_OnOneFailingStoreReselectedThreeTimes_RetriesLookupOnlyOnce|FullyQualifiedName~PopulateWithCurrent_CalledTwiceOnOneController_RetriesLookupOnlyOnce|FullyQualifiedName~PopulateWithCurrent_OnASecondControllerOverTheSameFailingStore_RetriesOnceMore|FullyQualifiedName~PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_NeverInvokesExchangeUserLookup|FullyQualifiedName~PopulateWithCurrent_WithNullCurrent_RendersPlaceholdersAndDoesNotThrow'
```

The whole `/TestCaseFilter:` argument was passed as a single-quoted string so PowerShell did not
interpret the disjunction operator.

EXIT_CODE: 0

TOTAL: 6
PASSED: 6
FAILED: 0

Per-test results:

- `PopulateWithCurrent_OnTwoFailingStoresInOneController_RetriesEachStoreOnce` — passed. This is
  the AC1 test, added by [P1-T1].
- `PopulateWithCurrent_OnOneFailingStoreReselectedThreeTimes_RetriesLookupOnlyOnce` — passed. This
  is the AC3 no-regression test, added by [P2-T8].
- `PopulateWithCurrent_CalledTwiceOnOneController_RetriesLookupOnlyOnce` — passed, unchanged, with
  its `Times.Once()` assertion intact. AC6.
- `PopulateWithCurrent_OnASecondControllerOverTheSameFailingStore_RetriesOnceMore` — passed,
  unchanged, with its `Times.Exactly(2)` assertion intact. AC4, the criterion that forbids a
  `static` latch.
- `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_NeverInvokesExchangeUserLookup` — passed,
  unchanged. AC6.
- `PopulateWithCurrent_WithNullCurrent_RendersPlaceholdersAndDoesNotThrow` — passed, unchanged.
  AC5, the criterion that forbids deriving the set key before the `Current is not null` conjunct.

Output Summary: `Test Run Successful.` 6 tests total, 6 passed, 0 failed, in 1.66 seconds. Exit
code 0. Both new tests pass and all four pre-existing R1 tests remain green with their assertions
unmodified.
