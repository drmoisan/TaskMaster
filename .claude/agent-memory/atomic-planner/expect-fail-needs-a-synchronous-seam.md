---
name: expect-fail-needs-a-synchronous-seam
description: An [expect-fail] task routed through an async void boundary cannot produce the stated RED — it either false-GREENs or aborts the testhost; route the fail-before through the synchronous seam and move the boundary-containment tests after the fix
metadata:
  type: feedback
---

Before writing an `[expect-fail]` acceptance that names a total/passed/failed triple and an exception
name, trace the path from the test's ACT call to the throw site and answer one question: **does the
exception reach the assertion synchronously?**

The trap: a handler declared `async void` (e.g. `BreadcrumbBridgeRouter.OnHostMessageReceived`,
`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:266`) with a narrow `catch` that does not match.
If there is no `await` between the entry and the throw, the faulted task rethrows at the `await`,
misses the catch, and reaches `AsyncVoidMethodBuilder.SetException`. On an MSTest thread there is no
`SynchronizationContext`, so it becomes an unhandled exception on a thread-pool thread. Two outcomes,
neither of which is the stated acceptance:

1. The `_host.Raise(...)` call returns normally, so a `NotThrow()` + `state-unchanged` pair PASSES
   pre-fix. The `[expect-fail]` task reports GREEN and the fail-before evidence is a false negative.
2. The queued rethrow lands first and the vstest testhost aborts with no complete TRX, so the
   TRX-existence acceptance is unsatisfiable.

**Remedy (the shape that cleared #498 preflight):** author the RED pair against the DIRECT path
using the test class's existing synchronous helper — here `Inbound(json)`, which calls
`ProcessInboundAsync(json).GetAwaiter().GetResult()` — so the exception surfaces on the MSTest
thread. Then move the `_host.Raise(...)` boundary-containment methods (which the AC often requires
LITERALLY) into a task authored and first executed AFTER the fix task, and say in that task's text
that they must not be run before the guard exists. Renumber nothing: the methods move between
tasks, the task IDs do not.

**Why:** #498 preflight revision 1, Finding 2 (BLOCKING). The plan had both range tests going through
the async-void seam and asserted "total 2, passed 0, failed 2 with System.ArgumentOutOfRangeException".

**How to apply:** applies to any `async void` event handler, any `[expect-fail]` whose ACT is an event
raise, and any fire-and-forget tail. Pairs with [[project_230_winforms_pump_seam_plan_facts]]
(awaited vs fire-and-forget tails decide completion-vs-fault test shape) and
[[project_468_preflight_revision_seams]] (seam before red test).

## Companion trap: Moq Times.Never() over a member the arrange already calls

`Times.Never()` on a member that the test's own `Bind()`/`Setup` path invokes is unsatisfiable by
construction — it fails whatever the implementation does. Before writing a `Times.Never()`
acceptance, check whether the arrange helper reaches that member (here `Bind()` -> `BindRowsAsync`
-> `FetchChainAsync` -> `_provider.ResolveLeafKeyAsync`). If it does, scope the assertion:
`_provider.Invocations.Clear();` immediately before the ACT, then verify `Times.Never()`. A landed
sibling test asserting `Times.Once` on the same member is the tell that the arrange path calls it.
(#498 Finding 3, BLOCKING.)

## Corollary: a Moq scoping fix can DELETE the only reason a test was RED

Scoping an over-broad assertion is correct, but re-run the RED analysis afterwards. #498 revision 2,
Finding 1 (BLOCKING): after `_provider.Invocations.Clear()` scoped the `Times.Never()`, every
remaining assertion in `HandleArrowKey_RightOnActivatedParent_ExpandsViaSingleImmediateSubfolderCall`
already held on `main`, so the `[expect-fail]` acceptance `total 2, passed 0, failed 2` became
unachievable. The test had silently become a CONTROL duplicating a landed pin.

**The test-1-is-a-control smell:** the plan's own prose says the fix "consumes the landed seam
unchanged" and cites a landed test pinning the same behavior. If the arrange path reaches the
landed happy path, the pre-fix branch under test is never entered.

**Remedy that keeps every task ID stable:** find a STATE the pre-fix branch order short-circuits on.
Here, `BreadcrumbRow.ActivateSegment` leaves `CollapsedAfterIndex` untouched
(`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs:168-171`) and `IsCollapsed =>
CollapsedAfterIndex.HasValue` (`:115`), so arranging a COLLAPSED activated row makes the pre-fix
`if (row.IsCollapsed) { ReExpand }` arm win and the provider call count is zero — deterministically
RED. Strengthening the test beats splitting the task; option (a) (author one method, retarget the
run tasks, add a post-fix control) costs a full renumber of the phase.

**Second-order check:** once the test pins an ORDERING, the FIX task must state that ordering
explicitly, including the case the test exercises. Also check the seam methods the fix now reaches
for state guards — `ToggleLeafExpanded` is a documented no-op while collapsed
(`BreadcrumbRow.cs:274-283`), so the fix has to clear the collapse as PART of the transition or the
expansion it performs is invisible.
