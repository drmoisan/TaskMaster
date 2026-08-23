---
name: verify-branch-family-additions-only-vs-main
description: An epic branch family cut before a docs-restoring merge will silently DELETE those files at fan-in; check git diff --name-status main..branch is additions-only before any fan-in or merge
metadata:
  type: feedback
---

Before any fan-in or merge, verify every branch in the epic family is additions-only against current
`main`:

```
git fetch origin
git diff --name-status origin/main..origin/<branch>   # expect only A lines; any D is a deletion you would ship
git merge-base --is-ancestor origin/main origin/<branch>   # must succeed
```

**Why:** on 2026-08-21 the `quickfiler-suite-determinism-foundation` family was cut from `main` at
`b9a9b92c`. Mid-run, PR #582 merged and moved `main` to `790bd39e`, restoring 61 promoted potential
records that had existed only on an unmerged branch. Measured against the new `main`, all four child
preservation branches and the integration branch showed `A=5 D=58` — every one of the 58 deletions
under `docs/features/potential/promoted/`. Merging any of them would have deleted 58 of the 61
records that had just been rescued, and the diff was the only thing that showed it: each branch was
internally consistent, every commit was intentional, and nothing conflicted.

It was 58 rather than 61 only because three of those records had already been restored onto the
branch family, so those three matched on both sides. That coincidence is what makes the number look
arbitrary; do not read a "nearly all" figure as a partial problem.

**How to apply:**
- Re-run the check after ANY long-running phase, not just at creation. A preservation commit made
  early in a multi-hour run is a snapshot of a `main` that may no longer exist.
- Fix by merging, not rebasing, once the branch is pushed and children are branched from it:
  merge `origin/main` into the integration branch, then merge the integration branch into each child.
  Rebasing a pushed branch that children were cut from orphans them.
- Re-verify after the merge and require `D=0` and `M=0`. A useful independent cross-check is a file
  count on the at-risk directory: it must be EQUAL on `main` and on every branch
  (`git ls-tree -r --name-only origin/<ref> -- <dir>/ | wc -l`).
- A `D` line is not necessarily a conflict. Git merges this cleanly and reports nothing, because
  deleting a file the branch never knew about is a perfectly valid merge result.

Related: [[recover-dead-prep-child-by-committing-then-relaunching]],
[[quickfiler-potential-docs-stranded-on-stale-epic-branch]].
