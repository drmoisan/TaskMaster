# QA Gate — AddItemsCore seam and its supersession test (P7-T6 remediation)

Timestamp: 2026-08-27T23-31

Command: see the coverage-delta artifact of the same timestamp for the measurements this record explains.

EXIT_CODE: 0

Output Summary: a coverage shortfall found by P7-T6 was remediated by extending the SR-5 seam pattern
already ratified for `SetSuggestionsCore`. The SR-1 split pair moved from 98.9726% back to 100.0000%
and new/changed-line coverage moved from 96.5116% to 100.0000%.

## What P7-T6 found

On the first post-merge measurement the SR-1 split pair scored 289/292 lines, a delta of **-1.0274 pp**
against the 280/280 baseline. P7-T6's acceptance requires every per-file delta to be at or above
-0.50 pp, so this FAILED the gate. The three uncovered lines were the whole of the `if (!ran)` block at
the end of `AddItems`:

```csharp
if (!ran)
{
    _upgradeLifetime.Abandon(lease);
}
```

## Why the lines were unreachable

`AddItems` takes a fresh lease from `BeginPopulation` and passes it straight to `RunSynchronous`.
`BeginPopulation` sets `_current` to that lease and bumps `_generation` to match, so
`IsGenerationCurrentCore` is true by construction and `ran` is always true on a single thread. The
`false` branch is reachable only under a concurrent supersession race, and the unit-test policy forbids
a second thread or a wall-clock wait to provoke one.

## Why the block was not simply deleted

`RunSynchronous` already calls `Abandon(lease)` on every `false` return, so the caller's `Abandon` is
redundant in effect. It is nonetheless DELIBERATE: the XML documentation on `RunSynchronous` states
that "the skip-path `Abandon` here is idempotent with respect to a caller that also calls it" and that
"`AddItems` settles its lease on `false`". Deleting the block would contradict a documented contract
and would silently change AC-14's subject matter. It was kept.

## The remedy

`AddItemsCore(items, lease)` was extracted as an `internal` seam, mirroring `SetSuggestionsCore`
exactly. `AddItems` now takes the lease and delegates; the test drives `AddItemsCore` with an
already-superseded lease and asserts the skip.

This remedy adds no file and no project-file entry, so SR-1, AC-23 and AC-24 are unaffected:

| Constraint | Effect |
| --- | --- |
| new production file | none; `AddItemsCore` lives in the existing `.Suggestions.cs` part |
| new test file | none; the test joins `BreadcrumbBridgeCoordinatorSupersessionTests.cs` |
| new `<Compile Include>` line | none |
| 500-line budget | `.Suggestions.cs` 111 to 123; supersession tests 140 to 191; both well under |
| determinism (AC-27) | one thread, no timer, no wall-clock wait, no temporary file |

`AddItems` remains a consumer of the `RunSynchronous` verdict for AC-12 purposes, through the same
entry-point-delegates-to-core shape that `SetSuggestions` already uses and that AC-12 already accepts.
