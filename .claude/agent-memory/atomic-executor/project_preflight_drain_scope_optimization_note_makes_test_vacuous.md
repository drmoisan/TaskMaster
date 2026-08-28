---
name: preflight-drain-scope-optimization-note-makes-test-vacuous
description: A "cosmetic" plan note telling the executor to SKIP draining on paths claimed to be synchronous can turn a negative-assertion test into an unfalsifiable one; verify every "this path is synchronous" claim against the real call chain under the harness's dispatcher mode.
metadata:
  type: project
---

A plan round that adds an efficiency/cosmetic note of the form "path X is synchronous
regardless of harness mode, so do not add pointless drains there" is a **behavioral**
claim, not a cosmetic one, and must be re-derived from the call chain before it is
accepted as a non-blocking edit.

Concrete case (#677 plan v1.3, `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`): the note
asserted that tests using `Close(BreadcrumbDropDownCloseReason)` were synchronous because
`CompleteAll` "executes synchronously and never schedules". True but irrelevant —
`CompleteAll` is only ever REACHED from a posted callback:

- `Close(reason)` with `OpenState` true -> `_openLifetime.InvalidateAndSchedule(() => CompleteClose(reason, true))`
- `Close(reason)` otherwise -> `TryCancelPendingOpen(...)`
- both -> `ScheduleObserved` -> `RunOnOwnerAsync` -> `BreadcrumbPopupUiOperations.PostAsync`
  -> `BreadcrumbUiDispatcher.Dispatch` -> `_context.Post(...)`

The repo's own exemplar contradicted the note in the same file the plan cited as a model:
`LifecycleHarness.Close` does `Host.Close(reason); Context.DrainAll();`.

**Why:** obeying the note would have left the negative test
(`...PredicateFalse_DoesNotFocusAnchor`, asserting `FocusAnchorCount == 0`) passing
identically with and without the fix, because nothing executed at all. The positive
control tests would have failed for a setup reason, masking the real signal. A "don't
bother" note is therefore the cheapest way to make a whole test class vacuous.

**How to apply:** when a preflight delta adds or keeps a claim that some act step needs no
drain / no pump / no await, trace the act step to the assertion target through the
dispatcher the harness actually installs. Two specific traps:
1. A synchronous *callee* (`CompleteAll`) says nothing about how its *caller* is reached.
2. "Regardless of harness mode" is almost always false when the plan itself switched the
   harness from an inline/current-thread dispatcher to a capturing-context one.
Also check the negative-assertion tests first: they fail silently (pass vacuously),
whereas the positive controls fail loudly and would have been caught at execution.

Related: [[inline-dispatch-harness-citation-makes-execution-time-test-vacuous]],
[[confirmatory-preflight-proportionate-bar]].
