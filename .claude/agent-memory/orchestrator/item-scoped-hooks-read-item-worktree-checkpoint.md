---
name: item-scoped-hooks-read-item-worktree-checkpoint
description: In a parallel item run, the model-routing hook and the pr-author hook resolve the item worktree from issue_num/branch/--head and read ITS gitignored checkpoint, not the session-root checkpoint the parent calls canonical
metadata:
  type: project
---

Observed on #968 (2026-10-03, parallel run bugs-2026-09-28). The parent named the session-root checkpoint as canonical. Two gates ignored it:

- `enforce-model-routing-receipt.ps1` denied `Agent(feature-review)` with MODEL_ROUTING_RECEIPT_BLOCKED, even though the session-root file held the receipt. It reads `<item worktree>/artifacts/orchestration/orchestrator-state.json`. `atomic-executor` passed only because the preparation run had left an executor receipt there.
- The PR-creation readiness check had to be run against the item worktree checkpoint, because `enforce-pr-author-skill.ps1` resolves the worktree from `--head`. That checkpoint still held a `local_execution_overrides` entry from the preparation run, so the gate fails. The session-root file passes the same check.

**Why:** preparation children write their own checkpoint inside the item worktree, and the hooks resolve identity before they read anything.

**Split resolution, confirmed on #964 / PR #977 (2026-10-03).** `enforce-pr-author-skill.ps1` resolves only the CHECKPOINT from `--head` (item worktree). The body, receipt and `pr_context.summary.txt` stay process-cwd-relative (session root). Working recipe: verify the session-root summary's owner (`Head ref (resolved)` line) has its PR merged (`gh pr list --head <branch> --state all`), then run `collect_pr_context` with `workspace_root` = session root and `target_ref` = your branch. Write the body and receipt into the session-root `artifacts/` (gitignored), `Start-Sleep 2` before stamping `created_at`, and run a bare `gh pr create --head ... --base main --body-file artifacts/pr_body_<N>.md`. It passed on the first attempt.

**How to apply:** at resume, read the item worktree checkpoint before the first delegation. Mirror every routing receipt and complexity assessment into it. Run `Invoke-OrchestratorStatePreflight` against it early, because an inherited override there is a policy hold (see [[pr-readiness-gate-bars-any-recorded-override]]). Opening the PR against the session-root file to avoid the item record would mean choosing whichever checkpoint passes, and that defeats the gate.
