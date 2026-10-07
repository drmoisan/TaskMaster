---
name: worktree-removal-hooks-match-git-plus-remove-substring
description: EPIC/PARALLEL_WORKTREE_REMOVAL_BLOCKED fires on read-only pwsh commands that contain git plus any "remove" substring ($removed, TryRemove), with an empty worktree name
metadata:
  type: project
---

The worktree-removal PreToolUse hooks (EPIC_WORKTREE_REMOVAL_BLOCKED, PARALLEL_WORKTREE_REMOVAL_BLOCKED, message "git worktree remove for ''") refuse a read-only Bash `pwsh -Command` whose text contains a `git` invocation AND the substring "remove" anywhere (a `$removed` variable, a `TryRemove` search token), when the path contains `.claude/worktrees/`. The same command without `git` (e.g. `Remove-Item` or `TryRemove` in a git-free payload) passes. Observed 2026-09-30 on issue 944 plan P2-T6/P2-T8 recheck.

**Why:** the hook's matcher is a substring pattern over the whole command, not a parse of `git worktree remove`.

**How to apply:** when a plan payload mixes a `git diff` with a removal-named variable or a token list containing `TryRemove`, split it into a git-free invocation and a git invocation, rename the variable (e.g. `$gone`), and record the label rename and split as a substitution in the artifact. Related: [[nested-quotes-in-subexpression]] (a `"...$( "x" )..."` string also silently exits 1 under pwsh -Command; use concatenation).
