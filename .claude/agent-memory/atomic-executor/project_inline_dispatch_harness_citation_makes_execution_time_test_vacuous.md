---
name: inline-dispatch-harness-citation-makes-execution-time-test-vacuous
description: A plan that pins an "evaluated at execution time, not scheduling time" test to an INLINE-dispatch test harness makes that test unable to fail; check the cited harness's SynchronizationContext before clearing it.
metadata:
  type: project
---

When a plan's headline acceptance criterion is "the predicate is evaluated at execution
time of the scheduled action, not at scheduling time", the pinning test needs a harness
whose dispatcher DEFERS callbacks so the flip-between-schedule-and-drain window exists.
Verify the cited harness actually defers. In QuickFiler.Test/Viewers, two look alike:

- `BreadcrumbDropDownLifecycleConcurrencyTests.cs` uses `InlineSynchronizationContext`
  (`Post(cb, s) => cb(s)`) — it executes immediately and provides NO gap. It does not
  reference `BreadcrumbPopupUiOperations` at all.
- The deferred-drain harness is `BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext`
  (`internal sealed`, with `DrainUntil` / `DrainAll`), consumed as
  `new BreadcrumbPopupUiOperations(new BreadcrumbUiDispatcher(context, errors.Enqueue))`
  — see `BreadcrumbDropDownCoverageThresholdTests.cs` and `BreadcrumbDropDownLifecycleCoverageTests.cs`.

Second-order trap: `BreadcrumbDropDownHost`'s PUBLIC constructors resolve their
`BreadcrumbPopupUiOperations` via `BreadcrumbPopupUiOperations.CaptureCurrent()`. A plan
that pins the harness to a public constructor AND wants a deferred drain must also state
`SynchronizationContext.SetSynchronizationContext(capturing)` BEFORE the `new` call, or
authorize the internal overload that takes `operations` explicitly. Omitting that ordering
silently yields the ambient (inline/none) context and the gap disappears again.

**Why:** an inline harness makes the execution-time test pass identically whether the
predicate is read at scheduling or at execution — the assertion cannot fail, which is
exactly the unfalsifiable-gate class `plan-acceptance-gates` exists to catch.

**How to apply:** during preflight, for any task whose stated purpose is an
execution-time / late-arrival / ordering pin, open the cited harness and read its
`SynchronizationContext.Post` implementation before accepting the citation. See
[[project_preflight_selfderived_gate_thresholds_are_blind]] for the sibling failure mode.
