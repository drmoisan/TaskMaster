# breadcrumb-coordinator-hub-defects (Spec)

- **Issue:** #501 (also closes #462, #500, #502)
- **Parent (optional):** epic `quickfiler-bug-family`
- **Owner:** drmoisan
- **Last Updated:** 2026-08-24T10-05
- **Status:** Ready for planning
- **Version:** 1.0
- **Work Mode:** `full-bug` — this file is the authoritative acceptance-criteria source. There is no
  `user-story.md`: the change is a four-defect internal correctness fix with no new user-facing
  capability.
- **Primary evidence:** `docs/features/active/breadcrumb-coordinator-hub-defects-501/research/2026-08-24T09-12-breadcrumb-ordering-invariants-research.md`
  (verified against HEAD `988e819b`). All file:line references in this spec are the research
  document's HEAD-verified numbers, not the older numbers carried by the promoted potential
  documents.

## Context

- **Summary.** Four defects in the QuickFiler breadcrumb coordinator and messenger hub share one root
  theme: an ordering or lifetime invariant that the code states but does not enforce. Each failure is
  silent — no exception reaches a user, no log entry is written, and no existing test observes it.
  All four are confirmed present on HEAD `988e819b`.
- **Observed environment(s).** QuickFiler VSTO add-in, .NET Framework 4.8.1, WebView2-hosted
  breadcrumb surfaces (collapsed and popup). Defects are in host-neutral coordinator code and are
  reachable in every environment the add-in runs in.
- **Customer impact and severity.**
  - #462 (Medium): after a successful drop-down close, a legitimate reopen request is silently
    dropped. The user presses the control and nothing happens.
  - #500 (Medium): an out-of-process WebView2 `PostWebMessageAsJson` call is made while the calling
    thread holds two nested re-entrant monitors. No deadlock is reachable on current wiring, but the
    lock scope is a latent hazard and the currency claim it appears to guarantee is not real.
  - #501 (Medium): when one attached surface throws during a broadcast, every later attachment in the
    enumeration receives nothing, while the replay cache records the message as delivered. A surface
    silently falls behind and a later `Attach` replays a state that surface never saw.
  - #502 (Low): a superseded population silently skips its guarded work and leaves
    `SuggestionsUpgrade` pointing at a stale, still-incomplete task. A companion leak (below) disposes
    no `CancellationTokenSource` for the skipped lease.
- **First observed date and version(s) impacted.** Discovered by static audit rather than a field
  report; recorded as potential documents on 2026-08-07 (#462) and 2026-08-08 (#500, #501, #502).
  Present on every build that contains the current shape of these four files, including HEAD
  `988e819b`.

## Repro & Evidence

Frequency for all four: **deterministic** given the stated ordering. None is timing-dependent, and
every one is reproducible on a single thread with injected seams (see `## Test Strategy`).

### #462 — reopen silently dropped after a successful close

1. Open the drop-down: `RequestOpen()` with the host returning `true`.
2. Drive a successful close (`SetDroppedDown(false)`), which reaches `_host.Close(reason)` and
   returns `true` from `CloseCore`.
3. The host becomes open again by a path that reaches neither `CloseCore` nor `RequestOpen`
   (`ControlledHost.SetOpen(true)` is the in-suite seam for this).
4. Call `RequestOpen()`.

- **Expected:** `RequestOpen` reaches `_host.OpenAsync` and returns a task that can complete `true`.
- **Actual:** `RequestOpen`'s guard at `BreadcrumbDropDownOpenCoordinator.cs:93-94` sees the still-set
  `_closePending` and returns the `ClosedTask` sentinel. `Requests` stays at one entry.
- **Silence:** the sentinel is a normal completed `false` task. No exception, no log, no error sink
  entry.

### #500 — WebView2 post reached under two nested monitors

1. `BreadcrumbBridgeCoordinator.SetSuggestions` calls `RunSynchronous`, which calls `TryRunCurrent`,
   which takes `BreadcrumbCoordinatorUpgradeLifetime._sync` at `:139`.
2. The guarded action runs at `:145`, still inside that lock.
3. It reaches `BreadcrumbMessengerHub.PostJson`, which takes the hub's own `_sync` at `:126`.
4. Still holding both, `PostToSurface` at `:133` calls `attachment.Messenger.PostJson(json)`, which in
   production is `WebView2Messenger.PostJson` (`WebView2Messenger.cs:55-69`). Its
   `BreadcrumbUiDispatcher.Dispatch` executes **inline** when already on the captured boundary
   (`BreadcrumbUiDispatcher.cs:78-95`), so `_coreWebView.PostWebMessageAsJson(json)` at
   `WebView2Messenger.cs:66` — an out-of-process call — is made under both monitors.

- **Expected:** no foreign or out-of-process call is made while either `_sync` is held.
- **Actual:** both are held.
- **Silence:** nothing observable fails today. The exposure is structural.

### #501 — one throwing surface starves the rest while the cache claims delivery

1. Attach two or more surfaces to `BreadcrumbMessengerHub`.
2. Dispose one messenger without calling `Detach` — a reachable ordering, confirmed at
   `BreadcrumbItemViewerLifecycleCoordinator.cs:270-279` and `:281-290`, which detach the collapsed
   and popup messengers through two independently ordered calls.
3. Call `hub.PostJson(json)`.

- **Expected:** exactly one delivery attempt per live attachment, and a replay cache that reflects
  what the surviving surfaces actually received.
- **Actual:** `CacheState` runs at `:130` before the broadcast; `WebView2Messenger.PostJson`'s
  `ThrowIfDisposed()` (`WebView2Messenger.cs:61`, throwing at `:130-136`) aborts the unguarded
  `foreach` at `:131-134`; every attachment after the failing one in `Dictionary.Values` order gets
  nothing, and the cache records the message as delivered.
- **Silence:** all six production callers run inside `BreadcrumbUiDispatcher.Dispatch`, which catches
  and routes to the error sink (`BreadcrumbUiDispatcher.cs:86-89`), so the throw never reaches the
  viewer, and the starved surfaces produce no signal at all.

### #502 — superseded lease silently skips the guarded action

1. `SetSuggestions` calls `BeginPopulation()` at `BreadcrumbBridgeCoordinator.cs:104`.
2. The lease is superseded (a competing `BeginPopulation` or an `Invalidate`).
3. `RunSynchronous` at `:105` calls `TryRunCurrent`, which returns `false` at
   `BreadcrumbCoordinatorUpgradeLifetime.cs:143`; `RunSynchronous` discards the value at `:115`.

- **Expected:** the caller learns the work was skipped and replaces the stale handle.
- **Actual:** the guarded lambda never runs, so `SuggestionsUpgrade = PopulateSuggestionsAsync(...)`
  at `:112` never executes and the property keeps the previous call's still-incomplete task.
- **Companion defect, same window:** because `Complete(lease)` is reached only from `RunAsync`'s
  `finally` (`:175`, `:199`) and from `Abandon` (`:100`), a skipped `RunSynchronous` never settles the
  lease. `lease.Settled` stays `false`, so `CancelLease`'s disposal condition
  `lease.Settled && !lease.SourceDisposed` (`:285-289`) never holds and the lease's
  `CancellationTokenSource` is never disposed — one leak per superseded population.
- **Silence:** `RunSynchronous` returns `void`. Nothing at any call site can observe the skip.

## Scope & Non-Goals

### In scope

Production files (all four are on this feature's owned list in `issue.md`):

- `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` (#462)
- `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` (#500 lifetime half, #502 mechanism)
- `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` (#500 hub half + #501, one combined edit)
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` (#502 call sites)

New production file:

- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs` — a new partial part of the owned
  type (SR-1, see `## Proposed Fix` → Design Decisions).

Project files — exactly two added lines, no other edit:

- `QuickFiler/QuickFiler.csproj`: one
  `<Compile Include="Viewers\BreadcrumbBridgeCoordinator.Suggestions.cs" />`.
- `QuickFiler.Test/QuickFiler.Test.csproj`: one
  `<Compile Include="Viewers\BreadcrumbBridgeCoordinatorSupersessionTests.cs" />`.

Test files: the regression tests enumerated in `## Test Strategy`, plus the one new test file
`QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs`.

### Out of scope / non-goals

- **Research §6.2 option C — a non-re-entrant guard.** Making the upgrade-lifetime guard reject or
  no-op on re-entrant mutation is the only design that delivers true check/action atomicity, but it
  converts a currently-routine re-entrant path (`BreadcrumbUiDispatcher`'s inline execution produces
  three nested self-acquisitions on the `SetSuggestions` happy path) into a throw or a silent no-op.
  That is a far larger behaviour change than #500 scopes. It needs its own issue and is not planned
  here.
- **The cross-thread interleaving of #500.** A second thread mutating `_current` between the currency
  check and the completion of the action cannot be reproduced without a second thread, and moving the
  action outside the lock does not narrow that window. The assertable content of #500 is exactly
  I-500.1 through I-500.4.
- **Rewriting or relaxing any existing test to accommodate a fix.** Three named tests must pass
  unmodified (see `## Acceptance Criteria`); a fix that requires editing them is the wrong fix.

### Explicitly excluded systems and files

These are owned by sibling epic children and must not be written by this feature:

- `QuickFiler/Viewers/WebView2Messenger.cs`, `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` — sibling
  feature 476.
- `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`,
  `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`,
  `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`, `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` —
  sibling feature 488.

### Cross-feature notes (recorded, not planned)

The research confirms by construction that **no recommended fix requires editing any excluded file**;
every recommended edit is confined to the four owned files, the new partial part, the two project-file
lines, and test files. The following are therefore recorded as cross-feature notes only:

1. **`WebView2Messenger.cs` (feature 476).** Its `ThrowIfDisposed()` at `:61` is the throw source for
   #501, and its inline `Dispatch` at `WebView2Messenger.cs:55-69` is the last hop of #500's nested-lock
   chain. Both are read-only evidence here. If feature 476 makes `PostJson` on a disposed messenger a
   no-op rather than a throw, #501's containment becomes redundant but not incorrect; the tests in this
   feature use fake surfaces and are unaffected either way.
2. **`BreadcrumbItemViewerLifecycleCoordinator.cs` (feature 488).** `DetachCollapsedMessenger`
   (`:270-279`) and `DetachPopupMessenger` (`:281-290`) are the two independently ordered calls that
   make #501's dispose-without-detach ordering reachable. Tightening that ordering would remove one
   trigger for #501 but not the underlying starvation; it belongs to feature 488.
3. **`ItemViewer.Breadcrumb.cs` (feature 488).** Confirms the production wiring that makes the hub the
   coordinator's messenger (`:53-57`, `:284`). Read-only.
4. **`BreadcrumbDropDownHost.cs` / `BreadcrumbPopupUiOperations.cs` (feature 488).** The host paths
   that can reopen a drop-down without reaching `RequestOpen` are the source of #462's recorded
   residual (SR-4 known limitation). Closing that residual at source is feature 488's business.

## Root Cause Analysis

### #462 — `CloseCore` never clears `_closePending` on the successful-close path

- **Confirmed root cause.** `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`, `CloseCore` at
  `:283-313`. The flag is latched at `:291`, cleared on the throw path at `:300` and on the
  not-closed path at `:309`, but the successful path at `:303-308` returns `true` without clearing:

  ```csharp
  if (closed)
  {
      lock (_sync)
          _generation++;
      return true;                 // :307 — no ClearClosePending()
  }
  ```

- **Mechanism.** `RequestOpen`'s guard at `:93-94` reads `if (_closePending && _host.IsOpen) return ClosedTask;`,
  and the clear at `:95` sits **after** the guard, so it is unreachable once the guard fires. Any
  reopen made while the host is open again returns the closed sentinel.
- **Why the failure is silent.** The sentinel is an ordinary already-completed `false` task. The
  return value is discarded at every production call site; no exception is thrown and nothing is
  logged.
- **The material fact the potential document omits.** `_closePending` is simultaneously the
  *repeated-close suppressor*: `CloseCore:289-290` returns `true` early when the flag is set, so a
  second `CloseCore` against an already-closed host never reaches `_host.Close`. Two existing tests
  assert exactly that suppression by name. This is why the potential document's literal remediation
  (clear the flag on the success path) is not safe as written.
- **Affected components.** `BreadcrumbDropDownOpenCoordinator` only. `CloseCore` has three callers,
  all internal to the type (`:148`, `:163`, `:258`), and all discard its `bool`.

### #500 — `TryRunCurrent` invokes the guarded action inside `_sync`

- **Confirmed root cause.** `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs`,
  `TryRunCurrent` at `:133-148`: `lock (_sync)` at `:139`, currency check at `:141`, `action()` at
  `:145`, `return true` at `:146` — the action executes inside the lock. The second half is
  `BreadcrumbMessengerHub.PostJson` at `:119-136`, which takes the hub's own `_sync` at `:126` and
  holds it across `PostToSurface` at `:133`.
- **Mechanism.** The verified chain is `BreadcrumbBridgeCoordinator.cs:266-275` →
  `BreadcrumbCoordinatorUpgradeLifetime.cs:130` (`Guard`) → `:139-147` (`TryRunCurrent`) →
  `BreadcrumbMessengerHub.cs:126,131-134` → `PostToSurface` at `:206` →
  `WebView2Messenger.PostJson` → inline `BreadcrumbUiDispatcher.Dispatch` →
  `_coreWebView.PostWebMessageAsJson` at `WebView2Messenger.cs:66`. `RunSynchronous` (`:111-122`)
  widens the exposure by putting the entire `SetSuggestions` body under the lock.
- **Why the failure is silent.** No deadlock is reachable on current wiring: the hub already snapshots
  `MessageReceived` under `_sync` at `:170` and raises it outside at `:172`, so there is no lock
  inversion, and every guarded action runs on one captured dispatcher boundary. The defect is a latent
  hazard plus a false implication — the code reads as though the currency check and the action are
  atomic, and they are not in any sense a re-entrant monitor can provide.
- **Additional evidence.** The file contradicts its own convention in five other places:
  `CancelLease` (`:258-295`), `Complete` (`:240-256`), `Abandon` (`:89-101`), `BeginPopulation`
  (`:48-70`) and `Invalidate` (`:72-87`) all deliberately perform their foreign calls outside the
  lock. `:145` is the only departure.
- **Affected components.** `BreadcrumbCoordinatorUpgradeLifetime` and `BreadcrumbMessengerHub`.

### #501 — `PostJson` caches before an unguarded broadcast

- **Confirmed root cause.** `QuickFiler/Viewers/BreadcrumbMessengerHub.cs`, `PostJson` at `:119-136`:

  ```csharp
  lock (_sync)                                              // :126
  {
      ThrowIfDisposed();                                    // :128
      string? type = MessageType(json);                     // :129
      CacheState(type, json);                               // :130 — cache BEFORE delivery
      foreach (Attachment attachment in _attachments.Values) // :131
      {
          PostToSurface(attachment, json, type);            // :133 — no try/catch anywhere
      }
  }
  ```

- **Mechanism.** The cache is written first, then an unguarded `foreach` broadcasts. A throw from any
  surface propagates out of `PostJson` and aborts the loop, so every attachment later in
  `Dictionary.Values` order is starved while `_cachedStates` claims delivery. `Attach` at `:82-93`
  demonstrates the omitted discipline: it wraps its replay in `try`/`catch` and rolls back the
  subscription on failure.
- **Why the failure is silent.** All six production callers
  (`BreadcrumbBridgeCoordinator.cs:271,318,233,282,378` and
  `BreadcrumbBridgeCoordinator.Search.cs:92`) execute inside `BreadcrumbUiDispatcher.Dispatch`, which
  catches and routes to the error sink (`BreadcrumbUiDispatcher.cs:86-89`). The starved surfaces emit
  no signal of their own, and the cache's false claim is only observable indirectly, on a later
  `Attach` replay.
- **Affected components.** `BreadcrumbMessengerHub` only.

### #502 — `RunSynchronous` discards `TryRunCurrent`'s `bool`

- **Confirmed root cause.** `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs`,
  `RunSynchronous` at `:111-122`:

  ```csharp
  try { TryRunCurrent(lease, operation); }   // :115 — bool discarded
  catch { Abandon(lease); throw; }
  ```

  and `BreadcrumbBridgeCoordinator.cs:100-115`, where `SuggestionsUpgrade = PopulateSuggestionsAsync(rows, lease)`
  sits **inside** the guarded lambda at `:112`.
- **Mechanism.** A superseded lease makes `TryRunCurrent` return `false` at `:143`. The lambda never
  runs, so `SuggestionsUpgrade` keeps the previous call's task, which may still be incomplete.
  `AddItems` (`:131-147`) has the same structure with its dispatch task discarded at `:141`.
- **Companion defect (same window, same fix).** With the action skipped, `Complete(lease)` is never
  called — the only paths that call it are `RunAsync`'s `finally` (`:175`, `:199`) and `Abandon`
  (`:100`). `lease.Settled` never becomes `true`, so `CancelLease`'s disposal condition
  `lease.Settled && !lease.SourceDisposed` (`:285-289`) never holds and the lease's
  `CancellationTokenSource` is leaked, one per superseded population.
- **Why the failure is silent.** `RunSynchronous` returns `void`, so no call site can observe the
  skip; the stale `SuggestionsUpgrade` is a well-formed `Task` that an awaiting caller simply waits on
  longer than it should; and a leaked `CancellationTokenSource` produces no diagnostic at all.
- **Affected components.** `BreadcrumbCoordinatorUpgradeLifetime` and `BreadcrumbBridgeCoordinator`.

## Ordering and State-Transition Invariants (Normative)

These invariants are the normative core of this specification. Each is test-assertable and is the
verification target of one or more acceptance criteria.

### I-462 — drop-down open/close state machine

- **I-462.1 (in-flight bound).** The in-flight close flag is `true` only between the latch at `:291`
  and the completion of `_host.Close(reason)` at `:296`. It must read `false` at every point at which
  control leaves `CloseCore` — success, not-closed, throw, and released.
- **I-462.2 (reopen after a successful close).** For a coordinator that is not released: after a
  `CloseCore` that returned `true`, and with `_host.IsOpen == true` reached by any path,
  `RequestOpen()` must return a task that is **not** the `ClosedTask` sentinel and must reach
  `_host.OpenAsync`. Assert `harness.Host.Requests` gains an entry and the returned task is not the
  already-completed `false` sentinel.
- **I-462.3 (idempotent close, preserved).** Two `CloseCore` calls with no intervening `RequestOpen`
  and no intervening host reopen must reach `_host.Close` exactly once. Assert
  `harness.Host.CloseReasons` has exactly one entry.
- **I-462.4 (generation monotonicity, preserved).** `_generation` increases exactly once per
  successful close (`:306`) and exactly once per `Invalidate` (`:327`). A close that returns `false`
  must not increment it.
- **I-462.5 (released terminality, preserved).** After `Release()`, `RequestOpen()` returns the closed
  sentinel and `CloseCore` returns `false` without touching `_host`.

I-462.2 is the failing-first assertion. I-462.1 and I-462.3 together are the constraint that rules out
the naive fix.

### I-500 — atomicity and lock scope

- **I-500.1 (no foreign call under `BreadcrumbCoordinatorUpgradeLifetime._sync`).** At the moment the
  guarded `action` executes, the calling thread must not hold the lifetime's `_sync`. Assert
  `Monitor.IsEntered(lifetimeSync) == false` observed from inside the action, where `lifetimeSync` is
  the reflected private `_sync` field.
- **I-500.2 (no foreign call under `BreadcrumbMessengerHub._sync`).** At the moment
  `IWebViewMessenger.PostJson` is invoked on an attached surface, the calling thread must not hold the
  hub's `_sync`. Assert `Monitor.IsEntered(hubSync) == false` observed from inside a fake surface's
  `PostJson`.
- **I-500.3 (currency claim honesty).** `TryRunCurrent` returns `true` if and only if the lease was
  current **at the moment the action was invoked**. It must not claim more than that. A re-entrant
  `BeginPopulation` / `Invalidate` / `TryDispose` performed *by the action itself* is observable
  afterwards through `IsCurrent(lease) == false`; the return value is not retro-actively falsified.
  Assert that an action calling `lifetime.Invalidate()` still yields `TryRunCurrent == true` and that
  `lifetime.IsCurrent(lease) == false` immediately afterwards.

  **I-500.3 is deliberately the weaker contract — "the action was invoked at entry-time currency" —
  and is explicitly NOT "the check and the action are atomic".** The strong form is unachievable with
  a re-entrant `Monitor`: the guarded action can, and on the `SetSuggestions` happy path routinely
  does, re-enter the same monitor and mutate the very state the check read. Claiming the strong form
  is the actual defect in #500, not merely the lock scope. The weak form is the contract the fix must
  preserve, and it is what #502 depends on (see `## Cross-Cutting NFR`).
- **I-500.4 (no re-entrant collection mutation during broadcast).** A re-entrant `Attach` or `Detach`
  performed from inside a surface's `PostJson` must not throw
  `InvalidOperationException: Collection was modified`. Assert the broadcast completes and the
  re-entrant call takes effect.

### I-501 — delivery and cache consistency

- **I-501.1 (no starvation).** For every attachment live at the moment `PostJson` is entered, exactly
  one delivery **attempt** is made, regardless of whether any earlier attempt threw. With N attached
  surfaces the total attempt count is N.
- **I-501.2 (containment).** A throw from one surface does not prevent delivery to any other surface,
  and does not leave `_attachments` / `_cachedStates` in a state that differs from the no-throw case
  except for the failed surface's own view.
- **I-501.3 (cache truthfulness).** After `PostJson(json)` returns, a later `Attach` replays a state
  that every surviving surface has already received. Equivalently: the replay cache must never hold a
  state that **no** surface received.
- **I-501.4 (diagnosability).** A delivery failure is not silently discarded; it reaches the
  repository logging pattern through the file's existing `log4net` logger (the pattern is already in
  the file at `:269-272`).
- **I-501.5 (`Attach` replay unchanged).** `Attach`'s existing transactional rollback (`:82-93`) is
  not weakened. Assert
  `BreadcrumbMessengerHubTests.Attach_ReplayFailureRollsBackSubscriptionAndAllowsRetry` (`:198-217`)
  still passes unmodified.

### I-502 — observability of a superseded population

- **I-502.1 (the skip is reported).** `RunSynchronous` returns `false` when, and only when, the
  guarded action did not run.
- **I-502.2 (`SuggestionsUpgrade` is never stale).** After `SetSuggestions` returns, the value of
  `SuggestionsUpgrade` is either the task created by *this* call or a task that is already completed.
  It is never the task created by an *earlier* call while that task is still incomplete. Assert by
  capturing `SuggestionsUpgrade` before the call, forcing the skip, and asserting the post-call value
  is not reference-equal to the captured incomplete task.
- **I-502.3 (no lease leak).** Every lease returned by `BeginPopulation` reaches `Settled == true`,
  including a lease whose guarded action was skipped. Assert `lease.Settled == true` and
  `lease.SourceDisposed == true` after the skip.
- **I-502.4 (`AddItems` parity).** The same skip in `AddItems` settles its lease. `AddItems` exposes
  no handle, so no observability obligation beyond I-502.3 applies to it.

## Proposed Fix

### Design summary (what changes where)

| Defect | File | Change |
| --- | --- | --- |
| #462 | `BreadcrumbDropDownOpenCoordinator.cs` | Split `_closePending` into `_closeInFlight` (cleared in a `finally`) and `_closeCompleted` (cleared by `RequestOpen` and `Invalidate`). Research §6.1 option D. |
| #500 lifetime | `BreadcrumbCoordinatorUpgradeLifetime.cs` | `TryRunCurrent` captures the currency verdict under `_sync`, releases, then invokes the action and returns the captured verdict. Research §6.2 option A. |
| #500 hub + #501 | `BreadcrumbMessengerHub.cs` | **One** rewrite of `PostJson`: snapshot attachments under `_sync` (cache write stays inside the lock), release, broadcast outside the lock with a per-surface `try`/`catch` that logs and continues. Research §6.3. |
| #502 | `BreadcrumbCoordinatorUpgradeLifetime.cs` + `BreadcrumbBridgeCoordinator.Suggestions.cs` | `RunSynchronous` returns `TryRunCurrent`'s `bool`; both call sites branch on `false`. Research §6.4 option C. |
| SR-1 budget | new `BreadcrumbBridgeCoordinator.Suggestions.cs` | Move `SetSuggestions`, `SuggestionsUpgrade`, `PopulateSuggestionsAsync`, `AddItems` into a new partial part. |

### Design Decisions (settled scope rulings — do not re-open)

- **SR-1 — APPROVED: create `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs`.**
  Move `SetSuggestions` (`:99-115`), `SuggestionsUpgrade` (`:117-118`), `PopulateSuggestionsAsync`
  (`:120-128`) and `AddItems` (`:130-147`) into it, and add exactly one
  `<Compile Include="Viewers\BreadcrumbBridgeCoordinator.Suggestions.cs" />` to
  `QuickFiler/QuickFiler.csproj`.
  *Rationale:* `BreadcrumbBridgeCoordinator.cs` is at 487 of a hard 500-line cap
  (`.claude/rules/general-code-change.md`) and the #502 fix adds 13-17 lines, so the file would land
  at ~500-504. The class is already `public sealed partial` (`:25`), and the repository has already
  made this exact split, for this exact file, for this exact reason:
  `BreadcrumbBridgeCoordinator.Search.cs` states in its own doc comment (`:10-13`) that it exists so
  the primary file "stays clear of the repository's 500-line ceiling". The new part is a part of an
  owned type and therefore counts as owned. After the move the primary file sits near 437 lines.
- **SR-2 — REJECTED: do not reuse `BreadcrumbBridgeCoordinator.Search.cs` as the destination.**
  *Rationale:* its doc comment states a narrower purpose (the folder-search presentation composite)
  that moving the suggestions members would falsify. Avoiding one project-file line is not worth
  making a file's stated purpose untrue.
- **SR-3 — DECIDED: swallow and log per surface; `PostJson` must NOT propagate a surface throw after
  the fix.**
  *Rationale:* all six production callers already run inside `BreadcrumbUiDispatcher.Dispatch`, which
  catches and routes to the error sink (`BreadcrumbUiDispatcher.cs:86-89`), so the throw reaches no
  user today; and no existing test asserts propagation (the two `ThrowOnPost` tests both throw during
  `Attach`-time replay, never during a broadcast). Per-surface containment plus an explicit log call
  through the file's existing `log4net` logger preserves diagnosability (I-501.4). The rejected
  alternative — collect failures and throw an `AggregateException` after the loop — also satisfies
  I-501.1 and breaks no existing test, but changes the exception type the dispatcher's sink observes
  for no benefit.
- **SR-4 — DECIDED: minimal two-flag form (research §6.1 option D), without the `&& !_host.IsOpen`
  refinement.**
  *Rationale:* the refinement `if (_closeCompleted && !_host.IsOpen) return true;` would read
  `_host.IsOpen` under `_sync` — the very lock-ordering hazard that #462's potential document flags
  and that #500 exists to remove. Adding it here would create a new instance of the class of defect
  this feature is closing.
  **KNOWN LIMITATION (accepted, recorded, not fixed here):** if the host is reopened by a path that
  reaches neither `RequestOpen` nor `Invalidate`, `_closeCompleted` stays `true` and a subsequent
  close request returns `true` without closing. This residual is **strictly narrower** than HEAD's
  behaviour, in which the single `_closePending` flag latches after *every* successful close and
  suppresses reopen unconditionally. Closing the residual at source belongs to the host paths owned by
  sibling feature 488 (see Cross-feature note 4).
- **SR-5 — APPROVED: add the `internal void SetSuggestionsCore(IReadOnlyList<FolderRow> rows, BreadcrumbUpgradeLease lease)`
  seam.**
  *Rationale:* the `SetSuggestions` window between `BeginPopulation` (`:104`) and `RunSynchronous`
  (`:105`) has no in-process seam — the statements are adjacent, `BreadcrumbCoordinatorUpgradeLifetime`
  is `sealed` with non-virtual members, and `_upgradeLifetime` is a `readonly` field assigned in the
  constructor. Splitting the public entry point from an `internal` core makes #502's I-502.2 assertion
  a direct, deterministic call instead of a reflective invocation.
  `[assembly: InternalsVisibleTo("QuickFiler.Test")]` already exists at
  `QuickFiler/Properties/AssemblyInfo.cs:5`, so no reflection is needed for the call itself. The
  rejected alternative (reflect the private members, research §5.4 option 2) produces a brittler test;
  the two-thread handshake (option 3) does not work at all, because the handshake would have to fire
  between two adjacent statements that no event can observe.

### Per-defect design, with rejected alternatives

#### #462 — research §6.1 option D (RECOMMENDED)

- `_closeInFlight` replaces `_closePending`: `true` only while `_host.Close(reason)` at `:296` is
  executing. Cleared on every exit, preferably by a `finally` around the `_host.Close` call, which
  also removes the duplicated `ClearClosePending()` at `:300` and `:309`.
- `_closeCompleted` is new: `true` after a close that returned `true`. Cleared by `RequestOpen` at the
  point that already clears the flag (`:95`) and by `Invalidate` at `:329`.
- `CloseCore` becomes: `if (_released) return false;` → `if (_closeInFlight) return true;` →
  `if (_closeCompleted) return true;` → latch `_closeInFlight`.
- `RequestOpen`'s guard at `:93` becomes `if (_closeInFlight && _host.IsOpen) return ClosedTask;`,
  which now means exactly what it says.

**Rejected alternatives.**

- *Option A — the potential document's literal remediation (clear the flag on the success path).*
  Rejected: it removes the repeated-close suppression `_closePending` accidentally also provides,
  breaking `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose`
  (`BreadcrumbDropDownOpenCoordinatorTests.cs:262-280`) and
  `SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired` (`…Part2.cs:120-140`). Both encode a
  deliberate contract in their names.
- *Option B — gate `CloseCore` on `!_host.IsOpen`.* Rejected:
  `PendingAutomaticClose_RequestsExplicitCommitWhenHostIsNotOpen` (`…Tests.cs:301-318`) proves that
  closing while `_host.IsOpen == false` is required behaviour, and the gate also re-introduces reading
  `_host.IsOpen` under `_sync`.
- *Option C — track the generation at which a close completed.* Rejected: `_generation` is incremented
  by the successful close itself (`:306`) and not by `RequestOpen`, so `_closedAtGeneration` would
  still equal `_generation` when a new open begins, suppressing the close of the new open. It works
  only with an extra reset in `RequestOpen`, which is strictly more state than option D for the same
  result.

**Observable behaviour change:** exactly one, and it is the point of the issue — a `RequestOpen` after
a successful close now opens instead of silently returning the closed sentinel.

#### #500 lifetime half — research §6.2 option A (RECOMMENDED)

```csharp
bool current;
lock (_sync) { current = IsGenerationCurrentCore(lease) && !lease.Token.IsCancellationRequested; }
if (!current) { return false; }
action();
return true;
```

Satisfies I-500.1 directly, matches the convention the same file already follows in five places, and
keeps the `bool` meaning "the action was invoked" — precisely what #502 needs.

**Rejected alternatives.**

- *Option B — move the action out **and** fold a post-action currency re-check into the return value.*
  Rejected: it makes `false` ambiguous ("did not run" versus "ran but was superseded"), which directly
  breaks the #502 fix — `SetSuggestions` would overwrite `SuggestionsUpgrade` with a completed task
  *after* the guarded lambda had already assigned the real one. If a post-action verdict is ever
  wanted it must be a separate `out` parameter or a separate `IsCurrent(lease)` call by the caller.
  See `## Cross-Cutting NFR`.
- *Option C — a non-re-entrant guard.* Rejected and moved out of scope entirely; see
  `## Scope & Non-Goals`.

**Documented consequence.** Two threads could now both pass the currency check and run their actions
concurrently where the monitor previously serialized them. On current wiring this is not reachable:
every guarded action runs on the captured `BreadcrumbUiDispatcher` boundary, and `RunSynchronous` is
reached only from `SetSuggestions` / `AddItems` on the viewer thread. Recorded here explicitly rather
than left implicit.

#### #500 hub half + #501 — research §6.3, ONE combined `PostJson` rewrite

**These are one edit, not two.** Containing the throw *inside* the existing `lock` would leave I-500.2
unsatisfied; narrowing the lock *without* containment would leave I-501.1 unsatisfied. Plan them as
one task with two acceptance criteria, not two tasks against the same method.

1. Under `lock (_sync)`: `ThrowIfDisposed()`; compute `type`; `CacheState(type, json)` — **the cache
   write stays inside the lock**; snapshot `_attachments.Values` into a local array.
2. Release the lock.
3. Iterate the snapshot **outside** the lock, wrapping each `PostToSurface` in its own `try`/`catch`
   that logs through the existing `log4net` logger (the pattern is already in the file at `:267-272`,
   `SafeUnsubscribe`) and continues to the next attachment.

**Where the cache write belongs.** Inside the lock and before the broadcast. It mutates `_cachedStates`
and `_sequence` at `:190`. Once the broadcast is contained, every live surface receives the message, so
the cache claim is true for all of them; the one surface that threw is stale by its own failure, and no
rollback can repair that without making the other surfaces stale too.

**Rejected alternatives.**

- *Defer the cache write until after a successful broadcast.* Rejected: if any surface throws, the
  cache retains the *previous* state, so a later `Attach` replays something older than what the
  surviving surfaces already have — a more severe violation of I-501.3 than the current behaviour.
- *Auto-detach a throwing surface.* Rejected: most invasive; a transient failure permanently drops a
  live surface; and mutating `_attachments` during the broadcast is exactly the hazard I-500.4
  identifies.
- *Propagate an `AggregateException` after the loop.* Rejected by SR-3.

**Observable behaviour changes:** (a) a broadcast throw no longer propagates out of `PostJson`;
(b) surfaces after the failing one now receive the message; (c) `PostJson` no longer holds `_sync`
while calling into a surface, so a re-entrant `Attach` / `Detach` / `PostJson` from a surface callback
no longer throws.

#### #502 — research §6.4 option C (RECOMMENDED = A plus a `false` branch at both call sites)

- `internal bool RunSynchronous(...)` with `return TryRunCurrent(lease, operation);` inside the
  existing `try`. No caller breaks: both production call sites are updated, and the two test call
  sites compile unchanged because `Action a = () => Foo();` is legal when `Foo()` returns `bool`.
- In `SetSuggestions`: when `RunSynchronous` returns `false`, set
  `SuggestionsUpgrade = Task.CompletedTask` (I-502.2 with the completed-task outcome the potential
  document explicitly accepts) and call `_upgradeLifetime.Abandon(lease)` to settle the unused lease
  (I-502.3). `Abandon` (`:89-101`) is safe here: it bumps `_generation` only when the lease is still
  `_current` (`:93-97`), which a superseded lease is not.
- In `AddItems`: on `false`, call `_upgradeLifetime.Abandon(lease)` only. No handle exists, so nothing
  further is observable; record the deliberate discard in an XML comment so the skip is intentional
  rather than accidental (I-502.4).

**Rejected alternative.** *Option B — hoist the `SuggestionsUpgrade` assignment out of the guarded
lambda on its own.* Rejected: it would call `PopulateSuggestionsAsync(rows, lease)` with rows never
applied to the router (`_router.SetSuggestionFallbacks(rows)` at `:109` is inside the skipped lambda).
The resulting task completes immediately and is harmless, but it is semantically incoherent and leaves
`AddItems` untouched.

**Why `Task.CompletedTask` and not `Task.FromCanceled`.** Eleven existing tests call
`SuggestionsUpgrade.GetAwaiter().GetResult()`, which would throw on a cancelled task. None reaches the
superseded branch today, so either choice compiles — but a completed task matches the property's
declared initial value (`BreadcrumbBridgeCoordinator.cs:118`) and cannot surprise a future caller.

### Boundaries and invariants to preserve

- No **public** signature changes anywhere. `SetSuggestions`, `AddItems`, `SuggestionsUpgrade` and
  `PostJson` keep their shapes. Only `RunSynchronous` (internal) gains a return type, and only
  `SetSuggestionsCore` (internal) is added.
- The three named existing tests in `## Acceptance Criteria` must pass **unmodified**.
- No file may exceed 500 lines after the change (`.claude/rules/general-code-change.md`).
- No write to any sibling-owned file listed in `## Scope & Non-Goals`.
- The cross-cutting NFR below binds the #500 and #502 fixes together.

### Dependencies or blocked work

- No external dependency and no blocking work. All four defects are on HEAD and all inputs are
  in-repo.
- **Internal sequencing constraint (one only):** the SR-1 partial split must precede (or be folded
  into) the #502 call-site change, so `BreadcrumbBridgeCoordinator.cs` never crosses 500 lines
  mid-plan. Recommended overall order: #462 (fully independent) → #500 lifetime half → #502 → #500 hub
  half + #501 as one task.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

| File | Nature | Current lines | Estimated after |
| --- | --- | ---: | ---: |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | modify | 355 | ~369 |
| `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | modify | 309 | ~321 |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | modify | 456 | ~474 (safe but tight) |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | modify (members removed) | 487 | ~437 |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs` | **new** | 0 | ~70 |
| `QuickFiler/QuickFiler.csproj` | one added line (after line 392) | — | +1 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | one added line | — | +1 |

#### Functions/classes/CLI commands impacted

- `BreadcrumbDropDownOpenCoordinator.CloseCore`, `.RequestOpen`, `.Invalidate`, `.ClearClosePending`
  (the last is absorbed into the `finally`).
- `BreadcrumbCoordinatorUpgradeLifetime.TryRunCurrent`, `.RunSynchronous`. `Guard` (`:124`) is
  unchanged and unaffected — it already discards the `bool`.
- `BreadcrumbMessengerHub.PostJson`.
- `BreadcrumbBridgeCoordinator.SetSuggestions` (plus the new `SetSuggestionsCore`), `.AddItems`,
  `SuggestionsUpgrade` — all relocated to the new partial part.
- No CLI command is affected.

#### Data flow and validation changes

- `PostJson` gains an intermediate snapshot of `_attachments.Values` taken under the lock; the
  broadcast then reads the snapshot, so a re-entrant `Attach` / `Detach` mutates the live dictionary
  without invalidating the in-progress enumeration (I-500.4).
- `TryRunCurrent` gains an intermediate `bool current` captured under the lock; the action then runs
  against the captured verdict.
- No serialization format, payload shape, or message schema changes.

#### Error handling and logging updates

- `PostJson` gains a per-surface `try`/`catch` that logs the failure through the file's existing
  `log4net` logger (I-501.4) and continues. It no longer propagates a surface throw (SR-3).
- No other error path changes. `RunSynchronous`'s existing `catch { Abandon(lease); throw; }` is
  preserved verbatim.
- No new logging category, appender, or configuration key.

#### Rollback/feature-flag considerations (if applicable)

None. The change is small, self-contained, and covered by regression tests; a revert of the commit is
the rollback. Introducing a feature flag for a correctness fix would preserve the defective path and
double the test matrix for no benefit.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

- `internal bool RunSynchronous(BreadcrumbUpgradeLease lease, Action operation)` — was `void`.
  Returns `true` when the guarded action was invoked, `false` when it was skipped because the lease
  was not current at entry.
- `internal bool TryRunCurrent(BreadcrumbUpgradeLease lease, Action action)` — signature unchanged;
  the `bool` contract is tightened in prose by I-500.3.
- `internal void SetSuggestionsCore(IReadOnlyList<FolderRow> rows, BreadcrumbUpgradeLease lease)` —
  new; the body of the current `SetSuggestions` after the lease has been obtained.
- `public void PostJson(string json)` — signature unchanged; the `ArgumentNullException` on a null
  `json` and the `ObjectDisposedException` from `ThrowIfDisposed()` are both preserved.

#### Required configuration keys and defaults

None. No configuration key is added, read, or changed.

#### Backward-compatibility expectations

- No public API changes, so no external caller is affected.
- Two internal signature changes, both reachable only from within `QuickFiler` and `QuickFiler.Test`
  (which is granted access by `[assembly: InternalsVisibleTo("QuickFiler.Test")]` at
  `QuickFiler/Properties/AssemblyInfo.cs:5`).
- Three deliberate behaviour changes, each the point of its issue: a reopen after a successful close
  now opens; a broadcast throw no longer propagates and no longer starves later surfaces; a superseded
  population now replaces `SuggestionsUpgrade` and settles its lease.

#### Performance constraints (latency/throughput/memory)

- No latency or throughput target changes. Lock hold times strictly decrease: both `TryRunCurrent` and
  `PostJson` hold their monitors for less work than before.
- One small allocation is added per `PostJson` call — the attachment snapshot array, bounded by the
  attachment count (two in production: collapsed and popup).
- Memory improves in one respect: one `CancellationTokenSource` per superseded population is now
  disposed instead of leaked.

## Assumptions, Constraints, Dependencies

- **Assumptions.** HEAD is `988e819b` or a descendant in which the cited line numbers have not
  drifted materially; the planner re-verifies line numbers before editing. `QuickFiler.Test` can reach
  `internal` members of `QuickFiler`. All guarded actions in production run on the captured
  `BreadcrumbUiDispatcher` boundary.
- **Constraints.**
  - Hard 500-line file cap (`.claude/rules/general-code-change.md`), which drives SR-1.
  - `.claude/rules/general-unit-test.md` bans `Thread.Sleep`, `Task.Delay` and real wall-clock waits
    in test code, and prohibits temporary files.
  - MSTest + Moq + FluentAssertions only (CLAUDE.md, C# Unit Test Policy); .NET Framework 4.8.1.
  - Ownership boundary from `issue.md`: four owned production files; six sibling-owned files are
    read-only.
  - Exactly two project-file lines may be added; no other project-file edit is authorized.
- **External dependencies.** None. No package is added, removed, or upgraded.

## Data / API / Config Impact

- **User-facing or API changes.** No public API change. One user-visible behaviour change: a
  drop-down reopen after a successful close now works (#462). The other three are internal correctness
  fixes with no visible surface.
- **Data or migration considerations.** None. No persisted state, schema, or stored data is touched.
- **Logging/telemetry updates.** One addition: `BreadcrumbMessengerHub.PostJson` logs a per-surface
  delivery failure through the file's existing `log4net` logger (I-501.4). No new appender, category,
  or configuration.
- **Compatibility notes.** No CLI flag, config schema, or version marker changes. Two `internal`
  signature changes are compile-time visible only within `QuickFiler` and `QuickFiler.Test`.

## Test Strategy

All tests are MSTest with Moq and FluentAssertions on .NET Framework 4.8.1. Determinism comes from
`Monitor.IsEntered` against a reflected private `_sync` field, injected re-entrant actions, and
explicit synchronization-context drain — **never** from threading or timing. `Thread.Sleep`,
`Task.Delay` and real wall-clock waits are banned by `.claude/rules/general-unit-test.md`, and the
suite currently contains none.

### Determinism seams already in the suite

- **`Monitor.IsEntered` lock-scope template:**
  `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs:145-192` reflects a private `_sync`
  field out and records `Monitor.IsEntered(sync)` from inside a mock callback, asserting `false`. This
  is the exact probe #500 needs, already passing today. `Monitor.IsEntered` reports whether the
  **current thread** holds the lock, so the probe is exact on a single thread with no synchronization
  primitives and no timing.
- **`CoordinatorHarness` / `ControlledHost`**
  (`BreadcrumbDropDownOpenCoordinatorTests.cs:323-372` and `:374-461`), `private sealed` and nested in
  the partial `[TestClass]`, so visible to all three parts. `SetOpen(bool)` (`:407`), `CloseResult`
  (`:397`), `CloseFailure` (`:398`), `CloseReasons` (`:395-396`), `Requests` (`:386-387`),
  `Enqueue` / `EnqueueThrow` (`:402-405`).
- **`CapturingSynchronizationContext`**
  (`BreadcrumbSelectorToggleUiBoundaryTests.cs:346-440`): queues posts and drains them explicitly via
  `DrainOne` / `DrainAll` / `DrainUntil` on the creator thread, throwing if drained elsewhere
  (`:406-409`).
- **Private-field reflection precedent** in the target file:
  `BreadcrumbCoordinatorUpgradeLifetimeTests.cs:93-105` (`SetCurrentLease` writing `_current` and
  `_generation` via `BindingFlags.Instance | BindingFlags.NonPublic`).
- **`_upgradeLifetime` reflection precedent:** `BreadcrumbCoordinatorLifecycleTests.cs:370-377`.

### Per-defect failing-first regression tests

| Defect | Test | Target file | Seam | RED on HEAD? |
| --- | --- | --- | --- | --- |
| #462 | reopen after successful close (I-462.2) plus the repeated-close guard (I-462.3) in the same or a sibling test | `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` (119 lines free) | `CoordinatorHarness` + `ControlledHost.SetOpen` + `CapturingSynchronizationContext.DrainUntil` | **Yes** — `Requests` stays at 1 and the task is the completed `false` sentinel |
| #500 lifetime | lifetime lock probe (I-500.1) and the honest-contract test (I-500.3) | `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` (378 lines free) | reflected `_sync` + `Monitor.IsEntered`; injected re-entrant `Invalidate` action | I-500.1 **yes**; I-500.3 **no** — it documents the contract the fix must preserve and guards against §6.2 option B |
| #500 hub | hub lock probe (I-500.2) and re-entrant `Attach` during broadcast (I-500.4) | `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` (86 lines free), with `BreadcrumbSelectorCoordinatorTests.cs` (66 free) as the sanctioned overflow home | reflected hub `_sync`; fake `IWebViewMessenger` calling `hub.Attach` re-entrantly | **Yes** for both — the probe reads `true` on HEAD, and the re-entrant `Attach` throws `InvalidOperationException: Collection was modified` |
| #501 | order-independent starvation (I-501.1) with containment (I-501.2) and cache truthfulness (I-501.3) | `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` | `TrackingMessenger` (`:364-412`) extended with an attempt counter, or a purpose-built local fake | **Yes** — total attempts is 1, not 2 |
| #502 companion | lease leak on a skipped `RunSynchronous` (I-502.3) | `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` | `BeginPopulation()` then `Invalidate()` on one thread | **Yes, and it is the only #502 assertion that is RED against HEAD without a signature change** — see below |
| #502 mechanism | `RunSynchronous` reports the skip (I-502.1) and `AddItems` parity (I-502.4) | `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` | same | Does not compile on HEAD (`void` return); authored after the signature change |
| #502 symptom | `SuggestionsUpgrade` is not stale (I-502.2) | **new file** `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs` | `SetSuggestionsCore` internal seam (SR-5) + reflected `_upgradeLifetime` | Does not compile on HEAD (seam does not exist) |

**Why the companion lease-leak test is the correct failing-first test for #502.** The repository's
Bugfix Workflow requires "the smallest deterministic test that reproduces the bug", failing before the
fix. A compile error is not a test failure, so the I-502.1 and I-502.2 assertions — both of which
require a signature or seam that does not exist on HEAD — cannot serve. The lease-leak assertion
compiles against HEAD today and fails, because a skipped `RunSynchronous` never calls
`Complete(lease)`. **Author it first.**

### Edge cases and negative scenarios

- #462: close that returns `false` (must not increment `_generation`, I-462.4); close that throws (must
  clear the in-flight flag, I-462.1); operations after `Release()` (I-462.5); two consecutive closes
  with no intervening reopen (I-462.3).
- #500: an action that re-enters the lifetime and invalidates its own lease (I-500.3); a re-entrant
  `Attach` and a re-entrant `Detach` from inside a surface `PostJson` (I-500.4).
- #501: **the starvation test must be ORDER-INDEPENDENT.** `Dictionary<TKey,TValue>.Values`
  enumeration order is not contractual, so a test attaching "throwing first, recording second" would
  silently pass on HEAD whenever the runtime happened to enumerate the recording surface first.
  Instead attach **two** surfaces that **both** throw and **both** increment an attempt counter
  *before* throwing, then assert the total is **2**. On HEAD the first throw aborts the `foreach`, so
  the total is 1 regardless of enumeration order.
- #501: a post with zero attachments; a post with one throwing and one recording surface (I-501.2);
  a fresh `Attach` after a partially failed broadcast (I-501.3).
- #502: a superseded lease in `SetSuggestions` and the same in `AddItems` (I-502.4); a lease that is
  current (the unchanged happy path).

### Error handling and logging verification

- Assert that a per-surface failure in `PostJson` is logged (I-501.4) rather than silently discarded,
  and that `PostJson` itself does not throw (SR-3).
- Assert that `RunSynchronous`'s existing throw path still calls `Abandon` and rethrows:
  `RunSynchronous_FailureAbandonsLinkedLeaseAndReportsCancellationFailure`
  (`BreadcrumbCoordinatorUpgradeLifetimeTests.cs:36-56`) must pass unchanged.

### Test file placement and headroom (exact, from research §7.3)

| File | Current lines | Headroom to 500 | Use |
| --- | ---: | ---: | --- |
| `BreadcrumbCoordinatorUpgradeLifetimeTests.cs` | 122 | **378** | #500 lifetime probe, I-500.3 contract test, #502 mechanism + lease-leak RED |
| `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | 381 | **119** | #462 reopen RED + repeated-close guard |
| `BreadcrumbMessengerHubTests.cs` | 414 | **86** | #501 starvation/containment/cache test; hub lock probe and re-entrancy test if they fit |
| `BreadcrumbSelectorCoordinatorTests.cs` | 434 | **66** | sanctioned overflow home for the #500 hub lock probe and the I-500.4 test; it owns the `Monitor.IsEntered` template |
| `BreadcrumbDropDownOpenCoordinatorTests.cs` | 463 | 37 | shared harness only — do not add tests |
| `BreadcrumbMessengerHubCoverageTests.cs` | 478 | **22 — no room** | — |
| `BreadcrumbBridgeCoordinatorTests.cs` | 488 | **12 — no room** | — |
| `BreadcrumbCoordinatorLifecycleTests.cs` | 489 | **11 — no room** | — |

**Consequence:** #502's coordinator-level test has no home in an existing file, so the new file
`QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs` is required, together with
one `<Compile Include>` line in `QuickFiler.Test/QuickFiler.Test.csproj`.

**Correction to `issue.md:59-61`, carried from research §7.3:** the `Breadcrumb*` entries in that item
group are **not** alphabetically ordered. Verified at `QuickFiler.Test/QuickFiler.Test.csproj:58-95`:
the sequence runs `BreadcrumbBridgeCoordinatorTests` (60), `BreadcrumbBridgeCoordinatorProbabilityTests`
(61), `BreadcrumbCoordinatorLifecycleTests` (62), … `BreadcrumbPopupUiOperationsDirectAdapterTests`
(65), `BreadcrumbUiThreadDispatchTests` (66), `BreadcrumbSelectorToggleUiBoundaryTests` (67). The
operative rule is therefore **"insert adjacent to the sibling test for the same production type"**
(i.e. immediately after line 60 for a `BreadcrumbBridgeCoordinator` test), which minimises merge
surface with sibling epic children just as effectively as alphabetical placement would.

**Test-file budget ruling.** Four hub-side assertions (I-500.2, I-500.4, I-501.1/2, I-501.3) compete
for 86 lines in `BreadcrumbMessengerHubTests.cs`. Consolidate I-501.1, I-501.2 and I-501.3 into a
single test (two counting-and-throwing surfaces plus one recording surface, one post, then a fresh
attach to check the replay). If the combined additions would push the file past 500 lines, move the
I-500.2 lock probe and the I-500.4 re-entrancy test to `BreadcrumbSelectorCoordinatorTests.cs`
(66 lines free, and it owns the `Monitor.IsEntered` template). **A third new test file is not
authorized** — the project-file line budget for this feature is exactly two.

### Coverage impact and targets for changed lines/modules

- Every changed production line must be covered by at least one of the tests above; changed-line
  coverage must not regress (`.claude/rules/general-unit-test.md`).
- The four owned files are existing, well-covered types; the new partial part contains only relocated
  members plus the `SetSuggestionsCore` seam, all of which stay covered by their existing tests.
- Repository line coverage must remain at or above the standing floor. Coverage is collected by the
  toolchain step 4 below.

### Toolchain commands to run (format → lint → type-check → test)

Run in this exact order; if any step fails or auto-fixes a file, restart from step 1.

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Two omissions are deliberate per CLAUDE.md and must not be "restored": **do not add
`/p:Nullable=enable`** (no project carries a `<Nullable>` element and there is no
`Directory.Build.props`, so the property conscripts files that never opted in; CI omits it), and
**do not use `/t:Build`** (MSBuild's up-to-date check does not invalidate on a command-line `/p:`
change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped and the gate cannot fail).

### Manual validation steps (if required)

None required. Every invariant in this spec is covered by an automated deterministic test. Optional
smoke validation in a live Outlook session: open the breadcrumb drop-down, close it, reopen it, and
confirm the reopen succeeds (#462).

## Cross-Cutting NFR

> **`TryRunCurrent`'s `bool` must continue to mean "the action was invoked at entry-time currency" —
> the currency verdict taken at entry, never a verdict recomputed after the action returns.**

This binds the #500 and #502 fixes together. If #500 is implemented as research §6.2 option B (fold a
post-action currency re-check into the return value), then #502's `false` branch in `SetSuggestions`
fires *after* the guarded lambda has already assigned `SuggestionsUpgrade`, and the fix overwrites a
live handle with `Task.CompletedTask` — **turning the #502 remedy into a fresh instance of the #502
defect**. Implement §6.2 option A.

The NFR is the reason I-500.3 is stated in its weaker, honest form and is included as a test even
though it is green on HEAD: it is the regression guard against a future attempt to strengthen the
return value. If a post-action verdict is ever wanted, it must be delivered as a separate `out`
parameter or a separate `IsCurrent(lease)` call by the caller, never by redefining this `bool`.

## Companion Defect (in scope): superseded-lease `CancellationTokenSource` leak

Recorded by research §1.4; not present in any promoted potential document.

When `TryRunCurrent` returns `false`, `Complete(lease)` is never called for that lease. The only paths
that call `Complete` are `RunAsync`'s `finally` (`BreadcrumbCoordinatorUpgradeLifetime.cs:175`,
`:199`) and `Abandon` (`:100`). `Complete` is what sets `lease.Settled = true`, and `CancelLease`
disposes the lease's `CancellationTokenSource` only when `lease.Settled && !lease.SourceDisposed`
(`:285-289`). A skipped `RunSynchronous` therefore leaks one `CancellationTokenSource` per superseded
population.

It is in scope because both files involved are owned, and it is closed by the same `false`-branch fix:
calling `_upgradeLifetime.Abandon(lease)` on the skip path settles the lease and permits disposal
(I-502.3). Its assertion is the correct failing-first test for #502 under the Bugfix Workflow, because
it is the only #502 assertion that is RED against HEAD without requiring a signature or seam change
first.

## Acceptance Criteria

Each item is independently verifiable and traceable to a numbered invariant or a named test.

**Defect fixes**

- [ ] AC-01 (#462, I-462.1) `_closePending` is replaced by `_closeInFlight` and `_closeCompleted` with
      distinct documented meanings; `_closeInFlight` is cleared in a `finally` around the
      `_host.Close(reason)` call and reads `false` on every exit from `CloseCore` — success,
      not-closed, throw, and released.
- [ ] AC-02 (#462, I-462.2) After a `CloseCore` that returned `true`, with the host open again and the
      coordinator not released, `RequestOpen()` reaches `_host.OpenAsync` and returns a task that is
      not the `ClosedTask` sentinel.
- [ ] AC-03 (#462, I-462.3/I-462.4/I-462.5) Idempotent close, generation monotonicity, and released
      terminality are all preserved: two closes with no intervening reopen reach `_host.Close` exactly
      once; a close returning `false` does not increment `_generation`; after `Release()`,
      `RequestOpen` returns the sentinel and `CloseCore` returns `false` without touching `_host`.
- [ ] AC-04 (#500, I-500.1) At the moment the guarded action executes,
      `Monitor.IsEntered(BreadcrumbCoordinatorUpgradeLifetime._sync)` observed from inside the action
      is `false`.
- [ ] AC-05 (#500, I-500.2) At the moment `IWebViewMessenger.PostJson` is invoked on an attached
      surface, `Monitor.IsEntered(BreadcrumbMessengerHub._sync)` observed from inside that surface is
      `false`.
- [ ] AC-06 (#500, I-500.3) `TryRunCurrent` returns `true` for an action that re-entrantly calls
      `lifetime.Invalidate()`, and `lifetime.IsCurrent(lease)` is `false` immediately afterwards; the
      return value is not retro-actively falsified.
- [ ] AC-07 (#500, I-500.4) A re-entrant `Attach` or `Detach` performed from inside a surface's
      `PostJson` does not throw `InvalidOperationException: Collection was modified`; the broadcast
      completes and the re-entrant call takes effect.
- [ ] AC-08 (#501, I-501.1) With two attached surfaces that both increment an attempt counter before
      throwing, a single `hub.PostJson` produces a total attempt count of exactly 2. The assertion is
      order-independent and does not rely on `Dictionary.Values` enumeration order.
- [ ] AC-09 (#501, I-501.2) With one throwing surface and one recording surface attached, the
      recording surface's `Posted` collection contains the payload after `PostJson` returns.
- [ ] AC-10 (#501, I-501.3) After a partially failed broadcast, a freshly attached surface replays a
      state that the surviving surface already received.
- [ ] AC-11 (#501, I-501.4 + SR-3) A per-surface delivery failure is logged through the hub's existing
      `log4net` logger, and `PostJson` does not propagate the surface throw to its caller.
- [ ] AC-12 (#502, I-502.1) `RunSynchronous` returns `bool`, returning `false` when and only when the
      guarded action did not run, and both `SetSuggestions` and `AddItems` consume the value.
- [ ] AC-13 (#502, I-502.2) After a `SetSuggestions` whose lease was superseded, `SuggestionsUpgrade`
      is not reference-equal to the previously captured incomplete task; it is `Task.CompletedTask`.
- [ ] AC-14 (#502, I-502.4) A superseded `AddItems` settles its lease via `Abandon`, and the
      deliberate discard of the unobservable handle is documented in an XML comment.

**Companion defect**

- [ ] AC-15 (companion, I-502.3) Every lease returned by `BeginPopulation` reaches `Settled == true`
      and `SourceDisposed == true`, including a lease whose guarded action was skipped; no
      `CancellationTokenSource` is leaked per superseded population.

**Failing-first regression tests (one per defect, RED before the fix)**

- [ ] AC-16 (#462) A regression test in
      `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` asserts I-462.2 using
      `CoordinatorHarness`, `ControlledHost.SetOpen`, and explicit `DrainUntil`; it is demonstrated
      RED against HEAD before the fix and green after.
- [ ] AC-17 (#500) A regression test in
      `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` asserts I-500.1 via
      `Monitor.IsEntered` against the reflected `_sync`; it is demonstrated RED against HEAD before the
      fix and green after.
- [ ] AC-18 (#501) A regression test in `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs`
      asserts I-501.1 with two counting-and-throwing surfaces; it is demonstrated RED against HEAD
      (total attempts 1) before the fix and green after (total attempts 2).
- [ ] AC-19 (#502 companion) The lease-leak regression test in
      `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` compiles against HEAD,
      is demonstrated RED there, and is green after the fix. It is authored **first** among the #502
      tests, since it is the only #502 assertion that is RED without a signature or seam change.

**Existing tests that must pass unmodified**

- [ ] AC-20 `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose`
      (`BreadcrumbDropDownOpenCoordinatorTests.cs:262-280`) passes with no edit to the test file.
- [ ] AC-21 `SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired`
      (`BreadcrumbDropDownOpenCoordinatorTests.Part2.cs:120-140`) passes with no edit to the test file.
      Together with AC-20 this is what rules out the naive #462 fix.
- [ ] AC-22 (I-501.5) `Attach_ReplayFailureRollsBackSubscriptionAndAllowsRetry`
      (`BreadcrumbMessengerHubTests.cs:198-217`) passes with no edit to the test file.

**Structure, ownership, and budget**

- [ ] AC-23 (SR-1) `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs` exists and contains
      `SetSuggestions`, `SetSuggestionsCore`, `SuggestionsUpgrade`, `PopulateSuggestionsAsync` and
      `AddItems`; `QuickFiler/QuickFiler.csproj` gains **exactly one**
      `<Compile Include="Viewers\BreadcrumbBridgeCoordinator.Suggestions.cs" />` line and no other
      edit.
- [ ] AC-24 `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorSupersessionTests.cs` exists and
      `QuickFiler.Test/QuickFiler.Test.csproj` gains **exactly one** `<Compile Include>` line for it,
      inserted adjacent to the sibling `BreadcrumbBridgeCoordinatorTests` entry (not by alphabetical
      order, which that item group does not follow). No third new test file is added.
- [ ] AC-25 No file in the change set exceeds 500 lines after the change, verified by line count on
      every added and modified `.cs` file.
- [ ] AC-26 The diff writes none of `WebView2Messenger.cs`, `WebView2BreadcrumbHost.cs`,
      `BreadcrumbItemViewerLifecycleCoordinator.cs`, `BreadcrumbPopupUiOperations.cs`,
      `BreadcrumbDropDownHost.cs`, or `ItemViewer.Breadcrumb.cs`.
- [ ] AC-27 No test added or modified by this change uses `Thread.Sleep`, `Task.Delay`, a real
      wall-clock wait, a temporary file, or a second thread for ordering; every ordering is driven by
      an injected delegate, a reflected `Monitor.IsEntered` probe, or an explicit
      synchronization-context drain.
- [ ] AC-28 (cross-cutting NFR) `TryRunCurrent`'s `bool` is the entry-time currency verdict only; no
      post-action currency re-check is folded into the return value, and AC-06's test guards this.

**Toolchain**

- [ ] AC-29 `dotnet tool run csharpier format .` applied and `dotnet tool run csharpier check .`
      reports no differences.
- [ ] AC-30 `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
      completes with no analyzer errors.
- [ ] AC-31 `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
      completes clean, with no `/p:Nullable=enable` added and `/t:Rebuild` (not `/t:Build`) used.
- [ ] AC-32 `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` passes with the full
      `QuickFiler.Test` suite green and no coverage regression on changed lines; all four toolchain
      steps pass in a single uninterrupted final pass.

## Risks & Mitigations

- **Risk: the naive #462 fix silently removes repeated-close suppression.** Two named existing tests
  encode that contract. *Mitigation:* SR-4 adopts the two-flag form, verified against all nine
  `CloseCore`-exercising tests in research §3.1; AC-20 and AC-21 gate it.
- **Risk: `BreadcrumbBridgeCoordinator.cs` breaches the 500-line cap mid-plan.** *Mitigation:* SR-1
  performs the partial split, and the split must precede or be folded into the #502 call-site change
  so the file never crosses 500 at any commit. AC-25 gates the end state.
- **Risk: `BreadcrumbMessengerHub.cs` lands at ~474 lines — safe but tight.** *Mitigation:* keep the
  `PostJson` rewrite compact and place the per-surface catch body on the existing `SafeUnsubscribe`
  logging pattern rather than introducing a new helper. If the estimate is exceeded, extract the
  broadcast loop into a small private method rather than adding another file.
- **Risk: strengthening `TryRunCurrent`'s `bool` re-creates #502.** *Mitigation:* the cross-cutting
  NFR, plus AC-06/AC-28 and the I-500.3 test as a standing regression guard.
- **Risk: an order-dependent #501 test passes vacuously on HEAD.** *Mitigation:* the two-throwing-
  surface attempt-counter shape mandated by AC-08 is order-independent by construction; AC-18
  additionally requires the test to be demonstrated RED against HEAD.
- **Risk: concurrent guarded actions after the lock narrows.** Two threads could now both pass the
  currency check. *Mitigation:* not reachable on current wiring (all guarded actions run on the
  captured dispatcher boundary; `RunSynchronous` is reached only from the viewer thread). Recorded
  explicitly here rather than left implicit; a genuinely non-re-entrant guard is deferred to its own
  issue.
- **Risk: merge conflict with sibling epic children in the two project files.** *Mitigation:* exactly
  one line is added to each, placed adjacent to the sibling entry for the same production type, which
  minimises the conflict surface.

## Rollout & Follow-up

- **Release/rollout steps.** Standard: land on a feature branch, full toolchain pass, PR against the
  epic's target branch. No flag, no staged rollout, no migration.
- **Post-fix monitoring or clean-up tasks.**
  - Watch the new `BreadcrumbMessengerHub` per-surface failure log entries. A steady stream would
    indicate the dispose-without-detach ordering in `BreadcrumbItemViewerLifecycleCoordinator` is
    firing routinely, which is feature 488's business (Cross-feature note 2).
  - File a follow-up issue for research §6.2 option C (a non-re-entrant upgrade-lifetime guard), which
    is explicitly out of scope here.
  - Record the SR-4 known limitation (`_closeCompleted` residual) against feature 488's host paths.
- **Links.**
  - Issue: https://github.com/drmoisan/TaskMaster/issues/501 (also closes #462, #500, #502)
  - Issue folder: `docs/features/active/breadcrumb-coordinator-hub-defects-501/issue.md`
  - Research: `docs/features/active/breadcrumb-coordinator-hub-defects-501/research/2026-08-24T09-12-breadcrumb-ordering-invariants-research.md`
  - Promoted potential documents:
    - `docs/features/potential/promoted/2026-08-07-breadcrumb-dropdown-coordinator-stale-closepending-drops-reopen.md`
    - `docs/features/potential/promoted/2026-08-08-breadcrumb-webview-post-executes-under-upgrade-lifetime-lock.md`
    - `docs/features/potential/promoted/2026-08-08-breadcrumb-hub-postjson-caches-before-broadcast-starves-attachments.md`
    - `docs/features/potential/promoted/2026-08-08-breadcrumb-suggestions-upgrade-silently-stale-on-superseded-lease.md`
