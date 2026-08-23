---
name: preserve-halted-child-worktree
description: A halted child's work is uncommitted and dies with its worktree; commit and push it immediately, but check evidence volume first because a full-suite child can leave hundreds of MB of raw .trx
metadata:
  type: feedback
---

When a child halts or is abandoned, its work is typically **uncommitted**. Commit and push it to
its own branch before doing anything else — but run `du -sh` on its evidence tree first.

**Why:** A halted child's worktree is the only copy of its evidence, and a locked worktree is one
`git worktree remove -f -f` away from gone. In the QuickFiler determinism epic, child #511 left 134
evidence files representing about six hours of measurement, none of it committed.

The trap is volume. That evidence totalled **1.2 GB**, of which 56 raw `.trx` result files were
about **359 MB and 1.86 million lines of XML**. I committed and pushed the lot before measuring it,
then had to rewrite the commit. Committing large evidence IS a repository norm here —
`docs/features/archive` carries individual cobertura XML files of 30 to 44 MB, and a single child's
two cobertura files accounted for 375,600 of one pull request's 393,000 insertions — but 359 MB in
one commit is an order of magnitude past that norm.

**How to apply:** Before `git add -A` in a halted child's worktree, run
`du -sh <feature>/evidence` and `find <feature>/evidence -name '*.trx' | wc -l`. Commit the
markdown artifacts, which carry the `Command`, `EXIT_CODE`, and `Output Summary` the evidence
conventions actually require, and leave raw `.trx` on disk only — they are regenerable by re-running
the recorded commands.

If you must exclude paths, scope the exclusion to that feature's folder. `git rm --cached` fed from
a repo-wide `git ls-tree | grep '\.trx$'` will happily stage the **deletion** of 43 `.trx` files
belonging to other features that were merely inherited from the integration branch. Prefer
`git reset --mixed <parent>`, then `git add -A`, then
`git reset -- '<feature>/evidence/**/*.trx'`, and assert both
`git diff --cached --name-status | grep -c '^D'` is 0 and the staged `.trx` count is 0 before
committing. Force-pushing the corrected commit is safe while the branch is unmerged and nobody else
consumes it.

Write the commit message so no reader mistakes preservation for delivery: state in the subject that
it is NOT a fix, and use `Refs #N`, never `Closes #N`. Related:
[[feedback_premise_falsified_child_halt]].
