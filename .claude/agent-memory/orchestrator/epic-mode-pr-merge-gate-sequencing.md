---
name: epic-mode-pr-merge-gate-sequencing
description: Epic child self-merge needs step9_status "passed" for the merge gate but "verified" for the completion gate — flip it between the merge command and orchestrator Stop
metadata:
  type: project
---

Epic child orchestrator self-merging its PR into the integration branch hits two gates with CONFLICTING step9_status vocabularies.

**Why:** `.claude/hooks/enforce-epic-merge-gate.ps1` (PreToolUse on `gh pr merge --merge`), child-feature path, requires TWO checkpoint facts: a **top-level** `epic_mode: true` (NOT nested under `epic_context` — it reads `$Checkpoint.epic_mode` directly) AND `step9_status == "passed"` (literal string). But `OrchestratorStateCompletion.Test-OrchestratorStateCompletionReadiness` (run by my own `validate-orchestrator-output.ps1` SubagentStop) REJECTS `step9_status: "passed"` as invalid — it wants `verified`. See [[orchestrator-state-validator-divergence]].

**How to apply:** Sequence the on-disk checkpoint (the gates read the file, not git):
1. Before `gh pr merge --merge`: set top-level `epic_mode: true` and `step9_status: "passed"` (keep uncommitted so the transient "passed" never enters the merged tree — the PR merges the already-committed branch state).
2. Run `gh pr merge <N> --merge`.
3. After merge: set `step9_status: "verified"`, `step10_status: "verified"`, record `epic_merge {merge_commit_sha, target_branch, merged_at, pr_number}`, next_step DONE.
4. Verify the final file with `Test-OrchestratorStateCompletionReadiness` (ExitCode 0) before terminating.
Both values truthfully represent the vacuous epic-mode CI pass (child→integration PRs get zero CI checks; `ci.yml` only triggers on PRs to main/development). Record `ci_gate.conclusion: "success"` with that note. The enforce-pr-author epic-base check additionally requires the literal `--base <epic_context.integration_branch>` on `gh pr create`.
