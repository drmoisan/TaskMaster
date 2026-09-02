---
name: planner-and-executor-observe-different-worktrees
description: A plan's claim about worktree-local git state (untracked siblings, dirty files, clean status) can be true in the planner's worktree and false in the executor's; always re-derive it in the executor's checkout
metadata:
  type: project
---

A plan assertion about **worktree-local** git state — "sibling folder X is untracked in this
checkout", "the tree is dirty", "path Y is not in the index" — is worktree-scoped, and the planner
and the executor frequently run in different worktrees. `atomic-planner` often runs in the session
worktree (`TaskMaster-wt/<ts>`), while `atomic-executor` runs in an isolated agent worktree
(`.claude/worktrees/agent-<id>`) on a different branch. The same path can be untracked in one and
committed in the other.

**Why:** on #637 round 2 the planner "corrected" a round-1 statement that no sibling folder under
`docs/features/active` was untracked, replacing it with an explicit claim that the `...-440` folder
"exists on disk ... and is reported as untracked by `git status --porcelain`". That was true in the
session worktree and false in the executor's: there `git ls-files` lists the 440 folder's files and
`git status --porcelain` is completely empty. A correct claim was reversed into a false one, and the
false claim was then propagated into a task body (`P8-T33`). This is the
[[project_preflight_citation_match_propagates_false_fact]] failure mode with a worktree twist: the
planner's observation was real, just about the wrong tree.

**How to apply:** distinguish two claim classes when reviewing.
- *Commit-scoped* claims (`git diff BASE..HEAD`, `git ls-files`, file contents at a ref) are branch
  properties and travel with the branch — verify once.
- *Worktree-scoped* claims (`git status --porcelain`, untracked-file existence, on-disk-but-not-in-index)
  do NOT travel — re-derive them in the executor's cwd before accepting or contradicting them.

Prefer plan prose that justifies pathspec scoping **prospectively** ("a concurrent run can leave an
untracked sibling before this task executes") over prose that asserts a specific present-tense tree
state, because the prospective form is worktree-neutral and cannot go stale between planning and
execution.
