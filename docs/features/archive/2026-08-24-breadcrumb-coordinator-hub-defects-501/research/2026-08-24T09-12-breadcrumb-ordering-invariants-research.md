# Breadcrumb coordinator/hub ordering and lifetime defects — implementation research

- Feature: `breadcrumb-coordinator-hub-defects-501` (primary issue #501; also closes #462, #500, #502)
- Research date: 2026-08-24
- Verified against: worktree `TaskMaster-wt/2026-08-23T22-51`, HEAD `988e819b`
- Scope: research only. No production, test, project, or configuration file was written or formatted.
- Toolchain: not run (explicitly out of scope for this task).

---

## 0. Executive summary

All four defects are present on HEAD. Line numbers for #500, #501 and #502 are **exactly** as the
promoted potential documents record them; only #462's file has drifted (+46 lines).

The single most consequential finding is that **the naive #462 fix named in its potential document
(call `ClearClosePending()` on the successful-close path) is not safe**: `_closePending` is doing two
jobs at once, and clearing it on success removes the *repeated-close suppression* that
`BreadcrumbDropDownOpenCoordinatorTests.cs:262-280` asserts by name. Section 6.1 derives a two-flag
split that fixes the reopen defect while preserving every existing assertion.

The second most consequential finding is that **#500's hub half and #501 are one change, not two**:
both rewrite the body of `BreadcrumbMessengerHub.PostJson`, and the natural shape (snapshot under the
lock, broadcast outside it with per-surface containment) satisfies both simultaneously.

The third is that `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs:145-192` already
contains a working, in-suite, deterministic template for the exact structural probe #500 needs
(`Monitor.IsEntered` against a reflected private `_sync` field, asserted from inside a mock callback).
#500 does **not** require an STA message pump.

---

## 1. Ground-truth verification against HEAD `988e819b`

Method: every file below was read end-to-end. Line numbers are from the current working tree.

### 1.1 #462 — `CloseCore` never clears `_closePending` on the successful-close path

**Present. Confirmed.** `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` (355 lines).

| Element | Current line | Potential doc line | Drift |
| --- | --- | --- | --- |
| `CloseCore` body | 283-313 | 237-267 | +46 |
| `_closePending = true` latch | 291 | 245 | +46 |
| `ClearClosePending()` on throw | 300 | 254 | +46 |
| `_host.Close(reason)` outside the lock | 296 | 250 | +46 |
| Successful path `return true` **without** clear | 307 | 261 | +46 |
| `ClearClosePending()` on the not-closed path | 309 | 263 | +46 |
| `RequestOpen` guard `_closePending && _host.IsOpen` | 93-94 | 92-93 | +1 |
| `SetDroppedDown` reopen-without-CloseCore path | 141-147 | 108-112 | +33 |

Current code, `BreadcrumbDropDownOpenCoordinator.cs:303-308`:

```csharp
if (closed)
{
    lock (_sync)
        _generation++;
    return true;
}
```

Every other exit clears the flag; this one does not. `RequestOpen` at `:93-94` reads it:

```csharp
if (_closePending && _host.IsOpen)
    return ClosedTask;
```

and the clear at `:95` (`_closePending = false;`) sits **after** the guard, so it is unreachable once
the guard fires.

**Divergence from the potential document:** none in substance. The "related nearby observations"
(`_host.IsOpen` read under `_sync` at `:93`; `CloseCore` returning `true` for "someone else is
closing" at `:289-290`) are both still accurate.

**One material fact the potential document does not record:** `_closePending` is simultaneously the
*repeated-close suppressor*. `CloseCore:289-290` returns `true` early when `_closePending` is set,
and because the flag latches after a success, a second `CloseCore` on an already-closed host never
reaches `_host.Close`. Two existing tests depend on that suppression (section 3.1). This is why the
literal remediation in the potential document is unsafe as written.

### 1.2 #500 — `TryRunCurrent` invokes the guarded action inside `_sync`

**Present. Confirmed, line-for-line.** `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs`
(309 lines).

```csharp
internal bool TryRunCurrent(BreadcrumbUpgradeLease lease, Action action)   // :133
{
    if (action == null) { throw new ArgumentNullException(nameof(action)); }
    lock (_sync)                                                           // :139
    {
        if (!IsGenerationCurrentCore(lease) || lease.Token.IsCancellationRequested)
        {
            return false;                                                  // :143
        }
        action();                                                          // :145
        return true;                                                       // :146
    }
}
```

The full chain the potential document asserts is verified independently:

1. `BreadcrumbBridgeCoordinator.cs:266-275` — the render post is wrapped by
   `_upgradeLifetime.Guard(lease, ...)` and handed to `_dispatcher.Dispatch`.
2. `BreadcrumbCoordinatorUpgradeLifetime.cs:130` — `Guard` returns `() => TryRunCurrent(lease, action)`.
3. `BreadcrumbCoordinatorUpgradeLifetime.cs:139-147` — `TryRunCurrent` calls `action()` at `:145`
   inside `lock (_sync)`.
4. `BreadcrumbMessengerHub.cs:126` — `PostJson` takes the hub's own `_sync` and, still holding it,
   calls `PostToSurface` at `:133` inside the `foreach` at `:131-134`.
5. `PostToSurface` at `:206` calls `attachment.Messenger.PostJson(json)`. In production that
   messenger is `WebView2Messenger`, whose `PostJson` (`QuickFiler/Viewers/WebView2Messenger.cs:55-69`)
   dispatches through `BreadcrumbUiDispatcher.Dispatch`, which executes **inline** when already on
   the captured boundary (`BreadcrumbUiDispatcher.cs:78-95`) and then calls
   `_coreWebView.PostWebMessageAsJson(json)` at `WebView2Messenger.cs:66`. That is the
   out-of-process WebView2 SDK call, reached under two nested monitors.

Production wiring that makes the messenger the hub is confirmed at
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:53-57` (`new BreadcrumbBridgeCoordinator(lifecycle.Hub, …)`)
and `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:284` (`var hub = new BreadcrumbMessengerHub();`).

Supporting observations verified:

- **No lock inversion.** `BreadcrumbMessengerHub.OnSurfaceMessageReceived` (`:157-173`) snapshots
  `MessageReceived` under `_sync` at `:170` and invokes it outside at `:172`.
- **`RunSynchronous` widens the exposure.** `BreadcrumbCoordinatorUpgradeLifetime.cs:111-122` puts
  the whole `SetSuggestions` body under the lock through `TryRunCurrent`.
- **Re-entrant self-acquisition is routine on the happy path.** `SetSuggestions` produces three
  nested acquisitions on one thread: `TryRunCurrent` `:139` → `IsCurrent` `:105` (reached from
  `BreadcrumbBridgeCoordinator.cs:262`) → `TryRunCurrent` `:139` again (reached from `Guard` `:130`
  through `BreadcrumbUiDispatcher.Dispatch`, which runs inline at `BreadcrumbUiDispatcher.cs:84`).
- **The file contradicts its own convention.** `CancelLease` (`:258-295`), `Complete` (`:240-256`),
  `Abandon` (`:89-101`), `BeginPopulation` (`:48-70`) and `Invalidate` (`:72-87`) all deliberately
  call `lease.Cancel()`, `DisposeLease`, and `_report` **outside** the lock. `:145` is the only
  departure.

### 1.3 #501 — `PostJson` caches before an unguarded broadcast

**Present. Confirmed, line-for-line.** `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` (456 lines).

```csharp
public void PostJson(string json)                       // :119
{
    if (json == null) { throw new ArgumentNullException(nameof(json)); }

    lock (_sync)                                        // :126
    {
        ThrowIfDisposed();                              // :128
        string? type = MessageType(json);               // :129
        CacheState(type, json);                         // :130  cache BEFORE delivery
        foreach (Attachment attachment in _attachments.Values)   // :131
        {
            PostToSurface(attachment, json, type);      // :133  no try/catch anywhere
        }
    }
}
```

Contrast `Attach` at `:82-93`, which wraps its replay and rolls back:

```csharp
try { messenger.MessageReceived += handler; ReplayCachedState(attachment); return true; }
catch { _attachments.Remove(messenger); SafeUnsubscribe(attachment); throw; }
```

The throw source is verified: `WebView2Messenger.PostJson` calls `ThrowIfDisposed()` at
`WebView2Messenger.cs:61`, which throws `ObjectDisposedException` at `:130-136`. The reachable
ordering (dispose the messenger without calling `Detach`) is verified at
`BreadcrumbItemViewerLifecycleCoordinator.cs:270-279` (`DetachCollapsedMessenger`) and `:281-290`
(`DetachPopupMessenger`) — two independently ordered calls, exactly as the potential document states.

**Divergence:** none. The claim that no existing test covers the multi-surface broadcast throw is
re-verified in section 4.3.

### 1.4 #502 — `RunSynchronous` discards `TryRunCurrent`'s `bool`

**Present. Confirmed, line-for-line.**

```csharp
internal void RunSynchronous(BreadcrumbUpgradeLease lease, Action operation)   // :111
{
    try { TryRunCurrent(lease, operation); }                                    // :115  bool discarded
    catch { Abandon(lease); throw; }
}
```

`BreadcrumbBridgeCoordinator.cs:100-115`:

```csharp
BreadcrumbUpgradeLease lease = _upgradeLifetime.BeginPopulation();   // :104
_upgradeLifetime.RunSynchronous(                                     // :105
    lease,
    () =>
    {
        string renderJson = _router.SetSuggestionFallbacks(rows);
        BreadcrumbSelectorState selectorState = _router.GetSelectorState();
        _ = PostRenderAndSelectorAsync(renderJson, selectorState, lease);   // :111
        SuggestionsUpgrade = PopulateSuggestionsAsync(rows, lease);         // :112  inside the lambda
    }
);
```

`AddItems` (`:131-147`) has the same structure with its dispatch task discarded at `:141`, and no
observable handle at all.

**Divergence from the potential document:** none in the cited lines. One **additional defect** in the
same window that the potential document does not record, and that the same fix closes:

> When `TryRunCurrent` returns `false`, `Complete(lease)` is never called for that lease, because the
> only paths that call `Complete` are `RunAsync`'s `finally` (`:175`, `:199`) and `Abandon` (`:100`).
> `Complete` is what sets `lease.Settled = true`, and `CancelLease` disposes the lease's
> `CancellationTokenSource` only when `lease.Settled && !lease.SourceDisposed` (`:285-289`).
> A skipped `RunSynchronous` therefore leaks one `CancellationTokenSource` per superseded population.

This is in scope: both files involved are owned.

---

## 2. Ordering and state-transition invariants (test-assertable form)

### 2.1 #462 — drop-down open/close state machine

Model the coordinator's close state as three mutually exclusive conditions rather than one flag.
Using the current field names (`_closePending` at `:28`, `_generation` at `:27`, `_released` at `:29`,
`_currentOpenTask` at `:26`):

- **I-462.1 (in-flight bound).** `_closePending` is `true` only between the latch at `:291` and the
  completion of the `_host.Close(reason)` call at `:296`. It must be `false` at every point at which
  control leaves `CloseCore`. *Assert:* on any path out of `CloseCore` — success, failure, throw, and
  released — a subsequent probe of the flag reads `false`.
- **I-462.2 (reopen after a successful close).** For a coordinator that is not released: after a
  `CloseCore` that returned `true`, and with `_host.IsOpen == true` reached by any path,
  `RequestOpen()` must return a task that is **not** the `ClosedTask` sentinel and must reach
  `_host.OpenAsync`. *Assert:* `harness.Host.Requests` gains an entry, and the returned task is not
  the already-completed `false` sentinel.
- **I-462.3 (idempotent close, preserved).** Two `CloseCore` calls with no intervening `RequestOpen`
  and no intervening host reopen must reach `_host.Close` exactly once. *Assert:*
  `harness.Host.CloseReasons` has exactly one entry.
- **I-462.4 (generation monotonicity, preserved).** `_generation` increases exactly once per
  successful close (`:306`) and exactly once per `Invalidate` (`:327`). A close that returns `false`
  must not increment it.
- **I-462.5 (released terminality, preserved).** After `Release()`, `RequestOpen()` returns the
  closed sentinel and `CloseCore` returns `false` without touching `_host`.

I-462.2 is the failing-first assertion. I-462.1 and I-462.3 together are the constraint that rules
out the naive fix.

### 2.2 #500 — atomicity and lock-scope

- **I-500.1 (no foreign call under `BreadcrumbCoordinatorUpgradeLifetime._sync`).** At the moment the
  guarded `action` executes, the calling thread must not hold the lifetime's `_sync`.
  *Assert:* `Monitor.IsEntered(lifetimeSync) == false` observed from inside the action, where
  `lifetimeSync` is the reflected private `_sync` field.
- **I-500.2 (no foreign call under `BreadcrumbMessengerHub._sync`).** At the moment
  `IWebViewMessenger.PostJson` is invoked on an attached surface, the calling thread must not hold
  the hub's `_sync`. *Assert:* `Monitor.IsEntered(hubSync) == false` observed from inside a fake
  surface's `PostJson`.
- **I-500.3 (currency claim honesty).** `TryRunCurrent` returns `true` if and only if the lease was
  current **at the moment the action was invoked**. It must not claim more than that. A re-entrant
  `BeginPopulation`/`Invalidate`/`TryDispose` performed *by the action itself* is observable
  afterwards through `IsCurrent(lease) == false`; the return value is not retro-actively falsified.
  *Assert:* an action that calls `lifetime.Invalidate()` still yields `TryRunCurrent == true`, and
  `lifetime.IsCurrent(lease) == false` immediately afterwards.
- **I-500.4 (no re-entrant collection mutation during broadcast).** A re-entrant `Attach` or `Detach`
  performed from inside a surface's `PostJson` must not throw
  `InvalidOperationException: Collection was modified`. *Assert:* the broadcast completes and the
  re-entrant call takes effect.

I-500.3 is deliberately weaker than "the check and the action are atomic". Section 6.2 explains why
the strong form is not achievable with a re-entrant `Monitor` and why claiming it is the actual
defect.

### 2.3 #501 — delivery/cache consistency

- **I-501.1 (no starvation).** For every attachment live at the moment `PostJson` is entered, exactly
  one delivery **attempt** is made, regardless of whether any earlier attempt threw.
  *Assert:* with N attached surfaces, the total attempt count is N.
- **I-501.2 (containment).** A throw from one surface does not prevent delivery to any other surface,
  and does not leave the hub's `_attachments`/`_cachedStates` in a state that differs from the
  no-throw case except for the failed surface's own view.
- **I-501.3 (cache truthfulness).** After `PostJson(json)` returns, a later `Attach` replays a state
  that every surviving surface has already received. Equivalently: the replay cache must never hold a
  state that **no** surface received.
- **I-501.4 (diagnosability).** A delivery failure is not silently discarded; it reaches the
  repository logging pattern (the hub already uses `log4net` at `:269-272`).
- **I-501.5 (`Attach` replay unchanged).** `Attach`'s existing transactional rollback (`:82-93`) is
  not weakened. *Assert:* `BreadcrumbMessengerHubTests.Attach_ReplayFailureRollsBackSubscriptionAndAllowsRetry`
  (`:198-217`) still passes unmodified.

### 2.4 #502 — observability of a superseded population

- **I-502.1 (the skip is reported).** `RunSynchronous` returns `false` when, and only when, the
  guarded action did not run.
- **I-502.2 (`SuggestionsUpgrade` is never stale).** After `SetSuggestions` returns, the value of
  `SuggestionsUpgrade` is either the task created by *this* call or a task that is already completed.
  It is never the task created by an *earlier* call while that task is still incomplete.
  *Assert:* capture `SuggestionsUpgrade` before the call, force the skip, and assert the post-call
  value is not reference-equal to the captured incomplete task.
- **I-502.3 (no lease leak).** Every lease returned by `BeginPopulation` reaches `Settled == true`,
  including a lease whose guarded action was skipped. *Assert:* `lease.Settled == true` and
  `lease.SourceDisposed == true` after the skip.
- **I-502.4 (`AddItems` parity).** The same skip in `AddItems` settles its lease. `AddItems` exposes
  no handle, so no observability obligation beyond I-502.3 applies to it.

---

## 3. Call-chain and blast-radius map

Search scope for every "no other caller" claim below: `rg` over the whole worktree, `--glob '**/*.cs'`,
patterns `CloseCore`, `ClearClosePending`, `RequestOpen`, `TryRunCurrent`, `RunSynchronous`,
`\.Guard\(`, `SetSuggestions\(`, `AddItems\(`, `SuggestionsUpgrade`, `PostRenderAndSelectorAsync`,
`new BreadcrumbMessengerHub`, `new BreadcrumbBridgeCoordinator`.

### 3.1 `BreadcrumbDropDownOpenCoordinator.CloseCore` (private, `:283`)

| Caller | Location | What it observes today | Change under the recommended fix (§6.1) |
| --- | --- | --- | --- |
| `SetDroppedDown` posted body | `BreadcrumbDropDownOpenCoordinator.cs:148` | `bool` discarded | none |
| `HandleSelectorOpenStateChanged` posted body | `:163` | `bool` discarded | none |
| `FinishOpenCore` | `:258` | `bool` discarded | none |

No external caller. The `bool` return is discarded at all three sites, so a return-value change would
be invisible; the observable surface is `_host.Close` call count and `_generation`.

**Tests that exercise `CloseCore` and could break.** All in `QuickFiler.Test/Viewers/`:

| Test | File:line | Asserts | Naive fix (clear on success) | Recommended fix (§6.1) |
| --- | --- | --- | --- | --- |
| `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose` | `BreadcrumbDropDownOpenCoordinatorTests.cs:262-280` | `CloseReasons == [Uncommitted]` after **two** `SetDroppedDown(false)` | **FAILS** — second `CloseCore` now reaches `_host.Close`, producing 2 entries | passes |
| `SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired` | `…Tests.Part2.cs:120-140` | `CloseReasons == [ExplicitCommit]` after **two** `HandleSelectorOpenStateChanged` on a closed selector | **FAILS** — same mechanism | passes |
| `PendingToggleClose_RejectedHostPerformsOneFallbackCancellation` | `…Tests.cs:282-299` | one close, `CancelCount == 1` | passes (`CloseResult=false` never latched) | passes |
| `PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen` | `…Tests.cs:301-318` | one `ExplicitCommit` | passes | passes |
| `SetDroppedDown_CloseThrows_ReportsOnceAndAllowsRetry` | `…Tests.Part2.cs:32-57` | two `Uncommitted` closes, one reported error | passes | passes |
| `ResetReleaseAndCloseResults_PreserveRetryAndBlockReleasedWork` | `…Tests.Part2.cs:142-185` | `CancelCount == 1` across a rejected then accepted close, then reopen after `Reset` | passes | passes |
| `RequestOpen_SelectorClosesBeforeSuccess_ClosesLatePopupExplicitly` | `…Tests.Part2.cs:76-92` | one `ExplicitCommit` from `FinishOpenCore` | passes | passes |
| `AssertDurableActivation` (`explicitCloseAtActivation == 1`, `CloseReasons == [ExplicitCommit]`) | `BreadcrumbSubfolderActivationTests.cs:192-228` | one close through the full `ItemViewer` path | passes — its mock `Close` returns `false` and records nothing when `_hostOpen` is already `false` (`:322-333`) | passes |
| `SetFolderDroppedDownFalse_RequestsOneUncommittedCloseAndRollback` | `BreadcrumbDropDownIntegrationTests.cs:88-107` | `Verify(Close(Uncommitted), Times.Once())` | passes (single close request) | passes |

This table is the core scheduling input: **the naive fix breaks two tests whose names encode the
contract they assert**, so it is not a drop-in.

### 3.2 `BreadcrumbDropDownOpenCoordinator.RequestOpen` (internal, `:85`)

| Caller | Location | Observation change |
| --- | --- | --- |
| `SetDroppedDown` posted body | `:145` | after the fix, a reopen that was previously dropped now proceeds |
| `HandleSelectorOpenStateChanged` posted body | `:161` | same |
| tests | `…Tests.cs:170,171,201,215,228,231,249,252,268,289,307`; `…Part2.cs:65,82,166,176,311`; `…Part3.cs:36,58,65,85,107,130,136,163` | none of these reach the stale-flag state today (each either never closes successfully first, or calls `Reset()`/`Release()` in between, which clears the flag at `:329`) |

No production caller outside the type. `ClearClosePending` (`:315`) has exactly two callers, `:300`
and `:309`, both inside `CloseCore`.

### 3.3 `BreadcrumbCoordinatorUpgradeLifetime.TryRunCurrent` (internal, `:133`)

| Caller | Location | Observes the `bool`? | Change |
| --- | --- | --- | --- |
| `RunSynchronous` | `BreadcrumbCoordinatorUpgradeLifetime.cs:115` | **no** (this is #502) | becomes `return TryRunCurrent(...)` |
| `Guard`'s returned closure | `:130` | no | none |
| tests | none call it directly (`rg TryRunCurrent` → 3 hits, all production) | — | — |

### 3.4 `BreadcrumbCoordinatorUpgradeLifetime.RunSynchronous` (internal, `:111`)

| Caller | Location | Observation change under §6.4 |
| --- | --- | --- |
| `BreadcrumbBridgeCoordinator.SetSuggestions` | `BreadcrumbBridgeCoordinator.cs:105` | consumes the new `bool`; on `false`, replaces `SuggestionsUpgrade` and settles the lease |
| `BreadcrumbBridgeCoordinator.AddItems` | `:135` | consumes the new `bool`; on `false`, settles the lease |
| `BreadcrumbCoordinatorUpgradeLifetimeTests.ArgumentGuards_NullInputsThrowArgumentNullException` | `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs:25` | **compiles unchanged.** `Action a = () => Foo();` is legal when `Foo()` returns `bool` (an invocation is a statement-expression body) |
| `…RunSynchronous_FailureAbandonsLinkedLeaseAndReportsCancellationFailure` | `…Tests.cs:47-48` | same — compiles and passes unchanged |

Changing `void` → `bool` on this `internal` method therefore breaks **no** existing caller.

### 3.5 `BreadcrumbCoordinatorUpgradeLifetime.Guard` (internal, `:124`)

Single production caller: `BreadcrumbBridgeCoordinator.PostRenderAndSelectorAsync:267`. One test
caller: `BreadcrumbCoordinatorUpgradeLifetimeTests.cs:26` (null-argument guard). Moving `action()`
outside the lock changes nothing that `Guard`'s callers observe, because `Guard` already discards the
`bool`.

### 3.6 `BreadcrumbMessengerHub.PostJson` (public, `:119`)

Production callers reach it as `IWebViewMessenger.PostJson` on the coordinator's `_messenger`:

| Caller | Location |
| --- | --- |
| `BreadcrumbBridgeCoordinator.PostRenderAndSelectorAsync` guarded lambda | `BreadcrumbBridgeCoordinator.cs:271` |
| `BreadcrumbBridgeCoordinator.PostSelectorStateCore` | `:318` (guarded by `_messenger is BreadcrumbMessengerHub` at `:297`) |
| `BreadcrumbBridgeCoordinator.SetTheme` | `:233` |
| `BreadcrumbBridgeCoordinator.PublishTransition` | `:282` |
| `BreadcrumbBridgeCoordinator.PublishRouterOutputs` | `:378` |
| `BreadcrumbBridgeCoordinator.Search.cs PublishSearchPresentation` | `BreadcrumbBridgeCoordinator.Search.cs:92` |

Every one of these executes inside `BreadcrumbUiDispatcher.Dispatch`, which catches and reports
(`BreadcrumbUiDispatcher.cs:86-89`). **Consequence:** today a broadcast throw is swallowed by the
dispatcher's error sink rather than surfacing to the viewer. If the fix stops `PostJson` throwing, the
only observable loss is that error-sink entry — which is why I-501.4 requires the hub to log.

Test callers of hub `PostJson`: `BreadcrumbMessengerHubTests.cs:29,30,53,93,148,152,174-177,205,224,302,303,323`;
`BreadcrumbMessengerHubCoverageTests.cs:27,40,79-83,87,117,316`;
`BreadcrumbSelectorCoordinatorTests.cs:198-215` (real hub as coordinator messenger).
**None asserts that `PostJson` propagates a surface throw** (verified: the two `ThrowOnPost` tests,
`BreadcrumbMessengerHubTests.cs:198-217` and `BreadcrumbMessengerHubCoverageTests.cs:316-322`, both
throw during `Attach`-time replay, never during a broadcast). So containing the throw breaks nothing.

### 3.7 `BreadcrumbBridgeCoordinator.SetSuggestions` (public, `:100`)

| Caller | Location | Observation change under §6.4 |
| --- | --- | --- |
| `ItemViewer.FolderSearch.cs:23` (`SetFolderSuggestions`) | production | none on the current path; on a superseded lease, `SuggestionsUpgrade` becomes a completed task instead of a stale one |
| `BreadcrumbBridgeCoordinatorTests.cs:347,366` | test | none |
| `BreadcrumbCoordinatorLifecycleTests.cs:50,52,86,125,129,156,192,232,260,317,345` | test | none — all run on one thread, so the lease is always current at `:105` |
| `BreadcrumbBridgeCoordinatorProbabilityTests.cs:40,80,130` | test | none |
| `BreadcrumbDuplicateIdentityIntegrationTests.cs:153` | test | none |
| `FolderBreadcrumbAssetContractTests.cs:186` | test | none |
| `BreadcrumbSubfolderActivationTests.cs:341-354` (via `SetSuggestionsAsync`) | test | n/a — `SetSuggestionsAsync` (`:89-97`) does not use `RunSynchronous` |

### 3.8 `BreadcrumbBridgeCoordinator.AddItems` (public, `:131`)

Production caller: `ItemViewer.FolderSearch.cs:20` (`SetFolderItems`). Test callers:
`BreadcrumbUiThreadDispatchTests.cs:79`, `BreadcrumbSubfolderActivationTests.cs:87,149`,
`BreadcrumbSelectorCoordinatorTests.cs:184,210,305,351`, `BreadcrumbPendingOpenCloseTests.cs:164`,
`BreadcrumbItemViewerLifecycleCoordinatorTests.cs:143`, `BreadcrumbCoordinatorLifecycleTests.cs:320`,
`BreadcrumbBridgeCoordinatorTests.cs:328`, `BreadcrumbDropDownReadinessTests.cs:364`,
`BreadcrumbBridgeCoordinatorProbabilityTests.cs:75`. All single-threaded; the lease is always current;
none observes a change.

`BreadcrumbSelectorCoordinatorTests.cs:145-192` deserves special mention: it drives `AddItems` and
asserts, via `Monitor.IsEntered` on the **router's** `_sync`, that posts happen outside that lock. The
#500 fix does not touch the router lock, so this test is unaffected — but it is the template for the
new tests (§5).

### 3.9 `BreadcrumbBridgeCoordinator.PostRenderAndSelectorAsync` (private, `:256`)

| Caller | Location | Change |
| --- | --- | --- |
| `SetSuggestions` guarded lambda | `:111` | runs outside the lifetime lock after §6.2 |
| `PopulateSuggestionsAsync` publish callback | `:127` | none (already outside) |
| `AddItems` guarded lambda (through `RunAsync`) | `:143` | runs outside the lifetime lock after §6.2 |
| `BreadcrumbCoordinatorLifecycleTests.PostRenderAndSelectorAsync_StaleLeaseReturnsCompletedWithoutPublishing` | `BreadcrumbCoordinatorLifecycleTests.cs:364-392` (reflection, `BindingFlags.NonPublic`) | none — it asserts the early return at `:262-265`, which is untouched |

### 3.10 Blast radius outside the four owned files

- No owned change alters a **public** signature. `SetSuggestions`, `AddItems`, `SuggestionsUpgrade`
  and `PostJson` keep their shapes; only `RunSynchronous` (internal) gains a return type.
- `QuickFiler/Viewers/WebView2Messenger.cs`, `WebView2BreadcrumbHost.cs` (feature 476) and
  `BreadcrumbItemViewerLifecycleCoordinator.cs`, `BreadcrumbPopupUiOperations.cs`,
  `BreadcrumbDropDownHost.cs`, `ItemViewer.Breadcrumb.cs` (feature 488) are **read-only** for this
  feature and none of the recommended fixes requires editing them. Confirmed by construction: every
  recommended edit is confined to the four owned files plus test files plus, if §7 forces a split, one
  `<Compile Include>` line per project file.
- `QuickFiler.csproj` and `QuickFiler.Test.csproj` are **not** in the owned list and **not** in the
  forbidden list. Section 7 flags the two circumstances that would require touching them.

---

## 4. Existing test inventory and seams

### 4.1 `BreadcrumbDropDownOpenCoordinator` (#462)

| File | Lines | Headroom to 500 | Seams it provides |
| --- | ---: | ---: | --- |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` | 463 | **37** | `CoordinatorHarness` (`:323-372`) and `ControlledHost` (`:374-461`), both `private sealed` nested in the **partial** `[TestClass]` and therefore visible to all three parts |
| `…Tests.Part2.cs` | 381 | **119** | `CountingCoordinatorProbe` (`:330-379`) — counting, individually faultable selector/open/cancel/detach seams |
| `…Tests.Part3.cs` | 173 | **327** | none of its own; scoped by its doc comment to issue #438 |

`ControlledHost` already exposes everything I-462.2 needs:

- `SetOpen(bool)` (`:407`) — drives `IsOpen` without routing through `RequestOpen`/`CloseCore`, which
  is exactly step 3 of the potential document's repro.
- `CloseResult` (`:397`) and `CloseFailure` (`:398`) — force the closed/not-closed/throwing paths.
- `CloseReasons` (`:395-396`) — the I-462.3 counter.
- `Requests` (`:386-387`) — the I-462.2 evidence that `OpenAsync` was reached.
- `Enqueue`/`EnqueueThrow` (`:402-405`) — deterministic open results with no timers.

Ordering is driven by `CapturingSynchronizationContext`
(`QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:346-440`), which queues posts and
drains them explicitly via `DrainOne`/`DrainAll`/`DrainUntil` on the creator thread, throwing if
drained from anywhere else (`:406-409`). Fully deterministic; no sleeps.

**Recommended home for the #462 regression test:** `…Tests.Part2.cs` (119 lines of headroom, generic
stated purpose, and `ControlledHost`/`CoordinatorHarness` are already in scope).

**No test reads `_closePending`.** Search scope: `rg '_closePending|closePending' QuickFiler.Test` →
zero hits.

### 4.2 `BreadcrumbCoordinatorUpgradeLifetime` (#500 mechanism, #502 mechanism)

| File | Lines | Headroom | Seams |
| --- | ---: | ---: | --- |
| `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` | 122 | **378** | `Mock<Action<Exception>>` report sink (`:40-42`); reflection helper `SetCurrentLease` writing `_current` and `_generation` via `BindingFlags.Instance \| BindingFlags.NonPublic` (`:93-105`); `ThrowingCancellationTokenSource` (`:107-120`) |

This is the roomiest and most appropriate file in the suite, and it already establishes the
private-field-reflection precedent the `Monitor.IsEntered` probe needs.

### 4.3 `BreadcrumbMessengerHub` (#501, #500 hub half)

| File | Lines | Headroom | Seams |
| --- | ---: | ---: | --- |
| `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` | 414 | **86** | `TrackingMessenger` (`:364-412`): `Posted`, `SubscriberCount`, `ThrowOnPost`, `Lifecycle`, `Receive`, `ReceiveLastRemoved`, `DisposeCount` |
| `QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs` | 478 | **22 — no room** | a richer `TrackingMessenger` (`:421-476`) with `SubscribeAttempts`, `UnsubscribeAttempts`, `ThrowOnSubscribe`, `ThrowOnUnsubscribe`, `ThrowOnDispose` |

The two `ThrowOnPost` tests confirmed to exercise **`Attach`-time replay only**, never a broadcast:
`BreadcrumbMessengerHubTests.cs:198-217` (`Attach_ReplayFailureRollsBackSubscriptionAndAllowsRetry`)
and `BreadcrumbMessengerHubCoverageTests.cs:302-331`
(`Attachment_ControllerAndHubFailures_ResetAndPermitRetry`, whose `hub.PostJson` at `:316` happens
with no throwing surface attached).

Neither `TrackingMessenger` counts **attempts** on `PostJson` — both only record successes or throw.
I-501.1 needs an attempt counter, so the regression test must add one (a two-line change to the
existing `TrackingMessenger` in `BreadcrumbMessengerHubTests.cs`, or a purpose-built local fake).

`BreadcrumbMessengerHubTests.cs` has 86 lines of headroom — enough for one ~45-line regression test
plus the counter, but not for two.

### 4.4 `BreadcrumbBridgeCoordinator` (#502 observable symptom)

| File | Lines | Headroom | Seams |
| --- | ---: | ---: | --- |
| `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs` | 488 | **12 — no room** | `Harness` (`:71-88`), `Mock<IWebViewMessenger>` with a `Posted` callback (`:122-124`), `InlineSynchronizationContext` (`:90-93`), `CreateContextOwnedCoordinator` (`:95-112`) |
| `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs` | 489 | **11 — no room** | `[TestInitialize]` installing a real `SynchronizationContext` (`:23-30`); hand-written `TrackingMessenger` (`:437-467`); `ViewerScope` (`:469-487`); reflection onto `_upgradeLifetime` (`:370-377`) |
| `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorProbabilityTests.cs` | 168 | **332** | scored-row fixtures; topically about percentage rendering |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` | 434 | **66** | `CreateCoordinator` helper; the `Monitor.IsEntered` lock-scope template at `:145-192` |

The three files most natural for a #502 coordinator-level test are all at or near the cap. See §7.3.

### 4.5 Determinism seams already relied on repo-wide

- `BreadcrumbUiDispatcher.Dispatch` runs **inline** when `IsCurrentBoundary()` holds
  (`BreadcrumbUiDispatcher.cs:78-95`), i.e. when the ambient `SynchronizationContext` is
  reference-equal to the captured one (`:269-272`). Installing a context in `[TestInitialize]` and
  keeping the test on one thread therefore makes every dispatch synchronous.
- `InlineSynchronizationContext` (`BreadcrumbBridgeCoordinatorTests.cs:90-93`) makes even the
  non-inline path synchronous by executing the callback on `Post`.
- `CapturingSynchronizationContext` gives explicit, manual, single-thread drain control.
- **No `Thread.Sleep`, `Task.Delay` or wall-clock wait exists anywhere in `QuickFiler.Test`.**
  Search scope: `rg 'Thread\.Sleep|Task\.Delay|DateTime\.Now' QuickFiler.Test` → four hits, all of
  them either an unrelated `DateTime.Now` field in `MailItemInfoTests.cs:25` or comments explicitly
  stating that no delay is used.

---

## 5. Determinism design — a failing-first test for each defect, with no threading and no timing

### 5.1 #462 — fully reproducible, no new technique needed

Follows the potential document's repro verbatim using `CoordinatorHarness` + `ControlledHost`:

1. `harness.Host.Enqueue(Task.FromResult(true))`; `RequestOpen()`; `DrainUntil` → host open.
2. Drive a successful close through `SetDroppedDown(false)` + `DrainAll()` → `CloseReasons == [Uncommitted]`,
   `Host.IsOpen == false`, and `_closePending` now latched (invisible, but its effect is next).
3. `harness.Host.SetOpen(true)` — reproduces step 3 of the potential document (host open again by a
   path that bypasses `CloseCore`/`RequestOpen`; `ControlledHost.SetOpen` at `:407` is exactly this seam).
4. `harness.SelectorOpen = true`; `harness.Host.Enqueue(Task.FromResult(true))`;
   `Task<bool> reopen = harness.Coordinator.RequestOpen(); harness.Context.DrainUntil(reopen);`
5. **RED assertion (I-462.2):** `harness.Host.Requests.Should().HaveCount(2)` and
   `reopen.Result.Should().BeTrue()`. On HEAD, `Requests` stays at 1 and `reopen` is the completed
   `false` sentinel.
6. **Guard assertion (I-462.3), same test or a sibling:** two consecutive `SetDroppedDown(false)`
   calls still yield exactly one `CloseReasons` entry.

Deterministic: one thread, explicit drain, no timers.

### 5.2 #500 — a structural lock probe, not a message pump

The potential document says constructing a repro "requires a re-entrant STA message pump, which
repository unit-test policy prohibits". That is true only of the *concurrency* framing. The
*lock-scope* framing (I-500.1, I-500.2) is directly and deterministically assertable, and the
repository already does exactly this elsewhere.

**Template that already exists and passes today:**
`QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs:145-192`
(`TransitionPublicationsAndEvents_RunAfterRouterLockIsReleased`) reflects
`FolderBreadcrumbBridgeRouter._sync` out (`:159-165`), then records
`Monitor.IsEntered(routerSync)` from inside the `Mock<IWebViewMessenger>.PostJson` callback (`:174`)
and from a `SelectionChanged` handler (`:179`), asserting both are `false` (`:190-191`).

**#500 lifetime probe (I-500.1).** In `BreadcrumbCoordinatorUpgradeLifetimeTests.cs` (378 lines of
headroom):

- Reflect `BreadcrumbCoordinatorUpgradeLifetime._sync` (same `BindingFlags` the file already uses at
  `:98`).
- `var lease = lifetime.BeginPopulation();`
- `bool heldDuringAction = true; lifetime.TryRunCurrent(lease, () => heldDuringAction = Monitor.IsEntered(sync));`
- **RED:** `heldDuringAction.Should().BeFalse();` — `true` on HEAD, `false` after the fix.

`Monitor.IsEntered` reports whether the **current thread** holds the lock, so this is exact on a
single thread with no synchronization primitives, no threads, and no timing.

**#500 hub probe (I-500.2).** In `BreadcrumbMessengerHubTests.cs`:

- Reflect `BreadcrumbMessengerHub._sync`.
- Attach a fake `IWebViewMessenger` whose `PostJson` records `Monitor.IsEntered(hubSync)`.
- `hub.PostJson(render)`.
- **RED:** the recorded value is `true` on HEAD, `false` after the fix.

**#500 re-entrancy behaviour (I-500.3, I-500.4), deterministic and injected, not threaded.**

- I-500.3: `lifetime.TryRunCurrent(lease, () => lifetime.Invalidate())` — assert the call returns
  `true` (the action *did* run under a current lease) and that `lifetime.IsCurrent(lease)` is `false`
  immediately after. This is the "injectable re-entrant action rather than a real message pump" the
  potential document asks for. Note it is **not** RED on HEAD: it documents the honest contract that
  the fix must preserve, and it is the regression guard against a future attempt to fold a post-action
  currency re-check into the return value (see §8).
- I-500.4: a fake surface whose `PostJson` calls `hub.Attach(otherSurface, …)` re-entrantly. On HEAD
  this throws `InvalidOperationException` ("Collection was modified") because the `foreach` at `:131`
  enumerates the live dictionary under the monitor; after the fix it does not. This is a genuine RED
  assertion for the hub half and requires no threads.

**What cannot be deterministically reproduced, stated plainly.** The *cross-thread* interleaving in
which a second thread mutates `_current` between the check at `:141` and the completion of `action()`
at `:145` cannot be reproduced without a second thread, and it is not the fix's target: moving
`action()` outside the lock does not make that window smaller. The assertable content of #500 is
exactly I-500.1 through I-500.4.

### 5.3 #501 — an order-independent starvation assertion

`Dictionary<TKey,TValue>.Values` enumeration order is not contractual, so a test that attaches
"throwing first, recording second" would silently pass on HEAD if the runtime happened to enumerate
the recording surface first. Use an order-independent shape:

- Attach **two** surfaces, **both** configured to throw on post, **both** incrementing an attempt
  counter *before* throwing.
- `hub.PostJson(render)`.
- **RED (I-501.1):** total attempts `== 2`. On HEAD the first throw aborts the `foreach`, so the total
  is `1` regardless of enumeration order.
- **Companion (I-501.2), order-independent by construction:** attach one throwing surface and one
  recording surface, post, and assert the recording surface's `Posted` contains the payload. Combined
  with the counter assertion above this is unambiguous.
- **I-501.3:** after the post, attach a fresh recording surface and assert the replayed state matches
  what the surviving surface received.
- **I-501.5:** `Attach_ReplayFailureRollsBackSubscriptionAndAllowsRetry` (`:198-217`) is left
  untouched and must still pass.

Deterministic: no threads, no timers, no order dependence.

### 5.4 #502 — split the assertion between the mechanism and the symptom

**Mechanism (I-502.1, I-502.3), fully deterministic**, in `BreadcrumbCoordinatorUpgradeLifetimeTests.cs`:

```
var lifetime = new BreadcrumbCoordinatorUpgradeLifetime(_ => { });
var lease = lifetime.BeginPopulation();
lifetime.Invalidate();                       // supersede, deterministic, same thread
bool ran = true;
bool result = lifetime.RunSynchronous(lease, () => ran = true);   // signature change
```
- assert the guarded action did not run, that `RunSynchronous` reported it, and (I-502.3) that the
  lease reaches `Settled == true` / `SourceDisposed == true`.

On HEAD `RunSynchronous` returns `void`, so this test does not compile until the signature changes.
A compile error is not a test failure, so to satisfy the repository's Bugfix Workflow ("smallest
deterministic test that reproduces the bug", failing before the fix) the **lease-leak** assertion is
the better RED: it compiles against HEAD today and fails, because a skipped `RunSynchronous` never
calls `Complete(lease)` (verified in §1.4). Recommend authoring that one first.

**Symptom (I-502.2) — the honest limitation.** The `SetSuggestions` window between `BeginPopulation`
(`:104`) and `RunSynchronous` (`:105`) has **no in-process seam**: the two statements are adjacent,
nothing is injectable between them, `BreadcrumbCoordinatorUpgradeLifetime` is `sealed` with
non-virtual members, and `_upgradeLifetime` is a `readonly` field assigned in the constructor
(`BreadcrumbBridgeCoordinator.cs:31,56`). Search scope for an existing interleaving:
`rg 'SetSuggestions\(|SuggestionsUpgrade' --glob '**/*.cs'` → 20 test call sites, none of which
supersedes between the two statements.

Three ways forward, in order of preference:

1. **Add an `internal` seam in an owned file.** Split `SetSuggestions` into the public entry point and
   an `internal void SetSuggestionsCore(IReadOnlyList<FolderRow> rows, BreadcrumbUpgradeLease lease)`.
   The test reflects `_upgradeLifetime` out (precedent: `BreadcrumbCoordinatorLifecycleTests.cs:370-377`),
   calls `BeginPopulation()`, `Invalidate()`, then invokes `SetSuggestionsCore` with the dead lease and
   asserts `SuggestionsUpgrade` was replaced. `[assembly: InternalsVisibleTo("QuickFiler.Test")]`
   exists at `QuickFiler/Properties/AssemblyInfo.cs:5`, so no reflection is needed for the call itself.
   Cost: ~4 lines in `BreadcrumbBridgeCoordinator.cs` (see §7.2 for the budget consequence).
2. **Assert through reflection only.** Invoke the private members the way
   `BreadcrumbCoordinatorLifecycleTests.cs:381-386` already invokes `PostRenderAndSelectorAsync`. No
   production change, but a brittler test.
3. **A two-thread handshake.** `BreadcrumbCoordinatorLifecycleTests.cs:350-351` already uses
   `Task.Run(...).GetAwaiter().GetResult()` plus `context.WaitForPost()` (a `SemaphoreSlim`
   wait-handle block), so a handshake-based cross-thread test has repo precedent and violates no
   banned API — `.claude/rules/general-unit-test.md` bans `Thread.Sleep`, `Task.Delay`, wall-clock
   waits and `Date.now()`, none of which a handshake uses. But the handshake would have to fire
   *between two adjacent statements*, which no event can observe. **This option does not actually
   work**; it is listed only so the planner does not spend time rediscovering that.

**Recommendation: option 1.** It is the only route that produces a genuine, non-reflective,
deterministic assertion of I-502.2.

---

## 6. Fix option analysis and recommendations

### 6.1 #462 — recommended: split `_closePending` into in-flight and completed

**Option A — the potential document's literal remediation (clear on the success path).** Add
`ClearClosePending()` (or `_closePending = false` inside the existing `lock` at `:305-306`) before the
`return true` at `:307`.
*Rejected.* It removes the repeated-close suppression that `_closePending` accidentally also provides,
breaking `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose`
(`BreadcrumbDropDownOpenCoordinatorTests.cs:262-280`) and
`SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired` (`…Part2.cs:120-140`). Both encode a
deliberate contract in their names; silently rewriting them would widen the behaviour change well
beyond what #462 asks for.

**Option B — gate `CloseCore` on `!_host.IsOpen`.**
*Rejected.* `PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen`
(`…Tests.cs:301-318`) proves that closing while `_host.IsOpen == false` is required behaviour (the
open is still pending), and `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose`
performs its *first* close in exactly that state. An `IsOpen` gate suppresses closes that must happen.
It also re-introduces reading `_host.IsOpen` under `_sync`, the hazard the potential document flags.

**Option C — track the generation at which a close completed.** Add `_closedAtGeneration` and suppress
when it equals `_generation`.
*Rejected.* `_generation` is incremented **by the successful close itself** (`:306`) and is *not*
incremented by `RequestOpen`, so `_closedAtGeneration` would still equal `_generation` when a new open
begins, suppressing the close of the new open. It works only with an extra reset in `RequestOpen`,
which is strictly more state than option D for the same result.

**Option D — RECOMMENDED. Two flags with distinct, documented meanings.**

- `_closeInFlight` (replaces `_closePending`): `true` only while `_host.Close(reason)` at `:296` is
  executing. Cleared on every exit — ideally in a `finally` around the `_host.Close` call, which also
  removes the duplicated `ClearClosePending()` at `:300` and `:309`.
- `_closeCompleted` (new): `true` after a close that returned `true`. Cleared by `RequestOpen` at the
  point that already clears the flag (`:95`) and by `Invalidate` at `:329`.

`CloseCore` becomes: `if (_released) return false;` → `if (_closeInFlight) return true;` →
`if (_closeCompleted) return true;` → latch `_closeInFlight`. `RequestOpen`'s guard at `:93` becomes
`if (_closeInFlight && _host.IsOpen) return ClosedTask;` — which now means exactly what it says.

Verification against every existing assertion is in the §3.1 table: **option D passes all nine**, and
satisfies I-462.1 through I-462.5.

*Residual, worth recording in `spec.md` but not fixing here:* if the host is reopened by a path that
never reaches `RequestOpen`, `_closeCompleted` stays `true` and a subsequent close request would
return `true` without closing. A refinement `if (_closeCompleted && !_host.IsOpen) return true;` also
passes every existing test and removes the residual, at the cost of reading `_host.IsOpen` under
`_sync`. The minimal form is recommended; the refinement is the fallback if review prefers it.

*Observable behaviour change:* one, and it is the point of the issue — a `RequestOpen` after a
successful close now opens instead of silently returning the closed sentinel.

### 6.2 #500 (lifetime half) — recommended: invoke the action outside the lock, return the entry-time verdict

**Option A — RECOMMENDED. Move `action()` outside `lock (_sync)`.** Under the lock, evaluate currency
and capture the verdict; release; invoke the action; return the captured verdict.

```
bool current;
lock (_sync) { current = IsGenerationCurrentCore(lease) && !lease.Token.IsCancellationRequested; }
if (!current) { return false; }
action();
return true;
```

- Satisfies I-500.1 directly.
- Matches the convention the same file already follows in five places (§1.2).
- Keeps the `bool` meaning "the action was invoked", which is precisely what #502 needs (§8).
- Breaks no existing test: `RunSynchronous_FailureAbandonsLinkedLeaseAndReportsCancellationFailure`
  (`BreadcrumbCoordinatorUpgradeLifetimeTests.cs:36-56`) still sees the throw propagate and `Abandon`
  called from `RunSynchronous`'s `catch`.
- **Documented consequence:** two threads could now both pass the currency check and run their actions
  concurrently, where the monitor previously serialized them. In production every guarded action runs
  on the captured `BreadcrumbUiDispatcher` boundary, and `RunSynchronous` is reached only from
  `SetSuggestions`/`AddItems` on the viewer thread, so this is not reachable on current wiring. Record
  it explicitly in `spec.md` rather than leaving it implicit.

**Option B — move the action out **and** fold a post-action currency re-check into the return value.**
*Rejected.* It makes `false` ambiguous ("did not run" vs "ran but was superseded"), which directly
breaks the #502 fix: `SetSuggestions` would overwrite `SuggestionsUpgrade` with a completed task
*after* the guarded lambda had already assigned the real one. If a post-action verdict is ever wanted,
it must be a separate `out` parameter or a separate `IsCurrent(lease)` call by the caller.

**Option C — make the guard non-re-entrant (a `[ThreadStatic]` or explicit "in guarded action" flag
that rejects re-entrant mutation).**
*Rejected for this feature.* It is the only design that would deliver true atomicity, but it converts
a currently-silent re-entrant mutation into a throw or a no-op on a path that
`BreadcrumbUiDispatcher`'s inline execution already makes routine (§1.2, third supporting
observation). That is a much larger behaviour change than #500 scopes, and it would need its own
issue.

### 6.3 #501 + #500 (hub half) — recommended: one combined rewrite of `PostJson`

These are **one change**. The shape that satisfies I-500.2, I-500.4 and I-501.1 through I-501.5
simultaneously:

1. Under `lock (_sync)`: `ThrowIfDisposed()`; compute `type`; `CacheState(type, json)`; **snapshot**
   `_attachments.Values` into a local array.
2. Release the lock.
3. Iterate the snapshot outside the lock, wrapping each `PostToSurface` in its own `try`/`catch` that
   logs through the existing `log4net` logger (the pattern is already in the file at `:267-272`,
   `SafeUnsubscribe`) and continues to the next attachment.

**Candidate fixes from the #501 potential document, assessed:**

| Candidate | Verdict | Rationale |
| --- | --- | --- |
| Catch per surface and continue | **RECOMMENDED** | Directly satisfies I-501.1/I-501.2. Minimal. Combines cleanly with the lock narrowing. |
| Defer the cache write until after a successful broadcast | Rejected | If any surface throws, the cache retains the *previous* state, so a later `Attach` replays something older than what the surviving surfaces already have — that violates I-501.3 more severely than the current behaviour does. |
| Auto-detach a throwing surface | Rejected | Most invasive; a transient failure permanently drops a live surface; and mutating `_attachments` during the broadcast is exactly the hazard I-500.4 identifies. |

**Where the cache write belongs.** Keep it inside the lock (it mutates `_cachedStates` and `_sequence`
at `:190`) and before the broadcast. Once the broadcast is contained, every live surface receives the
message, so the cache claim is true for all of them; the one surface that threw is stale by its own
failure, and no rollback can repair that without making the other surfaces stale too.

**Should `PostJson` still throw?** Recommend **no** — swallow-and-log per surface. Justification: all
six production callers (§3.6) already run inside `BreadcrumbUiDispatcher.Dispatch`, which catches and
routes to the error sink (`BreadcrumbUiDispatcher.cs:86-89`), so the throw is not currently surfacing
to a user; and no existing test asserts propagation (§4.3). The log call preserves diagnosability
(I-501.4). If review prefers propagation, the alternative is to collect failures and throw an
`AggregateException` **after** the loop — that also satisfies I-501.1, and would break no existing
test, but changes the exception type observed by the dispatcher's sink.

**Observable behaviour changes:** (a) a broadcast throw no longer propagates out of `PostJson`;
(b) surfaces after the failing one now receive the message; (c) `PostJson` no longer holds `_sync`
while calling into a surface, so a re-entrant `Attach`/`Detach`/`PostJson` from a surface callback no
longer throws or deadlocks.

### 6.4 #502 — recommended: surface the `bool` **and** use it at both call sites

**Option A — surface `TryRunCurrent`'s `bool` through `RunSynchronous`.** `internal bool RunSynchronous(...)`,
`return TryRunCurrent(lease, operation);` inside the existing `try`.
*Adopt.* No caller breaks (§3.4). This is the general mechanism fix and the only one that also helps
`AddItems`.

**Option B — hoist the `SuggestionsUpgrade` assignment out of the guarded lambda.**
*Rejected on its own.* It would call `PopulateSuggestionsAsync(rows, lease)` with rows that were never
applied to the router (`_router.SetSuggestionFallbacks(rows)` at `:109` is inside the skipped lambda).
The resulting task completes immediately — `RunAsync`'s `lease.Token.ThrowIfCancellationRequested()`
at `:165` throws and is swallowed by the `when (!IsGenerationCurrent(lease))` filter at `:169` — so it
is harmless, but it is semantically incoherent and leaves `AddItems` untouched.

**Option C — RECOMMENDED = A, plus a `false` branch at both call sites.**

In `SetSuggestions`: when `RunSynchronous` returns `false`, set `SuggestionsUpgrade = Task.CompletedTask`
(satisfying I-502.2 with the "completed task" outcome the potential document explicitly accepts) and
call `_upgradeLifetime.Abandon(lease)` to settle the unused lease (I-502.3). `Abandon` (`:89-101`) is
safe here: it bumps `_generation` only when the lease is still `_current` (`:93-97`), which a
superseded lease is not.

In `AddItems`: on `false`, call `_upgradeLifetime.Abandon(lease)` only. No handle exists, so nothing
further is observable; record the deliberate discard in an XML comment so the skip is intentional
rather than accidental.

*Why `Task.CompletedTask` rather than `Task.FromCanceled`:* eleven existing tests call
`SuggestionsUpgrade.GetAwaiter().GetResult()` (§3.7), which would throw on a cancelled task. None of
them reaches the superseded branch today, so either choice compiles and passes — but a completed task
matches the property's declared initial value (`BreadcrumbBridgeCoordinator.cs:118`) and cannot
surprise a future caller.

*Observable behaviour changes:* `RunSynchronous` gains a return value (internal); `SuggestionsUpgrade`
is replaced rather than left stale on a superseded population; one `CancellationTokenSource` per
skipped population is now disposed instead of leaked.

---

## 7. File-size budget (500-line cap, `.claude/rules/general-code-change.md`)

Line counts are exact (`rg -c '.*'`, which counts every line including blanks; cross-checked against
the `cat -n` line numbers from a full read of each file).

### 7.1 Owned production files

| File | Current | Est. added | Est. after | Verdict |
| --- | ---: | ---: | ---: | --- |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | 355 | +10 to +14 (one field, two guard lines, a `finally`, comments) | ~369 | safe |
| `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | 309 | +8 to +12 (`TryRunCurrent` restructure, `RunSynchronous` return, XML comments) | ~321 | safe |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 456 | +14 to +18 (snapshot local, per-surface `try`/`catch`, log call, comments) | ~474 | safe but tight |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 487 | +13 to +17 (`SetSuggestions` false-branch ≈ +9, `AddItems` false-branch ≈ +4, optional `SetSuggestionsCore` seam ≈ +4) | **~500-504** | **BREACH RISK** |

### 7.2 `BreadcrumbBridgeCoordinator.cs` — the one real budget problem

Three mitigations, in order of preference:

1. **New partial part `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs`.** The class is
   already `public sealed partial` (`BreadcrumbBridgeCoordinator.cs:25`) and the repository already
   established this exact precedent: `BreadcrumbBridgeCoordinator.Search.cs` (102 lines) states in its
   own doc comment (`:10-13`) that it exists "so `BreadcrumbBridgeCoordinator.cs` (487 lines) stays
   clear of the repository's 500-line ceiling". Move `SetSuggestions` (`:99-115`),
   `SuggestionsUpgrade` (`:117-118`), `PopulateSuggestionsAsync` (`:120-128`) and `AddItems`
   (`:130-147`) — about 50 lines — leaving the primary file near 437 with ample headroom.
   **Cost: one `<Compile Include="Viewers\BreadcrumbBridgeCoordinator.Suggestions.cs" />` line in
   `QuickFiler/QuickFiler.csproj`, inserted after line 392.** `QuickFiler.csproj` is neither owned nor
   forbidden by the feature's ownership boundary; the spec author must confirm this is acceptable.
2. **Move the same members into the existing `BreadcrumbBridgeCoordinator.Search.cs`** (102 lines,
   398 lines of headroom, already compiled). No project-file edit at all. Cost: the file's stated
   purpose ("the folder-search presentation composite") becomes inaccurate, and `.Search.cs` is not in
   the owned-file list either.
3. **Keep the edit under 12 added lines** (drop the `SetSuggestionsCore` seam, compress comments) to
   land at ≤499. Cost: no seam, so #502's coordinator-level test falls back to reflection (§5.4
   option 2), and the file is left with single-digit headroom for the next change.

**Recommendation: option 1.** It is the pattern the repository already chose for this exact file and
this exact reason.

### 7.3 Test files

| File | Current | Headroom | Suitable for |
| --- | ---: | ---: | --- |
| `BreadcrumbCoordinatorUpgradeLifetimeTests.cs` | 122 | 378 | #500 lifetime lock probe, #500 re-entrancy contract, #502 mechanism + lease-leak RED |
| `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | 381 | 119 | #462 reopen RED + repeated-close guard |
| `BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` | 173 | 327 | alternative #462 home (doc comment is scoped to #438; would need updating) |
| `BreadcrumbMessengerHubTests.cs` | 414 | 86 | #501 starvation RED + #500 hub lock probe (tight — budget ~45 and ~25 lines respectively; if both do not fit, the hub lock probe moves out) |
| `BreadcrumbSelectorCoordinatorTests.cs` | 434 | 66 | fallback home for the #500 hub lock probe (its `Monitor.IsEntered` template lives here) |
| `BreadcrumbDropDownOpenCoordinatorTests.cs` | 463 | 37 | shared harness only — do not add tests |
| `BreadcrumbMessengerHubCoverageTests.cs` | 478 | 22 | **no room** |
| `BreadcrumbBridgeCoordinatorTests.cs` | 488 | 12 | **no room** |
| `BreadcrumbCoordinatorLifecycleTests.cs` | 489 | 11 | **no room** |

**#502's coordinator-level test has no home in an existing file.** A new test file is required:
`QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs`. The feature's own
`issue.md:57-61` explicitly sanctions this ("A genuinely new test file requires a `Compile Include`
added only within the alphabetical `Breadcrumb*` neighbourhood of the item group at
`QuickFiler.Test/QuickFiler.Test.csproj` lines 57-175").

**Correction to `issue.md:59-61`:** the `Breadcrumb*` entries in that item group are **not**
alphabetically ordered. Verified at `QuickFiler.Test/QuickFiler.Test.csproj:58-95`: the sequence runs
`BreadcrumbBridgeCoordinatorTests` (60), `BreadcrumbBridgeCoordinatorProbabilityTests` (61),
`BreadcrumbCoordinatorLifecycleTests` (62), … `BreadcrumbPopupUiOperationsDirectAdapterTests` (65),
`BreadcrumbUiThreadDispatchTests` (66), `BreadcrumbSelectorToggleUiBoundaryTests` (67). The practical
guidance is "insert adjacent to the sibling test for the same production type" (i.e. after line 60 for
a `BreadcrumbBridgeCoordinator` test), which minimises merge conflict surface with sibling children
just as effectively.

`[assembly: InternalsVisibleTo("QuickFiler.Test")]` is present at
`QuickFiler/Properties/AssemblyInfo.cs:5`, so a new test file can reach `internal` members directly.

---

## 8. Interaction between the four fixes

### 8.1 #500 and #502 both change `BreadcrumbCoordinatorUpgradeLifetime` — they compose

They touch **different methods**:

- #500 rewrites the body of `TryRunCurrent` (`:133-148`).
- #502 changes the signature and `return` of `RunSynchronous` (`:111-122`) and adds `false` branches
  in `BreadcrumbBridgeCoordinator`.

There is **no textual conflict** and **no required ordering**, but there is one **semantic
constraint** that binds them:

> `TryRunCurrent`'s `bool` must continue to mean *"the action was invoked"* — i.e. the currency verdict
> taken at entry, **not** a verdict recomputed after the action returns.

If #500 is implemented as option B of §6.2 (fold a post-action re-check into the return value), then
#502's `false` branch in `SetSuggestions` fires *after* the guarded lambda has already assigned
`SuggestionsUpgrade`, and the fix overwrites a live handle with `Task.CompletedTask` — turning #502's
remedy into a new instance of #502's own defect. **Implement §6.2 option A.** Recording this constraint
in `spec.md` as an explicit NFR is worthwhile; a test for it is proposed as I-500.3 (§5.2).

Sequencing recommendation: implement #500's `TryRunCurrent` change first, then #502's `RunSynchronous`
change, because the second reads more naturally against the already-restructured first. Either order
compiles.

### 8.2 #500's hub narrowing and #501's broadcast containment are **one** change

Both rewrite the body of `BreadcrumbMessengerHub.PostJson` (`:119-136`), and they are not separable:
containing the throw *inside* the existing `lock` would leave I-500.2 unsatisfied, and narrowing the
lock *without* containment would leave I-501.1 unsatisfied. The combined shape in §6.3 satisfies both
in a single edit. Plan them as one task with two acceptance criteria, not two tasks against the same
method.

### 8.3 #462 is fully independent

`BreadcrumbDropDownOpenCoordinator.cs` shares no member, field, or call path with the other three. Its
only coupling to the feature is the shared 500-line budget and the shared test project. It can be
implemented, reviewed, and merged in any position in the sequence.

### 8.4 Recommended task ordering for the atomic plan

1. #462 (independent, self-contained, one file + one test file).
2. #500 lifetime half (`TryRunCurrent`) + its lock probe and re-entrancy contract tests.
3. #502 (`RunSynchronous` bool + both call sites), which depends on step 2's return-value semantics
   being settled.
4. #500 hub half + #501, as one combined `PostJson` task.
5. If §7.2 option 1 is taken, the `BreadcrumbBridgeCoordinator.Suggestions.cs` split must precede
   step 3 (or be folded into it) so the file never crosses 500 mid-plan.

---

## 9. Open questions for the spec author

1. **Project-file ownership.** §7.2 option 1 and §7.3's new test file each require one `<Compile Include>`
   line — in `QuickFiler/QuickFiler.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj` respectively.
   Neither project file is in this feature's owned list, and neither is in its forbidden list.
   `issue.md:57-61` already sanctions the test-project edit; the production-project edit needs an
   explicit ruling.
2. **`.Search.cs` ownership.** If §7.2 option 2 is preferred instead,
   `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs` would be written. It is a partial part of
   an owned type but is not itself named in the owned list.
3. **Should `PostJson` still propagate a surface throw?** §6.3 recommends swallow-and-log. The
   alternative (aggregate and throw after the loop) equally satisfies I-501.1 and breaks no existing
   test; the choice is a product decision about whether the viewer's error sink should keep seeing it.
4. **`_closeCompleted` residual (§6.1).** Minimal form, or the `&& !_host.IsOpen` refinement that
   removes the residual at the cost of an `IsOpen` read under `_sync`?
5. **`SetSuggestionsCore` seam.** Adding it makes #502's I-502.2 test non-reflective, at the cost of
   ~4 production lines and a slightly wider internal surface. Worth it?

---

## 10. Evidence index

Every claim above is grounded in a file read or a `rg` search performed during this session. The
principal sources:

- `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` (355 lines, read in full)
- `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` (309 lines, read in full)
- `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` (456 lines, read in full)
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` (487 lines, read in full)
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs` (102 lines, read in full)
- `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` (285 lines, read in full)
- `QuickFiler/Viewers/WebView2Messenger.cs:40-147` (read; not written)
- `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:240-328` (read; not written)
- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:49-57,270-309` (read; not written)
- `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` (463), `.Part2.cs` (381), `.Part3.cs` (173) — all read in full
- `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` (122, read in full)
- `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` (414, read in full)
- `QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs` (478, read in full)
- `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs` (488, read in full)
- `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs:1-120,300-489`
- `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs:140-209`
- `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:346-431`
- `QuickFiler.Test/Viewers/BreadcrumbSubfolderActivationTests.cs:125-360,413-423`
- `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs:120-239`
- `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs:70-119`
- `QuickFiler/QuickFiler.csproj:290-410`, `QuickFiler.Test/QuickFiler.Test.csproj:58-95`
- `QuickFiler/Properties/AssemblyInfo.cs:5`
- Line counts: `rg -c '.*' --glob '**/Breadcrumb*.cs'` (84 files, two pages)
- Absence checks: `rg '_closePending|closePending' QuickFiler.Test` → 0;
  `rg 'Thread\.Sleep|Task\.Delay|DateTime\.Now' QuickFiler.Test` → 4 hits, none a real wait;
  `rg 'Monitor\.IsEntered'` repo-wide → 2 production-test hits, both templates cited above.
