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

**Refinement (#327, 2026-07-16, dedicated agent worktree):** when the child orchestrator's session cwd IS the feature worktree (the harness gave me an isolated `.claude/worktrees/agent-<id>` worktree and I `git switch -c` the feature branch there), collect_pr_context wrote `pr_context.*` DIRECTLY into that worktree's `artifacts/` (returned paths were the worktree) and the hook read them there — no main-checkout copy step was needed. The main-checkout-landing behavior above applies when session cwd differs from the feature worktree. TWO quirks still bit: (a) collect_pr_context reported a `Head:` SHA one commit BEHIND my true branch tip and a second call did NOT rewrite the file (identical mtime) — do not trust the summary's `Head:` line; the pushed branch tip and GitHub's own base...head diff are authoritative, so it is harmless. (b) The receipt `created_at > summary mtime` check still held because I wrote the receipt after collect ran; no sleep was needed since minutes had elapsed. Child->integration PR #334 merged fine (merge commit 9559c73c) on blocking_count==0 with zero CI ([[project_epic_child_prs_no_ci]]).
