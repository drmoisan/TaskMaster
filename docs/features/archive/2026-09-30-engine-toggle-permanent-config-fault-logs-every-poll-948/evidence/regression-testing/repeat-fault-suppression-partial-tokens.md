# Repeat Fault Suppression Partial Tokens (P1-T1)

Timestamp: 2026-10-01T23-39
Command: CMD-TOKEN-COUNT with FILE TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs and TOKEN list TOKENS-PARTIAL
EXIT_CODE: 0
Output Summary: the new partial (281 lines as written) carries each of the seven method lines once, seven [TestMethod], no [TestClass], mock, MockBehavior or nested class; seven harnesses and seven Arrange/Act/Assert triples; every gated token at its required count; every banned determinism token 0; FIRST-LINE order .Be(1, ...) 53 < .BeSameAs(failure, ...) 57 < Times.Exactly(5), 61. No other file was modified by this task.

```
TOKEN [public async Task GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly()] = 1
TOKEN [public async Task GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly()] = 1
TOKEN [public async Task GetPressed_AfterSuppressedFaults_LaterSuccessfulPrimeCachesStateAndInvalidatesOnce()] = 1
TOKEN [public async Task GetPressed_WhenFailureKindChanges_LogsNewKindOnce()] = 1
TOKEN [public async Task GetPressed_WhenSpamFaultIsSuppressed_FirstTriageFaultIsStillLogged()] = 1
TOKEN [public async Task HandleToggleClickAsync_AfterSuppressedPrimeFault_StillLogsToggleFault()] = 1
TOKEN [public async Task GetPressed_WhenPrimeFaults_FirstReportStatesRepeatsAreNotLoggedAgain()] = 1
TOKEN [[TestMethod]] = 7
TOKEN [[TestClass]] = 0
TOKEN [public partial class EngineToggleStateCoordinatorTests] = 1
TOKEN [new Mock<] = 0
TOKEN [MockBehavior] = 0
TOKEN [private sealed class] = 0
TOKEN [private const string TriageEngine = "Triage";] = 1
TOKEN [var harness = new Harness();] = 7
TOKEN [// Arrange] = 7
TOKEN [// Act] = 7
TOKEN [// Assert] = 7
TOKEN [private static async Task<bool> PollAsync(Harness harness, string engineName, int polls)] = 1
TOKEN [for (var poll = 0; poll < polls; poll++)] = 1
TOKEN [pressed = harness.Coordinator.GetPressed(engineName);] = 1
TOKEN [await harness.Coordinator.GetPrimeTask(engineName);] = 1
TOKEN [await PollAsync(harness, SpamEngine, 5);] = 2
TOKEN [await PollAsync(harness, SpamEngine, 3);] = 1
TOKEN [await PollAsync(harness, SpamEngine, 4);] = 1
TOKEN [await PollAsync(harness, SpamEngine, 2);] = 2
TOKEN [await PollAsync(harness, TriageEngine, 1);] = 1
TOKEN [await PollAsync(harness, SpamEngine, 1);] = 1
TOKEN [.Be(1, "a repeated fault of one kind for one engine is reported once");] = 1
TOKEN [.BeSameAs(failure, "the first report carries the injected fault");] = 1
TOKEN [suppressing the report must not suppress the re-prime] = 1
TOKEN [repeated cancellations are one failure kind] = 1
TOKEN [the second identical fault is a suppressed repeat] = 1
TOKEN [each distinct failure kind is reported once] = 1
TOKEN [suppression is keyed by engine as well as by kind] = 1
TOKEN [one suppressed prime repeat, every toggle fault] = 1
TOKEN [the entry must state that repeats are suppressed] = 1
TOKEN [.Contain("not logged again"] = 1
TOKEN [.EndWith("Further failures of this kind for this engine are not logged again.");] = 1
TOKEN [Times.Exactly(5),] = 2
TOKEN [Times.Exactly(3),] = 1
TOKEN [Times.Exactly(4),] = 1
TOKEN [.BeAssignableTo<OperationCanceledException>(] = 1
TOKEN [Task.FromCanceled<bool>(new CancellationToken(true))] = 1
TOKEN [new IOException(] = 1
TOKEN [using System.IO;] = 1
TOKEN [using Moq;] = 1
TOKEN [.ThrowsAsync(toggleFailure)] = 1
TOKEN [.ContainSingle(] = 3
TOKEN [.HaveCount(2,] = 3
TOKEN [new[] { SpamToggleControlId },] = 1
TOKEN [harness.Invalidations.Should().BeEmpty(] = 2
TOKEN [harness.Notifications.Should().BeEmpty(] = 1
TOKEN [pressed.Should().BeFalse(] = 2
TOKEN [Regression tests for issue #948] = 1
TOKEN [Thread.Sleep] = 0
TOKEN [Task.Delay] = 0
TOKEN [SpinWait] = 0
TOKEN [while (] = 0
TOKEN [DateTime] = 0
TOKEN [Stopwatch] = 0
TOKEN [.Wait(] = 0
TOKEN [.Result] = 0
TOKEN [DoNotParallelize] = 0
TOKEN [[Timeout] = 0
TOKEN [GetTempPath] = 0
TOKEN [File.] = 0
TOKEN [TimeProvider] = 0
TOKEN [ManualResetEvent] = 0
FIRST-LINE [.Be(1, "a repeated fault of one kind for one engine is reported once");] = 53
FIRST-LINE [.BeSameAs(failure, "the first report carries the injected fault");] = 57
FIRST-LINE [Times.Exactly(5),] = 61
FIRST-LINE [public async Task GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly()] = 39
FIRST-LINE [public async Task GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly()] = 74
FILE-LINES: 281
```

(The FIRST-LINE rows for every other token were printed and are retained in the run; the three rows above are the ones the acceptance order clause reads, plus the first two method lines showing test A precedes test B.)

## PRECOMMIT-FORMAT-RECHECK: (P2-T9, after the scoped CSharpier format)

Timestamp: 2026-10-02T00-01. The scoped format rewrote both Write Set C# files (PRECOMMIT-FORMAT-REWRITES: 2), so P1-T1's CMD-TOKEN-COUNT with TOKENS-PARTIAL was re-run on the formatted partial (290 lines). Every count is unchanged from the P1-T1 run above: the seven method lines 1 each; [TestMethod] 7; [TestClass], new Mock<, MockBehavior and private sealed class 0; harness and Arrange/Act/Assert 7 each; the single-count tokens 1 each; await PollAsync(harness, SpamEngine, 5); and (…, 2); 2 each; Times.Exactly(5), 2; harness.Invalidations.Should().BeEmpty( 2; pressed.Should().BeFalse( 2; .ContainSingle( 3; .HaveCount(2, 3; Regression tests for issue #948 1; every banned token from Thread.Sleep through ManualResetEvent 0. FIRST-LINE: .Be(1, ...) 53 < .BeSameAs(failure, ...) 57 < Times.Exactly(5), 61. FILE-LINES: 290. Every P1-T1 clause holds; no repair was needed.

## POST-FORMAT: (P3-T2, after the repository-wide format of P3-T1)

Timestamp: 2026-10-02T00-08. CMD-TOKEN-COUNT with TOKENS-PARTIAL re-run on TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs:

```
TOKEN [public async Task GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly()] = 1
TOKEN [public async Task GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly()] = 1
TOKEN [public async Task GetPressed_AfterSuppressedFaults_LaterSuccessfulPrimeCachesStateAndInvalidatesOnce()] = 1
TOKEN [public async Task GetPressed_WhenFailureKindChanges_LogsNewKindOnce()] = 1
TOKEN [public async Task GetPressed_WhenSpamFaultIsSuppressed_FirstTriageFaultIsStillLogged()] = 1
TOKEN [public async Task HandleToggleClickAsync_AfterSuppressedPrimeFault_StillLogsToggleFault()] = 1
TOKEN [public async Task GetPressed_WhenPrimeFaults_FirstReportStatesRepeatsAreNotLoggedAgain()] = 1
TOKEN [[TestMethod]] = 7
TOKEN [[TestClass]] = 0
TOKEN [public partial class EngineToggleStateCoordinatorTests] = 1
TOKEN [new Mock<] = 0
TOKEN [MockBehavior] = 0
TOKEN [private sealed class] = 0
TOKEN [private const string TriageEngine = "Triage";] = 1
TOKEN [var harness = new Harness();] = 7
TOKEN [// Arrange] = 7
TOKEN [// Act] = 7
TOKEN [// Assert] = 7
TOKEN [private static async Task<bool> PollAsync(Harness harness, string engineName, int polls)] = 1
TOKEN [for (var poll = 0; poll < polls; poll++)] = 1
TOKEN [pressed = harness.Coordinator.GetPressed(engineName);] = 1
TOKEN [await harness.Coordinator.GetPrimeTask(engineName);] = 1
TOKEN [await PollAsync(harness, SpamEngine, 5);] = 2
TOKEN [await PollAsync(harness, SpamEngine, 3);] = 1
TOKEN [await PollAsync(harness, SpamEngine, 4);] = 1
TOKEN [await PollAsync(harness, SpamEngine, 2);] = 2
TOKEN [await PollAsync(harness, TriageEngine, 1);] = 1
TOKEN [await PollAsync(harness, SpamEngine, 1);] = 1
TOKEN [.Be(1, "a repeated fault of one kind for one engine is reported once");] = 1
TOKEN [.BeSameAs(failure, "the first report carries the injected fault");] = 1
TOKEN [suppressing the report must not suppress the re-prime] = 1
TOKEN [repeated cancellations are one failure kind] = 1
TOKEN [the second identical fault is a suppressed repeat] = 1
TOKEN [each distinct failure kind is reported once] = 1
TOKEN [suppression is keyed by engine as well as by kind] = 1
TOKEN [one suppressed prime repeat, every toggle fault] = 1
TOKEN [the entry must state that repeats are suppressed] = 1
TOKEN [.Contain("not logged again"] = 1
TOKEN [.EndWith("Further failures of this kind for this engine are not logged again.");] = 1
TOKEN [Times.Exactly(5),] = 2
TOKEN [Times.Exactly(3),] = 1
TOKEN [Times.Exactly(4),] = 1
TOKEN [.BeAssignableTo<OperationCanceledException>(] = 1
TOKEN [Task.FromCanceled<bool>(new CancellationToken(true))] = 1
TOKEN [new IOException(] = 1
TOKEN [using System.IO;] = 1
TOKEN [using Moq;] = 1
TOKEN [.ThrowsAsync(toggleFailure)] = 1
TOKEN [.ContainSingle(] = 3
TOKEN [.HaveCount(2,] = 3
TOKEN [new[] { SpamToggleControlId },] = 1
TOKEN [harness.Invalidations.Should().BeEmpty(] = 2
TOKEN [harness.Notifications.Should().BeEmpty(] = 1
TOKEN [pressed.Should().BeFalse(] = 2
TOKEN [Regression tests for issue #948] = 1
TOKEN [Thread.Sleep] = 0
TOKEN [Task.Delay] = 0
TOKEN [SpinWait] = 0
TOKEN [while (] = 0
TOKEN [DateTime] = 0
TOKEN [Stopwatch] = 0
TOKEN [.Wait(] = 0
TOKEN [.Result] = 0
TOKEN [DoNotParallelize] = 0
TOKEN [[Timeout] = 0
TOKEN [GetTempPath] = 0
TOKEN [File.] = 0
TOKEN [TimeProvider] = 0
TOKEN [ManualResetEvent] = 0
FIRST-LINE [.Be(1, "a repeated fault of one kind for one engine is reported once");] = 53
FIRST-LINE [.BeSameAs(failure, "the first report carries the injected fault");] = 57
FIRST-LINE [Times.Exactly(5),] = 61
PARTIAL-FILE-LINES: 290
```

Every P1-T1 clause holds on the post-format tree.
