---
name: followup-promotion-task-is-unexecutable-by-executor
description: A plan task saying "promote the follow-ups through the promotion lifecycle" cannot be executed by atomic-executor - the MCP tool is absent, the intake document breaches the scope gate, and the word "promotion" in a commit message trips a hook that aborts the whole Bash line
metadata:
  type: project
---

A `[P#-T#]` task worded "promote the N follow-up items through the repository's promotion lifecycle" is **structurally unexecutable** by the atomic-executor persona. Three independent blockers, all hit on #464 `[P11-T14]`:

1. **The MCP tool is not in the executor's tool set.** Only `run_poshqc_format`, `run_poshqc_analyze`, `run_poshqc_test` and `run_poshqc_analyze_autofix` are exposed. There is no `potential_to_issue`. The lifecycle cannot be invoked.
2. **The intake document breaches the plan's own scope gate.** The lifecycle's first stage is a file under `docs/features/potential/`. A terminal task like `[P11-T17]` requires `git diff --name-only <BASE>` to list ONLY the writable source paths and paths under `<FEATURE>/`. Writing the intake document makes that gate fail. The two tasks are in direct conflict.
3. **A `PROMOTION_MCP_ONLY_BLOCKED` hook matches the commit message.** A `git commit -m` whose body contains promotion-lifecycle wording ("promote", "promoted", "NOT promoted", `potential_to_issue`) is rejected with `Direct Bash promotion-script execution is not allowed in agent sessions`. The guard aborts the **whole chained line**, so a preceding `git add` in the same `&&` chain silently does not run either.

**Why:** the promotion lifecycle is deliberately MCP-gated so every issue carries a receipt, and the executor persona is deliberately not given issue-creating tools. `gh` IS installed and authenticated, so `gh issue create` is technically possible — but it bypasses the intake stage, invents an unauthorised path, and risks duplicates the receipt mechanism exists to prevent.

**How to apply:**
- Use the escape hatch such tasks normally carry ("where creation is unavailable in this environment, record the reason and the potential-document path"). Record one row per item with source citation, target intake filename, and a `gh issue list --state all --search "<terms>"` duplicate check so the orchestrator can promote without re-deriving anything.
- Say plainly it is an **outstanding handoff item, not a completed promotion**, and escalate it at plan completion. Do not let it read as done.
- For the commit: write the message to the session scratchpad and use `git commit -F <file>`, and reword the trigger tokens ("the lifecycle MCP tool", "no GitHub issue was created", "target document path"). Stage in a **separate** Bash call from the commit so a blocked commit does not silently drop the `git add`.
- At preflight, flag this class of task: a promotion task plus a terminal scope gate over `<FEATURE>/` only is a plan-level contradiction worth a revision.
- Related: [[project_blocked_bash_command_silently_drops_chained_checkoff]], [[project_plan_checkoff_fixpoint_breaks_terminal_clean_tree_gate]], [[project_baseline_sha_diff_conflates_merged_base]].
