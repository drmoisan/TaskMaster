# P0-T18 — Pre-change exit-shape counts of the predicate (baseline)

Timestamp: 2026-09-13T23-11

Command: six counts of the form
`(Select-String -Path UtilitiesCS\Threading\UiThread.cs -Pattern '<pattern>').Count`

EXIT_CODE: 0

Output Summary:

### The four exit-shape counts (the positive control for the AC1 gate in P2-T7 and P4-T15)

| # | Pattern | Recorded | Expected |
|---|---|---|---|
| 1 | `^\s*return ` | **9** | 9 |
| 2 | `^\s*return true;\s*$` | **2** | 2 |
| 3 | `^\s*return false;\s*$` | **2** | 2 |
| 4 | `^\s*return _context is DispatcherSynchronizationContext` | **1** | 1 |

Every count equals its expected value, so the FAIL condition is not met.

The whole-file count of 9 is the five accessor exits plus four returns in other members. Matched
line numbers, confirming that decomposition: **162, 169, 173, 178, 184** (the five accessor exits,
in source order) and **200, 212, 266, 285** (the four returns in other members), exactly as the
plan states.

### The two positive controls for the P2-T1 acceptance condition

| Pattern | Recorded | Expected |
|---|---|---|
| `_dispatcher is not null` | **0** | 0 |
| `Dispatcher\.FromThread\(Thread\.CurrentThread\)` | **1** | 1 |

Both equal their expected values, so the FAIL condition is not met. The single
`Dispatcher.FromThread(Thread.CurrentThread)` occurrence is at **line 186**, inside the dispatcher
exit, and is the fully qualified spelling
`System.Windows.Threading.Dispatcher.FromThread(Thread.CurrentThread)`.

P2-T1 asserts a delta of exactly one against each of these two counts: after the hardening the
`_dispatcher is not null` count must be 1 (up from 0) and the `Dispatcher.FromThread` count must be
2 (up from 1). Recording the pre-change values here is what makes those two assertions falsifiable.
