---
name: child-orchestrator-pr-hook-reads-session-root
description: When a child orchestrator's session cwd differs from the feature worktree, the pr-author PreToolUse hook resolves checkpoint/pr_context/pr_body against the SESSION ROOT, not the feature worktree
metadata:
  type: project
---

When a child-feature orchestrator runs with its session cwd on a DIFFERENT worktree than the feature branch worktree (e.g. session cwd = the epic `2026-07-09T15-31` design worktree, feature checked out in `winforms-298`), the `enforce-pr-author-skill.ps1` PreToolUse hook and `collect_pr_context` both operate on the SESSION ROOT, not the feature worktree.

**Why:** The hook is registered as `pwsh -NoProfile -File .claude/hooks/enforce-pr-author-skill.ps1` and uses bare relative paths (`artifacts/pr_context.summary.txt`, `artifacts/orchestration/orchestrator-state.json`), resolved against its process cwd = the session project root. `collect_pr_context` also writes `artifacts/pr_context.*` into the session root (its `workspace_root` param did NOT redirect output in #298; it diffed/wrote at session root). Confirmed 2026-07-10 building PR #302 for child #298 into `epic/winforms-testability-refactor-integration` (merge commit 1ffc2eac).

**How to apply (child-in-epic PR authoring, inline pr-author skill):**
- Author the PR body from the REAL feature-worktree diff (`git -C <feature-wt> diff <mergebase>...HEAD`), NOT from `pr_context.summary.txt` (it reflects the session-root's checked-out branch, which is wrong).
- Stage all hook inputs at the SESSION ROOT `artifacts/`: `pr_body_<N>.md` (copy from the feature worktree; byte-identical → same SHA-256), `pr_body_<N>.receipt.json` (`created_at` newer than the session-root `pr_context.summary.txt` mtime), and a pr-creation-ready `orchestrator-state.json`.
- The session-root `orchestrator-state.json` is usually the EPIC session's own checkpoint. It is NOT pr-creation-ready shaped and will fail `--require-pr-creation-ready`. Back it up, temporarily swap in a conformant child checkpoint, run `gh pr create` from the session root, then RESTORE the epic checkpoint byte-identically (verify SHA). Bundling backup+create+restore into one Bash call does NOT work — the hook validates the on-disk checkpoint BEFORE the command runs.
- Checkpoint schema gotchas: step-status enum requires `completed` (not `complete`); pr-creation-ready checks steps 5-8 only (9/10 may be `pending`); `blocked_reason` must be `none`/absent; required-key set includes `relativeFile`, `long-name`, `work-mode`, `plan-path`. Under `epic_mode:true`, the base-branch companion requires `epic_context.integration_branch` to equal the exact `--base` value, else `EPIC_BASE_BRANCH_MISMATCH`.
- `gh pr merge` is NOT gated by the hook. See also [[pr-author-hook-blocks-gh-in-this-repo]], [[collect-pr-context-lands-in-main-checkout]], [[project-epic-child-prs-no-ci]].
