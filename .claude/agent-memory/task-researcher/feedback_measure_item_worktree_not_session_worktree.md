---
name: measure-item-worktree-not-session-worktree
description: Read source facts from the item worktree, not the session worktree; session HEAD often predates merged issues and silently shifts every line citation
metadata:
  type: feedback
---

When an orchestrator delegates research while your cwd is a *session* worktree
(`TaskMaster-wt/<timestamp>`), read every source fact from the **item** worktree
(`TaskMaster-wt/item-<issue>`), using absolute paths with Read/Grep. Keep writing the artifact
to the session-worktree research path the orchestrator supplied — that is where its propagation
step reads from. Grep accepts an absolute `path` argument, so Bash is not needed.

**Why:** on issue #792 the session worktree HEAD predated two merged issues (#742, #743). Every
line citation came out uniformly `-1` and three line counts were low. The result looked like a
*discovery* — "the delegation brief is wrong, spec.md is right" — and was written up as a
premise disagreement. It was the opposite: the brief and the merged tree were right, and
`spec.md` was stale because it was authored before the merge. Two further findings collapsed for
the same reason: a "dangling link" and an "empty research/ directory" were both just files added
on the item branch and absent from the session checkout.

**How to apply:**
- Before citing any line number, confirm which tree you are reading. Two worktrees under
  `TaskMaster-wt/` can differ by hundreds of commits.
- Treat a *uniform* off-by-N across all citations in one file as a stale-tree signature, not as a
  brief error. A real citation error is not uniform.
- Treat "file/directory does not exist" as a stale-tree signature too, before reporting a
  dangling link.
- If the brief and `spec.md` disagree on numbers, the likely cause is that `spec.md` was authored
  at an older SHA — check that before asserting either side is wrong.
- Related: [[stale-base-deletes-silently-on-fan-in]], and the general rule that diff bases anchor
  to `origin/main`, never bare local `main`.
