---
name: verify-citations-in-the-assigned-worktree
description: Read every cited file in the assigned execution worktree; sibling session worktrees hold divergent copies of the same tracked file, so a line citation read elsewhere is wrong
metadata:
  type: feedback
---

Every file path and line citation in a plan must be re-derived by reading the file **inside the
worktree the plan will be committed from**, named explicitly in the delegation prompt. Never read a
"same" file from another `TaskMaster-wt/<session>/` directory, from a `.claude/worktrees/agent-*`
checkout, or from memory.

**Why:** on issue #752 round 2 I correctly spotted that a citation into
`.claude/agent-memory/_shared_no_absolute_host_paths.md` was wrong, then "fixed" it to lines 98-102
by reading the copy in a sibling session worktree. That copy carries a 10-line block inserted after
line 47 and is 102 lines; the execution worktree's copy is 92 lines with the target bullet at 88-92.
The pre-existing citation (lines 88-92, which `remediation-inputs` had given all along) was right,
and my "correction" broke it. `.claude/**` is tracked and materialises into every worktree, so the
same repo-relative path resolves to genuinely different content per checkout, and both reads look
equally authoritative.

**How to apply:** when the caller names a worktree, prefix every Read/Grep/Glob operand with that
absolute root and use no other root for the whole planning pass — including the pass where you are
only *revising*. Ambient cwd is not the execution worktree: the planner agent's own cwd is usually a
different session worktree entirely. When a revision round asserts a prior citation was wrong, first
re-read the file in the assigned worktree before believing either the old or the new number; check
whether the requirements document already carries a citation and treat disagreement with it as a
signal to re-measure, not to overwrite. State the verified worktree root next to each citation in
`SELF-REVIEW: RE-DERIVED THIS PASS` so the provenance is auditable.

Related: [[harness-git-status-may-describe-another-worktree]],
[[agent-memory-is-tracked-scope-git-gates]].
