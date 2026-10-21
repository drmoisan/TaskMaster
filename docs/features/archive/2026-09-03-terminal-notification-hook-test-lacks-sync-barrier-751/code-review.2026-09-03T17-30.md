# Code Review — terminal-notification-hook-test-lacks-sync-barrier (Issue #751)

- Artifact timestamp: 2026-09-03T17-30
- Branch: `bug/terminal-notification-hook-test-lacks-sync-barrier-751` @ `d2fbc327`
- Base: `f8414ee9` (independently re-derived as the merge base against both `main` and `origin/main`)
- Files reviewed in full: `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs`, `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs`, `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.Coverage.cs`, `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs`
- Verdict: **approve**. 0 blocking findings, 4 non-blocking findings originating in this review (NB-1, NB-5, NB-6, NB-7).

## The change

```csharp
// AppOlObjectsFolderTreeServiceTests.cs, TerminalNotificationHookFailure_DoesNotReplaceDispatchFault
dispatcher.Complete(run.Operation, DispatchMode.Faulted);
(await GetExceptionAsync(run.Worker)).Should().BeSameAs(fault);
await run.Operation.ReleaseAsync();
(await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);   // inserted
sut.LoadCount.Should().Be(0);
Volatile.Read(ref sut.InvokedTerminalHookCount).Should().Be(1);           // was: sut.InvokedTerminalHookCount.Should().Be(1);
```

```csharp
// AppOlObjectsFolderTreeServiceLifecycleTests.cs, ControlledAppOlObjects.OnFolderTreeServiceInitializationTerminal
Interlocked.Increment(ref InvokedTerminalHookCount);                      // was: InvokedTerminalHookCount++;
var signal = Interlocked.Exchange(ref _terminalSignal, Signal<TreeTask>());
signal.TrySetResult(terminalInitialization);
if (_throwFromTerminalHook)
    throw new InvalidOperationException("Controlled terminal hook failure.");
```

## Central question: does the barrier close the race, or only narrow it?

It closes it. The argument is an ordering argument and it holds on the three files, verified line by line rather than accepted from the plan.

**1. The awaited object is the same generation the incrementing invocation completes.** `StartWorkerAsync` captures `var terminal = sut.NextTerminal;` (`AppOlObjectsFolderTreeServiceTests.cs:258`) *before* starting the worker, and returns it as the tuple member `Terminal`. `NextTerminal` is `Volatile.Read(ref _terminalSignal).Task` (`AppOlObjectsFolderTreeServiceLifecycleTests.cs:157`). Inside the hook, `Interlocked.Exchange` installs a fresh `TaskCompletionSource` and returns the previous one, and `TrySetResult` is called on that previous one — which is exactly the instance whose `Task` the tuple holds. The captured member therefore completes; a freshly read `sut.NextTerminal` at the assertion point would bind to the newly installed generation and never complete. The delivered code uses the captured member, which is the correct choice and matches seven sibling call sites.

**2. The increment precedes the completion on the notifying thread, with a full fence between them.** Program order inside the override is: `Interlocked.Increment` (implies a full fence), then `Interlocked.Exchange` (a second full fence), then `TrySetResult`. The `TaskCompletionSource` was created with `TaskCreationOptions.RunContinuationsAsynchronously` (`Signal<T>`, `:449-450`), so the awaiting continuation is scheduled only after the result is published. The awaiting thread's resumption therefore happens-after the increment. This is a genuine happens-before edge, not a probability shift: there is no interleaving in which the continuation runs and the increment has not.

**3. The count cannot be observed as 2 instead of 1.** The production notifier is invoked only on a path guarded by a `TrySet*` that returned `true` (`AppOlObjects.FolderTreeService.cs:110-112`, `:219-221`, `:269-271`, `:322-324`), and each of those paths first nulls `_folderTreeServiceInitialization` under `_folderTreeServiceGate`. In this scenario the single terminal is `CompleteFolderTreeServiceCompositionFailure` reached from the faulted dispatch continuation. The subsequent `await run.Operation.ReleaseAsync()` executes the captured composition action, which returns immediately at the identity guard (`:177-179`) because the initialization field is already null — which is also why `LoadCount` remains 0. `CleanupAsync`'s `Dispose` finds a null initialization and notifies nothing, and it runs after the assertions in any case. `Be(1)` is therefore stable in both directions, not merely no longer flaky downward.

**4. The barrier cannot deadlock in the containment scenario.** The override signals before it throws (`:202` precedes `:203-204`), and the production notifier swallows the hook's exception (`NotifyFolderTreeServiceInitializationTerminal`, `:332-336`). Had the fixture thrown before signalling, the inserted await would hang. The delivered ordering is correct, and this is the property the test's own `throwFromTerminalHook: true` setup makes load-bearing.

**5. The `Volatile.Read` is not what closes the race, and is not redundant either.** With the barrier in place the acquire semantics of the continuation already order the read. The `Volatile.Read` guarantees the read is atomic and not hoisted or cached by the JIT, and it pairs the read side with the now-atomic write side. It is a correct secondary hardening, and the spec is right that it fixes visibility rather than ordering.

Conclusion: the race identified in the issue is eliminated, not narrowed. The measured effect is consistent — the target test's duration rose from 0.00164–0.00191 s to 0.00177–0.00235 s, the cost of one additional completed-task await.

## Determinism

- No `Thread.Sleep`, `Task.Delay`, `SpinWait`, wall-clock read, or polling loop is introduced. The only added constructs are `await` on an existing completion signal, `Interlocked.Increment`, and `Volatile.Read`.
- No `[Ignore]`, `[DoNotParallelize]`, retry wrapper, or narrowed test filter was introduced. Verified independently from the TRX files: `notExecuted = 0` on every run, and the test population is identical pre- and post-change (408 in `TaskMaster.Test`, 6984 solution-wide).
- The five-run green-after series is genuine evidence of the repaired test's stability. It is not evidence of a red-to-green transition, and the executor's own comparison artifact states that plainly rather than overclaiming.

## Counter synchronization completeness

Search across the repository for `InvokedTerminalHookCount` returns exactly three source occurrences:

| Location | Access | Synchronized |
|---|---|---|
| `AppOlObjectsFolderTreeServiceLifecycleTests.cs:158` | field declaration (`internal int`) | declaration |
| `AppOlObjectsFolderTreeServiceLifecycleTests.cs:200` | write | `Interlocked.Increment` |
| `AppOlObjectsFolderTreeServiceTests.cs:115` | read | `Volatile.Read` |

Every read and every write is now synchronized. This matches the fixture's existing `_loadCount` precedent (`Interlocked.Increment` at `:186`, `Volatile.Read` at `:160`) and its `sinkCounts`/`sinkThreads` precedent. The sibling field `LoadThreadId`, declared on the same line, remains a plain write — it is pre-existing, out of this change's footprint, and is read only after a completion await at its call sites.

## Design, structure, and repository conventions

- **Simplicity.** The fix reuses a signal the fixture already maintains. No new type, field, `TaskCompletionSource`, or helper is added. This is the smallest correct change and the right one.
- **Reuse.** The inserted line is character-shape-identical to seven existing call sites, so a reader familiar with the file needs no new concept.
- **Assertion strength.** The inserted assertion is not a bare await: it asserts that the task handed to the terminal hook is faulted with the object-identical `fault`. It therefore strengthens the test's coverage of the containment behaviour under review while serving as the barrier. That is a better choice than a discarding `await run.Terminal;`.
- **File size.** 493 and 490 lines, both under the 500-line cap. Counted directly.
- **Framework conventions.** MSTest attributes, Moq strict mocks, FluentAssertions — all preserved.
- **Formatting.** `dotnet tool run csharpier check` on both files returns exit 0, re-run by this reviewer.

## Non-blocking findings

**NB-1 — a future "hook never invoked" regression becomes a hang rather than an assertion failure.**
Before the change, a regression in which the terminal hook is never invoked failed the test immediately with `Expected sut.InvokedTerminalHookCount to be 1, but found 0`. After the change, the inserted `await run.Terminal` would block first, and nothing bounds that wait: the method carries no `[Timeout]`, `TaskMaster.Test` declares no assembly-level timeout, and `.github/workflows/_mstest-coverage.yml` invokes vstest without `/Settings`, so `TaskMaster.runsettings` is not applied in CI. `spec.md:281` anticipates the hazard ("it would hang until the test timed out") but no test timeout exists to stop it. The failure would surface as a stalled CI job rather than a named test failure.
*Recommendation:* add `[Timeout(5000)]` to `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault`, following `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs:32`, whose own comment already frames the attribute as "a deadlock bound, not a wait". Cost is one line against 7 remaining lines of headroom in that file.

**NB-5 — the fail-before route-2 justification leans on the weaker of its two reasons.**
Route 2 was selected by invoking the `spec.md` condition "exceeds the change budget or the remaining line headroom", evidenced by the Lifecycle file standing at 490 of 500 lines. That reasoning is strained: route-1 instrumentation is temporary and reverted before landing, so it never consumes landed headroom, and the 500-line cap is a property of the committed tree. The justification that actually carries the decision is the second recorded reason — a red obtained by deferring the fixture's own increment demonstrates that a deferred increment is observable as 0, which restates the race rather than reproducing it, and is not reproducible from the landed tree. The outcome is defensible and the spec authorized it; the recorded rationale should have led with the sound reason.

**NB-6 — format command shape.**
The write step ran `dotnet tool run csharpier format <two files>` rather than `CLAUDE.md`'s `format .`. Immaterial in effect: the verification step ran `csharpier check .` repository-wide at exit 0, which is the CI-parity gate, and this reviewer re-ran the scoped check at exit 0.

**NB-7 — the assertion couples to the field-ness of another type's internal member.**
`Volatile.Read(ref sut.InvokedTerminalHookCount)` takes a `ref` to an instance field of `ControlledAppOlObjects`. It compiles only while that member stays a field; promoting it to a property (for example, to mirror the `LoadCount` accessor pattern) breaks this assertion at compile time. The trade is deliberate and is consistent with the fixture's existing style, and a compile error is a safe failure mode, but it is worth knowing before refactoring the fixture.

## Items explicitly checked and found clean

- No production file changed: the diff restricted to all nine production project directories returns 0 rows, and `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs` has blob hash `fe980d3fc6726cbc49e391798489e0af82a683a1` at both base and head.
- No new `using` directive was required or added; `System.Threading` is already imported in both files.
- No coverage-configuration, runsettings, workflow, or project file changed.
- No absolute host path, account name, or machine name appears anywhere in the feature folder — a repeated leak class in this repository, and it is absent here. The coverage artifacts deliberately describe the two `.coverage` paths structurally instead of transcribing them, which is the correct handling.
- Evidence artifact internal timestamps (14:30–14:49) are consistent with the commit dates (14:35–14:49) and with the on-disk TRX and build-output timestamps (14:24–14:42). The `2026-09-03T11-48` filename suffix is the plan timestamp, not a claim about when each artifact was produced.
