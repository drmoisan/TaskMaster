# Anchor: Test-Side Facts (P0-T4)

Timestamp: 2026-10-01T17-37
Task: P0-T4
Command: CMD-LINECOUNT; CMD-TOKEN-COUNT over TaskMaster.Test\TaskMaster.Test.csproj, TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs, .PrimeFaultOrdering.cs, .Race.cs and .PrimeRegistration.cs (token lists per plan P0-T4); pwsh -NoProfile -Command NEW-NAME scan over TaskMaster.Test\**\*.cs
EXIT_CODE: 0

Output Summary:
- BASE-LINES-PROD: 442
- BASE-LINES-MAIN: 470
- BASE-LINES-RACE: 277
- BASE-LINES-PFO: 77
- BASE-LINES-PR: 175
- Each of the five is at most 500; the ThrowingSink path reads ABSENT.
- Project file: the four existing coordinator entries count 1 each; the ThrowingSink entry counts 0.
- PR-ENTRY-LINE: 361
- Main fixture: all fourteen tokens count 1; Errors.Add first line 417 < OnLogError?.Invoke first line 418 (the harness records the report before the hook can throw).
- PrimeFaultOrdering: both tokens count 1. Race token 1. Each PrimeRegistration token 1.
- NEW-NAME counts all 0; CS_LINES_SCANNED=16074 (at least 1000).
- Result: no FIXTURE SHAPE MISMATCH.

## CMD-LINECOUNT

```
LINES TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = 442
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs = 470
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.Race.cs = 277
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs = 77
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs = 175
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs = ABSENT
```

## CMD-TOKEN-COUNT: TaskMaster.Test\TaskMaster.Test.csproj

```
TOKEN [EngineToggleStateCoordinatorTests.cs] = 1
TOKEN [EngineToggleStateCoordinatorTests.Race.cs] = 1
TOKEN [EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs] = 1
TOKEN [EngineToggleStateCoordinatorTests.PrimeRegistration.cs] = 1
TOKEN [EngineToggleStateCoordinatorTests.ThrowingSink.cs] = 0
FIRST-LINE [EngineToggleStateCoordinatorTests.cs] = 352
FIRST-LINE [EngineToggleStateCoordinatorTests.Race.cs] = 359
FIRST-LINE [EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs] = 360
FIRST-LINE [EngineToggleStateCoordinatorTests.PrimeRegistration.cs] = 361
FIRST-LINE [EngineToggleStateCoordinatorTests.ThrowingSink.cs] = 0
```

## CMD-TOKEN-COUNT: TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs

```
TOKEN [private sealed class Harness] = 1
TOKEN [new Mock<IAppItemEngines>(MockBehavior.Strict);] = 1
TOKEN [Errors.Add(new LoggedError(message, exception));] = 1
TOKEN [OnLogError?.Invoke(message, exception);] = 1
TOKEN [internal Action<string, Exception> OnLogError { get; set; }] = 1
TOKEN [internal List<string> Invalidations] = 1
TOKEN [internal List<string> Notifications] = 1
TOKEN [internal List<LoggedError> Errors] = 1
TOKEN [private sealed class LoggedError] = 1
TOKEN [private const string SpamEngine =] = 1
TOKEN [private const string SpamToggleControlId =] = 1
TOKEN [HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate] = 1
TOKEN [GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime] = 1
TOKEN [GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse] = 1
FIRST-LINE [private sealed class Harness] = 403
FIRST-LINE [new Mock<IAppItemEngines>(MockBehavior.Strict);] = 424
FIRST-LINE [Errors.Add(new LoggedError(message, exception));] = 417
FIRST-LINE [OnLogError?.Invoke(message, exception);] = 418
FIRST-LINE [internal Action<string, Exception> OnLogError { get; set; }] = 445
FIRST-LINE [internal List<string> Invalidations] = 447
FIRST-LINE [internal List<string> Notifications] = 449
FIRST-LINE [internal List<LoggedError> Errors] = 451
FIRST-LINE [private sealed class LoggedError] = 457
FIRST-LINE [private const string SpamEngine =] = 25
FIRST-LINE [private const string SpamToggleControlId =] = 26
FIRST-LINE [HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate] = 334
FIRST-LINE [GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime] = 160
FIRST-LINE [GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse] = 213
```

## CMD-TOKEN-COUNT: TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs

```
TOKEN [GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged] = 1
TOKEN [[TestMethod]] = 1
FIRST-LINE [GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged] = 27
FIRST-LINE [[TestMethod]] = 26
```

## CMD-TOKEN-COUNT: TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.Race.cs

```
TOKEN [GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker] = 1
FIRST-LINE [GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker] = 204
```

## CMD-TOKEN-COUNT: TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs

```
TOKEN [GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns] = 1
TOKEN [GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime] = 1
TOKEN [GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime] = 1
FIRST-LINE [GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns] = 32
FIRST-LINE [GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime] = 85
FIRST-LINE [GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime] = 132
```

## NEW-NAME scan

```
NEW-NAME [GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime] = 0
NEW-NAME [GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime] = 0
NEW-NAME [GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared] = 0
NEW-NAME [HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport] = 0
NEW-NAME [ThrowingSink] = 0
CS_LINES_SCANNED=16074
```
