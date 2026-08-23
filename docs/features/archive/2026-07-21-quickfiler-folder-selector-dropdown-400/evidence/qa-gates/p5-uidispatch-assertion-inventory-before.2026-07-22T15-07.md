# P5-T174 — Pre-correction assertion inventory of `BreadcrumbUiThreadDispatchTests.cs` (read-only)

Timestamp: 2026-07-22T15-07Z

Command: `cd "C:/Users/DanMoisan/repos/TaskMaster-wt/2026-07-21T10-25" && sha256sum QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs coverage.config scripts/vscode/TaskMaster.cli.runsettings && wc -l QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs && grep -nE "Thread\.Sleep|Task\.Delay|SpinWait|Stopwatch|DateTime\.|Timeout|WaitOne|DoNotParallelize|\[Ignore|TestCategory|for *\(|while *\(" QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs && grep -n "BreadcrumbUiThreadDispatch" coverage.config scripts/vscode/TaskMaster.cli.runsettings TaskMaster.runsettings`

EXIT_CODE: 0

This artifact is the sole authorized **before** side of the P5-T184 anti-masking comparison. It was written before any
correction batch began. No file was modified while producing it.

## File identity (pre-correction)

| Item | Value |
|---|---|
| Path | `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs` |
| SHA-256 | `e4bd60150636a83ce977681249e03c63a2fc7ca96c32c5f8ef5bbb760926e62e` |
| Physical lines | 480 |
| `coverage.config` SHA-256 | `b9cd80356c6bdbe03807a0b8cb106ae03d24efbdbb2515097fbf003099050943` |
| `scripts/vscode/TaskMaster.cli.runsettings` SHA-256 | `98ef03a8d3b0ebb2ed7a765e3b5e1b58e774d20202df2f294c03a7260b9cef57` |

## The nine `[TestMethod]` names

1. `SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext` (line 23)
2. `InboundWorkerMessage_SchedulesEveryPostAndCallbackOnOwningContext` (line 66)
3. `DispatcherSchedulingFailure_IsReportedThroughObservableErrorSink` (line 108)
4. `DispatcherActionFailure_IsReportedExactlyOnce` (line 160)
5. `DispatchValue_AmbientOwningContext_StillSchedulesBeforeControlAccess` (line 187)
6. `DispatchValue_NestedSynchronousDispatch_ExecutesInlineWithoutAnotherPost` (line 218)
7. `DispatchValue_SchedulingFailure_ReportsOnceAndFaultsReturnedTask` (line 255)
8. `ProductionCaptureWithoutUiContext_FailsFast` (line 277)
9. `InboundCurrentDispatchFailure_IsObservedWithoutEscapingEventBoundary` (line 311)

## Per-method assertion expressions with source lines

### 1. `SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext`

| Lines | Assertion expression |
|---|---|
| 55-57 | `context.PostCount.Should().BeGreaterThan(0, "worker completion must cross the captured UI dispatcher")` |
| 58 | `messenger.Posted.Should().BeEmpty("the owning context has not run queued work yet")` |
| 60 | `messenger.Posted.Should().NotBeEmpty()` |
| 61 | `messenger.PostContexts.Should().OnlyContain(value => ReferenceEquals(value, context))` |
| 62 | `provider.VerifyAll()` |

Non-assertion observation gate: line 52 `await Task.WhenAny(population, context.FirstPost).ConfigureAwait(false)`.
Drain: line 59 `await context.DrainUntilAsync(population).ConfigureAwait(false)`.

### 2. `InboundWorkerMessage_SchedulesEveryPostAndCallbackOnOwningContext`

| Lines | Assertion expression |
|---|---|
| 96-98 | `context.PostCount.Should().BeGreaterThan(0, "worker-originated posts and callbacks require UI scheduling")` |
| 99 | `messenger.Posted.Should().BeEmpty()` |
| 100 | `callbackContexts.Should().BeEmpty()` |
| 102 | `messenger.Posted.Should().HaveCount(2)` |
| 103 | `messenger.PostContexts.Should().OnlyContain(value => ReferenceEquals(value, context))` |
| 104 | `callbackContexts.Should().ContainSingle().Which.Should().BeSameAs(context)` |

Non-assertion observation gate: line 93 `await Task.WhenAny(dispatch, context.FirstPost).ConfigureAwait(false)`.
Drain: line 101 `await context.DrainUntilAsync(dispatch).ConfigureAwait(false)`.

### 3. `DispatcherSchedulingFailure_IsReportedThroughObservableErrorSink`

| Lines | Assertion expression |
|---|---|
| 114-116 | `dispatcherType.Should().NotBeNull("dispatch failures require the planned host-neutral observable seam")` |
| 128-132 | `constructor.Should().NotBeNull("the dispatcher must accept its owning context and observable error sink")` |
| 142 | `dispatch.Should().NotBeNull("the dispatcher requires one focused Action boundary")` |
| 155 | `context.PostAttempts.Should().Be(1)` |
| 156 | `observed.Should().ContainSingle().Which.Should().BeSameAs(failure)` |

### 4. `DispatcherActionFailure_IsReportedExactlyOnce`

| Lines | Assertion expression |
|---|---|
| 183 | `observed.Should().ContainSingle().Which.Should().BeSameAs(failure)` |

### 5. `DispatchValue_AmbientOwningContext_StillSchedulesBeforeControlAccess`

| Lines | Assertion expression |
|---|---|
| 209 | `dispatchNull.Should().Throw<ArgumentNullException>().WithParameterName("action")` |
| 210 | `dispatch.IsCompleted.Should().BeFalse("ambient context alone is not an inline proof")` |
| 211 | `context.PostCount.Should().Be(1)` |
| 213 | `(await dispatch.ConfigureAwait(false)).Should().Be(42)` |
| 214 | `observed.Should().BeEmpty()` |

### 6. `DispatchValue_NestedSynchronousDispatch_ExecutesInlineWithoutAnotherPost`

| Lines | Assertion expression |
|---|---|
| 245 | `nested.Should().NotBeNull()` |
| 246 | `nested.Status.Should().Be(TaskStatus.RanToCompletion)` |
| 247 | `nested.GetAwaiter().GetResult().Should().Be(17)` |
| 249 | `observeFailure.Should().Throw<InvalidOperationException>().Which.Should().Be(failure)` |
| 250 | `context.PostCount.Should().Be(0)` |
| 251 | `observed.Should().ContainSingle().Which.Should().BeSameAs(failure)` |

### 7. `DispatchValue_SchedulingFailure_ReportsOnceAndFaultsReturnedTask`

| Lines | Assertion expression |
|---|---|
| 268-271 | `await observeFailure.Should().ThrowAsync<InvalidOperationException>().Where(value => ReferenceEquals(value, failure))` |
| 272 | `context.PostAttempts.Should().Be(1)` |
| 273 | `observed.Should().ContainSingle().Which.Should().BeSameAs(failure)` |

### 8. `ProductionCaptureWithoutUiContext_FailsFast`

| Lines | Assertion expression |
|---|---|
| 288-291 | `captureWithoutUiContext.Should().Throw<InvalidOperationException>().WithMessage("*owning UI synchronization context*")` |
| 302-307 | `dispatchWithoutContext.Should().ThrowAsync<InvalidOperationException>().WithMessage("*cannot marshal cross-thread UI work*").GetAwaiter().GetResult()` |

### 9. `InboundCurrentDispatchFailure_IsObservedWithoutEscapingEventBoundary`

| Lines | Assertion expression |
|---|---|
| 329 | `raiseInvalidCurrentMessage.Should().NotThrow()` |
| 331 | `coordinator.LastDispatch.Status.Should().Be(TaskStatus.RanToCompletion)` |
| 332 | `observed.Should().ContainSingle()` |
| 333 | `observed.Single().Should().BeOfType<ArgumentNullException>()` |

Total: 9 `[TestMethod]` declarations, 33 assertion expressions.

## Prohibited-construct absence (pre-correction state)

The pattern scan
`grep -nE "Thread\.Sleep|Task\.Delay|SpinWait|Stopwatch|DateTime\.|Timeout|WaitOne|DoNotParallelize|\[Ignore|TestCategory|for *\(|while *\("`
returned exactly two matches, both inside the `RecordingSynchronizationContext` drain harness:

- line 387 `while (!operation.IsCompleted)` — drain loop bounded by the observed operation's completion.
- line 395 `while (DrainOne()) { }` — final queue-drain loop bounded by queue emptiness.

Neither is a retry loop and neither carries a timing threshold. Confirmed absent from the file:

- `Thread.Sleep` — absent.
- `Task.Delay` — absent.
- Wall-clock waits (`Stopwatch`, `DateTime.*`, `Timeout`, `WaitOne` with a timeout) — absent.
- Retry loops — absent.
- Timing thresholds — absent.
- `[DoNotParallelize]` — absent.
- `[Ignore]` — absent.
- Category-based skip (`[TestCategory]`) — absent.

## Filter / exclusion absence

`grep -n "BreadcrumbUiThreadDispatch" coverage.config scripts/vscode/TaskMaster.cli.runsettings TaskMaster.runsettings`
returned no match. There is no coverage exclusion and no runsettings exclusion naming this class. The class appears only
as an **inclusive** term in the P5-T171 / P5-T183 seventeen-class `TestCaseFilter`, which is not a narrowing filter.

Output Summary: Pre-correction inventory captured for `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs`,
SHA-256 `e4bd6015...26e62e`, 480 physical lines (exactly at its 480-line cap). Nine `[TestMethod]` declarations and 33
assertion expressions recorded with source lines. No `Thread.Sleep`, `Task.Delay`, wall-clock wait, retry loop, timing
threshold, `[DoNotParallelize]`, `[Ignore]`, or category skip is present; the only two loop constructs are the
completion-bounded drain loops at lines 387 and 395. No coverage exclusion or runsettings exclusion names this class;
`coverage.config` SHA-256 `b9cd8035...050943` and `scripts/vscode/TaskMaster.cli.runsettings` SHA-256
`98ef03a8...b9cef57` recorded for the P5-T184 before/after hash comparison. EXIT_CODE: 0.
