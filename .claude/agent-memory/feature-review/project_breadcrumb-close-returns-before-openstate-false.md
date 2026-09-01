---
name: breadcrumb-close-returns-before-openstate-false
description: "#656: BreadcrumbDropDownHost.Close returns true before OpenState=false, so `_closeCompleted && IsOpen` IS occupiable in production; reopen-path enumerations answer a narrower question than they are credited with"
metadata:
  type: project
---

`BreadcrumbDropDownHost.Close` (`QuickFiler/Viewers/BreadcrumbDropDownHost.cs:251-254`) returns
`true` after only *scheduling* `CompleteClose`. `CompleteClose` (`:397-411`) is what sets
`OpenState = false`, and it is dispatched via `BreadcrumbDropDownOpenLifetime.ScheduleInvalidating`
-> `ScheduleObserved` -> `RunOnOwnerAsync` -> `_uiOperations.PostAsync`, i.e. queued on the **same**
`BreadcrumbPopupUiOperations` dispatcher the coordinator posts its own work to.

Consequence: `_closeCompleted == true && _host.IsOpen == true` is a state the **production** host can
occupy, with no substituted seam and no reopen.

**Why:** #656's spec and research artifact proved by exhaustive enumeration that no production path
reopens the host without `RequestOpen`/`Invalidate` (only `OpenState = true` is
`BreadcrumbDropDownOpenLifetime.cs:268`, reachable only via `RequestOpen`, which clears the flag).
That enumeration is correct, but it answers "can the host be *reopened* without the entry points?"
The guard `if (_closeCompleted && !hostOpen)` depends on a different proposition — "can both flags be
true at once?" — which the asynchronous close window answers yes. The spec reported the narrower
result as though it settled the broader one, and concluded a rollback would be "observationally
identical on every shipped path". Not established.

Second-order effect: a second `_host.Close` inside that window re-enters the `OpenState == true`
branch, whose `InvalidateAndSchedule` bumps the lifetime generation and makes the *first* scheduled
`CompleteClose` fail its lease check. The second reason wins, and `FinishClose` calls
`_cancelSelection()` only for `Uncommitted` (`:449-451`) — so an Uncommitted-then-ExplicitCommit pair
can drop the selection cancel.

Also: every test fake clears `IsOpen` synchronously inside `Close`
(`BreadcrumbDropDownOpenCoordinatorTests.cs:436-437`), so no test represents this timing. 100%
changed-line coverage and a 91% class branch rate did not touch it.

**How to apply:** when any breadcrumb review credits a reopen-path enumeration with proving a guard
unreachable, check whether the guard's actual predicate is about reopening or about two states
coinciding. Trace `Close` to whatever sets `OpenState = false` and confirm it is synchronous before
accepting the claim. Related: [[verify-the-asserted-evidence-mechanism]],
[[505-coordinator-prime-toggle-race]].
