---
name: checkout-collision-set-is-larger-than-status-suggests
description: git status --porcelain collapses an untracked DIRECTORY into one entry, so intersecting it with git diff HEAD..plan-branch under-reports the checkout collision set; let the failed checkout name the files instead
metadata:
  type: reference
---

Verified 2026-09-17 committing the `bugs-2026-09-17` run manifest.

**The trap.** Before switching the session worktree to the plan-home branch, I tried to predict the
collision set with `set(git status --porcelain) & set(git diff --name-only HEAD <plan-branch>)`. It
returned 6 files, all `" M"` tracked-modified. I cleaned those 6 and the checkout still aborted — on
10 UNTRACKED files under `docs/features/active/` that are tracked on the plan branch.

**Why.** `git status --porcelain` collapses a wholly-untracked directory into a single entry ending
in `/` (`?? docs/features/active/2026-09-02-.../`). The diff side lists individual FILE paths, so
the intersection can never match. The prediction is structurally blind to exactly the class the
memory on [[planner-git-commits-must-be-single-bare-segments]] warns about.

`git status --porcelain -uall` would expand them, but there is no reason to predict at all: the
failed checkout's own indented lines ARE the collision set, and git refuses rather than destroying
anything. **Just attempt the checkout and read the list.** Budget for two rounds — tracked-modified
collisions and untracked collisions surface separately, because cleaning the first class lets git
get far enough to discover the second.

**Both classes are recoverable, by different means.** Tracked-modified: copy to the scratchpad, then
`git checkout -- <paths>`. Untracked: MOVE to the scratchpad (a delete is unrecoverable — they exist
on no ref reachable from the session branch). Restore both by copying back after switching home, and
verify with `filecmp.cmp(shallow=False)` rather than trusting the copy.

The untracked `docs/features/active/**/issue.md` and `spec.md` files accumulate from the documented
planner-hook workaround where a child publishes its own two documents to the session root. They are
the standing source of this collision on every run.

**Also confirmed this run:** `artifacts/` is gitignored, so the planner checkpoint and the working
kickoff copy survive both branch switches untouched — write them before, during, or after, it does
not matter. And `--force-with-lease=<ref>:<expected-sha>` works on the plan-home branch and is worth
using for both pushes, since it makes the fast-forward requirement explicit rather than assumed.

See [[parallel-artifact-authoring-gotchas]] for the schema-side traps.
