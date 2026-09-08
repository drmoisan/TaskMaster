# P4-T5 — AC1 check-off note and its outstanding dependency

Timestamp: 2026-09-08T09-52
Task: [P4-T5]
Command: Edit of `<FEATURE>/spec.md` line 292, changing `- [ ] AC1:` to `- [x] AC1:` with the criterion text unchanged
EXIT_CODE: 0

## Criterion

AC1: `TryAddValuesAsync` no longer cancels on a fixed wall-clock window, or the window is driven by
an injected `TimeProvider`; the test passes deterministically under 24-worker parallel coverage
runs.

## Evidence cited for the check-off

| Clause | Evidence |
|---|---|
| "no longer cancels on a fixed wall-clock window" (first disjunct) | `evidence/regression-testing/fail-before-exception.2026-09-08T09-50.md` — `CancelAfter(` and `CreateLinkedTokenSource` both moved from 1 to 0 in `UtilitiesCS/Extensions/DictionaryExtensions.cs`, with `TryAddValues(key, value), token)` moving from 0 to 1 as the positive control |
| cancellation contract preserved | `evidence/regression-testing/p4-t4-ac1-pass-after.md` — `TryAddValuesAsync_PreCancelledToken_ThrowsTaskCanceledAndLeavesValueUnchanged` passes |
| "the test passes" | `evidence/regression-testing/p4-t4-ac1-pass-after.md` — `TryAddValuesAsync_UpdatesExistingValue` passes in 2.76 ms |

AC1 is satisfied by its first disjunct. The `TimeProvider` alternative was not taken, because the
deleted window had no production consumer and guarded a bounded compare-and-swap loop, so a seam
would have preserved a deadline with no semantic (decision D3).

## Outstanding dependency at the time of this check-off

The final clause of AC1, "the test passes deterministically under 24-worker parallel coverage
runs", is **not** discharged by any evidence available at P4-T5. A single scoped run of one test
class proves the test passes; it does not prove it passes under parallel load, which is the exact
condition the #780 failure depends on.

That clause is evidenced by **P8-T6**, the ten-run AC4 gate: ten consecutive full nine-assembly
`/InIsolation` runs with `Workers = 0` resolving to the local processor count, in which
`TryAddValuesAsync_UpdatesExistingValue` must pass every time. The check-off is recorded here
together with that dependency so the dependency is auditable rather than implicit.

If P8-T6 does not produce ten clean runs, this check-off must be reverted. The reconciliation task
P8-T11 re-reads spec.md after P8-T6 has run and lists `fail-before-exception`, `p4-t4` and
`p8-t6-ac4-ten-run.md` as the three artifacts that jointly discharge AC1.

## Acceptance evaluation

- `'- [x] AC1:'` counts 1 and `'- [ ] AC1:'` counts 0 in spec.md. Verified in the same task run.
- This note artifact exists and names P8-T6 as the evidence for the parallel-load clause. PASS

## Output Summary

AC1 checked off in spec.md on the strength of the structural deletion proof and the P4-T4 pass-after
run. The parallel-determinism clause remains dependent on P8-T6 and is recorded as such.
