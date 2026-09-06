---
name: terminal-hook-barrier-751
description: "#751: the flaky assert races a notify that runs AFTER the terminal TrySet on another thread; ReleaseAsync is a total no-op there; run.Terminal is the existing barrier 6 siblings already use; both test files are ~490/500 lines"
metadata:
  type: project
---

Issue #751 (`TerminalNotificationHookFailure_DoesNotReplaceDispatchFault`) is a **test-only** ordering
defect, verified 2026-09-03. Research artifact:
`docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/research/2026-09-03T09-45-terminal-notification-hook-sync-barrier-research.md`

**Why:** four non-obvious facts that a re-reader would otherwise have to re-derive:

1. **The issue's file attribution is half wrong.** The test method + `CreateSut` + `StartWorkerAsync` are in
   `AppOlObjectsFolderTreeServiceTests.cs`, but `ControlledUiDispatcher`, `ControlledAppOlObjects` and
   `ControlledDispatchOperation` are in the sibling partial `AppOlObjectsFolderTreeServiceLifecycleTests.cs`.
   Both files must be named in any plan.
2. **`await run.Operation.ReleaseAsync()` is a complete no-op on the fault path** — its captured action
   returns immediately on the `ReferenceEquals(initialization, _folderTreeServiceInitialization)` guard
   because the field was already nulled, and its `TrySetResult` loses to the earlier `TrySetException`.
   It looks like a barrier and is not one. There is also **no queue to pump**: `ControlledUiDispatcher`
   holds one `TaskCompletionSource`, not a collection, so "drain the pending queue" is not implementable.
3. **The barrier already exists and is already returned.** `StartWorkerAsync` captures `sut.NextTerminal`
   before starting the worker and returns it as tuple item 3. Six sibling tests await `run.Terminal`;
   the flaky one is the only one that ignores it. Reading `sut.NextTerminal` *at* the assertion instead
   would hang — the hook `Interlocked.Exchange`s in a fresh never-completed signal.
4. **Production notifies AFTER publishing the terminal result, deliberately and at all 5 call sites**,
   outside the composition lock, so overridable code never runs under the gate. Zero production overrides
   and zero production subclasses of `AppOlObjects` exist (all 8 subclasses are in `TaskMaster.Test`).
   Do not "fix" this in production.

**How to apply:** when a TaskMaster test asserts a counter incremented by a virtual hook, check whether the
hook fires before or after the `TaskCompletionSource.TrySet*` that releases the awaited task — the notify
being outside the lock means "the awaited task completed" never implies "the hook ran". Also check
`Interlocked`/`Volatile` symmetry: in this fixture `_loadCount` is correct and `InvokedTerminalHookCount`
is the sole plain non-volatile cross-thread counter.

**Hard constraint for any edit here:** `AppOlObjectsFolderTreeServiceTests.cs` is 492 lines and
`AppOlObjectsFolderTreeServiceLifecycleTests.cs` is 490 against the 500-line cap. Anything that adds a new
gate field, TCS, helper, or fixture type will not fit. The recommended fix is one line.

`[DoNotParallelize]` is a red herring: `TaskMaster.Test` has no `[assembly: Parallelize]` and CI passes no
`/Settings:`, so `TaskMaster.runsettings` is not applied and the class already runs sequentially.

Related: [[feedback-exemption-audit-check-proven-techniques]] (same habit — grep for the already-proven
technique in sibling tests before inventing a new mechanism).
