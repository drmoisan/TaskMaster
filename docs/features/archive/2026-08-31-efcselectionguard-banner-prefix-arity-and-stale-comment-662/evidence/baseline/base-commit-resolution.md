# Base Commit Resolution (P0-T2)

Timestamp: 2026-09-01T15-39

Command: `git rev-parse --verify 2b85134b42872e405602e6064e02dc9cda6c319b^{commit}`

EXIT_CODE: 0

Output Summary:

The command printed the 40-character object name
`2b85134b42872e405602e6064e02dc9cda6c319b`, which is equal to the anchor named
in the plan's "Working Directory and Base Commit" section. The anchor therefore
resolves in this worktree.

## Execution Amendment — the resolved anchor is NOT the gate anchor

The plan carries the section "Execution Amendment — corrected diff anchor
(orchestrator, 2026-09-01)", inserted immediately before "Fail-Closed Evidence
Rules". That amendment is binding and it overrides the pinned anchor in exactly
three tasks: P2-T16, P2-T18 and P2-T23.

The reason recorded by the amendment is that the pinned anchor is an ancestor
of both this branch's HEAD and of `origin/main`, and `origin/main` has advanced
well past it. The two-dot `git diff <anchor> -- <paths>` form therefore reports
every change `origin/main` accumulated since the anchor in addition to this
branch's own work, which makes those three gates unsatisfiable as written. The
amendment records that this was measured rather than predicted: run before any
edit existed, P2-T23's listing returned 22 paths against an asserted union of
4, and the same listing anchored at the merge base returned empty.

The gate anchor used by P2-T16, P2-T18 and P2-T23 is therefore resolved at run
time by `git merge-base origin/main HEAD`, never pasted as a literal, so that a
later reconciliation merge cannot re-stale it.

Merge base resolved at the start of this Phase 0 run:

- `git merge-base origin/main HEAD` -> `43dcc800e5c75ab1d1033f0eac0e4b61ac919b59`
- `git rev-parse origin/main` -> `43dcc800e5c75ab1d1033f0eac0e4b61ac919b59`

`origin/main` and the merge base are the same commit, which confirms that
`origin/main` is currently an ancestor of HEAD after the execution-start
reconciliation merge. This value is re-derived at every phase boundary rather
than cached, because a sibling item merging mid-run moves `origin/main` and
nothing in the local worktree signals it.

This task is recorded as passing on its own terms: the pinned anchor resolves,
`EXIT_CODE` is 0, and the printed object name equals the anchor. Recording that
it resolves while the three gates no longer use it is the evidence that the
substitution was deliberate rather than a workaround for an unresolvable ref.
