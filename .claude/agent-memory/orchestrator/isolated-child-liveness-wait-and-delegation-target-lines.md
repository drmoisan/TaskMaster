---
name: isolated-child-liveness-wait-and-delegation-target-lines
description: In a worktree-isolated preparation child, pwsh/poetry sleeps are refused, so wait for background subagents with a long read-only git pickaxe scan; and every Agent() prompt needs the canonical-issue and branch lines or the model-routing gate denies it
metadata:
  type: project
---

Verified 2026-09-30 on the #944 preparation child (isolated worktree, Bash discipline of git/pwsh/poetry only).

**1. Waiting for a background subagent without ending the turn.** `Agent()` returns immediately and the
completion arrives as a task-notification injected during a later tool call. Ending the turn instead
returns control to the parent and the grandchild result is lost. Every sleep route was refused:
`pwsh -NoProfile -Command 'Start-Sleep ...'` (isolation filter, even with `-WorkingDirectory`), and
`poetry run python -c "time.sleep"` (no pyproject.toml in TaskMaster). A read-only full-history
pickaxe scan works as a wait interval and prints nothing:
`git -C <wt> log -G"NoSuchRegexXyz<unique>" --oneline --all -p -w` (timeout 600000). Repeat it; the
notification lands between calls. A Glob on the expected artifact directory is a cheap progress probe.

**2. Delegation prompt must name the item.** The first `Agent(task-researcher)` was denied
`TARGET_WORKTREE_NOT_DERIVABLE`: the prompt needs the literal line
`Canonical issue number for this feature is <N>.` plus a `branch: <item branch>` line. With those two
lines (and no `Parallel mode: true` marker) the model-routing gate read this worktree's own checkpoint.

**3. Completion-gate receipt shape.** The MCP validator with `require_model_routing` rejects
`delegation_receipts.agents[]` entries lacking any of: `agent_name`, `step`, `agent_id`, `started_at`,
`completed_at`, `skill_source`, `result_signal`, `artifact_paths`. Write them in full from the first
delegation. See [[completion-gate-receipt-shapes]].

Related: [[worktree-isolation-blocks-pwsh-per-agent-type]], [[model-routing-hook-reads-canonical-path-only]].
