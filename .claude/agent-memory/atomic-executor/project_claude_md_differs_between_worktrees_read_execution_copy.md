---
name: claude-md-differs-between-worktrees-read-execution-copy
description: CLAUDE.md is branch-tracked and differs between worktrees, so the copy auto-loaded into context is the SESSION worktree's; a Phase 0 policy read must be taken against the execution worktree, where coverage floors and an extra evidence-format rule differ
metadata:
  type: project
---

`CLAUDE.md` is a tracked, branch-varying file. The copy Claude Code auto-loads into the system
prompt comes from the **session** worktree. When the plan directs work at a different execution
worktree, that auto-loaded copy is not the governing text.

Measured 2026-09-19 (issue 911): `git hash-object CLAUDE.md` gave `0c650735e…` in the execution
worktree against `67f75c93d…` in the session worktree, while the six `.claude/rules/*.md` files
were byte-identical across both. Two differences were load-bearing:

- **Coverage floors.** The execution copy's UT2 states C# line `>= 80%` / branch `>= 75%` and
  PowerShell line `>= 80%`, settled by the maintainer 2026-09-11 under issue #563. That contradicts
  `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`, which both state a
  uniform line floor of `>= 85%`. `CLAUDE.md` is first in the policy compliance order, so 80 wins —
  and it matters, because the repo's PowerShell aggregate sits at 83.93 percent, above the
  governing floor and below the rule-file figure.
- **`## Committed Test Evidence Format`.** Present only in the execution copy. Committed test
  evidence must be a *projection* of a tool's output; a raw coverage-collector document or a raw
  test-platform document is prohibited from git "in any form, including under a feature folder's
  evidence tree". Plans that write a Pester JaCoCo XML or a Cobertura/trx document straight into
  `<FEATURE>/evidence/` and then commit the folder collide with this.

**Why:** a Phase 0 policy-read task that cites line counts or quotes rules from the auto-loaded copy
is describing a different checkout, and the divergences are exactly the kind that silently change a
gate's threshold or make a planned evidence artifact uncommittable.

**How to apply:** in any multi-worktree run, hash the seven policy files in both worktrees, read in
full any that differ from the execution worktree, and record the divergence in the policy-read
artifact rather than resolving it — `.claude/rules/**` is push-down-owned and not editable here.
See [[planner-and-executor-observe-different-worktrees]].
