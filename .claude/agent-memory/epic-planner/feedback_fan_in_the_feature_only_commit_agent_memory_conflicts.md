---
name: fan-in-the-feature-only-commit-agent-memory-conflicts
description: Concurrent prep children each rewrite the shared .claude/agent-memory/*/MEMORY.md index, so merging a child's branch TIP conflicts there — and epic-planner cannot resolve it, because that path is outside the five staging-exempt trees; merge the last feature-only commit SHA instead
metadata:
  type: feedback
---

Merging a prepared child's branch tip conflicts on `.claude/agent-memory/orchestrator/MEMORY.md`
and `.claude/agent-memory/atomic-planner/MEMORY.md` as soon as a second child has already been
fanned in. Both children append to, and sometimes compact, the same shared index.

**Why epic-planner specifically cannot resolve it.** Concluding a conflicted merge requires a
`git commit`, and under [[integration-commit-form-constraints]] every operand must sit inside the
five staging-exempt trees. `.claude/agent-memory/` is not one of them, and a pathless `git commit`
is denied outright, so the merge cannot be concluded at all. `git merge --abort` is not modelled by
the gate and works.

**How to apply.** Before merging any prepared child, list its commits with
`git log --oneline --name-only --format="=== %h %s" HEAD..<child-branch>` and merge the last commit
whose file list is confined to `docs/features/active/<its-folder>/`, by SHA:

```
git merge --no-ff -m "docs(epic): fan in the prepared feature for issue N" <feature-only-sha>
```

Nothing is lost. The trailing memory commits stay on the child branch, and a branch ref survives
`git worktree remove`, so they remain recoverable after cleanup.

**The variant that does lose work:** a child that leaves its subagents' memory files UNCOMMITTED
(one on this run left five, citing its own plan's footprint rule) has them only in the worktree, so
`git worktree remove` destroys them. Either leave that worktree in place and say so, or accept the
loss deliberately — do not discover it afterwards.

Merging a branch tip is still fine while it is the FIRST child fanned in, or when the child touched
no shared index. The octopus merge of three predecessor branches at the start of this run succeeded
for that reason.

Related: [[recover-dead-prep-child-by-committing-then-relaunching]],
[[concurrent-prep-children-worktree-isolation]].
