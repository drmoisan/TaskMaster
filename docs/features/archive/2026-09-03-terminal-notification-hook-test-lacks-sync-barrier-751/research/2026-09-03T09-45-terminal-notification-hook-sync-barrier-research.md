# Research — Terminal notification hook test lacks a synchronization barrier (Issue #751)

- Timestamp: 2026-09-03T09-45
- Worktree: `<repo-root>` on branch `bug/terminal-notification-hook-test-lacks-sync-barrier-751` (from `origin/main` at `f8414ee9`)
- Scope: research only. No source or configuration file was modified.
- Mode: preparation-mode delegation from the orchestrator.

---

## 0. Correction to the issue text (verified)

The issue body states the test lives in `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs`.
That is correct for the **test method**, but the **fixture** it names (`ControlledUiDispatcher`,
`ControlledAppOlObjects`, `CreateSut`, `StartWorkerAsync`) lives in a different file. The class
`AppOlObjectsFolderTreeServiceLifecycleTests` is declared `partial` across two files:

| Member | File | Lines |
|---|---|---|
| `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault` (the failing test) | `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` | 102-120 |
| `CreateSut(ControlledUiDispatcher, bool, ...)` | `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` | 213-234 |
| `StartWorkerAsync` | `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` | 249-262 |
| `ControlledAppOlObjects` (holds `InvokedTerminalHookCount`) | `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` | 129-217 |
| `ControlledUiDispatcher` | `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` | 219-306 |
| `ControlledDispatchOperation` (owns `ReleaseAsync`) | `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` | 390-447 |
| `Signal<T>()` helper | `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` | 449-450 |

Production under test: `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs` (410 lines,
`partial class AppOlObjects`). There is no type named `AppOlObjectsFolderTreeService`; the service is
composed by the `AppOlObjects.FolderTreeService` property getter
(`TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs:20-168`).

Any spec or plan that repeats the issue's single-file framing will misdirect the edit. Both files must
be named.

---

## 1. Current state — what the test actually does

```
TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs:102-120
```

1. `:104-105` — builds `ControlledUiDispatcher(DispatchMode.Pending, fault: fault)`.
2. `:106` — `CreateSut(dispatcher, throwFromTerminalHook: true)`.
3. `:107` — `var run = await StartWorkerAsync(sut, dispatcher);`
4. `:110` — `dispatcher.Complete(run.Operation, DispatchMode.Faulted);`
5. `:111` — `(await GetExceptionAsync(run.Worker)).Should().BeSameAs(fault);`
6. `:112` — `await run.Operation.ReleaseAsync();`
7. `:113` — `sut.LoadCount.Should().Be(0);`
8. `:114` — `sut.InvokedTerminalHookCount.Should().Be(1);`  ← the flaky assertion
9. `:118` — `CleanupAsync(...)` in `finally`.

`StartWorkerAsync` (`AppOlObjectsFolderTreeServiceTests.cs:249-262`) returns a 3-tuple
`(Worker, Operation, Terminal)`. `Terminal` is captured at `:257` (`var terminal = sut.NextTerminal;`)
**before** the worker starts at `:258`, and returned at `:261`.

**The failing test is the only test in the class that never awaits `run.Terminal`.** Every sibling that
observes terminal-hook side effects does await it:

- `AppOlObjectsFolderTreeServiceLifecycleTests.cs:38` — `await GetExceptionAsync(await run.Terminal)`
- `AppOlObjectsFolderTreeServiceTests.cs:73` — `await GetExceptionAsync(await firstRun.Terminal)`
- `AppOlObjectsFolderTreeServiceTests.cs:143` — `var terminal = await run.Terminal;`
- `AppOlObjectsFolderTreeServiceTests.cs:308` — `await GetExceptionAsync(await run.Terminal)`
- `AppOlObjectsFolderTreeServiceTests.cs:341` — `await GetExceptionAsync(await staleRun.Terminal)`
- `AppOlObjectsFolderTreeServiceLifecycleTests.Coverage.cs:117` — `await GetExceptionAsync(await run.Terminal)`

That is six sibling call sites that use the barrier and one (line 114) that does not.

---

## 2. The exact race mechanism (found, not hypothesised)

### 2.1 The counter is written on a thread the test never joins

The hook override is:

```csharp
// TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs:196-205
protected internal override void OnFolderTreeServiceInitializationTerminal(
    TreeTask terminalInitialization
)
{
    InvokedTerminalHookCount++;                                        // :200
    var signal = Interlocked.Exchange(ref _terminalSignal, Signal<TreeTask>());  // :201
    signal.TrySetResult(terminalInitialization);                       // :202
    if (_throwFromTerminalHook)
        throw new InvalidOperationException("Controlled terminal hook failure.");  // :204
}
```

The production caller reaches that override through
`AppOlObjects.FolderTreeService.cs:328-337` (`NotifyFolderTreeServiceInitializationTerminal`, whose
`catch (Exception) { }` at `:336` is precisely what this test is verifying).

### 2.2 The terminal completion is published *before* the hook runs

For the fault path exercised by this test, the relevant production sequence on the thread that observes
the dispatch fault is:

```
AppOlObjects.FolderTreeService.cs
:275  ObserveFolderTreeServiceDispatchTerminal(initialization, dispatchTask)
:288  dispatchTask.GetAwaiter().GetResult()          -> throws `fault`
:298  catch (Exception) when (terminalStatus == TaskStatus.Faulted)
:300  CompleteFolderTreeServiceCompositionFailure(initialization, exception)
:257      lock (_folderTreeServiceGate)
:261          completed = initialization.TrySetException(exception);   <-- WORKER UNBLOCKS HERE
:262-266      _folderTreeServiceInitialization = null; ... (state reset)
:267      }  (lock released)
:269  if (completed)
:271      NotifyFolderTreeServiceInitializationTerminal(initialization.Task);
:334          OnFolderTreeServiceInitializationTerminal(terminalInitialization);
                 -> fixture :200  InvokedTerminalHookCount++
```

The worker task the test awaits is blocked at `AppOlObjects.FolderTreeService.cs:166`
(`return initialization.Task.GetAwaiter().GetResult();`). It is released by `TrySetException` at `:261`,
which is **ten source lines and one lock-release earlier** than the counter increment at fixture `:200`.
The notify is deliberately placed outside `_folderTreeServiceGate` (`:267` closes the lock, `:269-272`
notifies) so that overridable user code never runs under the composition lock. That design choice is
what creates the gap.

Therefore `await run.Worker` at test `:111` establishes **no** happens-before relationship with the
counter increment. The two proceed concurrently on different threads from `:261` onward.

### 2.3 `ReleaseAsync()` at line 112 is a no-op on this path and cannot serve as a barrier

`ControlledDispatchOperation.ReleaseAsync()`
(`AppOlObjectsFolderTreeServiceLifecycleTests.cs:418-433`) has `_releaseBackend == null` for a plain
`ControlledUiDispatcher` (the backend is only supplied by `QueuedStaDispatcher`, `:317-324`), so it calls
`Execute()` (`:435-446`) synchronously. `Execute` invokes the captured action, which is the closure
created at `AppOlObjects.FolderTreeService.cs:127-129`, i.e.
`CompleteFolderTreeServiceComposition(initialization, dispatcher)`.

That method returns immediately at `AppOlObjects.FolderTreeService.cs:182` because the guard at
`:177-183` finds `!ReferenceEquals(initialization, _folderTreeServiceInitialization)` — the field was
already nulled at `:262`. Then `_completion.TrySetResult(true)` (fixture `:440`) fails because the
completion was already faulted at `:413-415` by `dispatcher.Complete(...)` from test `:110`.

**`await run.Operation.ReleaseAsync()` therefore does nothing at all on this path and is not, and cannot
be, a barrier for the terminal hook.** The issue's phrase "immediately after
`await run.Operation.ReleaseAsync()`" is accurate as a description of position but misleading as a
description of what the await accomplishes.

### 2.4 Why it is intermittent rather than always red

There are two interleavings, and only one of them races. `Capture(action)`
(`AppOlObjectsFolderTreeServiceLifecycleTests.cs:296-305`) publishes the operation to
`NextCallbackCaptured` at the **start** of `InvokeAsync` (`:264`), before `InvokeAsync` returns at `:269`
and long before the production code registers its continuation. So `StartWorkerAsync` can return, and the
test can call `dispatcher.Complete(...)` at `:110`, while the worker thread is still between
`AppOlObjects.FolderTreeService.cs:127` and `:159`.

- **Interleaving (b) — no race.** The fault lands before the worker reaches
  `AppOlObjects.FolderTreeService.cs:159`. Then `dispatchTask.IsCompleted` is true and
  `ObserveFolderTreeServiceDispatchTerminal` is invoked **inline on the worker thread** at `:161`. The
  hook increment happens on the worker thread before it reaches `:166`, so the counter is already 1 by the
  time `run.Worker` completes. The test passes deterministically.
- **Interleaving (a) — race.** The fault lands after `:159` evaluated false. Only the `ContinueWith`
  registered at `:148-157` fires. Because `ControlledDispatchOperation._completion` is created by
  `Signal<T>()` with `TaskCreationOptions.RunContinuationsAsynchronously`
  (`AppOlObjectsFolderTreeServiceLifecycleTests.cs:395, 449-450`), the `TaskContinuationOptions.ExecuteSynchronously`
  at `:155` is overridden and the continuation is **queued to `TaskScheduler.Default`**, i.e. a thread-pool
  thread distinct from both the test thread and the worker thread. That thread races the test thread from
  `:261` onward, as traced in §2.2.

This exactly matches the observed symptom: passes on most runs, failed once on PR #746 under
`/EnableCodeCoverage`, then passed on re-run of identical code. Coverage instrumentation inserts a probe
write per basic block, which lengthens the notifying thread's path from `:261` to fixture `:200` and makes
a preemption inside that window more likely, without changing any semantics.

### 2.5 A second, independent defect in the same assertion: no memory barrier

`InvokedTerminalHookCount` is a plain non-volatile `int` field
(`AppOlObjectsFolderTreeServiceLifecycleTests.cs:158-159`), incremented with a non-atomic `++` at `:200`
on one thread and read with a plain field read at `AppOlObjectsFolderTreeServiceTests.cs:114` on another.

The sibling counter in the same fixture does it correctly:
`private int _loadCount;` (`:137`), `internal int LoadCount => Volatile.Read(ref _loadCount);` (`:160`),
`Interlocked.Increment(ref _loadCount)` (`:186`). Other cross-thread counters in the same file also use
`Volatile.Read` (`:312`, `:352`, `:357`, `:369`) and `Interlocked.Increment` (`:463`).
`InvokedTerminalHookCount` is the sole cross-thread counter in this fixture that is neither.

This is a latent second cause. Fixing only the ordering (§4) also fixes the visibility problem, because the
TCS completion at `:202` is a release and the awaiting resumption is an acquire, and `:201`'s
`Interlocked.Exchange` is a full fence sequenced between `:200` and `:202`. Hardening the counter is still
recommended as defence in depth (§4.3).

---

## 3. Test-only or production-reachable? — explicit answer

**Test-only.** The ordering it depends on is real production behaviour, but nothing in production observes
it, and the behaviour is intentional.

Evidence:

1. **The hook has no production override.** `OnFolderTreeServiceInitializationTerminal` is declared
   `protected internal virtual` with an empty body at
   `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs:352-354`. The only `override`s in the entire
   repository are in test code: `AppOlObjectsFolderTreeServiceLifecycleTests.cs:196` and
   `AppOlObjectsFolderTreeServiceLifecycleTests.Coverage.cs:173` (plus a `base.` forwarder for coverage at
   `Coverage.cs:145`). Verified by repo-wide grep restricted to `*.cs`.
2. **`AppOlObjects` has no production subclass at all.** Repo-wide grep for `: AppOlObjects` over `*.cs`
   returns eight matches, all under `TaskMaster.Test/AppGlobals/`
   (`AppOlObjectsTests.cs:326,346`; `AppOlObjectsFolderTreeServiceTests.cs:484`;
   `AppOlObjectsFolderTreeServiceLifecycleTests.cs:129`;
   `AppOlObjectsFolderTreeServiceLifecycleTests.Coverage.cs:128,148,178`;
   `AppOlObjectsCoverageTests.cs:212`). There is no production consumer of the extension point.
3. **The notify-after-publish ordering is deliberate.** All five production notify sites place the call
   *after* the terminal `TrySet*` and *outside* `_folderTreeServiceGate` (see §5 enumeration). Moving the
   notify inside the lock, or before the `TrySet*`, would run overridable code under the composition lock
   and would change the containment semantics that `NotifyFolderTreeServiceInitializationTerminal`'s
   `catch (Exception) { }` at `:336` exists to provide — which is the very behaviour this test asserts.
4. **The real dispatcher does not change the analysis.** `WpfUiDispatcher.InvokeAsync(Action)` forwards to
   `Dispatcher.InvokeAsync(action).Task` (`UtilitiesCS/Threading/WpfUiDispatcher.cs:43`). A real WPF
   `DispatcherOperation.Task` also completes on the dispatcher thread and resumes production's continuation
   on `TaskScheduler.Default` per `AppOlObjects.FolderTreeService.cs:156`, so production has the same
   thread hand-off. It simply has no observer that could notice.
5. **`IUiDispatcher` makes no promise the test relies on.** `UtilitiesCS/Threading/IUiDispatcher.cs:20-21`
   documents `InvokeAsync` as "Asynchronously executes `action` on the UI thread" and says nothing about
   any downstream hook. There is no contract for the test to be testing against.

**Conclusion for scope:** do not modify `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs`. The
defect is that the test asserts an unsynchronised observation of a documented-as-unordered side effect.
The fix belongs entirely in `TaskMaster.Test`.

Counter-check considered and rejected: "make production invoke the hook before publishing the terminal
result." Rejected because it inverts the meaning of "terminal" (the hook receives the already-terminal
`Task<IOutlookFolderTreeService>` — `AppOlObjects.FolderTreeService.cs:329`, `:352-354`), it would run
overridable code under the gate, and it would create a reentrancy path from user code back into
`FolderTreeService` while `_folderTreeServiceInitialization` is still live — the exact condition the
reentry guard at `:44-48` and `:95-105` exists to reject.

---

## 4. Recommended fix

### 4.1 Selected approach — await the barrier the fixture already provides

Use `run.Terminal`, which `StartWorkerAsync` already captures and returns
(`AppOlObjectsFolderTreeServiceTests.cs:257, 261`) and which six sibling tests already use (§1).

Insert one line between test `:111` and `:113`, of the shape used at `:73` and `:308`:

```csharp
(await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);
```

Why this is correct and deterministic:

- `run.Terminal` is completed at fixture `:202`, which is sequenced **after** the increment at `:200`, with
  a full fence (`Interlocked.Exchange`) at `:201` between them, and **before** the hook throws at `:204`.
  So the await cannot deadlock even with `throwFromTerminalHook: true`.
- The signal was captured at `:257` before any terminal could fire, so the await binds to the correct TCS
  generation. **Reading `sut.NextTerminal` at the assertion point instead would be wrong** — `:201` swaps in
  a fresh, never-completed signal, so a late read would hang until the test timed out.
- On this scenario the terminal fires exactly once (§5), so there is no ambiguity about which generation
  the await observes.
- It uses no wall-clock wait, no `Thread.Sleep`, no `Task.Delay`, no polling, and no new synchronization
  primitive.
- It strengthens the test's actual claim. The test is named
  `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault`, but today it only proves the *worker's*
  exception is `fault`. The terminal task handed to the hook is what "does not replace" refers to; asserting
  it directly closes that gap.

### 4.2 Explicitly rejected: pump/drain of a pending queue

There is no queue to drain. `ControlledUiDispatcher` holds no collection; `Capture`
(`AppOlObjectsFolderTreeServiceLifecycleTests.cs:296-305`) creates a single
`ControlledDispatchOperation` and publishes it through a one-shot TCS. `ReleaseAsync()` runs only that one
operation's own action, which on this path is a no-op (§2.3). Any plan that proposes "drain the
`ControlledUiDispatcher` pending queue" is proposing to build a mechanism that does not exist, for a hook
that is not dispatched through the dispatcher in the first place.

### 4.3 Recommended secondary hardening (zero net lines)

Bring `InvokedTerminalHookCount` into line with the `_loadCount` precedent (`:137, 160, 186`) without
adding any line to a near-cap file:

- `AppOlObjectsFolderTreeServiceLifecycleTests.cs:200` — `Interlocked.Increment(ref InvokedTerminalHookCount);`
- `AppOlObjectsFolderTreeServiceTests.cs:114` — `Volatile.Read(ref sut.InvokedTerminalHookCount).Should().Be(1);`
  (same shape as `Volatile.Read(ref sinkCounts[0]).Should().Be(1);` at `AppOlObjectsFolderTreeServiceLifecycleTests.cs:312`)

`System.Threading` is already imported in both files (`Lifecycle:2`, `Tests:3`). This is optional once §4.1
lands; it is cheap insurance and removes the last unsynchronised cross-thread field in the fixture.

### 4.4 Binding constraint the planner must respect — the 500-line ceiling

`.claude/rules/general-code-change.md` ("File Size Limit") and `CLAUDE.md` §4 cap every test file at 500
lines. Current sizes in this worktree:

| File | Lines | Headroom |
|---|---|---|
| `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` | 492 | 8 |
| `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` | 490 | 10 |
| `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.Coverage.cs` | 257 | 243 |

The §4.1 fix costs **one** line in the 492-line file and the §4.3 hardening costs **zero**. Any approach
that adds a new gate field, a new `TaskCompletionSource`, a new helper method, or a new fixture type will
consume most or all of the remaining headroom and may force a file split. That is a strong additional
argument for §4.1 over any bespoke-primitive alternative. CSharpier will reflow long lines, so the planner
should budget for the formatter possibly splitting the inserted statement across two lines.

---

## 5. Numeric Derivation Evidence

### Claim N1 — production dispatches the terminal notification from exactly 5 call sites

- **Complete Family:** every invocation in first-party production code that can reach
  `AppOlObjects.OnFolderTreeServiceInitializationTerminal`. Because the virtual is invoked from exactly one
  private wrapper, the family is the set of call sites of
  `AppOlObjects.NotifyFolderTreeServiceInitializationTerminal(Task<IOutlookFolderTreeService>)`, together
  with every direct invocation of the virtual itself. The method has no overloads (single declaration,
  single arity) and no other production member invokes it.
- **Exhaustive Search Scope:** all `*.cs` files in the worktree (production and test), unrestricted by
  directory, so that any production caller outside `TaskMaster/AppGlobals/` would be caught. Both the
  wrapper name and the virtual name were searched, covering the whole family rather than one name.
- **Inclusion Rules:** call expressions in production assemblies that pass a terminal
  `Task<IOutlookFolderTreeService>` into the notification path.
- **Exclusion Rules:** the wrapper's own declaration (`:328`); the virtual's own declaration (`:352`); the
  invocation inside the wrapper body (`:334`, which is the single funnel, not a dispatch site); all matches
  under `TaskMaster.Test/` (test-owned overrides and probes).
- **Primary Search Strategy or Query Expression:** Grep, pattern
  `override void OnFolderTreeServiceInitializationTerminal|OnFolderTreeServiceCompositionStarting|NotifyFolderTreeServiceInitializationTerminal`,
  glob `*.cs`, output mode content, whole worktree. Supplemented by a second Grep on the bare name
  `OnFolderTreeServiceInitializationTerminal`, glob `*.cs`, whole worktree, to cover the virtual directly.
- **Primary Member Set:**
  `AppOlObjects.FolderTreeService.cs:112`, `:221`, `:271`, `:324`, `:404`.
  (Excluded by rule: `:328` declaration, `:334` funnel, `:352` virtual declaration;
  `AppOlObjectsFolderTreeServiceLifecycleTests.cs:196`, `Coverage.cs:145`, `Coverage.cs:173` are test-owned.)
- **Primary Count:** 5
- **Cross-check Search Strategy or Query Expression:** independent full-file sequential read of
  `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs` lines 1-410 (the file's entire length),
  enumerating dispatch sites by enclosing member rather than by textual pattern.
- **Cross-check Member Set:**
  1. `:112` — inside the `FolderTreeService` getter, setup-failure branch (`:108-116`)
  2. `:221` — inside `CompleteFolderTreeServiceComposition`, successful-publish branch (`:219-222`)
  3. `:271` — inside `CompleteFolderTreeServiceCompositionFailure` (`:269-272`)
  4. `:324` — inside `CompleteFolderTreeServiceCompositionCancellation` (`:322-325`)
  5. `:404` — inside `Dispose` (`:402-405`)
- **Cross-check Count:** 5
- **Member-set Comparison:** normalized primary set `{112, 221, 271, 324, 404}` equals normalized
  cross-check set `{112, 221, 271, 324, 404}`. No element appears in one set only. Counts agree at 5. The
  two strategies are distinct (textual multi-pattern regex across the whole worktree vs. exhaustive
  sequential read of the single declaring file, enumerated by enclosing member).

### Claim N2 — the failing test's scenario fires the terminal hook exactly once

- **Complete Family:** the five production dispatch sites established by N1, evaluated against the concrete
  state trajectory of `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault`.
- **Exhaustive Search Scope:** all five sites of N1, each evaluated for reachability under this test's
  configuration (`DispatchMode.Pending` with a non-null fault; `Dispatcher` non-null; thread check returns
  false because `StartWorkerAsync:255` sets `ForceQueue = true` and `ControlledUiDispatcher.CheckAccess`
  at `:250-251` returns false when `ForceQueue`).
- **Inclusion Rules:** a site counts if its enclosing guard can evaluate true at least once during the test
  body (`AppOlObjectsFolderTreeServiceTests.cs:107-114`, before `CleanupAsync`).
- **Exclusion Rules:** sites whose guard is provably false for this trajectory; invocations occurring after
  the assertion at `:114` (i.e. inside `CleanupAsync`, `:118`).
- **Primary Search Strategy or Query Expression:** forward guard evaluation from the test body — trace each
  of the five sites' enclosing condition against the fixture configuration read from
  `AppOlObjectsFolderTreeServiceTests.cs:104-114` and `AppOlObjectsFolderTreeServiceLifecycleTests.cs:139-217`.
- **Primary Member Set:** `{:271}` only.
  `:112` excluded — `setupFailure` is null (dispatcher non-null at `:57-63`; `IsFolderTreeServiceDispatcherThread`
  returns false rather than throwing, fixture `:172-177`, since `dispatcherThreadCheckFailure` is null).
  `:221` excluded — `terminallyCompleted` requires `initialization.TrySetResult` at `:207`, but the
  operation's action never reaches `:207`: the release at test `:112` returns at `:182`.
  `:324` excluded — requires `TaskStatus.Canceled` at `:281-296`; `dispatcher.Complete(..., Faulted)` sets
  `TrySetException` (fixture `:414-415`), so the status is `Faulted`.
  `:404` excluded — `Dispose` runs only in `CleanupAsync` (`:276`) after the assertion, and by then
  `_folderTreeServiceInitialization` is already null (`:262`), so `initialization is not null` at `:394` is
  false and no notify occurs.
- **Primary Count:** 1
- **Cross-check Search Strategy or Query Expression:** backward idempotence argument from the shared
  completion guard, independent of which site is reached — every notify at `:221`, `:271`, `:324`, `:404`
  is gated on a boolean returned by a `TrySet*` on the same `TaskCompletionSource` instance
  (`:207`, `:261`, `:314`, `:396`), and `:112` is gated on `setupFailureCompleted` from `:89`/`:100` on that
  same instance. A `TaskCompletionSource` accepts exactly one terminal transition, so at most one
  `TrySet*` in the whole set can return true for a given `initialization`. At least one returns true here,
  because the test observes a faulted worker at `:111`, which requires the initialization task to have been
  completed terminally.
- **Cross-check Member Set:** exactly one successful terminal transition on the single `initialization`
  instance created at `AppOlObjects.FolderTreeService.cs:51-53`; observed as the faulted result asserted at
  test `:111` with `BeSameAs(fault)`, which identifies it as the `:261` transition inside
  `CompleteFolderTreeServiceCompositionFailure`, whose notify site is `:271`.
- **Cross-check Count:** 1
- **Member-set Comparison:** normalized primary set `{271}` equals normalized cross-check set `{271}`.
  Counts agree at 1. The two strategies are distinct (per-site forward guard evaluation vs. a
  single-terminal-transition idempotence argument over the shared TCS). This independently confirms that
  the expected value in the assertion, `Be(1)`, is correct and must not be relaxed as part of the fix; only
  the ordering is defective.

Note on double-invocation of the observer: `ObserveFolderTreeServiceDispatchTerminal` may be entered twice
(once from the continuation registered at `:148-157`, once inline from `:159-162`). This does not change
N2, because the second entrant's `TrySetException` at `:261` returns false and `completed` is false, so
`:271` is skipped.

---

## 6. Behaviour semantics for the fix

Success conditions after the change:

1. `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault` passes on every run, in both interleaving
   (a) and interleaving (b) of §2.4, with no dependence on thread-pool scheduling, core count, or coverage
   instrumentation.
2. `run.Worker` still faults with the object-identical `fault` instance (test `:111` unchanged).
3. The terminal `Task<IOutlookFolderTreeService>` delivered to the hook is faulted with the same
   object-identical `fault` instance — i.e. the hook's own `InvalidOperationException` from fixture `:204`
   did **not** replace it. This is the property the test name promises and the added assertion establishes.
4. `sut.LoadCount == 0` (test `:113`) is unaffected; the composition body never ran.
5. `sut.InvokedTerminalHookCount == 1` is asserted only after a happens-before edge from fixture `:200`.

Failure/edge conditions the fix must not introduce:

- No deadlock when `throwFromTerminalHook: true`. Guaranteed by the `:200 -> :201 -> :202 -> :204` ordering.
- No dependence on a freshly-read `sut.NextTerminal`; the captured `run.Terminal` must be used (§4.1).
- No change to `CleanupAsync` (`AppOlObjectsFolderTreeServiceTests.cs:270-280`), which must still tolerate
  an already-terminal initialization.

---

## 7. Testing implications (strategy only; no test code authored here)

- **Framework/libraries:** unchanged — MSTest, Moq, FluentAssertions, per `CLAUDE.md` CUT1/CUT2. The fix
  reuses the existing `GetExceptionAsync` helper (`AppOlObjectsFolderTreeServiceTests.cs:264-265`), which is
  FluentAssertions-based.
- **Determinism policy:** `.claude/rules/general-unit-test.md` ("Determinism Infrastructure") bans
  `Thread.Sleep`, `Task.Delay`, and real wall-clock waits in test code. The recommended fix introduces none;
  it awaits an existing `TaskCompletionSource`. No `TimeProvider`/`FakeTimeProvider` seam is required here,
  because the defect is an ordering omission, not a timing dependency — there is nothing to advance.
- **Parallelism is not a contributing factor and must not be "fixed" with `[DoNotParallelize]`.**
  `TaskMaster.Test` carries no `[assembly: Parallelize(...)]` (verified: the only such attribute in the repo
  is `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18`), and `.github/workflows/_mstest-coverage.yml:99`
  passes no `/Settings:`, so `TaskMaster.runsettings` (which does declare
  `<Workers>0</Workers><Scope>ClassLevel</Scope>` at lines 4-7) is **not applied in CI**. The class runs
  sequentially in the failing job. Adding `[DoNotParallelize]` would be a no-op that hides nothing and fixes
  nothing.
- **Red-before evidence.** The Bugfix Workflow in `CLAUDE.md` requires a failing regression test first. A
  naturally-red run is not reliably producible: the window is sub-microsecond and the test passes in
  interleaving (b) unconditionally. Two acceptable routes, in order of preference:
  1. Produce a deterministic red-before with a **temporary, reverted** instrumentation that defers fixture
     `:200`/`:202` past the assertion point (for example, a scoped local gate awaited at the top of the
     override), record the failing output as evidence, then revert the instrumentation and land only the
     §4.1 change. This yields a genuine red-before artifact.
  2. If (1) is judged to exceed the change budget or the 500-line headroom (§4.4), author a
     `no-fail-before-rationale` dossier under
     `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/regression-testing/`,
     following the established shape at
     `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/no-fail-before-rationale.2026-09-02T10-30.md`,
     and substitute a repeat-run stress record (precedent:
     `docs/features/archive/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/qa-gates/repeat-run-comparison.2026-08-08T17-03.md`).
- **Green-after:** repeat runs of `TaskMaster.Test` under the CI-shaped invocation
  `vstest.console.exe <assemblies> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
  (`.github/workflows/_mstest-coverage.yml:99`). Note the memory-recorded local caveat: local runs also need
  the `\.claude\` worktree exclusion so sibling worktrees' assemblies are not collected.
- **Coverage:** the change adds no production lines and no new production members, so the coverage
  denominator is unchanged. No coverage exemption is involved.

---

## 8. Related context checked

- **Issue #729 (`test-determinism-and-hygiene-debt`)** — the active folder
  `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/` exists and is thematically the
  parent programme (its `spec.md` finding 3 concerns `[DoNotParallelize]` on `DASLFilterParser*Tests.cs` in
  `UtilitiesCS.Test`). Nothing in #729 touches `TaskMaster.Test/AppGlobals/` or the folder-tree service
  lifecycle tests; its determinism work is `NonBlockingDelay`/`TimeProvider` and `Console.Out` parallelism.
  **No overlap, no conflict, no reusable seam.** Its value here is precedent only: the
  `no-fail-before-rationale` artifact shape cited in §7.
- **PR #746** — is the merge (`a679cd08`, per `issue.md:35`) that introduced the current state of these
  files. `git` history could not be queried in this session (the Bash tool is disabled for this agent), so
  the PR's diff was not inspected; the analysis above is derived entirely from the current file contents,
  which is sufficient because the defect is fully determined by the code as it stands. No material archaeology
  is required.
- **`.claude/rules/general-unit-test.md`** — the fix is aligned with UT1 (Determinism) and the
  Determinism Infrastructure section; it introduces no banned API.

---

## 9. Summary for the planner

| Question | Answer |
|---|---|
| Race exists? | Yes, mechanistically established (§2.2, §2.4) |
| Where? | Between `AppOlObjects.FolderTreeService.cs:261` (worker unblocks) and fixture `:200` (counter increments) on a different thread |
| Is `ReleaseAsync()` a barrier? | No — it is a complete no-op on this path (§2.3) |
| Is a pump/drain available? | No — `ControlledUiDispatcher` has no queue (§4.2) |
| Test-only or production-reachable? | **Test-only.** Zero production overrides, zero production subclasses, notify-after-publish is deliberate (§3) |
| Second latent cause? | Yes — `InvokedTerminalHookCount` is a plain non-volatile field with a non-atomic `++` (§2.5) |
| Recommended fix | Insert `(await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);` between `AppOlObjectsFolderTreeServiceTests.cs:111` and `:113` (§4.1); optionally harden the counter at zero line cost (§4.3) |
| Files to change | `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` (required), `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` (optional hardening) |
| Files that must NOT change | `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs` |
| Hard constraint | 500-line cap: 8 lines headroom in the required file, 10 in the optional one (§4.4) |
