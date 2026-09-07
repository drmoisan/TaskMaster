# Regression Test File Created (P1-T3)

Timestamp: 2026-08-27T10-40
Task: [P1-T3]
Command: `Select-String -SimpleMatch -Pattern '[TestMethod]' -Path 'QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs'` and `Select-String -SimpleMatch -Pattern '[Timeout(GateTimeoutMs)]'` against the same path
EXIT_CODE: 0
Output Summary: The file exists at
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`. The
`[TestMethod]` search returns exactly 6 matches and the `[Timeout(GateTimeoutMs)]` search returns
exactly 6 matches. The file measures 337 lines. `Thread.Sleep` and `Task.Delay` each return 0
matches.

## Acceptance verification

| Check | Required | Observed |
| --- | --- | --- |
| `[TestMethod]` match count | exactly 6 | 6 |
| `[Timeout(GateTimeoutMs)]` match count | exactly 6 | 6 |
| File exists at the stated path | yes | yes |
| Line count (informational) | at or under 500 | 337 |
| `Thread.Sleep` matches | 0 | 0 |
| `Task.Delay` matches | 0 | 0 |

PrimaryAssertionDoc: `R1 is the primary deterministic regression assertion and R4 is the supporting probabilistic one. R1 reproduces the issue #230 clobber precondition with no concurrency at all and proves the clobber itself is unreachable, and the clobber rather than the scheduling is the actual #230 mechanism. R4 exercises two concurrent transactions, but under a broken implementation it fails only probabilistically, because nothing can force the second caller to reach its acquisition point while the first still holds the gate and there is no deterministic way to prove the second caller is currently blocked without a timed wait, which the repository's determinism rules forbid.`

That sentence group is quoted here rather than asserted with a line-oriented search because it is
prose that CSharpier may rewrap across lines, which would make such a search return zero matches
whatever the executor wrote. It is transcribed from the class-level XML doc comment of
`QfcItemController_UiThreadDispatcherFixtureTests`, with the `///` prefixes and the `<para>` tags
removed and the line wrapping joined; no word was changed.

## Tests delivered

| # | Test name |
| --- | --- |
| R1 | `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` |
| R2 | `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose` |
| R3 | `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent` |
| R4 | `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` |
| R5 | `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` |
| R6 | `Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException` |

All six are declared `public async Task`, carry `[TestMethod]` and `[Timeout(GateTimeoutMs)]` on
their own lines, and follow Arrange-Act-Assert with explicit section comments. The class hosts
`private const int GateTimeoutMs = 60000;`.

R1, R2, and R3 enter through `QfcItemControllerTestSupport.EnsureUiThreadDispatcher()` rather than
`UiThreadDispatcherFixture.EnsureDispatcher()` directly, which is required rather than stylistic:
the fail-before premise `P0-T14` and `P1-T4` record rests on those tests being unable to compile
against the base branch, which is true only of the wrapper, whose return type is `void` at `HEAD`.

## Determinism and library compliance

- Framework: MSTest (`Microsoft.VisualStudio.TestTools.UnitTesting`).
- Assertions: FluentAssertions only, each with a `because:` reason.
- Mocking: none required by these tests, so Moq is not referenced.
- Cross-thread coordination: `ManualResetEventSlim` in R4 and awaited `Task` completion throughout.
  No `Thread.Sleep`, no `Task.Delay`, no wall-clock wait, no temporary file.
- Live dispatchers come from `QfcItemControllerTestSupport.StartRunningDispatcher()` with
  `QfcItemControllerTestSupport.ShutdownDispatcher(...)` in `finally`.
- Every test that needs a known field value acquires a transaction first and captures `original` from
  `UiThreadDispatcherFixture.Current` **after** acquisition, so the observation is made under the
  gate.

## One correction made during this task

The class doc comment first drafted quoted the attribute literally as `[Timeout(GateTimeoutMs)]`
inside a `<c>` element. That made the `[Timeout(GateTimeoutMs)]` search return 7 matches rather than
the required 6. The sentence was reworded to "the 60-second MSTest timeout attribute", which carries
the same meaning and restores the count to exactly 6. No test attribute was removed.
