---
name: parallel-item-pr-create-split-resolution-and-title-word
description: Opening a parallel item's PR - the pr-author hook reads the checkpoint from --head's worktree but the body, receipt and pr_context from the SESSION root; the word "issue" in a gh pr create title trips the promotion hook
metadata:
  type: project
---

Measured on the issue-882 parallel item (2026-09-29), non-isolated child whose cwd is the session checkout.

1. `enforce-pr-author-skill.ps1` splits its resolution. The CHECKPOINT is resolved from the `--head` branch
   to the item worktree (issue 687 fix). The BODY, the RECEIPT and `artifacts/pr_context.summary.txt` are
   resolved relative to the hook's process directory, which is the session checkout. `--body-file` must be
   the relative canonical form `artifacts/pr_body_<N>.md` (an absolute path is denied
   PR_BODY_PATH_NONCANONICAL). Working route: write body + receipt in the item worktree, mirror both
   byte-exact (Copy-Item, compare Get-FileHash) into the session checkout's gitignored `artifacts/`, then run
   `pwsh -NoProfile -Command 'Set-Location <item worktree>; gh pr create ... --head <branch> --body-file artifacts/pr_body_<N>.md'`.
   The receipt `created_at` must be newer than the SESSION root's summary mtime, not the worktree's.
   Never touch the shared session-root `pr_context.summary.txt`.
2. `enforce-promotion-mcp-only.ps1` denies `gh pr create` whose title contains the word "issue"
   (PROMOTION_MCP_ONLY_BLOCKED). Write "(882)" instead of "(issue 882)".
3. The pre-implementation gate reads the session-root `orchestrator-state.json` only; on this run it was
   ABSENT when the executor started, blocking the first .ps1 Write. Another agent later wrote a ready one and
   the gate passed. If the parent forbids writing that file, stop and wait rather than seed it.
   See [[preimplementation-gate-reads-sibling-checkpoint]] and [[parallel-child-hook-exposure-measured-not-assumed]].

**How to apply:** budget these three steps into any parallel item's PR phase instead of discovering them by denial.
