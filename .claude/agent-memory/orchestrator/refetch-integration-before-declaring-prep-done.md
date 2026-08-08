---
name: refetch-integration-before-declaring-prep-done
description: In a preparation-mode epic child, re-fetch and rebase onto the integration branch before declaring done; epic-planner publishes binding child directives mid-run and a plan cleared against a stale manifest is wrong
metadata:
  type: feedback
---

In a preparation-mode epic child, re-fetch the integration branch and rebase onto it **before**
declaring preparation complete — not only at branch setup. Then diff the epic manifest across the
range and read every section that names your child. If binding directives landed, absorb them
(spec, then plan) and re-run the planner/executor preflight cycle before finishing.

**Why:** On epic #136 child F1 (issue #432, 2026-08-07) the plan cleared preflight at 166 tasks, and
only the pre-commit `git diff` against `origin/<integration>` revealed the branch had advanced. The
tip commit was `docs(epic): F1 ledger/harness directives` — `epic-planner` had published
requirements binding **specifically on F1** while F1 was running, because three sibling children hit
gaps in F1's brief and escalated. Two of them contradicted the cleared plan outright: a mandated
third classification bucket where the plan had two, and a dynamic denominator that made the plan's
"halt if the compiled count is not 121" gate fire the moment any sibling added a file. Absorbing
them took the plan from 166 to 194 tasks. A second re-fetch before the final commit caught a further
manifest section. Shipping the 166-task plan would have handed `epic-orchestrator` a plan that
halts in Phase 0 and produces a ledger fifteen siblings then consume.

Mechanically, the drift is easy to miss: all worktrees share one `.git`, so a **sibling's** fetch
silently advances `refs/remotes/origin/<integration>` under you. Your own `git log` still shows the
old tip and nothing signals the move. See [[project_epic_child_stale_local_integration_ref]].

**How to apply:** Before the final commit of any preparation-mode child, run
`git fetch origin <integration-branch>`, then `git rev-list --count HEAD..origin/<integration>`. If
non-zero, `git log --oneline HEAD..origin/<integration> -- <epic-manifest-path>` and read the diff
of every commit that touched it. Rebase (expect conflicts only on shared
`.claude/agent-memory/*/MEMORY.md` indexes — resolve by union, and prefer `--ours` plus re-adding
your single entry when a subagent has compacted the index, so sibling entries are not dropped; see
[[epic-child-rebase-shared-memory-conflict]]). Wave-0 enabler children are the highest risk, because
every sibling that depends on them has both the motive and the standing to amend their brief while
they run. Budget for at least one such round rather than treating it as an exception.
