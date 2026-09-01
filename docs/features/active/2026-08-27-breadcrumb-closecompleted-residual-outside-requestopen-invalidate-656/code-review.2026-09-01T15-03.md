# Code Review — Issue #656 (breadcrumb `_closeCompleted` residual)

- Timestamp: 2026-09-01T15-03
- Branch: `bug/breadcrumb-closecompleted-residual-outside-requestopen-invalidate-656`
- Head SHA: `65d2f22b5100588eae8ac4de40e48f1ac391db34`
- Base: `main` at `5670b3cfe6a52e3b890bf80f0cd85a20d4fe4723`
- Code surface under review: 2 files, +58/-1 lines.

## Change summary

`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`

- Line 326: `bool hostOpen = _host.IsOpen;` hoisted above the `lock (_sync)` that opens `CloseCore`.
- Line 333: the completed-close guard narrowed from `if (_closeCompleted)` to
  `if (_closeCompleted && !hostOpen)`.
- Two `<remarks>` blocks added: on the `_closeCompleted` field (lines 46-52) and on `CloseCore`
  (lines 315-323).

`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs`

- One added test, `CloseCore_AfterSuccessfulCloseAndHostReopen_ReachesHostCloseAgain`.

## What the change does, stated as a truth table

| `_closeCompleted` | `hostOpen` | Before | After |
|---|---|---|---|
| false | either | fall through to `_host.Close` | unchanged |
| true | false | suppress, return true | unchanged |
| true | true | suppress, return true | **fall through to `_host.Close`** |

Exactly one cell changes. That is the minimum edit that achieves the stated objective, and it is
the right shape: it does not clear `_closeCompleted` on the successful-close path, which
`issue.md` records would break two standing tests, and it does not replace the flag with a bare
`!_host.IsOpen` gate, which would break
`PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen`.

## Positive observations

1. **Lock discipline is respected, and for a stated reason.** SR-4 of #501 declined this refinement
   when it was written as a read taken inside the lock. Hoisting the read keeps the count of foreign
   calls made under `_sync` unchanged. Verified directly: across all twelve `lock (_sync)` bodies in
   the file, the only `_host` member invocation is the pre-existing
   `if (_closeInFlight && _host.IsOpen)` at line 119 in `RequestOpen`. Lines 200, 205, 216, 265-266
   and 340 all sit outside any lock. No new foreign call under the lock was introduced.

2. **The added `<remarks>` explain why, not what.** The `CloseCore` block records the SR-4 precedent
   and the reason the read is outside the critical section. A future reader will not re-litigate the
   inside-the-lock variant, which is precisely the failure mode that produced this issue.

3. **The regression test is genuinely red-first.** The recorded red run is an assertion failure with
   the verbatim message `... but {BreadcrumbDropDownCloseReason.Uncommitted {value: 1}} contains 1
   item(s) less.` — not a compile failure and not a missing-symbol failure. That is the strong form
   of red-first evidence.

4. **The test is deterministic by construction.** One thread, an explicitly pumped context via
   `DrainUntil`/`DrainAll`, no timers, no sleeps, no temporary files. It satisfies the determinism
   infrastructure requirements in `.claude/rules/general-unit-test.md` without needing a fake clock,
   because it reads no clock.

5. **Footprint discipline held under pressure.** `issue.md` explicitly directs the fix at feature
   #488's host-surface files (`BreadcrumbItemViewerLifecycleCoordinator.cs`,
   `BreadcrumbDropDownHost.cs`, `ItemViewer.Breadcrumb.cs`). The research artifact established that
   touching them was unnecessary, and the diff touches none of them. That is the correct
   resolution of a directive that would otherwise have widened the change substantially.

## Findings

### CR-1 — The production-unreachability claim is overstated. The new branch is reachable on the shipped host without any seam substitution.

**Severity: Major. Non-blocking. Not merge-method-dependent.**

`spec.md` classifies this work as latent-correctness hardening and states, in **Mitigations and
rollbacks**, that "no production path can currently reach the changed state" and that a rollback
"restores behavior that is observationally identical to the fixed build on every shipped path."
That conclusion rests entirely on the reopen-path enumeration.

The enumeration itself is correct. I re-derived it independently:

- `OpenState = true` occurs at exactly one place in production, `BreadcrumbDropDownOpenLifetime.cs:268`.
- Every other production assignment sets it false (`BreadcrumbDropDownHost.cs:334, 402, 434, 460`).
- The only production callers of a host `OpenAsync` are `BreadcrumbDropDownOpenCoordinator.cs:265-266`,
  inside `BeginOpenCore`, reachable only from `RequestOpen`, which clears `_closeCompleted` at line 121.
- `IBreadcrumbDropDownHost` has exactly one production implementation.

So no *reopen* path bypasses both entry points. That much is established.

But the guard's new branch does not require a reopen. It requires only the state
`_closeCompleted == true && _host.IsOpen == true`, and the production host can occupy that state
through a mechanism the enumeration never examines: **`Close` returns `true` before `OpenState`
becomes `false`.**

```
BreadcrumbDropDownHost.cs:251-254
    if (OpenState)
    {
        _openLifetime.InvalidateAndSchedule(() => CompleteClose(reason, true));
        return true;
    }
```

`OpenState = false` is set inside `CompleteClose` (`BreadcrumbDropDownHost.cs:402`), and
`CompleteClose` is *scheduled*, not called. `InvalidateAndSchedule` reaches
`BreadcrumbDropDownOpenLifetime.ScheduleInvalidating`, which ends in `ScheduleObserved` →
`RunOnOwnerAsync` → `_uiOperations.PostAsync(...)` — the same `BreadcrumbPopupUiOperations`
dispatcher the coordinator posts its own `SetDroppedDown` and `HandleSelectorOpenStateChanged`
continuations to.

Therefore, when `CloseCore` observes `closed == true` and sets `_closeCompleted = true`, the host
still reports `IsOpen == true` until the queued `CompleteClose` runs. Any `CloseCore` dispatched in
that window reads `hostOpen == true` and now takes the new branch, on the shipped host, with no
substituted implementation.

Concrete consequence in that window. Suppose `CloseCore(Uncommitted)` completes and a second
`CloseCore(ExplicitCommit)` is dequeued before the pending `CompleteClose`:

- The second `_host.Close(ExplicitCommit)` re-enters the `OpenState == true` branch, so it calls
  `InvalidateAndSchedule` again. That bumps the lifetime generation via `InvalidateCore`, which makes
  the first scheduled `CompleteClose(Uncommitted, true)` fail its `IsLifecycleCurrent(lease, ...)`
  check and be skipped; the second, carrying `ExplicitCommit`, runs instead.
- `FinishClose` calls `_cancelSelection()` only when the reason is `Uncommitted`
  (`BreadcrumbDropDownHost.cs:449-451`). So the selection cancellation the pre-change code performed
  would be skipped.

**What I did and did not establish.** I established that the state is occupiable on the production
host and that the reason-substitution consequence follows if a second `CloseCore` is dequeued inside
the window. I did **not** establish that any real user gesture produces that interleaving. The
plausible trigger — the selector-close event that drives `HandleSelectorOpenStateChanged` — appears
to be raised from inside `CloseNative()`/`_cancelSelection()`, which run within `CompleteClose`
itself; if that is always the ordering, the second `CloseCore` is queued *after* the `CompleteClose`
and the window never opens. Settling that requires host-lifecycle analysis this change deliberately
stayed out of, and I am not going to assert either way on static reading alone.

**Why this is non-blocking.** No shipped-path defect is demonstrated. The direction of the change is
toward correctness — a close reaching a host that reports itself open is the behaviour the issue
asks for. The full suite is green. The affected sequence is exercised by no test either before or
after the change, so nothing regressed relative to what was verified.

**What is actually wrong** is the strength of the claim, not the code. The spec answers "can the host
be *reopened* without `RequestOpen`?" and then reports the answer as if it settled "can
`_closeCompleted && IsOpen` both be true in production?". Those are different questions, and the
second one has a different answer. The `<remarks>` on `CloseCore` inherits the same gap: it says
"the host state can change between the read and the lock; both directions are analysed in the spec
... and neither corrupts state", which is a narrower claim about the race window and does not cover
the asynchronous-close window at all.

**Recommendation (follow-up, not a merge condition):** amend the spec's Rollout and Risks sections to
state that the guard's new branch is reachable on the production host inside the
`Close`-returns-before-`CompleteClose`-runs window, and either demonstrate the event ordering that
closes the window or add a regression test whose fake defers `IsOpen = false` the way the real host
does. See CR-3.

### CR-2 — R-1's description of the not-open `Close` branch is inaccurate.

**Severity: Minor. Non-blocking. Not merge-method-dependent.**

`spec.md` R-1 states that a redundant close on an already-closed host is something
"`BreadcrumbDropDownHost.Close` (`:247-257`) handles by returning `false` without closing."

That is not what line 256 does:

```
return _openLifetime.TryCancelPendingOpen(() => CompleteClose(reason, OpenState));
```

`TryCancelPendingOpen` returns `false` only when `_disposed`, or `_openCompletion == null`, or a
close is already pending. Otherwise it invalidates the lifetime, schedules `CompleteClose`, and
returns **true** — and because it sets `_pendingCloseCompletion` first, that `CompleteClose` passes
its `if (!OpenState && !_openLifetime.IsPendingClose) return;` guard and runs `FinishClose(reason)`,
which cancels the selection for an `Uncommitted` reason.

In the specific state R-1 is reasoning about — immediately after a completed close —
`ScheduleInvalidating` has already nulled `_openCompletion`, so `TryCancelPendingOpen` does return
`false` and R-1's *conclusion* survives. But the stated reason is wrong, and a future reader who
applies R-1's rule to a state where an open is pending will reach the wrong answer. Since the
explicit purpose of the recorded analysis is "so a future reader does not re-derive it", an
inaccurate rule is worth correcting.

**Recommendation:** correct the R-1 sentence to name the actual mechanism (`_openCompletion` is null
after a completed close, so `TryCancelPendingOpen` short-circuits) rather than describing `Close` as
unconditionally returning false when not open.

### CR-3 — Line coverage of the new conjunct does not imply coverage of the behaviour that makes it reachable.

**Severity: Minor. Non-blocking. Not merge-method-dependent.**

Every fake host in the suite clears its open state synchronously inside `Close`:

```
QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs:436-437
    if (CloseResult)
        IsOpen = false;
```

The real host does not (CR-1). So the assertion in `evidence/qa-gates/coverage-delta` that "both
outcomes of the new conjunct are covered" is true at the branch level and is correctly evidenced —
`!hostOpen == true` by three standing guards, `!hostOpen == false` by the new test — but it does not
extend to the production timing. The new test reaches `!hostOpen == false` by calling
`harness.Host.SetOpen(true)` explicitly, which models the seam-substitution scenario, not the
asynchronous-close window.

This is the general pattern of a coverage figure being read as stronger evidence than it is: 100
percent line coverage on the changed lines and a 91.18 percent branch rate on the class say the code
was executed, not that the reachable production interleaving was represented.

**Recommendation:** if CR-1 is pursued, the cheapest closing move is a harness variant whose `Close`
returns `true` but defers `IsOpen = false` to the drained queue, then a test asserting the intended
behaviour for an `Uncommitted`-then-`ExplicitCommit` pair.

### CR-4 — Stale line references in `spec.md`.

**Severity: Informational. Non-blocking.**

Several `spec.md` citations use pre-change coordinates:

| Citation in spec.md | Actual post-change location |
|---|---|
| AC-19: `_closeCompleted` docs at `:38-46` | `:38-52` (the added `<remarks>` extends the block) |
| AC-19: `CloseCore` summary at `:302-307` | `:309-314`, with the added `<remarks>` at `:315-323` |
| Scope & Non-Goals: `OpenAsync` calls at `:258-259` | `:265-266` |
| Scope & Non-Goals: `RequestOpen` at `:115`, clears at `:114` | `:111`, clears at `:121` |

Every cited block exists and says what the spec says it says; only the numbers drifted, by exactly
the seven lines the field `<remarks>` added. This is normal for a spec authored before the edit. It
is worth a note only because AC-19 is phrased as a line-range check, and a reader running that check
literally against the post-change file will land slightly off.

### CR-5 — Four pre-existing uncovered lines in the coordinator.

**Severity: Informational. Non-blocking. Pre-existing, not introduced.**

The coordinator's only zero-hit lines are 120, 166, 247 and 330. Line 330 is `return false;` on the
`_released` exit of `CloseCore` — the method this change edits. It was uncovered before the change
and remains uncovered. Nothing in this change made it harder to cover; noted only so that a future
reader of the 98.32 percent figure knows where the gap sits.

## Design assessment

**Is the hoisted read the right design?** Yes, given the constraints. The three candidate shapes
were: clear `_closeCompleted` on the successful-close path (rejected in `issue.md` because it breaks
two standing tests); read `_host.IsOpen` inside the lock (rejected as SR-4 of #501 because it adds a
foreign call under `_sync`); hoist the read (chosen). The chosen shape is the only one that satisfies
both prior constraints, and the reasoning is recorded in-code rather than only in the spec.

**Does the unconditional read create a disposal hazard?** No. The read at line 326 precedes the
`_released` check, so `_host.IsOpen` is touched even after `Release()`. On the sole production
implementation `IsOpen` is `public bool IsOpen => OpenState;` — a plain auto-property read with no
dispose guard, so it cannot throw post-disposal. I verified this directly rather than relying on the
spec's R-3. The residual exposure is to a *future* implementation that throws or counts reads on
`IsOpen`; R-3 records that, which is the appropriate level of treatment.

**Is `_generation++` on the redundant close a problem?** In the CR-1 window the second successful
close increments `_generation` a second time and re-sets `_closeCompleted`. Since no open is in
flight in that state, the extra increment invalidates nothing. It is benign, but it is a second
observable delta that the spec's race analysis does not mention.

## Verdict

The code change is small, correctly shaped, well documented in-code, minimally scoped, and backed by
genuine red-first evidence and a green full suite. Merge is not blocked.

The one finding of substance is CR-1: the delivered artifacts state a stronger unreachability
conclusion than the analysis supports, because the reopen enumeration answers a narrower question
than the one the guard actually depends on. That is a defect in the recorded reasoning, not in the
two changed lines, and it is best resolved by amending the spec and, optionally, adding the deferred
`IsOpen` harness variant described in CR-3.

**Blocking findings: 0.** CR-1 Major non-blocking; CR-2 and CR-3 Minor non-blocking; CR-4 and CR-5
informational.
