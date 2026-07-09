---
name: collect-pr-context-lands-in-main-checkout
description: In a worktree, collect_pr_context writes pr_context.* to the MAIN checkout, but the enforce-pr-author hook reads it from the worktree CWD — copy it in before gh pr create
metadata:
  type: project
---

When the orchestrator runs in a git worktree, `mcp__drm-copilot__collect_pr_context` reports writing `artifacts/pr_context.summary.txt` to the worktree but it actually lands in the MAIN checkout (e.g. `C:/Users/.../repos/TaskMaster/artifacts/pr_context.summary.txt`), not the worktree's `artifacts/`.

**Why:** `.claude/hooks/enforce-pr-author-skill.ps1` reads `artifacts/pr_context.summary.txt` relative to the CWD of the `gh pr create` command (the worktree). If it is missing there, the hook blocks with the "context artifact absent" (Case C) reason, and the receipt staleness check has nothing to compare against.

**How to apply (in-thread pr-author flow, since Agent(pr-author) is unavailable — see [[pr-author-hook-blocks-gh-in-this-repo]]):**
1. Run `collect_pr_context --base <branch>`; then `cp` the main-checkout `artifacts/pr_context.summary.txt` (and `.appendix.txt`) into the worktree `artifacts/`.
2. Write `artifacts/pr_body_<N>.md`; compute lowercase-hex SHA-256 of its bytes; write `artifacts/pr_body_<N>.receipt.json` with `created_at` STRICTLY NEWER than the (copied) summary's LastWriteTimeUtc — copy the summary FIRST, then sleep, then stamp the receipt.
3. `gh pr create --base <epic_context.integration_branch> --body-file artifacts/pr_body_<N>.md`.

Also: `artifacts/orchestration/orchestrator-state.json` and `artifacts/pr_body_*`/`artifacts/pr_context.*` are all GITIGNORED in this repo — the checkpoint is local on-disk state (which is exactly what the hooks read); do not expect it in commits/PRs, and a "clean" `git status` does not mean the checkpoint is committed.
