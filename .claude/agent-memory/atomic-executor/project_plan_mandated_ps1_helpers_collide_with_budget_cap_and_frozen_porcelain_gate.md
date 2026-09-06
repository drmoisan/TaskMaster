---
name: plan-mandated-ps1-helpers-collide-with-budget-cap-and-frozen-porcelain-gate
description: A plan rule that mandates authoring snippets into .ps1 files (e.g. a backslash-literal authoring rule) is unexecutable unless it fixes ONE gitignored path — the budget hook caps distinct non-test .ps1 paths at 3, and an exclusive porcelain gate frozen at Phase 0 rejects any new untracked path
metadata:
  type: project
---

When a plan adds a rule of the form "author every backslash-bearing snippet into a `.ps1` file and
invoke it with `pwsh -File`", check two things before clearing it. Both are silent in the plan text
and both are hard blocks at execution time.

**Why (1) — the budget cap.** `.claude/hooks/enforce-powershell-batch-budget.ps1` is a PreToolUse
hook on `Write|Edit`. Default `ProdCap` is 3 (`:316`). A **new distinct** non-test `.ps1` path inside
the worktree root is DENIED once three are recorded (`:293`). An already-recorded path is allowed
unconditionally (`:289`), and a path outside the worktree root is discarded and spends nothing
(`:279-281`). So one reused helper is free forever; one helper per task exhausts the cap mid-plan.
The hook's three offered outs (raise the env var, delete the state file, split the batch) are all
governance bypasses an executor must not take.

**Why (2) — the frozen porcelain gate.** A Phase 5 format gate that asserts
`git status --porcelain` "lists no path outside the union of <PLAN WRITE SET>, <baseline set>,
<allowance>" has its exclusion set captured in Phase 0, *before* any helper exists. A helper written
to a non-ignored path therefore fails that gate, and the Phase 5 restart loop cannot clear it —
restarting re-runs the same porcelain capture against the same stray file.

**How to apply:** require exactly one fixed helper path under an already-gitignored directory
(in TaskMaster, `coverage/` — `coverage/*` at `.gitignore:144`), rewritten in place, with the agent
scratchpad outside the worktree as the fallback if the cap is already spent. Never let the plan
leave the location unspecified. Do not name an absolute scratchpad path in a committed plan; it
embeds the account name — see [[../_shared_no_absolute_host_paths]].

Related: [[project_powershell_scratch_script_budget_hook_blocks_helpers]],
[[project_tool_layer_collapses_double_backslash_in_file_content]],
[[project_agent_memory_tracked_breaks_unscoped_git_gates]]
