---
name: epic-child-self-merge-step9-passed-vs-verified
description: Epic-child self-merge needs on-disk step9_status "passed" for the merge hook, but the MCP validator rejects "passed"; flip to "verified" after merging
metadata:
  type: project
---

For an epic child orchestrator's own PR-into-integration self-merge, `.claude/hooks/enforce-epic-merge-gate.ps1` allows `gh pr merge --merge` on the per-feature path only when the on-disk checkpoint has `epic_mode == true` AND `step9_status == "passed"` (it reads the checkpoint file in the agent cwd directly). The alternative path is an epic checkpoint with `epic_merge_pr.ci_gate.conclusion == "success"` and a matching `pr_number`.

**Conflict:** `mcp__drm-copilot__validate_orchestration_artifacts` (orchestrator-state) REJECTS `step9_status: "passed"` — "invalid step9_status: passed". Its step-status enum is `pending|in_progress|completed|not-applicable|blocked|verified|failed` (no "passed"). So a checkpoint that satisfies the merge hook fails the MCP validator and vice-versa.

**How to apply (sequence):** set `step9_status: "passed"` on disk BEFORE the merge (satisfies the merge hook), run `gh pr merge <n> --merge`, then immediately flip `step9_status` to `"verified"` and set `step10_status: "completed"`, record `epic_merge {merge_commit_sha, target_branch, merged_at}` (and `epic_merge_pr`), and re-run the MCP validator — it now passes. Confirmed on #363 (PR #379 → integration merge df2235bc). Matches the prior finding [[epic-mode-pr-merge-gate-sequencing]].
