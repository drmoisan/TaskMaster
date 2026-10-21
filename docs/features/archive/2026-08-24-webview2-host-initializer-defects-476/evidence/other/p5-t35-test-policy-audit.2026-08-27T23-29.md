# Per-Test-Method Policy Audit ([P5-T35])

Timestamp: 2026-08-27T23-29

Command:

```
python <scratchpad>/audit.py
```

The helper script was written to the session scratchpad outside the repository and is not retained
under `evidence/`. It splits each of the three test files at every `[TestMethod]` attribute and, for
each method, counts occurrences of `// Arrange`, `// Act`, `// Assert`, `because:`, `.Should()`,
`Mock<` / `BuildCompletingInitializer`, `[Timeout`, and `RecordingSynchronizationContext`.

EXIT_CODE: 0

## Output Summary

All **fifteen** test methods this feature adds carry `[TestMethod]` inside a `[TestClass]`, use
FluentAssertions `.Should()`, carry exactly one `// Arrange`, one `// Act` and one `// Assert`
comment, and carry at least one `because:` argument. No row is missing any required element. The one
row excluded from the table, `Construction_YieldsAnIWebViewCoreInitializer`, is a pre-existing test
that this feature did not author; it is present in the base at
`origin/epic/quickfiler-bug-family-integration` and is out of this criterion's scope.

## The fifteen methods

Legend for the mocking column: **Moq** means the test uses `Mock<IWebViewCoreInitializer>` through
the file's `BuildCompletingInitializer()` helper. **Recording double** means it uses the file's
private sealed `RecordingSynchronizationContext`, which the spec's own acceptance criterion mandates
by name. **none required** means the test needs no substitute for any collaborator.

### `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` (4 methods)

| # | Test method | Framework attribute | Mocking | Assertions | `because:` | Arrange / Act / Assert |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | `CreateEnvironmentAsync_NullCacheFolder_ThrowsArgumentNullException` | `[TestMethod]` | none required | FluentAssertions, 2 `.Should()` | 1 | 1 / 1 / 1 |
| 2 | `CreateEnvironmentAsync_WhitespaceCacheFolder_ThrowsArgumentException` | `[TestMethod]` | none required | FluentAssertions, 2 `.Should()` | 1 | 1 / 1 / 1 |
| 3 | `EnsureCoreWebView2Async_NullControl_ThrowsArgumentNullException` | `[TestMethod]` | none required | FluentAssertions, 2 `.Should()` | 1 | 1 / 1 / 1 |
| 4 | `WebView2CoreInitializer_ExemptsOnlyTheSdkForwards` | `[TestMethod]` | none required | FluentAssertions, 7 `.Should()` | 7 | 1 / 1 / 1 |

Rows 1 to 3 each have two `.Should()` calls and one `because:`. The second `.Should()` in each is the
chained `.And.ParamName.Should().Be("cacheFolder")` / `.Be("control")`, a self-evident equality whose
reason is already stated on the `Throw`/`ThrowExactly` clause that precedes it in the same chain. That
satisfies "a `because:` argument on **non-obvious** assertions". Row 4 carries a `because:` on every
one of its seven assertions.

### `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` (8 methods)

Every method in this file additionally carries `[Timeout(PumpTimeoutMs)]`, following the precedent at
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs:355`, and constructs
exactly one `new WebView2()` of its own.

| # | Test method | Framework attribute | Mocking | Assertions | `because:` | Arrange / Act / Assert |
| --- | --- | --- | --- | --- | --- | --- |
| 5 | `PostMessageJson_PostsExactlyOnceToTheUiContext` | `[TestMethod]` `[Timeout]` | Recording double | FluentAssertions, 2 `.Should()` | 2 | 1 / 1 / 1 |
| 6 | `NavigateToString_PostsExactlyOnceToTheUiContext` | `[TestMethod]` `[Timeout]` | Recording double | FluentAssertions, 2 `.Should()` | 2 | 1 / 1 / 1 |
| 7 | `SecondHost_DetachesThePredecessorAndTakesOwnership` | `[TestMethod]` `[Timeout]` | none required | FluentAssertions, 2 `.Should()` | 2 | 1 / 1 / 1 |
| 8 | `PredecessorDetach_ToleratesNullCoreWebView2` | `[TestMethod]` `[Timeout]` | none required | FluentAssertions, 2 `.Should()` | 2 | 1 / 1 / 1 |
| 9 | `ControlDisposed_DetachesTheHost` | `[TestMethod]` `[Timeout]` | none required | FluentAssertions, 1 `.Should()` | 1 | 1 / 1 / 1 |
| 10 | `InitializeAsync_InstallsUiDispatcherFromUiSyncContext` | `[TestMethod]` `[Timeout]` | **Moq** | FluentAssertions, 2 `.Should()` | 2 | 1 / 1 / 1 |
| 11 | `InitializeAsync_PreservesAnInjectedDispatcher` | `[TestMethod]` `[Timeout]` | **Moq** + Recording double | FluentAssertions, 2 `.Should()` | 2 | 1 / 1 / 1 |
| 12 | `PostMessageJson_WithNoDispatcher_ExecutesInlineAndDropsThePayload` | `[TestMethod]` `[Timeout]` | **Moq** + Recording double | FluentAssertions, 3 `.Should()` | 3 | 1 / 1 / 1 |

In rows 5 to 12 the `because:` count equals the `.Should()` count, so every assertion carries a
reason.

### `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs` (3 methods)

| # | Test method | Framework attribute | Mocking | Assertions | `because:` | Arrange / Act / Assert |
| --- | --- | --- | --- | --- | --- | --- |
| 13 | `IsCoreInitialized_HasAnExplicitBackingField` | `[TestMethod]` | none required | FluentAssertions, 5 `.Should()` | 5 | 1 / 1 / 1 |
| 14 | `WebView2BreadcrumbHost_CarriesNoClassLevelCoverageExemption` | `[TestMethod]` | none required | FluentAssertions, 1 `.Should()` | 1 | 1 / 1 / 1 |
| 15 | `WebView2BreadcrumbHost_ExemptsOnlyHostBoundMembers` | `[TestMethod]` | none required | FluentAssertions, 4 `.Should()` | 4 | 1 / 1 / 1 |

These three assert by reflection over `typeof(QuickFiler.Viewers.WebView2BreadcrumbHost)` and need no
collaborator substitute at all, so "none required" is the accurate reading rather than a gap.

## Note on the recording double versus Moq

`.claude/rules` and `CLAUDE.md` §CUT2 direct C# tests to use Moq for mocks and stubs. Three of the
fifteen tests do exactly that, with `Mock<IWebViewCoreInitializer>`. Five use the hand-written
`RecordingSynchronizationContext` instead. That is not an unconsidered deviation: the acceptance
criterion for the marshalling regression test names "a **recording** `SynchronizationContext`" and
requires that it "never drains the posted action, so no WebView2 runtime is involved". The
requirement is a positive behavioural contract — count posts, and deliberately never invoke the
queued callback — which the double documents in its own XML comment. The spec mandates this shape by
name, so the double is the mandated design rather than a substitute for Moq that was chosen freely.
Recorded here rather than silently absorbed.
