---
name: log4net-memoryappender-shared-per-type-across-parallel-classes
description: log4net binds one logger per TYPE, so a MemoryAppender attached in one test class captures events from concurrently-running tests in other classes; exact-count assertions on it are order-dependent
metadata:
  type: project
---

`log4net.LogManager.GetLogger(typeof(X))` returns ONE logger per type for the whole process. A
`MemoryAppender` attached to it in `[TestInitialize]` therefore captures events emitted by every
test that drives `X`, including tests in OTHER classes running in parallel (MSTest here runs at
`Workers: 24, Scope: ClassLevel`). `[TestCleanup]` detaching does not help: the contamination
happens during the test, not after it.

**Why:** `SegmentActivate_CrossStoreAncestor_LeavesSelectionUnchangedAndDiagnoses` asserted
`RenderedMessages().Should().ContainSingle(m => m.Contains("rejected"))` and failed intermittently
with "but 2 such items were found". It passed on the first full-suite run and failed on the next
with no code change between them. The extra event came from another router test class
(`BreadcrumbBridgeRouterIssue439Tests`, `BreadcrumbBridgeRouterTests`) emitting its own rejection
diagnostic in the same window. `[DoNotParallelize]` on the asserting class alone would not fix it —
every writer class would have to be marked too, and a future writer class would silently reintroduce
the flake.

**How to apply:** when a spec requires proving "a diagnostic is emitted", assert EXISTENCE, never a
count:

```csharp
messages.Should().Contain(m => m.Contains(fragment));
messages.Where(m => m.Contains(fragment))
        .Should().OnlyContain(m => !m.Contains("@"));
```

Concurrency can only ADD events, never remove them, so the existence claim is deterministic. Pair it
with a behavioural assertion in the same test (here: the selection is unchanged) — that is what
proves the diagnostic came from THIS instance, since had it not rejected, the selection would have
changed. Also scope any "message must not leak X" assertion to the matching subset: an unfiltered
`NotContain` can fail on a concurrent test's unrelated log line that legitimately contains a path.

Note this also means `QuickFiler.Test` needed a log4net `<Reference>` plus a `packages.config` pin
added before the appender pattern (established in `TaskMaster.Test/AppGlobals/AppEventsTests.Helpers.cs`)
could be used at all — non-SDK `ProjectReference` does not flow package references to the compiler.
See [[legacy-csproj-no-transitive-compile-refs]].
