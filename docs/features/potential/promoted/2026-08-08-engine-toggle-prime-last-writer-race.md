# engine-toggle-prime-last-writer-race (Issue #525)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/engine-toggle-prime-last-writer-race/ (Issue #525)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Work Mode: minor-audit

- Issue: #525
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/525
- Last Updated: 2026-08-09
## Summary

`EngineToggleStateCoordinator.ApplyPrimeAsync` writes the prime result into `_pressedState` unconditionally. An in-flight prime that read the engine state *before* a toggle flipped it can therefore land *after* the toggle path wrote the fresh value, overwriting it with a stale one. Because a successful prime leaves its marker registered in `_primeTasks`, no re-prime ever occurs, so the stale toggle display persists for the rest of the session until the user clicks the toggle again.

Found by the feature review of the bundled #505/#506/#518 delivery, recorded as finding CR-1 in `docs/features/active/2026-08-08-ribbon-engine-toggle-state-guards-505/code-review.2026-08-08T21-59.md`. Dispositioned non-blocking there and promoted here rather than widening that delivery's scope.

## Environment

- OS/version: Windows 11, Outlook desktop (VSTO add-in host)
- Runtime: .NET Framework 4.8.1, TaskMaster VSTO add-in
- Command/flags used: Outlook Explorer ribbon, Spam Manager and Triage configuration menus
- Data source or fixture: Live Outlook profile during initial configuration load

## Steps to Reproduce

1. Start Outlook and open the Spam Manager (or Triage) configuration menu early, while `Globals.AF.Manager.Configuration` is still loading, so `GetPressed` starts a prime.
2. Click the engine-enabled toggle while that prime is still in flight.
3. Let the toggle complete (it writes the fresh value and invalidates), then let the prime continuation complete.
4. Reopen the menu and observe the toggle rendering the pre-toggle (stale) state.

## Expected Behavior

The cache converges to the true engine state. A prime must never overwrite a value written by the authoritative toggle path, and any cache write that leaves the display stale must be followed by a correcting refresh.

## Actual Behavior

Interleaving, per the review:

```
prime EngineActiveAsync resolves with the pre-toggle value
  -> toggle writes the fresh value + invalidates
  -> prime continuation writes the STALE value + invalidates
  -> GetPressed answers stale
  -> _primeTasks.ContainsKey blocks any re-prime for the session
```

The invalidation issued after the stale write makes Office re-query and read the stale cache, so the invalidation does not rescue it. The underlying configuration is correct throughout; only the displayed state is wrong.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: see the interleaving above. Relevant source: `EngineToggleStateCoordinator.ApplyPrimeAsync` (cache write), `ExecuteToggleAsync` (toggle write + invalidate), and the `_primeTasks.ContainsKey` re-prime guard in `StartPrimeIfNeeded`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Display-only, and strictly better than the pre-#505 behavior in which the toggles never reflected engine state at all. The window is narrow (a prime resolving concurrently with a click completion during initial configuration load) and the next click both operates on the true state and refreshes the cache.

## Suspected Cause / Notes

The design assumes "every cache write is followed by an invalidation, so the UI converges even when a prime and a toggle overlap". That assumption does not hold when the *stale* writer is the last writer, because the subsequent invalidation is answered from the stale cache.

Two related defects found in the same review, worth fixing together:

1. **Canceled prime is silently ignored** (review finding CR-2). `CompletePrime` reads `Task.Exception`, which is `null` for a canceled task, so it returns early: the prime marker stays registered, the key never re-primes, the cache stays unset, and nothing is logged. Low likelihood today because `EngineActiveAsync` carries no cancellation token.
2. **Uncovered defensive guard** (review finding CR-3). The `InvalidOperationException` guard in `ExecuteToggleAsync` is the only uncovered code in the new type (2 lines; the type sits at 99.15%). It is trivially reachable from the existing test harness with `EnginesAvailable = false` plus a direct `ExecuteToggleAsync` call.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: prime-vs-toggle interleaving (prime must not clobber a toggle write), canceled-prime handling, and the direct-caller `InvalidOperationException` guard. `EngineToggleStateCoordinator` is host-neutral and already unit-tested, so all three are testable at the existing seam with `TaskCompletionSource`; no sleeps, no live Outlook.
- [ ] Integration scenario to retest: open the configuration menu during startup, toggle while the prime is in flight, reopen the menu.
- [ ] Manual verification notes: covered by the same live-Outlook pass as the #505 AC-22 checklist.

Reviewer's recommended fix for the primary defect: in `ApplyPrimeAsync`, replace `_pressedState[engineName] = active` with `_pressedState.TryAdd(engineName, active)` and invalidate only on a successful add, so a prime can never overwrite the authoritative toggle path. Note that this closes prime-vs-toggle but not the residual toggle-vs-toggle double-click interleaving, which needs write versioning; scope that explicitly when the issue is worked.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
