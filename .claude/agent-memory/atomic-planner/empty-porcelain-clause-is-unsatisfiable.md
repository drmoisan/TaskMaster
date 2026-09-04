---
name: empty-porcelain-clause-is-unsatisfiable
description: A "git status --porcelain prints no lines" acceptance clause is unsatisfiable in any task that runs after a prior task wrote its own evidence artifact post-commit; use a negative path-class clause, and a double-amend shape for a terminal clean-tree proof
metadata:
  type: project
---

An acceptance clause reading "the `git status --porcelain` span prints no lines" fails in two very
common plan shapes, and both are invisible when the clause is read in isolation.

**Shape 1 — a prior task's own artifact.** Every evidence-producing task writes its artifact AFTER
running its commands, so an artifact recording a commit is necessarily uncommitted on disk. The next
task's porcelain span sees it. Chain three such tasks and the span is never empty again.

**Shape 2 — tracked residue outside the item's Write Set.** `.claude/agent-memory/**` is tracked and
sibling agents in the same run write into it. See [[agent-memory-is-tracked-scope-git-gates]].

**The satisfiable replacement is a NEGATIVE path-class clause,** not an emptiness clause: every
porcelain line names a path under the feature folder or under `.claude/agent-memory/`, and NO line
names a path under the code trees the item edits. That negative is what the emptiness clause was
actually standing in for — it proves no uncommitted edit is hiding a change from the anchored,
committed-only `git diff` beside it. Enumerate the allowed `.claude/agent-memory/` paths in the
artifact so the allowance is not a blank cheque.

**A terminal clean-worktree proof needs a double amend.** Recording an observation dirties the tree
it observes, so a single commit can never prove its own cleanliness. Terminating shape: (1) commit;
(2) write the artifact with that SHA; (3) tick the plan's remaining checkboxes; (4) `git add -A` +
`git commit --amend --no-edit`; (5) capture `git rev-parse HEAD` and `git status --porcelain` — this
span IS empty, because nothing has been written since step 4; (6) append both to the artifact;
(7) `git add -A` + `git commit --amend --no-edit` once more, with nothing written afterwards.

**Also check the companion diff's pathspec.** If a delivery task runs `git add -A`, agent-memory
enters `origin/main...HEAD`, so any later anchored diff whose pathspec includes `.claude` and asserts
"prints no lines" becomes unsatisfiable too. Add `":(exclude).claude/agent-memory/**"` to it and state
that the exclusion is co-extensive with the enumeration the delivery task records.

**Why:** #736 preflight round 1 flagged this on two tasks (DEF-5); re-deriving the fix surfaced a
third instance the reviewer had not found, in an anchored diff's `.claude` pathspec.

**How to apply:** grep every plan draft for `porcelain` and for `prints no lines`, and for each hit
ask which artifact the PREVIOUS task wrote. Related: [[diff-gates-need-a-commit-task]],
[[terminal-phase-planner-traps]].
