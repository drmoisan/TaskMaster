---
name: 505-coordinator-prime-toggle-race
description: 'EngineToggleStateCoordinator (#505 review CR-1): lazy prime can overwrite a fresher toggle-written cache value and persist stale for the session; recommended TryAdd fix, promotion recommended — check status in any later ribbon/coordinator review'
metadata:
  type: project
---

The #505/#506/#518 review (2026-08-08, PASS with 0 blocking) recorded one Major non-blocking finding, CR-1: in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, `ApplyPrimeAsync` writes `_pressedState[engineName] = active` unconditionally. An in-flight prime that read `EngineActiveAsync` before a toggle flipped the setting can write its stale value AFTER `ExecuteToggleAsync` wrote the fresh one; the successful prime task stays registered in `_primeTasks` (`ContainsKey` guard), so no re-prime ever occurs and the stale display persists until the next click. The spec's "every cache write is followed by an invalidation, so the UI converges" assumption fails in this interleaving.

**Why:** display-only, narrow window (prime resolving concurrently with a click during initial config load), strictly better than merge-base behavior, so dispositioned non-blocking with a recommended fix (`_pressedState.TryAdd` in the prime; residual toggle-vs-toggle double-click case needs write versioning) and a promotion recommendation. Also recorded: CR-2 canceled prime ignored by `CompletePrime` (`Task.Exception` null for Canceled -> marker never removed, key never re-primes, nothing logged); CR-3 the 2 uncovered defensive-guard lines are trivially testable with the existing harness.

**How to apply:** in any later review touching `EngineToggleStateCoordinator`, the ribbon toggle surface, or a follow-up issue citing this race, check whether CR-1/CR-2 were promoted or fixed before re-deriving the interleaving. Full analysis: `docs/features/active/2026-08-08-ribbon-engine-toggle-state-guards-505/code-review.2026-08-08T21-59.md`.
