---
name: planner-prompt-needs-issue-and-branch-lines-every-round
description: Every Agent(atomic-planner) prompt, including revision rounds, needs a "Canonical issue number for this feature is <N>." line and a "branch: <item branch>" label, or the prd-feature hook denies with TARGET_WORKTREE_NOT_DERIVABLE
metadata:
  type: project
---

`enforce-prd-feature-before-planner.ps1` identifies the item a planner delegation acts on from the
PROMPT TEXT. When the prompt carries neither a canonical issue-number line nor a branch signal
(`--head`, `--branch`, or `branch:`), it denies with
`PRD_FEATURE_BLOCKED: TARGET_WORKTREE_NOT_DERIVABLE`. It does not fall back to the checkpoint in that
case, even when the checkpoint is ready and names the feature folder.

Observed on #945 preparation (2026-09-30): the initial planner prompt passed because it happened to
contain a `Branch: bug/...` line. The round-1 revision prompt, written as a delta list without that
header, was denied. Re-issuing it with the two lines below admitted it at once:

```
Canonical issue number for this feature is 945.
branch: bug/sort-email-attachment-test-creates-directory-945
```

**How to apply:** put both lines near the top of EVERY planner and preflight prompt, revision rounds
included. A revision brief is the easiest place to drop them, because it reads as a continuation of
the earlier prompt. Related: [[prd-feature-hook-parses-prompt-paths]],
[[prd-feature-hook-picks-longest-active-path]].
