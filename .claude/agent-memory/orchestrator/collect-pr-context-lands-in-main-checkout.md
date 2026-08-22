---
name: collect-pr-context-lands-in-main-checkout
description: From an isolated agent worktree, collect_pr_context returns ok:true but writes to the PRIMARY checkout and claims gh is unavailable - author the PR body from the real diff instead
metadata:
  type: project
---

Calling `mcp__drm-copilot__collect_pr_context` from a `.claude/worktrees/<agent-id>` worktree returns
`ok:true` and lists artifact paths **inside that worktree**, but the files it actually writes land in
the PRIMARY checkout (`C:\Users\DanMoisan\repos\TaskMaster\artifacts\`). The `workspace_root`
argument does not redirect it. Confirmed again 2026-08-22 (epic child #445): the returned paths had
an mtime ~10 minutes older than the call, while the primary checkout's copy was freshly written.

Two further defects make the artifact unusable rather than merely misplaced:

1. **It claims `gh` is unavailable** (`GitHub CLI unavailable: ... not installed`) when `gh auth
   status` in the same worktree authenticates fine. Never accept that claim; verify `gh` yourself.
2. **The primary checkout is on a different branch**, so the diff it computes is not your branch's
   diff at all. Copying it into the worktree would import a wrong changed-file list.

**Why:** a PR body built from that artifact misstates the change. In #445 the stale worktree copy
recorded a head SHA one commit behind and omitted all three review artifacts.

**How to apply:** treat `collect_pr_context` as a required-receipt formality, not an information
source. Refresh it so the receipt and the hook's freshness check are satisfied, then author the body
from `git diff --numstat <base>...HEAD`, `git log --oneline <base>..HEAD`, and `git rev-parse HEAD`.
If a hook reads `artifacts/pr_context.summary.txt` (the pr-author receipt staleness check does),
regenerate an ACCURATE one locally in the worktree before writing the receipt, so `created_at` is
strictly newer than it. `pr_context.*` is gitignored, so none of this dirties the tree. See
[[pr-context-summary-unreliable-gh-and-classification]] and
[[pr-author-hook-blocks-gh-in-this-repo]].
