---
name: epic-child-rebase-shared-memory-conflict
description: When an epic child rebases onto an advanced integration tip, the only conflict is usually the shared .claude/agent-memory/<agent>/MEMORY.md index (siblings append to it); resolve by keeping both sides.
metadata:
  type: feedback
---

An epic integration branch advances as sibling children merge. A child branched from an older tip should rebase onto `origin/<integration>` before opening its PR (cleaner linear diff; avoids GitHub reconciling a stale base).

**Why:** siblings that touch disjoint source areas (verified: `git diff <fork-point>..origin/<integration> -- <my files>` is empty) rebase without source conflicts. The one predictable conflict is `.claude/agent-memory/<agent>/MEMORY.md` — multiple executors append entries to the same index. Resolve it by keeping BOTH sibling and local entries (union), then `git rebase --continue`.

**How to apply:**
1. Before rebasing, prove no overlap: `git diff <fork-point>..origin/<integration> -- <your production paths, your feature folder>` must be empty. Also grep your changed files for any symbol a sibling annotated (e.g. NewtonsoftHelpers, SVGControl) — if zero references, your pragma-gate result is invariant under their merge and no rebuild is needed.
2. Rebase; resolve the MEMORY.md union conflict; continue.
3. Regenerate PR context from the real diff — `mcp__drm-copilot__collect_pr_context` writes unreliably in an isolated agent worktree (lands in the main checkout / stale). Write `artifacts/pr_context.summary.txt` yourself from `git diff --name-status <base>..HEAD`; the pr-author hook only needs it to EXIST and be OLDER than the receipt `created_at`. See [[collect-pr-context-lands-in-main-checkout]] and [[agent-worktree-hooks-resolve-to-agent-cwd]].
