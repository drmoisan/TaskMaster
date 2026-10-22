# Finding 3 — Structural Acceptance Checks for P3-T6 through P3-T9

Timestamp: 2026-09-03T02-48
Tasks: [P3-T6], [P3-T7], [P3-T8], [P3-T9]
Command: identifier counts and brace-matched method-body extraction over `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, using `Get-Content -LiteralPath` and escaped literal regular expressions.
EXIT_CODE: 0

This artifact records the inline acceptance checks for the four edit tasks of the Finding 3 fix. It
is the record cited by the F3-AC1 through F3-AC4 check-offs.

File measured after all four edits: `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, 515 lines
(pre-format; the authoritative post-format count is P4-T2).

## P3-T6 — the versioned pressed-state cache

| Literal | Occurrences | Required |
|---|---|---|
| `ConcurrentDictionary<string, bool>` | **0** | 0 |
| `ConcurrentDictionary<string, PressedState>` | 2 | present |
| `PressedState` | 7 | present |
| `_stateSequence` | 2 | present |
| `NextSequence` | 3 | present |
| `TryApplyState` | 4 | present |
| `StringComparer.Ordinal` | 2 | ordinal comparison retained |
| `Interlocked.Increment` | 1 | sequence written only through interlocked |

The sequence source is `private long _stateSequence`, read and written only through
`NextSequence() => Interlocked.Increment(ref _stateSequence)`. The nested type is
`private sealed class PressedState` carrying `internal bool Active { get; }` and
`internal long Sequence { get; }`, both get-only. It is a reference type deliberately, so the
concurrent dictionary's `TryUpdate` comparand check is reference identity — the compare-and-swap
semantic the guard needs. A value tuple would be compared structurally, weakening the guard to "the
value looked the same".

### The synchronous reader keeps its contract

```
 142:         internal bool GetPressed(string engineName)
 143:         {
 144:             if (!EngineToggleCatalog.TryGetControlId(engineName, out var controlId))
 145:             {
 146:                 return false;
 147:             }
 148: 
 149:             if (_pressedState.TryGetValue(engineName, out var cached))
 150:             {
 151:                 return cached.Active;
 152:             }
 153: 
 154:             StartPrimeIfNeeded(engineName, controlId);
 155:             return false;
 156:         }
```

Return type is still `bool`. Token counts inside the body: `await` 0, `throw` 0, `.Result` 0,
`.Wait(` 0, `GetAwaiter` 0. The reader never awaits, never blocks and never throws; the only change
is unwrapping `.Active` from the cached observation.

## P3-T7 — the toggle writer

```
 297:             await engines.ToggleEngineAsync(engineName).ConfigureAwait(false);
 298: 
 299:             // The ticket is taken after the toggle completes and before the activation read,
 300:             // because that is the moment this observation window opens.
 301:             var sequence = NextSequence();
 302:             var active = await engines.EngineActiveAsync(engineName).ConfigureAwait(false);
 303: 
 304:             if (TryApplyState(engineName, active, sequence))
 305:             {
 306:                 _invalidateControl(controlId);
 307:             }
```

Ordering required by the acceptance criterion, and observed: the ticket capture at line 301 appears
AFTER the toggle await at 297 and BEFORE the activation-read await at 302. The invalidation call at
306 is inside the conditional opened at 304.

Update-before-invalidate ordering is preserved: `TryApplyState` performs the cache write, and the
invalidation only runs after it returns true. P3-T12 confirms the pre-existing ordering test
`ExecuteToggleAsync_PerformsToggleThenRefreshThenCacheThenInvalidate_InOrder` still passes
unmodified.

## P3-T8 — the prime writer

```
 389:             // The ticket is taken immediately before the activation read, so a prime whose
 390:             // observation began before a toggle's cannot overwrite the toggle's newer result.
 391:             var sequence = NextSequence();
 392:             var active = await engines.EngineActiveAsync(engineName).ConfigureAwait(false);
 393: 
 394:             if (TryApplyState(engineName, active, sequence))
 395:             {
 396:                 _invalidateControl(controlId);
 397:             }
```

The ticket capture at 391 appears before the activation-read await at 392, and the invalidation at
396 is inside the conditional. Conditional invalidation is correct: a rejected write means a newer
writer already stored its value and already invalidated.

## P3-T9 — prime completion

```
 414:         private void CompletePrime(Task completed, string engineName)
 415:         {
 416:             if (completed.Status == TaskStatus.RanToCompletion)
 417:             {
 418:                 return;
 419:             }
 420: 
 421:             _primeTasks.TryRemove(engineName, out _);
 422: 
 423:             var failure =
 424:                 (Exception)completed.Exception?.GetBaseException()
 425:                 ?? new TaskCanceledException(completed);
 426: 
 427:             _logError(BuildPrimeFailedMessage(engineName), failure);
 428:         }
```

- The method tests the completed task's STATUS against `TaskStatus.RanToCompletion`, not its
  exception. That is the whole defect: a canceled task carries a null `Exception`, so the previous
  exception-keyed early return fired for a cancellation.
- The marker removal (421) and the log call (427) are both on the non-completed path, below the
  early return.
- The faulted path still unwraps the base exception via `completed.Exception?.GetBaseException()`
  (424). An existing test asserts that unwrapped instance by reference, and P3-T12 records it as
  still passing.
- A `TaskCanceledException` is synthesized only when there is no exception to unwrap.
- `BuildPrimeFailedMessage` is reused unchanged; its text reads correctly for a cancellation, so no
  new message builder was added.

## Note carried to Phase 4

The file is 515 lines before the final format pass, which is above the 500-line ceiling. The
authoritative measurement is P4-T2, taken after P4-T1's format pass, and P4-T3 resolves the
contingency from that number.

Output Summary: All four edit tasks satisfy their stated acceptance conditions. The cache carries
zero `ConcurrentDictionary<string, bool>` occurrences and all four required new identifiers; the
reader still returns `bool` with no await, no blocking call and no throw; both writers capture their
ticket immediately before the activation read and invalidate only inside the conditional; and prime
completion tests status rather than exception, clearing the marker and logging on every non-completed
outcome while preserving base-exception unwrapping. The file measures 515 lines pre-format, so the
P4-T3 contingency is live.
