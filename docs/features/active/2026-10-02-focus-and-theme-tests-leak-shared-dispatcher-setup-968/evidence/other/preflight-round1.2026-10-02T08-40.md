# Preflight round 1 record (issue #968)

- Timestamp: 2026-10-02T08-40
- Reviewer: atomic-executor, DIRECTIVE: PREFLIGHT VALIDATION ONLY, read-only, no build or test access (pwsh refused under worktree isolation)
- Plan: plan.2026-10-02T05-42.md, blob 91b1232718a914b6b3e3904c1f8b3e7d3f754b28
- Result: PREFLIGHT: REVISIONS REQUIRED
- Convergence: CONVERGENCE: NO FURTHER ROUNDS EXPECTED (conditional on deltas applied verbatim)
- Defects reported: 10
- Status: deltas NOT yet applied; run held by coordinator directive before revision.

## Defects (summary; verbatim deltas are in the reviewer's report and must be re-applied by atomic-planner)

1. Payload channel: no rule for a refused pwsh invocation. Add a PWSH CHANNEL REFUSED stop rule; executor must run non-isolated.
2. Fact 11 is wrong: the branch had no commits above BASE when preflight ran. Since then the promoted record docs/features/potential/promoted/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup.md was committed in 53d975270 together with the feature documents, so it is now part of the inherited committed set (under docs/features/potential/, which P0-T3 already admits). Re-derive fact 11 and the INHERITED-COMMITTED expectation against the current branch; the staged-record delta (option B) is no longer needed.
3. F-FIELDS comment contains the literal `lock (FieldLock)`, making the post-change count 6, not 5. Reword the comment to "only while FieldLock is held".
4. P5-T2 nesting gate is unsatisfiable for test 4 (fresh pin acquired after earlier pin releases). Replace with per-pin, per-transaction ordering.
5. Test 4 cannot fail if the ownership flag is not cleared. Replace with the two-transaction variant (second transaction installs the captured parked instance; a pin under it must leave it in place). Knock-on count updates: P1-T3 PC tokens, P5-T1 PRIMARY 16 / CROSS 32 / CONTROL 28, P6-T18 13 of 13, spec item 4 and census sentence.
6. Indentation statement wrong for N1 and T1 (shown with an extra four-space Markdown indent).
7. WORKTREE must be written with forward slashes in git -C arguments.
8. AC11 literal `installed nothing carries` not recorded; add to the FIX token list (baseline vacuous, post 0).
9. P3-T9 wording: the TS list, not FT, is extended with "Issue #480 shared arrange helper".
10. No defined outcome for tests that relied on the leaked parked dispatcher (message "The UI dispatcher has not been captured"): add MESSAGE capture to CMD-COVERAGE-POST, a LEAK-DEPENDENT TEST EXPOSED stop in P6-T5, and a Risks bullet.

## Verified correct by the reviewer

Ordering and satisfiability of the expect-fail run, line and token citations other than defect 3, census 20/9/23, csproj lines, concurrency design (no pin spans a gate release after the change; no field write outside a transaction), R4 option (a), commands targeting this worktree, bootstrap ordering, evidence projections and paths, tonality.

## Remaining

Apply the deltas in place (atomic-planner), re-validate with the MCP plan validator, run a confirming preflight round (at least one round with build access from a non-isolated session), then write the clearance artifact.
