# qfcqueue-enqueueasync-jobsrunning-counter-leak (Potential Bug)

- Date captured: 2026-09-12
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

`QfcQueue.EnqueueAsync` increments the `_jobsRunning` counter outside the `try` block whose `finally` decrements it, and performs an awaited, throwable dispatcher call in the gap between the two. If that call throws, the counter is incremented and never decremented for the remaining lifetime of the `QfcQueue` instance. Three separate consumers spin on that counter reaching zero, so a single failure in the gap converts into an unbounded wait rather than a surfaced error. This was found during preparation for issue #871 and is deliberately out of that item's scope: #871 adds injectable seams and splits the file, and widening it to change control flow would mix a behavioural fix into a testability change.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable (C# / .NET Framework 4.8 VSTO add-in, MSTest + Moq)
- Command/flags used: static inspection of `QuickFiler/Controllers/QfcQueue.Enqueue.cs` and `QuickFiler/Controllers/QfcQueue.cs` at commit 2405a829d
- Data source or fixture: none; the defect is visible in the control flow as written

## Steps to Reproduce

1. Read `QuickFiler/Controllers/QfcQueue.Enqueue.cs:94`. `Interlocked.Increment(ref _jobsRunning);` executes unconditionally.
2. Read `QuickFiler/Controllers/QfcQueue.Enqueue.cs:97-99`. `await UiIdleCallAsync(() => _tlpTemplate.Clone(name: "BackgroundTableLayout"))` runs next. `UiIdleCallAsync` routes through `UiThread.Dispatcher`, which throws `InvalidOperationException` when the dispatcher has not been set, and `_tlpTemplate` is null until `TlpTemplate` has been assigned, which makes `Clone` throw `NullReferenceException`.
3. Read `QuickFiler/Controllers/QfcQueue.Enqueue.cs:103`. The `try` block opens only here, and its `finally` at lines 128-137 holds the matching `Interlocked.Decrement(ref _jobsRunning)`.
4. Conclude that any throw from step 2, and any throw from the `Task.Run` hook loop at lines 90-92 that occurs after a prior successful increment, escapes `EnqueueAsync` with the counter permanently one higher than it should be.
5. Observe the consumers that then never finish: `QfcQueue.cs:56` (`while (_jobsRunning > 0)` in `CompleteAddingAsync`), `QfcQueue.cs:117` (the `_queue.Count + _jobsRunning > 0` loop condition in `TryDequeueAsync`) and `QfcQueue.cs:217` (`while (JobsRunning > 0)` in `JobsToFinish`).

## Expected Behavior

The increment and the decrement are balanced on every control-flow path. Either the increment moves inside the `try`, or the `try` opens before the increment, so that a throw anywhere between the two restores the counter before the exception propagates. A caller that fails to enqueue a page receives the failure and the queue returns to a consistent state.

## Actual Behavior

The counter leaks by one per failed call. `CompleteAddingAsync` waits until its own timeout and then throws; `JobsToFinish` waits forever against a token that may never cancel; `TryDequeueAsync` keeps polling because it believes a producer is still running. The failure presents as a hang or a timeout far from the line that actually threw.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: no runtime log captured. The finding is a static control-flow reading made during preparation of issue #871 on 2026-09-12.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High: the observable symptom is a hang on the QuickFiler high-confidence display path, and the state is unrecoverable for the lifetime of the instance. Severity is tempered by the fact that the throwing conditions in step 2 are themselves uncommon in a correctly initialised add-in, so this is a latent fault rather than an everyday one.

## Suspected Cause / Notes

- The `try` was most likely placed to scope the two `catch` clauses around `LoadControllersViewersAsync` specifically, and the bookkeeping pair was never re-examined once the `finally` was added.
- `RemoveItem` (`QuickFiler/Controllers/QfcQueue.cs:189-210`) has the same increment-then-work-then-decrement shape with no `try` at all, so the same class of leak applies there and should be assessed in the same change.
- `ChangeIterationSize` (`QuickFiler/Controllers/QfcQueue.cs:340-399`) likewise increments at line 340 and decrements at line 399 with unguarded work in between.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: a test that forces the dispatcher call to throw and then asserts `JobsRunning` has returned to its pre-call value. Issue #871 introduces exactly the seam needed to inject that throw, so this item should be scheduled after #871 merges and should reuse its seams rather than adding new ones.
- [x] Integration scenario to retest: high-confidence mode with more than one page, confirming background pages still render after the control-flow change.
- [x] Manual verification notes: the fix must not change the exception type or the exception timing observed by callers; only the counter state on the failure path changes.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
