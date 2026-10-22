# PrimeRegistration Partial Tokens (P1-T1)

Timestamp: 2026-09-30T13-30
Command: CMD-TOKEN-COUNT (FILE TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs, TOKEN list TOKENS-PARTIAL)
EXIT_CODE: 0
Output Summary: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs created from the Delivered Source text (175 lines). Every TOKENS-PARTIAL count equals its required value; the FIRST-LINE ordering of test 1 holds (39 < 45 < 46 < 49 < 54 <= 56 < 59 < 70). No other file modified by this task.

## Counts (required value in parentheses)

| Token | Count | FIRST-LINE |
|---|---|---|
| `public async Task GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns()` | 1 (1) | 32 |
| `public async Task GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime()` | 1 (1) | 85 |
| `public async Task GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime()` | 1 (1) | 132 |
| `[TestMethod]` | 3 (3) | 31 |
| `[TestClass]` | 0 (0) | 0 |
| `public partial class EngineToggleStateCoordinatorTests` | 1 (1) | 17 |
| `var harness = new Harness();` | 3 (3) | 35 |
| `new Mock<` | 0 (0) | 0 |
| `MockBehavior` | 0 (0) | 0 |
| `private sealed class` | 0 (0) | 0 |
| `var handleCompletedDuringRead = true;` | 1 (1) | 39 |
| `Task handleSeenDuringRead = null;` | 1 (1) | 37 |
| `handleSeenDuringRead = harness.Coordinator.GetPrimeTask(SpamEngine);` | 1 (1) | 44 |
| `handleCompletedDuringRead = handleSeenDuringRead.IsCompleted;` | 1 (1) | 45 |
| `return Task.FromException<bool>(failure);` | 1 (1) | 46 |
| `// Act` | 3 (3) | 49 |
| `// Arrange` | 3 (3) | 34 |
| `// Assert` | 3 (3) | 52 |
| `.Should()` | 13 (no count gate; ordering only) | 54 |
| `must be registered before the activation read runs` | 1 (1) | 56 |
| `await handleSeenDuringRead;` | 1 (1) | 59 |
| `.NotBeSameAs(` | 1 (1) | 68 |
| `a failed prime removes its marker before its handle completes` | 1 (1) | 70 |
| `with no marker registered the returned handle is already complete` | 1 (1) | 74 |
| `a faulted prime is reported exactly once` | 1 (1) | 60 |
| `the sink receives the injected exception unchanged` | 2 (2) | 64 |
| `.BeSameAs(failure` | 2 (2) | 64 |
| `.ContainSingle(` | 3 (3) | 60 |
| `SetupSequence(x => x.EngineActiveAsync(SpamEngine))` | 2 (2) | 91 |
| `.Returns(Task.FromException<bool>(failure))` | 1 (1) | 92 |
| `.Returns(Task.FromCanceled<bool>(new CancellationToken(true)))` | 1 (1) | 138 |
| `.Returns(Task.FromResult(true));` | 2 (2) | 93 |
| `harness.Coordinator.GetPressed(SpamEngine);` | 5 (5) | 50 |
| `await harness.Coordinator.GetPrimeTask(SpamEngine);` | 2 (2) | 97 |
| `var secondPrime = harness.Coordinator.GetPrimeTask(SpamEngine);` | 2 (2) | 99 |
| `await secondPrime;` | 2 (2) | 100 |
| `Times.Exactly(2),` | 2 (2) | 105 |
| `a failed prime leaves no marker behind, so the later read starts a new prime` | 1 (1) | 106 |
| `a canceled prime leaves no marker behind, so the later read starts a new prime` | 1 (1) | 152 |
| `the new prime read the engine as active and cached that value` | 2 (2) | 111 |
| `only the successful prime changed state to display` | 2 (2) | 116 |
| `new[] { SpamToggleControlId },` | 2 (2) | 115 |
| `.BeAssignableTo<OperationCanceledException>(` | 1 (1) | 168 |
| `a canceled task carries no exception to unwrap, so one is synthesized` | 1 (1) | 169 |
| `Regression for issue #944` | 1 (at least 1) | 22 |
| `Invariant: the prime handle is registered before the activation read runs.` | 1 (1) | 23 |
| `using Moq;` | 1 (1) | 6 |
| `using System.Threading;` | 1 (1) | 2 |

## Ordering (test 1, by FIRST-LINE)

var handleCompletedDuringRead = true; (39) < handleCompletedDuringRead = handleSeenDuringRead.IsCompleted; (45) < return Task.FromException<bool>(failure); (46) < // Act (49) < .Should() (54) <= must be registered before the activation read runs (56) < await handleSeenDuringRead; (59) < a failed prime removes its marker before its handle completes (70). Holds; no assertion sits inside the setup callback.

## PRECOMMIT-FORMAT-RECHECK:

P2-T8, Timestamp: 2026-09-30T13-41. The scoped CSharpier format rewrote this partial (PRECOMMIT-FORMAT-REWRITES: 1; hash 6D28EBF80B5D4AE7C3C0099A8A329F7B8DF463E4258FF1B14D921CEB12C91A7C before, E7582241265ED1838921593C38420CEB1C3C8E9F101DD1405F55945C45A3A73B after). The rewrite converted the line endings from LF to CRLF (175 CR and 175 LF bytes after the format, the same convention as the Race partial); the text of every line is unchanged and the line count remains 175. CMD-TOKEN-COUNT with TOKENS-PARTIAL was re-run on the formatted file (Command: CMD-TOKEN-COUNT, FILE TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs; EXIT_CODE: 0).

Output Summary: every count and every FIRST-LINE value is identical to the P1-T1 table above (for example: the three method lines 1 each at 32, 85 and 132; `[TestMethod]` 3; `[TestClass]`, `new Mock<`, `MockBehavior`, `private sealed class` 0; `var harness = new Harness();` 3; `// Arrange`, `// Act`, `// Assert` 3 each; `.ContainSingle(` 3; `harness.Coordinator.GetPressed(SpamEngine);` 5; `.Should()` 13). The test 1 ordering holds unchanged: 39 < 45 < 46 < 49 < 54 <= 56 < 59 < 70. Every P1-T1 clause holds on the formatted text; no repair was needed.

## POST-FORMAT:

P3-T2, pass 1, Timestamp: 2026-09-30T13-44. Command: CMD-TOKEN-COUNT (FILE TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs, TOKEN list TOKENS-PARTIAL) on the tree after the P3-T1 repository-wide format (partial hash E7582241265ED1838921593C38420CEB1C3C8E9F101DD1405F55945C45A3A73B, unchanged by P3-T1). EXIT_CODE: 0.

Output Summary: every P1-T1 clause holds on the post-format tree. Rows re-printed (count, required value, FIRST-LINE):

| Token | Count | FIRST-LINE |
|---|---|---|
| `public async Task GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns()` | 1 (1) | 32 |
| `public async Task GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime()` | 1 (1) | 85 |
| `public async Task GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime()` | 1 (1) | 132 |
| `[TestMethod]` | 3 (3) | 31 |
| `[TestClass]` | 0 (0) | 0 |
| `public partial class EngineToggleStateCoordinatorTests` | 1 (1) | 17 |
| `var harness = new Harness();` | 3 (3) | 35 |
| `new Mock<` | 0 (0) | 0 |
| `MockBehavior` | 0 (0) | 0 |
| `private sealed class` | 0 (0) | 0 |
| `var handleCompletedDuringRead = true;` | 1 (1) | 39 |
| `Task handleSeenDuringRead = null;` | 1 (1) | 37 |
| `handleSeenDuringRead = harness.Coordinator.GetPrimeTask(SpamEngine);` | 1 (1) | 44 |
| `handleCompletedDuringRead = handleSeenDuringRead.IsCompleted;` | 1 (1) | 45 |
| `return Task.FromException<bool>(failure);` | 1 (1) | 46 |
| `// Act` | 3 (3) | 49 |
| `// Arrange` | 3 (3) | 34 |
| `// Assert` | 3 (3) | 52 |
| `.Should()` | 13 (ordering only) | 54 |
| `must be registered before the activation read runs` | 1 (1) | 56 |
| `await handleSeenDuringRead;` | 1 (1) | 59 |
| `.NotBeSameAs(` | 1 (1) | 68 |
| `a failed prime removes its marker before its handle completes` | 1 (1) | 70 |
| `with no marker registered the returned handle is already complete` | 1 (1) | 74 |
| `a faulted prime is reported exactly once` | 1 (1) | 60 |
| `the sink receives the injected exception unchanged` | 2 (2) | 64 |
| `.BeSameAs(failure` | 2 (2) | 64 |
| `.ContainSingle(` | 3 (3) | 60 |
| `SetupSequence(x => x.EngineActiveAsync(SpamEngine))` | 2 (2) | 91 |
| `.Returns(Task.FromException<bool>(failure))` | 1 (1) | 92 |
| `.Returns(Task.FromCanceled<bool>(new CancellationToken(true)))` | 1 (1) | 138 |
| `.Returns(Task.FromResult(true));` | 2 (2) | 93 |
| `harness.Coordinator.GetPressed(SpamEngine);` | 5 (5) | 50 |
| `await harness.Coordinator.GetPrimeTask(SpamEngine);` | 2 (2) | 97 |
| `var secondPrime = harness.Coordinator.GetPrimeTask(SpamEngine);` | 2 (2) | 99 |
| `await secondPrime;` | 2 (2) | 100 |
| `Times.Exactly(2),` | 2 (2) | 105 |
| `a failed prime leaves no marker behind, so the later read starts a new prime` | 1 (1) | 106 |
| `a canceled prime leaves no marker behind, so the later read starts a new prime` | 1 (1) | 152 |
| `the new prime read the engine as active and cached that value` | 2 (2) | 111 |
| `only the successful prime changed state to display` | 2 (2) | 116 |
| `new[] { SpamToggleControlId },` | 2 (2) | 115 |
| `.BeAssignableTo<OperationCanceledException>(` | 1 (1) | 168 |
| `a canceled task carries no exception to unwrap, so one is synthesized` | 1 (1) | 169 |
| `Regression for issue #944` | 1 (at least 1) | 22 |
| `Invariant: the prime handle is registered before the activation read runs.` | 1 (1) | 23 |
| `using Moq;` | 1 (1) | 6 |
| `using System.Threading;` | 1 (1) | 2 |

Ordering (test 1, by FIRST-LINE): 39 < 45 < 46 < 49 < 54 <= 56 < 59 < 70. Holds.

## PASS-2:

POST-FORMAT: (pass 2) P3-T2, pass 2, Timestamp: 2026-09-30T15-04. Command: CMD-TOKEN-COUNT (FILE TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeRegistration.cs, TOKEN list TOKENS-PARTIAL) on the tree after the pass-2 P3-T1 repository-wide format (partial hash E7582241265ED1838921593C38420CEB1C3C8E9F101DD1405F55945C45A3A73B, unchanged by P3-T1; 175 lines). EXIT_CODE: 0.

Output Summary: every P1-T1 clause holds on the pass-2 post-format tree; every row equals the pass-1 POST-FORMAT row. Rows re-printed (count, required value, FIRST-LINE):

| Token | Count | FIRST-LINE |
|---|---|---|
| `public async Task GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns()` | 1 (1) | 32 |
| `public async Task GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime()` | 1 (1) | 85 |
| `public async Task GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime()` | 1 (1) | 132 |
| `[TestMethod]` | 3 (3) | 31 |
| `[TestClass]` | 0 (0) | 0 |
| `public partial class EngineToggleStateCoordinatorTests` | 1 (1) | 17 |
| `var harness = new Harness();` | 3 (3) | 35 |
| `new Mock<` | 0 (0) | 0 |
| `MockBehavior` | 0 (0) | 0 |
| `private sealed class` | 0 (0) | 0 |
| `var handleCompletedDuringRead = true;` | 1 (1) | 39 |
| `Task handleSeenDuringRead = null;` | 1 (1) | 37 |
| `handleSeenDuringRead = harness.Coordinator.GetPrimeTask(SpamEngine);` | 1 (1) | 44 |
| `handleCompletedDuringRead = handleSeenDuringRead.IsCompleted;` | 1 (1) | 45 |
| `return Task.FromException<bool>(failure);` | 1 (1) | 46 |
| `// Act` | 3 (3) | 49 |
| `// Arrange` | 3 (3) | 34 |
| `// Assert` | 3 (3) | 52 |
| `.Should()` | 13 (ordering only) | 54 |
| `must be registered before the activation read runs` | 1 (1) | 56 |
| `await handleSeenDuringRead;` | 1 (1) | 59 |
| `.NotBeSameAs(` | 1 (1) | 68 |
| `a failed prime removes its marker before its handle completes` | 1 (1) | 70 |
| `with no marker registered the returned handle is already complete` | 1 (1) | 74 |
| `a faulted prime is reported exactly once` | 1 (1) | 60 |
| `the sink receives the injected exception unchanged` | 2 (2) | 64 |
| `.BeSameAs(failure` | 2 (2) | 64 |
| `.ContainSingle(` | 3 (3) | 60 |
| `SetupSequence(x => x.EngineActiveAsync(SpamEngine))` | 2 (2) | 91 |
| `.Returns(Task.FromException<bool>(failure))` | 1 (1) | 92 |
| `.Returns(Task.FromCanceled<bool>(new CancellationToken(true)))` | 1 (1) | 138 |
| `.Returns(Task.FromResult(true));` | 2 (2) | 93 |
| `harness.Coordinator.GetPressed(SpamEngine);` | 5 (5) | 50 |
| `await harness.Coordinator.GetPrimeTask(SpamEngine);` | 2 (2) | 97 |
| `var secondPrime = harness.Coordinator.GetPrimeTask(SpamEngine);` | 2 (2) | 99 |
| `await secondPrime;` | 2 (2) | 100 |
| `Times.Exactly(2),` | 2 (2) | 105 |
| `a failed prime leaves no marker behind, so the later read starts a new prime` | 1 (1) | 106 |
| `a canceled prime leaves no marker behind, so the later read starts a new prime` | 1 (1) | 152 |
| `the new prime read the engine as active and cached that value` | 2 (2) | 111 |
| `only the successful prime changed state to display` | 2 (2) | 116 |
| `new[] { SpamToggleControlId },` | 2 (2) | 115 |
| `.BeAssignableTo<OperationCanceledException>(` | 1 (1) | 168 |
| `a canceled task carries no exception to unwrap, so one is synthesized` | 1 (1) | 169 |
| `Regression for issue #944` | 1 (at least 1) | 22 |
| `Invariant: the prime handle is registered before the activation read runs.` | 1 (1) | 23 |
| `using Moq;` | 1 (1) | 6 |
| `using System.Threading;` | 1 (1) | 2 |

Ordering (test 1, by FIRST-LINE): 39 < 45 < 46 < 49 < 54 <= 56 < 59 < 70. Holds.
