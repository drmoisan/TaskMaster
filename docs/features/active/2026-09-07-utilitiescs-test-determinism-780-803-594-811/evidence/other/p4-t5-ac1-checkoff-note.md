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

## Resolution of that dependency (added 2026-09-08T10-40, after P8-T6 ran)

DEPENDENCY: DISCHARGED. The AC1 check-off stands.

P8-T6 ran ten consecutive full-suite runs on the unchanged source commit `03b7bd57` at 24 workers.
`TryAddValuesAsync_UpdatesExistingValue` read `Passed` in **all ten**, which is precisely the
clause this note was tracking: "the test passes deterministically under 24-worker parallel coverage
runs".

Nine of the ten runs were completely clean. Run 7 reported one failure, but it was a different
test in a different assembly area,
`MethodBodyReader_Tests.GetBodyCode_ReturnsConcatenatedInstructions`, caused by an unsynchronised
static in `ILGlobals` that lies outside this item's write set (see `p8-t4-ac4-runs.md`). That
failure defeats **AC4**, whose condition is a property of the whole run rather than of one test,
and AC4 is correspondingly left unchecked. It does not bear on AC1, whose condition names this
one test, and that test did not fail once in ten runs.

This supersedes the earlier conservative wording in this note, which said the check-off must be
reverted if P8-T6 did not produce ten clean runs. That wording conflated the two criteria: AC1's
clause is about the #780 test specifically, and AC4's is about the run as a whole. The precise
condition for AC1 was met.

The reconciliation task P8-T11 lists `fail-before-exception`, `p4-t4-ac1-pass-after.md` and
`p8-t6-ac4-ten-run.md` as the three artifacts that jointly discharge AC1.

## Acceptance evaluation

- `'- [x] AC1:'` counts 1 and `'- [ ] AC1:'` counts 0 in spec.md. Verified in the same task run.
- This note artifact exists and names P8-T6 as the evidence for the parallel-load clause. PASS

## Output Summary

AC1 checked off in spec.md on the strength of the structural deletion proof and the P4-T4 pass-after
run. The parallel-determinism clause remains dependent on P8-T6 and is recorded as such.
