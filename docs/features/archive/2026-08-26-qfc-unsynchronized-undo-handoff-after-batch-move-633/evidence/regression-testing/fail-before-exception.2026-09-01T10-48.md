# Fail-before exception dossier (P2-T6)

Timestamp: 2026-09-01T10-48
Task: [P2-T6]
Working directory: WORKTREE
Issue: #633

This dossier covers the two groups of tests for which a failing pre-fix run is structurally impossible.
It does not cover the barrier defect itself, which carries a real failing run recorded in
`FEATURE/evidence/regression-testing/fail-before-run.2026-09-01T10-46.md`.

WhyFailingRunImpossible:

**Group A — the seven queue-level tests that name an API which does not yet exist.** The six
`WhenDrainedAsync_*` tests specified by P5-T2 through P5-T7 and the test
`ItemProcessor_ThatThrows_StillDecrementsAndDrainCompletes` specified by P5-T8 all assert against
`FilerQueue.WhenDrainedAsync()`, a member that P3-T6 introduces. Running them against the pre-fix tree
does not produce a failing test: it produces a compile error, because the method the test names is not
declared on the type. A build that does not produce an assembly runs no test and yields no
`outcome="Failed"` result, so there is no failing run to record. A compile error is not a clean
fail-before witness — it demonstrates the absence of an API, not the presence of the defect — and
manufacturing one by committing intentionally non-compiling code would additionally break the P2-T4
compile gate that the two genuine fail-before tests depend on.

**Group B — `Enqueue_AfterPreviousBatchDrained_ProcessesSecondBatch`, the orphaned-item regression.**
This test falls in the same structural category as Group A, and additionally cannot be converted into a
compilable pre-fix witness.

First, the structural point. As specified in P5-T7, this test releases the first gate, **awaits the
drain**, enqueues a second item, and awaits the new drain. It therefore names `WhenDrainedAsync()` and
does not compile before Phase 3, exactly as the Group A tests do not.

Second, and independently, the window it guards has no deterministic pre-fix witness at all. The
orphaned-item window requires a producer's `Queue.Add` to land strictly between the worker's loop exit
at `QuickFiler/Controllers/FilerQueue.cs:48` and the guard reinstall at `:63`. Between those two
statements there is no seam, no `await`, and no observable state change: the `while` loop's condition
fails and control falls directly to the assignment. A test cannot place a statement into that interval
deterministically, because there is no point at which it could be made to run and nothing it could
observe to know it was inside the interval. Driving the window by repetition or by timing would be a
probabilistic race, which `.claude/rules/general-unit-test.md` prohibits.

It is therefore a post-fix regression guard rather than a fail-before witness. It is nonetheless a
discriminating one, not a tautology: it fails against a handshake that leaves the consumer-running flag
set after loop exit, which is precisely the defect class the P3-T5 repair must avoid reintroducing. Its
value is prospective — it constrains future edits to the handshake — rather than demonstrative of the
present defect.

This dossier deliberately does **not** claim that this test is green before the fix as well as after.
That statement would be false: the test does not compile before the fix.

## Divergence from the delegating expectation, recorded so a reviewer can check it

The delegating agent's expectation was that the orphan window would carry the real failing run and the
drain barrier the dossier. This plan inverts that split, for the derivation given above: the orphan
window is the half with no deterministic pre-fix witness, while the barrier defect does have one,
because the `ItemProcessor` seam added in Phase 1 lets a test hold an item inside a gated processor and
observe whether `BackGroundMoveAsync` dispatched while the queue was undrained. P2-T5 executed that
witness and both tests failed on the predicted assertion, which confirms the inversion was correct
rather than merely asserted.

## Absence-of-test proof

SearchScope: `QuickFiler.Test/` and every subdirectory, searched recursively. 153 `*.cs` files were
searched. The search was performed against the tree as it stood after Phase 1 and Phase 2 and before
any Phase 3 edit, which is the state in which the claim is made.

SearchPatterns: the literal token `WhenDrainedAsync`; additionally the literal test names
`Enqueue_AfterPreviousBatchDrained_ProcessesSecondBatch` and
`ItemProcessor_ThatThrows_StillDecrementsAndDrainCompletes`.

SearchResult: none. Zero matches for `WhenDrainedAsync` across all 153 files; zero matches for each of
the two named tests. No test naming `WhenDrainedAsync` exists in `QuickFiler.Test/` before Phase 3, so
there was no pre-existing test that could have been run to produce a failing witness for Group A or
Group B.

Command: `pwsh -NoProfile -File <scratchpad>/absenceproof.ps1`
EXIT_CODE: 0
