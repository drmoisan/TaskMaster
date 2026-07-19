---
name: epic-child-stale-local-integration-ref
description: Local branch ref for the epic integration branch can be stale in an agent worktree; always git fetch and use origin/<branch> as the tip before branching a child worktree or checking a Phase-0 gate.
metadata:
  type: project
---

For epic-child #374 (utilitiescs-nullable-dialogs-misc), the local ref
`epic/utilitiescs-nullable-remediation-integration` in the agent worktree
(`.claude/worktrees/agent-a7e55c484f5d14b45`) resolved to `6d4da8bb...`, an ancestor
commit that predated PR #379 (#363) and PR #382 (#364) both landing on the branch.
`git merge-base --is-ancestor` proved `6d4da8bb` was an ancestor of, not equal to,
`origin/epic/utilitiescs-nullable-remediation-integration` (`dffadd5a...`). Branching
the child worktree from the stale local ref would have made the Phase-0 gate (checking
`UtilitiesCS/Extensions/WinFormsExtensions.cs` for `#nullable enable`, proving #363
Batch D's merge) fail even though the epic orchestrator's briefing correctly stated the
dependency was merged — the briefing named the right commit, but the local ref hadn't
caught up to it yet.

**Why:** agent worktrees are created once and their local branch refs are not
automatically kept current with `origin/*` as sibling epic-child PRs merge over the
life of the epic. A `git checkout -b <child-branch> <local-integration-branch-name>`
without a preceding `git fetch origin` silently branches from stale history.

**How to apply:** in any epic-child orchestration, before creating/resetting the child
worktree branch from the epic integration tip, run `git fetch origin` and use
`origin/<integration-branch>`'s resolved SHA (not the bare local branch name) as the
base. Cross-check with `git merge-base --is-ancestor <briefed-commit> origin/<branch>`
when the briefing names a specific commit SHA for a dependency, to confirm the local
checkout actually contains it before trusting a Phase-0 gate check performed against
local refs. See also [[epic-child-pr-gate-gotchas]] and
[[unplanned-epic-child-worktree-mechanics]].
