# Anchor Test Side (P0-T7)

Timestamp: 2026-10-01T23-01
Command: CMD-LINECOUNT; pwsh csproj and fixture census payload over TaskMaster.Test\TaskMaster.Test.csproj and TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests*.cs; CMD-TOKEN-COUNT with FILE TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs
EXIT_CODE: 0
Output Summary: five fixture partials (shape S), each registered once; FIXTURE-ENTRIES=5; LAST-FIXTURE-ENTRY-LINE=362; NEW-ENTRY-COUNT=0; TESTMETHOD=29, DATAROW=3, EXPECTED-CASES=32; TESTCLASS=1; TRIAGEENGINE=0; every NEW-NAME count 0; primary-fixture harness tokens each 1, OnLogError 2.

## CMD-LINECOUNT output

```
LINES TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = 476
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs = 470
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs = 77
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs = 175
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.Race.cs = 277
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs = 215
PARTIAL-COUNT: 5
```

Anchor line counts (each at most 500):

- ANCHOR-LINES-PROD: 476 (TaskMaster\Ribbon\EngineToggleStateCoordinator.cs)
- ANCHOR-LINES-PRIMARY: 470 (EngineToggleStateCoordinatorTests.cs)
- ANCHOR-LINES-PRIMEFAULTORDERING: 77
- ANCHOR-LINES-PRIMEREGISTRATION: 175
- ANCHOR-LINES-RACE: 277
- ANCHOR-LINES-THROWINGSINK: 215
- ANCHOR-PARTIAL-COUNT: 5 (shape S)

The RepeatFaultSuppression path does not appear in the LINES rows.

## Census payload output

```
FIXTURE-ENTRIES=5 LAST-FIXTURE-ENTRY-LINE=362
NEW-ENTRY-COUNT=0
PARTIAL-REGISTERED [EngineToggleStateCoordinatorTests.cs] = 1
PARTIAL-REGISTERED [EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs] = 1
PARTIAL-REGISTERED [EngineToggleStateCoordinatorTests.PrimeRegistration.cs] = 1
PARTIAL-REGISTERED [EngineToggleStateCoordinatorTests.Race.cs] = 1
PARTIAL-REGISTERED [EngineToggleStateCoordinatorTests.ThrowingSink.cs] = 1
TESTMETHOD=29 DATATESTMETHOD=1 DATAROW=3 TESTCLASS=1 TRIAGEENGINE=0 EXPECTED-CASES=32
NEW-NAME [GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly] = 0
NEW-NAME [GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly] = 0
NEW-NAME [GetPressed_AfterSuppressedFaults_LaterSuccessfulPrimeCachesStateAndInvalidatesOnce] = 0
NEW-NAME [GetPressed_WhenFailureKindChanges_LogsNewKindOnce] = 0
NEW-NAME [GetPressed_WhenSpamFaultIsSuppressed_FirstTriageFaultIsStillLogged] = 0
NEW-NAME [HandleToggleClickAsync_AfterSuppressedPrimeFault_StillLogsToggleFault] = 0
NEW-NAME [GetPressed_WhenPrimeFaults_FirstReportStatesRepeatsAreNotLoggedAgain] = 0
```

- LAST-FIXTURE-ENTRY-LINE: 362 (the ThrowingSink entry under shape S; the P1-T2 insertion point is line 363)
- EXPECTED-CASES: 32

## CMD-TOKEN-COUNT (primary fixture) output

```
TOKEN [private sealed class Harness] = 1
TOKEN [new Mock<IAppItemEngines>(MockBehavior.Strict)] = 1
TOKEN [internal Mock<IAppItemEngines> Engines] = 1
TOKEN [internal EngineToggleStateCoordinator Coordinator] = 1
TOKEN [internal List<string> Invalidations] = 1
TOKEN [internal List<string> Notifications] = 1
TOKEN [internal List<LoggedError> Errors] = 1
TOKEN [private sealed class LoggedError] = 1
TOKEN [private const string SpamEngine =] = 1
TOKEN [private const string SpamToggleControlId =] = 1
TOKEN [OnLogError] = 2
FIRST-LINE [private sealed class Harness] = 403
FIRST-LINE [new Mock<IAppItemEngines>(MockBehavior.Strict)] = 424
FIRST-LINE [internal Mock<IAppItemEngines> Engines] = 423
FIRST-LINE [internal EngineToggleStateCoordinator Coordinator] = 426
FIRST-LINE [internal List<string> Invalidations] = 447
FIRST-LINE [internal List<string> Notifications] = 449
FIRST-LINE [internal List<LoggedError> Errors] = 451
FIRST-LINE [private sealed class LoggedError] = 457
FIRST-LINE [private const string SpamEngine =] = 25
FIRST-LINE [private const string SpamToggleControlId =] = 26
FIRST-LINE [OnLogError] = 418
```

## Acceptance checks

- every LINES value at most 500; PARTIAL-COUNT 5 (shape S)
- every PARTIAL-REGISTERED count 1; FIXTURE-ENTRIES (5) equals PARTIAL-COUNT (5)
- NEW-ENTRY-COUNT=0; TESTCLASS=1; TRIAGEENGINE=0; every NEW-NAME count 0
- the first ten primary-fixture tokens each count 1; OnLogError 2 (at least 1)
