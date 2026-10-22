# Anchor Test Side (P0-T8)

Timestamp: 2026-09-30T13-20
Command: CMD-LINECOUNT; CMD-TOKEN-COUNT (TaskMaster.Test\TaskMaster.Test.csproj, three compile-entry tokens); CMD-TOKEN-COUNT (TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs, eleven tokens); CMD-TOKEN-COUNT (TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs, two tokens); pwsh -NoProfile -Command (NEW-NAME counts over TaskMaster.Test *.cs)
EXIT_CODE: 0
Output Summary: Line counts 420 / 470 / 277 / 77, all at most 500; PrimeRegistration ABSENT. Project file: Race 1 (line 359), PrimeFaultOrdering 1 (line 360), PrimeRegistration 0. Main fixture: first ten tokens each 1, OnLogError 2. PrimeFaultOrdering partial: both tokens 1. Every NEW-NAME count 0. No FIXTURE SHAPE MISMATCH.

## Line counts (CMD-LINECOUNT)

- ANCHOR-LINES-PROD: 420 (TaskMaster\Ribbon\EngineToggleStateCoordinator.cs)
- ANCHOR-LINES-MAIN-FIXTURE: 470 (TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs)
- ANCHOR-LINES-RACE: 277 (TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.Race.cs)
- ANCHOR-LINES-PFO: 77 (TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs)
- LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs = ABSENT

## Project file (TaskMaster.Test\TaskMaster.Test.csproj)

| Token | Count | FIRST-LINE |
|---|---|---|
| EngineToggleStateCoordinatorTests.Race.cs | 1 | 359 |
| EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs | 1 | 360 |
| EngineToggleStateCoordinatorTests.PrimeRegistration.cs | 0 | 0 |

RACE-ENTRY-LINE: 359
PFO-ENTRY-LINE: 360
(P1-T2 insertion point: line 361)

## Main fixture (TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs)

| Token | Count | FIRST-LINE |
|---|---|---|
| private sealed class Harness | 1 | 403 |
| new Mock&lt;IAppItemEngines&gt;(MockBehavior.Strict) | 1 | 424 |
| internal Mock&lt;IAppItemEngines&gt; Engines | 1 | 423 |
| internal EngineToggleStateCoordinator Coordinator | 1 | 426 |
| internal List&lt;string&gt; Invalidations | 1 | 447 |
| internal List&lt;LoggedError&gt; Errors | 1 | 451 |
| private sealed class LoggedError | 1 | 457 |
| private const string SpamEngine = | 1 | 25 |
| private const string SpamToggleControlId = | 1 | 26 |
| public async Task GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime() | 1 | 160 |
| OnLogError | 2 | 418 |

## PrimeFaultOrdering partial

| Token | Count | FIRST-LINE |
|---|---|---|
| GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged() | 1 | 27 |
| [TestMethod] | 1 | 26 |

## New test names (must be absent)

- NEW-NAME [GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns] = 0
- NEW-NAME [GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime] = 0
- NEW-NAME [GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime] = 0
