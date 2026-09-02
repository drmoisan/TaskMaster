---
name: epic-child-pr-gate-gotchas
description: Three non-obvious gates when an epic-child orchestrator runs in an isolated worktree and PRs into the integration branch (collect_pr_context workspace quirk, nested epic_context.integration_branch, ci.yml base triggers)
metadata:
  type: project
---

When running as an epic-child `Agent(orchestrator)` inside an isolated worktree (e.g. `.claude/worktrees/agent-*`), PRing into the epic integration branch, three gates behaved non-obviously (verified 2026-07-08, F1 #261, PR #275 into `epic/store-lockup-resilience-integration`).

**1. `mcp__drm-copilot__collect_pr_context` writes to the WRONG workspace in an isolated worktree.** It reported the isolated-worktree artifact paths in its result JSON but actually wrote `artifacts/pr_context.summary.txt` / `.appendix.txt` into the MAIN checkout (`repos/TaskMaster/artifacts/`) and the shared checkout, NOT the isolated worktree. The `enforce-pr-author-skill.ps1` hook Test-Paths the context file relative to the gh-command cwd (the worktree), so it was absent there → PR_CONTEXT_MISSING. Fix: after calling the MCP tool, author `artifacts/pr_context.summary.txt` locally in the worktree from the real `git diff <base>..HEAD`, then write the body + SHA-256 receipt (`created_at` strictly newer than the summary file's mtime). See [[pr-author-hook-blocks-gh-in-this-repo]] and [[pr-context-summary-unreliable-gh-and-classification]]. Same defect as [[collect-pr-context-lands-in-main-checkout]] (confirmed standalone, non-epic instance).

**2. The epic base-branch check reads NESTED `epic_context.integration_branch`, not top-level `integration_branch`.** `enforce-pr-author-skill.epic-base-branch.ps1` (Check 6, `EPIC_BASE_BRANCH_MISMATCH`) requires the per-feature checkpoint to have `epic_mode: true` AND `epic_context.integration_branch` populated, and requires `gh pr create` to carry an exact `--base <that value>`. A top-level `integration_branch` alone is not read → the hook blocks with "no epic_context.integration_branch is recorded." Add an `epic_context` object with `integration_branch` to the checkpoint before creating the PR.

**3. `ci.yml` `pull_request` triggers are `branches: [main, development]` only.** A PR based on the epic integration branch triggers NO required CI check (empty `statusCheckRollup`, `mergeStateStatus: CLEAN`). Full CI runs only when the integration branch is later PR'd to main by the epic-orchestrator. So "CI-green" for an epic-child PR = MERGEABLE/CLEAN with no failing required checks; merge on that, and record the determination as `ci_gate`/`ci_determination`. Do not wait for checks that will never appear.

**Completion gate:** the MCP `validate_orchestration_artifacts --require-complete` enumerates a from-scratch large-route contract (task-researcher/prd-feature/pr-author agent receipts, promotion MCP receipts, pr_gate/ci_gate) that an implementation-only epic-child run legitimately lacks. The ACTUALLY enforced SubagentStop gate in this repo is the portable `Test-OrchestratorStateCompletionReadiness` (Python `scripts.dev_tools` is absent), which passed on the same checkpoint. See [[orchestrator-state-validator-divergence]].
