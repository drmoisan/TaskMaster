# Regression Testing: Token and Line Counts of the Formatted Partial (P1-T3)

Timestamp: 2026-10-01T17-49
Task: P1-T3
Command: CMD-TOKEN-COUNT (FILE TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs, TOKEN TOKENS-PARTIAL-947); CMD-LINECOUNT
EXIT_CODE: 0

Output Summary:
- Every one of the 40 TOKENS-PARTIAL-947 counts equals the value of the Delivered Source paragraph `Expected token counts on the formatted partial`.
- `[TestMethod]` 4; `[TestClass]`, `new Mock<`, `MockBehavior`, `private sealed class`, `Thread.Sleep`, `Task.Delay` 0 each.
- `var harness = new Harness();`, `// Arrange`, `// Act`, `// Assert` 4 each; `SetupSequence(...)`, `Times.Exactly(2),`, `.NotBeSameAs(` 2 each; `.Be(TaskStatus.RanToCompletion,`, `.NotThrowAsync(`, `ThrowsAsync(failure)`, `HandleToggleClickAsync(SpamEngine)` and the four method names 1 each.
- The eighteen reason and literal tokens are at their listed counts (see table below).
- No token is split across a line break (no FORMATTER SPLITS GATED TOKEN).
- PARTIAL-LINES-AFTER-P1: 215 (at most 500).
- The other five LINES values equal their BASE-LINES-* values (442, 470, 277, 77, 175).
- Result: P1-T3 acceptance holds.

## TOKEN lines

```
TOKEN [[TestMethod]] = 4
TOKEN [[TestClass]] = 0
TOKEN [new Mock<] = 0
TOKEN [MockBehavior] = 0
TOKEN [private sealed class] = 0
TOKEN [var harness = new Harness();] = 4
TOKEN [// Arrange] = 4
TOKEN [// Act] = 4
TOKEN [// Assert] = 4
TOKEN [SetupSequence(x => x.EngineActiveAsync(SpamEngine))] = 2
TOKEN [Times.Exactly(2),] = 2
TOKEN [.NotBeSameAs(] = 2
TOKEN [.Be(TaskStatus.RanToCompletion,] = 1
TOKEN [.NotThrowAsync(] = 1
TOKEN [ThrowsAsync(failure)] = 1
TOKEN [HandleToggleClickAsync(SpamEngine)] = 1
TOKEN [Thread.Sleep] = 0
TOKEN [Task.Delay] = 0
TOKEN [GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime] = 1
TOKEN [GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime] = 1
TOKEN [GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared] = 1
TOKEN [HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport] = 1
TOKEN [a throwing sink leaves no marker behind, so the later read starts a new prime] = 2
TOKEN [the later read registered a new prime] = 2
TOKEN [the sink was invoked once before it threw] = 4
TOKEN [the sink receives the injected exception unchanged] = 2
TOKEN [the new prime read the engine as active and cached that value] = 2
TOKEN [only the successful prime changed state to display] = 2
TOKEN [a canceled task carries no exception to unwrap, so one is synthesized] = 1
TOKEN [the prime handle never faults] = 1
TOKEN [the report is attempted before the marker is cleared] = 1
TOKEN [a failed prime leaves nothing to display] = 1
TOKEN [the sink exception is contained, so the marker is still cleared] = 1
TOKEN [the click boundary contains a failure of the sink itself] = 1
TOKEN [the sink receives the toggle fault unchanged] = 1
TOKEN [a failed toggle changed no state to display] = 1
TOKEN [a fault is logged, not surfaced as a notice] = 1
TOKEN [toggle failed] = 1
TOKEN [Regression for issue #947] = 4
TOKEN [sink failed] = 4
```

## FIRST-LINE lines (observations)

```
FIRST-LINE [[TestMethod]] = 32
FIRST-LINE [[TestClass]] = 0
FIRST-LINE [new Mock<] = 0
FIRST-LINE [MockBehavior] = 0
FIRST-LINE [private sealed class] = 0
FIRST-LINE [var harness = new Harness();] = 36
FIRST-LINE [// Arrange] = 35
FIRST-LINE [// Act] = 47
FIRST-LINE [// Assert] = 53
FIRST-LINE [SetupSequence(x => x.EngineActiveAsync(SpamEngine))] = 40
FIRST-LINE [Times.Exactly(2),] = 56
FIRST-LINE [.NotBeSameAs(] = 59
FIRST-LINE [.Be(TaskStatus.RanToCompletion,] = 163
FIRST-LINE [.NotThrowAsync(] = 203
FIRST-LINE [ThrowsAsync(failure)] = 195
FIRST-LINE [HandleToggleClickAsync(SpamEngine)] = 199
FIRST-LINE [Thread.Sleep] = 0
FIRST-LINE [Task.Delay] = 0
FIRST-LINE [GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime] = 33
FIRST-LINE [GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime] = 85
FIRST-LINE [GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared] = 140
FIRST-LINE [HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport] = 190
FIRST-LINE [a throwing sink leaves no marker behind, so the later read starts a new prime] = 57
FIRST-LINE [the later read registered a new prime] = 59
FIRST-LINE [the sink was invoked once before it threw] = 61
FIRST-LINE [the sink receives the injected exception unchanged] = 65
FIRST-LINE [the new prime read the engine as active and cached that value] = 69
FIRST-LINE [only the successful prime changed state to display] = 74
FIRST-LINE [a canceled task carries no exception to unwrap, so one is synthesized] = 117
FIRST-LINE [the prime handle never faults] = 163
FIRST-LINE [the report is attempted before the marker is cleared] = 166
FIRST-LINE [a failed prime leaves nothing to display] = 172
FIRST-LINE [the sink exception is contained, so the marker is still cleared] = 178
FIRST-LINE [the click boundary contains a failure of the sink itself] = 203
FIRST-LINE [the sink receives the toggle fault unchanged] = 208
FIRST-LINE [a failed toggle changed no state to display] = 209
FIRST-LINE [a fault is logged, not surfaced as a notice] = 210
FIRST-LINE [toggle failed] = 194
FIRST-LINE [Regression for issue #947] = 24
FIRST-LINE [sink failed] = 43
```

## LINES (CMD-LINECOUNT)

```
LINES TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = 442
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs = 470
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.Race.cs = 277
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs = 77
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs = 175
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs = 215
```

PARTIAL-LINES-AFTER-P1: 215

## POST-FORMAT:

Timestamp: 2026-10-01T18-02
Task: P2-T2 (P1-T3 commands re-run unchanged on the tree after the P2-T1 repository-wide format)
Command: CMD-TOKEN-COUNT (FILE TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs, TOKEN TOKENS-PARTIAL-947); CMD-LINECOUNT
EXIT_CODE: 0

Output Summary:
- Every one of the 40 TOKEN counts and every FIRST-LINE value is identical to the P1-T3 section above; every P1-T3 acceptance clause holds on the post-format tree.
- `[TestMethod]` 4; `[TestClass]`, `new Mock<`, `MockBehavior`, `private sealed class`, `Thread.Sleep`, `Task.Delay` 0 each; `.Be(TaskStatus.RanToCompletion,` 1.
- No token split across a line break.
- ThrowingSink partial LINES 215 (at most 500); production 476; the other four partials 470, 277, 77, 175.
- Result: POST-FORMAT clauses hold.

TOKEN lines:

```
TOKEN [[TestMethod]] = 4
TOKEN [[TestClass]] = 0
TOKEN [new Mock<] = 0
TOKEN [MockBehavior] = 0
TOKEN [private sealed class] = 0
TOKEN [var harness = new Harness();] = 4
TOKEN [// Arrange] = 4
TOKEN [// Act] = 4
TOKEN [// Assert] = 4
TOKEN [SetupSequence(x => x.EngineActiveAsync(SpamEngine))] = 2
TOKEN [Times.Exactly(2),] = 2
TOKEN [.NotBeSameAs(] = 2
TOKEN [.Be(TaskStatus.RanToCompletion,] = 1
TOKEN [.NotThrowAsync(] = 1
TOKEN [ThrowsAsync(failure)] = 1
TOKEN [HandleToggleClickAsync(SpamEngine)] = 1
TOKEN [Thread.Sleep] = 0
TOKEN [Task.Delay] = 0
TOKEN [GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime] = 1
TOKEN [GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime] = 1
TOKEN [GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared] = 1
TOKEN [HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport] = 1
TOKEN [a throwing sink leaves no marker behind, so the later read starts a new prime] = 2
TOKEN [the later read registered a new prime] = 2
TOKEN [the sink was invoked once before it threw] = 4
TOKEN [the sink receives the injected exception unchanged] = 2
TOKEN [the new prime read the engine as active and cached that value] = 2
TOKEN [only the successful prime changed state to display] = 2
TOKEN [a canceled task carries no exception to unwrap, so one is synthesized] = 1
TOKEN [the prime handle never faults] = 1
TOKEN [the report is attempted before the marker is cleared] = 1
TOKEN [a failed prime leaves nothing to display] = 1
TOKEN [the sink exception is contained, so the marker is still cleared] = 1
TOKEN [the click boundary contains a failure of the sink itself] = 1
TOKEN [the sink receives the toggle fault unchanged] = 1
TOKEN [a failed toggle changed no state to display] = 1
TOKEN [a fault is logged, not surfaced as a notice] = 1
TOKEN [toggle failed] = 1
TOKEN [Regression for issue #947] = 4
TOKEN [sink failed] = 4
```

FIRST-LINE lines:

```
FIRST-LINE [[TestMethod]] = 32
FIRST-LINE [[TestClass]] = 0
FIRST-LINE [new Mock<] = 0
FIRST-LINE [MockBehavior] = 0
FIRST-LINE [private sealed class] = 0
FIRST-LINE [var harness = new Harness();] = 36
FIRST-LINE [// Arrange] = 35
FIRST-LINE [// Act] = 47
FIRST-LINE [// Assert] = 53
FIRST-LINE [SetupSequence(x => x.EngineActiveAsync(SpamEngine))] = 40
FIRST-LINE [Times.Exactly(2),] = 56
FIRST-LINE [.NotBeSameAs(] = 59
FIRST-LINE [.Be(TaskStatus.RanToCompletion,] = 163
FIRST-LINE [.NotThrowAsync(] = 203
FIRST-LINE [ThrowsAsync(failure)] = 195
FIRST-LINE [HandleToggleClickAsync(SpamEngine)] = 199
FIRST-LINE [Thread.Sleep] = 0
FIRST-LINE [Task.Delay] = 0
FIRST-LINE [GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime] = 33
FIRST-LINE [GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime] = 85
FIRST-LINE [GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared] = 140
FIRST-LINE [HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport] = 190
FIRST-LINE [a throwing sink leaves no marker behind, so the later read starts a new prime] = 57
FIRST-LINE [the later read registered a new prime] = 59
FIRST-LINE [the sink was invoked once before it threw] = 61
FIRST-LINE [the sink receives the injected exception unchanged] = 65
FIRST-LINE [the new prime read the engine as active and cached that value] = 69
FIRST-LINE [only the successful prime changed state to display] = 74
FIRST-LINE [a canceled task carries no exception to unwrap, so one is synthesized] = 117
FIRST-LINE [the prime handle never faults] = 163
FIRST-LINE [the report is attempted before the marker is cleared] = 166
FIRST-LINE [a failed prime leaves nothing to display] = 172
FIRST-LINE [the sink exception is contained, so the marker is still cleared] = 178
FIRST-LINE [the click boundary contains a failure of the sink itself] = 203
FIRST-LINE [the sink receives the toggle fault unchanged] = 208
FIRST-LINE [a failed toggle changed no state to display] = 209
FIRST-LINE [a fault is logged, not surfaced as a notice] = 210
FIRST-LINE [toggle failed] = 194
FIRST-LINE [Regression for issue #947] = 24
FIRST-LINE [sink failed] = 43
```

LINES (CMD-LINECOUNT):

```
LINES TaskMaster\Ribbon\EngineToggleStateCoordinator.cs = 476
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs = 470
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.Race.cs = 277
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs = 77
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs = 175
LINES TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.ThrowingSink.cs = 215
```
