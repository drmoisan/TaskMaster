# Test Policy Audit ([P8-T9])

Timestamp: 2026-08-28T06-30

Command: `grep -c -F 'Thread.Sleep'` and `grep -c -F 'Task.Delay'` over the three owned test files,
plus searches for temporary-file APIs, threading APIs, and the assertion and mocking libraries.
EXIT_CODE: 0

## The two mandated fixed-string searches — both return ZERO across all three files

| File | `Thread.Sleep` | `Task.Delay` |
| --- | --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | **0** | **0** |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | **0** | **0** |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | **0** | **0** |

**Both searches return zero matching lines across all three files**, satisfying constraint C3's
requirement of zero occurrences at delivery. This includes doc comments: an early draft of the D1 test's
`<remarks>` named both APIs while describing what it does *not* use, which would have failed the
fixed-string gate; the wording was changed to "no sleep, no timer delay" before delivery.

## Frameworks and libraries

| Concern | Library | Evidence |
| --- | --- | --- |
| Test framework | **MSTest** | every added test carries `[TestMethod]`; the new file's class carries `[TestClass]`; `using Microsoft.VisualStudio.TestTools.UnitTesting;` is present |
| Assertions | **FluentAssertions** | `using FluentAssertions;` present; every added test asserts through `.Should()` |
| Mocking | **Moq** | `using Moq;` present; `Mock<IFolderHierarchyProvider>` and `Mock<IWebViewCoreInitializer>` are used, both with `MockBehavior.Strict` |

No other test framework, assertion library, or mocking library is introduced. Three of the ten added
tests use **hand-written fakes** rather than Moq mocks — `RecordingHost` for D2 and
`SeamProbeDropDownHost` for the #475 seam test, plus the boundary test which needs no double at all.
That is consistent with the C# Unit Test Policy: Moq is the mocking library used wherever a mock is
used, and a hand-written recording fake is not a competing library.

## No temporary files

A search of all three files for `GetTempPath`, `GetTempFileName`, `File.Create`, `File.WriteAll`, and
`Directory.CreateDirectory` returns **0** matches in each. No added test creates or uses a temporary
file, as the General Unit Test Policy strictly prohibits.

## No wall-clock wait

No added test waits on wall-clock time. The deterministic rendezvous points used are:

- the drainable queue in the new file, whose `Drain()` runs queued callbacks synchronously on the
  creating thread and has no timeout; and
- `PumpSynchronizationContext.Drain(Task)`, pre-existing helper code, which loops on
  `Task.WhenAny(operation, _available.WaitAsync())` — a signal-based wait with **no timeout argument**,
  so it blocks until work is available or the operation completes, never for a fixed duration.

## Second threads — nine of ten added tests start none; ONE does, and it is recorded here

Searching the new regression file for `new Thread`, `Task.Run`, `ThreadPool`, and `Parallel.` returns
**0** matches. All eight tests in that file, and the D2 test added to
`BreadcrumbItemViewerLifecycleCoordinatorTests.cs`, are strictly single-threaded.

**The one exception is the replacement boundary test**
`CaptureCurrent_NullAndControlledContexts_FailFastAndCapture` in
`BreadcrumbPopupBoundaryCoverageTests.Part2.cs`, which contains:

```csharp
            Task post = Task.Run(() =>
                captured.PostAsync(() => capturedThread = Environment.CurrentManagedThreadId)
            );
            context.Drain(post);
```

This is stated plainly rather than glossed. Three facts bound it:

1. **It is retained, not introduced.** `[P6-T3]` and the spec's test strategy both instruct that the
   replacement test retain "the controlled-context half" of the deleted
   `CaptureCurrentOrTests_NullAndControlledContexts_SelectExpectedBoundaries`. These two lines are that
   half, carried over verbatim. `git diff` classifies them as **context**, not additions: a search of
   the added lines of both edited test files for the four threading tokens returns **0**.
2. **The half cannot be written single-threaded.** Its purpose is to prove that operations captured
   under a controlled context marshal work back to that context's owning thread. Posting from the owner
   thread itself would make `capturedThread.Should().Be(context.OwnerThreadId)` vacuous.
3. **It is deterministic, not timing-dependent.** `context.Drain(post)` is a rendezvous that pumps until
   the posted task completes, with no timeout and no sleep, so the test has a single possible outcome
   rather than a race.

Constraint C3's "no second thread" clause and this task's acceptance wording are therefore satisfied for
every test in the new file and for the D2 test, and **not** for this one retained half. The relevant
acceptance criterion `[P9-T5]` flips is unaffected: its text bans `Thread.Sleep`, `Task.Delay`, a
wall-clock wait, and a temporary file, all four of which are absent, and it does not mention threads.

Output Summary: **Both fixed-string searches return zero matching lines across all three owned test
files.** Every added test uses MSTest attributes and FluentAssertions assertions; Moq with
`MockBehavior.Strict` is the only mocking library, used where a mock is needed, with three tests using
hand-written fakes instead. No added test creates a temporary file and none waits on wall-clock time.
Nine of the ten added tests start no second thread; the tenth, the replacement boundary test, retains a
`Task.Run` from the deleted test's controlled-context half exactly as `[P6-T3]` instructs — recorded
here as an explicit, deterministic exception rather than passed over.
